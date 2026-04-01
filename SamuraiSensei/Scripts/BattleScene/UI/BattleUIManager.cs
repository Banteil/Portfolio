using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    private static BattleUIManager instance = null;
    public static BattleUIManager Instance
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

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        for (int i = 0; i < 50; i++)
        {
            GameObject obj = Instantiate(BattleResourceManager.Instance.DamageTextObject, objectCanvasTr, false);
            damageTexts.Enqueue(obj.GetComponent<DamageTextObject>());
        }
    }

    [SerializeField]
    RectTransform mainCanvas;
    public RectTransform MainCanvas { get { return mainCanvas; } }

    Queue<DamageTextObject> damageTexts = new Queue<DamageTextObject>();
    public Queue<DamageTextObject> DamageTexts { get { return damageTexts; } }

    [SerializeField]
    Image skillCostGauge;
    public Image SkillCostGauge { get { return skillCostGauge; } }

    [SerializeField]
    Image[] characterLogo;
    public Image[] CharacterLogo { get { return characterLogo; } }

    [SerializeField]
    CharacterHpUI[] characterHpUI;
    public CharacterHpUI[] CharacterHpUI { get { return characterHpUI; } }

    [SerializeField]
    Transform skillListTr;
    public Transform SkillListTr { get { return skillListTr; } }
    [SerializeField]
    SkillItem[] skillItems = new SkillItem[4];
    public SkillItem[] SkillItems { get { return skillItems; } }

    [SerializeField]
    Transform objectCanvasTr;
    public Transform ObjectCanvasTr { get { return objectCanvasTr; } }

    [SerializeField]
    ResultUI result;
    public ResultUI Result { get { return result; } }

    [SerializeField]
    PauseUI pause;
    public PauseUI Pause { get { return pause; } }

    public void PlayerHpInfoUpdate(Playerble playerble)
    {
        characterHpUI[playerble.CurrentLine].HPText.text = playerble.HP.ToString();
        characterHpUI[playerble.CurrentLine].HPGauge.fillAmount = playerble.HP / playerble.Info.info.maxHP;
    }

    public void SkillItemSort(int index) => StartCoroutine(SkillItemSortProcess(index));

    IEnumerator SkillItemSortProcess(int index)
    {
        for (int i = 0; i < skillItems.Length; i++)
        {
            skillItems[i].IconImage.raycastTarget = false;
        }

        //사용한 스킬 아이템 뒤쪽 아이템이 정렬되는 연출 추가
        for (int i = index; i < skillItems.Length; i++)
        {
            if (i >= BattleManager.Instance.SkillDeck.Count)
            {
                skillItems[i].SkillInfo = null;
                continue;
            }

            skillItems[i].SkillInfo = BattleManager.Instance.SkillDeck[i];
            skillItems[i].ResetPosition(i + 1);
            skillItems[i].SlotMove();
        }

        bool complete = false;
        while(!complete)
        {
            for (int i = 0; i < skillItems.Length; i++)
            {
                if (skillItems[i].isMove)
                {
                    complete = false;
                    break;
                }
                complete = true;
            }
            yield return null;
        }

        for (int i = 0; i < skillItems.Length; i++)
        {
            skillItems[i].IconImage.raycastTarget = true;
        }
    }
}
