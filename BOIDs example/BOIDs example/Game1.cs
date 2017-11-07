using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

using Microsoft.Phone.Tasks;

namespace BOIDs_example
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // Adjust the counts of these to get your desired performance
        public const int BOID_COUNT = 40;
        public const int FALCON_COUNT = 1;

        private Texture2D boidTexture;
        private Texture2D falconTexture;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // we track our selected sprite so we can drag it around
        private Falcon selectedSprite;

        Falcon player;

        Flock flock;

        MediaPlayerLauncher mediaPlayerLauncher = new MediaPlayerLauncher();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
           
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.Window.Title = "2D Starling Swarm";

            TouchPanel.EnabledGestures =
                GestureType.Hold |
                GestureType.Tap |
                GestureType.DoubleTap |
                GestureType.FreeDrag |
                GestureType.Flick;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            boidTexture = Content.Load<Texture2D>("cell2");
            falconTexture = Content.Load<Texture2D>("cell");

            flock = new Flock(boidTexture, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, this, BOID_COUNT, spriteBatch, new Vector2(400,-20));

            Random rand = new Random();

            Vector2 tempDir;
            Vector2 tempLoc;

            player = new Falcon(falconTexture, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, flock, rand);

            tempLoc = new Vector2((float)
                rand.Next(GraphicsDevice.Viewport.Width), (float)rand.Next(GraphicsDevice.Viewport.Height));
            tempDir = new Vector2((float)
                rand.NextDouble() - 0.5f, (float)rand.NextDouble() - 0.5f);
            tempDir.Normalize();

            player.position = tempLoc;

            player.Size = new Vector2(1f, 1f);

            flock.AddFalcon(player);

            mediaPlayerLauncher.Media = new Uri("Wildlife.wmv", UriKind.Relative);
            mediaPlayerLauncher.Location = MediaLocationType.Data;
            mediaPlayerLauncher.Controls = MediaPlaybackControls.Pause | MediaPlaybackControls.Stop;
            mediaPlayerLauncher.Orientation = MediaPlayerOrientation.Landscape;
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            flock.Update(gameTime);
            // handle the touch input
            HandleTouchInput();
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        private void HandleTouchInput()
        {
            // we use raw touch points for selection, since they are more appropriate
            // for that use than gestures. so we need to get that raw touch data.
            TouchCollection touches = TouchPanel.GetState();

            // see if we have a new primary point down. when the first touch
            // goes down, we do hit detection to try and select one of our sprites.
            if (touches.Count > 0 && touches[0].State == TouchLocationState.Pressed)
            {
                // convert the touch position into a Point for hit testing
                Point touchPoint = new Point((int)touches[0].Position.X, (int)touches[0].Position.Y);

                // iterate our sprites to find which sprite is being touched. we iterate backwards
                // since that will cause sprites that are drawn on top to be selected before
                // sprites drawn on the bottom.
                selectedSprite = null;

                if (player.HitBounds.Contains(touchPoint))
                {
                    selectedSprite = player;
                }

                if (selectedSprite != null)
                {
                    // make sure we stop selected sprites
                    selectedSprite.velocity = Vector2.Zero;
                }
            }

            // next we handle all of the gestures. since we may have multiple gestures available,
            // we use a loop to read in all of the gestures. this is important to make sure the 
            // TouchPanel's queue doesn't get backed up with old data
            while (TouchPanel.IsGestureAvailable)
            {
                // read the next gesture from the queue
                GestureSample gesture = TouchPanel.ReadGesture();

                // we can use the type of gesture to determine our behavior
                switch (gesture.GestureType)
                {
                    // on taps, we change the color of the selected sprite
                    case GestureType.Tap:
                        break;
                    case GestureType.DoubleTap:
                        break;

                    // on drags, we just want to move the selected sprite with the drag
                    case GestureType.FreeDrag:
                        if (selectedSprite != null)
                        {
                            selectedSprite.Center += gesture.Delta;
                        }
                        break;

                    // on flicks, we want to update the selected sprite's velocity with
                    // the flick velocity, which is in pixels per second.
                    case GestureType.Flick:
                        if (selectedSprite != null)
                        {
                            selectedSprite.velocity = gesture.Delta;
                        }
                        break;
                }
            }

            //// lastly, if there are no raw touch points, we make sure no sprites are selected.
            //// this happens after we handle gestures because some gestures like taps and flicks
            //// will come in on the same frame as our raw touch points report no touches and we
            //// still want to use the selected sprite for those gestures.
            if (touches.Count == 0)
            {
                selectedSprite = null;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            flock.Draw(gameTime);
            //mediaPlayerLauncher.Show();
            
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
