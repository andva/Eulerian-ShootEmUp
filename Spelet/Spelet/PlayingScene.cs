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
using SkinnedModel;

namespace ClassLibrary
{
    public class PlayingScene : GameScene
    {
        SpriteBatch spriteBatch = null;
        Texture2D crossHair;
        Model modelHampus, modelRasmus, modelRifle;
        Level level;
        Vector3 riflePos = new Vector3(0.3f, 0.1f, 2f);
        Vector3 gunPos;
        float time;
        Vector2 middle;
        Game g;
        Matrix shadow;
        Vector3 lightDir1 = new Vector3(1, 1, 1);
        OtherPlayer[] a= new OtherPlayer[1];
        private float deathCount;
        private float hitCount;
        private float alpha = 0;
        
        public PlayingScene(Game game, Texture2D cross, Texture2D HUD)
            : base(game)
        {
            crossHair = cross;
            middle = new Vector2(base.Game.GraphicsDevice.Viewport.Width / 2 - 25,
                                base.Game.GraphicsDevice.Viewport.Height / 2 - 25);
            //Components.Add(new ImageComponent(game, HUD, ImageComponent.DrawMode.Stretch));
            spriteBatch = (SpriteBatch)Game.Services.GetService(
                                typeof(SpriteBatch));
            DebugShapeRenderer.Initialize(base.Game.GraphicsDevice);
            LoadModels();
            g = game;
            hitCount = 0;
            deathCount = 0;
        }

        /// <summary>
        /// Show the start scene
        /// </summary>
        public override void Show()
        {
            base.Show();
            gunPos = riflePos;
            Globals.clipPlayer.play(Globals.rifleClip, 100, 100, false);
            g.IsMouseVisible = false;
            Globals.isRunning = true;
        }
        public override void Hide()
        {
            g.IsMouseVisible = true;
            base.Hide();
            Globals.isRunning = false;
        }
        private void LoadModels()
        {
            modelHampus = Globals.hampus;
            modelRasmus = Globals.rasmus;
            modelRifle = Globals.rifle;
            level = Globals.level;
            shadow = Matrix.CreateShadow(lightDir1,
                new Plane(0, 1, 0, -1));
            //a[0] = new OtherPlayer(100, 2, 70, 0, 1, 0, 0);
            //Globals.players[0] = a[0];
            
        }
        public override void Update(GameTime gameTime)
        {
            
            HandleInput(gameTime);
            if (!Globals.player.alive)
            {
                if (deathCount != 5)
                {
                    deathCount += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
                }
                if (deathCount >= 5)
                {
                    resetPlayer();
                }
                Globals.s = "You got killed by player " + Globals.player.killer.ToString() + ", Respawning in " + (5 - (int)deathCount) + "!";
            }
            if (Globals.player.killingspree)
            {
                if(hitCount != 3)
                    hitCount += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
                if (hitCount >= 5)
                {
                    Globals.player.killingspree = false;
                    hitCount = 0;
                }
                Globals.s = "You got killed a enemy!";
            }
                
            alpha -= 0.01f;
            if (alpha < 0) alpha = 0;
            if (Globals.player.hitMe)
            {
                alpha += 0.2f;
                if (alpha > 1f) alpha = 1f;
            }

            base.Update(gameTime);
        }
        private void resetPlayer()
        {
            deathCount = 0;
            Globals.player.resetPlayer();
            Globals.player.rifle.resetGun();
            Random random = new Random();
            int x = random.Next(0, level.width -1);
            int z = random.Next(0, level.length - 1);
            float y = level.heightData[x, z];
            Globals.player.position = new Vector3(x, y, z);
            //Globals.player.position = new Vector3(50, 100, 50);
            Globals.player.killer = -1;
        }
        private void HandleInput(GameTime gameTime)
        {
            Globals.clipPlayer.update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            Globals.muzzleflash.update(); //Uppdatera muzzleflash
            Globals.blood.update(timeDifference); //Tar bort gamla bloodsplatter
            Globals.player.updatePlayer(timeDifference);
            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            foreach (OtherPlayer op in Globals.players)
            {
                if (op != null)
                {
                    if (keyState.IsKeyDown(Keys.Q))
                    {
                        op.alive = true;
                    }

                    op.UpdateAnimations(gameTime);
                }
            }

        }

        public override void Draw(GameTime gameTime)
        {
            String s = "";
            time = (float)((gameTime.TotalGameTime.TotalMilliseconds / 2000) % 2 * Math.PI);
            base.Game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawSkySphere();
            base.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default; //Polygon culling

            level.DrawLevel();

            s = DrawPlayers(s);

            Globals.blood.draw(); //rita ut blodsplatter

            base.Game.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);//Rensar djupet i bilden
            DebugShapeRenderer.Draw(gameTime, Globals.player.camera.view, Globals.player.camera.projection);
            if(!Globals.player.alive)
                spriteBatch.DrawString(Globals.deathFont, Globals.s, new Vector2(Constants.SCRWIDTH / 2 - 320, Constants.SCRHEIGHT / 3), Color.White);

            spriteBatch.End();
            spriteBatch.Begin();

            Globals.muzzleflash.draw();
            
            spriteBatch.End();
            spriteBatch.Begin();

            base.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default; //Polygon culling
            if (Globals.player.alive)
            {
                Globals.player.rifle.DrawGun(time);
                Globals.hud.DrawHUD();
                spriteBatch.Draw(crossHair, middle, Color.Cyan);
            }
            spriteBatch.Draw(Globals.gotHitTexture, new Rectangle(0, 0, Constants.SCRWIDTH, Constants.SCRHEIGHT), Color.White * alpha);
            Globals.player.hitMe = false;

            base.Draw(gameTime);  
        }
        private string DrawPlayers(string s)
        {
            if (Globals.players != null)
            {
                foreach (OtherPlayer op in Globals.players)
                {
                    if (op != null)
                    {
                        //DebugShapeRenderer.AddBoundingSphere(op.boundingSphere, Color.Red); //
                        op.Draw();
                    }
                } 
            }

            return s;
        }
        private void DrawSkySphere()
        {
            Globals.skySphereEffect.Parameters["ViewMatrix"].SetValue(
            Globals.player.camera.view);
            Globals.skySphereEffect.Parameters["ProjectionMatrix"].SetValue(
                                    Globals.player.camera.projection);
            foreach (ModelMesh mesh in Globals.skysphere.Meshes)
            {
                mesh.Draw();
            }
        }
    }
}
