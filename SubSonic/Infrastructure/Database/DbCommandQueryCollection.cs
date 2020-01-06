using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class DbCommandQueryCollection
        : ICollection<DbCommandQuery>
    {
        private readonly Dictionary<DbQueryType, DbCommandQuery> commands;

        public DbCommandQueryCollection()
        {
            commands = new Dictionary<DbQueryType, DbCommandQuery>();
        }

        public DbCommandQueryCollection(IEnumerable<DbCommandQuery> commands)
            : this()
        {
            if (commands is null)
            {
                throw new ArgumentNullException(nameof(commands));
            }

            foreach (DbCommandQuery command in commands)
            {
                this[command.QueryType] = command;
            }
        }

        public DbCommandQuery this[DbQueryType dbQueryType]
        {
            get
            {
                if (commands.ContainsKey(dbQueryType))
                {
                    return commands[dbQueryType];
                }
                return null;
            }
            set
            {
                if (commands.ContainsKey(dbQueryType))
                {
                    commands[dbQueryType] = value;
                }
                else
                {
                    commands.Add(dbQueryType, value);
                }
            }
        }

        public int Count => commands.Count;

        public bool IsReadOnly => false;

        public void Add(DbCommandQuery item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            commands.Add(item.QueryType, item);
        }

        public void Clear()
        {
            commands.Clear();
        }

        public bool Contains(DbCommandQuery item)
        {
            return commands.ContainsValue(item);
        }

        public void CopyTo(DbCommandQuery[] array, int arrayIndex)
        {
            commands.Values.CopyTo(array, arrayIndex);
        }

        public IEnumerator<DbCommandQuery> GetEnumerator()
        {
            return commands.Values.GetEnumerator();
        }

        public bool Remove(DbCommandQuery item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return commands.Remove(item.QueryType);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)commands.Values).GetEnumerator();
        }
    }
}
