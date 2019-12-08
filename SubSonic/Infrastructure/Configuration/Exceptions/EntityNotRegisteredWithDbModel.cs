using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class EntityNotRegisteredWithDbModelException
        : Exception
    {
        public EntityNotRegisteredWithDbModelException()
        {
        }

        public EntityNotRegisteredWithDbModelException(Type entityModelType)
            : base(SubSonicErrorMessages.EntityTypeIsNotRegisteredException.Format(entityModelType.IsNotNull(t => t.Name, "")))
        {
            if (entityModelType is null)
            {
                throw new ArgumentNullException(nameof(entityModelType));
            }
        }

        public EntityNotRegisteredWithDbModelException(string message) : base(message)
        {
        }

        public EntityNotRegisteredWithDbModelException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EntityNotRegisteredWithDbModelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
