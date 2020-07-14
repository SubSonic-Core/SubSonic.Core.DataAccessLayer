namespace SubSonic.Logging
{
    public interface ITraceLogger<out TCategoryName>
    {
        bool IsTraceLoggingEnabled { get; }

        void WriteTrace(string method, string message);
    }
}
