using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ClassLibrary
{
    public static class Constants
    {
        public const int PORT               = 14242;
        public const int MAXPLAYERS         = 10;
        public const string phpServers = "SERVERLIST";
        public const string phpAddServer = "NEWSERVER";

        public const int SCRWIDTH = 800;
        public const int SCRHEIGHT = 600;

        public const int HAMPUS = 0;
        public const int RASMUS = 1;

        public const Int32 NewConnection    = 1;
        public const Int32 ClientDisconnect = 2;
        public const Int32 PlayerUpdate     = 3;
        public const Int32 Bullet           = 4;
        public const Int32 Status           = 9;

        public const Int32 GUNPISTOL = 0;
        public const Int32 GUNMACHINE = 1;

        public const float BOLLRADIE          = 64;
        public const float NearClip = 1.0f;
        public const float FarClip = 1000.0f;
        public const float ViewAngle = 45.0f;

        public const float Velocity = 0.75f;
        public const float TurnSpeed = 0.025f;
        public const int MaxRange = 98;

        public const String ServerFileName = "Spelet_Host";
        public const String ClientFileName = "Spelet";

        private const String cP1 = "\\"+ ServerFileName + "\\bin\\Debug";
        private const String cP2 = "\\"+ ClientFileName + "\\"+ ClientFileName + "\\bin\\x86\\Debug\\";
        private const String cP3 = "\\" + ClientFileName + "\\" + ClientFileName + "\\bin\\x86\\Debug";

        private const String nP1 = "\\"+ ServerFileName +"\\bin\\Debug\\"+ ServerFileName +".exe";
        
        public static String[] curPath = new String[3] {cP1, cP2, cP3 };
        public static String[] newPath = new String[1] { nP1 };
    }
}
