namespace starinc.io.gallaryx
{
    public class UIScene : UIBase
    {
        protected override void OnAwake()
        {
            UIManager.Instance.SetCanvas(gameObject, true);
        }
    }
}
