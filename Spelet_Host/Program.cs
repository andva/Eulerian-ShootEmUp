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
                        else if (type == Constants.HitSomeone)
                        {
                            SendHit(msg);
                        }
                        else if (type == Constants.Status)
                        {
                            UpdateStatus(msg);
                        }
                        else if (type == Constants.RewardKiller)
                        {

                        }
                        break;
                }
                server.Recycle(msg);
            }
        }
        private void rewardK(NetIncomingMessage msg)
        {
            List<NetConnection> all = server.Connections; // Listar alla spelare
            all.Remove(msg.SenderConnection);
            int id = msg.ReadInt32();
            Console.WriteLine("Player " + id + " killed someone!");
            NetOutgoingMessage om = server.CreateMessage();
            om.Write(Constants.RewardKiller);
            om.Write(id);
            server.SendMessage(om, all, NetDeliveryMethod.Unreliable, 0);
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
            OtherPlayer player = Package.MsgToOtherPlayers(msg);
            List<NetConnection> all = server.Connections; // Listar alla spelare
            all.Remove(msg.SenderConnection);
            NetOutgoingMessage om = server.CreateMessage();
            om.Write(Constants.PlayerUpdate);
            Package.PlayerToOm(om, player);
            server.SendMessage(om, all, NetDeliveryMethod.UnreliableSequenced, 0);
        }
        static void SendHit(NetIncomingMessage msg)
        {
            int a = msg.ReadInt32();
            int hitter = msg.ReadInt32();
            List<NetConnection> all = server.Connections; // Listar alla spelare
            all.Remove(msg.SenderConnection);
            NetOutgoingMessage om = server.CreateMessage();
            om.Write(Constants.HitSomeone);
            om.Write(a);
            om.Write(hitter);
            server.SendMessage(om, all, NetDeliveryMethod.Unreliable,0);
        }
        static void UpdateStatus(NetIncomingMessage msg)
        {
            Int32 id = msg.ReadInt32();
            Int16 status = msg.ReadInt16();
            List<NetConnection> all = server.Connections; // Listar alla spelare
            all.Remove(msg.SenderConnection);
            NetOutgoingMessage om = server.CreateMessage();
            om.Write(Constants.Status);
            om.Write(id);
            om.Write(status);
            server.SendMessage(om, all, NetDeliveryMethod.UnreliableSequenced, 0);
        }
        public static int FindOpenSlot(Boolean[] openSlots)
        {
            int s = -1;
            for (int i = 1; i < Constants.MAXPLAYERS; i++)
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
            for (int i = 1; i < Constants.MAXPLAYERS; i++)
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
            Vector3 initialPosition = new Vector3(100, 70, 100);
            Int16 model = (Int16)random.Next(0, 4);
            //model = 2;
            int a = FindOpenSlot(openSlots);
            if (a >= 0)
            {
                openSlots[a] = false;
            }
            OtherPlayer player = new OtherPlayer(initialPosition.X, initialPosition.Y, initialPosition.Z, a, 0,0, false);
            NetOutgoingMessage om = server.CreateMessage();
            Console.WriteLine("Random val: " + model.ToString());
            player.model = model;
            om.Write(Constants.NewConnection);
            Package.PlayerToOm(om, player);
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


