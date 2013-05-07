using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Slime_Engine
{
    class Slime_AI
    {
        const float MAX_MOVEMENT = 10f;

        public Slime_AI() { }

        /// <summary>
        /// Computes and returns the new relative position of the AI slime with respect to
        /// the ground array with a constrained motion.
        /// </summary>
        /// <param name="ball_location">Volleyball current location with respect to the ground array</param>
        /// <param name="ball_velocity">Volleyball current velocity with respect to the ground array</param>
        /// <param name="slime_location">AI slime current location with respect to the ground array</param>
        /// <returns></returns>
        public Vector3 makeMove(Vector3 ball_location, Vector3 ball_velocity, Vector3 slime_location)
        {
            return Vector3.Zero;
        }
    }
}
