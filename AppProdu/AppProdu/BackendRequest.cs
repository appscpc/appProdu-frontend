using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AppProdu
{
    public class BackendRequest
    {
        public async Task<T> Get<T>(string url, string jsonData)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            Console.WriteLine("AQUI");
            HttpResponseMessage response = await client.PostAsync("/users/login", content);
            Console.WriteLine("AQUI2");
            //var response = await client.GetAsync(url);
            //Console.WriteLine("AQUI2");
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("AQUI3");

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
