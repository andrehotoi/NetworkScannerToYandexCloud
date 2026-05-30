using NetworkScanner;
using System.Threading.Tasks;

Console.WriteLine("╔═══════════════════════════════╗");
Console.WriteLine("║       СЕТЕВОЙ СКАНЕР          ║");
Console.WriteLine("╚═══════════════════════════════╝");
Console.WriteLine();


string baseIp = ReadString(
    "Введите первые три байта сети (пример: 192.168.0): ",
    @"^\d{1,3}\.\d{1,3}\.\d{1,3}$"
);


Console.WriteLine();
int lowRange = ReadInt("Нижняя граница диапазона (1-254): ", 1, 254);
int highRange = ReadInt("Верхняя граница диапазона (1-254): ", lowRange, 254);

Console.WriteLine();
Console.WriteLine("Режим сканирования портов:");
Console.WriteLine("  1. Быстрое    (10 портов)");
Console.WriteLine("  2. Стандартное (30 портов)");
int modeChoice = ReadInt("Выбор: ", 1, 2);
ScanMode mode = modeChoice == 2 ? ScanMode.Standard : ScanMode.Quick;


Console.WriteLine();
Console.WriteLine("═══════════════════════════════");
DateTime startTime = DateTime.Now;
Console.WriteLine($"Запуск: {startTime:HH:mm:ss}");
Console.WriteLine();

var scanner = new scanner();
var devices = await scanner.ScanRangeAsync(baseIp, lowRange, highRange, mode);

DateTime endTime = DateTime.Now;
TimeSpan execTime = endTime - startTime; 

Console.WriteLine();
Console.WriteLine("═══════════════════════════════");
Console.WriteLine($"Найдено устройств : {devices.Count}");
Console.WriteLine($"Время окончания   : {endTime:HH:mm:ss}");
Console.WriteLine($"Время выполнения  : {execTime.TotalSeconds:F1} сек");
Console.WriteLine("═══════════════════════════════");
Console.WriteLine();


var cloud = new CloudService();
await cloud.SaveReportAsync(devices.ToList());

while (true)
{
    Console.WriteLine("\n═══════════════════════════════");
    Console.WriteLine("Варианты действий:");
    Console.WriteLine("1. Просмотреть отчёт из облака");
    Console.WriteLine("2. Сравнить с отчётом из облака");
    Console.WriteLine("0. Выход");
    int menuChoice = ReadInt("Выбор: ", 0, 2);

    if (menuChoice == 0) break;

    var reports = await cloud.GetReportsListAsync();
    Console.WriteLine("\nДоступные отчёты:");
    for (int i = 0; i < reports.Count; i++)
        Console.WriteLine($"{i + 1}. {reports[i]}");

    if (menuChoice == 1)
    {
        int choice = ReadInt("\nВыберите номер отчёта: ", 1, reports.Count);
        var report = await cloud.DownloadReportAsync(reports[choice - 1]);

        Console.WriteLine($"\n═══ Отчёт: {reports[choice - 1]} ═══");
        foreach (var d in report)
        {
            Console.WriteLine($"IP:            {d.IpAddress}");
            Console.WriteLine($"Hostname:      {d.HostName}");
            Console.WriteLine($"ОС:            {d.OsGuess}");
            Console.WriteLine($"Ping:          {d.PingTime}мс");
            Console.WriteLine($"MAC:           {d.MacAddress}");
            Console.WriteLine($"Производитель: {d.Manufacturer}");
            Console.WriteLine($"Порты:         {string.Join(", ", d.OpenPorts)}");
            Console.WriteLine("─────────────────────────────");
        }
    }
    else if (menuChoice == 2)
    {
        int choice = ReadInt("\nВыберите номер отчёта для сравнения: ", 1, reports.Count);
        var oldReport = await cloud.DownloadReportAsync(reports[choice - 1]);

        var compareResults = cloud.CompareReports(oldReport, devices.ToList());
        PrintCompareResults(compareResults);
    }
}

Console.WriteLine("Нажмите любую клавишу...");
Console.ReadKey();


static int ReadInt(string prompt, int min, int max)
{
    while (true)
    {
        Console.Write(prompt);
        string input = Console.ReadLine();

        if (int.TryParse(input, out int value) && value >= min && value <= max)
            return value;

        Console.WriteLine($"Ошибка: введите число от {min} до {max}");
    }
}

static string ReadString(string prompt, string pattern)
{
    while (true)
    {
        Console.Write(prompt);
        string input = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(input) && System.Text.RegularExpressions.Regex.IsMatch(input, pattern))
            return input;

        Console.WriteLine("Ошибка: неверный формат. Пример: 192.168.0");
    }
}

static void PrintCompareResults(List<CompareResult> results)
{
    Console.WriteLine("\n═══════════════════════════════");
    Console.WriteLine("       РЕЗУЛЬТАТ СРАВНЕНИЯ");
    Console.WriteLine("═══════════════════════════════\n");

    foreach (var r in results)
    {
        switch (r.Status)
        {
            case CompareStatus.Unchanged:
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"{r.IpAddress}  [без изменений]");
                Console.WriteLine($"  Hostname:      {r.NewDevice.HostName}");
                Console.WriteLine($"  ОС:            {r.NewDevice.OsGuess}");
                Console.WriteLine($"  Ping:          {r.NewDevice.PingTime}мс");
                Console.WriteLine($"  MAC:           {r.NewDevice.MacAddress}");
                Console.WriteLine($"  Производитель: {r.NewDevice.Manufacturer}");
                Console.WriteLine($"  Порты:         {string.Join(", ", r.NewDevice.OpenPorts)}");
                break;

            case CompareStatus.New:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{r.IpAddress}  [НОВОЕ]");
                Console.WriteLine($"  Hostname:      {r.NewDevice.HostName}");
                Console.WriteLine($"  ОС:            {r.NewDevice.OsGuess}");
                Console.WriteLine($"  Ping:          {r.NewDevice.PingTime}мс");
                Console.WriteLine($"  MAC:           {r.NewDevice.MacAddress}");
                Console.WriteLine($"  Производитель: {r.NewDevice.Manufacturer}");
                Console.WriteLine($"  Порты:         {string.Join(", ", r.NewDevice.OpenPorts)}");
                break;

            case CompareStatus.Lost:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{r.IpAddress}  [ПРОПАЛО]");
                Console.WriteLine($"  Hostname:      {r.OldDevice.HostName}");
                Console.WriteLine($"  ОС:            {r.OldDevice.OsGuess}");
                Console.WriteLine($"  Ping:          {r.OldDevice.PingTime}мс");
                Console.WriteLine($"  MAC:           {r.OldDevice.MacAddress}");
                Console.WriteLine($"  Производитель: {r.OldDevice.Manufacturer}");
                Console.WriteLine($"  Порты:         {string.Join(", ", r.OldDevice.OpenPorts)}");
                break;

            case CompareStatus.Changed:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{r.IpAddress}  [ИЗМЕНИЛОСЬ]");
                Console.WriteLine($"  Hostname:      {r.NewDevice.HostName}");
                Console.WriteLine($"  ОС:            {r.NewDevice.OsGuess}");
                Console.WriteLine($"  Ping:          {r.NewDevice.PingTime}мс");
                Console.WriteLine($"  MAC:           {r.NewDevice.MacAddress}");
                Console.WriteLine($"  Производитель: {r.NewDevice.Manufacturer}");
                Console.WriteLine($"  Порты:         {string.Join(", ", r.NewDevice.OpenPorts)}");
                Console.WriteLine($"  Изменения:");
                foreach (var change in r.Changes)
                    Console.WriteLine($"    ~ {change}");
                break;
        }

        Console.ResetColor();
        Console.WriteLine();
    }

    Console.WriteLine("═══════════════════════════════");
    Console.WriteLine($"Новых устройств:      {results.Count(r => r.Status == CompareStatus.New)}");
    Console.WriteLine($"Пропавших:            {results.Count(r => r.Status == CompareStatus.Lost)}");
    Console.WriteLine($"Изменившихся:         {results.Count(r => r.Status == CompareStatus.Changed)}");
    Console.WriteLine($"Без изменений:        {results.Count(r => r.Status == CompareStatus.Unchanged)}");
    Console.WriteLine("═══════════════════════════════");
}