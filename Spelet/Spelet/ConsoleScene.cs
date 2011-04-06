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
    public class ConsoleScene : GameScene
    {
        SpriteBatch spriteBatch = null;
        SpriteFont smallFont;
        SpriteFont largeFont;
        public ServerList serverList;
        TextMenuComponent servers;
        String[] tot, uptext;

        public ConsoleScene(Game game, SpriteFont smallFont, SpriteFont largeFont,
            Texture2D background)
            : base(game)
        {
            this.largeFont = largeFont;
            this.smallFont = smallFont;
            uptext = new String[] { "Update servers", "Back" };
            servers = new TextMenuComponent(game, smallFont, largeFont);
            UpdateServerList();
            //Components.Add(new ImageComponent(game, background,
             //                               ImageComponent.DrawMode.Center));
            
            Components.Add(servers);

            spriteBatch = (SpriteBatch)Game.Services.GetService(
                                typeof(SpriteBatch));
        }
        public override void Show()
        {
            UpdateServerList();
            servers.Position = new Vector2(10, 5);
            servers.Visible = true;
            servers.Enabled = true;
            base.Show();
        }
        public override void Hide()
        {
            servers.Visible = false;
            servers.Enabled = false;
            base.Hide();
        }
        public override void Draw(GameTime gameTime)
        {           
            base.Draw(gameTime);
        }
        public void UpdateServerList()
        {
            serverList = Php.getServers();
            String[] s = serverList.ServerToStrings();
            tot = Union(s, uptext);
            servers.SetMenuItems(tot);
        }
        public int SelectedMenuIndex
        {
            get { return servers.SelectedIndex; }
        }
        private String[] Union(String[] a1, String[] a2)
        {
            String[] tot = new String[a1.Length + a2.Length];
            a1.CopyTo(tot, 0);
            a2.CopyTo(tot, a1.Length);
            return tot;
        }
    }
}
