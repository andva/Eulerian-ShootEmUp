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
        Vector3 riflePos = new Vector3(0.3f, 0.1f, 2f);
        Vector3 pisolPos = new Vector3(0.3f, -1.0f, 1.5f);
        Vector3 gunPos;
        float time;
        Vector2 middle;

        public PlayingScene(Game game, Texture2D cross, Texture2D HUD)
            : base(game)
        {
            crossHair = cross;
            middle = new Vector2(base.Game.GraphicsDevice.Viewport.Width / 2 - 25,
                                base.Game.GraphicsDevice.Viewport.Height / 2 - 25);
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
        private void DrawLevel()
        {
            Model model = level.model;
            Matrix world = Matrix.Identity;
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                ModelMesh mesh = model.Meshes.ElementAt(i);
                foreach (Effect effect in mesh.Effects)
                {
                    if (i == 0)
                    {
                        effect.CurrentTechnique = effect.Techniques["Bump"];
                        effect.Parameters["N_Texture"].SetValue(level.effectTextures[0]);
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
        private void DrawGun()
        {
            KeyboardState keyState = Keyboard.GetState();
            Matrix world = Matrix.Identity;
            int amplitude = 0;
            if (keyState.IsKeyDown(Keys.LeftShift))
            {
                amplitude = 7;
            }
            else amplitude = 2;

            float sinDisp = amplitude * (float)Math.Sin(time * amplitude) / 30;//sinus displacement
            float cosDisp = amplitude * (float)Math.Cos(time * amplitude) / 30;//cossinus displacement

            world = world * Matrix.CreateTranslation(gunPos) *
            Matrix.CreateTranslation(new Vector3(0.15f + cosDisp / 2, -12.2f - Math.Abs(sinDisp), -20 + cosDisp / 3)) *
            Matrix.CreateTranslation(new Vector3(0f, 0f, Globals.player.rifle.zRecoil)) *
            Matrix.CreateScale(0.5f, 0.5f, 0.5f) *
            Matrix.CreateRotationY(MathHelper.Pi) / 2 *
            Globals.player.camera.rotation * Matrix.CreateTranslation(Globals.player.camera.position + 10 * Globals.player.camera.rotatedTarget);
            Matrix[] bones = Globals.clipPlayer.GetSkinTransforms();
            foreach (ModelMesh mesh in currentWep.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.World = world;

                    effect.View = Globals.player.camera.view;
                    effect.Projection = Globals.player.camera.projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }
        }
        /// <summary>
        /// Show the start scene
        /// </summary>
        public override void Show()
        {
            base.Show();
            currentWep = modelRifle;
            gunPos = riflePos;
        }
        public override void Hide()
        {
            base.Hide();
        }
        public override void Draw(GameTime gameTime)
        {
            time = (float)((gameTime.TotalGameTime.TotalMilliseconds / 2000) % 2 * Math.PI);
            base.Game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            //base.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default; //Polygon culling
            DrawSkySphere();
            base.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default; //Polygon culling
            DrawLevel();
            base.Game.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);//Rensar djupet i bilden
            DrawGun();
            spriteBatch.Draw(crossHair, middle, Color.Cyan);
            base.Draw(gameTime);
        }

        private void DrawOtherPlayer(OtherPlayer otherPlayer, Matrix world)
        {
            Model model = Globals.hampus;
            if (otherPlayer.model == Constants.HAMPUS)
            {
                model = Globals.hampus;
            }
            else if (otherPlayer.model == Constants.RASMUS)
            {
                model = Globals.rasmus;
            }
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = Globals.shadowEffect;
                }
            }
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            Matrix[] bones = Globals.clipPlayer.GetSkinTransforms();
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques["Diffuse"];
                    effect.Parameters["Bones"].SetValue(bones);
                    effect.Parameters["View"].SetValue(Globals.player.camera.view);
                    effect.Parameters["Projection"].SetValue(Globals.player.camera.projection);
                    //effect.Parameters["Texture"].SetValue();
                }
                mesh.Draw();
            }
        }


    }
}
