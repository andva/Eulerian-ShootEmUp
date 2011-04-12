using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SkinnedModel;


namespace ClassLibrary
{
    public struct Level
    {
        public Model model;
        public Texture2D[] mapTexture;
        public Texture2D[] effectTextures;
    };
    public class Globals
    {
        public static Model rasmus, hampus, rifle, pistol, skysphere;
        public static Effect levelEffect, shadowEffect, skySphereEffect;
        public static Level level;
        public static Player player;
        public static ClipPlayer clipPlayer;
    }
}
