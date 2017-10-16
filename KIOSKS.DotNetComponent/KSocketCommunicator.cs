using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KioskDotNetService
{
    class KSocketCommunicator
    {
        private int m_iBufferSize;
        private int m_iReadTimeout;
        private Socket m_oSocket;

        public KSocketCommunicator()
        {
            LogFile.writeLog(LOG_OPTIONS.DEBUG, "KSocketCommunicator Object creating...");
            m_iReadTimeout = KioskDotNetConfig.Default.SOCKET_READ_TIMEOUT * 1000;
            m_iBufferSize = KioskDotNetConfig.Default.BUFFER_SIZE;
            m_oSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            LogFile.writeLog(LOG_OPTIONS.DEBUG, "KSocketCommunicator Object created.");
        }

        private bool connect(string strIP, int iPort, int iReadTimeout)
        {
            bool bReturn = true;
            try
            {
                LogFile.writeLog(LOG_OPTIONS.DEBUG, "Connecting to " + strIP + ":" + iPort.ToString());
                m_iReadTimeout = iReadTimeout * 1000;
                m_oSocket.ReceiveTimeout = m_iReadTimeout;
                m_oSocket.Connect(strIP, iPort);
                if (!m_oSocket.Connected)
                {
                    bReturn = false;
                    LogFile.writeLog(LOG_OPTIONS.ERROR, "Connection failed to remote host - " + strIP + ":" + Convert.ToString(iPort));
                }
                else
                {
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Connected to " + strIP + ":" + iPort.ToString());
                }
            }
            catch (Exception oEx)
            {
                bReturn = false;
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Connection failed to remote host - " + strIP + ":" + Convert.ToString(iPort) + ". Exception error : " + oEx.Message);
            }
            return bReturn;
        }

        public bool connect(string strIP, int iPort)
        {
            bool bReturn = true;
            try
            {
                LogFile.writeLog(LOG_OPTIONS.DEBUG, "Connecting to " + strIP + ":" + iPort.ToString());
                m_oSocket.ReceiveTimeout = m_iReadTimeout;
                m_oSocket.Connect(strIP, iPort);
                bReturn = true;
            }
            catch (Exception oEx)
            {
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Connection failed to remote host - " + strIP + ":" + Convert.ToString(iPort) + ". Exception error : " + oEx.Message);
                bReturn = false;
            }
            return bReturn;
        }

        public bool send(string strMessage)
        {
            byte[] abBuffer = ASCIIEncoding.ASCII.GetBytes(strMessage);
            bool bReturn = true;
            try
            {
                LogFile.writeLog(LOG_OPTIONS.DEBUG, "Message to be sent : " + strMessage);
                send(abBuffer);
            }
            catch (Exception oEx)
            {
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in KSocketConnection::send(ref Socket oSocket, string strRequest, ref string strReceive) - " + oEx.Message);
                bReturn = false;
            }
            return bReturn;
        }

        public bool send(string strMessage, ref string strReceived)
        {
            byte[] abBuffer = ASCIIEncoding.ASCII.GetBytes(strMessage);
            bool bReturn = true;
            try
            {
                if (send(abBuffer))
                {
                    //if (readForXml(ref strReceived))
                    //{
                    //    return true;
                    //}
                    //else
                    //{
                    //    return false;
                    //}
                }
                else
                {
                    return false;
                }
            }
            catch (Exception oEx)
            {
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in KSocketConnection::send(ref Socket oSocket, string strRequest, ref string strReceive) - " + oEx.Message);
                bReturn = false;
            }
            return bReturn;
        }

        private bool send(byte[] abBuffer)
        {
            bool bReturn = true;
            int iSentTotal = 0;
            int iSent = 0;
            try
            {
                if (m_oSocket.Connected)
                {
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Sending message...");

                    while (iSentTotal < abBuffer.Length)
                    {
                        iSent = 0;
                        iSent = m_oSocket.Send(abBuffer, iSentTotal, (abBuffer.Length - iSentTotal), SocketFlags.None);
                        iSentTotal += iSent;
                    }
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Message sent.");
                }
                else
                {
                    LogFile.writeLog(LOG_OPTIONS.ERROR, "Error - Unable to send due to no socket connection.");
                    bReturn = false;
                }
            }
            catch (Exception oEx)
            {
                bReturn = false;
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in send(byte[] abBuffer) - " + oEx.Message);
            }
            return bReturn;
        }

        public bool readForResponses(ref string strReceived)
        {
            int iRead = 0;
            string strRcv = "";
            bool bReturn = false;
            byte[] abBuffer = new byte[KioskDotNetConfig.Default.BUFFER_SIZE];
            try
            {
                LogFile.writeLog(LOG_OPTIONS.DEBUG, "Reading message...");
                while (true)
                {
                    int iResponseLength = 0;
                    if ((iRead = m_oSocket.Receive(abBuffer,abBuffer.Length,SocketFlags.None)) > 0)
                    {
                        strRcv += Encoding.UTF8.GetString(abBuffer, 0, iRead);
                        
                        string []aryMsg = strRcv.Split('#');
                        for (int i = 1; i < aryMsg.Length; i++)
                        {
                            iResponseLength = iResponseLength + Convert.ToInt32(aryMsg[i].Substring(0, 4));
                        }
                    
                        try
                        {
                            if(Convert.ToInt32(strRcv.Substring(1,4))==iRead)
                            {
                                strReceived = strRcv;
                                bReturn = true;
                                break;
                            }
                            else if(iResponseLength==iRead)
                            {
                                strReceived = strRcv;
                                bReturn = true;
                                break;
                            }
                            
                            
                        }
                        catch (Exception oEx)
                        {
                        }
                    }
                    else
                    {
                        bReturn = false;
                        LogFile.writeLog(LOG_OPTIONS.DEBUG, "Read complete.");
                        break;
                    }
                }
            }
            catch (Exception oEx)
            {
                bReturn = false;
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in read(ref string strBuffer) - " + oEx.Message);
            }
            return bReturn;
        }

        public void close()
        {
            try
            {
                LogFile.writeLog(LOG_OPTIONS.DEBUG, "Closing socket.");
                if (m_oSocket.Connected)
                {
                    m_oSocket.Shutdown(SocketShutdown.Both);
                    m_oSocket.Close();
                    LogFile.writeLog(LOG_OPTIONS.DEBUG, "Socket closed.");
                }
            }
            catch (Exception oEx)
            {
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception occured while closing the socket : " + oEx.Message);
            }
        }

    }
}
