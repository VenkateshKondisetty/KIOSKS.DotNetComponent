using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreScanner;
using System.Xml;
using System.Net.Sockets;

namespace KioskDotNetService
{
    class BarcodeScanner
    {
        CCoreScannerClass cCoreScannerClass;
        //public delegate void ChangedEventHandler(string BarcodeNumber);
        //public event ChangedEventHandler Changed;
        private NetworkStream stream;
        

        public BarcodeScanner(NetworkStream NSMain)
        {
            stream = NSMain;
            //Instantiate CoreScanner Class
            cCoreScannerClass = new CCoreScannerClass();
           
            //Call Open API
            short[] scannerTypes = new short[1];//Scanner Types you are interested in
            scannerTypes[0] = 1; // 1 for all scanner types
            short numberOfScannerTypes = 1; // Size of the scannerTypes array
            int status; // Extended API return code
            cCoreScannerClass.Open(0, scannerTypes, numberOfScannerTypes, out status);
            // Subscribe for barcode events in cCoreScannerClass
            cCoreScannerClass.BarcodeEvent += new
            _ICoreScannerEvents_BarcodeEventEventHandler(OnBarcodeEvent);
            // Let's subscribe for events
            int opcode = 1001; // Method for Subscribe events
            string outXML; // XML Output
            string inXML = "<inArgs>" +
            "<cmdArgs>" +
            "<arg-int>1</arg-int>" + // Number of events you want to subscribe
            "<arg-int>1</arg-int>" + // Comma separated event IDs
            "</cmdArgs>" +
            "</inArgs>";
            cCoreScannerClass.ExecCommand(opcode, ref inXML, out outXML, out status);
            


    }
        private string DecodeBarcode(string strXml)
        {
            System.Diagnostics.Debug.WriteLine("Initial XML" + strXml);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(strXml);

            string strData = String.Empty;
            string barcode = xmlDoc.DocumentElement.GetElementsByTagName("datalabel").Item(0).InnerText;
            string symbology = xmlDoc.DocumentElement.GetElementsByTagName("datatype").Item(0).InnerText;
            string[] numbers = barcode.Split(' ');

            foreach (string number in numbers)
            {
                if (String.IsNullOrEmpty(number))
                {
                    break;
                }

                strData += ((char)Convert.ToInt32(number, 16)).ToString();
            }
            return strData;

        }
        void OnBarcodeEvent(short eventType, ref string pscanData)
        {
            string BarcodeNumber = DecodeBarcode(pscanData);
            //if (Changed !=null)
            //{
            //    Changed(BarcodeNumber);
            //}
            ProcessBarcodeScan(BarcodeNumber);
        }
        void ProcessBarcodeScan(string strBarcode)
        {
            try
            {
                Byte[] response = EncodeDecode.EncodeMessageToSend(strBarcode);
                stream.Write(response, 0, response.Length);

                processStopScan();
            }
            catch (Exception oEx)
            {

                LogFile.writeLog(LOG_OPTIONS.ERROR, "Sending to web Browser failed");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
            }


        }
        private void processStopScan()
        {
            try
            {
                if (DisableBarcodeScanner())
                {
                    Byte[] response =EncodeDecode.EncodeMessageToSend("stop scan successful");
                    stream.Write(response, 0, response.Length);
                    DisposeScan();
                }
                else
                {
                    Byte[] response = EncodeDecode.EncodeMessageToSend("stop scan unsuccessful");
                    stream.Write(response, 0, response.Length);
                    DisposeScan();
                }

            }
            catch (Exception oEx)
            {
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Process stop scan failed");
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception error: " + oEx.Message);
                LogFile.writeLog(LOG_OPTIONS.ERROR, "Exception trace: " + oEx.StackTrace);
            }


        }
        public bool EnableBarcodeScanner()
        {
            int opCode = 2014;
            int status = 1;
           string inXml = "<inArgs>" +
                                "<scannerID>1</scannerID>" + "</inArgs>";
            string outXml = "";
            cCoreScannerClass.ExecCommand(opCode, ref inXml, out outXml, out status); 
            if (status==1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool DisableBarcodeScanner()
        {
            int opCode = 2013;
            int status = 1;
            string inXml = "<inArgs>" +
                                 "<scannerID>1</scannerID>" + "</inArgs>";
            string outXml = "";
            cCoreScannerClass.ExecCommand(opCode, ref inXml, out outXml, out status);
            if (status == 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool DisposeScan()
        {
            int iStatus = 1;
            cCoreScannerClass.Close(0, out iStatus);
            if (iStatus==1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
