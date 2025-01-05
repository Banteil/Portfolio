using UnityEngine;

namespace starinc.io.kingnslave
{
    public class ScreenTransitionManager : Singleton<ScreenTransitionManager>
    {
        [SerializeField]
        private DirectionTable directionTable;
        private BaseDirection currentDirection;

        public T ShowDirection<T>()
        {
            var data = directionTable.LoadingDirectionDatas.Find((data) => data.Direction is T);
            var direction = CreateDirectionObject<T>(data.Direction);
            currentDirection = direction as BaseDirection;
            return direction;
        }

        public void CloseDirection()
        {
            if (currentDirection == null) return;
            currentDirection.EndDirection();
            currentDirection = null;
        }

        private T CreateDirectionObject<T>(BaseDirection tableObject)
        {
            var obj = Instantiate(tableObject.gameObject, transform, false);
            obj.ExcludingClone();
            var result = obj.GetComponent<T>();
            return result;
        }
    }
}
