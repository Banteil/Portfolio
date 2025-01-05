namespace starinc.io.kingnslave
{
    public class UIScene : UIBase
    {
        protected override void InitializedProcess()
        {
            UIManager.Instance.SetCanvas(gameObject, true);
        }
    }
}
