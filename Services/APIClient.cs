using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StudentManager.Domain.Interfaces.Services;
using StudentsManager.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StudentsManager.UI.Services
{
    public class APIClient<T> : IAPIClient<T> where T : class
    {
        private readonly HttpClient _client;
        private readonly String _apiAddress;

        public APIClient(HttpClient client, IOptions<AppSettings> config)
        {
            _client = client;
            _apiAddress = config.Value.StudentServiceApiEndpoint;
        }

        public async Task<PaginatedList<T>> GetPaginatedListFromAPI(string uri)
        {
            uri = $"{_apiAddress}{uri}";
            PaginatedList<T> List = null;
            string response = null;

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
                new Uri(uri))
            {
                Version = HttpVersion.Version10
            };

            var messge = await _client.SendAsync(httpRequestMessage);

            if (messge.IsSuccessStatusCode)
            {
                response = messge.Content.ReadAsStringAsync().Result;

                List = JsonConvert.DeserializeObject<PaginatedList<T>>(response);
            }

            return List;
        }

        public async Task<IEnumerable<T>> GetListFromAPI(string uri)
        {
            uri = $"{_apiAddress}{uri}";
            IEnumerable<T> List = null;
            string response = null;

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
                new Uri(uri))
            {
                Version = HttpVersion.Version10
            };

            var messge = await _client.SendAsync(httpRequestMessage);

            if (messge.IsSuccessStatusCode)
            {
                response = messge.Content.ReadAsStringAsync().Result;

                List = JsonConvert.DeserializeObject<IEnumerable<T>>(response);
            }

            return List;
        }

        public async Task<T> GetEntityFromAPI(string uri)
        {
            uri = $"{_apiAddress}{uri}";
            T entity = null;
            string response = null;

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
                new Uri(uri))
            {
                Version = HttpVersion.Version10
            };

            var messge = await _client.SendAsync(httpRequestMessage);

            if (messge.IsSuccessStatusCode)
            {
                response = messge.Content.ReadAsStringAsync().Result;

                entity = JsonConvert.DeserializeObject<T>(response);
            }

            return entity;
        }

        public async Task PostEntityToAPI(string uri, T entity)
        {
            uri = $"{_apiAddress}{uri}";

            StringContent content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json");
             
            HttpResponseMessage messge = await _client.PostAsync(uri, content);

            //messge.EnsureSuccessStatusCode();
        }

        public async Task PutEntityToAPI(string uri, T entity)
        {
            uri = $"{_apiAddress}{uri}";

            StringContent content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json");
             
            HttpResponseMessage messge = await _client.PutAsync(uri, content);

            // messge.EnsureSuccessStatusCode();   
        }

        public async Task DeleteEntityFromAPI(string uri)
        {
            uri = $"{_apiAddress}{uri}";

            HttpResponseMessage messge = await _client.DeleteAsync(uri);

            // messge.EnsureSuccessStatusCode();
        }
    }
}
