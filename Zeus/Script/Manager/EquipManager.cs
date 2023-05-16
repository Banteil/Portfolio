using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class EquipManager : BaseObject<EquipManager>
    {
#if UNITY_EDITOR
        public GameObject[] EditorPreLoadObects;
#endif

        public Action<WeaponTableData> CallWeaponChange;
        public Action<bool> CallWeaponDissolveComplete;
        //현재 장비한아이템.
        internal int EquipedWeaponID { get; private set; }

        private Dictionary<int, GameObject[]> _weaponModels;
        private Dictionary<int, List<Material>> _dissolveMaterials;
        private float[] _weaponLifeTimes = new float[2];

        private void WeaponModelInitialized()
        {
            _weaponModels ??= new();
            _dissolveMaterials ??= new();
            var weaponTable = TableManager.GetWeaponTable();
            foreach (var item in weaponTable.weaponTableDatas)
            {
                var weaponModel = new GameObject[2];
                if (!string.IsNullOrEmpty(item.LeftWeapon))
                {
                    weaponModel[0] = TableManager.GetGameObject(item.LeftWeapon);
                    weaponModel[0].transform.SetParent(transform, false);
                    weaponModel[0].SetActive(false);
                }
                if (!string.IsNullOrEmpty(item.RightWeapon))
                {
                    weaponModel[1] = TableManager.GetGameObject(item.RightWeapon);
                    weaponModel[1].transform.SetParent(transform, false);
                    weaponModel[1].SetActive(false);
                }
                _weaponModels.Add(item.ID, weaponModel);
            }
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => AnimatorSpeedManager.Get().IsInitializedComplete);

#if UNITY_EDITOR
            for (int i = 0; i < EditorPreLoadObects.Length; i++)
            {
                var ob = EditorPreLoadObects[i];
                if (ob != null)
                    Destroy(ob);
            }
#endif

            WeaponModelInitialized();

            var playerData = TableManager.CurrentPlayerData;
            //저장되어있는 장비아이템.

            Debug.Log("Equip WeaponData ID = " + playerData.GetWeaponID());

            Equip(TypeEquipPosition.WEAPON, playerData.GetWeaponID());
        }

        internal void Equip(TypeEquipPosition position, int itemID)
        {
            var tableData = TableManager.GetWeaponTableData(itemID);
            if (tableData == null)
            {
                Debug.LogError($"Not Found Weapon Table ID : {itemID}");
                return;
            }

            switch (position)
            {
                case TypeEquipPosition.NONE:
                    break;
                case TypeEquipPosition.WEAPON:
                    {
                        //같은 아이템이면 맨손으로 돌아옵니다
                        if (EquipedWeaponID == itemID)
                            return;

                        ReleaseWeapon((int)TypeGrapHand.LEFT);
                        ReleaseWeapon((int)TypeGrapHand.RIGHT);

                        EquipedWeaponID = itemID;

                        var playerData = TableManager.CurrentPlayerData;
                        var currentEquipmentitem = playerData.WeaponInventoryDatas.Find(_ => _.InventoryPosition == TypeEquipPosition.WEAPON);
                        currentEquipmentitem.InventoryPosition = TypeEquipPosition.NONE;
                        var equipmentitem = playerData.WeaponInventoryDatas.Find(_ => _.TableID == itemID);
                        equipmentitem.InventoryPosition = TypeEquipPosition.WEAPON;
                        WeaponEquip(TypeEquipPosition.WEAPON);
                        if (PlayerUIManager.Get() != null)
                        {
                            PlayerUIManager.Get().RefreshWeaponIcon();
                        }
                    }
                    break;
                case TypeEquipPosition.HEAD:
                    break;
                case TypeEquipPosition.BODY:
                    break;
                case TypeEquipPosition.BOW:
                    {
                        EquipedWeaponID = itemID;
                        WeaponEquip(TypeEquipPosition.BOW);
                        if (PlayerUIManager.Get() != null)
                        {
                            PlayerUIManager.Get().RefreshWeaponIcon();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void ReleaseWeapon(int index)
        {
            if (!_weaponModels.ContainsKey(EquipedWeaponID))
                return;

            _weaponLifeTimes[index] = 0f;

            //mesh effect off;
            var models = _weaponModels[EquipedWeaponID];

            var item = models[index];
            if (item == null)
                return;

            DissolveDummy(item);

            EffectsManager.Get().ReleaseWeaponEffect(item);
            item.gameObject.SetActive(false);

            foreach (var data in models)
            {
                if (data == null)
                    continue;

                if (data.gameObject.activeSelf)
                    return;
            }

            CallWeaponDissolveComplete?.Invoke(false);
        }

        private void DissolveDummy(GameObject ob)
        {
            var dummy = Instantiate(ob, ob.transform.position, ob.transform.rotation);
            dummy.transform.SetParent(null);
            dummy.transform.localScale = Vector3.one;
            var renderers = dummy.GetComponentsInChildren<Renderer>();
            var find = false;
            foreach (var item in renderers)
            {
                foreach (var mat in item.materials)
                {
                    if (mat.HasProperty("_AdvancedDissolveCutoutStandardClip"))
                    {
                        mat.DOFloat(1f, "_AdvancedDissolveCutoutStandardClip", 0.7f).SetEase(Ease.OutSine).onComplete = () =>
                        {
                            Destroy(dummy);
                        };

                        find = true;
                    }
                }
            }

            EffectsManager.Get().ReleaseWeaponEffect(dummy);

            if (!find)
                Destroy(dummy);
        }

        public void WeaponEquip(TypeEquipPosition position)
        {
            var playdata = TableManager.CurrentPlayerData;
            var equipWeapon = playdata.WeaponInventoryDatas.Find(x => x.InventoryPosition == position);
            if (equipWeapon == null)
                return;

            var weaponTableData = TableManager.GetWeaponTableData(equipWeapon.TableID);
            if (weaponTableData == null)
                return;

            CallWeaponChange?.Invoke(weaponTableData);
        }

        internal GameObject GetWeaponModel(int index)
        {
            if (!_weaponModels.ContainsKey(EquipedWeaponID))
                return null;

            var models = _weaponModels[EquipedWeaponID];
            if (models.Length <= index)
                return null;

            var ob = models[index];
            if (ob == null)
            {
                var tableData = TableManager.GetWeaponTableData(EquipedWeaponID);
                if ((TypeGrapHand)index == TypeGrapHand.LEFT && !string.IsNullOrEmpty(tableData.LeftWeapon))
                {
                    ob = TableManager.GetGameObject(tableData.LeftWeapon);
                }
                if ((TypeGrapHand)index == TypeGrapHand.RIGHT && !string.IsNullOrEmpty(tableData.RightWeapon))
                {
                    ob = TableManager.GetGameObject(tableData.RightWeapon);
                }

                if (ob == null)
                    return null;
            }
            ob.SetActive(true);

            SetDissolve(ob);

            return ob;
        }

        private void SetDissolve(GameObject ob)
        {
            var renderers = ob.GetComponentsInChildren<Renderer>();
            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    foreach (var item in renderer.materials)
                    {
                        if (item.HasProperty("_AdvancedDissolveCutoutStandardClip"))
                        {
                            item.DOKill();
                            item.SetFloat("_AdvancedDissolveCutoutStandardClip", 1f);
                            if (_dissolveMaterials.ContainsKey(ob.GetInstanceID()))
                            {
                                var list = _dissolveMaterials[ob.GetInstanceID()];
                                list.Add(item);
                            }
                            else
                            {
                                var list = new List<Material>();
                                list.Add(item);
                                _dissolveMaterials.Add(ob.GetInstanceID(), list);
                            }
                        }
                    }
                }
            }
        }

        internal void DoWeaponDissolve(GameObject ob, int index, float duration, float lifeTime)
        {
            //현재 장비한 무기가 맞는지 체크.
            var models = _weaponModels[EquipedWeaponID];
            if (models.Length <= index)
                return;

            var model = models[index];
            if (model == null)
                return;

            if (ob != model)
            {
                Debug.LogError("Miss Match Object");
                return;
            }
            //////////////

            if (!_dissolveMaterials.ContainsKey(ob.GetInstanceID()))
            {
                return;
            }

            _weaponLifeTimes[index] = lifeTime;

            if (!ob.activeSelf)
            {
                ob.SetActive(true);
                SetDissolve(ob);
            }

            var materialsList = _dissolveMaterials[ob.GetInstanceID()];
            foreach (var material in materialsList)
            {
                if (material == null)
                {
                    Debug.LogError("dissolveMaterial is null");
                    return;
                }

                if (!material.HasProperty("_AdvancedDissolveCutoutStandardClip"))
                    continue;

                material.DOKill();
                material.DOFloat(0f, "_AdvancedDissolveCutoutStandardClip", duration).SetEase(Ease.InSine).onComplete = () =>
                {
                    material.DOKill();
                    EffectsManager.Get().SetWeaponEffect(ob, index == 1);
                };
            }
        }

        internal GameObject GetWeaponModel(int tableID, bool left)
        {
            if (!_weaponModels.ContainsKey(tableID))
                return null;

            var models = _weaponModels[tableID];
            var index = left ? 0 : 1;
            if (models.Length <= index)
                return null;

            return models[index];
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < _weaponLifeTimes.Length; i++)
            {
                var weaponLifeTime = _weaponLifeTimes[i];
                if (weaponLifeTime != 0)
                {
                    weaponLifeTime -= GameTimeManager.Instance.DeltaTime;
                    if (weaponLifeTime <= 0)
                    {
                        weaponLifeTime = 0f;
                        ReleaseWeapon(i);
                    }

                    _weaponLifeTimes[i] = weaponLifeTime;
                }
            }
        }
    }
}

