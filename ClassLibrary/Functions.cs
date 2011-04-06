using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Net;
using System.Diagnostics;
using System.Threading;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;

namespace ClassLibrary
{
    public static class Functions
    {
        public static Int32 ReadTransferType(this NetIncomingMessage message)
        {
            return message.ReadInt32();
        }
        public static Boolean IsServerRunning()
        {
            Process[] pname = Process.GetProcessesByName(Constants.ServerFileName);
            if (pname.Length != 0)
            {
                return true;
            }
            return false;
        }
        //För att starta client när server körs
        public static void RunClientFromServer()
        {
             RunProgram(Constants.ClientFileName);
        }

        public static void RunServerFromPath()
        {
            RunProgram(Constants.ServerFileName);
        }

        public static void RunProgram(String processName)
        {
            //Nuvarande sökväg
            String currentDir = System.Environment.CurrentDirectory;
            //Alla processer som heter samma sak som processName
            Process[] pname = Process.GetProcessesByName(processName);
            //Alla strängar i arrayen kontrolleras
            for (int i = 0; i < Constants.curPath.Length; i++)
            {
                //Om den adress man står i slutar med curPath
                if (currentDir.EndsWith(Constants.curPath[i]))
                {
                    //Om inga processer hittades
                    if (pname.Length == 0)
                    {
                        //Sökvägen till Clientprogrammet
                        String clientPath = Constants.newPath[0];
                        //Tar bort curPath-delen
                        String folderBase = currentDir.Substring(0, currentDir.Length - Constants.curPath[i].Length);
                        String nP = folderBase + clientPath;
                        //Öppnar programmet
                        Process process = new Process();
                        process.StartInfo.FileName = nP;
                        process.Start();
                    }
                }
            }
        }

        public static void EndProcess(String processName)
        {
            Process[] pname = Process.GetProcessesByName(processName);
            {
                foreach (Process p in pname)
                {
                    p.Kill();
                }
            }
        }

        public static void EndClient()
        {
            EndProcess(Constants.ClientFileName);
        }

        public static void EndServer()
        {
            EndProcess(Constants.ServerFileName);
        }
        public static string MyIp()
        {
            return MyIp2();
        }
        public static string MyIp2()
        {
            WebClient wc = new WebClient();
            string url = "http://www.student.itn.liu.se/~andva287/library/myip.php";
            string status = "";
            try
            {
                string myIp = wc.DownloadString(url);
                status = myIp;
            }
            catch (Exception e)
            {
                IPHostEntry iPHost = Dns.GetHostByName(Dns.GetHostName());
                return iPHost.AddressList[0].ToString();
            }

            return status;
        }
    }
}
