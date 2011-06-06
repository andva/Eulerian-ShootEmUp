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
    public class HUD
    {
        public SpriteFont hudFont;
        public SpriteBatch spriteBatch;

        public HUD(SpriteBatch sB, SpriteFont font)
        {
            hudFont = font;
            spriteBatch = sB;
        }

        public void DrawHUD()
        {
            int x1 = Constants.SCRWIDTH - 200;
            int y1 = Constants.SCRHEIGHT - 80;
            uint rounds = Globals.player.rifle.rounds;
            uint rounds_left = Globals.player.rifle.clip * Globals.player.rifle.MAXROUNDS;
            spriteBatch.DrawString(hudFont, rounds.ToString() + " / " + rounds_left.ToString(), new Vector2(x1, y1), new Color(30, 111, 185));

            int x2 = 50;
            int y2 = Constants.SCRHEIGHT - 80;
            spriteBatch.DrawString(hudFont, Globals.player.hp.ToString(), new Vector2(x2, y2), new Color(30, 111, 185));
        }

    }
}
