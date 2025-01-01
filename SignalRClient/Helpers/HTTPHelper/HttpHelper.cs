using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace SignalRClient.Helpers.HTTPHelper
{
    public static class HttpHelper
    {
        public static async Task<HttpResponseMessage> HTTPGet(string completeApiURL)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(completeApiURL);
            return response;
        }

        public static async Task<HttpResponseMessage> HTTPPost(string completeApiURL, object bodyData)
        {
            var httpBody = HttpHelper.ConvertToHttpContent(bodyData);
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "sdfghj456789sdfghj45678sdfghj45678sdfghj45678dfghj34567sdfgh34567dfghj45678sdfghj345678sdfghj3456");
            httpClient.DefaultRequestHeaders.Add("AppName", "signalrhub");
            HttpResponseMessage response = await httpClient.PostAsync(completeApiURL, httpBody);
            return response;

            //if (response.IsSuccessStatusCode)
            //{
            //    string responseBody = await response.Content.ReadAsStringAsync();
            //    Console.WriteLine(responseBody);
            //}
            //else
            //{
            //    Console.WriteLine($"HTTP request failed with status code: {response.StatusCode}");
            //}
        }

        public static HttpContent ConvertToHttpContent(object data)
        {
            // Serialize the object to JSON string
            string json = JsonConvert.SerializeObject(data);

            // Create StringContent with JSON as the content
            HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            return httpContent;
        }

    }
}
