using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Village : MonoBehaviour
{
    [SerializeField]
    string villageID;
    public string VillageID { get { return villageID; } set { villageID = value; } }
    [SerializeField]
    List<PersonalSpace> villageLandList = new List<PersonalSpace>();
    public List<PersonalSpace> VillageLandList { get { return villageLandList; } }

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            PersonalSpace pS = transform.GetChild(i).GetComponent<PersonalSpace>();
            pS.ObjectID = villageID + "PersonalSpace" + i;
            villageLandList.Add(pS);
        }
    }

    public int GetVillageNumber()
    {
        int num = 0;
        for (int i = 0; i < villageLandList.Count; i++)
        {
            if (!villageLandList[i].OwnerID.Equals("null"))
                num++;
        }
        return num;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Character"))
        {
            CharacterHitBox hit = collision.GetComponent<CharacterHitBox>();
            if (hit.CB.CompareTag("Player"))
            {                
                UIManager.Instance.GetUI("ResidentInformation").GetComponent<ResidentInformation>().SetResidentNumber(GetVillageNumber());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Character"))
        {
            CharacterHitBox hit = collision.GetComponent<CharacterHitBox>();
            if (hit.CB.CompareTag("Player"))
            {
                UIManager.Instance.GetUI("ResidentInformation").GetComponent<ResidentInformation>().SetResidentNumber(0);
            }
        }
    }
}
