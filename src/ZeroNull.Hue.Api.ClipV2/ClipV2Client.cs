using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZeroNull.Hue.Api.ClipV2
{
    public class ClipV2Client
    {
        private readonly RestSharp.RestClient restClient;
        private string appAccessKey = string.Empty;

        public ClipV2Client(string baseUrl)
        {
            restClient = new RestSharp.RestClient(baseUrl);
            restClient.UseNewtonsoftJson();
            restClient.RemoteCertificateValidationCallback = (s, cer, chain, errors) => true;
        }

        public void SetApplicationAccessKey(string key)
        {
            appAccessKey = key;
        }

        public Task<IRestResponse<IEnumerable<AuthResponse>>> GetApplicationAccessKey()
        {
            var request = new RestRequest("api", Method.POST)
                .AddJsonBody(new
                {
                    devicetype = "app_name#instance_name",
                    generateclientkey = true
                });

            return restClient.ExecuteAsync<IEnumerable<AuthResponse>>(request);
        }

        internal Task<IRestResponse<T>> Execute<T>(IRestRequest request)
        {
            request.RequestFormat = DataFormat.Json;
            if (!string.IsNullOrEmpty(appAccessKey))
            {
                request.AddHeader("hue-application-key", appAccessKey);
            }
            return restClient.ExecuteAsync<T>(request);
        }


        public Task<IRestResponse<ApiResponse<IEnumerable<ResourceIdentifier>>>> Post<T>(ResourceType type, T item)
        {
            var request = new RestRequest($"clip/v2/resource/{type}", Method.POST)
                .AddJsonBody(item);

            return Execute<ApiResponse<IEnumerable<ResourceIdentifier>>>(request);
        }

        public Task<IRestResponse<ApiResponse<JToken>>> Get(ResourceType type)
        {
            return Get<JToken>(type);
        }

        public Task<IRestResponse<ApiResponse<T>>> Get<T>(ResourceType type)
        {
            var request = new RestRequest($"clip/v2/resource/{type}", Method.GET);
            return Execute<ApiResponse<T>>(request);
        }

        public Task<IRestResponse<ApiResponse<JToken>>> Get(ResourceType type, string id)
        {
            return Get<JToken>(type, id);
        }

        public Task<IRestResponse<ApiResponse<T>>> Get<T>(ResourceType type, string id)
        {
            var request = new RestRequest($"clip/v2/resource/{type}/{{id}}", Method.GET)
                .AddParameter("id", id, ParameterType.UrlSegment);

            return Execute<ApiResponse<T>>(request);
        }

        public Task<IRestResponse<ApiResponse<IEnumerable<ResourceIdentifier>>>> Delete(ResourceType type, string id)
        {
            var request = new RestRequest($"clip/v2/resource/{type}/{{id}}", Method.DELETE)
                .AddParameter("id", id, ParameterType.UrlSegment);

            return Execute<ApiResponse<IEnumerable<ResourceIdentifier>>>(request);
        }

        public Task<IRestResponse<ApiResponse<IEnumerable<ResourceIdentifier>>>> Put<T>(ResourceType type, string id, T patchItem)
        {
            var request = new RestRequest($"clip/v2/resource/{type}/{{id}}", Method.PUT)
                .AddParameter("id", id, ParameterType.UrlSegment)
                .AddJsonBody(patchItem);

            return Execute<ApiResponse<IEnumerable<ResourceIdentifier>>>(request);
        }
    }
}
