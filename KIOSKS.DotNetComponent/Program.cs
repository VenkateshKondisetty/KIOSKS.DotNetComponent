using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KioskDotNetService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if (!DEBUG)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new KioskDotNetService()
            };
            ServiceBase.Run(ServicesToRun);
#else
            KioskDotNetService OService = new KioskDotNetService();
            OService.OnStartMethod();
            //myServ.Process();
            //// here Process is my Service function
            //// that will run when my service onstart is call
            //// you need to call your own method or function name here instead of Process();
#endif
        }
    }
}
