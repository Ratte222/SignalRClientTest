#region snippet_MainWindowClass
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace SignalRChatClient
{
    public partial class MainWindow : Window
    {
        HubConnection connection;
        //const string _myAccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbW9iaWxlcGhvbmUiOiIrMzgwNjcxMzMxODE1IiwibmJmIjoxNjQxODA2NzE3LCJleHAiOjE2NDE4MTAzMTcsImlhdCI6MTY0MTgwNjcxNywiaXNzIjoiVHJha2luZyBPd2wgSXNzdWVyIiwiYXVkIjoiVHJha2luZyBPd2wgQXVkaWVuY2UifQ.waOd6GyhJSnwSDUROMZSQhSlooD2O3t_i0QLAOg3Hog";
        //const string _chatRelationshipId = "1";
        //const string _baseUrl = @"https://api.trackowl.vrealsoft.com";
        const string _baseUrl = @"https://localhost:5001";
        const string _baseUrlChatHub = _baseUrl + @"/chatHub";
        const string _baseUrlApi = _baseUrl + @"/api/";
        public MainWindow()
        {
            InitializeComponent();

            

            #region snippet_ClosedRestart
            //connection.Closed += async (error) =>
            //{
            //    await Task.Delay(new Random().Next(0,5) * 1000);
            //    await connection.StartAsync();
            //};
            #endregion
        }

        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {
            connection = new HubConnectionBuilder()
                .WithUrl(_baseUrlChatHub, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(tokenTextBox.Text);
                })
                .Build();
            #region snippet_ConnectionOn
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                   var newMessage = $"{user}: {message}";
                   messagesList.Items.Add(newMessage);
                });
            });
            #endregion
            connection.On<string>("Notify", (message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{message}";
                    messagesList.Items.Add(newMessage);
                });
            });
            connection.On<string>("MessageReadOk", (message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{message}";
                    messagesList.Items.Add(newMessage);
                });
            });
            connection.On<string>("AnswerGetUnreadedMessages", (message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{message}";
                    messagesList.Items.Add(newMessage);
                });
            });
            try
            {
                
                await connection.StartAsync();
                messagesList.Items.Add("Connection started");
                connectButton.IsEnabled = false;
                disconnectButton.IsEnabled = true;
                sendButton.IsEnabled = true;

                await connection.InvokeAsync("EnterToGroup",
                    chatRelationshipIdTextBox.Text);
                
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
            }
        }

        private async void disconnectButton_Click(object sender, RoutedEventArgs e)
        {
            await connection.InvokeAsync("EscapeFromGruop",
                    chatRelationshipIdTextBox.Text);
            await connection.DisposeAsync();
            connectButton.IsEnabled = true;
            disconnectButton.IsEnabled = false;
        }
        private async void messageReadedButton_Click(object sender, RoutedEventArgs e)
        {
            await connection.InvokeAsync("MessageRead",
                    @"{ ""TextMessageId"":12,
                        ""UserId"":13}");
        }
        private async void getDataButton_Click(object sender, RoutedEventArgs e)
        {
            await connection.InvokeAsync("GetUnreadedMessages",
                    @"{ ""ChatRelationshipId"":" + chatRelationshipIdTextBox.Text +
                    @"}"/*, ""UserId"":13}"*/);
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            if (sendViaHttpCheckBox.IsChecked.Value)
            {
                HttpClient httpClient = new HttpClient();
                string content = string.Concat("?Message=", messageTextBox.Text, "&ChatRelationshipId=", chatRelationshipIdTextBox.Text);
                //HttpContent httpContent =  new StringContent("", Encoding.UTF8);
                //httpContent.Headers.Add("Authorization", $"Bearer {tokenTextBox.Text}");
                var req = new HttpRequestMessage(HttpMethod.Post, _baseUrlApi + @"Message/sendMessage" + content);
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenTextBox.Text);

                HttpResponseMessage otpCode = httpClient.SendAsync(req).GetAwaiter().GetResult();
            }
            else
            {
                #region snippet_ErrorHandling
                try
                {
                    #region snippet_InvokeAsync
                    await connection.InvokeAsync("SendMessage",
                        /*userTextBox.Text*/ chatRelationshipIdTextBox.Text, messageTextBox.Text);
                    #endregion
                }
                catch (Exception ex)
                {
                    messagesList.Items.Add(ex.Message);
                }
                #endregion
            }
        }

        private void authorizeButton_Click(object sender, RoutedEventArgs e)
        {
            
            //string baseUrl = @"https://localhost:5001/api/";
            HttpClient httpClient = new HttpClient();
            string content = string.Concat("{ \"phoneNumber\": \"", phoneTextBox.Text, "\" }");
            HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            HttpResponseMessage otpCode = httpClient.PostAsync(_baseUrlApi + @"User/sendSMS", httpContent).GetAwaiter().GetResult();
            string otpCodeStr = otpCode.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            content = string.Concat("{ \"phoneNumber\": \"", phoneTextBox.Text, "\", \"otpCode\": ", otpCodeStr, " }");
            httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            HttpResponseMessage token = httpClient.PostAsync(_baseUrlApi + @"User/smsAuthenticate", httpContent).GetAwaiter().GetResult();
            tokenTextBox.Text = JsonConvert.DeserializeObject<AuthDto>(token.Content.ReadAsStringAsync().GetAwaiter().GetResult()).Token;
        }
    }

    public class AuthDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long? AvatarId { get; set; }
        public string Language { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
    }
}
#endregion
