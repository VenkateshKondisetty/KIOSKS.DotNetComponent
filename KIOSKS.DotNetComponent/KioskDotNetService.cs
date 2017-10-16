using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KioskDotNetService
{
    public partial class KioskDotNetService : ServiceBase
    {
        KServer m_oServer;
        public KioskDotNetService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            OnStartMethod();
        }

        protected override void OnStop()
        {
            OnStopMethod();
        }
        public void OnStartMethod()
        {
            m_oServer = new KServer(KioskDotNetConfig.Default.LISTEN_PORT);
            m_oServer.start();
        }
        public void OnStopMethod()
        {
            m_oServer.stop();
        }

    }
}
