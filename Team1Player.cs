using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Autonomous.Public;
using Autonomous.Team1Player;

namespace Autonomous.Team1Player
{
    [Export(typeof(IPlayer))]
    [ExportMetadata("PlayerName", "Team1Player")]
    public class Team1Player : IPlayer
    {
        private string _playerId;

        public void Finish()
        {
        }

        public void Initialize(string playerId)
        {
            _playerId = playerId;
        }

        public PlayerAction Update(GameState gameState)
        {
            var self = gameState.GameObjectStates.First(o => o.Id == _playerId);
            var gameObjects = gameState.GameObjectStates.Where(g => g.GameObjectType != GameObjectType.Player).ToList();

            var objectInFront = GetClosestObjectInFront(gameObjects, self);

            float accelerationY = 1;
            bool left = false, right = false;

            float desiredX = GameConstants.LaneWidth / 2;
            
            List<GameObjectState> closestObjectsInLanes = GetClosestObjectsInLanes(gameObjects, self);    

            List<float> laneCollisionTimes = closestObjectsInLanes.Select(
                o => (o.BoundingBox.Y - self.BoundingBox.Y) / Math.Max((self.VY - o.VY), 1)
                
                ).ToList();

            laneCollisionTimes[0] = 0; //disable first lane, too dangerous

            //disable far away lanes
            if (self.IsInLane(1)) laneCollisionTimes[3] = 0;
            if (self.IsInLane(3)) laneCollisionTimes[1] = 0;

            int longestLane = laneCollisionTimes.IndexOf(laneCollisionTimes.Max());
            float LongestLaneX = Lanes.Positions[longestLane];

            //braking mechanism from sampleplayer
            if (objectInFront != null)
            {
                if (!IsDistanceSafe(objectInFront, self))
                {
                    accelerationY = -1;
                }
            }

            float centerX = (self.BoundingBox.Left + self.BoundingBox.Right) / 2;

            //move to longest lane
            desiredX = LongestLaneX;

            if (Math.Abs(desiredX - centerX) > 0.2)
            {
                if (desiredX < centerX)
                    left = true;
                else
                    right = true;
            }

            return new PlayerAction() { MoveLeft = left, MoveRight = right, Acceleration = accelerationY };
        }


        public bool IsDistanceSafe(GameObjectState objectInFront, GameObjectState self)
        {
            float otherSpeed = objectInFront.VY;
            float otherDistanceToStop = CalculateDistanceToStop(otherSpeed, objectInFront.MaximumDeceleration);
            float selfDistancveToStop = CalculateDistanceToStop(self.VY, self.MaximumDeceleration);

            float selfCenterY = (self.BoundingBox.Top + self.BoundingBox.Bottom) / 2;
            float otherCenterY = (objectInFront.BoundingBox.Top + objectInFront.BoundingBox.Bottom) / 2;

            float distanceBetweenCars = Math.Abs(selfCenterY - otherCenterY) - self.BoundingBox.Height / 2 - objectInFront.BoundingBox.Height / 2;
            float plusSafeDistance = 5;
            if (distanceBetweenCars < selfDistancveToStop - otherDistanceToStop + plusSafeDistance && self.VY > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private float CalculateDistanceToStop(float v, float breakDeceleration)
        {
            if (v == 0) return 0f;
            return 0.5f * v * v / breakDeceleration;
        }

        private GameObjectState GetClosestObjectInFront(IEnumerable<GameObjectState> objects, GameObjectState self)
        {
            return objects
                          .Where(o => o.BoundingBox.Y > self.BoundingBox.Y)
                          .FirstOrDefault(o => IsOverlappingHorizontally(self, o));
        }


        private List<GameObjectState> GetClosestObjectsInLanes(IEnumerable<GameObjectState> objects, GameObjectState self)
        {
            List<GameObjectState> closestObjects = new List<GameObjectState>();

            for (int lane = 0; lane < 4; lane++)
            {
                closestObjects.Add(
                objects.Where(o => o.BoundingBox.Y > self.BoundingBox.Bottom - 5 && o.IsInLane(lane) == true) //returns objects next to us too, to avoid collisions when overtaking
                .OrderBy(o => o.BoundingBox.Y)
                .FirstOrDefault()
                );
            }

            return closestObjects;
        }

       
        private bool IsOverlappingHorizontally(GameObjectState self, GameObjectState other)
        {
            var r1 = self.BoundingBox;
            var r2 = other.BoundingBox;
            return Between(r1.Left, r1.Right, r2.Left) ||
                   Between(r1.Left, r1.Right, r2.Right) ||
                   Between(r2.Left, r2.Right, r1.Right) ||
                   Between(r2.Left, r2.Right, r1.Left);
        }

        private static bool Between(float limit1, float limit2, float value)
        {
            return (value >= limit1 && value <= limit2) || (value >= limit2 && value <= limit1);
        }

    }
}
