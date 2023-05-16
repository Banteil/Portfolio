using UnityEngine;

namespace Zeus
{
    public class LootableTarget : MonoBehaviour
    {
        [SerializeField] private bool _destroyOnLoot;
        [SerializeField] private int _lootTableID;
        [SerializeField] private string _lootItemPath;
        [SerializeField, Range(0, 100)] private float _lootChance = 30f;

        private bool _isLoot = false;
        //private void OnDestroy()
        //{
        //    if (!Application.isPlaying) return;
        //    if (_destroyOnLoot) Loot();
        //}
        public void Loot()
        {
            if (_isLoot) return;

            _isLoot = true;

            var per = Random.Range(0, 100);
            if (_lootChance - per < 0) return;

            var tableData = TableManager.GetLootTableData(_lootTableID);
            if (tableData == null) return;

            // ������ ����
            for (int i = 0; i < tableData.Items.Length; i++)
            {
                var lootItemData = tableData.Items[i];
                if (lootItemData == null) continue;

                var lootObject = TableManager.GetGameObject(_lootItemPath);
                if (lootObject == null) continue;

                // LootItem ��ġ �ʱ�ȭ
                var lootPosition = transform.position;
                lootPosition.y += 0.2f;
                lootObject.transform.SetPositionAndRotation(lootPosition, Quaternion.identity);

                // LootItem ��ũ��Ʈ�� ���ٸ� �׳� �ν�����
                var lootItem = lootObject.GetComponent<LootItem>();
                if (lootItem == null)
                {
                    Destroy(lootObject);
                    continue;
                }

                // LootItem �ʱ�ȭ
                lootItem.Initialize(lootItemData);
            }
        }
    } 
}
