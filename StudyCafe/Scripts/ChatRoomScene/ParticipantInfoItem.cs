using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ParticipantInfoItem : MonoBehaviour
{
    const int body = 0;
    const int eyes = 1;
    const int top = 2;
    const int hair = 3;

    public Text indexText;
    public Text typeText;
    public Text nickNameText;
    public Image[] avatarParts;
    public GameObject banButton;
    int index;
    public int Index
    {
        get { return index; }
        set
        {
            index = value;
            indexText.text = index.ToString();
        }
    }
    [HideInInspector]
    public string emailID;

    public void SetAvatarImage(string avatarInfo)
    {
        string[] partsInfo = avatarInfo.Split(',');

        for (int i = 0; i < partsInfo.Length; i++)
        {
            Sprite[] sprites;
            string[] info = partsInfo[i].Split('_');
            string parts = info[0];
            int num = int.Parse(info[1]);

            switch (parts)
            {
                case "h":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Hair/hair_" + num);
                    if (!sprites.Length.Equals(0))
                        avatarParts[hair].sprite = sprites[0];
                    break;
                case "e":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Eye/eye_" + num);
                    if (!sprites.Length.Equals(0))
                        avatarParts[eyes].sprite = sprites[0];
                    break;
                case "t":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Top/top_" + num);
                    if(!sprites.Length.Equals(0))
                        avatarParts[top].sprite = sprites[0];
                    break;
                case "sk":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Body/body_" + num);
                    if (!sprites.Length.Equals(0))
                        avatarParts[body].sprite = sprites[0];
                    break;
                default:
                    continue;
            }
        }
    }

    public void BanPlayerButton() => RoomManager.Instance.ForcedExitPlayer(index);
}
