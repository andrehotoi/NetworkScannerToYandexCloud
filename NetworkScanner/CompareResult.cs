using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkScanner
{
    public enum CompareStatus
    {
        Unchanged,  
        New,        
        Lost,       
        Changed     
    }
    internal class CompareResult
    {
        public string IpAddress { get; set; }
        public CompareStatus Status { get; set; }
        public DeviceInfo OldDevice { get; set; }   
        public DeviceInfo NewDevice { get; set; }   
        public List<string> Changes { get; set; }   

        public CompareResult()
        {
            Changes = new List<string>();
        }
    }
}
