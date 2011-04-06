using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Diagnostics;
using System.Threading;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using ClassLibrary;

namespace Spelet_Host
{
    public class Program
    {
        private static NetServer server;
        private static Boolean[] openSlots;
        private static OtherPlayer[] players = new OtherPlayer[Constants.MAXPLAYERS];
        private static String command;
        private static String ip;
        private static ServerList sL;
        public static IPHostEntry IPHost;
        private static UpdatingDatabase uD;
        private static Thread commandThread, updateDatabaseT;
        public static CommandReader cR;
        private static Double nextSendUpdates;
        private static NetIncomingMessage msg;

        public static void Main(String[] args)
        {
            SetUpServer();
            SetUpThreads();
            SayHello();
            while (!Console.KeyAvailable || Console.ReadKey().Key != ConsoleKey.Escape)
            {
                HandleMsgs();
            }

        }

        static void HandleMsgs()
        {
            while ((msg = server.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryRequest:
                        // Server received a discovery request from a client;
                        server.SendDiscoveryResponse(null, msg.SenderEndpoint);
                        Console.WriteLine("Discovery request!");
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        StatusChanged(msg);
                        break;
                    case NetIncomingMessageType.Data:

                        // broadcast this to all connections, except sender
                        var type = msg.ReadTransferType();
                        if (type == Constants.PlayerUpdate)
                        {
                            Brodcast(msg);
                        }
                        else if (type == Constants.ClientDisconnect)
                        {
                            PlayerLeft(msg);
                        }
                        break;
                }
                server.Recycle(msg);
            }
        }

        static void SayHello()
        {
            Console.WriteLine("Welcome to the gameserver!");
            Console.WriteLine(CountConnected() + "/" + Constants.MAXPLAYERS + " slots available");
            Console.WriteLine(ip + ":14242");
            Console.WriteLine("Server is online! \nWrite all_commands for a list of commands \n");
        }

        static void SetUpThreads()
        {
            cR = new CommandReader();
            commandThread = new Thread(cR.getCommands);
            commandThread.Start();

            uD = new UpdatingDatabase();
            uD.openSlots = CountConnected();
            updateDatabaseT = new Thread(uD.UpdateDatabase);
            updateDatabaseT.Start();
        }

        static void SetUpServer()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("awesomegame");
            nextSendUpdates = NetTime.Now;
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.Port = Constants.PORT;
            config.MaximumConnections = Constants.MAXPLAYERS;
            openSlots = new Boolean[Constants.MAXPLAYERS];
            server = new NetServer(config);
            server.Start();

            for (int i = 0; i < Constants.MAXPLAYERS; i++)
            {
                openSlots[i] = true;
            }

            ip = "IP: " + Functions.MyIp();
        }

        static void Brodcast(NetIncomingMessage msg)
        {
            OtherPlayer player = Package.MsgToOtherPlayer(msg);
            List<NetConnection> all = server.Connections; // Listar alla spelare
            all.Remove(msg.SenderConnection);
            NetOutgoingMessage om = server.CreateMessage();
            om.Write(Constants.PlayerUpdate);
            Package.DataToOm(om, player);
            server.SendMessage(om, all, NetDeliveryMethod.UnreliableSequenced, 0);
        }

        static void BulletMsgs(NetIncomingMessage msg)
        {
            List<NetConnection> all = server.Connections; // Get other players
            all.Remove(msg.SenderConnection);

            Bullet b = Package.MsgToBullet(msg);
            NetOutgoingMessage om = server.CreateMessage();
            om.Write(Constants.Bullet);
            Package.SendBullet(om, b);
            server.SendMessage(om, all, NetDeliveryMethod.UnreliableSequenced, 0);
        }

        public static int FindOpenSlot(Boolean[] openSlots)
        {
            int s = -1;
            for (int i = 0; i < Constants.MAXPLAYERS; i++)
            {
                if (openSlots[i])
                {
                    s = i;
                    break;
                }
            }
            return s;
        }

        private static int CountConnected()
        {
            int s = 0;
            for (int i = 0; i < Constants.MAXPLAYERS; i++)
            {
                if (!openSlots[i])
                {
                    s++;
                }
            }
            return s;
        }

        static void DiscoveryRequest(NetIncomingMessage msg)
        {
            server.SendDiscoveryResponse(null, msg.SenderEndpoint);
            Console.WriteLine("Sending response!");
        }

        static void StatusChanged(NetIncomingMessage msg)
        {
            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
            if (status == NetConnectionStatus.Connected)
            {
                SendInitialData(msg.SenderConnection);
                Console.WriteLine("Player Connected!");
            }
        }

        static void PlayerLeft(NetIncomingMessage im)
        {
            Int32 id = im.ReadInt32();
            List<NetConnection> all = server.Connections; // get copy
            all.Remove(im.SenderConnection);
            NetOutgoingMessage leaveMsg = server.CreateMessage();
            Package.AnnouncePlayerLeft(leaveMsg, id);
            server.SendMessage(leaveMsg, all, NetDeliveryMethod.UnreliableSequenced, 1);
            openSlots[id] = true;
            players[id] = null;
            Console.WriteLine("Player {0} disconnected", id);
        }

        static void SendInitialData(NetConnection receiver)
        {
            Console.WriteLine("Sending initial data");
            Random random = new Random();
            float xr = random.Next(-500, 500);
            float zr = random.Next(-500, 500);
            Vector3 initialPosition = new Vector3(xr, 0, zr);

            int a = FindOpenSlot(openSlots);
            if (a >= 0)
            {
                openSlots[a] = false;
            }
            OtherPlayer player = new OtherPlayer(initialPosition.X, initialPosition.Y, initialPosition.Z, Constants.GUNMACHINE, a, 4);
            NetOutgoingMessage om = server.CreateMessage();

            om.Write(Constants.NewConnection);
            Package.DataToOm(om, player);
            Console.WriteLine(Package.DataToString(player));
            server.SendMessage(om, receiver, NetDeliveryMethod.Unreliable);
            Console.WriteLine(String.Format("Player {0} connected", player.IdToString()));
            players[a] = player;
        }

        public static void ReadCommands()
        {
            String[] all_Commands = new String[4];
            all_Commands[0] = "all_commands     : Writes a list of all commands";
            all_Commands[1] = "start_client     : Start a client that connects to the server";
            all_Commands[2] = "kill_client      : Ending all open clients";
            all_Commands[3] = "other            : List of all avaiable servers";


            command = Console.ReadLine();
            if (command.Equals("start_client"))
            {
                System.Console.WriteLine("Starting client!");
                Functions.RunClientFromServer();
            }
            else if (command.Equals("kill_client"))
            {
                System.Console.WriteLine("Ending client-process!");
                Functions.EndClient();
            }

            else if (command.Equals("all_commands"))
            {
                Console.WriteLine("\n");
                for (int i = 0; i < all_Commands.Length; i++)
                {
                    Console.WriteLine(all_Commands[i]);
                }
            }

            else if (command.Equals("other"))
            {
                sL = Php.getServers();
                sL.getIp();
            }

            else
            {
                System.Console.WriteLine("Unknown command! \n Write all_commands to see commands");
            }

        }
    }

    public class CommandReader
    {
        public Boolean stop = false;

        public void getCommands()
        {
            while (!stop)
            {
                Program.ReadCommands();
                Thread.Sleep(100);
            }
        }
        public void Sleep()
        {
            stop = true;
        }
        public void Awake()
        {
            stop = false;
            getCommands();
        }
    }

    public class UpdatingDatabase
    {
        public Boolean stop = false;
        public int openSlots;

        public void UpdateDatabase()
        {
            while (!stop)
            {
                Console.WriteLine("Updating serverlist!");
                Program.cR.Sleep();
                Php.addServer(Functions.MyIp(), Constants.MAXPLAYERS, openSlots, 0);
                Program.cR.Awake();
                Thread.Sleep(30000); //Sov i 30 sec
            }
        }
    }
}


