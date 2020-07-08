using System;
using System.Collections.Generic;
using System.Diagnostics;
#if NETSTANDARD2_1
using System.Diagnostics.CodeAnalysis;
#endif
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace SubSonic.Infrastructure
{
    public sealed partial class SubSonicCollection<TElement>
        : IAsyncEnumerator<TElement>
        , IAsyncStateMachine
        , IValueTaskSource<bool>
        , IValueTaskSource
        , IDisposable
    {
        private static readonly Action<object> CallbackCompleted = _ => Debug.Assert(false, "should not be invoked!");
        private Action<Object> continuation;
        private AsyncIteratorMethodBuilder asyncMethodBuilder;
        private CancellationTokenSource CancellationTokenSource;
        public CancellationToken CancellationToken => CancellationTokenSource?.Token ?? default;
        private ManualResetValueTaskSourceCore<bool> PromiseOfValueOrEnd;
        private TaskAwaiter StateAwaiter;
        private IAsyncStateMachine sm;

        private ExecutionContext executionContext;
        private object scheduler;
        private object state;

        private short token;
        private bool? result;
        private int index;
        private int? istate;
        private bool disposed;

        public TElement Current { get; private set; }

        private TElement GetElementAt(int idx)
        {
            if (TableData is IEnumerable<TElement> data)
            {
                return data.ElementAt(idx);
            }

            return default(TElement);
        }

        IAsyncEnumerator<TElement> IAsyncEnumerable<TElement>.GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            asyncMethodBuilder = new AsyncIteratorMethodBuilder();
            CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            disposed = false;

            return this;
        }

        public async ValueTask DisposeAsync()
        {
            await Task.CompletedTask.ConfigureAwait(false);

            Dispose();
        }

        public void MoveNext()
        {
            try
            {
                TaskAwaiter awaiter;

                if (disposed)
                {
                    goto DONE_ITERATING;
                }

                switch(istate.GetValueOrDefault(-1))
                {
                    case 0:
                        awaiter = StateAwaiter;
                        goto DONE_AWAIT;
                    case -4:
                        index++;
                        goto LOOP_CONDITION;
                    default:
                        index = 0;
                        goto LOOP_CONDITION;
                }
            LOOP_CONDITION:
                if (index >= Count)
                {
                    goto DONE_ITERATING;
                }
                awaiter = Task.Delay(index, CancellationToken).GetAwaiter();
                if(!awaiter.IsCompleted)
                {
                    istate = 0;
                    StateAwaiter = awaiter;
                    SetStateMachine(this);
                    asyncMethodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref sm);
                    return;
                }
            DONE_AWAIT:
                awaiter.GetResult();
                Current = GetElementAt(index);
                istate = -4;
                goto RETURN_TRUE_FROM_MOVENEXTASYNC;
            DONE_ITERATING:
                istate = -2;
                CancellationTokenSource?.Dispose();
                PromiseOfValueOrEnd.SetResult(result: false);
                return;
            RETURN_TRUE_FROM_MOVENEXTASYNC:
                PromiseOfValueOrEnd.SetResult(result: true);
            }
            catch(OperationCanceledException ex)
            {
                AbortExecution(ex);
            }
            catch(InvalidOperationException ex)
            {
                AbortExecution(ex);
            }

        }

        private void AbortExecution(Exception ex)
        {
            istate = -2;
            CancellationTokenSource?.Dispose();
            PromiseOfValueOrEnd.SetException(ex);
        }

        public ValueTask<bool> MoveNextAsync()
        {
            if (istate == -2)
            {
                return default;
            }

            ResetAndReleaseOperation();

            SetStateMachine(this);

            asyncMethodBuilder.MoveNext(ref sm);

            if (GetStatus(this.token) == ValueTaskSourceStatus.Succeeded)
            {
                GetResult(this.token);

                Debug.Assert(result.HasValue, "failed to complete");

                return new ValueTask<bool>(result.GetValueOrDefault());
            }
            else
            {
                return new ValueTask<bool>(this, token);
            }
        }

        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            if (this.token != token)
            {
                throw Error.InvalidOperation(SubSonicErrorMessages.MultipleContinuations);
            }

            if ((flags & ValueTaskSourceOnCompletedFlags.FlowExecutionContext) != 0)
            {
                this.executionContext = ExecutionContext.Capture();
            }

            if ((flags & ValueTaskSourceOnCompletedFlags.UseSchedulingContext) != 0)
            {
                SynchronizationContext sc = SynchronizationContext.Current;

                if (sc != null && sc.GetType() == typeof(SynchronizationContext))
                {
                    this.scheduler = sc;
                }
                else
                {
                    TaskScheduler ts = TaskScheduler.Current;
                    if (ts != TaskScheduler.Default)
                    {
                        this.scheduler = ts;
                    }
                }
            }

            this.state = state;

            var previousContinuation = Interlocked.CompareExchange(ref this.continuation, continuation, null);

            if (previousContinuation != null)
            {
                if (!ReferenceEquals(previousContinuation, CallbackCompleted))
                {
                    throw Error.InvalidOperation(SubSonicErrorMessages.ErrorPreviousContinuation);
                }

                this.executionContext = null;
                this.state = null;
            }

            InvokeContinuation(continuation, state);
        }

        private void InvokeContinuation(Action<object> continuation, object state)
        {
            if (continuation is null)
            {
                return;
            }

            object scheduler = this.scheduler ?? TaskScheduler.Default;

            if (scheduler != null)
            {
                if (scheduler is SynchronizationContext sc)
                {
                    sc.Post(s =>
                    {
                        var t = (Tuple<Action<object>, object>)s;
                        t.Item1(t.Item2);
                    }, Tuple.Create(continuation, state));
                }
                else if (scheduler is TaskScheduler ts)
                {
                    Task.Factory.StartNew(continuation, state, CancellationToken, TaskCreationOptions.DenyChildAttach, ts);
                }
                else
                {
                    throw Error.NotSupported($"{scheduler}");
                }
            }
#if NETSTANDARD2_1
            else if (PromiseOfValueOrEnd.RunContinuationsAsynchronously)
            {
                ThreadPool.QueueUserWorkItem(continuation, state, preferLocal: true);
            }
#endif
            else
            {
                continuation(state);
            }

        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            sm = stateMachine;
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            if (this.token != token)
            {
                throw Error.InvalidOperation(SubSonicErrorMessages.MultipleContinuations);
            }

            return PromiseOfValueOrEnd.GetStatus(token);
        }

        public void GetResult(short token)
        {
            if (this.token != token)
            {
                throw Error.InvalidOperation(SubSonicErrorMessages.MultipleContinuations);
            }

            this.result = PromiseOfValueOrEnd.GetResult(token);
        }

        bool IValueTaskSource<bool>.GetResult(short token)
        {
            if (this.token != token)
            {
                throw Error.InvalidOperation(SubSonicErrorMessages.MultipleContinuations);
            }

            while (GetStatus(token) == ValueTaskSourceStatus.Pending)
            {
                asyncMethodBuilder.MoveNext(ref sm);
            }

            GetResult(token);

            return result.Value;
        }

        private void ResetAndReleaseOperation()
        {
            CancellationToken.ThrowIfCancellationRequested();

            PromiseOfValueOrEnd.Reset();

            this.token = PromiseOfValueOrEnd.Version;
            this.continuation = null;
            this.scheduler = null;
            this.state = null;
            this.result = null;
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    CancellationTokenSource?.Dispose();
                    CancellationTokenSource = null;

                    istate = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SubSonicCollection()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
