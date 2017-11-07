using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BOIDs_example
{
    public class Flock : DrawableGameComponent
    {
        public const int FLOOR = -10;
        public const float X_BOUND = 790;
        public const float Y_BOUND = 480;

        public const int R_BOID_START = 70;        // Starting radius in which the boid positions are randomized
        public const int R_PRED_START = 200;        // Starting radius in which the predator positions are randomized
        
        private Vector2 avgCenter;          // Averaged center of the flock per frame
        private Vector2 avgVelocity;        // Averaged velocity of the flock per frame

        private List<Boid> starlings;   // List of boids
        private List<Falcon> falcons = new List<Falcon>();       // List of player controlled objects

        private Vector2 startPos;
        private int deathCount = 0;

        private int boidCount;

        Boid randBoid;

        private int timer;

        int count = 0;
        int boundryWidth;
        int boundryHeight;

        SpriteBatch spriteBatch;

        /// <summary>
        /// Tecture used to draw the Flock
        /// </summary>
        Texture2D boidTexture;

        /// <summary>
        /// Setup the flock boundaries and generate individual members of the flock
        /// </summary>
        /// <param name="tex"> The texture to be used by the birds</param>
        /// <param name="screenWidth">Width of the screen</param>
        /// <param name="screenHeight">Height of the screen</param>
        /// <param name="flockParameters">Behavior of the flock</param>
        public Flock(Texture2D tex, int screenWidth, int screenHeight, Game game, int flockCount, SpriteBatch spriteBatch, Vector2 spawnLocation)
            : base(game)
        {
            //// Initialize variables
            this.spriteBatch = spriteBatch;
            boidTexture = tex;
            boundryWidth = screenWidth;
            boundryHeight = screenHeight;
            boidCount = flockCount;
            startPos = spawnLocation;
   
            starlings = new List<Boid>();

            AddBoid();
        }

        private void AddBoid()
        {
            Vector2 tempDir;
            Vector2 tempPos;
            Random amountreleased = new Random();
            Random random = new Random();
            Random rand = new Random();

            int tempAmount = amountreleased.Next(1,4);

            tempDir = new Vector2((float)
                random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f);

            for (int i = 0; i < tempAmount; i++)
            {
                tempPos = new Vector2(random.Next((int)startPos.X - 20, (int)startPos.X + 20), startPos.Y);

                Boid temp = new Boid(boidTexture, boundryWidth, boundryHeight, this, tempDir, tempPos, 1); //tempLoc
                temp.Size = new Vector2(.5f, .5f);
                starlings.Add(temp);   // Add to the list of prey
            }
        }

        /// <summary>
        /// Update each flock member.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {

            // Initialize the flock center and velocity to 0
            avgCenter = new Vector2(0, 0);
            avgVelocity = new Vector2(0, 0);

            if (deathCount < boidCount && !(starlings.Count >= boidCount))
            {
                timer++;
                if (starlings.Count < boidCount && timer > 10)
                {
                    timer = 0;
                    AddBoid();
                }
            }

            // Add the position and velocity of each prey up
            for (int i = 0; i < starlings.Count; i++)
            {
                
                if (starlings[i].GetType() == typeof(Boid))
                {
                    avgCenter += starlings[i].position;
                    avgVelocity += starlings[i].Velocity;
                }
            }

            // Divide the flock center by the number of boids to get the average position
            avgCenter *= 1 / (float)starlings.Count;

            // Normalize the flock velocity, then adjust it by the following factor
            if (avgVelocity.LengthSquared() > 0)
            {
                avgVelocity.Normalize();
            }

            avgVelocity *= Boid.F_FOLLOW;

            if (count > 100 && starlings.Count > 0)
                RandAvoidance(starlings);
            else
                count++;

            // Update the state of each starling
            for (int i =0; i < starlings.Count; i++) //Bird starling in starlings)
            {
                starlings[i].update(gameTime, starlings, falcons, randBoid);
            }

            // Update the state of each falcon
            foreach (Falcon player in falcons)
            {
                player.Update(gameTime);
            }
        }

        private void RandAvoidance(List<Boid> BOIDs)
        {
            Random rand = new Random();

            int num = rand.Next(0, BOIDs.Count);
            randBoid = BOIDs[num];
            count = 0;
        }

        /// <summary>
        /// Calls Draw on every member of the Flock
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            foreach (Boid boid in starlings)
            {
                boid.Draw(spriteBatch, gameTime);
            }

            foreach (Falcon player in falcons)
            {
                player.Draw(spriteBatch, gameTime);
            }
        }

        public void RemoveBoid(Boid boid)
        {
            starlings.Remove(boid);
        }

        public void AddFalcon(Falcon temp)
        {
            falcons.Add(temp);  // Add to the list of predators
        }

        #region Accessors/Mutators
        public Vector2 FlockCenter { get { return avgCenter; } }
        public Vector2 FlockVelocity { get { return avgVelocity; } }
        public int DeathCount { get { return deathCount; } set { deathCount = value; } }
        #endregion
    }
}
