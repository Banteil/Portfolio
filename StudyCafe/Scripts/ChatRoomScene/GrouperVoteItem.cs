using UnityEngine;
using UnityEngine.UI;

public class GrouperVoteItem : MonoBehaviour
{
    public Image[] avatarParts;
    public Text nameText;
    public Button voteButton;
    public int voteCount = 0;
    public int index;
    [HideInInspector]
    public MVPVote voteScript;

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
                    avatarParts[3].sprite = sprites[0];
                    break;
                case "e":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Eye/eye_" + num);
                    avatarParts[1].sprite = sprites[0];
                    break;
                case "t":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Top/top_" + num);
                    avatarParts[2].sprite = sprites[0];
                    break;
                case "sk":
                    sprites = Resources.LoadAll<Sprite>("AvatarSprite/Body/body_" + num);
                    avatarParts[0].sprite = sprites[0];
                    break;
                default:
                    continue;
            }
        }
    }

    public void AddVoteCountButton()
    {
        voteScript.EndVote(index);
    }
}
