using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    [System.Serializable]
    public class Waypoint : Point
    {
        public List<Point> subPoints;
        public bool randomPatrolPoint;
        public bool rotateTo = true;

        public Vector3 GetRandomSubPoint()
        {
            System.Random random = new System.Random(100);
            var index = random.Next(0, subPoints.Count - 1);
            return GetSubPoint(index);
        }

        public Vector3 GetSubPoint(int index)
        {
            if (subPoints != null && subPoints.Count > 0 && index < subPoints.Count) return subPoints[index].position;

            return transform.position;
        }
    }
}