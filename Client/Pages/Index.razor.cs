using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SignalRClient.ClientSettings;
using SignalRClient.DataModals;
using SignalRClient.Enums;
using SignalRClient.Helpers.HTTPHelper;
using SignalRClient.Helpers.HTTPHelper.TCPRequestForHttpRequest;

namespace SignalRHub.Client.Pages
{
    public partial class Index
    {
        public static Timer HalfSecondTimer = new Timer(HalfSecondTimerCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
        public static DateTime LastTimeDataReceived { get; set; } = DateTime.UtcNow;
        public static Dictionary<string, object> ProcessedData { get; set; } = new();
        public static string _IdsToGetDataFromApiCallCommaSeprated = "";
        public static string IdsToGetDataFromApiCall
        {
            get => _IdsToGetDataFromApiCallCommaSeprated;
            set
            {
                if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value))
                {
                    var trimmed = value.Trim();
                    List<string> idsAsStrring = new List<string>();
                    if (trimmed.Contains(',') || trimmed.Contains(' ') || trimmed.Contains(';'))
                    {

                        if (trimmed.Contains(','))
                        {
                            foreach (var id in trimmed.Split(','))
                            {
                                idsAsStrring.Add(id.Trim());
                            }
                        }
                        else if (trimmed.Contains(' '))
                        {
                            foreach (var id in trimmed.Split(' '))
                            {
                                idsAsStrring.Add(id.Trim());
                            }
                        }
                        else if (trimmed.Contains(';'))
                        {
                            foreach (var id in trimmed.Split(';'))
                            {
                                idsAsStrring.Add(id.Trim());
                            }
                        }
                        else
                        {
                            // only 1 id
                            //this case should not occur
                            idsAsStrring.Add(value.Trim());
                        }
                    }
                    else
                    {
                        idsAsStrring.Add(value.Trim());
                    }
                    if (idsAsStrring?.Count() > 0)
                    {
                        foreach (var item in idsAsStrring)
                        {
                            var valueInt = Convert.ToInt32(item);
                            if ((!Ids?.Contains(valueInt)) ?? false)
                            {
                                Ids?.Add(valueInt);
                            }
                        }
                    }
                }
                _IdsToGetDataFromApiCallCommaSeprated = value;
            }
        }
        public static Dictionary<int, Dictionary<int, string>> DataGridData { get; set; } = new();
        public int Id { get; set; } = 1;
        public HttpClient HttpClient { get; set; }
        [Inject] public IConfiguration _configuration { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private NavigationManager NavigationManager { get; set; }
        public bool IsAdPort { get; set; } = false;

        public string MessageToSend { get; set; } = string.Empty;

        public List<int> SelectedIdToSubscribeOrUnSubcribe { get; set; } = new List<int>();

        public List<string> SelectedItemsToSubscribeOrUnSubscribeEncruptedTags
        {
            get
            {
                List<string> encruptedTags = new List<string>();
                try
                {
                    SelectedIdToSubscribeOrUnSubcribe?.ForEach(IdEach =>
                    {
                        //string encruptionKey = _configuration["TagEncruptionKey"];
                        string encriptedTagForRedisData = SignalRClient.SignalRClient.GetEncryptedTag(
                          receiveType: SignalRReceiveType.DataType1,
                          etityId: IdEach.ToString(),
                          out string nonEncruptedTagOnlyToDebug5,
                          encruptionKey: ClientSettings.TagEncruptionKey,
                          timeInterValForRecivingData_inMS: (int)SignalRDataReciveTimeInterval.Milliseconds200);
                        encruptedTags.Add(encriptedTagForRedisData);

                        string encriptedTagForData = SignalRClient.SignalRClient.GetEncryptedTag(
                                receiveType: SignalRReceiveType.DataType2,
                                etityId: IdEach.ToString(),
                                out string nonEncruptedTagOnlyToDebug4,
                                encruptionKey: ClientSettings.TagEncruptionKey,
                                timeInterValForRecivingData_inMS: (int)SignalRDataReciveTimeInterval.Milliseconds200);

                        encruptedTags.Add(encriptedTagForData);

                        string encriptedTagForModal = SignalRClient.SignalRClient.GetEncryptedTag(
                             receiveType: SignalRReceiveType.DataType3,
                             etityId: IdEach.ToString(),
                             out string nonEncruptedTagOnlyToDebug1,
                             encruptionKey: ClientSettings.TagEncruptionKey,
                             timeInterValForRecivingData_inMS: (int)SignalRDataReciveTimeInterval.Milliseconds200);
                        encruptedTags.Add(encriptedTagForModal);

                        string encriptedTagForData4Data = SignalRClient.SignalRClient.GetEncryptedTag(
                                receiveType: SignalRReceiveType.DataType4,
                                etityId: IdEach.ToString(),
                                out string nonEncruptedTagOnlyToDebug2,
                                encruptionKey: ClientSettings.TagEncruptionKey,
                                timeInterValForRecivingData_inMS: (int)SignalRDataReciveTimeInterval.Milliseconds200);

                        encruptedTags.Add(encriptedTagForData4Data);

                        string encriptedTagForDataModal = SignalRClient.SignalRClient.GetEncryptedTag(
                             receiveType: SignalRReceiveType.DataType5,
                             etityId: IdEach.ToString(),
                             out string nonEncruptedTagOnlyToDebug3,
                             encruptionKey: ClientSettings.TagEncruptionKey,
                             timeInterValForRecivingData_inMS: (int)SignalRDataReciveTimeInterval.Milliseconds200);
                        encruptedTags.Add(encriptedTagForDataModal);


                        string pingMessage = SignalRClient.SignalRClient.GetEncryptedTag(
                             receiveType: SignalRReceiveType.Ping,
                             etityId: IdEach.ToString(),
                             out string nonEncruptedTagOnlyToDebugPing,
                             encruptionKey: ClientSettings.TagEncruptionKey,
                             timeInterValForRecivingData_inMS: (int)SignalRDataReciveTimeInterval.Milliseconds200);
                        encruptedTags.Add(pingMessage);

                        string DataMessage = SignalRClient.SignalRClient.GetEncryptedTag(
                             receiveType: SignalRReceiveType.DataType6,
                             etityId: IdEach.ToString(),
                             out string nonEncruptedTagOnlyToDebug3DataMessage,
                             encruptionKey: ClientSettings.TagEncruptionKey,
                             timeInterValForRecivingData_inMS: (int)SignalRDataReciveTimeInterval.Milliseconds200);
                        encruptedTags.Add(DataMessage);

                        string encriptedTagForIOLogData = SignalRClient.SignalRClient.GetEncryptedTag(
                               receiveType: SignalRReceiveType.Logs,
                               etityId: IdEach.ToString(),
                               out string encriptedTagForIOLogDataForDebug,
                               encruptionKey: ClientSettings.TagEncruptionKey,
                               timeInterValForRecivingData_inMS: (int)SignalRDataReciveTimeInterval.Milliseconds200);

                        encruptedTags.Add(encriptedTagForIOLogData);

                        encruptedTags.Add(SignalRClient.SignalRClient.GetDebugTag());
                    });
                }
                catch (Exception ex)
                {

                    throw;
                }
                return encruptedTags;
            }
        }

        public string SendMessageReturnStatus = string.Empty;
        public string URLorIP = @"http://localhost:300/SignalRHub";
        public string URLorIPEnterded = @"https://example.example.com/signalRHub";

        public List<string> SelectedTagsToSendMessage { get; set; } = new List<string>();
        public List<string> TagsToSendMessage { get; set; } = new List<string>()
        {
                "hjlkuhiiusfjasf7sf8a7s98f7a9f79",
                "dfoisufiasuf98798f7as9s7f9a7f",
               "lkjfglsjlkdsljg98",
               "lkdjlksjflkjdsf98d7f89s7f9sd7f9ds7f",
               "defaulttag",
        };

        public static List<int> Ids { get; set; } = new List<int>()
        {
           1,
           2,
           3,
           0,
        };
        public static async void HalfSecondTimerCallback(object data)
        {
            if ((DateTime.UtcNow - LastTimeDataReceived).TotalSeconds > 10)
            {
                await SignalRClient.SignalRClient.SubscribeToTagsIfHubIsAlreadyConnected(new() { "ping" });
            }
        }

        protected override async Task OnInitializedAsync()
        {
            ConnectOnClick();
        }
        protected async Task ConnectOnClick()
        {
            var uri = new Uri(NavigationManager.Uri);
            if (IsAdPort)
            {
                URLorIP = $"{uri.Scheme}://{uri.Host}:{uri.Port}" + "/SignalRHub";
            }
            else
            {
                URLorIP = $"{uri.Scheme}://{uri.Host}" + "/SignalRHub";
            }

            if (await SignalRClient.SignalRClient.ConnectToHub(signalRHubURL: URLorIP,
                                                    appName: "signalrhub",
                                                    jwtToken: "sdfghj456789sdfghj45678sdfghj45678sdfghj45678dfghj34567sdfgh34567dfghj45678sdfghj345678sdfghj3456",
                                                                   tagsToSubsCribe: new List<string> { SignalRClient.SignalRClient.GetEncruptedErrorLogTag(), SignalRClient.SignalRClient.GetDebugTag() }

                                                    )
                )
            {
                SignalRClient.SignalRClient.MessageReceived -= HandleMessageReceived;
                SignalRClient.SignalRClient.MessageReceived += HandleMessageReceived;
            }
        }
        protected async Task ConnectURLorIPEnterded()
        {

            if (await SignalRClient.SignalRClient.ConnectToHub(signalRHubURL: URLorIPEnterded,
                                                    appName: "signalrhub",
                                                    jwtToken: "sdfghj456789sdfghj45678sdfghj45678sdfghj45678dfghj34567sdfgh34567dfghj45678sdfghj345678sdfghj3456",
                                                                   tagsToSubsCribe: new List<string> { SignalRClient.SignalRClient.GetEncruptedErrorLogTag(), SignalRClient.SignalRClient.GetDebugTag() }

                                                    )
                )
            {
                SignalRClient.SignalRClient.MessageReceived -= HandleMessageReceived;
                SignalRClient.SignalRClient.MessageReceived += HandleMessageReceived;
            }
        }

        //calls SignalR Hub function named as send message to send signalR message
        private async Task AddPortIntoConnectedURL()
        {
            IsAdPort = true;
            //OnInitializedAsync();
            ConnectOnClick();
        }
        private async Task RemovePortFromConnectedURL()
        {
            IsAdPort = false;
            //OnInitializedAsync();
            ConnectOnClick();
        }
        private async Task SubscribeSelectedTags()
        {
            var selectedItemsToSubscribeOrUnSubscribeEncruptedTags = SelectedItemsToSubscribeOrUnSubscribeEncruptedTags;
            await SignalRClient.SignalRClient.SubscribeToTagsIfHubIsAlreadyConnected(selectedItemsToSubscribeOrUnSubscribeEncruptedTags);
        }

        private async Task UnSubscribeSelectedTags()
        {

            await SignalRClient.SignalRClient.UnSubscribeSelectedTagsIfConnected(SelectedItemsToSubscribeOrUnSubscribeEncruptedTags);
        }
        private async Task MakeTCPCall()
        {
            byte[] requestBytesTCP = await CreateTCPRequestHelper.GetDataMessage(
                                messageTyepebyte: (byte)FrameMessageType.DataType3,
                                property1: 1,
                                property2: 2,
                                Id: Id//  id 
                                );




            var body = new SignalRClient.Helpers.HTTPHelper.ApiRequests.TCPRequestDTO()
            {
                TCPServerIP = "111.123.134.345",
                TCPServerPort = 303,
                TCPRequest = requestBytesTCP,
                EntityId = Id,
                UseClientTimer = false,
                RunAfterMilliconds = SignalRDataReciveTimeInterval.Milliseconds200,
                SignalRRequestType = SignalRReceiveType.DataType1
            };


            string url = ClientSettings.SignalRServerUrl + @"/api/SignalR/MakeTCPCall";

            HttpResponseMessage response = await HttpHelper.HTTPPost(url, body);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            else
            {
                Console.WriteLine($"HTTP request failed with status code: {response.StatusCode}");
            }
        }

        private async Task AddToIdList()
        {
            Ids.Add(Id);

            StateHasChanged();
        }
        private async Task GetDataFromMultiSelectDropDown()
        {
            _ = Task.Run(async () =>
             {
                 if (SelectedIdToSubscribeOrUnSubcribe?.Count > 0)
                 {
                     if (SelectedIdToSubscribeOrUnSubcribe?.Count > 0)
                     {
                         int maxConcurrentDownloads = 2; // Set the maximum concurrent downloads
                         SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrentDownloads);

                         foreach (var device in SelectedIdToSubscribeOrUnSubcribe)
                         {

                             string url = $"https://example.com:23490?Id={device}"; // Replace with the URL you want to call

                             try
                             {
                                 await semaphore.WaitAsync(); // Wait if the maximum concurrent downloads are reached
                                 await JSRuntime.InvokeVoidAsync("window.open", url, "_blank");
                             }
                             catch (Exception ex)
                             {
                                 continue; // Handle the exception or continue to the next URL
                             }
                             finally
                             {
                                 semaphore.Release(); // Release the semaphore after download
                             }
                         }
                     }
                 }
             }
  );

        }

        private async Task GetDataForSelectedIdsFromMultiselectMenuFromOtherEnvironment()
        {

            _ = Task.Run(async () =>
              {
                  if (SelectedIdToSubscribeOrUnSubcribe?.Count > 0)
                  {
                      if (SelectedIdToSubscribeOrUnSubcribe?.Count > 0)
                      {
                          int maxConcurrentDownloads = 2; // Set the maximum concurrent downloads
                          SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrentDownloads);

                          foreach (var device in SelectedIdToSubscribeOrUnSubcribe)
                          {

                              string url = $"https://test-argus.example.com:23490?Id={device}"; // Replace with the URL you want to call

                              try
                              {
                                  await semaphore.WaitAsync(); // Wait if the maximum concurrent downloads are reached
                                  await JSRuntime.InvokeVoidAsync("window.open", url, "_blank");
                              }
                              catch (Exception ex)
                              {
                                  continue; // Handle the exception or continue to the next URL
                              }
                              finally
                              {
                                  semaphore.Release(); // Release the semaphore after download
                              }
                          }
                      }
                  }
              }
              );
        }
        private async Task GetDataForSelectedIdsFromMultiselectMenuFromAnotherEnvironment()
        {

            _ = Task.Run(async () =>
            {
                if (SelectedIdToSubscribeOrUnSubcribe?.Count > 0)
                {
                    if (SelectedIdToSubscribeOrUnSubcribe?.Count > 0)
                    {
                        int maxConcurrentDownloads = 2; // Set the maximum concurrent downloads
                        SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrentDownloads);

                        foreach (var device in SelectedIdToSubscribeOrUnSubcribe)
                        {

                            string url = $"https://example.com:23490?Id={device}"; // Replace with the URL you want to call

                            try
                            {
                                await semaphore.WaitAsync(); // Wait if the maximum concurrent downloads are reached
                                await JSRuntime.InvokeVoidAsync("window.open", url, "_blank");
                            }
                            catch (Exception ex)
                            {
                                continue; // Handle the exception or continue to the next URL
                            }
                            finally
                            {
                                semaphore.Release(); // Release the semaphore after download
                            }
                        }
                    }
                }
            }
            );
        }
        private async Task SendMessage()
        {
            //SendMessageReturnStatus = await SignalRClient.SendMessage(ClientSettings.SignalRHubUrl, SelectedTagsToSendMessage, MessageToSend);
            SendMessageReturnStatus = await SignalRClient.SignalRClient.SendMessage(hubCompleteurl: ClientSettings.SignalRHubUrl,
                                                                        appName: "signalrhub",
                                                                        tagsOnWhichToSend: SelectedTagsToSendMessage,
                                                                        nonSerialezedDataToSend: MessageToSend,
                                                                        jwtToken: "sdfghj456789sdfghj45678sdfghj45678sdfghj45678dfghj34567sdfgh34567dfghj45678sdfghj345678sdfghj3456");
            StateHasChanged();
        }

        private async void HandleMessageReceived(string tag, string jsonData)
        {
            LastTimeDataReceived = DateTime.UtcNow;
            if (string.IsNullOrEmpty(jsonData) || jsonData.Length < 3)
            {
                return;
            }
            if (string.IsNullOrEmpty(jsonData) || jsonData.Length < 3)
            {
                return;
            }
            if (jsonData.Contains("dataModal"))
            {
                SignalrClientDataModal data = SignalRClient.SignalRClient.GetDataObject<SignalrClientDataModal>(jsonData);

                //if (jsonData.Contains("PhaseStatusYellows"))
                
            } 
            if (ProcessedData?.Count() > 0 && ProcessedData.Keys.Contains(tag))
            {
                ProcessedData[tag] = jsonData;
            }
            else
            {
                ProcessedData.Add(tag, jsonData);
            }

            ProcessedData = ProcessedData;
            StateHasChanged();
        }

        public async ValueTask DisposeAsync()
        {
            //if (hubConnection is not null)
            //{
            //    await hubConnection.DisposeAsync();
            //}
            await this.DisposeAsync();
        }
    }
}