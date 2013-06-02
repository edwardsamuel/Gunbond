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
    class CreateRoomState : DrawableGameState
    {
        private IGameStateService gameStateService;
        private IGuiService guiService;
        private IInputService inputService;
        private GraphicsDeviceManager graphics;
        private ContentManager content;
        private SpriteBatch spriteBatch;

        private Texture2D background;
        private SpriteFont header;
        private Screen createRoomScreen;

        private IGameState previousState;
        private MouseMoveDelegate mouseMove;

        private InputControl roomNameInput;
        private ChoiceControl twoChoice;
        private ChoiceControl fourChoice;
        private ChoiceControl sixChoice;
        private ChoiceControl eightChoice;

        public CreateRoomState(IGameStateService gameStateService, IGuiService guiService,
                        IInputService inputService, GraphicsDeviceManager graphics, 
                        ContentManager content)
        {
            this.gameStateService = gameStateService;
            this.guiService = guiService;
            this.inputService = inputService;
            this.graphics = graphics;
            this.content = content;

            this.previousState = gameStateService.ActiveState;
            this.mouseMove = new MouseMoveDelegate(mouseMoved);

            createRoomScreen = new Screen(458, 274);
            /*mainMenuScreen.Desktop.Bounds = new UniRectangle(
              new UniScalar(0.1f, 0.0f), new UniScalar(0.1f, 0.0f), // x and y = 10%
              new UniScalar(0.8f, 0.0f), new UniScalar(0.8f, 0.0f) // width and height = 80%
            );*/

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            LoadContent(createRoomScreen, content);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
            spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.DrawString(header, "Create Room", new Vector2(111, 19), Color.Black);
            spriteBatch.End();
        }

        protected override void OnEntered()
        {
            base.OnEntered();

            guiService.Screen = createRoomScreen;

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
            background = content.Load<Texture2D>("Images\\CreateRoom\\background2");
            header = content.Load<SpriteFont>("Images\\CreateRoom\\header");

            LabelControl roomNameLabel = new LabelControl("Room Name");
            roomNameLabel.Bounds = new UniRectangle(44, 91, 98, 20);
            roomNameLabel.Name = "Label Room Name";

            roomNameInput = new InputControl();
            roomNameInput.Bounds = new UniRectangle(217, 88, 213, 26);
            roomNameInput.Name = "Input Room Name";

            LabelControl maxPlayerLabel = new LabelControl("Maimum Player");
            maxPlayerLabel.Bounds = new UniRectangle(44, 155, 123, 20);
            maxPlayerLabel.Name = "Label Maximum Player";

            twoChoice = new ChoiceControl();
            twoChoice.Name = "Choice Two Player";
            twoChoice.Text = "2";
            twoChoice.Bounds = new UniRectangle(217, 155, 35, 20);
            twoChoice.Selected = true;

            fourChoice = new ChoiceControl();
            fourChoice.Name = "Choice Four Player";
            fourChoice.Text = "4";
            fourChoice.Bounds = new UniRectangle(258, 155, 35, 20);

            sixChoice = new ChoiceControl();
            sixChoice.Name = "Choice Six Player";
            sixChoice.Text = "6";
            sixChoice.Bounds = new UniRectangle(299, 155, 35, 20);

            eightChoice = new ChoiceControl();
            eightChoice.Name = "Choice Eight Player";
            eightChoice.Text = "8";
            eightChoice.Bounds = new UniRectangle(340, 155, 35, 20);

            ButtonControl createRoomButton = new ButtonControl();
            createRoomButton.Bounds = new UniRectangle(48, 208, 104, 46);
            createRoomButton.Name = "Create Room Button";
            createRoomButton.Pressed += new EventHandler(createRoomPressed);
            createRoomButton.Text = "Create";

            ButtonControl cancelButton = new ButtonControl();
            cancelButton.Bounds = new UniRectangle(326, 208, 104, 46);
            cancelButton.Name = "Cancel Button";
            cancelButton.Pressed += new EventHandler(cancelPressed);
            cancelButton.Text = "Cancel";

            mainScreen.Desktop.Children.Add(roomNameLabel);
            mainScreen.Desktop.Children.Add(roomNameInput);
            mainScreen.Desktop.Children.Add(maxPlayerLabel);
            mainScreen.Desktop.Children.Add(twoChoice);
            mainScreen.Desktop.Children.Add(fourChoice);
            mainScreen.Desktop.Children.Add(sixChoice);
            mainScreen.Desktop.Children.Add(eightChoice);

            mainScreen.Desktop.Children.Add(createRoomButton);
            mainScreen.Desktop.Children.Add(cancelButton);
        }

        private void createRoomPressed(Object obj, EventArgs args)
        {
            roomNameInput.Text = roomNameInput.Text.Trim();
            if ((roomNameInput.Text != "") && (roomNameInput.Text.Length <= 50))
            {
                int max_player = 2;
                if (fourChoice.Selected)
                {
                    max_player = 4;
                }
                else if (sixChoice.Selected)
                {
                    max_player = 6;
                }
                else if (eightChoice.Selected)
                {
                    max_player = 8;
                }
                if (Game1.main_console.CreateRoom(roomNameInput.Text, max_player))
                {
                    gameStateService.Switch(new RoomState(previousState, gameStateService, guiService, inputService, graphics, content));
                }
                else
                {
                    Game1.MessageBox(new IntPtr(0), "Cannot connect room.", "[ERROR] Connection", 0);
                }
            }
            else if (roomNameInput.Text.Length > 50)
            {
                Game1.MessageBox(new IntPtr(0), "Room name can only have maximum 50 characters.", "[ERROR] Room name", 0);
            }
            else
            {
                Game1.MessageBox(new IntPtr(0), "Please enter the room name.", "[ERROR] Room Name", 0);
            }
        }

        private void cancelPressed(Object obj, EventArgs args)
        {
            gameStateService.Switch(previousState);
        }

        private void mouseMoved(float x, float y)
        {
            if (((x >= 217) && (x <= 430)) && (y >= 88) && (y <= 114))
            {
                if (Game1.cursorPath != @"Content\Mouse\beam_r.cur")
                {
                    // move to input room name
                    Game1.cursorPath = @"Content\Mouse\beam_r.cur";
                    Game1.cursorTrigger = true;
                }
            }
            else if (((x >= 48) && (x <= 152)) && (y >= 208) && (y <= 254))
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_link.cur")
                {
                    // move to button create room
                    Game1.cursorPath = @"Content\Mouse\aero_link.cur";
                    Game1.cursorTrigger = true;
                }
            }
            else if (((x >= 326) && (x <= 430)) && (y >= 208) && (y <= 254))
            {
                if (Game1.cursorPath != @"Content\Mouse\aero_link.cur")
                {
                    // move to button cancel
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
