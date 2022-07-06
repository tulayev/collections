using Collections.Models.ViewModels;
using Nest;

namespace Collections.Utils
{
    public static class ElasticSearchExtensions
    {
        public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
        {
            string url = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
                ? configuration["Elasticsearch:Url"]
                : Environment.GetEnvironmentVariable("BONSAI_URL");

            string defaultIndex = configuration["Elasticsearch:Index"];

            var settings = new ConnectionSettings(new Uri(url))
                    .PrettyJson()
                    .DefaultIndex(defaultIndex);

            AddDefaultMappings(settings);

            var client = new ElasticClient(settings);
            services.AddSingleton<IElasticClient>(client);

            CreateIndex(client, defaultIndex);
        }

        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            settings.DefaultMappingFor<ElasticItemViewModel>(p => p);
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