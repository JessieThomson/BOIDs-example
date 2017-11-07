using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BOIDs_example
{
    public enum AnimalType
    {
        // no type
        Generic,
        // flies around and reacts
        Bird,
        // controled by the mouse, birds fly towards it
        Falcon
    }
    /// <summary>
    /// base class for moveable, drawable critters onscreen
    /// </summary>
    public class Animal
    {
        /// <summary>
        /// texture drawn to represent this animal
        /// </summary>
        protected Texture2D texture;
        /// <summary>
        /// tint color to draw the texture with
        /// </summary>
        protected Color color = Color.White;
        /// <summary>
        /// center of the draw texture
        /// </summary>
        protected Vector2 textureCenter;
        /// <summary>
        /// movement speed in pixels/second
        /// </summary>

        public Vector2 position;

        public Vector2 Center;
        public Vector2 velocity;

        /// <summary>
        /// The animal type
        /// </summary>
        public AnimalType AnimalType
        { get { return animaltype; } }
        protected AnimalType animaltype = AnimalType.Generic;

        public bool Fleeing
        { get { return fleeing; } set { fleeing = value; } }
        protected bool fleeing = false;

        public int BoundryWidth
        { get { return boundryWidth; } }
        protected int boundryWidth;

        public int BoundryHeight
        { get { return boundryHeight; } }
        protected int boundryHeight;

        // the minimum and maximum scale values for the sprite
        public const float MinScale = .5f;
        public const float MaxScale = 2f;

        private float scale = 1f;

        public float Scale
        { get { return scale; } set { scale = MathHelper.Clamp(value, MinScale, MaxScale); } }

        /// <summary>
        /// Direction the animal is moving in
        /// </summary>
        public Vector2 Direction
        { get { return direction; } set { direction = value; } }
        protected Vector2 direction;

        /// <summary>
        /// Location on screen
        /// </summary>
        public Vector2 Location
        { get { return location; } set { location = value; } }
        protected Vector2 location;

        public Rectangle HitBounds
        {
            get
            {
                // create a rectangle based on the texture
                Rectangle r = new Rectangle(
                    (int)(Center.X - texture.Width / 2 * Scale),
                    (int)(Center.Y - texture.Height / 2 * Scale),
                    (int)(texture.Width * Scale),
                    (int)(texture.Height * Scale));

                // inflate the texture a little to give us some additional pad room
                r.Inflate(10, 10);
                return r;
            }
        }

        /// <summary>
        /// Sets the boundries the animal can move in the texture used in Draw
        /// </summary>
        /// <param name="tex">Texture to use</param>
        /// <param name="screenSize">Size of the sample screen</param>
        public Animal(Texture2D tex, int screenWidth, int screenHeight)
        {
            if (tex != null)
            {
                texture = tex;
                textureCenter = new Vector2(texture.Width / 2, texture.Height / 2);
            }
            boundryWidth = screenWidth;
            boundryHeight = screenHeight;
        }

        /// <summary>
        /// Empty update function
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
        }

        /// <summary>
        /// Draw the Animal with the specified SpriteBatch
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="gameTime"></param>
        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            float rotation = (float)Math.Atan2(direction.Y, direction.X);

            spriteBatch.Draw(texture, location, null, color,
                rotation, textureCenter, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}
