using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KioskDotNetService
{
    class KConnection
    {
        private TcpClient m_oSocket;
        private int m_iBufferSize;
        private NetworkStream stream;
        
        

        public KConnection(TcpClient oSocket)
        {
            try
            {
                LogFile.writeLog(LOG_OPTIONS.DEBUG, "Creating connection object.");
                m_oSocket = oSocket;
                m_iBufferSize = KioskDotNetConfig.Default.BUFFER_SIZE;
                LogFile.writeLog(LOG_OPTIONS.DEBUG, "Connection object created.");
            }
            catch (Exception oEx)
            {
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: KConnection()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
            }
        }

        public async void run()
        {

            byte[] abBuffer = new byte[KioskDotNetConfig.Default.BUFFER_SIZE];

            try
            {
                if (m_oSocket.Connected)
                {
                    m_oSocket.ReceiveTimeout = KioskDotNetConfig.Default.SOCKET_READ_TIMEOUT * 1000;
                    while (true)
                    {
                        stream = m_oSocket.GetStream();
                        while (!stream.DataAvailable) ;

                        Byte[] bytes = new Byte[m_oSocket.Available];

                        stream.Read(bytes, 0, bytes.Length);
                        //translate bytes of request to string
                        String data = Encoding.UTF8.GetString(bytes);
                        string decodedMsg = EncodeDecode.DecodeMessage(bytes);
                        //APIService.Client.APIClient client = new APIService.Client.APIClient();
                        //await client.GetAPIResponse<object>("api/gift-card/detail");
                        LogFile.writeLog(LOG_OPTIONS.DEBUG, "Message Received From Browser :" + decodedMsg);


                        if (new Regex("^GET").IsMatch(data))
                        {
                            Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                                + "Connection: Upgrade" + Environment.NewLine
                                + "Upgrade: websocket" + Environment.NewLine
                                + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                                    SHA1.Create().ComputeHash(
                                        Encoding.UTF8.GetBytes(
                                            new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                                        )
                                    )
                                ) + Environment.NewLine
                                + Environment.NewLine);
                            String initialResponse = Encoding.UTF8.GetString(response);
                            stream.Write(response, 0, response.Length);
                            LogFile.writeLog(LOG_OPTIONS.DEBUG, "Response Wrote to Browser :" + initialResponse);
                        }
                        else
                        {
                            ProcessResponsesMain(decodedMsg, stream);

                        }

                    }
                }
            }
            catch (Exception oEx)
            {
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: run()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
                m_oSocket.Close();
            }
        }
        private void ProcessDeviceStatusCheck()
        {
            //check device status
        }
        private void CheckStatus(NetworkStream NSMain)
        {
            //ProcessPCEFTPOSResponses("checkstatus", NSMain);

        }
        private void ProcessInitialStatus(string request, NetworkStream NSMain)
        {
            ProcessDeviceStatusCheck();
            KPCEFTPOS objPCEFTPOS = new KPCEFTPOS(NSMain);
            objPCEFTPOS.ProcessPCEFTPOSResponses(request);
            
        }
       
        private void ProcessResponsesMain(string request,NetworkStream NSMain)
        {

            try
            {
                KPCEFTPOS objPCEFTPOS = new KPCEFTPOS(NSMain);
                if(request=="initialstatus")
                {
                    ProcessInitialStatus(request,NSMain);
                }
                else if(request.Contains("getlasttran"))
                {
                    objPCEFTPOS.ProcessPCEFTPOSResponses("logon");
                    objPCEFTPOS.ProcessPCEFTPOSResponses(request);
                }
                else if(request=="checkstatus")
                {
                    objPCEFTPOS.ProcessPCEFTPOSResponses(request);
                }
                else if(request.Contains("pay"))
                {
                    objPCEFTPOS.ProcessPCEFTPOSResponses(request);
                    
                }
                else if (request == "logon")
                {
                    objPCEFTPOS.ProcessPCEFTPOSResponses(request);
                }
                else if (request == "rsalogon")
                {
                    objPCEFTPOS.ProcessPCEFTPOSResponses(request);
                }
                else if (request == "scan")
                {
                    BarcodeScanner  oScanner = new BarcodeScanner(NSMain);
                    if (oScanner.EnableBarcodeScanner())
                    {
                        LogFile.writeLog(LOG_OPTIONS.DEBUG, "Barcode scanner enabled");
                        //oScanner.Changed += new BarcodeScanner.ChangedEventHandler(ProcessBarcodeScan);
                    }
                    else
                    {
                        LogFile.writeLog(LOG_OPTIONS.ERROR, "Cannot Enable the Barcode scanner");
                    }
                }
                else
                {
                    Byte[] response =EncodeDecode.EncodeMessageToSend("Unidentified request");
                    stream.Write(response, 0, response.Length);
                }
                

            }
            catch (Exception oEx)
            {

                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: processResponsesMain()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
            }
        }
        public void closeConnection()
        {
            try
            {
                m_oSocket.Close();
            }
            catch (Exception oEx)
            {
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: CloseConnection()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
            }
        }
    }
}
