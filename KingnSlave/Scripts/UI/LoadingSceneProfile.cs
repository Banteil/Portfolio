using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class LoadingSceneProfile : MonoBehaviour
    {
        [SerializeField]
        private Define.UserDataType userDataType;
        private string playerSid;

        [SerializeField]
        private RawImage playerProfileImage;

        [SerializeField]
        private TMP_Text playerNickname;

        private const string RED_TEAM_GO_NAME = "RedTeam";
        private const string BLUE_TEAM_GO_NAME = "BlueTeam";


        private void Start()
        {
            if (GameManager.Instance.CurrentGameMode == Define.GamePlayMode.Practice)
            {
                gameObject.SetActive(false);
            }
            if (UserDataManager.Instance.OpponentDataList.Count <= 0)
            {
                Debug.Log("OpponentDataList: Count 0");
                return;
            }

            if (userDataType == Define.UserDataType.You)
            {
                playerSid = UserDataManager.Instance.MySid;
                playerNickname.text = UserDataManager.Instance.MyData.nickname;
                playerProfileImage.texture = UserDataManager.Instance.MyProfileImage;
                if (playerProfileImage == null)
                {
                    NetworkManager.Instance.GetTexture((texture) => 
                    { 
                        playerProfileImage.texture = texture;
                        UserDataManager.Instance.MyProfileImage = texture;
                    }, UserDataManager.Instance.MyData.profile_image);
                }
            }
            else
            {
                playerSid = UserDataManager.Instance.OpponentDataList[0].sid;
                playerNickname.text = UserDataManager.Instance.OpponentDataList[0].nickname;
                playerProfileImage.texture = UserDataManager.Instance.OpponentProfileImageList[0];
                if (playerProfileImage == null)
                {
                    NetworkManager.Instance.GetTexture((texture) =>
                    {
                        playerProfileImage.texture = texture;
                        UserDataManager.Instance.OpponentProfileImageList[0] = texture;
                    }, UserDataManager.Instance.OpponentDataList[0].profile_image);
                }
            }

            // 진영 선택
            if (GameManager.Instance.RedTeamPlayerSid == playerSid)
            {
                // Red Team(Top UI)
                transform.SetParent(GameObject.Find(RED_TEAM_GO_NAME)?.transform, false);
            }
            else
            {
                // Blue Team(Bottom UI)
                transform.SetParent(GameObject.Find(BLUE_TEAM_GO_NAME)?.transform, false);
            }

        }
    }
}