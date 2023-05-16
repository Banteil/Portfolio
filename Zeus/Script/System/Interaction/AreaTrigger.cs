using UnityEngine;

namespace Zeus
{
    public class AreaTrigger : InteractionTrigger
    {
        [SerializeField] private int _areaID;

        public override void OnEnter(InteractionActor actor)
        {
            var str = TableManager.GetString(_areaID);
            if (str != null)
                PlayerUIManager.Get().GetUI<PlayerTextTypeUI>(TypePlayerUI.AREA).SetVisible(str);
        }
    } 
}
