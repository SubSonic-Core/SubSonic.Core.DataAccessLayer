using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace SubSonic
{
    public class EntityNotRegisteredWithDbModelException
        : Exception
    {
        public EntityNotRegisteredWithDbModelException()
        {
        }

        public EntityNotRegisteredWithDbModelException(Type entityModelType)
            : base(string.Format(CultureInfo.CurrentCulture, SubSonicErrorMessages.EntityTypeIsNotRegisteredException, entityModelType.IsNotNull(t => t.Name, "")))
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
