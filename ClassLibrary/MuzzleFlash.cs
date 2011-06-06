using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ClassLibrary
{
    public class MuzzleFlash
    {
        Texture2D[] flash;
        SpriteBatch spriteBatch;
        public bool show;
        int frame;
        int startFrame = 2;

        public MuzzleFlash(SpriteBatch sB, Texture2D[] f)
        {
            flash = f;
            spriteBatch = sB;
            show = false;
            frame = startFrame;
        }

        public void activate()
        {
            show = true;
            frame = startFrame;
        }

        public void update()
        {
            if (show)
            {
                frame++;
                if (frame == flash.Length - 1)
                {
                    frame = startFrame;
                    show = false;
                }
            }
        }

        public void draw()
        {
            if (show)
            {
                Vector2 middle = new Vector2(Constants.SCRWIDTH / 2 - 500, Constants.SCRHEIGHT / 2 - 300);

                spriteBatch.Draw(flash[frame], middle, Color.White);
            }
        }
    }
}
