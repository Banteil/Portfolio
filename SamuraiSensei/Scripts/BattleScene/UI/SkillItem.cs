using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SkillItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    const float itemLocalXPos = 135f;
    const float itemLocalYPos = -135f;
    const float itemDis = 270f;
    const float activeYPos = 80f;

    [SerializeField]
    int index;

    Image iconImage;
    public Image IconImage { get { return iconImage; } }
    [SerializeField]
    Image decorationFrameImage;
    public Image DecorationFrameImage { get { return decorationFrameImage; } }
    [SerializeField]
    TextMeshProUGUI skillNameText;
    public TextMeshProUGUI SkillNameText { get { return skillNameText; } }

    Skill skillInfo;
    public Skill SkillInfo
    {
        get { return skillInfo; }
        set
        {
            skillInfo = value;
            if (skillInfo == null)
            {
                gameObject.SetActive(false);
                return;
            }
            IconImage.sprite = ResourceManager.Instance.SkillIconList[skillInfo.id];
            decorationFrameImage.enabled = false;
            if (ResourceManager.Instance.SkillFrameList[(int)skillInfo.propertie].Length > 0)
            {
                decorationFrameImage.sprite = ResourceManager.Instance.SkillFrameList[(int)skillInfo.propertie][0];
                decorationFrameImage.enabled = true;
            }
            else
                decorationFrameImage.enabled = false;
            skillNameText.text = skillInfo.skillName;
        }
    }

    RectTransform rT;
    public RectTransform RT { get { return rT; } }

    SkillRange skillRange;
    bool activePossible = false;
    public bool isMove;

    private void Awake()
    {
        iconImage = GetComponent<Image>();
        rT = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        skillNameText.enabled = false;
        GameObject rangeObj = Instantiate(BattleResourceManager.Instance.SkillRangeList[skillInfo.id], BattleManager.Instance.ObjectTr, false);
        skillRange = rangeObj.GetComponent<SkillRange>();
        skillRange.RangeImageActive(false);
        transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float yDis = (activeYPos - rT.anchoredPosition.y) / (activeYPos - itemLocalYPos);
        if (yDis < 0) yDis = 0;
        else if (yDis > 1) yDis = 1;
        Vector2 pos = new Vector2(yDis, yDis);
        transform.localScale = pos;

        bool isActive = rT.anchoredPosition.y >= activeYPos;
        skillRange.RangeImageActive(isActive);
        activePossible = isActive;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //스킬명 다시 활성화
        skillNameText.enabled = true;
        //스컬 범위 객체 제거
        if (skillRange != null)
        {
            Destroy(skillRange.gameObject);
            skillRange = null;
        }

        //발동 시킬 수 없었다면 포지션만 리셋
        if (!activePossible)
        {
            BattleUIManager.Instance.SkillItems[index].ResetPosition(index);
        }
        else
        {
            //스킬 사용 시 발생하는 코스트 계산
            float cost = BattleManager.Instance.GetSkillValue(skillInfo.cost, skillInfo);
            //현재 소유한 코스트 수치로 스킬을 사용할 수 있는지 체크
            if (BattleManager.Instance.Cost < cost)
            {
                //사용 불가 시 상태 원래대로 되돌림
                BattleUIManager.Instance.SkillItems[index].ResetPosition(index);
            }
            else
            {
                //코스트 소모
                BattleManager.Instance.Cost -= cost;
                //스킬 이펙트 생성
                GameObject obj = Instantiate(BattleResourceManager.Instance.SkillEffectList[skillInfo.id], BattleManager.Instance.ObjectTr, false);
                obj.GetComponent<SkillEffect>().SkillInfo = skillInfo;
                switch (skillInfo.control)
                {
                    case ControlType.FLEXIBLE:
                        //마우스 위치에 맞춤
                        obj.transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        break;
                    case ControlType.LINE:
                        //캐릭터 중 스킬을 소유한 캐릭터의 라인 위치에 맞춤(X축)
                        for (int i = 0; i < BattleManager.Instance.Playerbles.Length; i++)
                        {
                            if (BattleManager.Instance.Playerbles[i].Info.info.id.Equals(skillInfo.characterID))
                            {
                                Vector3 charPos = BattleManager.Instance.Playerbles[i].transform.position;
                                obj.transform.position = new Vector3(charPos.x, obj.transform.position.y);
                                break;
                            }
                        }
                        break;
                }

                //사용한 스킬 리스트 정리
                BattleManager.Instance.UsedSkills.Add(skillInfo);
                BattleManager.Instance.SkillDeck.RemoveAt(index);
                //현재 인덱스를 기준으로 아이템 재정렬
                BattleUIManager.Instance.SkillItemSort(index);                 
            }
        }
        //스케일 원래대로 돌림
        transform.localScale = Vector3.one;
    }

    public void ResetPosition(int index)
    {
        rT.anchoredPosition = new Vector2(itemLocalXPos + (itemDis * index), itemLocalYPos);
    }

    public void SlotMove() => StartCoroutine(SlotMoveProcess());

    public IEnumerator SlotMoveProcess()
    {
        Vector2 target = transform.localPosition;
        target.x -= 270f;
        isMove = true;
        while (true)
        {
            transform.localPosition = Vector2.Lerp(transform.localPosition, target, 0.5f);
            float dis = Vector2.Distance(target, transform.localPosition);
            if (dis <= 0.1f)
            {
                isMove = false;
                break;
            }

            yield return null;
        }
    }
}
