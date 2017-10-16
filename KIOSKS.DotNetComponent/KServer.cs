using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KioskDotNetService
{
    class KServer
    {
        Thread m_oThread;
        KListener m_oListener;

        public KServer(int iPort)
        {
            m_oListener = new KListener(iPort);
            m_oThread = new Thread(new ThreadStart(m_oListener.run));
        }

        public void stop()
        {
            m_oListener.stop();
        }

        public void start()
        {
            if ((m_oThread.ThreadState == ThreadState.Stopped) || (m_oThread.ThreadState == ThreadState.Unstarted))
            {
                m_oThread.Start();
            }
        }
    }
}
