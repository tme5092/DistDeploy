using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DeployListener
{
    internal delegate void ProcessShellScript(string filename);

    internal class DeployWorker : Base
    {
        public DeployWorker() : base()
        {
        }

        private AppSettingsReader Reader = new AppSettingsReader();
        private TcpListener tcpServer;

        public void StartClient()
        {
            try
            {
                // Tell the server to start deployment
                ConnectClient();

                // Do the deployment
                ProcessPowerShellScript(GetSetting<string>("PRE_File"));

                // Finish deployment
                ConnectClient();
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
            }
        }

        private void ConnectClient()
        {
            try
            {
                Logger.Debug("Connect client");
                IPEndPoint tcpEndpoint = new IPEndPoint(IP2Long(GetSetting<string>("IP")), GetSetting<int>("tcpPort"));
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(tcpEndpoint);


                NetworkStream networkStream = tcpClient.GetStream();
                StreamReader clientStreamReader = new StreamReader(networkStream);
                StreamWriter clientStreamWriter = new StreamWriter(networkStream);
                clientStreamWriter.Write("1");
                clientStreamWriter.Flush();

                tcpClient.Close();
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
                throw e;
            }

        }

        public void StartListener()
        {

            try
            {
                Host();

            }
            catch (Exception e)
            {
                Logger.Fatal(e);
                throw e;
            }

            while (true)
            {
                try
                {
                    Process(ProcessPowerShellScript, GetSetting<string>("PRE_File"));

                    // .. Wait to be copied

                    Process(ProcessPowerShellScript, GetSetting<string>("POST_File"));

                }
                catch (Exception e)
                {
                    Logger.Fatal(e);
                }
            }
        }

        void Process(ProcessShellScript script, string filename)
        {
            Byte[] bytes = new Byte[256];
            String data = null;

            TcpClient client = tcpServer.AcceptTcpClient();
            NetworkStream stream = client.GetStream();

            data = System.Text.Encoding.ASCII.GetString(bytes, 0, 0);
            stream.Write(new byte[] { (byte)'0' }, 0, 1);

            // Execute Script
            script(filename);

            client.Close();
        }

        private void ProcessPowerShellScript(string filename)
        {
            Logger.DebugFormat("Try to execute {0}", filename);

            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();

            Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            runspace.Open();

            RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);
            scriptInvoker.Invoke("Set-ExecutionPolicy Unrestricted");

            Pipeline pipeline = runspace.CreatePipeline();

            //Here's how you add a new script with arguments
            Command myCommand = new Command(filename);
            //CommandParameter testParam = new CommandParameter("key", "value");
            //myCommand.Parameters.Add(testParam);

            pipeline.Commands.Add(myCommand);

            // Execute PowerShell script
            var results = pipeline.Invoke();

        }


        private void Host()
        {
            Logger.Debug("Host service");
            IPEndPoint tcpEndpoint = new IPEndPoint(IP2Long(GetSetting<string>("IP")), GetSetting<int>("tcpPort"));
            tcpServer = new TcpListener(tcpEndpoint);

            tcpServer.Start();
        }

        #region Helper

        T GetSetting<T>(string key)
        {
            try
            {
                switch (typeof(T).Name)
                {
                    default:
                        return (T)Reader.GetValue(key, typeof(T));
                }

            }
            catch (Exception e)
            {
                Logger.Fatal(e);
                throw e;
            }

        }

        static long IP2Long(string ip)
        {
            string[] ipBytes;
            long num = 0;
            if (!string.IsNullOrEmpty(ip))
            {
                ipBytes = ip.Split('.');
                for (int i = ipBytes.Length - 1; i >= 0; i--)
                {
                    num |= (long.Parse(ipBytes[i]) << (i * 8));
                }
            }
            return (long)num;
        }

        #endregion Helper


    }
}
