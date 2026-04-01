using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VillageArea : MonoBehaviour
{
    [SerializeField]
    List<Village> villageList = new List<Village>();
    public List<Village> VillageList { get { return villageList; } }

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Village village = transform.GetChild(i).GetComponent<Village>();
            village.VillageID = SceneManager.GetActiveScene().name + "Village" + i;
            villageList.Add(village);
        }
    }
 
}
