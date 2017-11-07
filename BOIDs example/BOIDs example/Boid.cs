using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BOIDs_example
{
    public class Boid : Animal
    {
        public const float BOID_ACC = 10f;

        public const int F_FOLLOW = 20;         // How closely the boid wants to follow the general direction of the flock
        public int F_PREDATOR = 90;     // (-170) How repulsed/attracted the boid is by a predator
        public const int R_VISIBILITY = 100;  // Square of the distance at which the boid can see a predator
        public Vector2 Size;
        public Vector2 Velocity;
        
        protected Random random;

        private Flock flock;    // Flock this prey is a part of
        private bool isDead;
        private int bHealth;
        private const int minDist = 15;// Least distance apart of the boids
        private float maxSpeed;

        
        /// <summary>
        /// Boid constructor
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="dir">movement direction</param>
        /// <param name="loc">spawn location</param>
        /// <param name="screenSize">screen size</param>
        public Boid(Texture2D tex, int screenWidth, int screenHeight, Flock _owner, Vector2 dir, Vector2 loc, int health)
            : base(tex, screenWidth, screenHeight)
        {
            bHealth = health;
            direction = dir;
            direction.Normalize();
            position = loc;

            random = new Random((int)loc.X + (int)loc.Y);

            flock = _owner;
            maxSpeed = 320;//220
            isDead = false;
        }

        /// <summary>
        /// Update the boid
        /// </summary>
        /// <param name="rand"> Random number generator</param>
        /// <param name="boids"> List of boids in the flock</param>
        /// <param name="elapsedTime"> Elapsed time for this frame</param>
        /// <param name="flockCenter"> Center of the flock</param>
        /// <param name="flockVelocity"> Average velocity of the flock</param>
        public void update(GameTime gameTime, List<Boid> objects, List<Falcon> falcons, Boid randBoid)
        {
            if (isDead == false)
            {
                float rndMultiplier = (float)random.Next(0, 3);//(float)random.NextDouble();

                Vector2 velocityUpdate = new Vector2(0, 0);     // Vector which will modify the boids velocity vector  

                //Methods for moving the boids indepentant of other variables, such as predator position & side bounds
                MoveBoidAway(objects); //Moving boids away from each other
                MoveCloser(objects); //Moving boids closer to each other
                MoveWith(objects); //Moving boids in a similar direction to each other

                // Get the vector from this boid to its neighbor
                Vector2 distToPlayer = falcons[0].position;
                distToPlayer -= position;

                // If it is within range, add the vector to the repulsion vector
                if (distToPlayer.Length() < R_VISIBILITY)
                {
                    F_PREDATOR = 170;
                    maxSpeed = 200;
                }
                else
                {
                    F_PREDATOR = 90;
                    maxSpeed = 320;
                }

                //Seeking the player
                Vector2 predatorAvoidance = new Vector2(0, 0);
                Falcon predator;

                // Loop through each predator and get the vector to it
                predator = (Falcon)falcons[0];

                // Get the vector from this boid to its neighbor
                Vector2 vecToPredator = predator.position;
                vecToPredator -= position;
                vecToPredator *= rndMultiplier / vecToPredator.LengthSquared();
                predatorAvoidance += vecToPredator;

                //For collision with the player
                if (predator.HitBounds.Contains((int)position.X, (int)position.Y))
                {
                    bHealth--;
                    if (bHealth <= 0)
                    {
                        flock.DeathCount++; //For the flock to keep track of the amount of boids that have died so it won't keep spawning them
                        isDead = true; //If collides the boid is triggered as dead
                        //Then remove the BOID from the overall list
                        flock.RemoveBoid(this);
                    }
                }

                if (predatorAvoidance.LengthSquared() > 0)
                {
                    predatorAvoidance.Normalize();
                    predatorAvoidance *= F_PREDATOR;
                    velocityUpdate += predatorAvoidance;
                }

                //Keeping Within the bounds
                Vector2 inBoundsVector = new Vector2(0, 0);

                // Stay within the side bounds
                if (position.X < Flock.FLOOR)
                    inBoundsVector.X = 1 / Math.Abs(position.X + Flock.X_BOUND);
                else if (position.X > Flock.X_BOUND)
                    inBoundsVector.X = -1 / Math.Abs(position.X - Flock.FLOOR);

                // Stay within the floor and ceiling
                if (position.Y < Flock.FLOOR)
                    inBoundsVector.Y = 1 / Math.Abs(position.Y - Flock.Y_BOUND);
                else if (position.Y > Flock.Y_BOUND)
                    inBoundsVector.Y = -1 / Math.Abs(position.Y - Flock.FLOOR);

                inBoundsVector *= 40000;
                velocityUpdate += inBoundsVector;

                //If the boid is the randomly selected boid, move it with a random variance to the rest of the group
                if (randBoid == this)
                {
                    maxSpeed += 50; //Need to change this to give the selected Boid a random movement across the swarm
                }

                // Velocity and position Update
                // Modify our velocity update vector to take into account acceleration over time
                velocityUpdate *= (float)(BOID_ACC * gameTime.ElapsedGameTime.TotalSeconds);

                // Apply the update to the velocity
                Velocity += velocityUpdate;

                // If our velocity vector exceeds the max speed, throttle it back to the MAX_SPEED
                if (Velocity.Length() > maxSpeed)
                {
                    Velocity.Normalize();
                    Velocity *= maxSpeed;
                }

                Direction = -Velocity;

                // Update the position of the boid
                this.position += this.Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            }
        }

        //Credit to http://www.coderholic.com/javascript-boids/ for these next three movement methods
        /// <summary>
        /// Method to move the current boid away from the neighbouring boids
        /// </summary>
        /// <param name="BOIDs">List of current Boids in the game</param>
        private void MoveBoidAway(List<Boid> BOIDs)
        {
            float distanceX = 0;//distance variables to test and assign
            float distanceY = 0;
            int numClose = 0; //Variable to track how many boids are close

            for (int i = 0; i < BOIDs.Count; i++)
            {

                if (BOIDs[i].position.X != this.position.X && BOIDs[i].position.Y != this.position.Y)
                {
                    Boid boid = BOIDs[i];

                    //Finding the distance of the current boid from the neighbouring boid
                    float distX = this.position.X - boid.position.X;
                    float distY = this.position.Y - boid.position.Y;
                    float dist = (float)Math.Sqrt(distX * distX + distY * distY); // square root of the distances squared

                    //if the distance between the boids is less than the minimum distance
                    if (dist < minDist) 
                    {
                        numClose++;//Incrementing the number of close boids
                        float xdiff = (this.position.X - boid.position.X); //Finding the float difference in positions of the boid & neighbour
                        float ydiff = (this.position.Y - boid.position.Y);

                        //If tests to determine if the if the difference is to the left or right of the current boid
                        // - so determining it being a positive or negative number from each other
                        if (xdiff >= 0)
                            xdiff = (float)Math.Sqrt(minDist) - xdiff; //difference becomes positive sqrt of the minimum distance minus the difference
                        else if (xdiff < 0)
                            xdiff = -(float)Math.Sqrt(minDist) - xdiff;

                        //If tests to determine if the if the difference is above or below the current boid
                        if (ydiff >= 0)
                            ydiff = (float)Math.Sqrt(minDist) - ydiff;
                        else if (ydiff < 0)
                            ydiff = -(float)Math.Sqrt(minDist) - ydiff;

                        distanceX += xdiff;
                        distanceY += ydiff;
                    }
                }
            }

            if (numClose != 0)
            {
                this.Velocity.X -= distanceX / 5;
                this.Velocity.Y -= distanceY / 5;  
            }
        }

        /// <summary>
        /// Moves the current boid closer to it's neighbours
        /// </summary>
        /// <param name="BOIDs">List of current Boids in the game</param>
        private void MoveCloser(List<Boid> BOIDs)
        {
            float avgX = 0;  
            float avgY = 0;

            float dist = 300;//set distance for the boid to move towards neighbours

            for (int i = 0; i < BOIDs.Count; i++)//Looping through boids
            {
                if (BOIDs[i] != this)//Making sure the boid won't move towards itself
                {
                    Boid boid = (Boid)BOIDs[i];
                    if (this.position.X - boid.position.X < dist && //If the distance between neighbour and current boid is within range
                        this.position.Y - boid.position.Y < dist)
                    {
                        //storing the distance
                        avgX += (this.position.X - boid.position.X);
                        avgY += (this.position.Y - boid.position.Y);  
                    }
                }
            }

            //averaging the overall distances that have been added by the number of boids in play
            avgX /= BOIDs.Count;  
            avgY /= BOIDs.Count;

            //asum of the x & y variables to the power of 2
            double average = (avgX * avgX) + (avgY * avgY);
            dist = (float)Math.Sqrt(average) * -1;

            if (dist != 0)
            {
                double averageVelX = this.Velocity.X + (avgX / dist) * 0.15;
                double averageVelY = this.Velocity.Y + (avgY / dist) * 0.15;

                //setting the velocity X & Y to the smaller of the two variables
                this.Velocity.X = (float)Math.Min(averageVelX, (double)this.maxSpeed);
                this.Velocity.Y = (float)Math.Min(averageVelY, (double)this.maxSpeed);
            }
        }

        /// <summary>
        /// Moving the boids in the similar direction to the near-by neighbours. 
        /// Similar to the closer method, only it compares the velocities of the neighbour boids
        /// </summary>
        /// <param name="BOIDs">List of current Boids in the game</param>
        private void MoveWith(List<Boid> BOIDs)
        {
            float avgX = 0;  
            float avgY = 0;

            float dist = 300;

            for (int i = 0; i < BOIDs.Count; i++)
            {
                if (BOIDs[i] != this)
                {
                    Boid boid = (Boid)BOIDs[i];
                    if (this.position.X - boid.position.X < dist &&
                        this.position.Y - boid.position.Y < dist)
                    {
                        avgX += boid.Velocity.X;
                        avgY += boid.Velocity.Y; 
                    }
                }
            }

            avgX /= BOIDs.Count;
            avgY /= BOIDs.Count;

            double average = (avgX * avgX) + (avgY * avgY);
            dist = (float)Math.Sqrt(average) * -1;

            if (dist != 0)
            {
                double averageVelX = this.Velocity.X + (avgX / dist) * 0.15;
                double averageVelY = this.Velocity.Y + (avgY / dist) * 0.15;

                this.Velocity.X = (float)Math.Min(averageVelX, (double)this.maxSpeed);
                this.Velocity.Y = (float)Math.Min(averageVelY, (double)this.maxSpeed);
            }
        }

        /// <summary>
        /// Draw the boid
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="gameTime"></param>
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            //Color tintColor = color;
            float rotation = 0.0f;
            rotation = (float)Math.Atan2(direction.Y, direction.X);

            spriteBatch.Begin();
            // Draw the animal, centered around its position, and using the 
            //orientation and tint color.

            if (isDead == false)
                spriteBatch.Draw(texture, this.position, null, Color.White,
                    rotation, textureCenter, 0.2f, SpriteEffects.None, 0.0f);

            spriteBatch.End();
        }
    }
}
