using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{

    public class CharacterObjectManager : BaseObject<CharacterObjectManager>
    {
        public UnityAction<Character> CallCharacterAdd;
        public UnityAction<Character> CallCharacterRemove;

        List<Character> _allCharacterList;
        Dictionary<string, List<IControlAIZeus>> _characterGroups;

        List<HitBack> _hitBackList;

        protected override void _OnAwake()
        {
            _allCharacterList ??= new List<Character>();
            _characterGroups ??= new Dictionary<string, List<IControlAIZeus>>();
            _hitBackList ??= new List<HitBack>();
        }

        #region 캐릭터 관리 함수

        public void AddCharacterList(Character character)
        {
            if (_allCharacterList.Exists(x => x.GUID.Equals(character.GUID))) return;
            _allCharacterList.Add(character);
            CallCharacterAdd?.Invoke(character);
            Debug.Log($"Add Character List : {character.gameObject.name}");
        }

        public void RemoveCharacterList(GameObject obj)
        {
            var character = obj.GetComponent<Character>();
            int index = _allCharacterList.Select(x => x.GUID).ToList().IndexOf(character.GUID);
            if (index < 0) return;
            _allCharacterList.RemoveAt(index);
            Debug.Log($"Remove Character List : {obj.name}");
            if (SceneLoadManager.IsLoading) return;
            CallCharacterRemove?.Invoke(character);
        }

        public Character GetPlayerbleCharacter()
        {
            var player = _allCharacterList.Find(x => x.TypeCharacter.Equals(TypeCharacter.PLAYERBLE));
            return player;
        }

        public void AllAIDestroy()
        {
            var destroyList = _allCharacterList.FindAll(x => x.TypeCharacter.Equals(TypeCharacter.AI));
            foreach (var character in destroyList)
            {
                Destroy(character.gameObject);
            }
        }

        public void AllZoneAIDestroy(int zoneID)
        {
            var destroyList = _allCharacterList.FindAll(x => x.TypeCharacter == TypeCharacter.AI && x.ZoneID == zoneID);
            foreach (var character in destroyList)
            {
                Destroy(character.gameObject);
            }
        }

        public void AddHitBackList(string guid, Damage damage)
        {
            var character = _allCharacterList.Find(x => x.GUID.Equals(guid));
            if (character == null) return;
            var hitBack = new HitBack(character, damage);
            _hitBackList.Add(hitBack);

            //Debug.Log($"Hit Back Success : {character.gameObject.name} / _hitBackList Count : {_hitBackList.Count}");
        }

        private void FixedUpdate()
        {
            HitBackProcess();
        }

        void HitBackProcess()
        {
            for (int i = _hitBackList.Count - 1; i >= 0; --i)
            {
                if (_hitBackList[i].IsEnd)
                {
                    _hitBackList.RemoveAt(i);
                    continue;
                }

                _hitBackList[i].Process();
            }
        }

        #endregion

        #region AI 그룹 관리 함수

        public void AddGroupMember(IControlAIZeus zeusAI)
        {
            if (string.IsNullOrEmpty(zeusAI.GroupID)) return;

            if (!_characterGroups.ContainsKey(zeusAI.GroupID))
            {
                var characterList = new List<IControlAIZeus>();
                characterList.Add(zeusAI);
                _characterGroups.Add(zeusAI.GroupID, characterList);
            }
            else
            {
                if (_characterGroups[zeusAI.GroupID].Exists(x => x.GUID.Equals(zeusAI.GUID))) return;
                _characterGroups[zeusAI.GroupID].Add(zeusAI);
            }
        }

        public void RemoveGroupMember(IControlAIZeus zeusAI)
        {
            if (!_characterGroups.ContainsKey(zeusAI.GroupID)) return;
            var aiList = _characterGroups[zeusAI.GroupID];

            int index = aiList.Select(T => T.GUID).ToList().IndexOf(zeusAI.GUID);
            if (index < 0) return;
            aiList.RemoveAt(index);

            if (aiList.Count <= 0)
                _characterGroups.Remove(zeusAI.GroupID);
        }

        public bool OtherMemberIsAttacking(IControlAIZeus zeusAI, int memberNum = 1)
        {
            if (zeusAI.IsAttacking || _characterGroups[zeusAI.GroupID].Count <= memberNum) return false;

            int check = memberNum;
            foreach (var enemy in _characterGroups[zeusAI.GroupID])
            {
                if (enemy.GUID.Equals(zeusAI.GUID)) continue;
                if (enemy.IsAttacking)
                {
                    check--;
                    //Debug.Log(zeusAI.gameObject.name + "의 기준, " + enemy.gameObject.name + "이 공격 중 체크");
                    if (check <= 0)
                        return true;
                }
            }
            return false;
        }

        public bool IsEmptyGroup(string groupID)
        {
            if (!_characterGroups.ContainsKey(groupID)) return false;
            return _characterGroups[groupID].Count <= 0;
        }

        public bool IsAlone(IControlAIZeus zeusAI)
        {
            if (!_characterGroups.ContainsKey(zeusAI.GroupID)) return true;
            bool value = _characterGroups[zeusAI.GroupID].Count <= 1;
            return value;
        }

        public int GroupSideMoveDirection(IControlAIZeus zeusAI)
        {
            int direction = zeusAI.StrafeCombatSide;
            if (IsAlone(zeusAI)) return direction;

            float mag = float.PositiveInfinity;
            var saveDir = zeusAI.transform.right;
            foreach (var enemy in _characterGroups[zeusAI.GroupID])
            {
                if (enemy.GUID.Equals(zeusAI.GUID)) continue;
                var dir = enemy.transform.position - zeusAI.transform.position;
                if (mag < dir.magnitude) continue;
                mag = dir.magnitude;
                saveDir = dir;
            }

            var cross = Vector3.Cross(zeusAI.transform.right, saveDir).y;
            if (cross > 0) direction = -1;
            else if (cross < 0) direction = 1;
            else direction = 0;
            //Debug.Log($"{zeusAI.gameObject.name}의 좌우 이동 방향 : {direction}");
            return direction;
        }

        internal Character[] GetZoneCharacter(int removeCharacterZoneID, TypeCharacter charactertype)
        {
            var list = _allCharacterList.FindAll(_ => _.TypeCharacter.Equals(charactertype) && _.ZoneID == removeCharacterZoneID);
            return list.ToArray();
        }
        #endregion
    }

    public class HitBack
    {
        Transform _tr;
        HitBackInfo _hitBackInfo;
        Vector3 _hitBackDir;
        float _midHeight;
        float _time = 0f;
        Animator _animator;
        Rigidbody _rigidbody;

        public bool IsEnd { get; private set; }

        public HitBack(Character character, Damage damage)
        {
            _tr = character.transform;
            _midHeight = _tr.GetComponent<CapsuleCollider>().height * 0.5f;
            _hitBackInfo = damage.AttackerHitProperties.HitBackInfo;
            switch (_hitBackInfo.Base)
            {
                case HitBackInfo.HitBackBase.HITPOINT:
                    _hitBackDir = character.transform.position - damage.HitPosition;
                    break;
                case HitBackInfo.HitBackBase.SENDER:
                    _hitBackDir = character.transform.position - damage.AttackPosition;
                    break;
                case HitBackInfo.HitBackBase.CAMERAFRONT:
                    _hitBackDir = Camera.main.transform.forward;
                    break;
                default:
                    _hitBackDir = damage.Sender.forward;
                    break;
            }

            _animator = character.GetComponent<Animator>();
            _rigidbody = character.GetComponent<Rigidbody>();

            _hitBackDir.y = 0;
            if (_hitBackInfo.Duration <= 0) _hitBackInfo.Duration = 0.1f;
            IsEnd = false;
        }

        //physics.simulator의 영향을 받기때문에 Time.deltaTime사용한다.
        public void Process()
        {
            if (_rigidbody == null)
            {
                IsEnd = true;
                return;
            }
            else if (_time >= _hitBackInfo.Duration)
            {
                IsEnd = true;
                _rigidbody.velocity = Vector3.zero;
                return;
            }

            if (GameTimeManager.Instance.FixedDeltaTime == 0)
                return;

            _time += GameTimeManager.Instance.FixedDeltaTime;

            var direction = _hitBackDir;
            var evalue = _time / _hitBackInfo.Duration;
            var speedMultiplier = _hitBackInfo.Curve.Evaluate(evalue);

            direction.y = 0;
            //magnitude : 벡터의 길이
            direction = direction.normalized * Mathf.Clamp(direction.magnitude, 0, 1f);
            Vector3 targetPosition = /*(_animator != null && _animator.applyRootMotion ? _animator.rootPosition : _rigidbody.position)*/_rigidbody.position + direction *
                (_hitBackInfo.Speed * speedMultiplier) * GameTimeManager.Instance.FixedDeltaTime; //(_animator != null && _animator.applyRootMotion ? GameTimeManager.Instance.DeltaTime : GameTimeManager.Instance.FixedDeltaTime);

            //Debug.Log($"HitBack Process direction : {direction} / evalue : {evalue} /  speedMultiplier : {speedMultiplier} / targetPosition : {targetPosition}");

            Vector3 targetVelocity = (targetPosition - _tr.position) / GameTimeManager.Instance.FixedDeltaTime; //(_animator.applyRootMotion ? GameTimeManager.Instance.DeltaTime : GameTimeManager.Instance.FixedDeltaTime);

            targetVelocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = targetVelocity;
        }
    }
}

