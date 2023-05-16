using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zeus
{
    [System.Serializable]
    public struct ZoneObjectsInfo
    {
        public string Name;
        public Vector3 SpawnPosition;
        public Quaternion SpawnRotation;
    }

    [System.Serializable]
    public class ZoneInfo
    {
        public int ZoneID;
        public int AreaID;
        public TypeHitMaterial GroundMaterial;

        public ZoneInfo(ZoneInfo info)
        {
            GroundMaterial = info.GroundMaterial;
            ZoneID = info.ZoneID;
        }
    }

    [RequireComponent(typeof(Collider))]
    public class ZoneData : MonoBehaviour
    {
        public ZoneInfo Info;
        public Transform PlayerStartPosition;
        public List<ZoneData> BeforeZones = new List<ZoneData>();
        public List<ZoneData> AfterZones = new List<ZoneData>();

        internal bool CharacterClear;
        public List<ZoneObjectsInfo> ZoneCharacterInfos = new List<ZoneObjectsInfo>();
        public List<ZoneObjectsInfo> ZoneObjectInfos = new List<ZoneObjectsInfo>();

        protected ZoneEventData[] zoneEventDatas;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
            col.enabled = true;
            zoneEventDatas = GetComponentsInChildren<ZoneEventData>();
        }

        private IEnumerator Start()
        {
            //이미 클리어한 존인지 판단.
            CharacterClear = TableManager.CurrentPlayerData.SaveZoneData.ClearZoneIDs != null ? TableManager.CurrentPlayerData.SaveZoneData.ClearZoneIDs.Contains(Info.ZoneID) : false;
            
            yield return new WaitUntil(() => ZoneDataManager.Get() != null);
            foreach (var eventData in zoneEventDatas) { eventData.Initialize(); }
            ZoneDataManager.Get().ZoneClearCallback?.Invoke();
        }

        void CharacterAddZone(Collider other)
        {
            var combatManager = other.GetComponent<CombatManager>();
            if (combatManager == null) return;

            var character = other.GetComponentInChildren<Character>(true);
            if (character == null)
                return;

            //씬에 세워져 있는 케릭터 첫등록.
            if (combatManager.ZoneInfo == null)
            {
                combatManager.ZoneInfo = new ZoneInfo(Info);
                character.ZoneID = combatManager.ZoneInfo.ZoneID;
            }
            else
            {
                //이미 등록된 아이다 나가주세요.
                if (character.TypeCharacter != TypeCharacter.PLAYERBLE)
                    return;
            }

            if (character.TypeCharacter != TypeCharacter.PLAYERBLE)
            {
                //우리존 아이가 아니다.
                if (character.ZoneID != Info.ZoneID)
                    return;

                if (CharacterClear)
                {
                    Destroy(other.gameObject);
                    return;
                }

                var info = new ZoneObjectsInfo();
                info.Name = other.name;
                info.SpawnPosition = other.transform.position;
                info.SpawnRotation = other.transform.rotation;

                ZoneCharacterInfos.Add(info);
            }
            else
            {
                combatManager.ZoneInfo = new ZoneInfo(Info);
                character.ZoneID = combatManager.ZoneInfo.ZoneID;
                TableManager.AutoSave();
            }
        }

        void ObjectAddZone(Collider other)
        {
            var zoneObject = other.GetComponent<ZoneObject>();
            if (zoneObject == null) return;

            //씬에 세워져 있는 오브젝트 첫등록.
            if (zoneObject.ZoneInfo == null)
            {
                zoneObject.ZoneInfo = new ZoneInfo(Info);
            }
            else
                return;

            if (zoneObject.ZoneInfo.ZoneID != Info.ZoneID)
                return;

            //이 부분 처리를 어떻게 할지 생각 중
            //if (CharacterClear)
            //{
            //    Destroy(zoneObject.gameObject);
            //    return;
            //}

            ZoneObjectInfos.Add(zoneObject.ObjectInfo);
        }

        private void OnTriggerEnter(Collider other)
        {
            CharacterAddZone(other);
            ObjectAddZone(other);
        }

        //존 초기화.
        internal void Initialized()
        {
            if (ZoneObjectInfos.Count > 0)
            {
                var objects = GameObject.Find("Objects").transform;
                if (objects == null)
                {
                    objects = new GameObject("Objects").transform;
                }
                var boxCol = GetComponent<BoxCollider>();
                var zoneObjects = Physics.OverlapBox(transform.position, boxCol.size * 0.5f, transform.rotation).ToList().FindAll((x) => x.gameObject.GetComponent<ZoneObject>() != null);
                foreach (var item in zoneObjects)
                {
                    var zoneObject = item.GetComponent<ZoneObject>();
                    if (zoneObject.ZoneInfo == null) continue;
                    Destroy(item.gameObject);
                }

                foreach (var item in ZoneObjectInfos)
                {
                    var ob = TableManager.GetGameObject($"Prefabs/Object/{item.Name}");
                    if (ob == null)
                    {
                        Debug.LogError($"Not Found LogError {item.Name}");
                        continue;
                    }

                    ob.name = ob.name.Replace("(Clone)", "");
                    ob.transform.SetParent(objects);
                    ob.transform.SetPositionAndRotation(item.SpawnPosition, item.SpawnRotation);
                }
            }
            ZoneObjectInfos.Clear();

            if (ZoneCharacterInfos.Count > 0)
            {
                var enemies = GameObject.Find("Enemies").transform;
                if (enemies == null)
                {
                    enemies = new GameObject("Enemies").transform;
                    enemies.tag = "Enemy";
                }
                CharacterObjectManager.Get().AllZoneAIDestroy(Info.ZoneID);

                foreach (var item in ZoneCharacterInfos)
                {
                    var ob = TableManager.GetGameObject($"Prefabs/Character/{item.Name}");
                    if (ob == null)
                    {
                        Debug.LogError($"Not Found LogError {item.Name}");
                        continue;
                    }

                    ob.name = ob.name.Replace("(Clone)", "");
                    ob.transform.SetParent(enemies);
                    ob.transform.SetPositionAndRotation(item.SpawnPosition, item.SpawnRotation);
                }
            }
            ZoneCharacterInfos.Clear();
        }
    }
}
