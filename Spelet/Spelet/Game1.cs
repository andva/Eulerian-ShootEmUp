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
        SpriteFont smallFont, largeFont, deathFont;
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
        SkinningData hampusSkinningData, valterSkinningData, axelSkinningData, rasmusSkinningData;

        //Klass för animationsklipp
        AnimationClip rifleClip;

        float fps;

        //Translationsvektor för vapnet
        //Borde läggas i vapenklass!
       Vector3 riflePos = new Vector3(0.3f, 0.1f, 2f);
        Vector3 gunPos;
        Effect skySphereEffect, mapEffect, shadowEffect;
        Texture2D[] mapTexture = new Texture2D[2];
        Texture2D[] hampusTexture = new Texture2D[2];
        Texture2D[] rasmusTexture = new Texture2D[2];
        public Model skySphere, rifleModel, pistolModel, rasmus, hampus, currentWep, valter, axel;

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
            graphics.IsFullScreen = true;
            //graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();
            Window.Title = "Deathmatch";
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
            LoadKillers();
            nwClient = new NWClient(device);
            LoadModels();
            LoadScenes();

            //TA BORT SEN (bara till för att synka heightmap)
            Globals.effect = Content.Load<Effect>("effects"); //används till rita ut heightmap onö

            loadMuzzleFlash();

            loadBloodBillboard();
        }

        private void LoadScenes()
        {
            //Laddar scenes och hämtar all content som behövs
            //Spelet är uppbyggt med "scener" som är olika spellägen
            //Dessa är bland annat startmenyn och själva spelläget
            smallFont = Content.Load<SpriteFont>("menuSmall");
            largeFont = Content.Load<SpriteFont>("menuLarge");
            Globals.font = smallFont;
            startBackgroundTexture = Content.Load<Texture2D>("images/MainMenuFinal");

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
            
            LoadHUD();
            LoadGlobals();
        }
        private void LoadHUD()
        {
            crossHair = Content.Load<Texture2D>("Images/crosshair1");
            HUD = Content.Load<Texture2D>("Images/HUD");
            SpriteFont hudFont = Content.Load<SpriteFont>("hudFont");
            Texture2D gotHitTexture = Content.Load<Texture2D>("Images/bloodHit");
            Globals.gotHitTexture = gotHitTexture;
            Globals.hud = new HUD(spriteBatch, hudFont);
            SpriteFont deathFont = Content.Load<SpriteFont>("death");
            Globals.deathFont = deathFont;
        }

        private void loadMuzzleFlash()
        {
            Texture2D[] flash = new Texture2D[24];
            for (int i = 0; i < 24; i++)
            {
                if(i < 10)
                    flash[i] = Content.Load<Texture2D>("Images/muzzleflash/Muzzle_0000" + i.ToString());
                else
                    flash[i] = Content.Load<Texture2D>("Images/muzzleflash/Muzzle_000" + i.ToString());
            }
            
            Globals.muzzleflash = new MuzzleFlash(spriteBatch, flash);
        }

        private void loadBloodBillboard()
        {
            Texture2D[] texture = new Texture2D[64];
            for (int i = 0; i < 24; i++)
            {
                if (i < 10)
                    texture[i] = Content.Load<Texture2D>("Images/bloodsplat/Blood_0000" + i.ToString());
                else
                    texture[i] = Content.Load<Texture2D>("Images/bloodsplat/Blood_000" + i.ToString());
            }

            Globals.blood = new BloodList(texture);
            Globals.device = device;
        }

        private void LoadSkySphere()
        {

            skySphereEffect = Content.Load<Effect>("Effects/SkySphere");
            skySphere = Content.Load<Model>("Models/SphereHighPoly");
            TextureCube SkyboxTexture = Content.Load<TextureCube>("Images/sky_cube");
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
            Texture2D heightMap = Content.Load<Texture2D>("Images/heightmap3fix");

            Model levelModel = Content.Load<Model>("Models/Map");
            /*foreach (ModelMesh mesh in levelModel.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = mapEffect;
                }
            }*/

            Globals.level = new Level(device, levelModel, mapEffect, heightMap);
        }
        private void LoadKillers()
        {
            rasmus = Content.Load<Model>("Models/rasmusFinal");
            hampus = Content.Load<Model>("Models/hampusFinal");
            valter = Content.Load<Model>("Models/valterFinal");
            axel = Content.Load<Model>("Models/axelFinal5");
            rifleModel = Content.Load<Model>("Models/rifleHands1");
            gunPos = riflePos;
            currentWep = rifleModel;

            hampusSkinningData = hampus.Tag as SkinningData;
            rasmusSkinningData = rasmus.Tag as SkinningData;
            axelSkinningData = axel.Tag as SkinningData;
            valterSkinningData = valter.Tag as SkinningData;
            if (hampusSkinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag. Har du satt SkinnedModelProcessor? ");
            Globals.hampusSkinningData = hampusSkinningData;
            Globals.rasmusSkinningData = rasmusSkinningData;
            Globals.axelSkinningData = axelSkinningData;
            Globals.valterSkinningData = valterSkinningData;
            rifleSkinningData = rifleModel.Tag as SkinningData;
            if (rifleSkinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag. Har du satt SkinnedModelProcessor? ");

            clipPlayer = new ClipPlayer(rifleSkinningData, fps);
            rifleClip = rifleSkinningData.AnimationClips["Take 001"];

            shadowEffect = Content.Load<Effect>("Effects/ShadowEffect"); 
        }


        private void LoadGlobals()
        {
            Globals.rifleClip = rifleClip;
            Globals.rifle = rifleModel;
            Globals.hampus = hampus;
            Globals.valter = valter;
            Globals.axel = axel;
            Globals.rasmus = rasmus;
            Globals.clipPlayer = clipPlayer;
            Globals.rifleSkinningData = rifleSkinningData;
            //Globals.shadowEffect = shadowEffect;
            Globals.skysphere = skySphere;
            Globals.skySphereEffect = skySphereEffect;
            //Globals.hampusTexture = hampusTexture;
            
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
