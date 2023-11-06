using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Net.Http;

namespace OLXChecker
{
    internal class OLXApi
    {
        private static readonly string baseUrl = "https://www.olx.pl";
        public static async Task<RestResponse> RefreshTokens(Account account)
        {
            var options = new RestClientOptions(baseUrl)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/api/open/oauth/token", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("client_id", account.Id);
            request.AddParameter("client_secret", account.Secret);
            request.AddParameter("refresh_token", account.Refresh);
            RestResponse response = await client.ExecuteAsync(request);

            return response;
        }
        public static async Task<RestResponse> Authorizate(Account account, string code)
        {
            var options = new RestClientOptions(baseUrl)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/api/open/oauth/token", Method.Post);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("scope", "v2 read write");
            request.AddParameter("client_id", account.Id);
            request.AddParameter("client_secret", account.Secret);
            request.AddParameter("code", code);
            request.AddParameter("redirect_uri", "https://idzczak-meble.pl/");
            RestResponse response = await client.ExecuteAsync(request);

            return response;
        }
        public static async Task<RestResponse> GetThreads(Account account)
        {
            var options = new RestClientOptions(baseUrl)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/api/partner/threads", Method.Get);
            request.AddHeader("Authorization", $"Bearer {account.Access}");
            request.AddHeader("Version", "2.0");
            RestResponse response = await client.ExecuteAsync(request);

            return response;
        }
        public static async Task<RestResponse> GetConversationsCounter(Account account)
        {
            var options = new RestClientOptions("https://api.chat.olx.pl/api")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/conversations/counters", Method.Get);
            request.AddHeader("Authorization", $"Bearer {account.Access}");
            request.AddHeader("Version", "2.0");
            RestResponse response = await client.ExecuteAsync(request);

            return response;
        }
        public static async Task<string> GetCodeFromUrlAsync(string url)
        {
            try
            {
                // Create an HttpClient instance
                using (HttpClient client = new HttpClient())
                {
                    // Send an HTTP GET request to the URL
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the content of the response as a string
                        string content = await response.Content.ReadAsStringAsync();

                        // Parse the query string to get the "code" parameter
                        NameValueCollection queryParameters = System.Web.HttpUtility.ParseQueryString(new Uri(url).Query);
                        string code = queryParameters["code"];

                        return code;
                    }
                    else
                    {
                        Console.WriteLine("HTTP request failed with status code: " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return null;
        }
    }
}
