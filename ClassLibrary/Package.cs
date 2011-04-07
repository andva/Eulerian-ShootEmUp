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
        public static String DataToString(AbsData d)
        {
            String id = d.GetId().ToString();
            String x = d.GetXposString();
            String y = d.GetYposString();
            String z = d.GetZposString();
            String alive = d.isAliveToString();
            String weapon = d.CurrentWeaponToString();
            String dir = Convert.ToString(d.forwardDir);
            string msg = id + x + y + z + weapon + alive + dir;
            return msg;
        }
        public static void DataToOm(NetOutgoingMessage om, AbsData d)
        {
            om.Write(d.alive);
            om.Write(d.GetXpos());
            om.Write(d.GetYpos());
            om.Write(d.GetZpos());
            om.Write((Int32)d.id);
            om.Write(d.weapon);
            om.Write(d.forwardDir);
        }

        public static void ToOtherPlayers(NetIncomingMessage im, AbsData[] d)
        {
            bool alive = im.ReadBoolean();
            float xp = im.ReadFloat();
            float yp = im.ReadFloat();
            float zp = im.ReadFloat();
            Int32 id = im.ReadInt32();
            Int16 wep = im.ReadInt16();
            float forw = im.ReadFloat();

            if (d[id] == null)
            {
                d[id] = new OtherPlayer(xp, yp, zp, wep, id, forw);
            }

            if (!alive)
            {
                d[id].ChangeLifeStatus(false);
            }
            d[id].ChangePosition(xp, yp, zp);
            d[id].ChangeWeapon(wep);
            d[id].ChangeForwardDir(forw);

        }
        public static Player MsgToPlayer(NetIncomingMessage im, GraphicsDevice device)
        {
            bool alive = im.ReadBoolean();
            float xp = im.ReadFloat();
            float yp = im.ReadFloat();
            float zp = im.ReadFloat();
            Int32 id = im.ReadInt32();
            Int16 wep = im.ReadInt16();
            float dir = im.ReadFloat();
            Player d = new Player(xp, yp, zp, wep, id, device);
            d.forwardDir = d.camera.GetForwardDir();
            return d;
        }
        public static OtherPlayer MsgToOtherPlayer(NetIncomingMessage im)
        {
            bool alive = im.ReadBoolean();
            float xp = im.ReadFloat();
            float yp = im.ReadFloat();
            float zp = im.ReadFloat();
            Int32 id = im.ReadInt32();
            Int16 wep = im.ReadInt16();
            float dir = im.ReadFloat();

            OtherPlayer d = new OtherPlayer(xp, yp, zp, wep, id, dir);
            return d;
        }
        public static Bullet MsgToBullet(NetIncomingMessage im)
        {
            float x = im.ReadFloat();
            float y = im.ReadFloat();
            float z = im.ReadFloat();
            Int32 id = im.ReadInt32();
            Vector3 v = new Vector3(x, y, z);
            Bullet b = new Bullet(v, id);
            return b;
        }

        public static void SendBullet(NetOutgoingMessage bullet, Vector3 dir, Int32 id)
        {
            bullet.Write(Constants.Bullet);
            bullet.Write(dir.X);
            bullet.Write(dir.Y);
            bullet.Write(dir.Z);
            bullet.Write(id);
        }

        public static void SendBullet(NetOutgoingMessage bullet, Bullet b)
        {
            Package.SendBullet(bullet, b.dir, b.id);
        }

        public static void SendBullet(NetOutgoingMessage bullet, float x, float y, Int32 id)
        {
            Vector3 v = new Vector3 (x, y, 0);
            SendBullet(bullet, v, id);
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
