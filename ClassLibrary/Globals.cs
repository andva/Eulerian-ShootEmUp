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
    public class Globals
    {
        public static Model rasmus, hampus, rifle, pistol, skysphere;
        public static Effect shadowEffect, skySphereEffect;
        public static Level level;
        public static Player player;
        public static ClipPlayer clipPlayer;
        public static AnimationClip rifleClip, pistolClip;
        public static SkinningData pistolSkinningData, rifleSkinningData;
        public static AudioManager audioManager;
    }
}
