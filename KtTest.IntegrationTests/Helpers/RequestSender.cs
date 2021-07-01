using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace KtTest.IntegrationTests.Helpers
{
    public class RequestSender
    {
        private readonly HttpClient httpClient;
        private readonly AuthenticationHeaderValue defaultAuthHeaderValue;

        public RequestSender(HttpClient httpClient, string defaultBearerToken)
        {
            defaultAuthHeaderValue = new AuthenticationHeaderValue("Bearer", defaultBearerToken);
            this.httpClient = httpClient;
        }

        public Task<HttpResponseMessage> GetAsync(string url)
        {
            return GetAsync_(url);
        }

        public Task<HttpResponseMessage> GetAsync(string url, string token)
        {
            return GetAsync_(url, token);
        }

        private Task<HttpResponseMessage> GetAsync_(string url, string token = null)
        {
            AuthenticationHeaderValue header = 
                IfTokenIsNotEmptyCreateNewAuthenticationHeaderOtherwiseDefault(token);


            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
            {
                requestMessage.Headers.Authorization = header;
                return httpClient.SendAsync(requestMessage);
            }
        }

        public Task<HttpResponseMessage> PostAsync(string url, string json)
        {
            return PostAsync_(url, json);
        }

        public Task<HttpResponseMessage> PostAsync(string url, string json, string token)
        {
            return PostAsync_(url, json);
        }

        private Task<HttpResponseMessage> PostAsync_(string url, string json, string token = null)
        {
            AuthenticationHeaderValue header =
                IfTokenIsNotEmptyCreateNewAuthenticationHeaderOtherwiseDefault(token);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, url))
            {
                requestMessage.Headers.Authorization = header;
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
                return httpClient.SendAsync(requestMessage);
            }
        }

        public Task<HttpResponseMessage> DeleteAsync(string url)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url))
            {
                requestMessage.Headers.Authorization = defaultAuthHeaderValue;
                return httpClient.SendAsync(requestMessage);
            }
        }

        private AuthenticationHeaderValue IfTokenIsNotEmptyCreateNewAuthenticationHeaderOtherwiseDefault(string token)
        {
            if (token == null)
                return defaultAuthHeaderValue;

            return new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
