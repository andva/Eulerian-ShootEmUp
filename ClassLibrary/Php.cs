using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Lidgren.Network;
using System.Threading;
using System.Web;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;

namespace ClassLibrary
{
    public class Php
    {
        public static Stream dataStream = null;
        public static StreamReader reader = null;
        public static WebResponse response = null;
        public static String responseString = null;
        public static String postData;
        public static String getUri = "http://www.student.itn.liu.se/~andva287/library/phpScripts.php";
        public static String postUri = "http://www.student.itn.liu.se/~andva287/library/newserver.php";
        public static WebRequest request;

        private static string webPost(string uri, string postString)
        {
            request = WebRequest.Create(uri);
            request.Method = "POST";
            postData = postString;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            response = request.GetResponse();
            dataStream = response.GetResponseStream();
            reader = new StreamReader(dataStream);
            responseString = reader.ReadToEnd();
            if (reader != null) reader.Close();
            if (dataStream != null) dataStream.Close();
            if (response != null) response.Close();
            return responseString;
        }

        public static string formatString()
        {
            string postString = "Format=" + Constants.phpServers;
            return webPost(getUri, postString);
        }

        public static ServerList getServers()
        {
            String tableString = Php.formatString();
            if (tableString != "")
            {
                List<string> ip = new List<string>();
                List<Int32> max = new List<Int32>();
                List<Int32> openSlots = new List<Int32>();
                List<Int32> ping = new List<Int32>();
                const string SERVER_VALID_DATA_HEADER = "SERVER_";
                String rows = tableString.Trim().Substring(SERVER_VALID_DATA_HEADER.Length);
                String[] words = Regex.Split(rows, "_ROW_");
                
                foreach (string word in words)
                {
                    String[] cols = Regex.Split(word, "_COL_");
                    if (cols.Length == 4)
                    {
                        String temp = cols[0].Trim();
                        if(temp.Equals(Functions.MyIp()))
                        {
                            ip.Add("localhost");
                        }
                        else
                        {
                            ip.Add(temp);
                        }
                        max.Add(Convert.ToInt32(cols[1].Trim()));
                        openSlots.Add(Convert.ToInt32(cols[2].Trim()));
                        ping.Add(Convert.ToInt32(cols[3].Trim()));

                    }
                }
                ServerList sL = new ServerList(ip, max, openSlots, ping);
                return sL;
            }
            else return null;

        }


        public static void addServer(String ip, int max, int openSlots, int ping)
        {
            string postString = "Format=" + Constants.phpAddServer + 
                                "&ip=" + ip +
                                "&max=" + max + 
                                "&players=" + openSlots +
                                "&ping=" + ping;
            webPost(postUri, postString);
        }
        
    }


}
