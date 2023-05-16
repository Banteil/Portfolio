using UnityEngine;
using System.Collections.Generic;

namespace Zeus
{
    public class WaypointArea : MonoBehaviour
    {
        public List<Waypoint> waypoints;
        public bool randomWayPoint;
#if UNITY_EDITOR
        [SerializeField,HideInInspector]
        private bool editMode;
#endif
        public Waypoint GetRandomWayPoint()
        {
            System.Random random = new System.Random(100);
            var _nodes = GetValidPoints();
            var index = random.Next(0, waypoints.Count - 1);
            if (_nodes != null && _nodes.Count > 0 && index < _nodes.Count)
                return _nodes[index];

            return null;
        }
      
        public List<Waypoint> GetValidPoints(bool reverse = false)
        {
            var _nodes = waypoints.FindAll(node => node.isValid);
            if (reverse) _nodes.Reverse();
            return _nodes;
        }

        public List<Point> GetValidSubPoints(Waypoint waipoint,bool reverse = false)
        {
            var _nodes = waipoint.subPoints.FindAll(node => node.isValid);
            if (reverse) _nodes.Reverse();
            return _nodes;
        }

        public Waypoint GetWayPoint(int index)
        {
            var _nodes = GetValidPoints();
            if (_nodes != null && _nodes.Count > 0 && index < _nodes.Count) return _nodes[index];

            return null;
        }
    }
}