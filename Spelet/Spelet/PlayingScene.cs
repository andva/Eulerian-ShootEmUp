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
    public class PlayingScene : GameScene
    {
        SpriteBatch spriteBatch = null;
        Texture2D crossHair, HUD;

        public PlayingScene(Game game, Texture2D crossHair, Texture2D HUD)
            : base(game)
        {
            
            spriteBatch = (SpriteBatch)Game.Services.GetService(
                                typeof(SpriteBatch));
        }

        /// <summary>
        /// Show the start scene
        /// </summary>
        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }

        public override void Draw(GameTime gameTime)
        {
            
            base.Game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            base.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default; //Polygon culling
            float t = (float)gameTime.TotalGameTime.Seconds;


            base.Game.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);//Rensar djupet i bilden
            Vector2 middle = new Vector2(base.Game.GraphicsDevice.Viewport.Width / 2 - 25,
                                         base.Game.GraphicsDevice.Viewport.Height / 2 - 25);

            spriteBatch.Draw(crossHair, middle, Color.Cyan);
            spriteBatch.Draw(HUD, new Rectangle(0, 0, Constants.SCRWIDTH, Constants.SCRHEIGHT), Color.White);
            base.Draw(gameTime);
        }


    }
}
