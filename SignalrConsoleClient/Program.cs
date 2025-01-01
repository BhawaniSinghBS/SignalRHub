using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SignalrConsoleClient
{
    internal class Program
    {
        static DateTime TimeOfFileCreated = DateTime.UtcNow;
        static string filePath = $@"C:\Users\UserName\Desktop\Folder1\{DateTime.UtcNow:dd-MM-yyyy HH-mm-ss}";

        static void Main(string[] args)
        {
            Console.WriteLine("Signalr console client");
            Console.WriteLine("Press key to continue");
            //Console.ReadKey();
            List<string> list = new List<string>()
            {
                SignalRClient.SignalRClient.GetEncruptedErrorLogTag()
            };
            for (int i = 0; i <= 60000; i++)
            {
                if (i != int.MinValue)
                {
                    continue;
                }
                Console.Write($"Tag subcribed for : {i}"); 

                var tagA = SignalRClient.SignalRClient.GetEncryptedTag(
                    SignalRClient.Enums.SignalRReceiveType.DataType2, i.ToString(), out string nonEncruptedTag4);
                list.Add(tagA); 
            }

            SignalRClient.SignalRClient.ConnectToHub(signalRHubURL: "https://localhost:300/SignalRHub",
                                                                appName: "SignalRHub",
                                                                jwtToken: "kjhdsfkjhfkdsfh87979fs979fs987f9s7f9dsfjdskfjdskfj",
                                                                               tagsToSubsCribe: list,

                                                                               functionToHandleDatasAtClientApplication: HandleMessageReceived).Wait();

            Console.WriteLine($"Subscribed and connected at {DateTime.UtcNow}");

            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
        }
        private static async void HandleMessageReceived(string tag, string jsonData)
        {
            if (tag == "ping")
            {
                return;
            }
            List<string> linesToWrite = new List<string>();



            linesToWrite.Add($"--------------------------------------------------------------------------------------------------------------------");
            linesToWrite.Add($"Data recived at : {DateTime.UtcNow} ");
            JObject jsonObject = JsonConvert.DeserializeObject<JObject>(jsonData);
           
            linesToWrite.Add(jsonObject.ToString());
            Console.WriteLine("--------------------------------------------------------------------------------------------");
            Console.WriteLine("--------------------------------------------------------------------------------------------");
            Console.WriteLine("--------------------------------------------------------------------------------------------");

            //// Iterate through properties and print each property and value
            foreach (var property in jsonObject.Properties())
            {
                linesToWrite.Add($"{property.Name}: {property.Value}");

                if (property.Name == "modalTypeName")
                {
                    Console.WriteLine($"{property.Name}: {property.Value}");
                }

                if (property.Name == "dataModal")
                {

                    JObject dataModalObject = (JObject)property.Value;

                    if (dataModalObject.TryGetValue("signalRTimeStamp", out JToken signalRTimeStampValue))
                    {
                        Console.WriteLine($"SignalRTimeStamp: {signalRTimeStampValue}");
                        Console.WriteLine(DateTime.UtcNow);
                    }
                }
            }
            Console.WriteLine($"--------------------------------------------------------------------------------------------------------------------");
            linesToWrite.Add($"--------------------------------------------------------------------------------------------------------------------");
            System.IO.File.AppendAllLines(filePath, linesToWrite);
        }
    }
}