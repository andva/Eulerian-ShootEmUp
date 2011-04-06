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
                                    player = Package.MsgToPlayer(im, device);
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
                                case Constants.Bullet:
                                    Bullet b = Package.MsgToBullet(im);
                                    b.CheckHits(player, players);
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
            if (connected && internetConnection)
            {
                player.updatePlayer(timeDifference);
                om = client.CreateMessage();
                om.Write(Constants.PlayerUpdate);
                Package.DataToOm(om, player);
                client.SendMessage(om, NetDeliveryMethod.UnreliableSequenced);
            }
        }

        /// <summary>
        /// Skickar ut nuvarande position till andra spelare
        /// </summary>
        public void GunMsg(Vector2 mPos)
        {
            Vector2 bulletDirection = mPos - player.GetPosition2();
            NetOutgoingMessage bullet = client.CreateMessage();
            Package.SendBullet(bullet, bulletDirection.X, bulletDirection.Y, player.GetId());
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
    }
}
