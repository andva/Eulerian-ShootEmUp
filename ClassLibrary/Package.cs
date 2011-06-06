using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassLibrary
{
    public static class Package
    {
        public static void PlayerToOm(NetOutgoingMessage om, AbsData d)
        {
            om.Write(d.alive);
            om.Write(d.GetXpos());
            om.Write(d.GetYpos());
            om.Write(d.GetZpos());
            om.Write((Int32)d.id);
            om.Write(d.xr);
            om.Write(d.yr);
            om.Write(d.model);
        }

        public static void ToOtherPlayers(NetIncomingMessage im, OtherPlayer[] d)
        {
            bool alive = im.ReadBoolean();
            float xp = im.ReadFloat();
            float yp = im.ReadFloat();
            float zp = im.ReadFloat();
            Int32 id = im.ReadInt32();
            float xr = im.ReadFloat();
            float yr = im.ReadFloat();
            Int16 model = im.ReadInt16();
            if (d[id] == null)
            {
                d[id] = new OtherPlayer(xp, yp, zp, id, model, xr, yr);
            }
            d[id].model = model;
            d[id].xr = xr;
            d[id].yr = yr;
            
            if (!alive)
            {
                d[id].ChangeLifeStatus(false);
            }
            d[id].position = new Vector3(xp, yp, zp);
            d[id].boundingSphere.Center = new Vector3(d[id].position.X, d[id].position.Y + Constants.HEADMAX/2, d[id].position.Z);
        }
        
        public static Player MsgToPlayer(NetIncomingMessage im, GraphicsDevice device)
        {

            bool alive = im.ReadBoolean();
            float xp = im.ReadFloat();
            float yp = im.ReadFloat();
            float zp = im.ReadFloat();
            Int32 id = im.ReadInt32();
            float xr = im.ReadFloat();
            xr = im.ReadFloat();
            Int32 mo = im.ReadInt32();
            Player d = new Player(xp, yp, zp, id, mo, device);
            Globals.player = d;
            return d;
        }
        public static OtherPlayer MsgToOtherPlayers(NetIncomingMessage im)
        {
            bool alive = im.ReadBoolean();
            float xp = im.ReadFloat();
            float yp = im.ReadFloat();
            float zp = im.ReadFloat();
            Int32 id = im.ReadInt32();
            float xr = im.ReadFloat();
            float yr = im.ReadFloat();
            Int16 model = im.ReadInt16();
            OtherPlayer d = new OtherPlayer(xp, yp, zp, id, xr, yr, false);
            d.model = model;
            return d;
        }

        public static void AnnouncePlayerLeft(NetOutgoingMessage om, Int32 id)
        {
            om.Write(Constants.ClientDisconnect);
            om.Write(id);
        }

        public static void PlayerLeft(NetIncomingMessage im, AbsData[] players)
        {
            Int32 id = im.ReadInt32();
            players[id] = null;
        }

        public static float StringToFloat(String s)
        {
            float a = (float)Convert.ToDouble(s);
            return a;
        }
    }
}
