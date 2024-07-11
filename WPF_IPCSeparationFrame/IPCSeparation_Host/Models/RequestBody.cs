using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_IPCSeparationFrame.IPCSeparation_Host.Models
{
    public class RequestBody
    {
        public RequestBody(string Data)
        {
            requestData = Data;
            bytes = Encoding.UTF8.GetBytes(Data);
        }
        public string requestData { get; set; }
        public byte[] bytes { get; set; }
    }
}
