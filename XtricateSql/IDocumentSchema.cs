namespace XtricateSql
{
    public interface IDocumentSchema
    {
        IDocumentStorage<T> Storage<T>();
    }
}