using Autonomous.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autonomous.Team1Player
{
    public static class GameObjectStateExtension
    {

        /// <summary>
        /// Returns if an object is in a given lane. Returns true for both lanes if the ibject is somewhere inbetween.
        /// </summary>
        public static bool IsInLane(this GameObjectState obj, int lane)
        {
            bool inLane = true;
            int leftLane = lane - 1;
            if (leftLane >= 0)
            {
                inLane &= Lanes.Positions[leftLane] + Lanes.Tolerance < obj.BoundingBox.CenterX;
            }

            int rightLane = lane + 1;
            if (rightLane <=3 )
            {
                inLane &= Lanes.Positions[rightLane] - Lanes.Tolerance > obj.BoundingBox.CenterX;
            }

            return inLane;
        }
    }

}
