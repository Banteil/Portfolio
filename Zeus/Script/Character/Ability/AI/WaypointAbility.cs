using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class WaypointAbility : AIOnlyAbility
    {
        [SerializeField] protected WaypointArea _waypointArea;
        [SerializeField] protected float _changeWaypointDistance;
        [SerializeField] protected bool _invertWaypointsOrder;
        [SerializeField] protected bool _randomStartingPoint = true;
        [SerializeField] protected bool _randomWaypoint = true;
        [SerializeField] protected bool _startUsingSpecificWaypoint = false;
        [SerializeField] protected int _startWaypointIndex;
        [SerializeField] protected bool _startUsingNearWayPoint = false;
        [SerializeField] protected bool _selfStartingPoint;
        [SerializeField] protected Transform _customStartingPoint;

        private int _currentWaypoint;
        private bool _isWaypointStarted;

        public Vector3 SelfStartPosition { get; set; }

        public Waypoint TargetWaypoint { get; protected set; }

        public List<Waypoint> VisitedWaypoints { get; set; }

        public bool SelfStartingPoint
        {
            get => _selfStartingPoint;
            protected set => _selfStartingPoint = value;
        }

        public float ChangeWaypointDistance { get; protected set; }

        public bool CustomStartPoint => !_selfStartingPoint && _customStartingPoint != null;

        public Vector3 CustomStartPosition => CustomStartPoint ? _customStartingPoint.position : _zeusAI.transform.position;


        public WaypointArea WaypointArea
        {
            get => _waypointArea;
            set
            {
                if (value != null && value != _waypointArea)
                {
                    var waypoints = value.GetValidPoints();
                    if (_randomStartingPoint)
                        _currentWaypoint = Random.Range(0, waypoints.Count);
                }

                _waypointArea = value;
            }
        }

        protected override void Initialize()
        {
            ChangeWaypointDistance = _changeWaypointDistance;
            SelfStartPosition = (!_selfStartingPoint && _customStartingPoint)
                ? _customStartingPoint.position
                : _zeusAI.transform.position;
        }

        public void NextWayPoint() => TargetWaypoint = GetWaypoint();


        private Waypoint GetWaypoint()
        {
            if (_waypointArea == null) return null;
            var waypoints = _waypointArea.GetValidPoints(_invertWaypointsOrder);

            if (!_isWaypointStarted)
            {
                if (_startUsingSpecificWaypoint)
                    _currentWaypoint = _startWaypointIndex % waypoints.Count;

                else if (_startUsingNearWayPoint)
                    _currentWaypoint = GetNearPointIndex();

                else if (_randomWaypoint)
                    _currentWaypoint = Random.Range(0, waypoints.Count);
                else _currentWaypoint = 0;
            }

            if (_isWaypointStarted)
            {
                if (_randomWaypoint)
                    _currentWaypoint = Random.Range(0, waypoints.Count);
                else
                    _currentWaypoint++;
            }

            if (!_isWaypointStarted)
            {
                _isWaypointStarted = true;
                VisitedWaypoints = new List<Waypoint>();
            }

            if (_currentWaypoint >= waypoints.Count)
                _currentWaypoint = 0;

            if (waypoints.Count == 0)
                return null;

            if (VisitedWaypoints.Count == waypoints.Count)
                VisitedWaypoints.Clear();

            if (VisitedWaypoints.Contains(waypoints[_currentWaypoint]))
                return null;

            return waypoints[_currentWaypoint];
        }

        private int GetNearPointIndex()
        {
            var waypoint = _waypointArea.GetValidPoints(_invertWaypointsOrder);
            var targetWay = 0;
            var dist = Mathf.Infinity;
            for (int i = 0; i < waypoint.Count; i++)
            {
                var d = Vector3.Distance(_zeusAI.transform.position, waypoint[i].position);
                if (d < dist)
                {
                    targetWay = i;
                    dist = d;
                }
            }

            return targetWay;
        }
    }
}
