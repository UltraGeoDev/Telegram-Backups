using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace YaDiskTools 
{
    class YaDisk (string? token, ILogger logger) {
        public readonly HttpClient client = new();
        public readonly string token = token!;

        public readonly ILogger logger = logger;

        public async Task CreateFolder() {
            const string base_url = "https://cloud-api.yandex.net/v1/disk/resources?";
            const string base_path = "%2F" + "backups";   

            // Create folder
            using var requestMessage = new HttpRequestMessage(HttpMethod.Put, base_url + $"path={base_path}&overwrite=true");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("OAuth", token);

            using HttpResponseMessage response = await client.SendAsync(requestMessage);
            int status_code = (int)response.StatusCode;

            if (status_code != 409 && status_code != 201) {
                logger.LogError("Failed to create folder in yandex disk");
                throw new Exception();
            }


            logger.LogInformation("Created backup folder in yandex disk");
        }

        public async Task UploadBackupJSON(string json_path) {
            const string base_url = "https://cloud-api.yandex.net/v1/disk/resources/upload?";

            string file_name = Path.GetFileName(json_path);
            string upload_path = "backups" + "%2F" + file_name;

            // Get upload url
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, base_url + $"path={upload_path}&overwrite=true");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("OAuth", token);

            using HttpResponseMessage response = await client.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();        

            var jsonString = await response.Content.ReadAsStringAsync();
            var json_data = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

            string upload_url = json_data!["href"].ToString();

            // Upload json file
            using var uploadMessage = new HttpRequestMessage(HttpMethod.Put, upload_url);
            uploadMessage.Headers.Authorization = new AuthenticationHeaderValue("OAuth", token);

            await using var stream = File.OpenRead(json_path);
            using var content = new MultipartFormDataContent {
                { new StreamContent(stream), "file", file_name}
            };

            uploadMessage.Content = content;
            await client.SendAsync(uploadMessage);
        }
    }
}
