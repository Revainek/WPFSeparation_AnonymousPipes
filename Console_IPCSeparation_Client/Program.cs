using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Windows;
using Newtonsoft.Json;
using System.Threading;
using System;
using System.Windows.Interop;
using System.IO.Pipes;
using System.IO;
using IpcAnonymousPipes;
using System.Text;
using System.Linq;

using System.Collections.Generic;




internal class Program
{


    private static void Main(string[] args)
    {

        // Hosting process might set some initial arguments for startup of separated process/module via Process Start Arguments
        // Also first arguments contains Handles to Host Process for establishing the Anonymous pipe.
        string ExampleInitializationArgument1 = "";
        string ExampleInitializationArgument2 = "";
        string InHandle = "";
        string OutHandle = "";
        string prefixIn = "--InPipeHandle=";
        string prefixOut = "--OutPipeHandle=";
        if (args.Length > 5)
        {

            InHandle = Environment.GetCommandLineArgs().First((string x) => x.StartsWith(prefixIn, StringComparison.OrdinalIgnoreCase)).Remove(0, prefixIn.Length);
            OutHandle = Environment.GetCommandLineArgs().First((string x) => x.StartsWith(prefixOut, StringComparison.OrdinalIgnoreCase)).Remove(0, prefixOut.Length);
            if (args[2] == "/SharedLocation")
            {

                ExampleInitializationArgument1 = args[3];
            }
            if (args[4] == "/DataSource")
            {

                ExampleInitializationArgument2 = args[5];
            }
        }

        /// Pipe client instance is created from WPF_IPCSeparationFrame and subscribed 
        PipeClient Client;

        string receivedMessage = "";
        Client = new PipeClient(InHandle, OutHandle);
        Client.Disconnected += PipeClient_Disconnected;
        Client.ReceiveAsync(ReceiveAction);
        int timeout = 100;

        //Startup of Separated process instance.
        SomeLongRunningProcessInstance SeparatedProcess = new SomeLongRunningProcessInstance();



        while (SeparatedProcess.IsReady == false && timeout > 0)
        {
            Thread.Sleep(3000);
            timeout--;
        }
        string? ProcessResult = null;
        if (SeparatedProcess.IsReady == true)
        {
            // Process is sustained and awaiting requests.
            while (SeparatedProcess.IsReady == true)
            {

                while (string.IsNullOrEmpty(receivedMessage))
                {
                    // while there is no received (async) message, the process will await - unless it's disconnected.
                    Thread.Sleep(100);
                }


                // when the message is received, further steps occur and long running operation is being processed

                var result = SeparatedProcess.SomeLongRunningProcess(receivedMessage);

                // result is being serialized into Json object

                ProcessResult = JsonConvert.SerializeObject(result, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                //send action
                SendResponse();


                // clearing data and awaiting further tasks
                result = null;
                receivedMessage = "";
            }
        }
        else
        {
            string message = "SeparatedProcess failed to start in reasonable amount of time ";
        }

        // closing on disconnect
        static void PipeClient_Disconnected(object? sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        // receive action
        void ReceiveAction(PipeMessageStream stream)
        {
            string text = Encoding.UTF8.GetString(stream.ReadToEnd());
            receivedMessage = text;
        }
        // response send action
        void SendResponse()
        {
            Client.Send(Encoding.UTF8.GetBytes(ProcessResult));
        }




    }
    public class SomeLongRunningProcessInstance
    {
        /// <summary>
        /// Since it's an empty processing unit, it's immediately read.
        /// </summary>
        public bool IsReady { get; set; } = true;

        public string SomeLongRunningProcess(string inputData)
        {
            Thread.Sleep(3000);
            return "Operation result " + Environment.ProcessId;
        }
    }
}
