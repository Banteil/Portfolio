using RayFire;
using UnityEngine;

namespace Zeus
{
    public class CrackMeUp : MonoBehaviour
    {
        [Tooltip("������ �����ư� ���� ���Ҷ� �����߰���.")]
        public float AddforceDirectionHeightOffset = 2f;

        void Start()
        {
            if (!TryGetComponent<Character>(out var myCharacter))
                return;

            //if (myCharacter.DeathByType.Equals(Character.DeathBy.Crack))
            //{
            //    var rfireRigids = GetComponentsInChildren<RayfireRigid>(true);
            //    foreach (var item in rfireRigids)
            //    {
            //        item.Initialize();
            //    }
            //}
        }

        /// <summary>
        /// ��Ų�� �޽ø� �޽÷� ����ŷ
        /// </summary>
        //void BakeMesh()
        //{
        //    SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        //    Mesh[] meshes = new Mesh[skinnedMeshRenderers.Length];

        //    for (int i = 0; i < meshes.Length; i++)
        //    {
        //        meshes[i] = new Mesh();
        //        skinnedMeshRenderers[i].BakeMesh(meshes[i]);
        //        if (skinnedMeshRenderers[i].gameObject.GetComponent<MeshFilter>() == null)
        //            skinnedMeshRenderers[i].gameObject.AddComponent(typeof(MeshFilter));
        //    }

        //    CreateRayfireComponents(meshes);
        //}

        /// <summary>
        /// RayFire ������Ʈ �߰� �� ���� �ʱ�ȭ
        /// </summary>
        /// <param name="meshes"></param>
        //void CreateRayfireComponents(Mesh[] meshes)
        //{
        //    MeshRenderer[] _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        //    SkinnedMeshRenderer[] _skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        //    foreach (MeshRenderer meshRenderer in _meshRenderers)
        //    {
        //        GameObject meshGO = meshRenderer.gameObject;
        //        RayfireRigid meshRigid = meshGO.ComponentAdd<RayfireRigid>();
        //        SettingRayfireRigid(ref meshRigid);
        //    }

        //    for (int i = 0; i < _skinnedMeshRenderers.Length; i++)
        //    {
        //        SkinnedMeshRenderer skinnedMeshRenderer = _skinnedMeshRenderers[i];

        //        GameObject skinnedMeshGO = skinnedMeshRenderer.gameObject;
        //        skinnedMeshGO.GetComponent<MeshFilter>().mesh = meshes[i];

        //        RayfireRigid skinnedMeshRigid = skinnedMeshGO.ComponentAdd<RayfireRigid>();
        //        SettingRayfireRigid(ref skinnedMeshRigid);
        //        skinnedMeshRigid.GetComponent<MeshRenderer>().enabled = false;
        //    }
        //}

        //void SettingRayfireRigid(ref RayfireRigid rayfireRigid)
        //{
        //    rayfireRigid.Initialize();
        //    rayfireRigid.demolitionType = DemolitionType.AwakePrefragment;
        //    rayfireRigid.physics.mt = MaterialType.DenseRock;
        //    rayfireRigid.physics.exclude = true;
        //    rayfireRigid.materials.innerMaterial = crackMaterial;
        //    rayfireRigid.materials.outerMaterial = crackMaterial;
        //    rayfireRigid.meshDemolition.amount = demolitionAmount;
        //    rayfireRigid.meshDemolition.properties.l = false;
        //    rayfireRigid.meshDemolition.properties.t = false;

        //    RFDemolitionMesh.Awake(rayfireRigid);
        //    //�ı� �̺�Ʈ �� fragment�� ������ �ٵ� �ǰ� ���⿡ �´� ���� �༭ ������ ���󰡰� ��
        //    //Event ������ for���� ������ ���´� ���� ����
        //    rayfireRigid.demolitionEvent.LocalEvent += (RayfireRigid rd) =>
        //    {
        //        vControlAI controlAI = gameObject.ComponentAdd<vControlAI>();
        //        Vector3 hitDir = (transform.position - controlAI.LastHitPosition).normalized;
        //        foreach (var fragment in rd.fragments)
        //        {
        //            fragment.physics.rigidBody.AddForce(hitDir * Random.Range(50f, 100f));
        //        }
        //    };

        //    Destroy(rayfireRigid.GetComponent<MeshCollider>());
        //    Destroy(rayfireRigid.GetComponent<Rigidbody>());
        //}

        public void Crack()
        {
            ControlAI controlAI = gameObject.ComponentAdd<ControlAI>();
            var position = transform.position;
            position.y += AddforceDirectionHeightOffset;
            Vector3 hitDir = (position - controlAI.LastHitPosition).normalized;

            var rfireRigids = GetComponentsInChildren<RayfireRigid>(true);
            foreach (var item in rfireRigids)
            {
                item.rootChild.SetPositionAndRotation(transform.position, item.rootChild.rotation);
                item.rootChild.gameObject.SetActive(true);

                foreach (var fragment in item.fragments)
                {
                    fragment.physics.rigidBody.AddForce(hitDir * Random.Range(50f, 100f));
                }
            }
        }
    }
}

