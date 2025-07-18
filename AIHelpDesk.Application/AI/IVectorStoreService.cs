namespace AIHelpDesk.Infrastructure
{
    public interface IVectorStoreService
    {
        Task StoreAsync(string tenant, IEnumerable<(string id, float[] vector, string text)> items);
        Task<IReadOnlyList<(string id, float score, string text)>> QueryAsync(string tenant, float[] queryVector, int topK = 5);
    }
}