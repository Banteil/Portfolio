using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public enum TypeEquipPosition
    {
        NONE,
        WEAPON,
        BOW,
        HEAD,
        BODY,
        HAND,
        FOOT,

        CONSUME,
        RUNE,
    }

    [System.Serializable]
    public class InventoryData
    {
        public TypeEquipPosition InventoryPosition;
        public int TableID;
    }
    [System.Serializable]
    public class InventoryDataAmount : InventoryData
    {
        public int Amount;
    }
    [System.Serializable]
    public class SaveZoneData
    {
        public SaveZoneData()
        {
            ClearZoneIDs = new();
        }

        ~SaveZoneData()
        {
            ClearZoneIDs = null;
        }

        public string SceneName;
        public int ZoneID;
        public HashSet<int> ClearZoneIDs;
    }

    [System.Serializable]
    public class PlayerData
    {
        public List<InventoryData> WeaponInventoryDatas;
        public List<InventoryDataAmount> ConsumeInventoryDatas;
        public List<InventoryDataAmount> RuneInventoryDatas;
        public List<QuestSerializeData> ProgressQuestDatas;
        public List<int> ClearQuestIDs;
        public int Coins;
        public SaveZoneData SaveZoneData;
        public Dictionary<int, int[]> WeaponSkillSelectData = new Dictionary<int, int[]>();
        internal bool AutoSaveData;

        internal void AddWeapon(int itemID, TypeEquipPosition position = TypeEquipPosition.NONE)
        {
            var inventoryData = new InventoryData()
            {
                InventoryPosition = position,
                TableID = itemID,
            };

            WeaponInventoryDatas.Add(inventoryData);
        }

        internal void Initailized()
        {
            WeaponInventoryDatas ??= new();
            ConsumeInventoryDatas ??= new();
            RuneInventoryDatas ??= new();
            ProgressQuestDatas ??= new();
            ClearQuestIDs ??= new();
            SaveZoneData = null;
            SaveZoneData ??= new();
            WeaponSkillSelectData ??= new();

            WeaponInventoryDatas.Clear();
            ConsumeInventoryDatas.Clear();
            RuneInventoryDatas.Clear();
            ProgressQuestDatas.Clear();
            ClearQuestIDs.Clear();
            WeaponSkillSelectData.Clear();

            AddWeapon(6002);
            AddWeapon(6003, TypeEquipPosition.WEAPON);
            AddWeapon(6004, TypeEquipPosition.BOW);

            AddConsume(6102, 2, TypeEquipPosition.CONSUME);
            AddConsume(6103, 2);
            AddConsume(6104, 2);

            AddRune(4101, TypeEquipPosition.RUNE);
            AddRune(4102); AddRune(4103); AddRune(4104); AddRune(4105); AddRune(4106); AddRune(4107); AddRune(4108);
        }

        internal int GetWeaponID(TypeEquipPosition findPosition = TypeEquipPosition.WEAPON)
        {
            var ob = WeaponInventoryDatas.Find(_ => _.InventoryPosition == findPosition);
            if (ob != null)
            {
                return ob.TableID;
            }

            return 0;
        }

        internal InventoryData GetInventoryData(int itemID)
        {
            var data = WeaponInventoryDatas.Find(_ => _.TableID == itemID);

            return data;
        }

        #region QuestData
        // 리스트에서 해당 퀘스트를 ID로 검색후 없다면 생성
        internal bool AddQuest(QuestSO quest, out QuestSerializeData data)
        {
            data = null;

            if (quest == null) return false;

            // 이미 클리어한 내역이 있는 퀘스트라면 추가하지 않음
            if (IsQuestClear(quest.ID)) return false;

            // 현재 진행중인 퀘스트가 있다면 추가하지 않음
            data = GetProgressQuestData(quest.ID);
            if (data != null) return false;

            // 퀘스트정보 기록
            data = new QuestSerializeData
            {
                ID = quest.ID,
                Values = new int[quest.Steps.Length],
                IsDone = false,
            };
            ProgressQuestDatas.Add(data);
            return true;
        }
        internal bool RemoveQuest(int questID)
        {
            var quest = GetProgressQuestData(questID);
            return ProgressQuestDatas.Remove(quest);
        }
        internal QuestSerializeData GetProgressQuestData(int questID)
        {
            var quest = ProgressQuestDatas.Find(x => x.ID == questID);
            return quest;
        }
        internal bool HasQuest(int questID)
        {
            return ProgressQuestDatas.Exists(x => x.ID == questID);
        }
        internal bool IsQuestClear(int questID)
        {
            return ClearQuestIDs.Exists(x => x == questID);
        }
        internal void ClearQuest(int questID)
        {
            if (RemoveQuest(questID))
                ClearQuestIDs.Add(questID);
        }
        #endregion
        #region ConsumeData
        internal void AddConsume(int consumeID, int amount = 1, TypeEquipPosition position = TypeEquipPosition.NONE)
        {
            var index = ConsumeInventoryDatas.FindIndex(x => x.TableID == consumeID);
            if (index == -1)
            {
                var consumeData = new InventoryDataAmount()
                {
                    InventoryPosition = position,
                    TableID = consumeID,
                    Amount = amount,
                };
                ConsumeInventoryDatas.Add(consumeData);
            }
            else
            {
                ConsumeInventoryDatas[index].Amount += amount;
            }
        }
        internal bool RemoveConsume(int consumeID)
        {
            var index = ConsumeInventoryDatas.FindIndex(x => x.TableID == consumeID);
            if (index == -1) return false;
            if (ConsumeInventoryDatas[index].Amount > 0)
                --ConsumeInventoryDatas[index].Amount;
            else return false;
            //if (amount <= 0) ConsumeInventoryDatas.RemoveAt(index);
            return true;
        }
        internal void EquipConsume(int consumeID)
        {
            var currentConsume = ConsumeInventoryDatas.Find(x => x.InventoryPosition == TypeEquipPosition.CONSUME);
            if (currentConsume != null)
                currentConsume.InventoryPosition = TypeEquipPosition.NONE;
            var consumeData = GetConsumeData(consumeID);
            if (consumeData != null)
                consumeData.InventoryPosition = TypeEquipPosition.CONSUME;
        }
        internal int GetEquipConsumeID()
        {
            var cinsumeData = ConsumeInventoryDatas.Find(x => x.InventoryPosition == TypeEquipPosition.CONSUME);
            if (cinsumeData != null) return cinsumeData.TableID;
            else return 0;
        }
        internal InventoryDataAmount GetConsumeData(int consumeID)
        {
            var index = ConsumeInventoryDatas.FindIndex(x => x.TableID == consumeID);
            if (index == -1) return null;
            return ConsumeInventoryDatas[index];
        }
        #endregion
        #region CoinData
        public bool EnoughCoin(int value)
        {
            return Coins >= value;
        }
        public void IncreaseCoin(int value)
        {
            Coins += value;
        }
        public bool DecreaseCoin(int value)
        {
            if (!EnoughCoin(value)) return false;
            else Coins -= value;
            return true;
        }
        #endregion
        #region RuneData
        internal void AddRune(int runeID, TypeEquipPosition position = TypeEquipPosition.NONE)
        {
            var index = RuneInventoryDatas.FindIndex(x => x.TableID == runeID);
            if (index == -1)
            {
                var runeData = new InventoryDataAmount()
                {
                    InventoryPosition = position,
                    TableID = runeID,
                    Amount = 1,
                };
                RuneInventoryDatas.Add(runeData);
            }
            else
            {
                RuneInventoryDatas[index].Amount++;
            }
        }
        internal bool RemoveRune(int runeID)
        {
            var index = RuneInventoryDatas.FindIndex(x => x.TableID == runeID);
            if (index == -1) return false;
            if (RuneInventoryDatas[index].Amount > 0)
                --RuneInventoryDatas[index].Amount;
            else return false;
            return true;
        }
        internal void EquipRune(int runeID)
        {
            var currentRune = RuneInventoryDatas.Find(x => x.InventoryPosition == TypeEquipPosition.RUNE);
            if (currentRune != null)
                currentRune.InventoryPosition = TypeEquipPosition.NONE;
            var runeData = GetRuneData(runeID);
            if (runeData != null)
                runeData.InventoryPosition = TypeEquipPosition.RUNE;
        }
        internal int GetEquipRuneID()
        {
            var runeData = RuneInventoryDatas.Find(x => x.InventoryPosition == TypeEquipPosition.RUNE);
            if (runeData != null) return runeData.TableID;
            else return 0;
        }
        internal InventoryDataAmount GetRuneData(int runeID)
        {
            var index = RuneInventoryDatas.FindIndex(x => x.TableID == runeID);
            if (index == -1) return null;
            return RuneInventoryDatas[index];
        }
        #endregion
    }

    [CreateAssetMenu(fileName = "SaveData", menuName = "Zeus/Table/SaveData")]
    [System.Serializable]
    public class SaveData : ScriptableObject
    {
        public List<PlayerData> PlayerDatas;

        internal void LoadData()
        {
            var jsonString = PlayerPrefs.GetString("AutoSaveData", string.Empty);
            if (string.IsNullOrEmpty(jsonString))
            {
                PlayerDatas = null;
                return;
            }

            PlayerDatas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PlayerData>>(jsonString);
        }

        internal PlayerData GetPlayData(int slotIndex)
        {
            if (PlayerDatas == null || PlayerDatas.Count <= slotIndex)
            {
                return null;
            }

            return PlayerDatas[slotIndex];
        }

        internal void AutoSave(PlayerData data)
        {
            var index = 0;
            if (PlayerDatas != null)
            {
                index = PlayerDatas.Count.NextBounderyNumber(true, 0);
            }
            DataSave(index, data);
        }

        internal void DataSave(int slotIndex, PlayerData data, bool auto = true)
        {
            PlayerDatas ??= new();

            if (PlayerDatas.Count > slotIndex)
                PlayerDatas[slotIndex] = data;
            else
                PlayerDatas.Add(data);

            var saveData = Newtonsoft.Json.JsonConvert.SerializeObject(PlayerDatas);
            data.AutoSaveData = auto;
            PlayerPrefs.SetString("AutoSaveData", saveData);
        }
    }
}
