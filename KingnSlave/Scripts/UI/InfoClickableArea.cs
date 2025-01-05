using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.kingnslave
{
    public class InfoClickableArea : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private Define.UserDataType profileUIOwner;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (profileUIOwner == Define.UserDataType.You)
            {
                AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
                UIManager.Instance.ShowUserProfile(UserDataManager.Instance.MyData);
            }
            else
            {
                AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
                UIManager.Instance.ShowUserProfile(UserDataManager.Instance.OpponentDataList[0]);
            }
        }
    }
}