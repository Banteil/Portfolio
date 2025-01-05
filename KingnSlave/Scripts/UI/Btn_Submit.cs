using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.kingnslave
{
    public class Btn_Submit : MonoBehaviour, IPointerClickHandler
	{
        public void OnPointerClick(PointerEventData eventData)
        {
            if (InGameManager.Instance.You.Phase != Define.InGamePhase.MainPhase) { return; }
            //AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.SubmitCard));
            InGameManager.Instance.YourSubmitStart?.Invoke();
        }
	}
}