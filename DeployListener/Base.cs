using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DeployListener
{
    class Base
    {
        private ILog logger;

        internal ILog Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        public Base()
        {
            XmlConfigurator.Configure();
            logger = LogManager.GetLogger(this.GetType());
        }
    }
}
