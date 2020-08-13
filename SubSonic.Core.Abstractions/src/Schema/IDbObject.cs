namespace SubSonic.Schema
{
    public interface IDbObject
    {
        string Name { get; }
        string FriendlyName { get; }
        string QualifiedName { get; }
        string SchemaName { get; }
        DbObjectTypeEnum DbObjectType { get; }
    }
}
