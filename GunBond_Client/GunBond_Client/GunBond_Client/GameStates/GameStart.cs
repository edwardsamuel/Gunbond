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

using Nuclex.Game;
using Nuclex.Game.States;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface.Visuals.Flat;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.Input;
using Nuclex.Input.Devices;
using Gunbond;
using Gunbond_Client.Util;
using Gunbond_Client.Model;

namespace GunBond_Client.GameStates
{
    

    class GameStart : DrawableGameState
    {        
        private IGameStateService gameStateService;
        private IGuiService guiService;
        private IInputService inputService;
        private ContentManager Content;
        private IGameState previousState;

        private Screen gameStartScreen;

        enum GameState { Play, GameOver, BackToRoom };
        GameState gamestate = GameState.Play;
        MouseState prev;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GraphicsDevice device;
        Texture2D backgroundTexture;
        Texture2D foregroundTexture;
        Texture2D cannonTexture;
        Texture2D rocketTexture;
        Texture2D smokeTexture;
        Texture2D groundTexture;
        Texture2D exitTexture;
        int screenWidth = 680;
        int screenHeight = 680;
        float playerScaling;
        Peer currentPlayer; 
        SpriteFont font;
        TimeSpan clock;

        int numberOfPlayers;
        int numberOfPlayerAlive;
        // array of carriageTexture
        Texture2D[] carriageTexture = new Texture2D[8];

        // rocket:
        bool rocketFlying = false;
        Vector2 rocketPosition;
        Vector2 rocketDirection;
        float rocketAngle;
        float rocketScaling = 0.1f;

        // smoke:
        List<Vector2> smokeList = new List<Vector2>();
        Random randomizer = new Random();

        // terrain:
        int[] terrainContour;

        // color arrays:
        Color[,] rocketColorArray;
        Color[,] foregroundColorArray;
        Color[,] carriageColorArray;
        Color[,] cannonColorArray;

        List<Peer> teamA;
        List<Peer> teamB;
        int idxA;
        int idxB;

        public GameStart(IGameStateService gameStateService, IGuiService guiService,
                        IInputService inputService, GraphicsDeviceManager graphics, 
                        ContentManager content, List<Peer> teamA, List<Peer> teamB)
        {
            this.gameStateService = gameStateService;
            this.guiService = guiService;
            this.inputService = inputService;
            this.graphics = graphics;
            this.Content = content;
            this.previousState = gameStateService.ActiveState;

            this.numberOfPlayers = Game1.main_console.Room.Members.Count();
            gameStartScreen = new Screen(680, 680);

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            Game1.main_console.GameEvent += ProcessMessages;

            this.teamA = teamA;
            this.teamB = teamB;
            idxA = 0;
            idxB = 0;
            
            LoadContent();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void OnEntered()
        {
            guiService.Screen = gameStartScreen;
            // TODO: Add your initialization logic here
            // set size of backbuffer, which contain what will be drawn to the screen
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
        }

        private void SetUpPlayers()
        // initializes array of PlayerData objects
        {
            for (int i = 0; i < numberOfPlayers; i++)
            {
                Game1.main_console.Room.Members[i].CarriageTexture = carriageTexture[i];
                Game1.main_console.Room.Members[i].Position.X = screenWidth / (numberOfPlayers + 1) * (i + 1);
                Game1.main_console.Room.Members[i].Position.Y = terrainContour[(int)Game1.main_console.Room.Members[i].Position.X];
            }
            numberOfPlayerAlive = numberOfPlayers;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            device = graphics.GraphicsDevice;

            // linking variable backgroundTexture to an img named "background"
            backgroundTexture = Content.Load<Texture2D>("bg_blue");

            // array of CariageTexture
            carriageTexture[0] = Content.Load<Texture2D>("P1");
            carriageTexture[1] = Content.Load<Texture2D>("P2");
            carriageTexture[2] = Content.Load<Texture2D>("P3");
            carriageTexture[3] = Content.Load<Texture2D>("P4");
            carriageTexture[4] = Content.Load<Texture2D>("P5");
            carriageTexture[5] = Content.Load<Texture2D>("P6");
            carriageTexture[6] = Content.Load<Texture2D>("P7");
            carriageTexture[7] = Content.Load<Texture2D>("P8");

            cannonTexture = Content.Load<Texture2D>("cannon");

            playerScaling = 50.0f / (float)carriageTexture[0].Width;

            font = Content.Load<SpriteFont>("myFont");

            rocketTexture = Content.Load<Texture2D>("rocket");
            smokeTexture = Content.Load<Texture2D>("smoke");
            groundTexture = Content.Load<Texture2D>("ground");
            exitTexture = Content.Load<Texture2D>("Images\\Lobby\\Exit");

            GenerateTerrainContour();
            SetUpPlayers();
            FlattenTerrainBelowPlayers();
            CreateForeground();

            currentPlayer = teamA[0];
            rocketColorArray = TextureTo2DArray(rocketTexture);
            carriageColorArray = TextureTo2DArray(currentPlayer.CarriageTexture);
            cannonColorArray = TextureTo2DArray(cannonTexture);
            // TODO: use this.Content to load your game content here
        }

        private void FlattenTerrainBelowPlayers()
        {
            foreach (Peer player in Game1.main_console.Room.Members)
                if (player.IsAlive)
                    for (int x = 0; x < 40; x++)
                        terrainContour[(int)player.Position.X + x] = terrainContour[(int)player.Position.X];
        }

        private void GenerateTerrainContour()
        // initializes array of coordinates and fills it with values
        {
            terrainContour = new int[screenWidth];

            double rand1 = randomizer.NextDouble() + 1;
            double rand2 = randomizer.NextDouble() + 2;
            double rand3 = randomizer.NextDouble() + 5;

            float offset = screenHeight * (float)(0.6);
            float peakheight = 80;
            float flatness = 70;

            for (int x = 0; x < screenWidth; x++)
            {
                double height = peakheight / rand1 * Math.Sin((float)x / flatness * rand1 + rand1);
                height += peakheight / rand2 * Math.Sin((float)x / flatness * rand2 + rand2);
                height += peakheight / rand3 * Math.Sin((float)x / flatness * rand3 + rand3);
                height += offset;
                terrainContour[x] = 250; //(int)height;
            }
        }

        private void CreateForeground()
        {
            Color[] foregroundColors = new Color[screenWidth * screenHeight];
            Color[,] groundColors = TextureTo2DArray(groundTexture);

            for (int x = 0; x < screenWidth; x++)
            {
                for (int y = 0; y < screenHeight; y++)
                {
                    if (y > terrainContour[x])
                        foregroundColors[x + y * screenWidth] = groundColors[x % groundTexture.Width, y % groundTexture.Height];
                    else
                        foregroundColors[x + y * screenWidth] = Color.Transparent;
                }
            }

            foregroundTexture = new Texture2D(device, screenWidth, screenHeight, false, SurfaceFormat.Color);
            foregroundTexture.SetData(foregroundColors);
            foregroundColorArray = TextureTo2DArray(foregroundTexture);
        }

        private Color[,] TextureTo2DArray(Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);
            Color[,] colors2D = new Color[texture.Width, texture.Height];
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    colors2D[x, y] = colors1D[x + y * texture.Width];

            return colors2D;
        }

        private Vector2 TexturesCollide(Color[,] tex1, Matrix mat1, Color[,] tex2, Matrix mat2)
        {
            Matrix mat1to2 = mat1 * Matrix.Invert(mat2);
            int width1 = tex1.GetLength(0);
            int height1 = tex1.GetLength(1);
            int width2 = tex2.GetLength(0);
            int height2 = tex2.GetLength(1);

            for (int x1 = 0; x1 < width1; x1++)
            {
                for (int y1 = 0; y1 < height1; y1++)
                {
                    Vector2 pos1 = new Vector2(x1, y1);
                    Vector2 pos2 = Vector2.Transform(pos1, mat1to2);

                    int x2 = (int)pos2.X;
                    int y2 = (int)pos2.Y;
                    if ((x2 >= 0) && (x2 < width2))
                    {
                        if ((y2 >= 0) && (y2 < height2))
                        {
                            if (tex1[x1, y1].A > 0)
                            {
                                if (tex2[x2, y2].A > 0)
                                {
                                    Vector2 screenPos = Vector2.Transform(pos1, mat1);
                                    return screenPos;
                                }
                            }
                        }
                    }
                }
            }

            return new Vector2(-1, -1);
        }

        private Vector2 CheckTerrainCollision()
        {
            Matrix rocketMat = Matrix.CreateTranslation(-42, -240, 0) * Matrix.CreateRotationZ(rocketAngle) * Matrix.CreateScale(rocketScaling) * Matrix.CreateTranslation(rocketPosition.X, rocketPosition.Y, 0);
            Matrix terrainMat = Matrix.Identity;
            Vector2 terrainCollisionPoint = TexturesCollide(rocketColorArray, rocketMat, foregroundColorArray, terrainMat);
            return terrainCollisionPoint;
        }

        private Vector2 CheckPlayersCollision()
        {
            Matrix rocketMat = Matrix.CreateTranslation(-42, -240, 0) * Matrix.CreateRotationZ(rocketAngle) * Matrix.CreateScale(rocketScaling) * Matrix.CreateTranslation(rocketPosition.X, rocketPosition.Y, 0);
            foreach (Peer player in Game1.main_console.Room.Members)
            {
                if (player.IsAlive)
                {
                    if (player != currentPlayer)
                    {
                        int xPos = (int) player.Position.X;
                        int yPos = (int) player.Position.Y;


                        Matrix carriageMat = Matrix.CreateTranslation(0, -player.CarriageTexture.Height, 0) * Matrix.CreateScale(playerScaling) * Matrix.CreateTranslation(xPos, yPos, 0);
                        Vector2 carriageCollisionPoint = TexturesCollide(carriageColorArray, carriageMat, rocketColorArray, rocketMat);
                        if (carriageCollisionPoint.X > -1)
                        {
                            player.Health -= currentPlayer.Power / 3;
                            if (player.Health <= 0)
                            {
                                player.IsAlive = false;
                                numberOfPlayerAlive -= 1;
                            }
                            return carriageCollisionPoint;
                        }

                        //Matrix cannonMat = Matrix.CreateTranslation(-11, -50, 0) * Matrix.CreateRotationZ(player.Angle) * Matrix.CreateScale(playerScaling) * Matrix.CreateTranslation(xPos + 20, yPos - 10, 0);
                        //Vector2 cannonCollisionPoint = TexturesCollide(cannonColorArray, cannonMat, rocketColorArray, rocketMat);
                        //if (cannonCollisionPoint.X > -1)
                        //{
                        //    players[i].Health -= players[currentPlayer].Power;
                        //    if (players[i].Health == 0)
                        //    {
                        //        players[i].IsAlive = false;
                        //    }
                        //    return cannonCollisionPoint;
                        //}
                    }
                }
            }
            return new Vector2(-1, -1);
        }

        private bool CheckOutOfScreen()
        // check whether the rocket is still inside the window
        {
            bool rocketOutOfScreen = rocketPosition.Y > screenHeight;
            rocketOutOfScreen |= rocketPosition.X < 0;
            rocketOutOfScreen |= rocketPosition.X > screenWidth;

            return rocketOutOfScreen;
        }

        private void CheckCollisions(GameTime gameTime)
        {
            Vector2 terrainCollisionPoint = CheckTerrainCollision();
            Vector2 playerCollisionPoint = CheckPlayersCollision();
            bool rocketOutOfScreen = CheckOutOfScreen();

            if (playerCollisionPoint.X > -1)
            {
                rocketFlying = false;

                smokeList = new List<Vector2>();
                NextPlayer();
            }

            if (terrainCollisionPoint.X > -1)
            {
                rocketFlying = false;

                smokeList = new List<Vector2>();
                NextPlayer();
            }

            if (rocketOutOfScreen)
            {
                rocketFlying = false;

                smokeList = new List<Vector2>();
                NextPlayer();
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {            
            // Allows the game to exit
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            //    this.Exit();

            //// TODO: Add your update logic here
            clock += gameTime.ElapsedGameTime;
            if (clock.TotalSeconds >= 20)
            {                
                NextPlayer();
            }

            if (currentPlayer.PeerId == Game1.main_console.PeerId)
            {
                ProcessKeyboard();
            }
            if (rocketFlying)
            {
                UpdateRocket();
                CheckCollisions(gameTime);
            }

            MouseState mouseState;

            switch (gamestate)
            {
                case GameState.Play:
                    if (teamA.FindIndex(fpeer => fpeer.IsAlive == true) == -1 || teamB.FindIndex(fpeer => fpeer.IsAlive == true) == -1)
                    {
                        gamestate = GameState.GameOver;
                    }
                    break;
                case GameState.GameOver:
                    mouseState = Mouse.GetState();
                    System.Console.WriteLine("pencet" + mouseState.X + ", " + mouseState.Y);
                    if (mouseState.X > 300 && mouseState.X < 500 && mouseState.Y < 400 && mouseState.Y > 250 && mouseState.LeftButton == ButtonState.Pressed && prev.LeftButton == ButtonState.Released)
                    {
                        //gamestate = GameState.BackToRoom;                        
                        DrawableGameState state = new RoomState(previousState, gameStateService, guiService, inputService, graphics, Content);
                        gameStateService.Switch(state);
                    }
                    break;
                case GameState.BackToRoom:

                    break;
            }

            prev = Mouse.GetState();
            //if(clock%20 == 0)
            //{
            //    Game1.main_console.GameEvent += ProcessMessages;
            //    if (clock == 480)
            //    {
            //        clock = 1;
            //    }
            //}       
        }

		private void ProcessMessages(Message msg)
        {
            Logger.WriteLine("MAAAAAAAAASSSSSSSSSUU");
            float xPos; // x position
            float yPos; // y position
            float power; // power
            float angle; // angle
            float life; // damage

            bool isRocketFlying;
            int PeerID;
            msg.GetMessageGame(out xPos, out yPos, out angle, out power, out life, out isRocketFlying, out PeerID);

            currentPlayer.Position.X = xPos;
            currentPlayer.Position.Y = yPos;
            currentPlayer.Power = power;
            currentPlayer.Angle = angle;
            rocketFlying = isRocketFlying;
            //rocketDamage = damage;
            currentPlayer.Health = life;

            if (isRocketFlying)
            {
                rocketPosition = currentPlayer.Position;
                rocketPosition.X += 20;
                rocketPosition.Y -= 10;
                rocketAngle = currentPlayer.Angle;
                Vector2 up = new Vector2(0, -1);
                Matrix rotMatrix = Matrix.CreateRotationZ(rocketAngle);
                rocketDirection = Vector2.Transform(up, rotMatrix);
                rocketDirection *= currentPlayer.Power / 50.0f;
            }            

            Logger.WriteLine("-----------------");
            Logger.WriteLine("players" + currentPlayer + PeerID);
            Logger.WriteLine(currentPlayer.Position.X);
            Logger.WriteLine(currentPlayer.Position.Y);
            Logger.WriteLine(currentPlayer.Power);
            Logger.WriteLine(currentPlayer.Angle);
            Logger.WriteLine("Rocket status:" + isRocketFlying);
            Logger.WriteLine(currentPlayer.Health);
            Logger.WriteLine("-----------------");
        }

		private void SendMsgDefault(bool isFlying)
        {
            ProcessMessages(Game1.main_console.SendGame(
                currentPlayer.Position.X,
                currentPlayer.Position.Y,
                currentPlayer.Angle,
                currentPlayer.Power,
                currentPlayer.Health,
                isFlying,
                currentPlayer.PeerId));
        }

        private void UpdateRocket()
        {
            if (rocketFlying)
            {
                // update rocket
                Vector2 gravity = new Vector2(0, 1);
                rocketDirection += gravity / 10.0f;
                rocketPosition += rocketDirection;
                rocketAngle = (float) Math.Atan2(rocketDirection.X, -rocketDirection.Y);

                // update smoke trails
                for (int i = 0; i < 5; i++)
                {
                    Vector2 smokePos = rocketPosition;
                    smokePos.X += randomizer.Next(10) - 5;
                    smokePos.Y += randomizer.Next(10) - 5;
                    smokeList.Add(smokePos);
                }
            }
        }

        private void NextPlayer()
        // increment the currentPlayer value
        {
            if(teamA.FindIndex(fpeer => fpeer.IsAlive == true) == -1)
            {
                return;
            }

            if (teamB.FindIndex(fpeer => fpeer.IsAlive == true) == -1)
            {
                return;
            }

            if (teamA.FindIndex(fpeer => fpeer == currentPlayer) >= 0)
            {
                do
                {
                    idxB = (idxB + 1) % teamB.Count;
                    currentPlayer = teamB[idxB];
                    if (currentPlayer.IsAlive)
                    {
                        break;
                    } 
                }
                while (!currentPlayer.IsAlive);
            }
            else if (teamB.FindIndex(fpeer => fpeer == currentPlayer) >= 0)
            {
                do
                {
                    idxA = (idxA + 1) % teamA.Count;
                    currentPlayer = teamA[idxA];
                    if (currentPlayer.IsAlive)
                    {
                        break;
                    } 
                }
                while (!currentPlayer.IsAlive);
            }
            clock = TimeSpan.Zero;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {            
            // TODO: Add your drawing code here

            // starting the spriteBatch first before drawing images
            if (gamestate == GameState.Play)
            {
                spriteBatch.Begin();
                DrawScenery();
                DrawCannon();
                DrawPlayers();
                DrawText();
                DrawRocket();
                DrawSmoke();
                spriteBatch.End();
            }
            
            else if (gamestate == GameState.GameOver)
            {
                spriteBatch.Begin();
                DrawScenery();
                DrawExitGame();
                spriteBatch.End();
            }            
        }

        private void DrawScenery()
        {
            // draw a rectangle that covers the entire window, which
            // upper-left corner in the (0,0) pxl, having width & height of the screen:
            Rectangle screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);

            // ask the spritebatch to draw the image
            // draw the backgroundTexture & foregroundTexture, spanning the entire window
            spriteBatch.Draw(backgroundTexture, screenRectangle, Color.White);
            spriteBatch.Draw(foregroundTexture, screenRectangle, Color.White);
        }

        private void DrawPlayers()
        {
            // for each of our players, check if its still alive. If it is,
            // draw the carriage texture at the players position
            foreach (Peer player in Game1.main_console.Room.Members)
            {
                if (player.IsAlive)
                {
                    int xPos = (int)player.Position.X;
                    int yPos = (int)player.Position.Y;
                    Vector2 cannonOrigin = new Vector2(11, 60);

                    spriteBatch.Draw(player.CarriageTexture, player.Position, null, Color.White, 0, new Vector2(0, player.CarriageTexture.Height), playerScaling, SpriteEffects.None, 0);
                    //spriteBatch.Draw(cannonTexture, new Vector2(xPos + 25, yPos - 20), null, Color.Red, player.Angle, cannonOrigin, 0.7f, SpriteEffects.None, 1);
                }
            }
        }

        private void DrawCannon()
        {
            Peer player = currentPlayer;
            int xPos = (int)player.Position.X;
            int yPos = (int)player.Position.Y;
            Vector2 cannonOrigin = new Vector2(11, 60);
            spriteBatch.Draw(cannonTexture, new Vector2(xPos + 25, yPos - 20), null, Color.Red, player.Angle, cannonOrigin, 0.7f, SpriteEffects.None, 1);
        }

        private void DrawText()
        {
            Peer player = currentPlayer;
            int currentAngle = (int)MathHelper.ToDegrees(player.Angle);

            spriteBatch.DrawString(font, "Cannon angle: " + currentAngle.ToString(), new Vector2(20, 20), Color.Black);
            spriteBatch.DrawString(font, "Cannon power: " + player.Power.ToString(), new Vector2(20, 45), Color.White);
            spriteBatch.DrawString(font, "Life: " + player.Health.ToString(), new Vector2(20, 65), Color.Black);
            spriteBatch.DrawString(font, "Time remaining: " + (20 - clock.TotalSeconds), new Vector2(300, 20), Color.Black);
        }

        private void DrawRocket()
        {
            if (rocketFlying)
            {
                spriteBatch.Draw(rocketTexture, rocketPosition, null, Color.White, rocketAngle, new Vector2(42, 240), 0.1f, SpriteEffects.None, 1);
            }
        }

        private void DrawSmoke()
        {
            foreach (Vector2 smokePos in smokeList)
                spriteBatch.Draw(smokeTexture, smokePos, null, Color.White, 0, new Vector2(40, 35), 0.2f, SpriteEffects.None, 1);
        }

        private void DrawExitGame()
        {
            spriteBatch.Draw(exitTexture, new Vector2(300,300), Color.White);
        }

        private void ProcessKeyboard()
        {
            //SendMsgDefault();
            KeyboardState keybState = Keyboard.GetState();

            //float tempPosX = players[currentPlayer].Position.X;
            //float tempPosY = players[currentPlayer].Position.Y;
            //float tempAngle = players[currentPlayer].Angle;
            //float tempPower = players[currentPlayer].Power;
            // menaikkan power dengan huruf W
            // menurunkan power dengan huruf Q
            if (keybState.IsKeyDown(Keys.Q))
            {
                currentPlayer.Power -= 1f;
                SendMsgDefault(false);
            }

            if (keybState.IsKeyDown(Keys.W))
            {
                currentPlayer.Power += 1f;
                SendMsgDefault(false);
            }
            // menaikkan angle dengan up arrow
            // mennurunkan angle dengan down arrow
            if (keybState.IsKeyDown(Keys.Down))
            {
                currentPlayer.Angle -= 0.01f;
                if (currentPlayer.Angle > MathHelper.PiOver2)
                    currentPlayer.Angle = -MathHelper.PiOver2;
                if (currentPlayer.Angle < -MathHelper.PiOver2)
                    currentPlayer.Angle = MathHelper.PiOver2;

                SendMsgDefault(false);
            }

            if (keybState.IsKeyDown(Keys.Up))
            {
                currentPlayer.Angle += 0.01f;
                if (currentPlayer.Angle > MathHelper.PiOver2)
                    currentPlayer.Angle = -MathHelper.PiOver2;
                if (currentPlayer.Angle < -MathHelper.PiOver2)
                    currentPlayer.Angle = MathHelper.PiOver2;

                SendMsgDefault(false);
            }
            // menggerakkan karakter ke kiri dan kanan dengan left-right arrow
            if (keybState.IsKeyDown(Keys.Left))
            {
                currentPlayer.Position.X -= 1f;

                SendMsgDefault(false);
            }
            if (keybState.IsKeyDown(Keys.Right))
            {
                currentPlayer.Position.X += 1f;

                SendMsgDefault(false);
            }

            if (keybState.IsKeyDown(Keys.PageDown))
            {
                currentPlayer.Power -= 20;
                if (currentPlayer.Power > 500)
                    currentPlayer.Power = 500;
                if (currentPlayer.Power < 0)
                    currentPlayer.Power = 0;
                SendMsgDefault(false);
            }
            if (keybState.IsKeyDown(Keys.PageUp))
            {
                currentPlayer.Power += 20;
                if (currentPlayer.Power > 500)
                    currentPlayer.Power = 500;
                if (currentPlayer.Power < 0)
                    currentPlayer.Power = 0;

                SendMsgDefault(false);
            }
            
            if (keybState.IsKeyDown(Keys.Enter) || keybState.IsKeyDown(Keys.Space))
            {                
                if (rocketFlying != true)
                {
                    rocketFlying = true;
                    SendMsgDefault(rocketFlying);
                }                
            }
        }        
    }
}
