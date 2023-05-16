using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public enum TypeEffects
    {
        NONE,
        EVASION_TRANSFORM = 2001,
        LOOTING_COIN = 5001,
        LOOTING_WEAPON,
        LOOTING_CONSUME,
    }

    public class EffectsManager : BaseObject<EffectsManager>
    {
        public GameObject[] DecalPool;
        [SerializeField]
        private LayerMask _groundLayer = 1 << 0;
        [SerializeField]
        private LayerMask _layerOfObstacles = 1 << 0;
        [SerializeField]
        private List<Material> BloodMaterials; //�Ƕ��� ���� ���׸���.

        private Dictionary<Material, IEnumerator> _materialCutouts = new Dictionary<Material, IEnumerator>();
        private Dictionary<int, EffectController> _effects = new Dictionary<int, EffectController>();
        private List<ParticleSystem> _particles = new List<ParticleSystem>();
        private Dictionary<ParticleSystem, float> _oldParticlesSpeed;
        private Dictionary<int, Material[]> _originalMaterials = new Dictionary<int, Material[]>();

        internal void SetBloodFx(Vector3 hitPosition, Vector3 direction)
        {
            var randomIndex = UnityEngine.Random.Range(0, DecalPool.Length);
            var rotation = Quaternion.LookRotation(direction);
            var bloodOb = Instantiate(DecalPool[randomIndex], hitPosition, Quaternion.Euler(0, rotation.eulerAngles.y + 90f, 0));
            //�Ѹ� ���� üũ.
            var comp = bloodOb.ComponentAdd<BFX_BloodSettings>();
            comp.GroundHeight = -10f; //�⺻����.
            bloodOb.SetActive(true);
            var hits = Physics.RaycastAll(hitPosition, Vector3.down, 5f, _groundLayer);
            foreach (var item in hits)
            {
                comp.GroundHeight = hitPosition.y - item.distance;
                break;
            }
            bloodOb.DestroyTimer(6f);

            var layer = 0;
            var check = false;
            var targets = Physics.SphereCastAll(hitPosition, 1, direction, 0.01f);
            #region attatch boolMaterial
            foreach (var item in targets)
            {
                layer = 1 << item.transform.gameObject.layer;
                check = Util.CheckFlag(_layerOfObstacles.value, layer);
                if (!check)
                    continue;

                var renderers = item.transform.GetComponentsInChildren<Renderer>();
                if (renderers.Length != 0)
                {
                    //������ ���̸� �Ǹ� �ȹ�����.
                    foreach (var renderer in renderers)
                    {
                        foreach (var data in renderer.materials)
                        {
                            if (data.HasProperty("_AdvancedDissolveCutoutStandardClip"))
                            {
                                var amount = data.GetInt("_AdvancedDissolveCutoutStandardClip");
                                if (amount != 0)
                                    return;
                            }
                        }
                    }

                    foreach (var re in renderers)
                    {
                        layer = 1 << re.gameObject.layer;
                        check = Util.CheckFlag(_layerOfObstacles.value, layer);
                        if (!check)
                            continue;

                        if (!_originalMaterials.ContainsKey(re.GetInstanceID()))
                        {
                            _originalMaterials.Add(re.GetInstanceID(), re.materials);
                        }

                        var matIndex = UnityEngine.Random.Range(0, BloodMaterials.Count);
                        var mat = BloodMaterials[matIndex];
                        //���׸��� ���������� �������� �̹� ���õȰ� ������ ã�Ƽ� �ƾƿ��� �����ش�.
                        var find = false;
                        var findIndex = 0;
                        for (int i = 0; i < re.materials.Length; i++)
                        {
                            if (!find)
                            {
                                find = re.materials[i].name.Contains(mat.name);
                            }

                            if (find)
                            {
                                findIndex = i;
                                break;
                            }
                        }

                        if (find)
                        {
                            //�̵̹��ư��� �ڷ�ƾ���ִٸ� ���߰� �ٽ� ������.
                            if (_materialCutouts.ContainsKey(re.materials[findIndex]))
                            {
                                StopCoroutine(_materialCutouts[re.materials[findIndex]]);
                                _materialCutouts[re.materials[findIndex]] = BloodEffectRemove(re, re.materials[findIndex], 3f, 1f);
                                StartCoroutine(_materialCutouts[re.materials[findIndex]]);
                            }
                            else
                            {
                                var ie = BloodEffectRemove(re, re.materials[findIndex], 3f, 1f);
                                _materialCutouts.Add(re.materials[findIndex], ie);
                                StartCoroutine(ie);
                            }
                        }
                        else
                        {
                            var matArray = new Material[re.materials.Length + 1];
                            for (int i = 0; i < re.materials.Length; i++)
                            {
                                matArray[i] = re.materials[i];
                            }

                            matArray[re.materials.Length] = mat;
                            re.materials = matArray;
                            var mat2 = re.materials[^1];
                            var ie = BloodEffectRemove(re, mat2, 3, 1f);
                            _materialCutouts.Add(mat2, ie);
                            StartCoroutine(ie);
                        }
                    }
                }
            }
            #endregion
        }

        private IEnumerator BloodEffectRemove(Renderer re, Material mat, float lifeTime, float removeDuration)
        {
            var elapsedTime = 0f;
            var cutOut = 0f;
            if (mat.HasProperty("_Cutout"))
                mat.SetFloat("_Cutout", cutOut);
            else
            {
                Debug.LogError("Not Found _Cutout Property");
                yield break;
            }

            yield return new WaitForSeconds(lifeTime);

            if (re == null)
            {
                _materialCutouts.Remove(mat);
                yield break;
            }

            while (cutOut < 1)
            {
                yield return new WaitForEndOfFrame();

                if (mat == null)
                {
                    yield break;
                }

                elapsedTime += GameTimeManager.Instance.DeltaTime;
                cutOut = Mathf.Clamp(elapsedTime / removeDuration, 0f, 1f);
                mat.SetFloat("_Cutout", cutOut);
            }

            if (re == null)
            {
                _materialCutouts.Remove(mat);
                yield break;
            }

            if (_originalMaterials.ContainsKey(re.GetInstanceID()))
            {
                re.materials = _originalMaterials[re.GetInstanceID()];
            }
            _originalMaterials.Remove(re.GetInstanceID());
            _materialCutouts.Remove(mat);
        }

        internal int SetEffect(int tableID, Vector3 position, Vector3 lookDirection, Transform parent = null, float duration = 0, float scale = 1)
        {
            var createController = new EffectController();
            var createID = createController.StartEffect(tableID, position, lookDirection, parent, duration, scale);
            _effects.Add(createID, createController);
            return createID;
        }

        internal int SetEffect(GameObject ob, Vector3 _position, Transform parent = null, float duration = 0, float scale = 1)
        {
            var createController = new EffectController();
            var createID = createController.StartEffect(ob, _position, parent, duration, scale);
            _effects.Add(createID, createController);
            return createID;
        }

        internal bool SetDuration(int objectID, float duration)
        {
            if (_effects.ContainsKey(objectID))
            {
                var controller = _effects[objectID];
                return controller.UpdateRemainTime(duration);
            }
            return false;
        }

        internal void SetWeaponEffect(GameObject weaponModel, bool rightHand)
        {
            var component = weaponModel.GetComponentInChildren<PSMeshRendererUpdater>();
            if (component != null)
                return;

            var tableID = TableManager.CurrentPlayerData.GetWeaponID();
            var tableData = TableManager.GetWeaponTableData(tableID);
            var path = rightHand ? tableData.RightHandMeshEffectPath : tableData.LeftHandMeshEffectPath;
            if (tableData == null || string.IsNullOrEmpty(path))
                return;

            TableManager.Instance.GetGameObjectAsync(path, ob =>
            {
                if (ob == null)
                    return;

                ob.transform.SetParent(weaponModel.transform, false);
                component = ob.GetComponent<PSMeshRendererUpdater>();
                if (component == null)
                {
                    Debug.LogError("Not Found PSMeshRendererUpdater Component");
                    return;
                }

                component.MeshObject = weaponModel;
                component.UpdateMeshEffect();
            });
        }

        internal void ReleaseWeaponEffect(GameObject weaponModel)
        {
            var renderers = weaponModel.GetComponentsInChildren<Renderer>();
            var notFound = false;
            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.materials)
                {
                    if (_materialCutouts.ContainsKey(mat))
                    {
                        StopCoroutine(_materialCutouts[mat]);
                        _materialCutouts.Remove(mat);
                        mat.SetFloat("_Cutout", 1f);
                    }
                    else
                    {
                        notFound = true;
                        break;
                    }
                }

                if (notFound)
                    break;

                if (_originalMaterials.ContainsKey(renderer.GetInstanceID()))
                {
                    renderer.materials = _originalMaterials[renderer.GetInstanceID()];
                    _originalMaterials.Remove(renderer.GetInstanceID());
                }
            }

            if (notFound)
            {
                var newMaterials = new List<Material>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    newMaterials.Clear();
                    var rebderer = renderers[i];
                    for (int j = 0; j < rebderer.materials.Length; j++)
                    {
                        var mat = rebderer.materials[j];
                        var index = BloodMaterials.FindIndex(_ => mat.name.Contains(_.name));
                        if (index != -1) //find
                            continue;

                        newMaterials.Add(mat);
                    }

                    rebderer.materials = newMaterials.ToArray();
                }
            }

            //�Ǹ��׸��� ����ɷ� �ѹ� ���� ó�ְ� ������ ���׸����� ���� �����ȴ�.
            var component = weaponModel.GetComponentInChildren<PSMeshRendererUpdater>();
            if (component != null)
            {
                component.UpdateMeshEffect();
                Destroy(component.gameObject);
            }
        }

        internal void ReleaseEffect(int objectID)
        {
            if (!_effects.ContainsKey(objectID))
                return;

            var controller = _effects[objectID];
            controller.Destroy();
            _effects.Remove(objectID);
        }

        readonly List<int> removeList = new();
        private void Update()
        {
            foreach (var item in _effects)
            {
                if (item.Value == null)
                {
                    removeList.Add(item.Key);
                    continue;
                }

                //��ü ��ƼŬ ���ο찡 �ɷ������� �ð� ����� ���� �ʴ´�.
                var timeCalculate = _oldParticlesSpeed == null || _oldParticlesSpeed.Count == 0;
                item.Value.Update(timeCalculate);

                if (!item.Value.Inifinity && item.Value.RemainTime <= 0)
                {
                    removeList.Add(item.Key);
                }
            }

            foreach (var item in removeList)
            {
                _effects.Remove(item);
            }

            if (removeList.Count > 0)
                removeList.Clear();
        }

        internal void AddParticle(GameObject ob)
        {
            var paticles = ob.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var item in paticles)
            {
                _particles.Add(item);
                Debug.Log($"AddParticle : {item.gameObject.name}");
            }

            if (_setSpeed != -1f)
                SetParticlesSpeed(_setSpeed);
        }
        private float _setSpeed = -1f;
        internal void SetParticlesSpeed(float speed)
        {
            _setSpeed = speed;
            _oldParticlesSpeed ??= new();
            for (int i = _particles.Count - 1; i >= 0; --i)
            {
                var item = _particles[i];

                if (item == null)
                {
                    _particles.RemoveAt(i);
                    continue;
                }

                if (_oldParticlesSpeed.ContainsKey(item))
                    continue;

                _oldParticlesSpeed.Add(item, item.main.simulationSpeed);

                var main = item.main;
                main.simulationSpeed = main.simulationSpeed * speed;
            }
        }

        internal void RevertParticleSpeed()
        {
            if (_oldParticlesSpeed == null)
                return;

            for (int i = _particles.Count - 1; i >= 0; --i)
            {
                var item = _particles[i];

                if (item == null)
                {
                    _particles.RemoveAt(i);
                    continue;
                }

                if (_oldParticlesSpeed.ContainsKey(item))
                {
                    var main = item.main;
                    main.simulationSpeed = _oldParticlesSpeed[item];
                }
            }

            _oldParticlesSpeed.Clear();
            _setSpeed = -1f;
        }
    }
}

