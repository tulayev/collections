using Collections.Models.ViewModels;
using Nest;

namespace Collections.Services
{
    public class AppElasticClient
    {
        private readonly IElasticClient _client;

        public AppElasticClient(IElasticClient client)
        {
            _client = client;
        }

        public async Task AddToElasticIndex(ElasticItemViewModel model) =>
            
            await _client.IndexDocumentAsync(model);

        public async Task UpdateElasticItem(int itemId, ElasticItemViewModel model) =>

            await _client.UpdateAsync<ElasticItemViewModel>(
                itemId,
                u => u.Index("items").Doc(model)
            );

        public async Task RemoveFromElasticIndex(int id) => 
            
            await _client.DeleteAsync<ElasticItemViewModel>(id);
    }
}
