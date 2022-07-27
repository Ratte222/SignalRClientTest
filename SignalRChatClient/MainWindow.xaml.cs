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
        HubConnection notificationConnection;
        //const string _myAccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbW9iaWxlcGhvbmUiOiIrMzgwNjcxMzMxODE1IiwibmJmIjoxNjQxODA2NzE3LCJleHAiOjE2NDE4MTAzMTcsImlhdCI6MTY0MTgwNjcxNywiaXNzIjoiVHJha2luZyBPd2wgSXNzdWVyIiwiYXVkIjoiVHJha2luZyBPd2wgQXVkaWVuY2UifQ.waOd6GyhJSnwSDUROMZSQhSlooD2O3t_i0QLAOg3Hog";
        //const string _chatRelationshipId = "1";
        //const string _baseUrl = @"https://api.trackowl.vrealsoft.com";
        const string _baseUrl = @"https://localhost:5001";
        const string _baseUrlChatHub = _baseUrl + @"/chatHub";
        const string _baseUrlNotificationHub = _baseUrl + @"/notificationHub";
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
            connection.Closed += async (errore) =>
            {
                messagesList.Items.Add(errore.Message);
                await Connect();
            };
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
            await Connect();
        }

        private async Task notificationConnect()
        {
            try
            {
                await notificationConnection.StartAsync();
                BT_NotificationDisconnect.IsEnabled = true;
                BT_GetNotification.IsEnabled = true;
                BT_NotificationConnect.IsEnabled = false;
            }
            catch(Exception ex)
            {
                notificationMessagesList.Items.Add(ex.Message);
            }
        }

        private async Task Connect()
        {
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

        private async void BT_NotificationConnect_Click(object sender, RoutedEventArgs e)
        {
            notificationConnection = new HubConnectionBuilder()
                .WithUrl(_baseUrlNotificationHub, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(TB_NotificationAuthToken.Text);
                })
                .Build();
            notificationConnection.Closed += async (errore) =>
            {
                notificationMessagesList.Items.Add(errore.Message);
                await notificationConnect();
            };
            //#region snippet_ConnectionOn
            //notificationConnection.On<string, string>("ReceiveMessage", (user, message) =>
            //{
            //    this.Dispatcher.Invoke(() =>
            //    {
            //       var newMessage = $"{user}: {message}";
            //        notificationMessagesList.Items.Add(newMessage);
            //    });
            //});
            //#endregion
            notificationConnection.On<string>("UserAcceptInvite", (message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{message}";
                    notificationMessagesList.Items.Add(newMessage);
                });
            });
            notificationConnection.On<string>("SendNotifications", (message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{message}";
                    notificationMessagesList.Items.Add(newMessage);
                });
            });
            notificationConnection.On<string>("ViewNotificationOk", (message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{message}";
                    notificationMessagesList.Items.Add(newMessage);
                });
            });
            await notificationConnect();
        }

        private async void BT_GetNotification_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await notificationConnection.InvokeAsync("GetNotifications",
                //$"{{\"PageNumber\": 1, \"PageLength\": 20 }}"
                @"{}"
                );
                await notificationConnection.InvokeAsync("GetActualNotifications",
                $"{{\"PageNumber\": 1, \"PageLength\": 20 }}"
                //@"{}"
                );
            }
            catch(Exception ex)
            {
                notificationMessagesList.Items.Add(ex.Message);
            }
        }

        private async void disconnectButton_Click(object sender, RoutedEventArgs e)
        {
            connectButton.IsEnabled = true;
            disconnectButton.IsEnabled = false;
            try
            {
                await connection.InvokeAsync("EscapeFromGruop",
                      chatRelationshipIdTextBox.Text);
                await connection.DisposeAsync();                
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
            }
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

        private async void BT_NotificationDisconnect_Click(object sender, RoutedEventArgs e)
        {
            try { await notificationConnection.DisposeAsync(); }
            catch(Exception ex)
            {
                notificationMessagesList.Items.Add(ex.Message);
            }
            finally
            {
                BT_GetNotification.IsEnabled = false;
                BT_NotificationConnect.IsEnabled = true;
                BT_NotificationDisconnect.IsEnabled = false;
            }
        }

        private async void BT_ViewNotification_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await notificationConnection.InvokeAsync("ViewNotification",
                    @"{ ""InviteNotificationId"":" + TB_NotificationId.Text +
                    @"}");
            }
            catch(Exception ex)
            {
                notificationMessagesList.Items.Add(ex.Message);
            }
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
