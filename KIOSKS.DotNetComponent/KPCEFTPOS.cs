using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KioskDotNetService
{
    public class KPCEFTPOS
    {
        private bool bPCEFTPOS_LOGGEDON = false;
        private bool bPCEFTPOS_RSA_LOGGEDON = false;
        NetworkStream stream;
        public KPCEFTPOS(NetworkStream NSMain)
        {
            stream = NSMain;
        }

        public void ProcessPCEFTPOSResponses(string strRequest)
        {
            try
            {
                KSocketCommunicator oSocketPCEFTPOS = new KSocketCommunicator();
                LogFile.writeLog(LOG_OPTIONS.DEBUG, "Connecting to PCEFTPOS Socket...");
                oSocketPCEFTPOS.connect(KioskDotNetConfig.Default.PCEFTPOS_IP, KioskDotNetConfig.Default.PCEFTPOS_PORT);
                LogFile.writeLog(LOG_OPTIONS.DEBUG, "Connected to PCEFTPOS Socket");
                LogFile.writeLog(LOG_OPTIONS.DEBUG, "Sending request to PCEFTPOS......");
                LogFile.writeLog(LOG_OPTIONS.INFO, "Request received from Kiosk : " + strRequest);
                if (oSocketPCEFTPOS.send(ProcessRequests(strRequest)))
                {
                    processReceivedResponses(stream, strRequest, oSocketPCEFTPOS);
                }
                else
                {
                    LogFile.writeLog(LOG_OPTIONS.ERROR, "Sending to PCEFTPOS failed");
                }
            }
            catch (Exception oEx)
            {
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: ConnectToPCEFTPOS()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);

            }

        }
        private string ProcessRequests(string RequestFromBrowser)
        {
            string strCommand = "";
            try
            {
                
                if (RequestFromBrowser == "logon")
                {
                    strCommand = "#0016G 000100000";
                }
                else if (RequestFromBrowser == "rsalogon")
                {
                    strCommand = "#0016G4000100000";
                }
                else if (RequestFromBrowser == "initialstatus")
                {
                    strCommand = "#0011K   00";
                }
                else if (RequestFromBrowser == "#00073L")
                {
                    strCommand = "#00073";
                }
                else if (RequestFromBrowser == "checkstatus")
                {
                    strCommand = "#0011K   00";
                }
                else if (RequestFromBrowser.Contains("pay"))
                {
                    string amount = RequestFromBrowser.Split(' ')[1];
                    string tranID = RequestFromBrowser.Split(' ')[2];
                    tranID = tranID.TrimEnd();
                    tranID = tranID.PadRight(16, ' ');
                    amount = Convert.ToInt64((Convert.ToDecimal(amount) * 100)).ToString();
                    amount = amount = amount.PadLeft(9, '0');
                    //string cardno = "4507880121656412";
                    strCommand = "#0161M000P00000000000" + amount + "000000" + tranID + "00                                                                  00                                    000";

                }
                else if(RequestFromBrowser.Contains("getlasttran"))
                {
                    //string tranID = RequestFromBrowser.Split(' ')[1];
                    //tranID = tranID.TrimEnd();
                    //tranID = tranID.PadRight(16, ' ');
                    strCommand = "#0009N000";//+ tranID;
                }
                else if (RequestFromBrowser == "#00073C")
                {
                    strCommand = "#00073 ";
                }
                else if (RequestFromBrowser == "#00073A")
                {
                    strCommand = "#00073 ";
                }
                else if (RequestFromBrowser == "preceipt")
                {
                    strCommand = "#00073 ";
                }
            }
            catch (Exception oEx)
            {

                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: ProcessRequests()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Command processed to send to PCEFTPOS: " + strCommand);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
            }
          
            LogFile.writeLog(LOG_OPTIONS.INFO, "Command processed to send to PCEFTPOS: " + strCommand);
            return strCommand;
        }
        private void processReceivedResponses(NetworkStream stream, string request, KSocketCommunicator oSocketPCEFTPOS)
        {
            try
            {
                if (request == "initialstatus")
                {
                    processReceivedMsgInitialStatus(stream, oSocketPCEFTPOS);
                }
                else if (request == "checkstatus")
                {
                    processCheckStatus(stream, oSocketPCEFTPOS);
                }
                else if(request.Contains("getlasttran"))
                {
                    processGetLastTran(stream, oSocketPCEFTPOS);
                }
                else if (request.Contains("pay"))
                {
                    processPay(stream, oSocketPCEFTPOS);
                }
                else if (request.Contains("preceipt"))
                {
                    processPay(stream, oSocketPCEFTPOS);
                }
                else if (request == "#00073C")
                {
                    processPay(stream, oSocketPCEFTPOS);
                }
                else if (request == "#00073A")
                {
                    processPay(stream, oSocketPCEFTPOS);
                }
                else if (request == "#00073L")
                {
                    processLogon(stream, oSocketPCEFTPOS);
                }
                else if (request == "logon")
                {
                    processLogon(stream, oSocketPCEFTPOS);
                }
                else if (request == "rsalogon")
                {
                    processRSALogon(stream, oSocketPCEFTPOS);
                }
                
               

            }
            catch (Exception oEx)
            {

                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: processResponses()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
            }

        }
        private void processGetLastTran(NetworkStream stream, KSocketCommunicator oSocketPCEFTPOS)
        {
            try
            {
                string strResponse = "";
                bool bContinueWhile = true;
                while (true && bContinueWhile)
                {
                    if (oSocketPCEFTPOS.readForResponses(ref strResponse))
                    {

                        string[] aryMultipleMsg = strResponse.Split('#');
                        for (int i = 1; i < aryMultipleMsg.Length; i++)
                        {
                            LogFile.writeLog(LOG_OPTIONS.INFO, "Received Responses : " + "#" + aryMultipleMsg[i]);

                            if ((aryMultipleMsg[i].IndexOf('N') == 4))
                            {

                                if (aryMultipleMsg[i][6] == '0')
                                {

                                    string strResponse1 = aryMultipleMsg[i].Replace("\0", string.Empty);
                                    Byte[] response = EncodeDecode.EncodeMessageToSend("GetlastTranscation request failed : "+aryMultipleMsg[i].Substring(8, 2) + "," + aryMultipleMsg[i].Substring(10, 20));
                                    stream.Write(response, 0, response.Length);
                                    bContinueWhile = false;
                                    break;

                                }
                                else if (aryMultipleMsg[i][6] == '1')
                                {

                                    if(aryMultipleMsg[i][7] == '0')
                                    {
                                        Byte[] response = EncodeDecode.EncodeMessageToSend("Last Transcation Failed : " + aryMultipleMsg[i].Substring(8, 2) + "," + aryMultipleMsg[i].Substring(10, 20));
                                        stream.Write(response, 0, response.Length);
                                        bContinueWhile = false;
                                        break;
                                    }
                                    else if(aryMultipleMsg[i][7] == '1')
                                    {
                                        Byte[] response = EncodeDecode.EncodeMessageToSend("Last Transcation successfull : " + aryMultipleMsg[i].Substring(8, 2) + "," + aryMultipleMsg[i].Substring(10, 20));
                                        stream.Write(response, 0, response.Length);
                                        bContinueWhile = false;
                                        break;
                                    }

                                    
                                }

                            }


                        }

                    }
                    else
                    {
                        break;
                    }
                }

            }
            catch (Exception oEx)
            {

                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: processCheckStatus()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
            }

        }

        private void processCheckStatus(NetworkStream stream, KSocketCommunicator oSocketPCEFTPOS)
        {
            try
            {
                string strResponse = "";
                bool bContinueWhile = true;
                while (true && bContinueWhile)
                {
                    if (oSocketPCEFTPOS.readForResponses(ref strResponse))
                    {

                        string[] aryMultipleMsg = strResponse.Split('#');
                        for (int i = 1; i < aryMultipleMsg.Length; i++)
                        {
                            LogFile.writeLog(LOG_OPTIONS.INFO, "Received Responses : " + "#" + aryMultipleMsg[i]);

                            if ((aryMultipleMsg[i].IndexOf('K') == 4))
                            {

                                if (aryMultipleMsg[i][6] == '0')
                                {

                                    string strResponse1 = aryMultipleMsg[i].Replace("\0", string.Empty);
                                    Byte[] response = EncodeDecode.EncodeMessageToSend("Stutus failed : "+aryMultipleMsg[i].Substring(7, 2) + "," + aryMultipleMsg[i].Substring(9, 20));
                                    stream.Write(response, 0, response.Length);

                                }
                                else if (aryMultipleMsg[i][6] == '1')
                                {

                                    if (aryMultipleMsg[i].Substring(9, 20).TrimEnd() == "LOGON REQUIRED")
                                    {
                                        Byte[] response = EncodeDecode.EncodeMessageToSend("Need to Login : " + aryMultipleMsg[i].Substring(7, 2) + "," + aryMultipleMsg[i].Substring(9, 20));
                                        stream.Write(response, 0, response.Length);

                                        if (oSocketPCEFTPOS.send(ProcessRequests("logon")))
                                        {

                                            processReceivedResponses(stream, "logon", oSocketPCEFTPOS);
                                        }
                                        else
                                        {
                                            LogFile.writeLog(LOG_OPTIONS.ERROR, "Sending to PCEFTPOS failed");
                                        }

                                        if (oSocketPCEFTPOS.send(ProcessRequests("checkstatus")))
                                        {

                                            processReceivedResponses(stream, "checkstatus", oSocketPCEFTPOS);
                                        }
                                        else
                                        {
                                            LogFile.writeLog(LOG_OPTIONS.ERROR, "Sending to PCEFTPOS failed");
                                        }

                                        bContinueWhile = false;
                                        break;

                                    }
                                    else if (aryMultipleMsg[i].Substring(9, 20).TrimEnd() == "READY")
                                    {
                                        Byte[] response = EncodeDecode.EncodeMessageToSend("Ready for the payment : " + aryMultipleMsg[i].Substring(7, 2) + "," + aryMultipleMsg[i].Substring(9, 20));
                                        stream.Write(response, 0, response.Length);
                                        bContinueWhile = false;
                                        break;
                                    }
                                }

                            }


                        }

                    }
                    else
                    {
                        break;
                    }
                }

            }
            catch (Exception oEx)
            {

                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: processCheckStatus()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
            }
     
        }

        private void processReceivedMsgInitialStatus(NetworkStream stream, KSocketCommunicator oSocketPCEFTPOS)
        {
            string strResponse = "";
            bool bContinueWhile = true;
            while (true && bContinueWhile)
            {
                if (oSocketPCEFTPOS.readForResponses(ref strResponse))
                {

                    string[] aryMultipleMsg = strResponse.Split('#');
                    for (int i = 1; i < aryMultipleMsg.Length; i++)
                    {
                        LogFile.writeLog(LOG_OPTIONS.INFO, "Received Responses : " + "#" + aryMultipleMsg[i]);

                        if ((aryMultipleMsg[i].IndexOf('K') == 4))
                        {

                            if (aryMultipleMsg[i][6] == '0')
                            {

                                string strResponse1 = aryMultipleMsg[i].Replace("\0", string.Empty);
                                Byte[] response = EncodeDecode.EncodeMessageToSend(aryMultipleMsg[i].Substring(9, 20).TrimEnd());
                                stream.Write(response, 0, response.Length);

                            }
                            else if (aryMultipleMsg[i][6] == '1')
                            {
                                if (aryMultipleMsg[i].Substring(9, 20).TrimEnd() == "LOGON REQUIRED")
                                {
                                    if (oSocketPCEFTPOS.send(ProcessRequests("logon")))
                                    {

                                        processLogon(stream, oSocketPCEFTPOS);
                                    }
                                    else
                                    {
                                        LogFile.writeLog(LOG_OPTIONS.ERROR, "Sending to PCEFTPOS failed");
                                    }
                                    bContinueWhile = false;
                                    break;

                                }
                            }


                            //LogonResponse.SuccessFlag = aryMultipleMsg[i][6] == '1' ? "SUCCESS" : "FAIL";
                            //LogonResponse.ResponseCode = aryMultipleMsg[i].Substring(7, 2);
                            //LogonResponse.ResponseText = aryMultipleMsg[i].Substring(9, 20);
                            //LogonResponse.TerminalID = aryMultipleMsg[i].Substring(29, 8);
                            //LogonResponse.MerchantID = aryMultipleMsg[i].Substring(37, 15);
                            //LogonResponse.BankDate = aryMultipleMsg[i].Substring(52, 6);
                            //LogonResponse.BankTime = aryMultipleMsg[i].Substring(58, 6);
                            //LogonResponse.Stan = aryMultipleMsg[i].Substring(64, 6);
                            //LogonResponse.PinPadVersion = aryMultipleMsg[i].Substring(70, 16);

                            //bPCEFTPOS_LOGGEDON = true;
                            //string strResponse1 = aryMultipleMsg[i].Replace("\0", string.Empty);
                            //Byte[] response = EncodeMessageToSend("LogonSuccess");
                            //stream.Write(response, 0, response.Length);


                        }


                    }

                }
                else
                {
                    break;
                }
            }
        }
        private void processPay(NetworkStream stream, KSocketCommunicator oSocketPCEFTPOS)
        {
            try
            {
                string strResponse = "";
                bool bContinueWhile = true;
                string intermediateResponse = "";
                while (true && bContinueWhile)
                {
                    if (oSocketPCEFTPOS.readForResponses(ref strResponse))
                    {

                        string[] aryMultipleMsg = strResponse.Split('#');
                        for (int i = 1; i < aryMultipleMsg.Length; i++)
                        {
                            LogFile.writeLog(LOG_OPTIONS.INFO, "Received Responses : " + "#" + aryMultipleMsg[i]);

                            if ((aryMultipleMsg[i].IndexOf('M') == 4) && (aryMultipleMsg[i][6] == '1'))
                            {
                                //string strResponse1 = individualResponse.Replace("\0", string.Empty);
                                //Byte[] response = EncodeMessageToSend("#" + strResponse1);
                                //stream.Write(response, 0, response.Length);
                                //bContinueWhile = false;
                                //break;
                                //LogonResponse.SuccessFlag = aryMultipleMsg[i][6] == '1' ? "SUCCESS" : "FAIL";
                                //LogonResponse.ResponseCode = aryMultipleMsg[i].Substring(7, 2);
                                //LogonResponse.ResponseText = aryMultipleMsg[i].Substring(9, 20);
                                //LogonResponse.TerminalID = aryMultipleMsg[i].Substring(29, 8);
                                //LogonResponse.MerchantID = aryMultipleMsg[i].Substring(37, 15);
                                //LogonResponse.BankDate = aryMultipleMsg[i].Substring(52, 6);
                                //LogonResponse.BankTime = aryMultipleMsg[i].Substring(58, 6);
                                //LogonResponse.Stan = aryMultipleMsg[i].Substring(64, 6);
                                //LogonResponse.PinPadVersion = aryMultipleMsg[i].Substring(70, 16);



                                // bPCEFTPOS_LOGGEDON = true;
                                string strResponse1 = aryMultipleMsg[i].Replace("\0", string.Empty);
                                //Byte[] response = EncodeDecode.EncodeMessageToSend("PaymentSuccess : " + aryMultipleMsg[i].Substring(7, 2) + "," + aryMultipleMsg[i].Substring(9, 20));
                                Byte[] response = EncodeDecode.EncodeMessageToSend("PaymentSuccess : " + aryMultipleMsg[i]);
                                stream.Write(response, 0, response.Length);
                                bContinueWhile = false;
                                break;

                            }
                            else if ((aryMultipleMsg[i].IndexOf('M') == 4) && (aryMultipleMsg[i][6] == '0'))
                            {
                                //string strResponse1 = individualResponse.Replace("\0", string.Empty);
                                //Byte[] response = EncodeMessageToSend("#" + strResponse1);
                                //stream.Write(response, 0, response.Length);
                                //bContinueWhile = false;
                                //break;
                                bPCEFTPOS_LOGGEDON = false;
                                string strResponse1 = aryMultipleMsg[i].Replace("\0", string.Empty);
                                //Byte[] response =EncodeDecode.EncodeMessageToSend("Payment unsuccessful : " + aryMultipleMsg[i].Substring(7, 2) + "," + aryMultipleMsg[i].Substring(9, 20));
                                Byte[] response = EncodeDecode.EncodeMessageToSend("Payment unsuccessful : " + aryMultipleMsg[i]);
                                stream.Write(response, 0, response.Length);
                                bContinueWhile = false;
                                //break;


                            }
                            else if ((aryMultipleMsg[i].IndexOf('R') == 5))
                            {
                                // intermediateResponse = "preceipt";
                                if (oSocketPCEFTPOS.send(ProcessRequests("preceipt")))
                                {
                                    processReceivedResponses(stream, "preceipt", oSocketPCEFTPOS);
                                }
                                else
                                {
                                    LogFile.writeLog(LOG_OPTIONS.ERROR, "Sending to PCEFTPOS failed");
                                }
                                bContinueWhile = false;
                                break;
                            }
                            else if (aryMultipleMsg[i] == "00073C")
                            {
                                //intermediateResponse = "00073C";

                                if (oSocketPCEFTPOS.send(ProcessRequests("#00073C")))
                                {
                                    processReceivedResponses(stream, "#00073C", oSocketPCEFTPOS);
                                }
                                else
                                {
                                    LogFile.writeLog(LOG_OPTIONS.ERROR, "Sending to PCEFTPOS failed");
                                }

                                bContinueWhile = false;
                                break;


                            }
                            else if (aryMultipleMsg[i] == "00073A")
                            {
                                //intermediateResponse = "00073C";

                                if (oSocketPCEFTPOS.send(ProcessRequests("#00073A")))
                                {
                                    processReceivedResponses(stream, "#00073A", oSocketPCEFTPOS);
                                }
                                else
                                {
                                    LogFile.writeLog(LOG_OPTIONS.ERROR, "Sending to PCEFTPOS failed");
                                }

                                bContinueWhile = false;
                                break;


                            }

                        }

                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception oEx)
            {

                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: processPay()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
            }
 
 
        }

        private void processLogon(NetworkStream stream, KSocketCommunicator oSocketPCEFTPOS)
        {
            try
            {
                string strResponse = "";
                bool bContinueWhile = true;
                while (true && bContinueWhile)
                {
                    if (oSocketPCEFTPOS.readForResponses(ref strResponse))
                    {

                        string[] aryMultipleMsg = strResponse.Split('#');
                        for (int i = 1; i < aryMultipleMsg.Length; i++)
                        {
                            LogFile.writeLog(LOG_OPTIONS.INFO, "Received Responses : " + "#" + aryMultipleMsg[i]);

                            if ((aryMultipleMsg[i].IndexOf('G') == 4) && (aryMultipleMsg[i][6] == '1'))
                            {
                                //string strResponse1 = individualResponse.Replace("\0", string.Empty);
                                //Byte[] response = EncodeMessageToSend("#" + strResponse1);
                                //stream.Write(response, 0, response.Length);
                                //bContinueWhile = false;
                                //break;
                                LogonResponse.SuccessFlag = aryMultipleMsg[i][6] == '1' ? "SUCCESS" : "FAIL";
                                LogonResponse.ResponseCode = aryMultipleMsg[i].Substring(7, 2);
                                LogonResponse.ResponseText = aryMultipleMsg[i].Substring(9, 20);
                                LogonResponse.TerminalID = aryMultipleMsg[i].Substring(29, 8);
                                LogonResponse.MerchantID = aryMultipleMsg[i].Substring(37, 15);
                                LogonResponse.BankDate = aryMultipleMsg[i].Substring(52, 6);
                                LogonResponse.BankTime = aryMultipleMsg[i].Substring(58, 6);
                                LogonResponse.Stan = aryMultipleMsg[i].Substring(64, 6);
                                LogonResponse.PinPadVersion = aryMultipleMsg[i].Substring(70, 16);



                                bPCEFTPOS_LOGGEDON = true;
                                string strResponse1 = aryMultipleMsg[i].Replace("\0", string.Empty);
                                Byte[] response =EncodeDecode.EncodeMessageToSend("LogonSuccess: "+ aryMultipleMsg[i].Substring(7, 2)+","+ aryMultipleMsg[i].Substring(9, 20));
                                stream.Write(response, 0, response.Length);
                                bContinueWhile = false;
                                break;

                            }
                            else if ((aryMultipleMsg[i].IndexOf('G') == 4) && (aryMultipleMsg[i][6] == '0'))
                            {
                                //string strResponse1 = individualResponse.Replace("\0", string.Empty);
                                //Byte[] response = EncodeMessageToSend("#" + strResponse1);
                                //stream.Write(response, 0, response.Length);
                                //bContinueWhile = false;
                                //break;
                                bPCEFTPOS_LOGGEDON = false;
                                string strResponse1 = aryMultipleMsg[i].Replace("\0", string.Empty);
                                Byte[] response=EncodeDecode.EncodeMessageToSend("LogonFail : " + aryMultipleMsg[i].Substring(7, 2) + "," + aryMultipleMsg[i].Substring(9, 20));
                                stream.Write(response, 0, response.Length);
                                bContinueWhile = false;
                                //break;


                            }
                            //else if (aryMultipleMsg[i]=="00073L")
                            //{
                            //    ProcessPCEFTPOSResponses("#00073L", stream);

                            //}

                        }

                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception oEx)
            {

                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: processLogon()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
            }
 
        }
        private void processRSALogon(NetworkStream stream, KSocketCommunicator oSocketPCEFTPOS)
        {
            try
            {
                string strResponse = "";
                bool bContinueWhile = true;
                while (true && bContinueWhile)
                {
                    if (oSocketPCEFTPOS.readForResponses(ref strResponse))
                    {

                        string[] aryMultipleMsg = strResponse.Split('#');
                        for (int i = 1; i < aryMultipleMsg.Length; i++)
                        {
                            LogFile.writeLog(LOG_OPTIONS.INFO, "Received Responses : " + "#" + aryMultipleMsg[i]);

                            if ((aryMultipleMsg[i].IndexOf('G') == 4) && (aryMultipleMsg[i][6] == '1'))
                            {
                               
                                LogonResponse.SuccessFlag = aryMultipleMsg[i][6] == '1' ? "SUCCESS" : "FAIL";
                                LogonResponse.ResponseCode = aryMultipleMsg[i].Substring(7, 2);
                                LogonResponse.ResponseText = aryMultipleMsg[i].Substring(9, 20);
                                LogonResponse.TerminalID = aryMultipleMsg[i].Substring(29, 8);
                                LogonResponse.MerchantID = aryMultipleMsg[i].Substring(37, 15);
                                LogonResponse.BankDate = aryMultipleMsg[i].Substring(52, 6);
                                LogonResponse.BankTime = aryMultipleMsg[i].Substring(58, 6);
                                LogonResponse.Stan = aryMultipleMsg[i].Substring(64, 6);
                                LogonResponse.PinPadVersion = aryMultipleMsg[i].Substring(70, 16);

                                bPCEFTPOS_RSA_LOGGEDON = true;
                                string strResponse1 = aryMultipleMsg[i].Replace("\0", string.Empty);
                                Byte[] response = EncodeDecode.EncodeMessageToSend("RSALogonSuccess: " + aryMultipleMsg[i].Substring(7, 2) + "," + aryMultipleMsg[i].Substring(9, 20));
                                stream.Write(response, 0, response.Length);
                                bContinueWhile = false;
                                break;

                            }
                            else if ((aryMultipleMsg[i].IndexOf('G') == 4) && (aryMultipleMsg[i][6] == '0'))
                            {
                               
                                bPCEFTPOS_RSA_LOGGEDON = false;
                                string strResponse1 = aryMultipleMsg[i].Replace("\0", string.Empty);
                                Byte[] response = EncodeDecode.EncodeMessageToSend("RSALogonFail : " + aryMultipleMsg[i].Substring(7, 2) + "," + aryMultipleMsg[i].Substring(9, 20));
                                stream.Write(response, 0, response.Length);
                                bContinueWhile = false;
                                //break;

                            }

                        }

                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception oEx)
            {

                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error occured in function: processRSALogon()");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
            }
         
        }
               
    }
}
