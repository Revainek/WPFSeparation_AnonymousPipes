using IpcAnonymousPipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_IPCSeparationFrame.IPCSeparation_Host.Models
{
    public class PoolObject
    {
        public PoolObject(int handle, bool readystatus, PipeServer instance)
        {
            ClientOutputHandle = handle;
            isReady = readystatus;
            PipeInstance = instance;
        }
        public int ClientOutputHandle { get; set; }
        public bool isReady { get; set; }
        public PipeServer PipeInstance { get; set; }
    }
}
