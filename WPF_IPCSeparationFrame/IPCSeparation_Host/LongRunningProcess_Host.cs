using IpcAnonymousPipes;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_IPCSeparationFrame.IPCSeparation_Host.Models;

namespace WPF_IPCSeparationFrame.IPCSeparation_Host
{
    /// <summary>
    /// 
    /// </summary>
    internal class LongRunningProcess_Host
    {
        /// <summary>
        /// Predefined amount of Clients to be created at Host initialization
        /// </summary>
        private const int CountClients = 10;
        /// <summary>
        /// Relative or absolute path to client executable 
        /// </summary>
        //private const string ClientPath = @"C:\...\Console_IPCSeparation_Client.exe";
        private const string ClientPath = @"C:\Temp_Work\PERICAD\Others\Console_IPCSeparation_Client\bin\Debug\net6.0-windows\Console_IPCSeparation_Client.exe";

        /// <summary>
        /// Blocking collection of Separated Process clients to perform long running action from queue
        /// </summary>
        BlockingCollection<PoolObject> _pools { get; set; } = new BlockingCollection<PoolObject>();
        /// <summary>
        /// A queue of tasks that will be distributed to PoolObjects 
        /// </summary>
        ConcurrentQueue<RequestBody> requests { get; set; } = new ConcurrentQueue<RequestBody>();
        /// <summary>
        /// A Drop location for responses coming from Pool Objects
        /// </summary>
        ConcurrentDictionary<int, ResponseBody> _ResponseBodyPool { get; set; } = new ConcurrentDictionary<int, ResponseBody>();

        /// <summary>
        /// List with output, taking data from ResponseBodyPool Dictionary
        /// </summary>
        List<ResponseBody> _OutputResponses { get; set; } = new List<ResponseBody>();

        /// <summary>
        /// Constructor takes initial data to start a fixed amount of client processes and initialize a long lasting local connection that can accept request queues
        /// </summary>
        public LongRunningProcess_Host()
        {
            for (int i = 0; i < CountClients; i++)
            {
                PipeServer Server = new PipeServer();
                // Create pipe server
                Server = new PipeServer();
                Server.Connected += PipeServer_Connected;
                Server.Disconnected += PipeServer_Disconnected;
                string SharedLocation = @"C:\SomeRandomPath\";
                string DataSource = "SomeExampleString";



                // Start client process with command line arguments
                ProcessStartInfo info = new ProcessStartInfo();
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.FileName = ClientPath;
                info.Arguments = Server.GetClientArgs() + " /SharedLocation " + SharedLocation + " /DataSource " + DataSource;

                
                info.UseShellExecute = false;
                Process.Start(info);

                // Receiving on background thread
                Server.ReceiveAsync(stream =>
                {
                    string text = Encoding.UTF8.GetString(stream.ReadToEnd());
                    var OutputData = JsonConvert.DeserializeObject<string>(text);
                    _ResponseBodyPool.TryAdd(Server.ClientOutputHandle.GetHashCode(), new ResponseBody() { ResponseData = OutputData });
                });

                _pools.Add(new PoolObject(Server.ClientOutputHandle.GetHashCode(), true, Server));
            }
        }
        /// <summary>
        /// Simplified implementation of Sending List of Input data, to be distributed and processed among Clients
        /// </summary>
        /// <param name="InputDataList"> List of input data for processing</param>
        /// <returns></returns>
        public List<ResponseBody> EnqueueTestListForShuttering(List<string> InputDataList)
        {
            _OutputResponses.Clear();
            _ResponseBodyPool.Clear();
            // Enqueue requests
            foreach (var data in InputDataList)
            {
                requests.Enqueue(new RequestBody(data));
            }
            //Defined action to dequeue requests
            Action action = () =>
            {
                try
                {
                    PoolObject AvailableClient;
                    while (!requests.IsEmpty)
                    {
                        while (_pools.TryTake(out AvailableClient))
                        {
                            RequestBody request;
                            while (requests.TryDequeue(out request))
                            {
                                AvailableClient.PipeInstance.Send(request.bytes);
                                while (_ResponseBodyPool.ContainsKey(AvailableClient.ClientOutputHandle) == false)
                                {
                                    Thread.Sleep(1);
                                }
                                if (_ResponseBodyPool.ContainsKey(AvailableClient.ClientOutputHandle) && request != null)
                                {
                                    ResponseBody entry;
                                    _ResponseBodyPool.TryRemove(AvailableClient.ClientOutputHandle, out entry);
                                    // Providing relation between input Data and output Data
                                    entry.InputDataFromRequest = request.requestData;
                                    _OutputResponses.Add(entry);

                                }
                            }
                            //Readding Client back to pool of idling clients after there are no more requests
                            var respawnneeded = _pools.TryAdd(AvailableClient);
                            break;
                        }
                    }
                }
                catch { }
            };
            //Assigning number of actions equal to number of Client instanes created
            var actArray = new Action[_pools.Count];
            for (int i = 0; i < _pools.Count; i++)
            {
                actArray[i] = action;
            }
            Parallel.Invoke(actArray);

            return _OutputResponses;
        }
        #region Pipe setup
        private void PipeServer_Connected(object sender, EventArgs e)
        {
        }

        private void PipeServer_Disconnected(object sender, EventArgs e)
        {
            var server = sender as PipeServer;
            if (server != null)
            {
                server.Dispose();
            }
        }
        #endregion
    }
}
