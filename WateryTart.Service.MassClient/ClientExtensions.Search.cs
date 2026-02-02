using System.Collections;
using WateryTart.Service.MassClient.Messages;

namespace WateryTart.Service.MassClient
{
    public static partial class MassClientExtensions
    {
        extension(IMassWsClient c)
        {
            public async Task<SearchResponse> SearchAsync(string query, int? limit = null, bool library_only = true)
            {

                var args = new Hashtable
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
}
