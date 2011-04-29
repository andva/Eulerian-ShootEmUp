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
        

        //Startmeny
        StartScene startScene;
        //Serverlistan
        ConsoleScene consoleScene;
        //själva scenen där man spelar vårt awsc000la sp3l! ;)
        PlayingScene playingScene;
        //Aktiv scen
        GameScene activeScene;

        KeyboardState prevKS;
        KeyboardState nowKS;

        //Klass för att spela upp clip
        ClipPlayer clipPlayer;

        SkinningData rifleSkinningData;
        SkinningData pistolSkinningData;

        //Klass för animationsklipp
        AnimationClip rifleClip;
        AnimationClip pistolClip;

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
        public Model skySphere, rifleModel, pistolModel, rasmus, hampus, currentWep;

        Texture2D crossHair, HUD;
        Texture2D startBackgroundTexture;
        float timeDifference = 0;
        float time = 0;
        TimeSpan PassedTime = new TimeSpan(0);

#endregion
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //init ljud
            Globals.audioManager = new AudioManager(this);
            Components.Add(Globals.audioManager);
        }


        #region loadFunctions
        protected override void Initialize()
        {
            //Sätter upplösning och fullskärmsläge
            graphics.PreferredBackBufferWidth = Constants.SCRWIDTH;
            graphics.PreferredBackBufferHeight = Constants.SCRHEIGHT;
            graphics.IsFullScreen = false;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();
            Window.Title = "Valter är bäst, såklart!!!!";
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
            LoadGlobals();
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
            Texture2D[] mEffect = new Texture2D[2];
            mEffect[0] = Content.Load<Texture2D>("Images/normal_map");
            mEffect[1] = Content.Load<Texture2D>("Images/height_map");
            Texture2D heightMap = Content.Load<Texture2D>("Images/heightmap");

            Model levelModel = Content.Load<Model>("Models/level3");
            foreach (ModelMesh mesh in levelModel.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = mapEffect;
                }
            }

            Globals.level = new Level(levelModel, mapTexture, mEffect, mapEffect, heightMap);
        }
        private void LoadKillers()
        {
            rasmus = Content.Load<Model>("Models/RasmusEMBED");
            hampus = Content.Load<Model>("Models/HampusEMBED");
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
            
            shadowEffect = Content.Load<Effect>("Effects/ShadowEffect");
           
        }
        private void LoadGlobals()
        {
            Globals.rifleClip = rifleClip;
            Globals.pistolClip = pistolClip;
            Globals.rifle = rifleModel;
            Globals.hampus = hampus;
            Globals.rasmus = rasmus;
            Globals.pistol = pistolModel;
            Globals.clipPlayer = clipPlayer;
            Globals.pistolSkinningData = pistolSkinningData;
            Globals.rifleSkinningData = rifleSkinningData;
            Globals.shadowEffect = shadowEffect;
            Globals.skysphere = skySphere;
            Globals.skySphereEffect = skySphereEffect;
        }
        #endregion
        
        protected override void Update(GameTime gameTime)
        {
            timeDifference = (float)gameTime.ElapsedGameTime.TotalSeconds;
            time = (float)((gameTime.TotalGameTime.TotalMilliseconds / 2000) % 2 * Math.PI);
            fps = 1 / (timeDifference);
            clipPlayer.updateFps(fps);

            nwClient.UpdatePos(timeDifference);
            nwClient.GetMsgs();
            


            HandleScenesInput(gameTime);

            base.Update(gameTime);
        }
        
        #region drawingFunctions
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            base.Draw(gameTime);
            spriteBatch.End();
        }
        private void DrawStationary(OtherPlayer otherPlayer, Matrix world)
        {
            /*Model models = rasmus;
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
            me.Draw();*/
        }
        #endregion

        #region handleInput
        private void HandleScenesInput(GameTime gameTime)
        {
            nowKS = Keyboard.GetState();


            if (nowKS.IsKeyUp(Keys.Escape) && prevKS.IsKeyDown(Keys.Escape))
            {
                if (activeScene == playingScene)
                    ShowScene(startScene);
                else
                    this.Exit();
            }

            prevKS = nowKS;
            // Handle Start Scene Input
            if (activeScene == startScene)
            {
                HandleStartSceneInput();
            }
            if(activeScene == consoleScene)
            {
                HandleConsoleSceneInput();
            }

        }
        private void HandleStartSceneInput()
        {
            if (CheckClick())
            {
                switch (startScene.SelectedMenuIndex)
                {
                    case 0:
                        if (!Functions.IsServerRunning())
                            Functions.RunServerFromPath();
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
                    case 3:
                        this.Exit();
                        break;
                }
            }
        }
        private void HandleConsoleSceneInput()
        {
            if (nwClient.connected)
            {
                ShowScene(playingScene);
            }
            else if (CheckClick())
            {
                int _s = consoleScene.SelectedMenuIndex;
                if (_s != -1 && _s < consoleScene.serverList.CountServers())
                {
                    string k = consoleScene.serverList.getIp(_s);

                    nwClient.TryConnectIp(k);
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
        protected void ShowScene(GameScene scene)
        {
            activeScene.Hide();
            activeScene = scene;
            scene.Show();
            activeScene.Show();
        }
        #endregion

        #region quit Code
        protected override void UnloadContent()
        {
            exit();
        }
        private void exit()
        {
            nwClient.LeaveMsg();
        }
        void Game1_Exiting(object sender, EventArgs e)
        {
            // Add any code that must execute before the game ends.
            exit();
        }
        #endregion
    }
}
