namespace Zeus
{
    public class BossMapManager : UnitySingleton<BossMapManager>
    {
        public void LoadSecondMap()
        {
            SceneLoadManager.Instance.LoadScene("2ndMap");
        }
    }
}
