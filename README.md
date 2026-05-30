# NetworkScannerToYandexCloud
C# / .NET 6, без сторонних зависимостей. Встроенные библиотеки: System.Net — Ping, DNS, TCP; System.Text.Json — сериализация. Внешние API: macvendors.com, Яндекс.Диск REST API.

Сканирует диапазон IP через ICMP Ping, определяет ОС по TTL, hostname через DNS, MAC через ARP, производителя через REST API. Отчёты в JSON сохраняются на Яндекс.Диск, доступно сравнение отчётов.

  # Возможности:
- Сканирование диапазона IP-адресов через Ping
- Определение ОС устройства по TTL
- Получение hostname через DNS
- Получение MAC-адреса через ARP-таблицу
- Определение производителя устройства по MAC через [macvendors.com] (https://macvendors.com)
- Проверка открытых портов (10 или 30 портов на выбор)
- Проверка открытых портов (10 или 30 портов на выбор)
- Сохранение отчётов в Яндекс.Диск
- Просмотр истории отчётов из облака
- Сравнение двух отчётов с выводом изменений

  # Стек:
- C# / .NET 6
- `System.Net.NetworkInformation` — Ping
- `System.Net.Sockets` — TCP, проверка портов
- `System.Net.Dns` — DNS резолвинг
- `System.Net.Http` — HTTP запросы к API
- `System.Text.Json` — сериализация отчётов
- Яндекс.Диск REST API — облачное хранилище

  # Запуск:
1. Клонируйте репозиторий
2. Откройте в Visual Studio 2022
3. Получите токен Яндекс.Диска на [oauth.yandex.ru](https://oauth.yandex.ru) и вставьте в `CloudService.cs`
4. Запустите проект (`F5`)

  # Использование:
- Введите первые три байта сети (пример: 192.168.0)
- Введите диапазон (например 1-254)
- Выберите режим сканирования портов: 1 (10 портов) / 2 (30 портов)
После сканирования:
1 — просмотреть отчёт из облака
2 — сравнить с отчётом из облака
0 — выход

  # Структура проекта:
NetworkScanner/
-Program.cs          — меню, точка входа
-Scanner.cs          — логика сканирования
-CloudService.cs     — работа с Яндекс.Диском
-DeviceInfo.cs       — модель устройства
-CompareResult.cs    — модель результата сравнения

# EN:

C# / .NET 6, no third-party dependencies. Built-in libraries: System.Net — Ping, DNS, TCP; System.Text.Json — serialization. External APIs: macvendors.com, Yandex.Disk REST API.
Scans IP range via ICMP Ping, detects OS by TTL, hostname via DNS, MAC via ARP, manufacturer via REST API. JSON reports are saved to Yandex.Disk with historical comparison support.

# Features:
IP range scanning via Ping
OS detection by TTL
Hostname resolution via DNS
MAC address retrieval via ARP table
Device manufacturer detection by MAC via macvendors.com
Open port scanning (10 or 30 ports selectable)
Report saving to Yandex.Disk
Cloud report history viewer
Two-report comparison with change output

# Stack:
C# / .NET 6
System.Net.NetworkInformation — Ping
System.Net.Sockets — TCP, port scanning
System.Net.Dns — DNS resolution
System.Net.Http — HTTP requests to APIs
System.Text.Json — report serialization
Yandex.Disk REST API — cloud storage

# Getting started:
Clone the repository
Open in Visual Studio 2022
Get a Yandex.Disk token at oauth.yandex.ru and paste it into CloudService.cs
Run the project (F5)

# Usage:
Enter the first three bytes of the network (example: 192.168.0)
Enter the range (example: 1-254)
Select port scan mode: 1 (10 ports) / 2 (30 ports)

After scanning:
1 — view a report from the cloud
2 — compare with a report from the cloud
0 — exit

# Project structure:
NetworkScanner/

Program.cs — menu, entry point
Scanner.cs — scanning logic
CloudService.cs — Yandex.Disk integration
DeviceInfo.cs — device model
CompareResult.cs — comparison result model
