using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary
{
    public class ServerList
    {
        public List<string> ip = new List<string>();
        public List<Int32> max = new List<Int32>();
        public List<Int32> openSlots = new List<Int32>();
        public List<Int32> ping = new List<Int32>();

        public ServerList(List<string> ip, List<Int32> max, List<Int32> openSlots, List<Int32> ping)
        {
            this.ip = ip;
            this.max = max;
            this.openSlots = openSlots;
            this.ping = ping;
        }
        public void getIp()
        {
            foreach (string a in ip)
            {
                Console.WriteLine(a);
            }
        }
        public string getIp(int a)
        {
            return ip[a];
        }
        public String ServerToTable()
        {
            String a = "";
            for (int i = 0; i < ip.Count; i++)
            {
                a = ip[i] + " " + openSlots[i] + "/" + max[i] +
                    " " + ping[i];
            }
            return a;
        }
        public String[] ServerToStrings()
        {
            String[] k = new String[ip.Count];
            for (int i = 0; i < ip.Count; i++)
            {
                k[i] = ip[i] + " " + openSlots[i] + "/" + max[i] +
                    " " + ping[i];
            }
            return k;
        }
        public int CountServers()
        {
            return ip.Count;
        }
    }
}
