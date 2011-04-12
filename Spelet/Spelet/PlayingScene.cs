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
        Model modelHampus, modelRasmus, modelPistol, modelRifle, currentWep;
        Level level;

        public PlayingScene(Game game, Texture2D crossHair, Texture2D HUD)
            : base(game)
        {
            Vector2 middle = new Vector2(base.Game.GraphicsDevice.Viewport.Width / 2 - 25,
                                base.Game.GraphicsDevice.Viewport.Height / 2 - 25);
            Components.Add(new ImageComponent(game, crossHair, (int)middle.Y, (int)middle.X));
            Components.Add(new ImageComponent(game, HUD, ImageComponent.DrawMode.Stretch));
            spriteBatch = (SpriteBatch)Game.Services.GetService(
                                typeof(SpriteBatch));
            LoadModels();

        }
        private void LoadModels()
        {
            modelHampus = Globals.hampus;
            modelRasmus = Globals.rasmus;
            modelPistol = Globals.pistol;
            modelRifle = Globals.rifle;
            level = Globals.level;
        }
        private void DrawLevel(Model model, Matrix world, float t)
        {

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                ModelMesh mesh = model.Meshes.ElementAt(i);
                foreach (Effect effect in mesh.Effects)
                {
                    if (i == 0)
                    {
                        effect.CurrentTechnique = effect.Techniques["Bump"];
                        effect.Parameters["N_Texture"].SetValue(level.effectTextures[1]);
                    }
                    if (i == 1)
                    {
                        effect.CurrentTechnique = effect.Techniques["Basic"];
                    }

                    effect.Parameters["Texture"].SetValue(Globals.level.mapTexture[i]);
                    effect.Parameters["Projection"].SetValue(Globals.player.camera.projection);
                    effect.Parameters["View"].SetValue(Globals.player.camera.view);
                    effect.Parameters["lightDir1"].SetValue(new Vector3(0, 0, 5));
                }
                mesh.Draw();
            }
            Globals.levelEffect.Parameters["ViewMatrix"].SetValue(
                        Globals.player.camera.view);
            Globals.levelEffect.Parameters["ProjectionMatrix"].SetValue(
                                    Globals.player.camera.projection);
            foreach (ModelMesh mesh in level.model.Meshes)
            {
                mesh.Draw();
            }
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
            DrawSkySphere();

            base.Game.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);//Rensar djupet i bilden

            base.Draw(gameTime);
        }


    }
}
