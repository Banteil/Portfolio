using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.gallaryx
{
    [CreateAssetMenu(fileName = "Exhibits Table", menuName = "Scriptable Object/Exhibits Table", order = int.MaxValue)]
    public class ExhibitsTable : ScriptableObject
    {
        public List<string> ExhibitsDataPathList = new List<string>();
    }
}