using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkScanner
{
    internal class DeviceInfo
    {
        public string IpAddress { get; set; }      
        public string HostName { get; set; }        
        public bool IsAlive { get; set; }           
        public long PingTime { get; set; }          
        public List<int> OpenPorts { get; set; }    
        public string OsGuess { get; set; }        
        public string MacAddress { get; set; }      
        public string Manufacturer { get; set; }
        public DeviceInfo()
        {
            OpenPorts = new List<int>();
            OsGuess = "ОС неизвестна";
            MacAddress = "MAC-адрес недоступен";
            Manufacturer = "Производитель не определён";
        }
    }
}
