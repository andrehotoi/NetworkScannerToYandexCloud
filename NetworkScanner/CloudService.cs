using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace NetworkScanner
{
    internal class CloudService
    {
        private readonly HttpClient _http = new HttpClient();
        private readonly string _token = "YOUR_YANDEX_TOKEN_HERE";

        public CloudService()
        {
            _http.DefaultRequestHeaders.Add("Authorization", $"OAuth {_token}");
        }

        public async Task SaveReportAsync(List<DeviceInfo> devices)
        {
            await CreateFolderIfNotExists();
            string json = JsonSerializer.Serialize(devices);
            string fileName = $"scan_{DateTime.Now:yyyy-MM-dd_HH-mm}.json";
            string path = $"/NetworkScanner/{fileName}";
            string url = $"https://cloud-api.yandex.net/v1/disk/resources/upload?path={path}&overwrite=true";

            var response = await _http.GetAsync(url);

            string responseText = await response.Content.ReadAsStringAsync();

            var json_response = JsonSerializer.Deserialize<JsonElement>(responseText);
            string href = json_response.GetProperty("href").GetString();

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var uploadResponse = await _http.PutAsync(href, content);

            if (uploadResponse.IsSuccessStatusCode)
                Console.WriteLine($"Отчёт сохранён: {fileName}");
            else
                Console.WriteLine($"Ошибка загрузки: {uploadResponse.StatusCode}");
        }

        private async Task CreateFolderIfNotExists()
        {
            string url = "https://cloud-api.yandex.net/v1/disk/resources?path=/NetworkScanner";
            var response = await _http.PutAsync(url, null);
            
        }

        public async Task<List<string>> GetReportsListAsync()
        {
            string url = "https://cloud-api.yandex.net/v1/disk/resources?path=/NetworkScanner";
            var response = await _http.GetAsync(url);
            string responseText = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(responseText);

            var items = json.GetProperty("_embedded").GetProperty("items");

            var reports = new List<string>();
            foreach (var item in items.EnumerateArray())
            {
                string name = item.GetProperty("name").GetString();
                if (name.EndsWith(".json"))
                    reports.Add(name);
            }

            return reports;
        }

        public async Task<List<DeviceInfo>> DownloadReportAsync(string fileName)
        {
            string url = $"https://cloud-api.yandex.net/v1/disk/resources/download?path=/NetworkScanner/{fileName}";
            var response = await _http.GetAsync(url);
            string responseText = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(responseText);
            string href = json.GetProperty("href").GetString();

            string fileContent = await _http.GetStringAsync(href);

            var devices = JsonSerializer.Deserialize<List<DeviceInfo>>(fileContent);
            return devices;
        }

        public List<CompareResult> CompareReports(List<DeviceInfo> oldReport, List<DeviceInfo> newReport)
        {
            var results = new List<CompareResult>();

            var allIps = oldReport.Select(d => d.IpAddress)
                .Union(newReport.Select(d => d.IpAddress))
                .Distinct();

            foreach (string ip in allIps)
            {
                var oldDevice = oldReport.FirstOrDefault(d => d.IpAddress == ip);
                var newDevice = newReport.FirstOrDefault(d => d.IpAddress == ip);

                var result = new CompareResult
                {
                    IpAddress = ip,
                    OldDevice = oldDevice,
                    NewDevice = newDevice
                };

                if (oldDevice == null)
                {
                    result.Status = CompareStatus.New;
                }
                else if (newDevice == null)
                {
                    result.Status = CompareStatus.Lost;
                }
                else
                {
                    var changes = new List<string>();

                    if (oldDevice.HostName != newDevice.HostName)
                        changes.Add($"Hostname: было {oldDevice.HostName} - стало {newDevice.HostName}");

                    if (oldDevice.OsGuess != newDevice.OsGuess)
                        changes.Add($"ОС: было {oldDevice.OsGuess} - стало {newDevice.OsGuess}");

                    if (oldDevice.Manufacturer != newDevice.Manufacturer)
                        changes.Add($"Производитель: было {oldDevice.Manufacturer} - стало {newDevice.Manufacturer}");

                    if (oldDevice.MacAddress != newDevice.MacAddress)
                        changes.Add($"MAC: было {oldDevice.MacAddress} - стало {newDevice.MacAddress}");

                    if (Math.Abs(oldDevice.PingTime - newDevice.PingTime) > 10)
                        changes.Add($"Ping: было {oldDevice.PingTime}мс - стало {newDevice.PingTime}мс");

                    var addedPorts = newDevice.OpenPorts.Except(oldDevice.OpenPorts).ToList();
                    var removedPorts = oldDevice.OpenPorts.Except(newDevice.OpenPorts).ToList();

                    if (addedPorts.Any())
                        changes.Add($"Новые порты: {string.Join(", ", addedPorts)}");

                    if (removedPorts.Any())
                        changes.Add($"Закрытые порты: {string.Join(", ", removedPorts)}");

                    result.Changes = changes;
                    result.Status = changes.Any() ? CompareStatus.Changed : CompareStatus.Unchanged;
                }

                results.Add(result);
            }

            return results.OrderBy(r => int.Parse(r.IpAddress.Split('.').Last())).ToList();
        }
    }

}
