using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace starinc.io.gallaryx
{
    public class AutomaticViewingAbility : BaseAbility
    {
        #region Cache
        private CameraController _cameraController;
        private Queue<Exhibits> _autoExhibitsQueue = new Queue<Exhibits>();
        private Exhibits _destinationExhibits;
        #endregion

        [SerializeField]
        private float _maxViewingTime = 5f;
        private float _viewingTimer = 0f;

        public bool IsProcessing { get; private set; }
        public Action StartAutomaticProcessing, EndAutomaticProcessing;

        protected override void OnAwake()
        {
            base.OnAwake();
            _cameraController = FindFirstObjectByType<CameraController>();            
        }

        protected override void Initialize()
        {
            base.Initialize();
            if (_characterController.Brain == null)
            {
                RemoveAbility();
                return;
            }            
        }

        /// <summary>
        /// 자동 관람 실행 함수
        /// </summary>
        public void StartAutomaticViewing()
        {            
            _autoExhibitsQueue = SetExhibitsQueue();
            if (_autoExhibitsQueue == null || _autoExhibitsQueue.Count <= 0) return;
            _characterController.Type = CharacterType.AI;
            _cameraController.MovementType = CameraMovementType.Auto;
            SetNextPath();
            IsProcessing = true;
            _characterController.Brain.StoppingDistanceCallback += StoppingDistanceEvent;
            StartAutomaticProcessing?.Invoke();
        }

        /// <summary>
        /// 전시관 내 전시물들을 큐에 세팅하는 함수<br></br>
        /// 플레이어 캐릭터 기준, 가장 가까운 전시물 부터 시작해서 큐에 등록됨
        /// </summary>
        /// <returns></returns>
        private Queue<Exhibits> SetExhibitsQueue()
        {
            if (MainSceneManager.Instance.Exhibition.ExhibitsList == null || MainSceneManager.Instance.Exhibition.ExhibitsList.Count == 0) return null;
            var closeExhibits = MainSceneManager.Instance.Exhibition.ExhibitsList.OrderBy((exhibits) =>
            {
                var playerPos = CharacterTransform.position;
                var exhibitsPos = exhibits.transform.position;
                if (exhibits.ExhibitsType == Define.FileType.EMPTY) exhibitsPos *= float.PositiveInfinity;
                var result = (exhibitsPos - playerPos).magnitude;
                if (float.IsNaN(result)) result = float.PositiveInfinity;
                return result;
            }).First();
            var index = MainSceneManager.Instance.Exhibition.ExhibitsList.IndexOf(closeExhibits);
            Debug.Log(closeExhibits.name);
            var sortingList = new List<Exhibits>();
            for (int i = index; i < MainSceneManager.Instance.Exhibition.ExhibitsList.Count; i++)
            {
                if(MainSceneManager.Instance.Exhibition.ExhibitsList[i].ExhibitsType != Define.FileType.EMPTY)
                    sortingList.Add(MainSceneManager.Instance.Exhibition.ExhibitsList[i]);
            }

            for (int i = 0; i < index; i++)
            {
                if (MainSceneManager.Instance.Exhibition.ExhibitsList[i].ExhibitsType != Define.FileType.EMPTY)
                    sortingList.Add(MainSceneManager.Instance.Exhibition.ExhibitsList[i]);
            }
            return new Queue<Exhibits>(sortingList);
        }

        /// <summary>
        /// 자동 관람 종료 함수
        /// </summary>
        public void EndAutomaticViewing()
        {
            IsProcessing = false;
            _autoExhibitsQueue?.Clear();
            _destinationExhibits = null;
            _characterController.Type = CharacterType.Player;
            _cameraController.MovementType = CameraMovementType.Manual;
            _characterController.Brain.StoppingDistanceCallback -= StoppingDistanceEvent;
            EndAutomaticProcessing?.Invoke();
        }

        /// <summary>
        /// 큐에 등록된 다음 전시물을 목표 지점으로 세팅하는 함수 
        /// </summary>
        private void SetNextPath()
        {
            if(_autoExhibitsQueue.Count == 0)
            {
                EndAutomaticViewing();
                return;
            }

            _destinationExhibits = _autoExhibitsQueue.Dequeue();
            var destinationPos = _destinationExhibits.transform.position + (_destinationExhibits.transform.forward * 2f);
            destinationPos.y = CharacterTransform.position.y;

            var aiBrain = _characterController.Brain as AIBrain;
            aiBrain.TargetTransform = _destinationExhibits.transform;
            aiBrain.GoalLocation = destinationPos;
            _viewingTimer = 0f;
        }

        /// <summary>
        /// 전시물에 도착할 때 부터 관람 시간을 측정, 시간이 다 되면 다음 전시물로 이동<br></br>
        /// 전시물을 클릭하여 상호작용 시엔 관람 시간이 흐르지 않음
        /// </summary>
        protected override void AbilityInputAction()
        {
            if(_characterController.Brain.ArrivalAtDestination)
            {
                _viewingTimer += _characterController.IsInteraction ? 0f : Time.deltaTime;
                if(_viewingTimer >= _maxViewingTime)
                {
                    _viewingTimer = 0f;
                    SetNextPath();
                }
            }                
        }

        private void StoppingDistanceEvent()
        {
            _characterController.transform.position = _characterController.Brain.GoalLocation;
        }

        /// <summary>
        /// 상호작용 여부를 체크하여 캐릭터 ai 활성화 여부 결정 
        /// </summary>
        /// <param name="isInteraction"></param>
        protected override void Call_CharacterInteractionState(bool isInteraction)
        {
            if (IsProcessing && !isInteraction)
            {
                var aiBrain = _characterController.Brain as AIBrain;
                aiBrain.ActiveAI = true;
            }
        }
    }
}
