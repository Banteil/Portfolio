using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonalSpace : MonoBehaviour
{
    [SerializeField]
    protected string objectID;
    public string ObjectID 
    { 
        get { return objectID; } 
        set 
        { 
            objectID = value;
            data.objectID = objectID;
        } 
    }

    [SerializeField]
    string ownerID = "null";
    public string OwnerID 
    { 
        get { return ownerID; } 
        set 
        { 
            ownerID = value;
            data.ownerID = ownerID;
        } 
    }

    Village myVillage;
    public Village MyVillage { get { return myVillage; } }
    House house;
    LandSaleSignPost signPost;

    PersonalSpaceData data = new PersonalSpaceData();
    public PersonalSpaceData Data
    {
        get { return data; }
        set
        {
            data = value;
            if (!data.ownerID.Equals("null"))
            {
                signPost.gameObject.SetActive(false);
                ownerID = data.ownerID;
                house.SetSaveData(value);
            }
        }
    }

    private void Start()
    {
        myVillage = transform.parent.GetComponent<Village>();
        house = transform.GetChild(1).GetComponent<House>();
        signPost = transform.GetChild(2).GetComponent<LandSaleSignPost>();
        data.position = transform.position;
    }

    public string GetSpaceName()
    {
        string name = "";
        if (ownerID.Equals("null"))
        {
            for (int i = 0; i < myVillage.VillageLandList.Count; i++)
            {
                if(myVillage.VillageLandList[i].Equals(this))
                {
                    name = (i + 1) + "번 부지";
                    return name;
                }
            }
        }

        for (int i = 0; i < CharacterManager.Instance.PlayerbleList.Count; i++)
        {
            if (CharacterManager.Instance.PlayerbleList[i].ObjectID.Equals(ownerID))
            {
                name = CharacterManager.Instance.PlayerbleList[i].name + "의 개인 공간";
                return name;
            }
        }

        return name;
    }
}
