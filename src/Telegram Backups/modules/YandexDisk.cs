using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace YaDiskTools
{
    class YaDisk(string? token)
    {
        // Yandex disk client
        public readonly HttpClient client = new() { Timeout = TimeSpan.FromMinutes(10) };
        public readonly string token = token!;

        public async Task<string> CreateFolder(string folder_path)
        {
            const string base_url = "https://cloud-api.yandex.net/v1/disk/resources?";
            string base_path = folder_path.Replace("/", "%2F");

            // Create folder
            using var requestMessage = new HttpRequestMessage(
                HttpMethod.Put,
                base_url + $"path={base_path}&overwrite=true"
            );
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("OAuth", token);

            using HttpResponseMessage response = await client.SendAsync(requestMessage);
            int status_code = (int)response.StatusCode;

            if (status_code != 409 && status_code != 201)
            {
                throw new Exception();
            }

            return base_path;
        }

        public async Task<string?> UploadBackupFile(string? file_path, string dir_path)
        {
            // Check if file exists
            if (file_path is null)
                return null;

            // Create upload url
            const string base_url = "https://cloud-api.yandex.net/v1/disk/resources/upload?";

            string file_name = Path.GetFileName(file_path);
            string upload_path = dir_path.Replace("/", "%2F") + file_name;

            // Get upload url
            using var requestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                base_url + $"path={upload_path}&overwrite=true"
            );
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("OAuth", token);

            using HttpResponseMessage upload_url_response = await client.SendAsync(requestMessage);
            upload_url_response.EnsureSuccessStatusCode();

            var jsonString = await upload_url_response.Content.ReadAsStringAsync();
            var json_data = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

            string upload_url = json_data!["href"].ToString();

            // Upload json file
            using var uploadMessage = new HttpRequestMessage(HttpMethod.Put, upload_url);
            uploadMessage.Headers.Authorization = new AuthenticationHeaderValue("OAuth", token);

            await using var stream = File.OpenRead(file_path);
            using var content = new MultipartFormDataContent
            {
                { new StreamContent(stream), "file", file_name }
            };

            uploadMessage.Content = content;
            using HttpResponseMessage upload_file_response = await client.SendAsync(uploadMessage);

            upload_file_response.EnsureSuccessStatusCode();

            return file_name;
        }
    }
}
