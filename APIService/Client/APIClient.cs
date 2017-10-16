using APIService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace APIService.Client
{
    public class APIClient : HttpClient
    {
        private HttpClient client;
        private HttpClientHandler handler;
        public APIClient()
        {
            //Start : Credentials

            handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential("gfsuser", "gfsuser123!");

            //End : Credentials   

            client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://uat1.woolworthsmoney.com.au/");

            //Start : Default Headers 

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Basic Z2ZzdXNlcjpnZnN1c2VyMTIzIQ==");

            //End : Default Headers
        }

        public async Task<T> GetAPIResponse<T>(string query)
        {
            dynamic responseObj = null;

            // HttpResponseMessage response = await client.GetAsync(query);
            Body body = new Body { token = "ek1:a4303041aad645d0e26cb4a526d01558", usa = "old:1baa0dbdadcd14719511ec51b9379a00d2fbcc24661155ee1b08fd51d80ecdca" };
            HttpResponseMessage response = await client.PostAsJsonAsync(query, body);
            if (response.IsSuccessStatusCode)
            {
                responseObj = await response.Content.ReadAsAsync<T>();
                //responseObj = await response.Content.ReadAsAsync<T>(); use this if you dont want deserialization to happen over here

            }
            else
            {
                //logging mechanism
            }
            return responseObj;
        }
    }
}
