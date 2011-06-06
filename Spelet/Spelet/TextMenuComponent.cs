using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ClassLibrary
{
    /// <summary>
    /// This is a game component that implements a menu with text elements.
    /// </summary>
    public class TextMenuComponent : DrawableGameComponent
    {
        // Spritebatch
        protected SpriteBatch spriteBatch = null;
        // Fonts
        protected readonly SpriteFont regularFont, selectedFont;
        // Colors
        protected Color regularColor = new Color(180, 180, 180), selectedColor = Color.White;
        // Menu Position
        protected Vector2 position = new Vector2();
        // Items
        protected int selectedIndex = -1;
        private readonly List<string> menuItems;
        // Used for handle input
        protected KeyboardState oldKeyboardState;
        protected GamePadState oldGamePadState;
        // Size of menu in pixels
        protected int width, height;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">the main game object</param>
        /// <param name="normalFont">Font to regular items</param>
        /// <param name="selectedFont">Font to selected item</param>
        public TextMenuComponent(Game game, SpriteFont normalFont, 
            SpriteFont selectedFont) : base(game)
        {
            regularFont = normalFont;
            this.selectedFont = selectedFont;
            menuItems = new List<string>();

            // Get the current spritebatch
            spriteBatch = (SpriteBatch) 
                Game.Services.GetService(typeof (SpriteBatch));

            // Used for input handling
            oldKeyboardState = Keyboard.GetState();
            oldGamePadState = GamePad.GetState(PlayerIndex.One);
        }

        /// <summary>
        /// Set the Menu Options
        /// </summary>
        /// <param name="items"></param>
        public void SetMenuItems(string[] items)
        {
            menuItems.Clear();
            menuItems.AddRange(items);
            CalculateBounds();
        }

        /// <summary>
        /// Width of menu in pixels
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Height of menu in pixels
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// Selected menu item index
        /// </summary>
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { selectedIndex = value; }
        }

        /// <summary>
        /// Regular item color
        /// </summary>
        public Color RegularColor
        {
            get { return regularColor; }
            set { regularColor = value; }
        }

        /// <summary>
        /// Selected item color
        /// </summary>
        public Color SelectedColor
        {
            get { return selectedColor; }
            set { selectedColor = value; }
        }

        /// <summary>
        /// Position of component in screen
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Get the menu bounds
        /// </summary>
        protected void CalculateBounds()
        {
            width = 0;
            height = 0;
            foreach (string item in menuItems)
            {
                Vector2 size = selectedFont.MeasureString(item);
                if (size.X > width)
                {
                    width = (int) size.X;
                }
                height += selectedFont.LineSpacing;
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            GamePadState gamepadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            int xmin = (int)Position.X;
            int xmax = (int)Position.X + width;
            int x = mouseState.X;
            int y = mouseState.Y;
            bool _t = true;
            for (int i = 0; i < menuItems.Count; i++)
            {
                int ymin = (int)Position.Y + selectedFont.LineSpacing * i;
                int ymax = (int)Position.Y + selectedFont.LineSpacing * (i + 1);
                /*if (i == menuItems.Count - 1)
                {
                    ymin += selectedFont.LineSpacing;
                    ymax += selectedFont.LineSpacing;
                }*/
                if (xmin < x && x < xmax && y > ymin && y < ymax)
                {
                    selectedIndex = i;
                    _t = false;
                }
            }
            if(_t)
            {
                selectedIndex = -1;
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// Allows the game component to draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            float y = position.Y;
            for (int i = 0; i < menuItems.Count; i++)
            {
                SpriteFont font;
                Color theColor;
                if (i == SelectedIndex)
                {
                    font = selectedFont;
                    theColor = selectedColor;
                }
                else
                {
                    font = regularFont;
                    theColor = regularColor;
                }

                // Draw the text shadow
                spriteBatch.DrawString(font, menuItems[i], 
                    new Vector2(position.X + 1, y + 1), Color.Black);
                // Draw the text item
                spriteBatch.DrawString(font, menuItems[i], 
                    new Vector2(position.X, y), theColor);
                y += font.LineSpacing;
            }

            base.Draw(gameTime);
        }
    }
}