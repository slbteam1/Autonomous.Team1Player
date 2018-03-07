using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autonomous.Team1Player
{
    public class Lanes
    {
        public const float Tolerance = 0.2f;
        public static float[] Positions
        {
            get
            {
                return new float[] { -4.3f, -1.5f, 1.5f, 4.3f };
            }
        }
    }
}
