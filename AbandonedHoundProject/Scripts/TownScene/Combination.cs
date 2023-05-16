using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.Linq;

public class Combination : Singleton<Combination>
{
    public Transform combinationCanvas;
    public Transform leftGemContent;
    public Transform leftGemOptionContent;
    public Image leftGemImage;
    public Text leftGemType;
    public Text leftGemValue;

    public Transform rightGemContent;
    public Transform rightGemOptionContent;
    public Image rightGemImage;
    public Text rightGemType;
    public Text rightGemValue;

    public GameObject combinationPanel;
    public RawImage combinationFade;
    public Text errorText;

    public GameObject combinationGemInfoPanel;
    public Text combiGemType;
    public Text combiGemValue;
    public Transform combiGemOptionContent;
    public Image combiGemIcon;
    public Text reslutText;

    public Gem leftSelectedGem = new Gem();
    public Gem rightSelectedGem = new Gem();

    [HideInInspector]
    public Image leftSelectedImage, rightSelectedImage;
    [HideInInspector]
    public bool diceEnd;
    Sprite basicGem;


    void Start()
    {
        Sprite[] icon = Resources.LoadAll<Sprite>("Icons/IconSet");
        basicGem = icon[68];

        for (int i = 0; i < PlayerState.Instance.getGemList.Count; i++)
        {
            Gem gem = new Gem();
            gem = PlayerState.Instance.getGemList[i];

            GameObject combiGemSlot = Instantiate(Resources.Load("GemSlot") as GameObject);
            combiGemSlot.AddComponent<CombinationSelectGem>();
            combiGemSlot.GetComponent<CombinationSelectGem>().gem = gem;
            combiGemSlot.GetComponent<CombinationSelectGem>().leftGem = true;
            combiGemSlot.transform.SetParent(leftGemContent);
            combiGemSlot.transform.localScale = Vector3.one;
        }        
    }

    public void StartCombination()
    {
        if (PlayerState.Instance.Money < 1000)
        {
            StartCoroutine(TextFadeDisplay(errorText));
            return;
        }
        StartCoroutine(CombinationPresent());
    }

    Gem CreateCombinationGem(int diceNum)
    {
        Gem gem = new Gem();

        gem.GemKind = leftSelectedGem.GemKind;
        gem.gemOptionList.Clear();

        if (diceNum <= 4) //실패
        {
            reslutText.text = "조합 실패...";
            int rand = Random.Range(0, 2);
            switch (rand)
            {
                case 0:
                    gem.statValue = leftSelectedGem.statValue;
                    gem.gemOptionList = leftSelectedGem.gemOptionList;
                    break;
                case 1:
                    gem.statValue = rightSelectedGem.statValue;
                    gem.gemOptionList = rightSelectedGem.gemOptionList;
                    break;
            }
        }
        else if (diceNum <= 10) //성공
        {
            reslutText.text = "조합 성공";
            int value = leftSelectedGem.statValue + rightSelectedGem.statValue;
            int range = Random.Range(6, 11);
            int statValue = (int)(value * 0.01f * (range * 10));            
            gem.statValue = statValue;

            List<GemOption> combiOption = new List<GemOption>();
            combiOption = leftSelectedGem.gemOptionList.ToList();

            foreach (GemOption rightGemOption in rightSelectedGem.gemOptionList)
            {
                bool isDuplicate = false;
                if (combiOption.Count > 0)
                {
                    for (int i = 0; i < combiOption.Count; i++)
                    {
                        if (combiOption[i].kind.Equals(rightGemOption.kind))
                        {
                            GemOption gemOption = new GemOption();
                            gemOption = combiOption[i];
                            
                            string dialog;
                            char[] split = gemOption.value.ToString().ToCharArray();
                            string[] temp = gemOption.dialog.Split(split);

                            value = gemOption.value + rightGemOption.value;
                            range = Random.Range(6, 11);
                            gemOption.value = (int)(value * 0.01f * (range * 10));

                            dialog = temp[0] + gemOption.value + temp[temp.Length - 1];
                            gemOption.dialog = dialog;

                            combiOption[i] = gemOption;

                            isDuplicate = true;
                            break;
                        }
                    }
                }

                if (isDuplicate) continue;
                else combiOption.Add(rightGemOption);
            }

            gem.gemOptionList = combiOption;
        }
        else //대성공
        {
            reslutText.text = "조합 대성공!";
            gem.statValue = leftSelectedGem.statValue + rightSelectedGem.statValue;

            List<GemOption> combiOption = new List<GemOption>();
            combiOption = leftSelectedGem.gemOptionList.ToList();

            foreach (GemOption rightGemOption in rightSelectedGem.gemOptionList)
            {
                bool isDuplicate = false;
                if (combiOption.Count > 0)
                {
                    for (int i = 0; i < combiOption.Count; i++)
                    {
                        if (combiOption[i].kind.Equals(rightGemOption.kind))
                        {
                            GemOption gemOption = new GemOption();
                            gemOption = combiOption[i];

                            string dialog;
                            char[] split = gemOption.value.ToString().ToCharArray();
                            string[] temp = gemOption.dialog.Split(split);

                            gemOption.value += rightGemOption.value;
                            dialog = temp[0] + gemOption.value + temp[temp.Length - 1];
                            gemOption.dialog = dialog;

                            Debug.Log(gemOption.value);
                            combiOption[i] = gemOption;

                            isDuplicate = true;
                            break;
                        }
                    }
                }

                if (isDuplicate) continue;
                else combiOption.Add(rightGemOption);
            }

            gem.gemOptionList = combiOption;
        }

        return gem;
    }

    IEnumerator CombinationPresent()
    {
        combinationFade.raycastTarget = true;

        combinationCanvas.gameObject.SetActive(false);
        int diceNum = DiceManager.Instance.DiceRoll(2);
        while (!diceEnd) yield return null;
        combinationCanvas.gameObject.SetActive(true);
        diceEnd = false;

        Gem combinationGem = CreateCombinationGem(diceNum);        

        PlayerState.Instance.Money -= 1000;
        for (int i = 0; i < PlayerState.Instance.getGemList.Count; i++)
        {
            if (PlayerState.Instance.getGemList[i].Equals(leftSelectedGem))
            {
                PlayerState.Instance.getGemList.RemoveAt(i);
                leftSelectedGem = new Gem();
                break;
            }
        }

        for (int i = 0; i < PlayerState.Instance.getGemList.Count; i++)
        {
            if (PlayerState.Instance.getGemList[i].Equals(rightSelectedGem))
            {
                PlayerState.Instance.getGemList.RemoveAt(i);
                rightSelectedGem = new Gem();
                break;
            }
        }

        PlayerState.Instance.getGemList.Add(combinationGem);

        Transform leftGemImageParent = leftGemImage.transform.parent;
        Transform rightGemImageParent = rightGemImage.transform.parent;

        leftGemImage.transform.SetParent(combinationCanvas);
        rightGemImage.transform.SetParent(combinationCanvas);

        float leftGemDistance = (leftGemImage.transform.position - combinationPanel.transform.position).sqrMagnitude;
        float rightGemDistance = (rightGemImage.transform.position - combinationPanel.transform.position).sqrMagnitude;
        while (leftGemDistance > float.Epsilon && rightGemDistance > float.Epsilon)
        {
            leftGemImage.transform.position = Vector3.MoveTowards(leftGemImage.transform.position, combinationPanel.transform.position, 1000f * Time.deltaTime);
            leftGemDistance = (leftGemImage.transform.position - combinationPanel.transform.position).sqrMagnitude;
            rightGemImage.transform.position = Vector3.MoveTowards(rightGemImage.transform.position, combinationPanel.transform.position, 1000f * Time.deltaTime);
            rightGemDistance = (rightGemImage.transform.position - combinationPanel.transform.position).sqrMagnitude;
            yield return null;
        }

        yield return StartCoroutine(EffectManager.Instance.FadeOut(combinationFade, 1f));
        leftGemImage.transform.SetParent(leftGemImageParent);
        rightGemImage.transform.SetParent(rightGemImageParent);
        leftGemImage.transform.localPosition = new Vector3(0, 440f, 0);
        rightGemImage.transform.localPosition = new Vector3(0, 440f, 0);
        ResetAllInfo();

        combinationGemInfoPanel.SetActive(true);
        combiGemType.text = combinationGem.gemKindName;
        switch (combinationGem.GemKind)
        {
            case GemKind.RED:
                combiGemValue.text = "Str + " + combinationGem.statValue;
                break;
            case GemKind.BLUE:
                combiGemValue.text = "Int + " + combinationGem.statValue;
                break;
            case GemKind.GREEN:
                combiGemValue.text = "Dex + " + combinationGem.statValue;
                break;
            case GemKind.YELLOW:
                combiGemValue.text = "Vit + " + combinationGem.statValue;
                break;
        }
        combiGemIcon.sprite = combinationGem.gemSprite;

        for (int i = 0; i < combinationGem.gemOptionList.Count; i++)
        {
            GameObject gemOption = Instantiate(Resources.Load("GemOptionList") as GameObject);
            gemOption.transform.GetChild(0).GetComponent<Text>().text = combinationGem.gemOptionList[i].dialog;
            gemOption.transform.SetParent(combiGemOptionContent);
        }

        yield return StartCoroutine(EffectManager.Instance.FadeIn(combinationFade, 1f));
        StartCoroutine(TextFadeDisplay(reslutText));
    }

    public void CombinationGemInfoPanelExit()
    {
        for (int i = 0; i < combiGemOptionContent.childCount; i++)
        {
            Destroy(combiGemOptionContent.GetChild(i).gameObject);
        }
        combinationGemInfoPanel.SetActive(false);
    }


    IEnumerator TextFadeDisplay(Text text)
    {
        yield return StartCoroutine(EffectManager.Instance.FadeOut(text, 0.5f));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(EffectManager.Instance.FadeIn(text, 0.5f));
    }

    public void ResetInfoLeftTouch()
    {
        for (int i = 0; i < rightGemContent.childCount; i++)
        {
            Destroy(rightGemContent.GetChild(i).gameObject);
        }

        for (int i = 0; i < leftGemOptionContent.childCount; i++)
        {
            Destroy(leftGemOptionContent.GetChild(i).gameObject);
        }

        for (int i = 0; i < rightGemOptionContent.childCount; i++)
        {
            Destroy(rightGemOptionContent.GetChild(i).gameObject);
        }

        rightGemImage.sprite = basicGem;
        rightGemType.text = "";
        rightGemValue.text = "";

        if (leftSelectedImage != null)
        {
            leftSelectedImage.color = new Color32(255, 255, 255, 255);
            leftSelectedImage = null;
        }

        combinationPanel.SetActive(false);
    }

    public void ResetInfoRightTouch()
    {
        for (int i = 0; i < rightGemOptionContent.childCount; i++)
        {
            Destroy(rightGemOptionContent.GetChild(i).gameObject);
        }

        if (rightSelectedImage != null)
        {
            rightSelectedImage.color = new Color32(255, 255, 255, 255);
            rightSelectedImage = null;
        }
    }

    public void ResetAllInfo()
    {
        for (int i = 0; i < leftGemContent.childCount; i++)
        {
            Destroy(leftGemContent.GetChild(i).gameObject);
        }

        for (int i = 0; i < PlayerState.Instance.getGemList.Count; i++)
        {
            Gem gem = new Gem();
            gem = PlayerState.Instance.getGemList[i];

            GameObject combiGemSlot = Instantiate(Resources.Load("GemSlot") as GameObject);
            combiGemSlot.AddComponent<CombinationSelectGem>();
            combiGemSlot.GetComponent<CombinationSelectGem>().gem = gem;
            combiGemSlot.GetComponent<CombinationSelectGem>().leftGem = true;
            combiGemSlot.transform.SetParent(leftGemContent);
            combiGemSlot.transform.localScale = Vector3.one;
        }

        for (int i = 0; i < rightGemContent.childCount; i++)
        {
            Destroy(rightGemContent.GetChild(i).gameObject);
        }

        for (int i = 0; i < leftGemOptionContent.childCount; i++)
        {
            Destroy(leftGemOptionContent.GetChild(i).gameObject);
        }

        for (int i = 0; i < rightGemOptionContent.childCount; i++)
        {
            Destroy(rightGemOptionContent.GetChild(i).gameObject);
        }

        leftGemImage.sprite = basicGem;
        leftGemType.text = "";
        leftGemValue.text = "";

        rightGemImage.sprite = basicGem;
        rightGemType.text = "";
        rightGemValue.text = "";

        if (leftSelectedImage != null)
        {
            leftSelectedImage.color = new Color32(255, 255, 255, 255);
        }
        leftSelectedImage = null;
        rightSelectedImage = null;

        leftSelectedGem = new Gem();
        rightSelectedGem = new Gem();

        combinationPanel.SetActive(false);
    }
}
