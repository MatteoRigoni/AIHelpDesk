using Microsoft.Extensions.DependencyInjection;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIHelpDesk.Infrastructure;

/// <summary>
/// Adapter concreto per IVectorStoreService (layer Infrastructure).
/// </summary>
public sealed class QdrantVectorStoreService : IVectorStoreService
{
    private readonly QdrantClient _client;

    public QdrantVectorStoreService(
        [FromKeyedServices("mainQdrant")] QdrantClient client) =>
        _client = client;

    private static string Collection(string tenant) => $"tenant_{tenant}";

    // ------------------------------------------------------------
    //  STORE
    // ------------------------------------------------------------
    public async Task StoreAsync(
        string tenant,
        IEnumerable<(string id, float[] vector, string text)> items)
    {
        var name = Collection(tenant);
        var first = items.First();

        // 2️⃣  Overload corretto: (collectionName, vectorSize, distance)
        if (!await _client.CollectionExistsAsync(name))
        {
            await _client.CreateCollectionAsync(
                collectionName: name, vectorsConfig: new VectorParams
                {
                    Size = (ulong)first.vector.Length,
                    Distance = Distance.Cosine
                });

            // 3️⃣  PointStruct<string> + payload dizionario
            var points = items.Select(i =>
            new PointStruct()
            {
                Id = new PointId { Num = Convert.ToUInt64(i.id) },
                Vectors = i.vector,
                Payload = {
                ["text"] = i.text
            }
            });

            await _client.UpsertAsync(name, points.ToList());
        }
    }

    // ------------------------------------------------------------
    //  QUERY
    // ------------------------------------------------------------
    public async Task<IReadOnlyList<(string id, float score, string text)>> QueryAsync(
        string tenant,
        float[] queryVector,
        int topK = 5)
    {
        var name = Collection(tenant);

        var hits = await _client.SearchAsync(
            name, queryVector,
            limit: (ulong) topK,
            payloadSelector: new WithPayloadSelector() { Enable = true });

        return hits.Select(h =>
        {
            // .TryGetValue restituisce Value → StringValue
            var text = h.Payload.TryGetValue("text", out var v)
                         ? v.StringValue
                         : string.Empty;

            return (id: h.Id.ToString(), score: h.Score, text);
        }).ToList();
    }
}
