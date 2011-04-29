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
        Model modelHampus, modelRasmus, modelPistol, modelRifle, currentWep;
        Level level;
        Vector3 riflePos = new Vector3(0.3f, 0.1f, 2f);
        Vector3 pisolPos = new Vector3(0.3f, -1.0f, 1.5f);
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
            currentWep = modelRifle;
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
            modelPistol = Globals.pistol;
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
                if (currentWep == Globals.rifle)
                {
                    Globals.clipPlayer.play(Globals.rifleClip, 1, 1000, false);
                }
                if (currentWep == Globals.pistol)
                {
                    Globals.clipPlayer.play(Globals.pistolClip, 2, 1000, false);
                }
            }
            if (CheckClick() && canShoot && isShooting == false)
            {
                //Skjutanimation
                isShooting = true;
                if (currentWep == Globals.rifle)
                {
                    Globals.clipPlayer.play(Globals.rifleClip, 102, 124, true);
                }
                if (currentWep == Globals.pistol)
                {
                    Globals.clipPlayer.play(Globals.pistolClip, 400, 430, true);
                }

            }
            if (!CheckClick() && isShooting == true)
            {
                //Sluta skjuta
                isShooting = false;

                if (currentWep == Globals.rifle)
                {
                    Globals.clipPlayer.play(Globals.rifleClip, 116, 124, false);
                }
                if (currentWep == Globals.pistol)
                {
                    Globals.clipPlayer.play(Globals.pistolClip, 200, 200, false);
                }
            }
            if (keyState.IsKeyDown(Keys.R))
            {
                //Ladda om
                if (currentWep == Globals.rifle)
                {
                    Globals.clipPlayer.play(Globals.rifleClip, 125, 283, false);
                }
                if (currentWep == Globals.pistol)
                {
                    Globals.clipPlayer.play(Globals.pistolClip, 200, 400, true);
                }
            }
            if (keyState.IsKeyDown(Keys.Q))
            {
                //byt vapen
                if (currentWep == Globals.rifle)
                {
                    Globals.clipPlayer.play(Globals.rifleClip, 340, 379, false);
                }
                if (currentWep == Globals.pistol)
                {
                    Globals.clipPlayer.play(Globals.pistolClip, 1, 1, false);
                }
            }

            if (keyState.IsKeyDown(Keys.LeftShift) && isRunning == false)
            {
                canShoot = false;
                isRunning = true;
                //Börja springa
                if (currentWep == Globals.rifle)
                {
                    Globals.clipPlayer.play(Globals.rifleClip, 284, 339, false);
                }
                if (currentWep == Globals.pistol)
                {
                    Globals.clipPlayer.play(Globals.pistolClip, 200, 200, false);
                }
            }
            if (keyState.IsKeyUp(Keys.LeftShift) && isRunning == true)
            {
                isRunning = false;
                canShoot = true;
                //Sluta springa
                if (currentWep == Globals.pistol)
                {
                    Globals.clipPlayer.play(Globals.rifleClip, 309, 340, false);
                }
                if (currentWep == Globals.pistol)
                {
                    Globals.clipPlayer.play(Globals.pistolClip, 200, 200, false);
                }

            }

            if (Globals.clipPlayer.inRange(379, 379) && currentWep == Globals.rifle)
            {
                ChangeWeapon(Globals.pistol);
            }
            if (Globals.clipPlayer.inRange(1, 1) && currentWep == Globals.pistol)
            {
                ChangeWeapon(Globals.rifle);
            }
        }

        private void ChangeWeapon(Model m)
        {
            if (m == Globals.pistol)
            {
                currentWep = m;
                gunPos = pisolPos;
                Globals.clipPlayer = new ClipPlayer(Globals.pistolSkinningData, 60);
                Globals.clipPlayer.play(Globals.pistolClip, 2, 200, false);
            }
            else if (m == Globals.rifle)
            {
                currentWep = m;
                gunPos = riflePos;
                Globals.clipPlayer = new ClipPlayer(Globals.rifleSkinningData, 60);
                Globals.clipPlayer.play(Globals.rifleClip, 1, 105, false);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            time = (float)((gameTime.TotalGameTime.TotalMilliseconds / 2000) % 2 * Math.PI);
            base.Game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawSkySphere();
            base.Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default; //Polygon culling
            DrawLevel();
            OtherPlayer a = new OtherPlayer(10, 0, 100, 0, 10, 0);
            DrawOtherPlayer(a, Matrix.Identity);
            //DrawShadow(a, Matrix.Identity);
            base.Game.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);//Rensar djupet i bilden
            DrawGun();
            spriteBatch.Draw(crossHair, middle, Color.Cyan);
            base.Draw(gameTime);
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
            Globals.player.camera.rotation * 
            Matrix.CreateTranslation(Globals.player.camera.position + 10 * Globals.player.camera.rotatedTarget);
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
            for(int i = 0; i < bones.Length; i++)
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
        private void DrawShadow(OtherPlayer otherPlayer, Matrix w)
        {
            Model models = Globals.rasmus;
            if (otherPlayer.model == Constants.HAMPUS)
            {
                models = Globals.hampus;
            }
            else if (otherPlayer.model == Constants.RASMUS)
            {
                models = Globals.rasmus;
            }
            foreach (ModelMesh mesh in models.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = Globals.shadowEffect;
                }
            }
            Matrix[] bones = new Matrix[models.Bones.Count];
            models.CopyAbsoluteBoneTransformsTo(bones);

            //set render states
            base.Game.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            base.Game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Matrix[] shadowBones = new Matrix[bones.Length];
            w = Matrix.CreateRotationY(otherPlayer.forwardDir) * Matrix.CreateTranslation(otherPlayer.position);
            for (int i = 0; i < shadowBones.Length; i++)
            {
                shadowBones[i] = bones[i] * w * shadow ;
            }

            foreach (ModelMesh mesh in models.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques["Shadow"];
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
