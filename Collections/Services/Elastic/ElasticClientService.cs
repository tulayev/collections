using Collections.Models.ViewModels;
using Nest;

namespace Collections.Services.Elastic
{
    public class ElasticClientService : IElasticClientService
    {
        private readonly IConfiguration _config;
        private readonly IElasticClient _client;

        public ElasticClientService(IConfiguration config)
        {
            _config = config;
            _client = InitClient();
        }

        private ElasticClient InitClient()
        {
            var url = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                ? _config["Elasticsearch:Url"]
                : Environment.GetEnvironmentVariable("BONSAI_URL");

            var defaultIndex = _config["Elasticsearch:Index"];

            var settings = new ConnectionSettings(new Uri(url))
                    .PrettyJson()
                    .DefaultIndex(defaultIndex);

            AddDefaultMappings(settings);

            var client = new ElasticClient(settings);

            CreateIndex(client, defaultIndex);

            return client;
        }

        public async Task<ISearchResponse<T>> SearchAsync<T>(string keyword) where T : class
        {
            return await _client.SearchAsync<T>(
                s => s.Query(
                        q => q.QueryString(d => d.Query('*' + keyword + '*'))
                    ).Size(1000)
            );
        }

        public async Task<ExistsResponse> DocumentExistsAsync<T>(int id) where T : class
        {
            return await _client.DocumentExistsAsync<T>(id);
        }

        public async Task<IndexResponse> AddToElasticIndexAsync<T>(T model) where T : class
        {
            return await _client.IndexDocumentAsync(model);
        }

        public async Task<UpdateResponse<T>> UpdateElasticItemAsync<T>(int itemId, T model) where T : class
        {
            return await _client.UpdateAsync<T>(
                itemId,
                u => u.Index(_config["Elasticsearch:Url"]).Doc(model)
            );
        }

        public async Task<DeleteResponse> RemoveFromElasticIndexAsync<T>(int id) where T : class
        {
            return await _client.DeleteAsync<T>(id);
        }

        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            settings.DefaultMappingFor<ElasticItemViewModel>(x => x);
        }

        private static void CreateIndex(IElasticClient client, string indexName)
        {
            client.Indices.Create(indexName, i => i.Map<ElasticItemViewModel>(
                x => x.AutoMap()
                        .Properties(p => p.Nested<CommentDto>(c => c.Name(c => c.Comments).AutoMap()
                        .Properties(cb => cb.Keyword(c => c.Name(nn => nn.Body)))))
                    )
            );
        }
    }
}
