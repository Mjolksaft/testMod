using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Terraria;

namespace testMod
{
    internal static class WebLogger
    {
        private static readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        public static void Notify(string action, string sender, string target, string detail, string result = "success")
        {
            Task.Run(() => SendNotification(action, sender, target, detail, result));
        }

        private static void SendNotification(string action, string sender, string target, string detail, string result)
        {
            int port = Netplay.ListenPort;
            string url = $"http://localhost:8080/mod/{port}/log";

            var payload = new
            {
                action,
                sender,
                target,
                detail,
                result
            };

            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = _client.PostAsync(url, content).GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                    Console.WriteLine($"WebLogger: {action} notification failed ({(int)response.StatusCode})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebLogger: {action} notification error: {ex.Message}");
            }
        }
    }
}
