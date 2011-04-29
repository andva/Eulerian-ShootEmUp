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
        Boolean isRunning = false;
        Boolean canShoot = true;
        Boolean isShooting = false;
        Matrix shadow;
        Vector3 lightDir1 = new Vector3(1, 1, 1);


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
            g = game;
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
        }
        public override void Hide()
        {
            g.IsMouseVisible = true;
            base.Hide();
        }
        private void LoadModels()
        {
            modelHampus = Globals.hampus;
            modelRasmus = Globals.rasmus;
            modelRifle = Globals.rifle;
            level = Globals.level;
            shadow = Matrix.CreateShadow(lightDir1,
                new Plane(0, 1, 0, -1));
        }
        public override void Update(GameTime gameTime)
        {
            HandleInput(gameTime);
            base.Update(gameTime);
        }
        private void HandleInput(GameTime gameTime)
        {
            Globals.clipPlayer.update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            Globals.player.updatePlayer(timeDifference);
            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (keyState.IsKeyDown(Keys.D1))
            {
                //Spela alla animationer
                Globals.clipPlayer.play(Globals.rifleClip, 1, 1000, false);

            }
            if (keyState.IsKeyDown(Keys.R))
            {
                //Ladda om
                Globals.clipPlayer.play(Globals.rifleClip, 125, 283, false);
            }
            if (keyState.IsKeyDown(Keys.Q))
            {
                //byt vapen
                Globals.clipPlayer.play(Globals.rifleClip, 340, 379, false);
            }

            if (keyState.IsKeyDown(Keys.LeftShift) && isRunning == false)
            {
                canShoot = false;
                isRunning = true;
                //Börja springa
                Globals.clipPlayer.play(Globals.rifleClip, 284, 339, false);
            }
            if (keyState.IsKeyUp(Keys.LeftShift) && isRunning == true)
            {
                isRunning = false;
                canShoot = true;
                //Sluta springa
                Globals.clipPlayer.play(Globals.rifleClip, 309, 340, false);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            time = (float)((gameTime.TotalGameTime.TotalMilliseconds / 2000) % 2 * Math.PI);
            base.Game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawSkySphere();
            base.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default; //Polygon culling
            level.DrawLevel();
            OtherPlayer a = new OtherPlayer(10, 0, 100, 0, 10, 0);
            DrawOtherPlayer(a, Matrix.Identity);
            //DrawShadow(a, Matrix.Identity);
            base.Game.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);//Rensar djupet i bilden
            //DrawGun();
            Globals.player.rifle.DrawGun(time);
            spriteBatch.Draw(crossHair, middle, Color.Cyan);
            base.Draw(gameTime);
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

        private void DrawOtherPlayer(OtherPlayer otherPlayer, Matrix w)
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
            //w = Matrix.CreateRotationY(otherPlayer.forwardDir) * Matrix.CreateTranslation(otherPlayer.position);
            base.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            base.Game.GraphicsDevice.BlendState = BlendState.Opaque;
            Matrix[] bones = Globals.clipPlayer.GetSkinTransforms(); //FÖRSTÖRS HÄR!! Fixa en till clip-player!
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i] = bones[i] * w;
            }
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

            base.Game.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            base.Game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Matrix[] shadowBones = new Matrix[bones.Length];
            for (int i = 0; i < shadowBones.Length; i++)
            {
                shadowBones[i] = bones[i] * shadow;
            }

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques["Diffuse"];
                    effect.Parameters["Bones"].SetValue(shadowBones);
                    effect.Parameters["View"].SetValue(Globals.player.camera.view);
                    effect.Parameters["Projection"].SetValue(Globals.player.camera.projection);
                }
                mesh.Draw();
            }
            base.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            base.Game.GraphicsDevice.BlendState = BlendState.Opaque;
        }
    }
}
