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

namespace ClassLibrary
{
    public class NWClient
    {
        double nextSendUpdates = NetTime.Now;
        NetClient client;
        NetOutgoingMessage om;
        NetIncomingMessage im;
        NetPeerConfiguration config;
        public Player player;
        public OtherPlayer[] players;
        public Boolean connected = false;
        GraphicsDevice device;
        bool internetConnection = false;
        Int16 oldStatus = 1;
        Int32 oldKiller = -1;


        /// <summary>
        /// Skapar en client och försöker ansluta till lokal
        /// server. Lägg i init!
        /// </summary>
        public NWClient(GraphicsDevice device)
        {
            SetupClientNw(device);
            this.device = device;
        }
        /// <summary>
        /// Skapar en client och försöker ansluta till server
        /// med angiven ip-adress. Försöker tills ansluten
        /// </summary>
        public NWClient(string ip, GraphicsDevice device)
        {
            SetupClientNw(device);
            
            while (!connected)
            {
                TryConnectIp(ip);
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// Configurerar clienten
        /// </summary>
        private void SetupClientNw(GraphicsDevice device)
        {
            InternetConnected();
            //Skapa nätverksinställningar och starta clienten
            if (internetConnection)
            {
                config = new NetPeerConfiguration("awesomegame");
                config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
                client = new NetClient(config);
                client.Start();
            }

            player = new Player(new Vector3(0, 0, 0), device);
            Globals.player = player;
            Globals.players = new OtherPlayer[Constants.MAXPLAYERS];

            Globals.audioManager.attachCameraToAudio(player.camera);
        }
        /// <summary>
        /// Försöker ansluta till lokal server
        /// </summary>
        public void TryConnectLocal()
        {
            client.DiscoverKnownPeer("localhost", Constants.PORT);
            GetMsgs();
        }
        /// <summary>
        /// Försöker ansluta till ip
        /// </summary>
        public void TryConnectIp(String iP)
        {
            if (internetConnection)
            {
                client.DiscoverKnownPeer(iP, Constants.PORT);
                //client.DiscoverKnownPeer(iP, Constants.PORT);
                GetMsgs();
            }
            
        }

        /// <summary>
        /// Hämtar alla nya meddelanden, läggs i
        /// update-funktionen!
        /// </summary>
        public void GetMsgs()
        {
            if (internetConnection)
            {
                while ((im = client.ReadMessage()) != null)
                {
                    switch (im.MessageType)
                    {
                        case NetIncomingMessageType.DiscoveryResponse:
                            //Ansluter om server hittas
                            client.Connect(im.SenderEndpoint);
                            break;
                        case NetIncomingMessageType.Data:
                            //Undersöker vilken sorts meddelande som mottas
                            var type = im.ReadTransferType();
                            switch (type)
                            {
                                //Ny anslutning
                                case Constants.NewConnection:
                                    players = new OtherPlayer[Constants.MAXPLAYERS];
                                    Globals.players = players;
                                    player = Package.MsgToPlayer(im, device);
                                    Console.WriteLine("Constants.NewConnection:" + player.model);
                                    Globals.player = player;
                                    connected = true;
                                    break;

                                //Ngn annan disconnectade
                                case Constants.ClientDisconnect:
                                    Package.PlayerLeft(im, players);
                                    break;

                                //Spelaruppdatering
                                case Constants.PlayerUpdate:
                                    Package.ToOtherPlayers(im, players);
                                    break;

                                //Någon har skjutit
                                case Constants.HitSomeone:
                                    Int32 k = im.ReadInt32();
                                    Int32 shooter = im.ReadInt32();
                                    if (k == Globals.player.id)
                                    {
                                        Globals.player.GotHit(10, shooter);
                                    }
                                    break;

                                case Constants.Status:
                                    Int32 iii = im.ReadInt32();
                                    Int16 st = im.ReadInt16();
                                    if(Globals.players[iii] != null)
                                        Globals.players[iii].activity = st;
                                    break;
                                case Constants.RewardKiller:
                                    /*Int32 jj = im.ReadInt32();
                                    if (Globals.player.id == jj)
                                        Globals.player.killingspree = true;*/
                                    break;
                            }
                            break;
                    }
                    client.Recycle(im);
                }
            }
        }

        /// <summary>
        /// Skickar ut nuvarande position till andra spelare,
        /// lägg i update
        /// </summary>
        public void UpdatePos(float timeDifference)
        {
            if (Globals.players != null)
            {
                foreach (OtherPlayer op in Globals.players)
                {
                    if (op != null)
                    {
                        if (connected && internetConnection)
                        {
                            if (op.hit)
                            {
                                om = client.CreateMessage();
                                om.Write(Constants.HitSomeone);
                                om.Write(op.id);
                                om.Write(Globals.player.id);
                                Console.WriteLine(Globals.player.id);
                                client.SendMessage(om, NetDeliveryMethod.UnreliableSequenced);
                                op.hit = false;
                            }
                            if (Globals.player.activity != oldStatus)
                            {
                                om = client.CreateMessage();
                                om.Write(Constants.Status);
                                om.Write(Globals.player.id);
                                om.Write(Globals.player.activity);
                                client.SendMessage(om, NetDeliveryMethod.UnreliableSequenced);
                                oldStatus = Globals.player.activity;
                            }
                            /*if (Globals.player.killer != -1 && Globals.player.killer != oldKiller)
                            {
                                om = client.CreateMessage();
                                om.Write(Constants.RewardKiller);
                                om.Write(Globals.player.killer);
                                client.SendMessage(om, NetDeliveryMethod.Unreliable);
                                oldKiller = Globals.player.killer;
                            }*/
                            
                        }
                    }
                }
                if (connected && internetConnection)
                {
                    om = client.CreateMessage();
                    om.Write(Constants.PlayerUpdate);
                    Package.PlayerToOm(om, Globals.player);
                    client.SendMessage(om, NetDeliveryMethod.UnreliableSequenced);
                }
            }
            else
            {
                //Console.WriteLine("players = null!");
                if (Globals.players == null)
                {
                    Console.WriteLine("G.players = null!");
                }
            }
        }

        /// <summary>
        /// Skickar ut nuvarande position till andra spelare
        /// </summary>
        public void GunMsg(Vector2 mPos)
        {
            NetOutgoingMessage bullet = client.CreateMessage();

            client.SendMessage(bullet, NetDeliveryMethod.Unreliable);
        }

        /// <summary>
        /// Berättar vem som lämnar spelet
        /// </summary>
        public void LeaveMsg()
        {
            if (internetConnection)
            {
                NetOutgoingMessage om = client.CreateMessage();
                om.Write(Constants.ClientDisconnect);
                Int32 id = player.id;
                om.Write(id);
                client.SendMessage(om, NetDeliveryMethod.ReliableOrdered);
            }

        }
        public void InternetConnected()
        {
            try
            {
                System.Net.IPHostEntry host = System.Net.Dns.GetHostByName("www.google.com");
                internetConnection = true;
            }
            catch 
            {
                internetConnection = false; // host not reachable.
            }
        }
        public OtherPlayer who(int id)
        {
            return players[id];
        }
    }
}
