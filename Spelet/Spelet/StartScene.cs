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
    public class StartScene : GameScene
    {
        SpriteBatch spriteBatch = null;
        /*SpriteFont smallFont;
        SpriteFont largeFont;*/
        TextMenuComponent menu;
        String[] items = { "Host game", "Connect to game", "Single player", "Exit" };


        public StartScene(Game game, SpriteFont smallFont, SpriteFont largeFont,
            Texture2D background)
            : base(game)
        {
            Components.Add(new ImageComponent(game, background,
                                            ImageComponent.DrawMode.Center));
            menu = new TextMenuComponent(game, smallFont, largeFont);
            menu.SetMenuItems(items);
            Components.Add(menu);
            spriteBatch = (SpriteBatch)Game.Services.GetService(
                                typeof(SpriteBatch));
        }

        /// <summary>
        /// Show the start scene
        /// </summary>
        public override void Show()
        {

            // Put the menu centered in screen
            menu.Position = new Vector2((Game.Window.ClientBounds.Width -
                              menu.Width) / 2, 100);
            //menu.Position = new Vector2(menu.Width / 5, Game.Window.ClientBounds.Height - (menu.Height + menu.Width / 5));
            menu.Visible = true;
            menu.Enabled = true;
            base.Show();
        }

        public override void Hide()
        {
            menu.Visible = false;
            menu.Enabled = false;
            base.Hide();
        }

        public int SelectedMenuIndex
        {
            get { return menu.SelectedIndex; }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }


    }
}
