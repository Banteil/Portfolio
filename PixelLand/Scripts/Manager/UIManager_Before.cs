using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Before : MonoBehaviour
{
    private static UIManager_Before instance = null;

    private Transform _transform;

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        _transform = transform;
    }

    private void Start()
    {
        var windows = GetComponentsInChildren<UIWindow>(true);
        foreach (var window in windows)
            AddWindow(window.WindowName, window);
    }

    public static UIManager_Before Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    [SerializeField]
    GameObject bottomUI;
    public GameObject BottomUI { get { return bottomUI; } }
    [SerializeField]
    GameObject middleUI;
    public GameObject MiddleUI { get { return middleUI; } }
    [SerializeField]
    GameObject topUI;
    public GameObject TopUI { get { return topUI; } }

    [SerializeField]
    Inventory inventoryUI;
    public Inventory InventoryUI { get { return inventoryUI; } }

    [SerializeField]
    QuestUI questUI;
    public QuestUI QuestUI { get { return questUI; } }

    [SerializeField]
    Minimap minimapUI;
    public Minimap MinimapUI { get { return minimapUI; } }

    [SerializeField]
    WeaponInfoUI _weaponInfoUI;
    public WeaponInfoUI WeaponInfo { get { return _weaponInfoUI; } }

    [SerializeField]
    MapInfoUI _mapInfoUI;
    public MapInfoUI MapInfo { get { return _mapInfoUI; } }

    [SerializeField]
    TextBox textBox;
    public TextBox TextBox { get { return textBox; } }
    [SerializeField]
    Image playerHpGauge;
    [SerializeField]
    Text playerHpText;
    [SerializeField]
    Image playerStaminaGauge;
    [SerializeField]
    ResidentInformation residentInfo;
    public ResidentInformation ResidentInfo { get { return residentInfo; } }
    [SerializeField]
    Text coinNumberText;
    public Text CoinNumberText { get { return coinNumberText; } }

    [SerializeField]
    ItemInfomation itemInfoUI;
    public ItemInfomation ItemInfoUI { get { return itemInfoUI; } }
    [SerializeField]
    LogInfo logInfoUI;
    public LogInfo LogInfoUI { get { return logInfoUI; } }

    [SerializeField]
    Animation fadeAnim;

    ItemObject dragObject;
    public ItemObject DragObject { get { return dragObject; } set { dragObject = value; } }

    public void PlayerHpFillAmount()
    {
        playerHpGauge.fillAmount = GameManager.Instance.Player.HP / GameManager.Instance.Player.Info.MaxHP;
        playerHpText.text = GameManager.Instance.Player.HP.ToString("000");
    }

    public void PlayerStaminaFillAmount()
    {
        playerStaminaGauge.fillAmount = GameManager.Instance.Player.Stamina / GameManager.Instance.Player.Info.MaxStamina;
    }

    public void DisplayDamageValue(Vector3 pos, float value)
    {
        GameObject obj = Instantiate(ResourceManager.Instance.DamageText);
        obj.transform.position = pos;
        obj.GetComponent<DamageText>().SettingText(value);
    }

    public void InventoryActive()
    {
        if (!InventoryUI.gameObject.activeSelf)
            InventoryUI.gameObject.SetActive(true);
        else
            InventoryUI.gameObject.SetActive(false);
    }

    public void QuestUIActive()
    {
        if (!questUI.gameObject.activeSelf)
            questUI.gameObject.SetActive(true);
        else
            questUI.gameObject.SetActive(false);
    }

    public void MinimapUIActive()
    {
        if (!minimapUI.gameObject.activeSelf)
            minimapUI.gameObject.SetActive(true);
        else
            minimapUI.gameObject.SetActive(false);
    }

    public IEnumerator FadeOut()
    {
        fadeAnim.Play("FadeOut");
        while (fadeAnim.isPlaying) { yield return null; }
    }

    public IEnumerator FadeIn()
    {
        fadeAnim.Play("FadeIn");
        while (fadeAnim.isPlaying) { yield return null; }
    }

    private Dictionary<string, UIWindow> _windowDictionary = new Dictionary<string, UIWindow>();
    public Dictionary<string, UIWindow> WindowDictionary => _windowDictionary;

    public void AddWindow(string windowName, UIWindow window)
    {
        if (!_windowDictionary.ContainsKey(windowName))
            _windowDictionary.Add(windowName, window);
    }

    public void RemoveWindow(string windowName)
    {
        if (_windowDictionary.ContainsKey(windowName))
            _windowDictionary.Remove(windowName);
    }

    public UIWindow GetWindow(string windowName)
    {
        UIWindow window = null;
        _windowDictionary.TryGetValue(windowName, out window);
        return window;
    }

    public UIWindow OpenWindow(string windowName)
    {
        var window = GetWindow(windowName);
        window?.Open();
        return window;

        //var windows = _transform.GetComponentsInChildren<UIWindow>();
        //var target = windows.FirstOrDefault(x => x.WindowName.Equals(windowName));
        //return target;
    }

    public void CloseWindow(string windowName)
    {
        var window = GetWindow(windowName);
        window?.Close();
        //
    }
}
