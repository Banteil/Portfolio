using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public enum TypeBuff
    {
        None = 0,
        BLESS = 1, //ATTACK POWER UP
        SPEEDUP = BLESS << 1,
    }

    public enum TypeDeBuff
    {
        None = 0,
        STOP = 1,
        SLOW = STOP << 1,
    }

    public class BuffManager : BaseObject<BuffManager>
    {
        private Dictionary<int, List<BuffController>> _buffControllers;
        private List<int> removeOwnerList = new List<int>();

        //같은 버프는 시간이 초기화되면서 들어간다.
        internal void AddBuff(int ownerID, int tableID)
        {
            _buffControllers ??= new();

            var tableData = TableManager.GetBuffTableData(tableID);
            if (tableData == null)
            {
                Debug.LogError($"Not Found Buff Table Data ID : {tableData}");
                return;
            }

            var data = new BuffController
            {
                TableData = tableData,
                Duration = tableData.Duration,
                OwnerID = ownerID,
                Infinity = tableData.Duration == 0,
            };

            if (_buffControllers.ContainsKey(ownerID))
            {
                ReleaseBuff(ownerID, tableID);
                var list = _buffControllers[ownerID];
                list.Add(data);
            }
            else
            {
                var list = new List<BuffController>();
                list.Add(data);
                _buffControllers.Add(ownerID, list);
            }
        }

        internal void ReleaseBuff(int ownerID, int tableID)
        {
            if (_buffControllers == null)
                return;

            if (_buffControllers.ContainsKey(ownerID))
            {
                var list = _buffControllers[ownerID];
                var index = list.FindIndex(_ => _.TableData.ID == tableID);
                if (index != -1)
                {
                    list.RemoveAt(index);
                }
            }
        }

        internal List<BuffController> GetBuffTableData(int owerID)
        {
            if (_buffControllers.ContainsKey(owerID))
            {
                return _buffControllers[owerID];
            }

            return null;
        }

        private void Update()
        {
            if (_buffControllers == null || _buffControllers.Count <= 0)
                return;

            foreach (var item in _buffControllers)
            {
                for (int i = item.Value.Count - 1; i >= 0; --i)
                {
                    //data is buff list.
                    var data = item.Value[i];
                    if (data != null && data.Duration > 0)
                    {
                        data.CustomUpdate();
                    }

                    if (data == null || !data.Infinity && data.Duration <= 0)
                    {
                        item.Value.RemoveAt(i);
                    }
                }

                if (item.Value.Count == 0)
                {
                    removeOwnerList.Add(item.Key);
                }
            }

            foreach (var item in removeOwnerList)
            {
                _buffControllers.Remove(item);
            }
            removeOwnerList.Clear();
        }
    }

    public class BuffController
    {
        internal bool Infinity { get; set; }
        internal BuffTableData TableData { get; set; }
        internal int OwnerID { get; set; }

        internal float Duration
        {
            get { return duration; }
            set
            {
                elapsedTime = 0f;
                duration = value;
            }
        }
        internal float RemainTime { get { return duration - elapsedTime; } }

        private float duration;
        private float elapsedTime;

        internal void CustomUpdate()
        {
            if (Infinity)
                return;

            elapsedTime += Time.deltaTime;

            if (elapsedTime >= duration)
            {
                elapsedTime = duration;
            }
        }
    }
}