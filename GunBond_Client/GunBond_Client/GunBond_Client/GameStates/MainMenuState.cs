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

namespace GunBond_Client.GameStates
{
    class MainMenuState : DrawableGameState
    {
        private IGameStateService gameStateService;
        private IGuiService guiService;
        private IInputService inputService;
        private GraphicsDeviceManager graphics;
        private ContentManager content;
        private SpriteBatch spriteBatch;

        private Texture2D background;
        private Screen mainMenuScreen;
        private Song backgroundMusic;

        private InputControl usernameInput;

        private MouseMoveDelegate mouseMove;
        private KeyDelegate keyHit;

        public MainMenuState(IGameStateService gameStateService, IGuiService guiService, 
                        IInputService inputService, GraphicsDeviceManager graphics, ContentManager content)
        {
            this.gameStateService = gameStateService;
            this.guiService = guiService;
            this.inputService = inputService;
            this.graphics = graphics;
            this.content = content;

            this.mouseMove = new MouseMoveDelegate(mouseMoved);
            this.keyHit = new KeyDelegate(keyboardEntered);

            mainMenuScreen = new Screen(349, 133);
            /*mainMenuScreen.Desktop.Bounds = new UniRectangle(
              new UniScalar(0.1f, 0.0f), new UniScalar(0.1f, 0.0f), // x and y = 10%
              new UniScalar(0.8f, 0.0f), new UniScalar(0.8f, 0.0f) // width and height = 80%
            );*/

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            LoadContent(mainMenuScreen, content);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
            spriteBatch.End();
        }

        protected override void OnEntered()
        {
            base.OnEntered();

            guiService.Screen = mainMenuScreen;

            graphics.PreferredBackBufferWidth = (int)guiService.Screen.Width;
            graphics.PreferredBackBufferHeight = (int)guiService.Screen.Height;
            graphics.ApplyChanges();

            Game1.music = backgroundMusic;
        }

        protected override void OnLeaving()
        {
            base.OnLeaving();
            MediaPlayer.Stop();
            inputService.GetMouse().MouseMoved -= mouseMove;
            inputService.GetKeyboard().KeyPressed -= keyHit;
        }

        private void LoadContent(Screen mainScreen, ContentManager content)
        {
            background = content.Load<Texture2D>("Images\\MainMenu\\background1");
            backgroundMusic = content.Load<Song>("Music\\02 Gunbound- The Lobby");

            LabelControl usernameLabel = new LabelControl("Username");
            usernameLabel.Bounds = new UniRectangle(16, 27, 83, 24);
            usernameLabel.Name = "Label Username";

            usernameInput = new InputControl();
            usernameInput.Bounds = new UniRectangle(106, 27, 216, 26);
            usernameInput.Name = "Input Username";

            ButtonControl loginGameButton = new ButtonControl();
            loginGameButton.Bounds = new UniRectangle(32, 82, 120, 35);
            loginGameButton.Name = "Login Button";
            loginGameButton.imageTexture = content.Load<Texture2D>("Images\\MainMenu\\Login");
            loginGameButton.imageHover = content.Load<Texture2D>("Images\\MainMenu\\Login-hover");
            loginGameButton.Pressed += new EventHandler(loginPressed);

            ButtonControl exitGameButton = new ButtonControl();
            exitGameButton.Bounds = new UniRectangle(185, 82, 120, 35);
            exitGameButton.Name = "Exit Button";
            exitGameButton.imageTexture = content.Load<Texture2D>("Images\\MainMenu\\exitb");
            exitGameButton.imageHover = content.Load<Texture2D>("Images\\MainMenu\\exitb-hover");
            exitGameButton.Pressed += new EventHandler(exitPressed);

            mainScreen.Desktop.Children.Add(usernameLabel);
            mainScreen.Desktop.Children.Add(usernameInput);
            mainScreen.Desktop.Children.Add(loginGameButton);
            mainScreen.Desktop.Children.Add(exitGameButton);

            inputService.GetMouse().MouseMoved += mouseMove;
            inputService.GetKeyboard().KeyPressed += keyHit;
        }

        private void login()
        {
            usernameInput.Text = usernameInput.Text.Trim();
            if (usernameInput.Text != "")
            {
                if (Game1.main_console.ConnectTracker())
                {
                    DrawableGameState state = new LobbyState(gameStateService, guiService, inputService, graphics, content, usernameInput.Text);
                    gameStateService.Switch(state);
                }
                else
                {
                    Game1.MessageBox(new IntPtr(0), "Cannot connect to tracker.", "[ERROR] Connection", 0);
                }
            }
            else
            {
                Game1.MessageBox(new IntPtr(0), "Please enter your username.", "[ERROR] Username", 0);
            }
        }

        private void loginPressed(Object obj, EventArgs args)
        {
            login();
        }

        private void exitPressed(Object obj, EventArgs args)
        {
            Game1.quit = true;
        }

        private void mouseMoved(float x, float y)
        {
            if (((x >= 106) && (x <= 322)) && (y >= 27) && (y <= 53))
            {
                if (Game1.cursorPath != @"Content\Mouse\beam_r.cur")
                {
                    // move to input username
                    Game1.cursorPath = @"Content\Mouse\beam_r.cur";
                    Game1.cursorTrigger = true;
                }
            }
            else if (((x >= 32) && (x <= 152)) && (y >= 82) && (y <= 117))
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_link.cur")
                {
                    // move to button login
                    Game1.cursorPath = @"Content\Mouse\aero_link.cur";
                    Game1.cursorTrigger = true;
                }
            }
            else if (((x >= 185) && (x <= 305)) && (y >= 82) && (y <= 117))
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_link.cur")
                {
                    // move to button exit
                    Game1.cursorPath = @"Content\Mouse\aero_link.cur";
                    Game1.cursorTrigger = true;
                }
            }
            else
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_arrow.cur")
                {
                    Game1.cursorPath = @"Content\Mouse\aero_arrow.cur";
                    Game1.cursorTrigger = true;
                }
            }
        }

        private void keyboardEntered(Keys key)
        {
            if ((usernameInput.HasFocus) && (Keys.Enter.Equals(key)))
            {
                login();
            }
        }
    }
}
