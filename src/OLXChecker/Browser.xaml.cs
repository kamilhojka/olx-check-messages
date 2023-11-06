using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace OLXChecker
{
    public partial class Browser : Window
    {
        private string _authUrl = null;
        private string _state = null;
        private readonly CountdownEvent PageLoad = new CountdownEvent(1);
        public string Code { get; set; }

        public Browser(string authUrl, string state)
        {
            InitializeComponent();

            _authUrl = authUrl;
            _state = state;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (webView != null)
            {
                await webView.EnsureCoreWebView2Async(null);

                // Delete existing Cookies so previous logins won't remembered
                webView.CoreWebView2.CookieManager.DeleteAllCookies();

                // Navigate
                webView.CoreWebView2.Navigate(_authUrl);

                await Task.Run(() => { WaitForPageLoad(); });

                await webView.ExecuteScriptAsync("document.getElementById('onetrust-accept-btn-handler').click();");
            }
        }

        private void webView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            string absoluteUri = webView.Source.AbsoluteUri;

            //Intiail the Countdown Event of Every Navigation
            PageLoad.Signal();
            Thread.Sleep(1);
            PageLoad.Reset();

            if (absoluteUri.Contains("/?code=") && absoluteUri.Contains($"&state={_state}"))
            {
                // Parse the URL and get the query string
                Uri uri = new Uri(absoluteUri);
                string queryString = uri.Query;

                // Parse the query string
                NameValueCollection queryParams = HttpUtility.ParseQueryString(queryString);

                // Get the values of the "code" and "state" parameters
                string code = queryParams["code"];
                string state = queryParams["state"];

                if (state != _state)
                {
                    throw new Exception("Błędny stan żądania!");
                }

                Code = code;
                Close();
            }
        }

        public void WaitForPageLoad()
        {
            //Wait the Page Load for Maximum 20 Seconds
            PageLoad.Wait(20000);
        }
    }
}
