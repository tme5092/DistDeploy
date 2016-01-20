using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DeployListener.Svc
{
    public partial class DeploySvc : ServiceBase
    {
        private ILog logger;

        protected ILog Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private void InitializeLogger()
        {
            XmlConfigurator.Configure();
            logger = LogManager.GetLogger(this.GetType());
        }

        public DeploySvc()
        {

            try
            {
                this.ServiceName = "DeployListener";

                InitializeLogger();
                InitializeComponent();

                dp = new DeployWorker();
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
                throw e;
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    dp.StartListener();
                });
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
            }
        }

        private DeployWorker dp;

        protected override void OnStop()
        {
        }
    }
}
