using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Slime_Engine
{
    class Slime_AI
    {
        const float MAX_MOVEMENT = 3.8f;

        public Slime_AI() { }

        /// <summary>
        /// Computes and returns the new relative position of the AI slime with respect to
        /// the ground array with a constrained motion.
        /// </summary>
        /// <param name="ball_location">Volleyball current location with respect to the ground array</param>
        /// <param name="ball_velocity">Volleyball current velocity with respect to the ground array</param>
        /// <param name="slime_location">AI slime current location with respect to the ground array</param>
        /// <returns></returns>
        public Vector3 makeMove(Vector3 ball_location, Vector3 slime_location)
        {
            if (ballIsOverOpponentSide(ball_location))
                return track(ball_location, slime_location);
            else
                return moveToward(ball_location, slime_location);
        }

        private Vector3 moveToward(Vector3 ball_location, Vector3 slime_location)
        {
            Vector3 difference = ball_location - slime_location;
            difference.Y += 7f; // allows slime to hit it on front of ball so it goes over net
            float newX, newY;

            if (Math.Abs(difference.X) > MAX_MOVEMENT)
                newX = getSign(difference.X) * MAX_MOVEMENT;
            else
                newX = difference.X;

            if (Math.Abs(difference.Y) > MAX_MOVEMENT)
                newY = getSign(difference.Y) * MAX_MOVEMENT;
            else
                newY = difference.Y;

            return new Vector3(slime_location.X + newX, slime_location.Y + newY, slime_location.Z);

        }

        //Returns the sign of a float
        private float getSign(float number)
        {
            if (number >= 0)
                return 1;
            else
                return -1;
        }

        private bool ballIsOverOpponentSide(Vector3 ball_location)
        {
            return ball_location.Y <= 0;
        }

        private Vector3 track(Vector3 ball_location, Vector3 slime_location)
        {
            Vector3 difference = ball_location - slime_location;
            float newX;
            if (Math.Abs(difference.X) > MAX_MOVEMENT)
                newX = getSign(difference.X) * MAX_MOVEMENT;
            else
                newX = difference.X;
            return new Vector3(slime_location.X + newX, slime_location.Y, slime_location.Z);
        }
    }
}