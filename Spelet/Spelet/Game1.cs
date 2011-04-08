using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using ClassLibrary;
using SkinnedModel;

namespace Spelet
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region declarations
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        NWClient nwClient;
        SpriteFont smallFont, largeFont;
        Vector3 lightDir1 = new Vector3(1, 1, 1);
        Matrix shadow;

        //Startmeny
        StartScene startScene;

        //Serverlistan
        ConsoleScene consoleScene;

        //själva scenen där man spelar vårt awsc000la sp3l! ;)
        PlayingScene playingScene;

        //Aktiv scen
        GameScene activeScene;

        //Klass för att spela upp clip
        ClipPlayer clipPlayer;

        SkinningData rifleSkinningData;
        SkinningData pistolSkinningData;

        //Klass för animationsklipp
        AnimationClip rifleClip;
        AnimationClip pistolClip;

        //Rasmus Skit
        Boolean isRunning = false;
        Boolean canShoot = true;
        Boolean isShooting = false;

        float fps;

        //Translationsvektor för vapnet
        //Vector3 gunPos = new Vector3(1 / 3, 0, 2);
        //Borde läggas i vapenklass!
        Vector3 riflePos = new Vector3(0.3f, 0.1f, 2f);
        Vector3 pisolPos = new Vector3(0.3f, -1.0f, 1.5f);
        Vector3 gunPos;
        Effect skySphereEffect, mapEffect, shadowEffect;
        Texture2D[] mapTexture = new Texture2D[2];
        Texture2D normalMap, heightMap;
        public Model skySphere, rifleModel, pistolModel, rasmus, hampus, level, currentWep;
        int amplitude = 0;

        Texture2D crossHair, HUD;
        Texture2D startBackgroundTexture;
        float timeDifference = 0;
        float time = 0;
        TimeSpan PassedTime = new TimeSpan(0);

        //Statusinfo
        String status = "h";
#endregion
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            //Sätter upplösning och fullskärmsläge
            graphics.PreferredBackBufferWidth = Constants.SCRWIDTH;
            graphics.PreferredBackBufferHeight = Constants.SCRHEIGHT;
            graphics.IsFullScreen = false;
            graphics.PreferMultiSampling = true;
            //showRealFPS();
            graphics.ApplyChanges();
            Window.Title = "Axel är bäst, alltid!!!";
            this.IsMouseVisible = true;

            base.Initialize();
        }
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            Services.AddService(typeof(SpriteBatch), spriteBatch);

            device = graphics.GraphicsDevice;
            
            //Initierar clienten, skapar spelaren och kameran också!
            
            nwClient = new NWClient(device);
            LoadModels();
            LoadScenes(); 
        }
        #region loadFunctions
        private void LoadScenes()
        {
            //Laddar scenes och hämtar all content som behövs
            //Spelet är uppbyggt med "scener" som är olika spellägen
            //Dessa är bland annat startmenyn och själva spelläget
            smallFont = Content.Load<SpriteFont>("menuSmall");
            largeFont = Content.Load<SpriteFont>("menuLarge");
            startBackgroundTexture = Content.Load<Texture2D>("images/SpaceBackground");

            //Skapar en startscen där smallfont är fonten som används när man inte har
            //musen över en länk och largefont är för vald font. StartBack.. är scenens
            //bakgrund.
            startScene = new StartScene(this, smallFont, largeFont,
                startBackgroundTexture);

            //Samma som ovan
            consoleScene = new ConsoleScene(this, smallFont, largeFont, startBackgroundTexture);

            playingScene = new PlayingScene(this, crossHair, HUD);

            //Components är typ spelets komponenter :O används för att den ska rita ut allt o shit!
            Components.Add(consoleScene);
            Components.Add(startScene);
            Components.Add(playingScene);

            //Visar startscenen
            startScene.Show();
            activeScene = startScene;
        }
        private void LoadModels()
        {
            LoadSkySphere();
            LoadMap();
            LoadKillers();
            LoadHUD();
            
        }
        private void LoadHUD()
        {
            crossHair = Content.Load<Texture2D>("Images/crosshair1");
            HUD = Content.Load<Texture2D>("Images/HUD");
        }
        private void LoadSkySphere()
        {
            skySphereEffect = Content.Load<Effect>("Effects/SkySphere");
            skySphere = Content.Load<Model>("Models/SphereHighPoly");
            TextureCube SkyboxTexture = Content.Load<TextureCube>("Images/uffizi_cross");
            skySphereEffect.Parameters["ViewMatrix"].SetValue(nwClient.player.camera.view);
            skySphereEffect.Parameters["ProjectionMatrix"].SetValue(nwClient.player.camera.projection);
            skySphereEffect.Parameters["SkyboxTexture"].SetValue(SkyboxTexture);
            // Set the Skysphere Effect to each part of the Skysphere model
            foreach (ModelMesh mesh in skySphere.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = skySphereEffect;
                }
            }
        }
        private void LoadMap()
        {
            mapEffect = Content.Load<Effect>("Effects/MapShader");
            mapTexture[0] = Content.Load<Texture2D>("Images/color_map");
            mapTexture[1] = Content.Load<Texture2D>("Images/wallTile");
            normalMap = Content.Load<Texture2D>("Images/normal_map");
            heightMap = Content.Load<Texture2D>("Images/height_map");
            level = Content.Load<Model>("Models/level3");
            foreach (ModelMesh mesh in level.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = mapEffect;
                }
            }
        }
        private void LoadKillers()
        {
            rasmus = Content.Load<Model>("Models/RasmusEMBED");
            hampus = Content.Load<Model>("Models/HampusEMBED");
            Globals.hampus = Content.Load<Model>("Models/HampusEMBED");
            Globals.rasmus = Content.Load<Model>("Models/RasmusEMBED");
            rifleModel = Content.Load<Model>("Models/rifleHands1");
            pistolModel = Content.Load<Model>("Models/pistolarms1");

            gunPos = riflePos;
            currentWep = rifleModel;

            pistolSkinningData = pistolModel.Tag as SkinningData;
            if (pistolSkinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag. Har du satt SkinnedModelProcessor? ");
            rifleSkinningData = rifleModel.Tag as SkinningData;
            if (rifleSkinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag. Har du satt SkinnedModelProcessor? ");

            clipPlayer = new ClipPlayer(rifleSkinningData, fps);
            rifleClip = rifleSkinningData.AnimationClips["Take 001"];
            pistolClip = pistolSkinningData.AnimationClips["Take 001"];
            shadow = Matrix.CreateShadow(lightDir1,
                new Plane(0, 1, 0, -1));
            shadowEffect = Content.Load<Effect>("Effects/ShadowEffect");
        }
        #endregion
        protected override void UnloadContent()
        {
            exit();
        }
        protected override void Update(GameTime gameTime)
        {
            timeDifference = (float)gameTime.ElapsedGameTime.TotalSeconds;
            time = (float)((gameTime.TotalGameTime.TotalMilliseconds / 2000) % 2 * Math.PI);
            fps = 1 / (timeDifference);
            clipPlayer.updateFps(fps);

            nwClient.UpdatePos(timeDifference);
            nwClient.GetMsgs();
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Escape))
                this.Exit();

            HandleScenesInput(gameTime);

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (activeScene == playingScene)
            {
                DrawPlayingScene(gameTime);
            }
            spriteBatch.Begin();
            spriteBatch.DrawString(smallFont, status, new Vector2(0, 10), Color.White);
            spriteBatch.DrawString(smallFont,"FPS: " + fps + " ", new Vector2(0, 40), Color.White);
            base.Draw(gameTime);
            spriteBatch.End();
        }
        #region drawingFunctions
        private void DrawPlayingScene(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default; //Polygon culling
            float t = (float)gameTime.TotalGameTime.Seconds;

            DrawLevel(level, Matrix.Identity, t);

            OtherPlayer a = new OtherPlayer(10, 0, 100, 0, 10, 0);

            //graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default; //Polygon culling
            //DrawStationary(a, Matrix.Identity);
            DrawOtherPlayer(a, Matrix.Identity);
            DrawShadow(a, Matrix.Identity);
            if (nwClient.connected)
            {
                for (int i = 0; i < Constants.MAXPLAYERS; i++)
                {
                    if (nwClient.players[i] != null)
                    {
                        if (nwClient.players[i].isAlive())
                        {
                            DrawStationary(nwClient.players[i], Matrix.Identity);
                        }
                    }
                }
            }
            device.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);//Rensar djupet i bilden

            DrawGun(currentWep, Matrix.Identity);
            Vector2 middle = new Vector2(device.Viewport.Width / 2 - 25, device.Viewport.Height / 2 - 25);
            spriteBatch.Begin();
            spriteBatch.Draw(crossHair, middle, Color.Cyan);
            spriteBatch.Draw(HUD, new Rectangle(0, 0, Constants.SCRWIDTH, Constants.SCRHEIGHT), Color.White);

            spriteBatch.End();
        }
        private void DrawGun(Model model, Matrix world)
        {
            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.LeftShift))
            {
                amplitude = 7;
            }
            else amplitude = 2;

            float sinDisp = amplitude * (float)Math.Sin(time * amplitude) / 30;//sinus displacement
            float cosDisp = amplitude * (float)Math.Cos(time * amplitude) / 30;//cossinus displacement

            world = world * Matrix.CreateTranslation(gunPos) * 
            Matrix.CreateTranslation(new Vector3(0.15f + cosDisp / 2, -12.2f - Math.Abs(sinDisp), -20 + cosDisp / 3)) *
            Matrix.CreateTranslation(new Vector3(0f, 0f, nwClient.player.rifle.zRecoil)) *
            Matrix.CreateScale(0.5f, 0.5f, 0.5f) *
            Matrix.CreateRotationY(MathHelper.Pi) / 2 *
            nwClient.player.camera.rotation * Matrix.CreateTranslation(nwClient.player.camera.position + 10 * nwClient.player.camera.rotatedTarget);
            Matrix[] bones = clipPlayer.GetSkinTransforms();
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.World = world;

                    effect.View = nwClient.player.camera.view;
                    effect.Projection = nwClient.player.camera.projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }
        }
        private void DrawOtherPlayer(OtherPlayer otherPlayer, Matrix world)
        {
            Model model = rasmus;
            if (otherPlayer.model == Constants.HAMPUS)
            {
                model = hampus;
            }
            else if (otherPlayer.model == Constants.RASMUS)
            {
                model = rasmus;
            }
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = shadowEffect;
                }
            }
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            Matrix[] bones = clipPlayer.GetSkinTransforms();
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques["Diffuse"];
                    effect.Parameters["Bones"].SetValue(bones);
                    effect.Parameters["View"].SetValue(nwClient.player.camera.view);
                    effect.Parameters["Projection"].SetValue(nwClient.player.camera.projection);
                    //effect.Parameters["Texture"].SetValue();
                }
                mesh.Draw();
            }
        }
        private void DrawStationary(OtherPlayer otherPlayer, Matrix world)
        {
            Model models = rasmus;
            if (otherPlayer.model == Constants.HAMPUS)
            {
                models = hampus;
            }
            else if (otherPlayer.model == Constants.RASMUS)
            {
                models = rasmus;
            }
            
            world = Matrix.CreateRotationY(otherPlayer.forwardDir)*Matrix.CreateTranslation(otherPlayer.position);
            
            for (int i = 1; i < models.Meshes.Count; i++)
            {
                ModelMesh mesh = models.Meshes[i];
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = world;
                    // Use the matrices provided by the game camera
                    effect.View = nwClient.player.camera.view;
                    effect.Projection = nwClient.player.camera.projection;
                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }
                mesh.Draw();
            }
            ModelMesh me = models.Meshes[0];
            foreach (BasicEffect effect in me.Effects)
            {
                effect.EnableDefaultLighting();
                effect.PreferPerPixelLighting = true;
                effect.World = world;
                // Use the matrices provided by the game camera
                effect.View = nwClient.player.camera.view;
                effect.Projection = nwClient.player.camera.projection;
                effect.SpecularColor = new Vector3(0.25f);
                effect.SpecularPower = 16;
            }
            me.Draw();
        }
        private void DrawLevelBasic(Model model, Matrix world)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = world;
                    // Use the matrices provided by the game camera
                    effect.View = nwClient.player.camera.view;
                    effect.Projection = nwClient.player.camera.projection;
                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }
                mesh.Draw();

            }
        }
        private void DrawLevel(Model model, Matrix world, float t)
        {
            
            for(int i = 0; i < model.Meshes.Count; i++)
            {
                ModelMesh mesh = model.Meshes.ElementAt(i);
                foreach(Effect effect in mesh.Effects)
                {
                    if (i == 0)
                    {
                        effect.CurrentTechnique = effect.Techniques["Bump"];
                        effect.Parameters["N_Texture"].SetValue(normalMap);
                    }
                    if (i == 1)
                    {
                        effect.CurrentTechnique = effect.Techniques["Basic"];
                    }
                    
                    effect.Parameters["Texture"].SetValue(mapTexture[i]);
                    effect.Parameters["Projection"].SetValue(nwClient.player.camera.projection);
                    effect.Parameters["View"].SetValue(nwClient.player.camera.view);
                    effect.Parameters["lightDir1"].SetValue(new Vector3(-(float)Math.Sin(t/2),-(float)Math.Sin(t/2) , 5));
                }
                mesh.Draw();
                status = t.ToString();
                
            }
            skySphereEffect.Parameters["ViewMatrix"].SetValue(
                        nwClient.player.camera.view);
            skySphereEffect.Parameters["ProjectionMatrix"].SetValue(
                                    nwClient.player.camera.projection);
            foreach (ModelMesh mesh in skySphere.Meshes)
            {
                mesh.Draw();
            }
        }
        private void DrawShadow(OtherPlayer otherPlayer, Matrix world)
        {
            Model models = rasmus;
            if (otherPlayer.model == Constants.HAMPUS)
            {
                models = hampus;
            }
            else if (otherPlayer.model == Constants.RASMUS)
            {
                models = rasmus;
            }
            foreach (ModelMesh mesh in models.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = shadowEffect;
                }
            }
            Matrix[] bones = new Matrix[models.Bones.Count];
            models.CopyAbsoluteBoneTransformsTo(bones);

            //set render states
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Matrix[] shadowBones = new Matrix[bones.Length];
            for (int i = 0; i < shadowBones.Length; i++)
            {
                shadowBones[i] = bones[i] * shadow;
            }

            foreach (ModelMesh mesh in models.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques["SkinnedModelTechnique"];
                    effect.Parameters["Bones"].SetValue(shadowBones);
                    effect.Parameters["View"].SetValue(nwClient.player.camera.view);
                    effect.Parameters["Projection"].SetValue(nwClient.player.camera.projection);
                }
                mesh.Draw();
            }

            world = Matrix.CreateRotationY(otherPlayer.forwardDir) * Matrix.CreateTranslation(otherPlayer.position);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
        }
        #endregion
        protected void ShowScene(GameScene scene)
        {
            activeScene.Hide();
            activeScene = scene;
            scene.Show();
            activeScene.Show();
            if (scene == playingScene)//vad gör denna? MousePos sköts annars helt från cameraklassen
            {
                this.IsMouseVisible = false;
                clipPlayer.play(rifleClip, 100, 100, false);
            }
        }
        #region handleInput
        private void HandleScenesInput(GameTime gameTime)
        {
            
            // Handle Start Scene Input
            if(activeScene == startScene)
            {
                HandleStartSceneInput();
            }
            if(activeScene == consoleScene)
            {
                HandleConsoleSceneInput();
            }
            if(activeScene == playingScene)
            {
                HandlePlayingSceneInput(gameTime);
            }

        }
        private void HandleConsoleSceneInput()
        {
            if (nwClient.connected)
            {
                ShowScene(playingScene); 
                status = "Connected!";
            }
            else if (CheckClick())
            {
                int _s = consoleScene.SelectedMenuIndex;
                status = Convert.ToString(consoleScene.serverList.CountServers());
                if (_s != -1 && _s < consoleScene.serverList.CountServers())
                {
                    string k = consoleScene.serverList.getIp(_s);

                    nwClient.TryConnectIp(k);
                    
                    status = "Trying to connect to: " + k;
                    nwClient.GetMsgs();
                }
                else
                {
                    if (_s == consoleScene.serverList.CountServers())
                    {
                        consoleScene.UpdateServerList();
                    }
                    if (_s  == consoleScene.serverList.CountServers() + 1)
                    {
                        ShowScene(startScene);
                    }
                }
            }
        }
        private void HandleStartSceneInput()
        {
            if (CheckClick())
            {
                switch (startScene.SelectedMenuIndex)
                {
                    case 0:
                        if(!Functions.IsServerRunning())
                            Functions.RunServerFromPath();
                        nwClient.TryConnectLocal();
                        if (nwClient.connected)
                        {
                            ShowScene(playingScene);
                        }
                        break;
                    case 1:
                        ShowScene(consoleScene);
                        break;
                    case 2:
                        ShowScene(playingScene);
                        break;

                }
            }
        }
        private void HandlePlayingSceneInput(GameTime gameTime)
        {
            clipPlayer.update(gameTime.ElapsedGameTime, true, Matrix.Identity);
            timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            nwClient.player.updatePlayer(timeDifference);
            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (nwClient.player != null)
            {
                status += Convert.ToString(nwClient.player.forwardDir) + "\n";
            }
            if (nwClient.players != null)
            {
                foreach (OtherPlayer op in nwClient.players)
                {
                    if (op != null)
                    {
                        status += Convert.ToString(op.forwardDir) + "\n";
                    }
                }
            }
            if (keyState.IsKeyDown(Keys.D1))
            {
                //Spela alla animationer
                if(currentWep == rifleModel)
                {
                    clipPlayer.play(rifleClip, 1, 1000, false);
                }
                if(currentWep == pistolModel)
                {
                    clipPlayer.play(pistolClip, 2, 1000, false);
                }
            }
            if (CheckClick() && canShoot && isShooting == false)
            {
                //Skjutanimation
                isShooting = true;
                if(currentWep == rifleModel)
                {
                    clipPlayer.play(rifleClip, 102, 124, true);
                }
                if(currentWep == pistolModel)
                {
                    clipPlayer.play(pistolClip, 400, 430, true);
                }
                
            }
            if (!CheckClick() && isShooting == true)
            {
                //Sluta skjuta
                isShooting = false;
                
                if(currentWep == rifleModel)
                {
                    clipPlayer.play(rifleClip, 116, 124, false);
                }
                if(currentWep == pistolModel)
                {
                    clipPlayer.play(pistolClip, 200, 200, false);
                }
            }
            if (keyState.IsKeyDown(Keys.R))
            {
                //Ladda om
                if(currentWep == rifleModel)
                {
                    clipPlayer.play(rifleClip, 125, 283, false);
                }
                if(currentWep == pistolModel)
                {
                    clipPlayer.play(pistolClip, 200, 400, true);
                }
            }
            if (keyState.IsKeyDown(Keys.Q) )
            {
                //byt vapen
                if (currentWep == rifleModel)
                {
                    clipPlayer.play(rifleClip, 340, 379, false);
                }
                if (currentWep == pistolModel)
                {
                    clipPlayer.play(pistolClip, 1, 1, false);
                }
            }

            if (keyState.IsKeyDown(Keys.LeftShift) && isRunning == false)
            {
                canShoot = false;
                isRunning = true;
                //Börja springa
                if (currentWep == rifleModel)
                {
                    clipPlayer.play(rifleClip, 284, 339, false);
                }
                if (currentWep == pistolModel)
                {
                    clipPlayer.play(pistolClip, 200, 200, false);
                }
            }
            if (keyState.IsKeyUp(Keys.LeftShift) && isRunning == true)
            {
                isRunning = false;
                canShoot = true;
                //Sluta springa
                if (currentWep == pistolModel)
                {
                    clipPlayer.play(rifleClip, 309, 340, false);
                }
                if (currentWep == pistolModel)
                {
                    clipPlayer.play(pistolClip, 200, 200, false);
                }

            }

            if (clipPlayer.inRange(379, 379) && currentWep == rifleModel)
            {
                ChangeWeapon(pistolModel);
            }
            if (clipPlayer.inRange(1, 1) && currentWep == pistolModel)
            {
                ChangeWeapon(rifleModel);
            }
        }
        private void ChangeWeapon(Model m)
        {
            if (m == pistolModel)
            {
                currentWep = m;
                gunPos = pisolPos;
                clipPlayer = new ClipPlayer(pistolSkinningData, fps);
                clipPlayer.play(pistolClip, 2, 200, false);
            }
            else if (m == rifleModel)
            {
                currentWep = m;
                gunPos = riflePos;
                clipPlayer = new ClipPlayer(rifleSkinningData, fps);
                clipPlayer.play(rifleClip, 1, 105, false);
            }
        }
        private bool CheckClick()
        {
            // Get the Keyboard and GamePad state
            MouseState ms = Mouse.GetState();

            if (ms.LeftButton == ButtonState.Pressed)
            {
                return true;
            }
            return false;
        }
        #endregion

        private void exit()
        {
            nwClient.LeaveMsg();
        }
        void Game1_Exiting(object sender, EventArgs e)
        {
            // Add any code that must execute before the game ends.
            exit();
        }
    }
}
