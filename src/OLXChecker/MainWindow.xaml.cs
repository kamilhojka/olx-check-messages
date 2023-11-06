using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace OLXChecker
{
    public partial class MainWindow : Window
    {
        private Timer timer;
        private ObservableCollection<Account> accounts = new ObservableCollection<Account>();

        public MainWindow()
        {
            InitializeComponent();
        }
        private void TimerCallback(object state)
        {
            Dispatcher.Invoke(async () =>
            {
                AppWindow.IsEnabled = false;
                await GetMessagesForAllAccounts();
                AppWindow.IsEnabled = true;
            });
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) // Method to move window
        {
            Mouse.OverrideCursor = Cursors.SizeAll;
            try
            {
                base.OnMouseLeftButtonDown(e);
                DragMove();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async void GetAccounts()
        {
            AppWindow.IsEnabled = false;

            string jsonFilePath = "appsettings.json";

            try
            {
                if (!File.Exists(jsonFilePath)) throw new Exception("Brak pliku konfiguracyjnego");

                string jsonString = File.ReadAllText(jsonFilePath);

                JObject data = JObject.Parse(jsonString);

                foreach (var item in data["Accounts"])
                {
                    accounts.Add(new Account
                    {
                        Id = (int)item["Id"],
                        Name = item["Name"].ToString(),
                        Secret = item["Secret"].ToString(),
                        NumberOfMessages = 0
                    });
                }

                await GetMessagesForAllAccounts();
            }
            catch (Exception ex)
            {
                new MessageBoxCustom(ex.Message, MessageType.Error, MessageButtons.Ok).ShowDialog();
                Application.Current.Shutdown();
            }

            AppWindow.IsEnabled = true;
        }

        private async Task GetMessagesForAllAccounts()
        {
            try
            {
                for (int i = 0; i < accounts.Count; i++)
                {
                    accounts[i] = await WaproAPI.GetTokensForAccount(accounts[i]);
                }

                for (int i = 0; i < accounts.Count; i++)
                {
                    Account account = accounts[i];

                    if (!await GetConversationsCounter(account) && await RefreshTokens(account))
                    {
                        await GetConversationsCounter(account);
                    }
                }

                InfoRefreshTextBlock.Text = $"Ostatnio odświeżono o {DateTime.Now:HH:mm:ss}";
                int totalUnreadCount = accounts.Sum(x => x.NumberOfMessages);
                if (totalUnreadCount > 0) ShowToastNotification(totalUnreadCount);
            }
            catch (Exception ex)
            {
                new MessageBoxCustom(ex.Message, MessageType.Error, MessageButtons.Ok).ShowDialog();
            }

            AccountsItemsControl.ItemsSource = accounts;
        }

        private async Task GetMessagesForAccount(Account account)
        {
            try
            {
                if (!await GetConversationsCounter(account) && await RefreshTokens(account))
                {
                    await GetConversationsCounter(account);
                }
            }
            catch (Exception ex)
            {
                new MessageBoxCustom(ex.Message, MessageType.Error, MessageButtons.Ok).ShowDialog();
            }
        }

        private static async Task<bool> RefreshTokens(Account account)
        {
            RestResponse response = await OLXApi.RefreshTokens(account);

            if (response.IsSuccessful)
            {
                JObject tokens = JObject.Parse(response.Content);
                account.Access = tokens["access_token"].ToString();
                account.Refresh = tokens["refresh_token"].ToString();
            }
            else
            {
                account.Access = "";
                account.Refresh = "";
            }

            await WaproAPI.UpdateTokensForAccount(account);
            return response.IsSuccessful;
        }

        private static async Task<bool> GetThreads(Account account)
        {
            RestResponse response = await OLXApi.GetThreads(account);

            if (response.IsSuccessful)
            {
                dynamic data = JObject.Parse(response.Content);
                int totalUnreadCount = 0;

                foreach (JObject item in data.data)
                {
                    totalUnreadCount += (int)item.GetValue("unread_count");
                }

                account.NumberOfMessages = totalUnreadCount;
            }

            return response.IsSuccessful;
        }

        private static async Task<bool> GetConversationsCounter(Account account)
        {
            RestResponse response = await OLXApi.GetConversationsCounter(account);

            if (response.IsSuccessful)
            {
                dynamic data = JObject.Parse(response.Content);
                account.NumberOfMessages = (int)data.data.active.unread;
            }

            return response.IsSuccessful;
        }

        private void MinimizeApp(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void RefreshApp(object sender, RoutedEventArgs e)
        {
            AppWindow.IsEnabled = false;

            await GetMessagesForAllAccounts();

            AppWindow.IsEnabled = true;
        }

        private void ExitAppHandler() // Exit app handler
        {
            bool result = new MessageBoxCustom("Czy na pewno chcesz wyjść?", MessageType.Warning, MessageButtons.YesNo).ShowDialog() ?? false;
            if (result) Application.Current.Shutdown();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    e.Handled = true;
                    ExitAppHandler();
                    break;
                default:
                    return;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            GetAccounts();

            // Create and configure the timer
            timer = new Timer(TimerCallback, null, 5 * 60 * 1000, 5 * 60 * 1000); // 5 minutes in milliseconds
        }

        private void AuthButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Account account = accounts.FirstOrDefault(x => x.Id == (int)((System.Windows.Controls.Button)sender).Tag) ?? throw new Exception("Błąd podczas pobierania konta");

                AppWindow.IsEnabled = false;

                string state = GenerateRandomHash(6);

                Browser browser = new Browser(
                    $"https://www.olx.pl/oauth/authorize/?client_id={account.Id}&response_type=code&state={state}&scope=read+write+v2",
                    state
                );

                browser.Closed += async (s, args) =>
                {
                    if (browser.Code != null)
                    {
                        RestResponse response = await OLXApi.Authorizate(account, browser.Code);
                        if (!response.IsSuccessful)
                        {
                            new MessageBoxCustom("Błąd podczas autoryzacji", MessageType.Error, MessageButtons.Ok).ShowDialog();
                        }
                        else
                        {
                            JObject tokens = JObject.Parse(response.Content);
                            account.Access = tokens["access_token"].ToString();
                            account.Refresh = tokens["refresh_token"].ToString();
                            await WaproAPI.UpdateTokensForAccount(account);
                            await GetMessagesForAccount(account);
                        }
                    }
                    AppWindow.IsEnabled = true;
                };

                browser.ShowDialog();
            }
            catch (Exception ex)
            {
                new MessageBoxCustom(ex.Message, MessageType.Error, MessageButtons.Ok).ShowDialog();
            }
        }

        private static string GenerateRandomHash(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var hashBuilder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(0, chars.Length);
                hashBuilder.Append(chars[index]);
            }

            return hashBuilder.ToString();
        }

        private void ShowToastNotification(int totalUnreadCount)
        {
            string message = totalUnreadCount == 1 ? "nowa wiadomość" : totalUnreadCount < 5 ? "nowe wiadomości" : "nowych wiadomości";

            new ToastContentBuilder()
            .AddText("Nowe wiadomości!")
            .AddText($"{totalUnreadCount} {message}")
            .Show();
        }
    }
}
