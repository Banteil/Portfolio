using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace Zeus
{
    public class TableManager : UnitySingleton<TableManager>
    {
        internal static bool IsNewGame { get; set; }
        private static PlayerData _playerData;
        internal static PlayerData CurrentPlayerData
        {
            get
            {
                if (_playerData == null)
                {
                    var data = GetSaveDatas();
                    data.LoadData();
                    _playerData = LoadPlayerData(0);
                }

                return _playerData;
            }
        }
        private static SaveData _saveData;
        protected override void _OnAwake()
        {
            ApplicationIsQuitting = false;
            SceneManager.activeSceneChanged += SceneChagned;
        }

        private void SceneChagned(Scene s0, Scene s1)
        {
            Resources.UnloadUnusedAssets();
        }

        internal static SaveData GetSaveDatas()
        {
            if (_saveData == null)
            {
                _saveData = Resources.Load<SaveData>("Table/SaveData");
            }

            return _saveData;
        }

        internal static void AutoSave()
        {
            _saveData.AutoSave(CurrentPlayerData);
        }

        internal static bool HasSaveData()
        {
            var data = GetSaveDatas();
            if (data == null)
            {
                Debug.LogError("Not Found SaveData !!!!");
                return false;
            }

            data.LoadData();

            return data.PlayerDatas != null && data.PlayerDatas.Count > 0;
        }

        internal static PlayerData LoadPlayerData(int slotIndex)
        {
            var data = GetSaveDatas();
            if (data != null)
                _playerData = data.GetPlayData(slotIndex);

            if (_playerData == null)
            {
                _playerData = new PlayerData();
                _playerData.Initailized();
            }

            return _playerData;
        }

        internal static void SavePlayerData(int slotIndex)
        {
            var data = GetSaveDatas();
            if (data != null)
            {
                data.DataSave(slotIndex, _playerData);
            }
        }

        internal static string GetString(int stringID)
        {
            var data = Resources.Load<StringTable>("Table/StringTable");
            if (data == null)
                return string.Empty;

            return data.GetString(stringID);
        }

        #region WeaponTable

        //internal void WeaponModelInitialized()
        //{
        //    int initCount = CurrentPlayerData.WeaponInventoryDatas.Count * 2;

        //    foreach (var item in CurrentPlayerData.WeaponInventoryDatas)
        //    {
        //        GetWeaponModel(item.TableID, true, result =>
        //        {
        //            --initCount;
        //            WeaponInit = initCount <= 0;
        //        });
        //    }

        //    foreach (var item in CurrentPlayerData.WeaponInventoryDatas)
        //    {
        //        GetWeaponModel(item.TableID, false, result =>
        //        {
        //            --initCount;
        //            WeaponInit = initCount <= 0;
        //        });
        //    }
        //}
        internal static WeaponTable GetWeaponTable()
        {
            var data = Resources.Load<WeaponTable>("Table/WeaponTable");
            return data;
        }

        internal static WeaponTableData GetWeaponTableData(int id)
        {
            var data = Resources.Load<WeaponTable>("Table/WeaponTable");
            return data.GetData(id);
        }

        //private IEnumerator _leftWeaponLoad;
        //private IEnumerator _rightWeaponLoad;
        //private Dictionary<int, GameObject[]> loadedWeapon;
        //internal void GetWeaponModel(int id, bool left, UnityAction<GameObject> callBack)
        //{
        //    loadedWeapon ??= new();

        //    var index = left ? 0 : 1;
        //    if (loadedWeapon.ContainsKey(id))
        //    {
        //        var array = loadedWeapon[id];
        //        if (array[index] != null)
        //        {
        //            callBack?.Invoke(array[index]);
        //            return;
        //        }
        //    }

        //    var data = GetWeaponTableData(id);
        //    if (data == null)
        //        callBack?.Invoke(null);

        //    var path = left ? data.LeftWeapon : data.RightWeapon;
        //    if (string.IsNullOrEmpty(path))
        //    {
        //        callBack?.Invoke(null);
        //        return;
        //    }

        //    var ie = left ? _leftWeaponLoad : _rightWeaponLoad;
        //    if (ie != null)
        //        StopCoroutine(ie);

        //    ie = GameObjectLoad(path, ob =>
        //    {
        //        if (ob == null)
        //        {
        //            Debug.LogError($"Not Found Object : {path}");
        //            callBack?.Invoke(null);
        //            return;
        //        }

        //        ob.SetActive(false);

        //        if (loadedWeapon.ContainsKey(id))
        //        {
        //            loadedWeapon[id][index] = ob;
        //            callBack?.Invoke(loadedWeapon[id][index]);
        //        }
        //        else
        //        {
        //            var array = new GameObject[2];
        //            array[index] = ob;
        //            loadedWeapon.Add(id, array);
        //            callBack?.Invoke(array[index]);
        //        }
        //    });
        //    StartCoroutine(ie);
        //}

        internal static int GetNextWeaponID(bool next)
        {
            var currentWeaponID = CurrentPlayerData.GetWeaponID();
            var index = CurrentPlayerData.WeaponInventoryDatas.FindIndex(_ => _.TableID == currentWeaponID);
            if (index == -1)
            {
                Debug.LogError($"Not Found Item ID  : {currentWeaponID}");
                return -1;
            }

            var nextIndex = index.NextBounderyNumber(next, CurrentPlayerData.WeaponInventoryDatas.Count - 1);
            return CurrentPlayerData.WeaponInventoryDatas[nextIndex].TableID;
        }
        internal static int GetNextWeaponID(TypeWeapon typeWeapon)
        {
            var currentWeaponID = CurrentPlayerData.GetWeaponID();

            CurrentPlayerData.WeaponInventoryDatas = CurrentPlayerData.WeaponInventoryDatas.OrderBy(_ => _.TableID).ToList();
            var list = new List<int>();
            foreach (var item in CurrentPlayerData.WeaponInventoryDatas)
            {
                var tableData = GetWeaponTableData(item.TableID);
                if (tableData.WeaponCategory == typeWeapon)
                {
                    list.Add(tableData.ID);
                }
            }

            if (list.Count < 0)
                return 0;

            var index = list.FindIndex(_ => _ == currentWeaponID);
            if (index == -1)
                index = 0;

            var nextIndex = index.NextBounderyNumber(true, list.Count - 1);
            return list[nextIndex];
        }
        #endregion 
        #region SkillTable
        internal static SkillTable GetSkillTable()
        {
            var data = Resources.Load<SkillTable>("Table/SkillTable");
            return data;
        }

        internal static SkillTableData GetSkillTableData(int id)
        {
            var data = Resources.Load<SkillTable>("Table/SkillTable");
            return data.GetData(id);
        }
        #endregion
        #region SoundTable
        internal static SoundTable GetSoundTable()
        {
            var data = Resources.Load<SoundTable>("Table/SoundTable");
            return data;
        }

        private static SoundTableData GetSoundTableData(int id)
        {
            var data = Resources.Load<SoundTable>("Table/SoundTable");
            if (data == null)
                return null;

            return data.GetData(id);
        }

        internal static AudioClip GetAudioClip(int tableId, bool random = false)
        {
            var data = GetSoundTableData(tableId);
            if (data == null)
                return null;

            var path = data.AssetName;

            if (random && data.RandomAssetName.Length > 0)
            {
                var index = UnityEngine.Random.Range(0, data.RandomAssetName.Length);
                path = data.RandomAssetName[index];
            }

            return Resources.Load<AudioClip>(path);
        }

        internal static AudioClip GetAudioClip(string assetName)
        {
            var tableData = Resources.Load<SoundTable>("Table/SoundTable");
            if (tableData == null)
                return null;

            var path = string.Empty;
            foreach (var item in tableData.TableDatas)
            {
                if (item.AssetName.Contains(assetName))
                {
                    path = item.AssetName;
                    break;
                }

                foreach (var data in item.RandomAssetName)
                {
                    if (data.Contains(assetName))
                    {
                        path = assetName;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return Resources.Load<AudioClip>(path);
        }

        internal void GetAudioClipAsync(int tableId, UnityAction<AudioClip> callBack)
        {
            var data = GetSoundTableData(tableId);
            if (data == null)
                callBack?.Invoke(null);

            StartCoroutine(AudioClipLoad(data.AssetName, callBack));
        }
        private IEnumerator AudioClipLoad(string path, UnityAction<AudioClip> callBack)
        {
            var req = Resources.LoadAsync(path);
            yield return req;

            callBack?.Invoke(req.asset as AudioClip);
        }

        #endregion
        #region EffectTable
        internal static AssetTableData GetEffectTableData(int tableID)
        {
            var data = Resources.Load<EffectTable>("Table/EffectTable");
            if (data == null)
                return null;

            return data.GetData(tableID);
        }

        internal void GetGameObjectAsync(string path, UnityAction<GameObject> callBack)
        {
            StartCoroutine(GameObjectLoad(path, callBack));
        }

        private IEnumerator GameObjectLoad(string path, UnityAction<GameObject> callBack)
        {
            var req = Resources.LoadAsync(path);
            yield return req;

            var ob = req.asset as GameObject;

            if (ob == null)
            {
                Debug.LogError($"Not Found Object path : {path}");
                callBack?.Invoke(null);
                yield break;
            }

            var result = Instantiate(ob);
            if (EffectsManager.Get() != null)
            {
                EffectsManager.Get().AddParticle(result);
            }

            if (AnimatorSpeedManager.Get() != null)
            {
                var anims = result.GetComponentsInChildren<Animator>();
                foreach (var item in anims)
                {
                    AnimatorSpeedManager.Get().AddAnimator(item);
                }
            }

            callBack?.Invoke(result);
            Resources.UnloadUnusedAssets();
        }

        internal static GameObject GetEffectGameObject(int tableID)
        {
            var data = GetEffectTableData(tableID);
            if (data == null)
                return null;

            return GetGameObject(data.AssetName);
        }

        internal static GameObject GetGameObject(string path)
        {
            var ob = Resources.Load<GameObject>(path);
            if (ob == null) return null;

            var result = Instantiate(ob);
            if (result == null) return null;

            if (EffectsManager.Get() != null)
            {
                EffectsManager.Get().AddParticle(result);
            }

            if (AnimatorSpeedManager.Get() != null)
            {
                var anims = result.GetComponentsInChildren<Animator>();
                foreach (var item in anims)
                {
                    AnimatorSpeedManager.Get().AddAnimator(item);
                }
            }

            return result;
        }
        #endregion
        #region SpriteLoad
        internal Sprite GetSprite(string path)
        {
            var sprite = Resources.Load<Sprite>(path);
            return sprite;
        }

        internal void GetSpriteAsync(string path, UnityAction<Sprite> callback)
        {
            StartCoroutine(SpriteLoad(path, callback));
        }

        private IEnumerator SpriteLoad(string path, UnityAction<Sprite> callBack)
        {
            var req = Resources.LoadAsync<Sprite>(path);
            yield return req;

            callBack?.Invoke(req.asset as Sprite);

            Resources.UnloadUnusedAssets();
        }
        #endregion
        #region QuestTable
        internal static QuestTable GetQuestTable()
        {
            var data = Resources.Load<QuestTable>("Table/QuestTable");
            return data;
        }
        #endregion
        #region BuffTable
        internal static BuffTableData GetBuffTableData(int tableID)
        {
            var data = Resources.Load<BuffTable>("Table/BuffTable");
            return data.GetData(tableID);
        }
        #endregion
        #region ConsumeTable
        internal static ConsumeTableData GetConsumeTableData(int tableID)
        {
            var data = Resources.Load<ConsumeTable>("Table/ConsumeTable");
            return data?.GetData(tableID);
        }
        #endregion
        #region ExcutionTable

        internal static ExcutionTable GetExcutionTable()
        {
            var data = Resources.Load<ExcutionTable>("Table/ExcutionTable");
            return data;
        }

        internal static ExcutionTableData GetExcutionTableData(int id)
        {
            var data = Resources.Load<ExcutionTable>("Table/ExcutionTable");
            return data.GetData(id);
        }

        internal static RuntimeAnimatorController GetAnimator(string path)
        {
            var animator = Resources.Load<RuntimeAnimatorController>(path);
            return animator;
        }

        internal static PlayableAsset GetCutScene(string path)
        {
            var cutScene = Resources.Load<PlayableAsset>(path);
            return cutScene;
        }
        internal static ExcutionBehaviour GetExcutionBehaviour(string path)
        {
            var excutionBehaviour = GetGameObject(path)?.GetComponent<ExcutionBehaviour>();
            return excutionBehaviour;
        }

        #endregion
        #region LootTable
        internal static LootTable GetLootTable()
        {
            var table = Resources.Load<LootTable>("Table/LootTable");
            return table;
        }
        internal static LootTableData GetLootTableData(int tableID)
        {
            var data = GetLootTable()?.GetData(tableID);
            return data;
        }
        #endregion
        #region RuneTable
        internal static RuneTableData GetRuneTableData(int tableID)
        {
            var data = Resources.Load<RuneTable>("Table/RuneTable");
            return data?.GetData(tableID);
        }
        #endregion

        protected override void _OnDestroy()
        {
            _playerData = null;
        }

        internal static int GetWeaponDamage()
        {
            var data = GetWeaponTableData(CurrentPlayerData.GetWeaponID());
            if (data == null)
                return 0;

            return data.Damage;
        }

        internal static int GetWeaponSkillID(int tableID, int index)
        {
            var selectIndex = 0;
            if (CurrentPlayerData.WeaponSkillSelectData.ContainsKey(tableID))
            {
                selectIndex = CurrentPlayerData.WeaponSkillSelectData[tableID][index];
            }
            else
            {
                //0~7은 슬롯1스킬 8부터 2슬롯인데 지금은 기본장착해주기위해서.
                var newIndex = new int[] { 0, 8 };
                CurrentPlayerData.WeaponSkillSelectData.Add(tableID, newIndex);
                selectIndex = newIndex[index];
            }

            var weaponTableData = GetWeaponTableData(tableID);
            if (weaponTableData == null)
                return 0;

            //Debug.Log($"index : {index} / selectIndex : {CurrentPlayerData.WeaponSkillSelectData[tableID][index]}");

            return weaponTableData.SkillDatas[selectIndex].SkillID;
        }

        internal static void SelectWeaponSkill(int skillTableID)
        {
            var tableID = CurrentPlayerData.GetWeaponID();
            var weaponTableData = GetWeaponTableData(tableID);
            var skillDataIndex = Array.FindIndex(weaponTableData.SkillDatas, x => x.SkillID == skillTableID);
            var index = skillDataIndex < 8 ? 0 : 1;
            if (CurrentPlayerData.WeaponSkillSelectData.ContainsKey(tableID))
            {
                CurrentPlayerData.WeaponSkillSelectData[tableID][index] = skillDataIndex;
            }
            else
            {
                //0~7은 슬롯1스킬 8부터 2슬롯인데 지금은 기본장착해주기위해서.
                var newIndex = new int[] { 0, 8 };
                newIndex[index] = skillDataIndex;
                CurrentPlayerData.WeaponSkillSelectData.Add(tableID, newIndex);
            }

            //Debug.Log($"skillTableID : {skillTableID} / index : {index} / skillDataIndex : {CurrentPlayerData.WeaponSkillSelectData[tableID][index]}");

            AutoSave();
        }
    }
}
