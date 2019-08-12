using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ICGGSAssignment
{
    public static class Functions
    {
        private static Random rand = new Random(DateTime.Now.Millisecond);

        // Assigned in GameplayScreen - used to access player object globally
        public static Piranha Player;

        #region PowerUp Effects
        public static bool NoChaseActive = false;
        public static bool RepelActive = false;
        public static bool SlowFishActive = false;
        public static bool SlowMinesActive = false;
        #endregion

        // Assigned in GameplayScreen - used to access viewport dimensions globally
        public static Point GameSize = new Point(0, 0);

        // AlertBox singleton instance
        public static AlertBox Alert = AlertBox.Instance;

        /// <summary>
        /// Used to determine whether an object is at its wanted position. 
        /// Position and WantedPosition are rounded before they are compared
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <returns>True if object is at its WantedPosition, otherwise false</returns>
        public static bool ObjectReachedDestination(GameObject obj)
        {
            Vector2 p = new Vector2((float)Math.Round(obj.Position.X), (float)Math.Round(obj.Position.Y));
            return (p == obj.WantedPosition || obj.WantedPosition == Vector2.Zero);
        }

        /// <summary>
        /// Determine a random position on the screen
        /// </summary>
        /// <returns>A Vector2 containing the new screen position</returns>
        public static Vector2 RandScreenPos()
        {
            return new Vector2(rand.Next(GameSize.X), rand.Next(GameSize.Y));
        }

        /// <summary>
        /// Determine a random position on the screen with bounds
        /// </summary>
        /// <param name="bounds">The rectangle to confine the position</param>
        /// <returns>A Vector2 containing the new screen position</returns>
        public static Vector2 RandScreenPos(Rectangle bounds)
        {
            return new Vector2(rand.Next(bounds.Left, bounds.Width), rand.Next(bounds.Top, bounds.Height));
        }

        /// <summary>
        /// Random number generator
        /// </summary>
        /// <param name="l">The lower bound</param>
        /// <param name="h">The upper bound</param>
        /// <returns>A number between "l" and "h"</returns>
        public static int Rand(int l, int h)
        {
            return rand.Next(l, h);
        }

        /// <summary>
        /// Returns either true or false (randomly)
        /// </summary>
        public static bool RandBool { get { return (rand.Next(2) == 0); } }

        /// <summary>
        /// Generate a random double that is between the min/max and isn't zero
        /// </summary>
        /// <param name="min">Minimum double</param>
        /// <param name="max">Maximum double</param>
        /// <returns>The random double</returns>
        public static double RandDouble(double min, double max)
        {
            double r = 0;
            if (rand.Next(2) == 0)
            {
                do
                {
                    r = rand.NextDouble();
                } while (r > max || r == 0);
            }
            else
            {
                do
                {
                    r = -rand.NextDouble();
                } while (r < min || r == 0);
            }
            return r;
        }

        /// <summary>
        /// Gets a list of active objects within the given list
        /// </summary>
        /// <param name="list">A list of GameObject to check</param>
        /// <returns>The new list</returns>
        public static List<GameObject> GetActiveObjects(List<GameObject> list)
        {
            return list.FindAll(delegate(GameObject o) { return (o.IsActive); });
        }

        /// <summary>
        /// Gets a list of active fish within the given list
        /// </summary>
        /// <param name="list">A list of Fish to check</param>
        /// <returns>The new list</returns>
        public static List<Fish> GetActiveObjects(List<Fish> list)
        {
            return list.FindAll(delegate(Fish f) { return (f.IsActive); });
        }

        /// <summary>
        /// Rotates a Vector2 by the given angle
        /// </summary>
        /// <param name="v">The Vector2 to rotate</param>
        /// <param name="angle">The angle to rotate by</param>
        /// <returns>The transformed Vector2</returns>
        public static Vector2 RotateVector2(Vector2 v, float angle)
        {
            Vector2 vec = new Vector2(
                (float)(v.X * Math.Cos(angle) + v.Y * Math.Sin(angle)),
                (float)(-1 * v.X * Math.Sin(angle) + v.Y * Math.Cos(angle)));
            return vec;
        }

        /// <summary>
        /// Helper for moving a value around in a circle.
        /// </summary>
        public static Vector2 MoveInCircle(GameTime gameTime, float speed)
        {
            double time = gameTime.TotalGameTime.TotalSeconds * speed;

            float x = (float)Math.Cos(time);
            float y = (float)Math.Sin(time);

            return new Vector2(x, y);
        }
    }
}
