using Nest;

namespace Collections.Services.Elastic
{
    public interface IElasticClientService
    {
        Task<ISearchResponse<T>> SearchAsync<T>(string keyword) where T : class;
        Task<ExistsResponse> DocumentExistsAsync<T>(int id) where T : class;
        Task<IndexResponse> AddToElasticIndexAsync<T>(T model) where T : class;
        Task<UpdateResponse<T>> UpdateElasticItemAsync<T>(int itemId, T model) where T : class;
        Task<DeleteResponse> RemoveFromElasticIndexAsync<T>(int id) where T : class;
    }
}
