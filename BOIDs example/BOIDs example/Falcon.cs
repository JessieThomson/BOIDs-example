using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace BOIDs_example
{
    public class Falcon : Animal
    {
        public Vector2 Size;            // Size of the object in the world. 1 unit = 1 m

        // this is the amount of velocity that is maintained after
        // the sprite bounces off of the wall
        public const float BounceMagnitude = .5f;

        // this is the percentage of velocity lost each second as
        // the sprite moves around.
        public const float Friction = 3.0f;

        /// <summary>
        /// Sets up player object texture and screenBoundary
        /// </summary>
        /// <param name="tex">Texture to use</param>
        /// <param name="screenSize">Size of the sample screen</param>
        public Falcon(Texture2D tex, int screenWidth, int screenHeight, Flock _owner, Random _rand)
            : base(tex, screenWidth, screenHeight)
        {
            texture = tex;

            boundryWidth = screenWidth;
            boundryHeight = screenHeight;
        }

        /// <summary>
        /// Moves the player object within the bounds of the screen
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) 
        {
            if (gameTime != null)
            {
                // move the sprite based on the velocity
                Center += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

                // apply friction to the velocity to slow the sprite down
                velocity *= 1f - (Friction * (float)gameTime.ElapsedGameTime.TotalSeconds);

                // calculate the scaled width and height for the method
                float halfWidth = (texture.Width * Scale) / 2f;
                float halfHeight = (texture.Height * Scale) / 2f;

                // check each side to make sure the sprite is in the bounds. if
                // the sprite is outside the bounds, we move the sprite and reverse
                // the velocity on that axis.

                if (Center.X < 0 + halfWidth)
                {
                    Center.X = 0 + halfWidth;
                    velocity.X *= -BounceMagnitude;
                }

                if (Center.X > boundryWidth - halfWidth)
                {
                    Center.X = boundryWidth - halfWidth;
                    velocity.X *= -BounceMagnitude;
                }

                if (Center.Y < 0 + halfHeight)
                {
                    Center.Y = 0 + halfHeight;
                    velocity.Y *= -BounceMagnitude;
                }

                if (Center.Y > boundryHeight - halfHeight)
                {
                    Center.Y = boundryHeight - halfHeight;
                    velocity.Y *= -BounceMagnitude;
                }

                position = Center;
            }
        }

        /// <summary>
        /// Draw the player sprite
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="gameTime"></param>
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, Center, null, color,
                0f, textureCenter, Scale, SpriteEffects.None, 0.0f);
            spriteBatch.End();
        }
    }
       
}
