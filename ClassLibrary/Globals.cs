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
        public static Model rasmus, hampus, axel, valter, rifle, pistol, skysphere;
        public static Effect shadowEffect, skySphereEffect;
        public static Level level;
        public static Player player;
        public static ClipPlayer clipPlayer, hampCP;
        public static AnimationClip rifleClip, hampClip;
        public static SkinningData hampusSkinningData, 
                                    rasmusSkinningData,
                                    axelSkinningData,
                                    valterSkinningData,
                                    rifleSkinningData;
        public static AudioManager audioManager;
        public static Texture2D[] hampusTexture = new Texture2D[2];
        public static OtherPlayer[] players;
        public static Boolean isRunning = false;
        public static SpriteFont font, deathFont;
        public static HUD hud;
        public static MuzzleFlash muzzleflash;
        public static String s;
        public static Effect effect; //Används för rita ut banan mha hightmap och sprites (Tagen från www.riemers.net/index.php)

        public static BloodList blood;
        public static GraphicsDevice device; //används för blod

        public static Texture2D gotHitTexture;
    }
}
