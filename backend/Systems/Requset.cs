using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Systems
{
    public class Requset
    {
        public RestClient Client;

        public RestRequest ClientRequest;

        public Requset()
        {
            Client = new RestClient();
            Client.Timeout = 5000;

            ClientRequest = new RestRequest();
        }

        public async Task<JObject> HttpRequset(Uri Uri, Method method, Dictionary<string, string> Headers = null, List<Parameter> parameters = null)
        {
            Client.BaseUrl = Uri;
            ClientRequest.Method = method;

            if (Headers != null)
            {
                if (Headers.Count >= 1)
                {
                    foreach (var item in Headers)
                    {
                        ClientRequest.AddHeader(item.Key, item.Value);
                    }
                }
            }

            if (parameters != null)
            {
                if (parameters.Count >= 1)
                {
                    foreach (var item in parameters)
                    {
                        ClientRequest.AddParameter(item);
                    }
                }
            }

            var Content = ((await Client.ExecuteAsync(ClientRequest)).Content);

            return JObject.Parse(Content);
        }
    }
}
