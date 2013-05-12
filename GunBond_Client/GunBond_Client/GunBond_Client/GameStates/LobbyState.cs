using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
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
    class LobbyState : DrawableGameState
    {
        private IGameStateService gameStateService;
        private IGuiService guiService;
        private IInputService inputService;
        private GraphicsDeviceManager graphics;
        private ContentManager content;
        private SpriteBatch spriteBatch;

        private String username;
        private int page;

        private Texture2D background;
        private Screen lobbyScreen;
        private Song backgroundMusic;

        private Texture2D avatar;
        private Texture2D mainPanel;
        private Texture2D userPanel;
        private Texture2D roomPanel;
        private List<LabelControl> roomIDLabels;
        private List<LabelControl> roomCapLabels;
        private List<ButtonControl> joinButtons;
        private List<bool> panelVisibility;
        private List<EventHandler> joinEventHandlers;

        private MouseMoveDelegate mouseMove;
        List<Message.MessageRoomBody> allRoom;

        public LobbyState(IGameStateService gameStateService, IGuiService guiService,
                        IInputService inputService, GraphicsDeviceManager graphics, 
                        ContentManager content, String username)
        {
            allRoom = Game1.main_console.ListRooms();
            
            this.gameStateService = gameStateService;
            this.guiService = guiService;
            this.inputService = inputService;
            this.graphics = graphics;
            this.content = content;
            
            this.username = username;
            this.page = 0;

            roomIDLabels = new List<LabelControl>();
            roomCapLabels = new List<LabelControl>();
            joinButtons = new List<ButtonControl>();
            panelVisibility = new List<bool>();
            joinEventHandlers = new List<EventHandler>();

            this.mouseMove = new MouseMoveDelegate(mouseMoved);
   
            lobbyScreen = new Screen(1194, 692);
            /*mainMenuScreen.Desktop.Bounds = new UniRectangle(
              new UniScalar(0.1f, 0.0f), new UniScalar(0.1f, 0.0f), // x and y = 10%
              new UniScalar(0.8f, 0.0f), new UniScalar(0.8f, 0.0f) // width and height = 80%
            );*/

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            LoadContent(lobbyScreen, content);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.Draw(mainPanel, new Rectangle(12, 12, 869, 519), Color.White * 0.5f);
            spriteBatch.Draw(userPanel, new Rectangle(900, 12, 282, 519), Color.White * 0.5f);
            spriteBatch.End();

            spriteBatch.Begin();
            if (panelVisibility[0])
            {
                spriteBatch.Draw(roomPanel, new Rectangle(22, 29, 420, 73), Color.White);
            }
            if (panelVisibility[1])
            {
                spriteBatch.Draw(roomPanel, new Rectangle(448, 29, 420, 73), Color.White);
            }
            if (panelVisibility[2])
            {
                spriteBatch.Draw(roomPanel, new Rectangle(22, 131, 420, 73), Color.White);
            }
            if (panelVisibility[3])
            {
                spriteBatch.Draw(roomPanel, new Rectangle(448, 131, 420, 73), Color.White);
            }
            if (panelVisibility[4])
            {
                spriteBatch.Draw(roomPanel, new Rectangle(22, 233, 420, 73), Color.White);
            }
            if (panelVisibility[5])
            {
                spriteBatch.Draw(roomPanel, new Rectangle(448, 233, 420, 73), Color.White);
            }
            if (panelVisibility[6])
            {
                spriteBatch.Draw(roomPanel, new Rectangle(22, 335, 420, 73), Color.White);
            }
            if (panelVisibility[7])
            {
                spriteBatch.Draw(roomPanel, new Rectangle(448, 335, 420, 73), Color.White);
            }
            if (panelVisibility[8])
            {
                spriteBatch.Draw(roomPanel, new Rectangle(22, 437, 420, 73), Color.White);
            }
            if (panelVisibility[9])
            {
                spriteBatch.Draw(roomPanel, new Rectangle(448, 437, 420, 73), Color.White);
            }
            spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.Draw(avatar, new Rectangle(942, 55, 200, 200), Color.White);
            spriteBatch.End();
        }

        protected override void OnEntered()
        {
            base.OnEntered();

            guiService.Screen = lobbyScreen;

            graphics.PreferredBackBufferWidth = (int)guiService.Screen.Width;
            graphics.PreferredBackBufferHeight = (int)guiService.Screen.Height;
            graphics.ApplyChanges();

            Game1.music = backgroundMusic;

            inputService.GetMouse().MouseMoved += mouseMove;

            Refresh();
        }

        protected override void OnLeaving()
        {
            base.OnLeaving();
            inputService.GetMouse().MouseMoved -= mouseMove;
        }

        private void Refresh()
        {
            allRoom = Game1.main_console.ListRooms();
            for (int x = 0; x < 10; ++x)
            {
                lobbyScreen.Desktop.Children.Remove(roomIDLabels[x]);
                lobbyScreen.Desktop.Children.Remove(roomCapLabels[x]);
                lobbyScreen.Desktop.Children.Remove(joinButtons[x]);
                joinButtons[x].Pressed -= joinEventHandlers[x];
                panelVisibility[x] = false;
            }


            int i = page * 10;
            while ((i < (page + 1) * 10) && (i < allRoom.Count))
            {
                panelVisibility[i] = true;
                roomIDLabels[i].Text = allRoom[i].roomId;
                roomCapLabels[i].Text = "" + allRoom[i].currentPlayer + " / " + allRoom[i].maxPlayers;

                lobbyScreen.Desktop.Children.Add(roomIDLabels[i]);
                lobbyScreen.Desktop.Children.Add(roomCapLabels[i]);
                lobbyScreen.Desktop.Children.Add(joinButtons[i]);

                joinButtons[i].Pressed += joinEventHandlers[i];
                i++;
            }

            
            
            //while ((i < (page + 1) * 10) && (i < rooms.Count))
            //{
            //    panelVisibility[i] = true;
            //    roomIDLabels[i].Text = "Room :" + rooms[i].ToLanguageString();
            //    roomCapLabels[i].Text = "" + rooms[i].CURRENT_PLAYER + " / " + rooms[i].MAX_PLAYER;

            //    lobbyScreen.Desktop.Children.Add(roomIDLabels[i]);
            //    lobbyScreen.Desktop.Children.Add(roomCapLabels[i]);
            //    lobbyScreen.Desktop.Children.Add(joinButtons[i]);

            //    joinButtons[i].Pressed += joinEventHandlers[i];

            //    i++;
            //}
        }

        private void LoadContent(Screen mainScreen, ContentManager content)
        {
            background = content.Load<Texture2D>("Images\\Lobby\\background1");
            backgroundMusic = content.Load<Song>("Music\\03 Gunbound- Now Loading");

            avatar = content.Load<Texture2D>("Images\\Lobby\\Gunbound_dragon");
            int width = 869;
            int height = 519;
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
            for (int i = (width * (height-1)); i < (width * height); ++i)
            {
                data[i] = Color.Black;
            }
            mainPanel = new Texture2D(graphics.GraphicsDevice, width, height);
            mainPanel.SetData(data);

            width = 282;
            height = 519;
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
                    data[i] = Color.White;
                }
            }
            for (int i = (width * (height - 1)); i < (width * height); ++i)
            {
                data[i] = Color.Black;
            }
            userPanel = new Texture2D(graphics.GraphicsDevice, width, height);
            userPanel.SetData(data);

            roomPanel = content.Load<Texture2D>("Images\\Lobby\\roombox");
            width = roomPanel.Width;
            height = roomPanel.Height;
            data = new Color[width * height];
            roomPanel.GetData(data);
            for (int i = 0; i < width; ++i)
            {
                data[i] = Color.Black;
            }
            for (int i = width; i < (width * (height - 1)); i += width)
            {
                data[i] = Color.Black;
                data[i + (width - 1)] = Color.Gray;
            }
            for (int i = (width * (height - 1)); i < (width * height); ++i)
            {
                data[i] = Color.Gray;
            }
            roomPanel.SetData(data);

            Texture2D buttonBackGround = content.Load<Texture2D>("Images\\Lobby\\buttes2");
            int j = 0;
            for (int i = 0; i < 10; i+=2)
            {
                LabelControl roomIDLeftLabel = new LabelControl("Test dulu");
                roomIDLeftLabel.Bounds = new UniRectangle(25, 32 + 102 * j, 55, 16);
                roomIDLeftLabel.Name = "Label Room ID " + (i + 1);

                LabelControl roomCapLeftLabel = new LabelControl("Sekian / sekian");
                roomCapLeftLabel.Bounds = new UniRectangle(25, 50 + 102 * j, 55, 16);
                roomCapLeftLabel.Name = "Label Room Cap " + (i + 1);

                ButtonControl joinLeftButton = new ButtonControl();
                joinLeftButton.Text = "JOIN";
                joinLeftButton.Bounds = new UniRectangle(334, 67 + 102 * j, 101, 28);
                joinLeftButton.Name = "Join Button " + (i + 1);
                joinLeftButton.imageTexture = buttonBackGround;

                LabelControl roomIDRightLabel = new LabelControl("Test dulu");
                roomIDRightLabel.Bounds = new UniRectangle(451, 32 + 102 * j, 55, 16);
                roomIDRightLabel.Name = "Label Room ID " + (i + 2);

                LabelControl roomCapRightLabel = new LabelControl("Sekian / sekian");
                roomCapRightLabel.Bounds = new UniRectangle(451, 50 + 102 * j, 55, 16);
                roomCapRightLabel.Name = "Label Room Cap " + (i + 2);

                ButtonControl joinRightButton = new ButtonControl();
                joinRightButton.Text = "JOIN";
                joinRightButton.Bounds = new UniRectangle(760, 67 + 102 * j, 101, 28);
                joinRightButton.Name = "Join Button " + (i + 2);
                joinRightButton.imageTexture = buttonBackGround;

                roomIDLabels.Add(roomIDLeftLabel);
                roomCapLabels.Add(roomCapLeftLabel);
                joinButtons.Add(joinLeftButton);
                panelVisibility.Add(false);
                joinEventHandlers.Add(new EventHandler(joinRoomPressed));
                roomIDLabels.Add(roomIDRightLabel);
                roomCapLabels.Add(roomCapRightLabel);
                joinButtons.Add(joinRightButton);
                panelVisibility.Add(false);
                joinEventHandlers.Add(new EventHandler(joinRoomPressed));

                /*mainScreen.Desktop.Children.Add(roomIDLeftLabel);
                mainScreen.Desktop.Children.Add(roomCapLeftLabel);
                mainScreen.Desktop.Children.Add(joinLeftButton);
                mainScreen.Desktop.Children.Add(roomIDRightLabel);
                mainScreen.Desktop.Children.Add(roomCapRightLabel);
                mainScreen.Desktop.Children.Add(joinRightButton);*/

                j++;
            }

            LabelControl usernameLabel = new LabelControl(username);
            usernameLabel.Bounds = new UniRectangle(938, 20, 157, 32);
            usernameLabel.Name = "Label Username";

            ButtonControl createRoomButton = new ButtonControl();
            createRoomButton.Bounds = new UniRectangle(15, 537, 151, 151);
            createRoomButton.Name = "Create Room Button";
            createRoomButton.imageTexture = content.Load<Texture2D>("Images\\Lobby\\CreateRoom");
            createRoomButton.imageHover = content.Load<Texture2D>("Images\\Lobby\\CreateRoomHover");
            createRoomButton.Pressed += new EventHandler(createRoomPressed);

            ButtonControl refreshButton = new ButtonControl();
            refreshButton.Bounds = new UniRectangle(200, 537, 151, 151);
            refreshButton.Name = "Refresh Button";
            refreshButton.imageTexture = content.Load<Texture2D>("Images\\Lobby\\Refresh");
            refreshButton.imageHover = content.Load<Texture2D>("Images\\Lobby\\RefreshHover");
            refreshButton.Pressed += new EventHandler(refreshPressed);

            ButtonControl leftPageButton = new ButtonControl();
            leftPageButton.Bounds = new UniRectangle(622, 537, 151, 151);
            leftPageButton.Name = "Left Page Button";
            leftPageButton.imageTexture = content.Load<Texture2D>("Images\\Lobby\\LeftPage");
            leftPageButton.imageHover = content.Load<Texture2D>("Images\\Lobby\\LeftPageHover");
            leftPageButton.Pressed += new EventHandler(leftPagePressed);

            ButtonControl rightPageButton = new ButtonControl();
            rightPageButton.Bounds = new UniRectangle(781, 537, 151, 151);
            rightPageButton.Name = "Right Page Button";
            rightPageButton.imageTexture = content.Load<Texture2D>("Images\\Lobby\\RightPage");
            rightPageButton.imageHover = content.Load<Texture2D>("Images\\Lobby\\RightPageHover");
            rightPageButton.Pressed += new EventHandler(rightPagePressed);

            ButtonControl exitGameButton = new ButtonControl();
            exitGameButton.Bounds = new UniRectangle(1026, 537, 151, 151);
            exitGameButton.Name = "Exit Button";
            exitGameButton.imageTexture = content.Load<Texture2D>("Images\\Lobby\\Exit");
            exitGameButton.imageHover = content.Load<Texture2D>("Images\\Lobby\\ExitHover");
            exitGameButton.Pressed += new EventHandler(exitPressed);

            mainScreen.Desktop.Children.Add(usernameLabel);
            mainScreen.Desktop.Children.Add(createRoomButton);
            mainScreen.Desktop.Children.Add(refreshButton);
            mainScreen.Desktop.Children.Add(leftPageButton);
            mainScreen.Desktop.Children.Add(rightPageButton);
            mainScreen.Desktop.Children.Add(exitGameButton);
        }

        private void joinRoomPressed(Object obj, EventArgs args)
        {
            int i = 0;
            bool check = true;
            while ((check) && (i < joinButtons.Count))
            {
                if (joinButtons[i].Equals(obj))
                    check = false;
                ++i;
            }
            --i;

            if (Game1.main_console.JoinRoom(roomIDLabels[i].Text))
            {
                gameStateService.Switch(new RoomState(this, gameStateService, guiService, inputService, graphics, content));
            }
            else
            {
                Game1.MessageBox(new IntPtr(0), "Fail to join room.", "[ERROR] Join", 0);
            }
        }

        private void createRoomPressed(Object obj, EventArgs args)
        {
            gameStateService.Switch(new CreateRoomState(gameStateService, guiService, inputService, graphics, content));
        }

        private void refreshPressed(Object obj, EventArgs args)
        {
            Refresh();
        }

        private void leftPagePressed(Object obj, EventArgs args)
        {
            List<Message.MessageRoomBody> allRoom = Game1.main_console.ListRooms();
            if (page > 0)
            {
                page --;
            }
            if ((page - 1) * 10 > allRoom.Count)
            {
                page = 0;
            }
            Refresh();
        }

        private void rightPagePressed(Object obj, EventArgs args)
        {
            Game1.main_console.ListRooms();
            if (page < (int)(Game1.main_console.Room.Members.Count/10))
            {
                page++;
            }
            Refresh();
        }

        private void exitPressed(Object obj, EventArgs args)
        {
            gameStateService.Pop();
            Game1.quit = true;
        }

        private void mouseMoved(float x, float y)
        {
            if (((x >= 15) && (x <= 165)) && (y >= 537) && (y <= 688))
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_link.cur")
                {
                    // move to button create room
                    Game1.cursorPath = @"Content\Mouse\aero_link.cur";
                    Game1.cursorTrigger = true;
                }
            }
            else if (((x >= 200) && (x <= 351)) && (y >= 537) && (y <= 688))
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_link.cur")
                {
                    // move to button refresh
                    Game1.cursorPath = @"Content\Mouse\aero_link.cur";
                    Game1.cursorTrigger = true;
                }
            }
            else if (((x >= 622) && (x <= 773)) && (y >= 537) && (y <= 688))
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_link.cur")
                {
                    // move to button left page
                    Game1.cursorPath = @"Content\Mouse\aero_link.cur";
                    Game1.cursorTrigger = true;
                }
            }
            else if (((x >= 781) && (x <= 932)) && (y >= 537) && (y <= 688))
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_link.cur")
                {
                    // move to button right page
                    Game1.cursorPath = @"Content\Mouse\aero_link.cur";
                    Game1.cursorTrigger = true;
                }
            }
            else if (((x >= 1026) && (x <= 1177)) && (y >= 537) && (y <= 688))
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
    }
}
