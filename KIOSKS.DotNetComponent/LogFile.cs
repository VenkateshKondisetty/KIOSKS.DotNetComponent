using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KioskDotNetService
{
    
        enum LOG_OPTIONS : int
        {
            ERROR,
            INFO,
            DEBUG,
            WARNING,
            SQL,
            XML,
            KEYS,
            SECURE,
            PROPERTIES
        };

        enum LOG_CHANNEL : int
        {
            FILE,
            ONLINE,
            BOTH
        };

        static class LogFile
        {
            private static int MAX_LOG_INDEX;
            private static int MAX_LOG_SIZE; //Megabites

            private static string m_strFilePath;
            private static string m_strFile;
            private static string m_strFileName;
            private static string m_strCurrentFile;
            private static StreamWriter m_oSW = null;
            private static FileStream m_oFile = null;
            private static int m_iSequenceId;

            private static bool m_bError;
            private static bool m_bInfo;
            private static bool m_bDebug;
            private static bool m_bWarning;
            private static bool m_bSql;
            private static bool m_bXml;
            private static bool m_bKeys;
            private static bool m_bSecure;
            private static bool m_bProperties;

            private static object m_oLock = new object();
            private static object m_oReadLock = new object();

            static LogFile()
            {
                createPath();
                if (KioskDotNetConfig.Default.LOG_FILE_PATH.Trim().LastIndexOf('\\') == (KioskDotNetConfig.Default.LOG_FILE_PATH.Trim().Length - 1))
                {
                    m_strFilePath = KioskDotNetConfig.Default.LOG_FILE_PATH;
                }
                else
                {
                    m_strFilePath = KioskDotNetConfig.Default.LOG_FILE_PATH + "\\";
                }
                m_strFileName = KioskDotNetConfig.Default.LOG_FILE_NAME;
                MAX_LOG_INDEX = KioskDotNetConfig.Default.LOG_FILE_COUNT;
                MAX_LOG_SIZE = KioskDotNetConfig.Default.MAX_LOG_SIZE;
                m_bError = KioskDotNetConfig.Default.LOG_OPTION_ERROR;
                m_bInfo = KioskDotNetConfig.Default.LOG_OPTION_INFO;
                m_bDebug = KioskDotNetConfig.Default.LOG_OPTION_DEBUG;
                m_bWarning = KioskDotNetConfig.Default.LOG_OPTION_WARNING;
                m_bSql = KioskDotNetConfig.Default.LOG_OPTION_SQL;
                m_bXml = KioskDotNetConfig.Default.LOG_OPTION_XML;
                m_bKeys = KioskDotNetConfig.Default.LOG_OPTION_KEYS;
                m_bSecure = KioskDotNetConfig.Default.LOG_OPTION_SECURE;
                m_bProperties = true;
                m_iSequenceId = 0;
                m_strCurrentFile = getCurrentLogFile();
                writeHeader();
            }

            private static void writeHeader()
            {
                SettingsPropertyCollection oProperties = KioskDotNetConfig.Default.Properties;
                writeLog(LOG_OPTIONS.PROPERTIES, "=============================================================");
                writeLog(LOG_OPTIONS.PROPERTIES, "Date            : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                writeLog(LOG_OPTIONS.PROPERTIES, "Application Name: " + KioskDotNetConfig.Default.MODULE_NAME);
                writeLog(LOG_OPTIONS.PROPERTIES, "Version         : " + KioskDotNetConfig.Default.MODULE_VERSION);
                writeLog(LOG_OPTIONS.PROPERTIES, "Copyright by " + KioskDotNetConfig.Default.COMPANY_NAME);
                writeLog(LOG_OPTIONS.PROPERTIES, "-------------------------------------------------------------");
                writeLog(LOG_OPTIONS.PROPERTIES, "Application Settings...");
                foreach (SettingsProperty oProperty in oProperties)
                {
                    writeLog(LOG_OPTIONS.PROPERTIES, oProperty.Name + ": " + KioskDotNetConfig.Default.PropertyValues[oProperty.Name].PropertyValue);
                }
                writeLog(LOG_OPTIONS.PROPERTIES, "=============================================================");
            }

            private static void createPath()
            {
                if (!Directory.Exists(KioskDotNetConfig.Default.LOG_FILE_PATH))
                {
                    Directory.CreateDirectory(KioskDotNetConfig.Default.LOG_FILE_PATH);
                }
            }

            public static void setLogOptions(bool bError, bool bInfo, bool bDebug, bool bWarning, bool bSql, bool bXml, bool bKeys, bool bSecure)
            {
                lock (m_oLock)
                {
                    m_bError = bError;
                    m_bInfo = bInfo;
                    m_bDebug = bDebug;
                    m_bWarning = bWarning;
                    m_bSql = bSql;
                    m_bXml = bXml;
                    m_bKeys = bKeys;
                    m_bSecure = bSecure;
                }
            }

            public static void writeLog(LOG_OPTIONS options, string strLine)
            {
                lock (m_oLock)
                {
                    try
                    {
                        if (m_strFilePath.Trim().Length != 0)
                        {
                            m_strFile = m_strFilePath + getLogFile();
                            m_oFile = new FileStream(m_strFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                            m_oSW = new StreamWriter(m_oFile);
                            writeLine(options, strLine);
                        }
                    }
                    catch (Exception)
                    {
                        m_oFile.Close();
                    }
                }

            }

            private static void writeLine(LOG_OPTIONS Option, string strLine)
            {
                string strDebugLine = "";
                try
                {
                    m_oFile.Seek(0, SeekOrigin.End);
                    switch (Option)
                    {
                        case LOG_OPTIONS.DEBUG:
                            if (m_bDebug)
                            {
                                strDebugLine = "[" + Convert.ToString(DateTime.Now) + "][DEBUG]" + strLine;
                            }

                            break;
                        case LOG_OPTIONS.ERROR:
                            if (m_bError)
                            {
                                strDebugLine = "[" + Convert.ToString(DateTime.Now) + "][ERROR]" + strLine;
                            }
                            break;
                        case LOG_OPTIONS.INFO:
                            if (m_bInfo)
                            {
                                strDebugLine = "[" + Convert.ToString(DateTime.Now) + "][INFO]" + strLine;
                            }
                            break;
                        case LOG_OPTIONS.KEYS:
                            if (m_bKeys)
                            {
                                strDebugLine = "[" + Convert.ToString(DateTime.Now) + "][KEY]" + strLine;
                            }
                            break;
                        case LOG_OPTIONS.SECURE:
                            if (m_bSecure)
                            {
                                strDebugLine = "[" + Convert.ToString(DateTime.Now) + "][SECURE]" + strLine;
                            }
                            break;
                        case LOG_OPTIONS.SQL:
                            if (m_bSql)
                            {
                                strDebugLine = "[" + Convert.ToString(DateTime.Now) + "][SQL]" + strLine;
                            }
                            break;
                        case LOG_OPTIONS.WARNING:
                            if (m_bWarning)
                            {
                                strDebugLine = "[" + Convert.ToString(DateTime.Now) + "][WARNING]" + strLine;
                            }
                            break;
                        case LOG_OPTIONS.PROPERTIES:
                            if (m_bProperties)
                            {
                                strDebugLine = "[" + Convert.ToString(DateTime.Now) + "][PROPERTIES]" + strLine;
                            }
                            break;
                        case LOG_OPTIONS.XML:
                            if (m_bXml)
                            {
                                if (!m_bSecure)
                                {
                                    if ((strLine.IndexOf("<PIN_BLOCK>") >= 0) && (strLine.IndexOf("</PIN_BLOCK>") > strLine.IndexOf("<PIN_BLOCK>")))
                                    {
                                        string strFirstPart = strLine.Substring(0, (strLine.IndexOf("<PIN_BLOCK>") + 11));
                                        string strLastPart = strLine.Substring(strLine.IndexOf("</PIN_BLOCK>"));
                                        strLine = strFirstPart + "..." + strLastPart;
                                    }
                                    if ((strLine.IndexOf("<PIN_REF>") >= 0) && (strLine.IndexOf("</PIN_REF>") > strLine.IndexOf("<PIN_REF>")))
                                    {
                                        string strFirstPart = strLine.Substring(0, (strLine.IndexOf("<PIN_REF>") + 9));
                                        string strLastPart = strLine.Substring(strLine.IndexOf("</PIN_REF>"));
                                        strLine = strFirstPart + "..." + strLastPart;
                                    }
                                    if ((strLine.IndexOf("DECRYPTED_VALUE") >= 0) && (strLine.IndexOf("</DECRYPTED_VALUE>") > strLine.IndexOf("<DECRYPTED_VALUE>")))
                                    {
                                        string strFirstPart = strLine.Substring(0, (strLine.IndexOf("<DECRYPTED_VALUE>") + 17));
                                        string strLastPart = strLine.Substring(strLine.IndexOf("</DECRYPTED_VALUE>"));
                                        strLine = strFirstPart + "..." + strLastPart;
                                    }
                                }
                                strDebugLine = "[" + Convert.ToString(DateTime.Now) + "] " + strLine;
                            }
                            break;
                    }
                    if (strDebugLine.Trim().Length > 0)
                    {
                        m_oSW.WriteLine(strDebugLine);
                    }
                    m_oSW.Flush();
                    m_oSW.Close();
                    m_oFile.Close();
                }
                catch
                {
                }
            }

            private static string getLogFile()
            {
                if (m_strCurrentFile.IndexOf(DateTime.Now.ToString("ddMMyyyy")) < 0)
                {
                    m_strCurrentFile = getCurrentLogFile();
                }
                else
                {
                    FileInfo oFile = new FileInfo(m_strFilePath + m_strCurrentFile);
                    if ((oFile.Length / (1024 * 1024)) >= Convert.ToInt16(MAX_LOG_SIZE))
                    {
                        m_strCurrentFile = m_strFileName + DateTime.Now.ToString("ddMMyyyy") + "_" + getNextLogFileIndex().ToString() + ".txt";
                    }
                }
                return m_strCurrentFile;
            }

            private static int getNextLogFileIndex()
            {
                return (Convert.ToInt16(m_strCurrentFile.Substring(m_strCurrentFile.IndexOf("_") + 1, ((m_strCurrentFile.IndexOf(".") - m_strCurrentFile.IndexOf("_") - 1)))) + 1);
            }

            private static string getCurrentLogFile()
            {
                string strCurrentLogFile = "";
                string strFilePattern = m_strFileName + DateTime.Now.ToString("ddMMyyyy") + "*.txt";
                string[] strFiles = Directory.GetFiles(KioskDotNetConfig.Default.LOG_FILE_PATH, strFilePattern, SearchOption.TopDirectoryOnly);
                cleanLogs();
                if (strFiles != null)
                {
                    if (strFiles.Length > 0)
                    {
                        List<string> lstFiles = new List<string>();
                        foreach (string strFile in strFiles)
                        {
                            lstFiles.Add(strFile);
                        }
                        lstFiles.Sort();
                        string strLogFile = lstFiles[lstFiles.Count - 1].Substring(lstFiles[lstFiles.Count - 1].LastIndexOf('\\') + 1);
                        int i1 = strLogFile.LastIndexOf(".");
                        int i2 = strLogFile.LastIndexOf("_");


                        int iIndex = Convert.ToInt16(strLogFile.Substring(strLogFile.LastIndexOf("_") + 1, ((strLogFile.LastIndexOf(".") - (strLogFile.LastIndexOf("_")) - 1))).Trim());
                        FileInfo oFile = new FileInfo(m_strFilePath + "\\" + strLogFile);
                        if ((oFile.Length / (1024 * 1024)) >= Convert.ToInt16(MAX_LOG_SIZE))
                        {
                            iIndex = iIndex + 1;
                            strCurrentLogFile = m_strFileName + DateTime.Now.ToString("ddMMyyyy") + "_" + iIndex.ToString() + ".txt";
                            FileStream oFS = new FileStream(m_strFilePath + strCurrentLogFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                            oFS.Close();
                            return strCurrentLogFile;
                        }
                        else
                        {
                            return strLogFile;
                        }
                    }
                    else
                    {
                        strCurrentLogFile = m_strFileName + DateTime.Now.ToString("ddMMyyyy") + "_0.txt";
                        FileStream oFS = new FileStream(m_strFilePath + strCurrentLogFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                        oFS.Close();
                        return strCurrentLogFile;
                    }
                }
                else
                {
                    strCurrentLogFile = m_strFileName + DateTime.Now.ToString("ddMMyyyy") + "_0.txt";
                    FileStream oFS = new FileStream(m_strFilePath + strCurrentLogFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                    oFS.Close();
                    return strCurrentLogFile;
                }
            }

            private static void cleanLogs()
            {
                try
                {
                    DirectoryInfo oDirInfo = new DirectoryInfo(KioskDotNetConfig.Default.LOG_FILE_PATH);
                    FileInfo[] oFiles = oDirInfo.GetFiles(KioskDotNetConfig.Default.LOG_FILE_NAME + "*.txt");
                    foreach (FileInfo oFile in oFiles)
                    {
                        TimeSpan oDateTime = DateTime.Now.Subtract(oFile.LastWriteTime);
                        if (oDateTime.TotalDays > KioskDotNetConfig.Default.LOG_RETAIN_HISTORY)
                        {
                            oFile.Delete();
                        }
                    }
                }
                catch
                {
                }
            }
        }
    
}
