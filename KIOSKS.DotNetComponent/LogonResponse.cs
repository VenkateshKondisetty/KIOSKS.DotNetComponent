using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KioskDotNetService
{
     static class LogonResponse
    {
        public static string SuccessFlag { get; set; }
        public static string ResponseCode { get; set; }
        public static string  ResponseText { get; set; }

        public static string TerminalID { get; set; }
        public static string MerchantID { get; set; }
        public static string  BankDate { get; set; }
        public static string BankTime { get; set; }
        public static string  Stan { get; set; }
        public static string PinPadVersion { get; set; }
    }
}
