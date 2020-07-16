using System;
using System.Collections.Generic;

namespace SubSonic.Schema
{
    using Linq.Expressions;
    public interface IDbEntityModel
        : IDbObject
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IDbEntityProperty this[string name] { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IDbEntityProperty this[int index] { get; }
        /// <summary>
        /// 
        /// </summary>
        DbCommandQueryCollection Commands { get; }
        /// <summary>
        /// 
        /// </summary>
        ICollection<IDbRelationshipMap> RelationshipMaps { get; }
        /// <summary>
        /// 
        /// </summary>
        ICollection<IDbEntityProperty> Properties { get; }
        /// <summary>
        /// 
        /// </summary>
        bool HasRelationships { get; }
        /// <summary>
        /// 
        /// </summary>
        Type EntityModelType { get; }
        /// <summary>
        /// 
        /// </summary>
        DbTableExpression Table { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        DbTableExpression GetTableType(string name);
        /// <summary>
        /// 
        /// </summary>
        bool DefinedTableTypeExists { get; }
        /// <summary>
        /// 
        /// </summary>
        IDbObject DefinedTableType { get; }
        /// <summary>
        /// 
        /// </summary>
        int ObjectGraphWeight { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        object CreateObject();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetPrimaryKey();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        IDbRelationshipMap GetRelationshipWith(IDbEntityModel model);
        /// <summary>
        /// increments the object graph weight
        /// </summary>
        void IncrementObjectGraphWeight();
        /// <summary>
        /// Get the navigation property for
        /// </summary>
        /// <param name="model"><see cref="IDbEntityModel"/></param>
        /// <returns><see cref="IDbEntityProperty"/></returns>
        IDbEntityProperty GetNavigationPropertyFor(IDbEntityModel model);
    }
}
