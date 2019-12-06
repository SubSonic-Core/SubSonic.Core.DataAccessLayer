using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Infrastructure
{
    public class EntityNotRegisteredWithDbModel
        : Exception
    {
        public EntityNotRegisteredWithDbModel(Type entityModelType)
            : base($"Type \"{entityModelType.Name}\", is not registed with the db model.")
        {

        }
    }
}
