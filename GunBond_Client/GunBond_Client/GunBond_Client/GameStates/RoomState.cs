using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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

using Gunbond_Client.Model;
using Gunbond;

namespace GunBond_Client.GameStates
{
    class RoomState : DrawableGameState
    {
        private IGameStateService gameStateService;
        private IGuiService guiService;
        private IInputService inputService;
        private GraphicsDeviceManager graphics;
        private ContentManager content;
        private SpriteBatch spriteBatch;

        private Texture2D background;
        private SpriteFont headerTeam;
        private Screen roomScreen;

        private IGameState previousState;
        private MouseMoveDelegate mouseMove;

        private Texture2D teamPanel;
        private Texture2D playerPanel;
        private List<LabelControl> playerIDLabels;
        private List<bool> panelVisibility;

        public bool isStart = false;

        List<Peer> teamA;
        List<Peer> teamB;


        public RoomState(IGameState previousState, IGameStateService gameStateService, IGuiService guiService,
                        IInputService inputService, GraphicsDeviceManager graphics,
                        ContentManager content)
        {
            this.gameStateService = gameStateService;
            this.guiService = guiService;
            this.inputService = inputService;
            this.graphics = graphics;
            this.content = content;

            this.previousState = previousState;
            this.mouseMove = new MouseMoveDelegate(mouseMoved);

            this.teamA = new List<Peer>();
            this.teamB = new List<Peer>();

            playerIDLabels = new List<LabelControl>();
            panelVisibility = new List<bool>();
            roomScreen = new Screen(1184, 682);

            Game1.main_console.StartEvent += Start;

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            LoadContent(roomScreen, content);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (isStart)
            {

                DrawableGameState state = new GameStart(gameStateService, guiService, inputService, graphics, content, teamA, teamB);
                gameStateService.Switch(state);
                Game1.main_console.StartEvent -= Start;
            }

            for (int x = 0; x < 8; ++x)
            {
                roomScreen.Desktop.Children.Remove(playerIDLabels[x]);
                panelVisibility[x] = false;
            }
            if (Game1.main_console.Room == null)
            {
                return;
            }
            
            int i = 0;
            while (i < Game1.main_console.Room.Members.Count)
            {
                panelVisibility[i] = true;
                playerIDLabels[i].Text = Game1.main_console.Room.Members[i].PeerId.ToString();
                roomScreen.Desktop.Children.Add(playerIDLabels[i]);
                i++;
            }
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.Draw(teamPanel, new Rectangle(13, 13, 315, 591), Color.White * 0.5f);
            spriteBatch.Draw(teamPanel, new Rectangle(443, 13, 315, 591), Color.White * 0.5f);
            spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.DrawString(headerTeam, "Team A", new Vector2(109, 22), Color.Black);
            spriteBatch.DrawString(headerTeam, "Team B", new Vector2(539, 22), Color.Black);
            if (panelVisibility[0])
            {
                spriteBatch.Draw(playerPanel, new Rectangle(17, 68, 308, 123), Color.White);
            }
            if (panelVisibility[1])
            {
                spriteBatch.Draw(playerPanel, new Rectangle(447, 68, 308, 123), Color.White);
            }
            if (panelVisibility[2])
            {
                spriteBatch.Draw(playerPanel, new Rectangle(17, 197, 308, 123), Color.White);
            }
            if (panelVisibility[3])
            {
                spriteBatch.Draw(playerPanel, new Rectangle(447, 197, 308, 123), Color.White);
            }
            if (panelVisibility[4])
            {
                spriteBatch.Draw(playerPanel, new Rectangle(17, 326, 308, 123), Color.White);
            }
            if (panelVisibility[5])
            {
                spriteBatch.Draw(playerPanel, new Rectangle(447, 326, 308, 123), Color.White);
            }
            if (panelVisibility[6])
            {
                spriteBatch.Draw(playerPanel, new Rectangle(17, 455, 308, 123), Color.White);
            }
            if (panelVisibility[7])
            {
                spriteBatch.Draw(playerPanel, new Rectangle(447, 455, 308, 123), Color.White);
            }
            spriteBatch.End();
        }

        protected override void OnEntered()
        {
            base.OnEntered();

            guiService.Screen = roomScreen;

            graphics.PreferredBackBufferWidth = (int)guiService.Screen.Width;
            graphics.PreferredBackBufferHeight = (int)guiService.Screen.Height;
            graphics.ApplyChanges();

            inputService.GetMouse().MouseMoved += mouseMove;
        }

        protected override void OnLeaving()
        {
            base.OnLeaving();
            inputService.GetMouse().MouseMoved -= mouseMove;
        }

        private void LoadContent(Screen mainScreen, ContentManager content)
        {
            background = content.Load<Texture2D>("Images\\Room\\background2");
            headerTeam = content.Load<SpriteFont>("Images\\Room\\headerTeam");

            int width = 315;
            int height = 591;
            Color[] data = new Color[width * height];
            for (int i = 0; i < width; ++i)
            {
                data[i] = Color.Black;
            }
            for (int i = width; i < (width * (height - 1)); ++i)
            {
                if (((i % width) == 0) || ((i % width) == (width - 1)))
                {
                    data[i] = Color.Black;
                }
                else
                {
                    data[i] = Color.White;
                }
            }
            for (int i = (width * (height - 1)); i < (width * height); ++i)
            {
                data[i] = Color.Black;
            }
            teamPanel = new Texture2D(graphics.GraphicsDevice, width, height);
            teamPanel.SetData(data);

            width = 308;
            height = 123;
            data = new Color[width * height];
            for (int i = 0; i < width; ++i)
            {
                data[i] = Color.Black;
            }
            for (int i = width; i < (width * (height - 1)); ++i)
            {
                if (((i % width) == 0) || ((i % width) == (width - 1)))
                {
                    data[i] = Color.Black;
                }
                else
                {
                    data[i] = Color.Aquamarine;
                }
            }
            for (int i = (width * (height - 1)); i < (width * height); ++i)
            {
                data[i] = Color.Black;
            }
            playerPanel = new Texture2D(graphics.GraphicsDevice, width, height);
            playerPanel.SetData(data);

            int j = 0;
            for (int i = 0; i < 8; i += 2)
            {
                LabelControl playerIDLeftLabel = new LabelControl("0.0.0.0");
                playerIDLeftLabel.Bounds = new UniRectangle(21, 72 + 129 * j, 115, 18);
                playerIDLeftLabel.Name = "Label Player ID " + (i + 1);

                LabelControl playerIDRightLabel = new LabelControl("0.0.0.1");
                playerIDRightLabel.Bounds = new UniRectangle(451, 72 + 129 * j, 115, 18);
                playerIDRightLabel.Name = "Label Player ID " + (i + 2);

                playerIDLabels.Add(playerIDLeftLabel);
                panelVisibility.Add(false);
                playerIDLabels.Add(playerIDRightLabel);
                panelVisibility.Add(false);

                /*mainScreen.Desktop.Children.Add(playerIDLeftLabel);
                mainScreen.Desktop.Children.Add(playerIDRightLabel);*/

                j++;
            }

            ButtonControl leftTeamButton = new ButtonControl();
            leftTeamButton.Bounds = new UniRectangle(335, 13, 40, 40);
            leftTeamButton.Name = "Left Team Button";
            leftTeamButton.imageTexture = content.Load<Texture2D>("Images\\Room\\LeftTeam");
            leftTeamButton.Pressed += new EventHandler(leftTeamPressed);

            ButtonControl rightTeamButton = new ButtonControl();
            rightTeamButton.Bounds = new UniRectangle(397, 13, 40, 40);
            rightTeamButton.Name = "Right Team Button";
            rightTeamButton.imageTexture = content.Load<Texture2D>("Images\\Room\\RightTeam");
            rightTeamButton.Pressed += new EventHandler(rightTeamPressed);

            ButtonControl chooseVehicleButton = new ButtonControl();
            chooseVehicleButton.Bounds = new UniRectangle(14, 612, 150, 60);
            chooseVehicleButton.Name = "Choose Vehicle Button";
            chooseVehicleButton.imageTexture = content.Load<Texture2D>("Images\\Room\\ChooseVehicle");
            chooseVehicleButton.imageHover = content.Load<Texture2D>("Images\\Room\\ChooseVehicleHover");
            chooseVehicleButton.Pressed += new EventHandler(chooseVehiclePressed);

            ButtonControl readyButton = new ButtonControl();
            readyButton.Bounds = new UniRectangle(443, 612, 150, 60);
            readyButton.Name = "Ready Button";
            readyButton.imageTexture = content.Load<Texture2D>("Images\\Room\\ReadyButton");
            readyButton.imageHover = content.Load<Texture2D>("Images\\Room\\ReadyButtonHover");
            readyButton.Pressed += new EventHandler(readyPressed);

            ButtonControl startGameButton = new ButtonControl();
            startGameButton.Bounds = new UniRectangle(608, 612, 150, 60);
            startGameButton.Name = "Start Game Button";
            startGameButton.imageTexture = content.Load<Texture2D>("Images\\Room\\PlayButton");
            startGameButton.imageHover = content.Load<Texture2D>("Images\\Room\\PlayButtonHover");
            startGameButton.Pressed += new EventHandler(startGamePressed);

            ButtonControl backButton = new ButtonControl();
            backButton.Bounds = new UniRectangle(1022, 612, 150, 60);
            backButton.Name = "Back Button";
            backButton.imageTexture = content.Load<Texture2D>("Images\\Room\\BackButton");
            backButton.imageHover = content.Load<Texture2D>("Images\\Room\\BackButtonHover");
            backButton.Pressed += new EventHandler(backPressed);

            mainScreen.Desktop.Children.Add(leftTeamButton);
            mainScreen.Desktop.Children.Add(rightTeamButton);
            mainScreen.Desktop.Children.Add(chooseVehicleButton);
            mainScreen.Desktop.Children.Add(readyButton);
            mainScreen.Desktop.Children.Add(startGameButton);
            mainScreen.Desktop.Children.Add(backButton);
        }

        private void leftTeamPressed(Object obj, EventArgs args)
        {
        }

        private void rightTeamPressed(Object obj, EventArgs args)
        {
        }

        private void chooseVehiclePressed(Object obj, EventArgs args)
        {
        }

        private void readyPressed(Object obj, EventArgs args)
        {
        }

        private void startGamePressed(Object obj, EventArgs args)
        {
            Start(Game1.main_console.StartGame());

            DrawableGameState state = new GameStart(gameStateService, guiService, inputService, graphics, content, teamA, teamB);
            gameStateService.Switch(state);
            Game1.main_console.StartEvent -= Start;
        }

        private void backPressed(Object obj, EventArgs args)
        {
            if (Game1.main_console.Quit())
            {
                gameStateService.Switch(previousState);
            }
            else
            {
                Game1.MessageBox(new IntPtr(0), "Cannot exit from room.", "[ERROR] Connection", 0);
            }
        }

        private void mouseMoved(float x, float y)
        {
            if (((x >= 335) && (x <= 375)) && (y >= 13) && (y <= 53))
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_link.cur")
                {
                    // move to button left team
                    Game1.cursorPath = @"Content\Mouse\aero_link.cur";
                    Game1.cursorTrigger = true;
                }
            }
            else if (((x >= 397) && (x <= 437)) && (y >= 13) && (y <= 53))
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_link.cur")
                {
                    // move to button right team
                    Game1.cursorPath = @"Content\Mouse\aero_link.cur";
                    Game1.cursorTrigger = true;
                }
            }
            else if (((x >= 14) && (x <= 164)) && (y >= 612) && (y <= 672))
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_link.cur")
                {
                    // move to button choose vehicle
                    Game1.cursorPath = @"Content\Mouse\aero_link.cur";
                    Game1.cursorTrigger = true;
                }
            }
            else if (((x >= 443) && (x <= 593)) && (y >= 612) && (y <= 672))
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_link.cur")
                {
                    // move to button ready
                    Game1.cursorPath = @"Content\Mouse\aero_link.cur";
                    Game1.cursorTrigger = true;
                }
            }
            else if (((x >= 608) && (x <= 758)) && (y >= 612) && (y <= 672))
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_link.cur")
                {
                    // move to button start game
                    Game1.cursorPath = @"Content\Mouse\aero_link.cur";
                    Game1.cursorTrigger = true;
                }
            }
            else if (((x >= 1022) && (x <= 1172)) && (y >= 612) && (y <= 672))
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_link.cur")
                {
                    // move to button back
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

        private void Start(Message m)
        {
            isStart = true;

            int peerId; 
            String roomId;
            List<int> teamA;
            List<int> teamB;

            m.GetStart(out peerId, out roomId, out teamA, out teamB);

            foreach (var x in teamA)
            {
                this.teamA.Add(Game1.main_console.Room.Members.Find(f => f.PeerId == x));
            }

            foreach (var x in teamB)
            {
                this.teamB.Add(Game1.main_console.Room.Members.Find(f => f.PeerId == x));
            }
        }
    }
}
