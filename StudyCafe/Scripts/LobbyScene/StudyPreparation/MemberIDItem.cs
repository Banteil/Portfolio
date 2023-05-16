using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MemberIDItem : MonoBehaviour
{    
    public Text mailText;
    [HideInInspector]
    public InviteMember invite;
    [HideInInspector]
    public int index;

    public void DeleteMailButton() => invite.DeleteMail(index);

}
