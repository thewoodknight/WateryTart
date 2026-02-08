using WateryTart.Service.MassClient.Messages;
using WateryTart.Service.MassClient.Responses;

namespace WateryTart.Service.MassClient
{
    public static partial class MassClientExtensions
    {
        public static async Task<SearchResponse> SearchAsync(this IMassWsClient c, string query, int? limit = null, bool library_only = true)
        {
            var args = new Dictionary<string, object>()
            {
                { "search_query", query },
                { "library_only", library_only }
            };

            if (limit != null)
                args.Add("limit", limit);

            var m = new Message(Commands.Search)
            {
                args = args
            };
            
            return await SendAsync<SearchResponse>(c, m);
        }
    }
}
