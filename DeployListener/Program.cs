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

    class Program : Base
    {

        public Program() : base()
        {
        }

        static void Main(string[] args)
        {
            DeployWorker p = new DeployWorker();

            if (args.Length > 0)
            {
                p.Logger.Debug("Acting as Server");
                p.StartListener();
            }
            else
            {
                p.Logger.Debug("Acting as Client");
                p.StartClient();
            }
        }


    }
}
