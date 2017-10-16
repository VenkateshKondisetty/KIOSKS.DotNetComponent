using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KioskDotNetService
{
    class KSocketConnection
    {
        public Thread m_oThread;
        public KConnection m_oHandler;
    }
    class KListener
    {
        private bool m_bRun;
        private TcpListener m_oListener;
        private List<KSocketConnection> m_oaConnectionList;

        public KListener()
        {
            try
            {
                m_oaConnectionList = new List<KSocketConnection>();
                m_bRun = true;
                m_oListener = new TcpListener(IPAddress.Parse(KioskDotNetConfig.Default.KIOSK_SERVICE_IP),KioskDotNetConfig.Default.LISTEN_PORT);
                // Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //IPEndPoint oLocalEndPoint = new IPEndPoint(IPAddress.Any, config.Default.LISTEN_PORT);
                LogFile.writeLog(LOG_OPTIONS.DEBUG, "Listen server getting start on port : " + Convert.ToString(KioskDotNetConfig.Default.LISTEN_PORT));
                //m_oListener.Bind(oLocalEndPoint);
            }
            catch (Exception oEx)
            {
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: KListnerConstructor()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
                m_bRun = false;
            }
        }

        public KListener(int iPort)
        {
            try
            {
                m_oaConnectionList = new List<KSocketConnection>();
                m_bRun = true;
                m_oListener =new TcpListener(IPAddress.Parse(KioskDotNetConfig.Default.KIOSK_SERVICE_IP), KioskDotNetConfig.Default.LISTEN_PORT);
                //IPEndPoint oLocalEndPoint = new IPEndPoint(IPAddress.Any, iPort);
                LogFile.writeLog(LOG_OPTIONS.DEBUG, "Listen server getting start on port : " + Convert.ToString(iPort));
                //m_oListener.Bind(oLocalEndPoint);
            }
            catch (Exception oEx)
            {
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: KListnerConstructor()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
                m_bRun = false;
            }
        }

        public void run()
        {
            try
            {
                m_oListener.Start();
                //.Listen(config.Default.CONNECTION_QUEUE_SIZE);
                //CDevices.Initialize();
                //CMClients.Initialize();
                while (m_bRun)
                {
                    LogFile.writeLog(LOG_OPTIONS.INFO, "Waiting for client connection...");
                    TcpClient oClientSocket = m_oListener.AcceptTcpClient();
                    //Socket oSocket = m_oListener.Accept();
                    LogFile.writeLog(LOG_OPTIONS.INFO, "Connection receieved, and Processing started...");
                    KSocketConnection oConnection = new KSocketConnection();
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Handler object creating...");
                    KConnection oClientConnection = new KConnection(oClientSocket);
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Handler object created");
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Creating handling thread...");
                    Thread oClientThread = new Thread(new ThreadStart(oClientConnection.run));
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Handling thread created");
                    oConnection.m_oHandler = oClientConnection;
                    oConnection.m_oThread = oClientThread;
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Connection storing in list...");
                    m_oaConnectionList.Add(oConnection);
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Connection stored.");
                    oClientThread.Start();
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Handling thread started.");
                    clearConnections();
                }
            }
            catch (Exception oEx)
            {
                LogFile.writeLog(LOG_OPTIONS.ERROR, "initialization failed");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
            }
        }

        public void stop()
        {
            try
            {
                LogFile.writeLog(LOG_OPTIONS.INFO, "Stoping main listening thread.");
                m_bRun = false;
                TcpClient oTemp = new TcpClient();
                oTemp.Connect(KioskDotNetConfig.Default.KIOSK_SERVICE_IP, KioskDotNetConfig.Default.LISTEN_PORT);
                oTemp.Close();
                //Socket oSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //oSocket.Connect("127.0.0.1", KioskDotNetConfig.Default.LISTEN_PORT);
                //oSocket.Close();
                LogFile.writeLog(LOG_OPTIONS.DEBUG, "Executing closeConnection() method to close all available connections...");
                LogFile.writeLog(LOG_OPTIONS.INFO, "Closing existing connections...");
                closeConnections();
                LogFile.writeLog(LOG_OPTIONS.INFO, "Existing connections closed.");
                m_oListener.Stop();
                LogFile.writeLog(LOG_OPTIONS.INFO, "Main listening thread closed.");
            }
            catch (Exception oEx)
            {
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: Stop()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
            }
        }

        private void clearConnections()
        {
            LogFile.writeLog(LOG_OPTIONS.DEBUG, "Cleaning idle threads...");
            int i = 0;
            LogFile.writeLog(LOG_OPTIONS.INFO, "Existing Connections : " + m_oaConnectionList.Count.ToString());
            while (i < m_oaConnectionList.Count)
            {
                if (!m_oaConnectionList[i].m_oThread.IsAlive)
                {
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Idle thread found...");
                    m_oaConnectionList.RemoveAt(i);
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Removed idle thread");
                    LogFile.writeLog(LOG_OPTIONS.INFO, "Existing Connections : " + m_oaConnectionList.Count.ToString());
                    if (i == 0)
                    {
                        i = 0;
                    }
                    else
                    {
                        i--;
                    }
                }
                else
                {
                    i++;
                }
            }
        }

        private void closeConnections()
        {
            int i = 0;
            LogFile.writeLog(LOG_OPTIONS.DEBUG, "Closing available connections...");
            while (i < m_oaConnectionList.Count)
            {
                if (m_oaConnectionList[i].m_oThread.IsAlive)
                {
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Alive thread found and closing it..");
                    m_oaConnectionList[i].m_oHandler.closeConnection();
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Alive thread closed. Removing from list...");
                    m_oaConnectionList.RemoveAt(i);
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Removed from list.");
                    if (i == 0)
                    {
                        i = 0;
                    }
                    else
                    {
                        i--;
                    }
                }
                else
                {
                    i++;
                }
            }
            LogFile.writeLog(LOG_OPTIONS.INFO, "Existing Connections : " + m_oaConnectionList.Count.ToString());
        }
    }
}
