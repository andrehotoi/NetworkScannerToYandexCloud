using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net.Http;


namespace NetworkScanner
{
    public enum ScanMode
    {
        Quick,      
        Standard    
    }
    internal class scanner
    {
        private static readonly int[] QuickPorts = new[]
        {
            80, 443, 22, 21, 3389, 8080, 8443, 23, 25, 53
        };

        private static readonly int[] StandardPorts = new[]
        {
            80, 443, 22, 21, 3389, 8080, 8443, 23, 25, 53, 62078,
            110, 143, 5432, 6379, 554, 1883, 5000, 8888, 135,
            137, 139, 445, 1080, 1194, 1723, 4444, 5900, 9090, 5555
        };

        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<DeviceInfo> PingHostAsync(string ip, ScanMode mode = ScanMode.Quick)
        {
            var device = new DeviceInfo();
            device.IpAddress = ip;
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(ip, 3000);
                if (reply.Status == IPStatus.Success)
                {
                    device.IsAlive = true;  
                    device.PingTime = reply.RoundtripTime;
                    device.HostName = GetHostName(ip);
                    if (reply.Options != null)
                        device.OsGuess = GuessOs(reply.Options.Ttl);
                    device.MacAddress = GetMacAddress(ip);
                    device.Manufacturer = await GetManufacturerAsync(device.MacAddress);
                }
            }
            catch
            {
                device.IsAlive = false;
            }
            return device;
        }
        private String GetHostName(string ip)
        {
            try
            {
                var HostEntry = Dns.GetHostEntry(ip);
                var HostName = HostEntry.HostName;
                return HostName;
            }
            catch
            {
                return "Host не найден";
            }
        }
        public async Task<List<DeviceInfo>> ScanRangeAsync(string BaseIp, int Start, int End, ScanMode mode = ScanMode.Quick)
        {
            var results = new List<DeviceInfo>();
            for (int i = Start; i < End + 1; i++)
            {
                string ip = $"{BaseIp}.{i}";
                Console.WriteLine();
                Console.WriteLine($"Проверяю {ip}...");
                var device = await PingHostAsync(ip);
                if (device.IsAlive == false)
                {
                    Console.WriteLine($"Нет ответа от {ip}");
                }
                else
                {
                    Thread.Sleep(100);
                    Console.WriteLine($"Устройство с {ip} найдено");
                    Console.WriteLine($"Ping: {device.PingTime}");
                    Console.WriteLine($"HostName: {device.HostName}");
                    Console.WriteLine($"ОС: {device.OsGuess}");
                    Thread.Sleep(50);
                    Console.WriteLine($"MacAddress: {device.MacAddress}");
                    Thread.Sleep(50);
                    Console.WriteLine($"Производитель: {device.Manufacturer}");
                    Thread.Sleep(100);
                    Console.WriteLine();
                    device.OpenPorts = CheckPorts(ip, mode == ScanMode.Quick ? QuickPorts : StandardPorts);
                    results.Add(device);
                }

                Console.WriteLine();
            }
            return results;
        }

        private List<int> CheckPorts(string ip, int[] ports)
        {
            var OpenPorts = new List<int>();
            Console.WriteLine("Начинаю анализ портов...");
            foreach (int port in ports)
            {
                using var client = new TcpClient();
                try
                {
                    var connectTask = client.ConnectAsync(ip, port);
                    if (connectTask.Wait(500) && client.Connected)
                    {
                        Console.WriteLine($"Порт {port} открыт на {ip}");
                        OpenPorts.Add(port);
                    }
                    
                }
                catch
                {
                }
            }

            Console.WriteLine("Анализ портов завершён");
            return OpenPorts;
        }

        private string GuessOs(int ttl)
        {
            if (ttl <= 64) return "Linux / Android / IOS / macOS";
            if (ttl <= 128) return "Windows";
            if (ttl <= 255) return "Сетевое оборудование";
            return "Неизвестная ОС";
        }

        private string GetMacAddress(string ip)
        {
            if (!ip.StartsWith("192.168.") && !ip.StartsWith("10."))
                return "недоступен";
            try
            {
                var process = new Process();
                process.StartInfo.FileName = "arp";
                process.StartInfo.Arguments = "-a";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                foreach (string line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {

                    if (line.TrimStart().StartsWith(ip + " ") ||
                        line.TrimStart().StartsWith(ip + "\t"))
                    {
                        var parts = line.Split(
                            new char[] { ' ', '\t' },
                            StringSplitOptions.RemoveEmptyEntries
                        );
                        if (parts.Length >= 2)
                            return parts[1];
                    }
                }
                return "не найден";
            }
            catch
            {
                return "ошибка";
            }
        }

        private async Task<string> GetManufacturerAsync(string mac)
        {
            if (mac == "недоступен" || mac == "не найден" || mac == "ошибка")
                return "—";
            try
            {
                string url = $"https://api.macvendors.com/{mac}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();

                return "неизвестен";
            }
            catch
            {
                return "ошибка API";
            }
        }

    }
}
