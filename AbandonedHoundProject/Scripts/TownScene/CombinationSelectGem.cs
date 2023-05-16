using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombinationSelectGem : SelectGem
{
    [HideInInspector]
    public bool leftGem;

    void Start()
    {
        transform.GetChild(0).GetComponent<Image>().sprite = gem.gemSprite;
    }

    public override void OnPointerDown(PointerEventData eventData) { }

    public override void OnPointerUp(PointerEventData eventData)
    {
        switch (leftGem)
        {
            case true:
                {
                    List<Gem> equalTypeGemList = new List<Gem>();

                    Combination.Instance.ResetInfoLeftTouch();

                    Combination.Instance.leftSelectedGem = gem;
                    Image sel = GetComponent<Image>();
                    Combination.Instance.leftSelectedImage = sel;
                    sel.color = new Color32(0, 0, 0, 255);

                    Combination.Instance.leftGemImage.sprite = gem.gemSprite;
                    Combination.Instance.leftGemType.text = gem.gemKindName;
                    switch (gem.GemKind)
                    {
                        case GemKind.RED:
                            Combination.Instance.leftGemValue.text = "Str + " + gem.statValue;
                            foreach (Gem gem in PlayerState.Instance.getGemList)
                            {
                                if (gem.GemKind.Equals(GemKind.RED) && !gem.Equals(this.gem))
                                    equalTypeGemList.Add(gem);
                            }
                            break;
                        case GemKind.BLUE:
                            Combination.Instance.leftGemValue.text = "Int + " + gem.statValue;
                            foreach (Gem gem in PlayerState.Instance.getGemList)
                            {
                                if (gem.GemKind.Equals(GemKind.BLUE) && !gem.Equals(this.gem))
                                    equalTypeGemList.Add(gem);
                            }
                            break;
                        case GemKind.GREEN:
                            Combination.Instance.leftGemValue.text = "Dex + " + gem.statValue;
                            foreach (Gem gem in PlayerState.Instance.getGemList)
                            {
                                if (gem.GemKind.Equals(GemKind.GREEN) && !gem.Equals(this.gem))
                                    equalTypeGemList.Add(gem);
                            }
                            break;
                        case GemKind.YELLOW:
                            Combination.Instance.leftGemValue.text = "Vit + " + gem.statValue;
                            foreach (Gem gem in PlayerState.Instance.getGemList)
                            {
                                if (gem.GemKind.Equals(GemKind.YELLOW) && !gem.Equals(this.gem))
                                    equalTypeGemList.Add(gem);
                            }
                            break;
                    }

                    for (int i = 0; i < gem.gemOptionList.Count; i++)
                    {
                        GameObject gemOption = Instantiate(Resources.Load("GemOptionList") as GameObject);
                        gemOption.transform.GetChild(0).GetComponent<Text>().text = gem.gemOptionList[i].dialog;
                        gemOption.transform.SetParent(Combination.Instance.leftGemOptionContent);
                    }

                    for (int i = 0; i < equalTypeGemList.Count; i++)
                    {
                        GameObject gemSlot = Instantiate(Resources.Load("GemSlot") as GameObject);
                        gemSlot.AddComponent<CombinationSelectGem>();
                        gemSlot.GetComponent<CombinationSelectGem>().gem = equalTypeGemList[i];
                        gemSlot.transform.SetParent(Combination.Instance.rightGemContent);
                        gemSlot.transform.localScale = Vector3.one;
                    }
                    break;
                }
            case false:
                {
                    Combination.Instance.ResetInfoRightTouch();

                    Combination.Instance.rightSelectedGem = gem;
                    Image sel = GetComponent<Image>();
                    Combination.Instance.rightSelectedImage = sel;
                    sel.color = new Color32(0, 0, 0, 255);

                    Combination.Instance.rightGemImage.sprite = gem.gemSprite;
                    Combination.Instance.rightGemType.text = gem.gemKindName;
                    switch (gem.GemKind)
                    {
                        case GemKind.RED:
                            Combination.Instance.rightGemValue.text = "Str + " + gem.statValue;
                            break;
                        case GemKind.BLUE:
                            Combination.Instance.rightGemValue.text = "Int + " + gem.statValue;
                            break;
                        case GemKind.GREEN:
                            Combination.Instance.rightGemValue.text = "Dex + " + gem.statValue;
                            break;
                        case GemKind.YELLOW:
                            Combination.Instance.rightGemValue.text = "Vit + " + gem.statValue;
                            break;
                    }

                    for (int i = 0; i < gem.gemOptionList.Count; i++)
                    {
                        GameObject gemOption = Instantiate(Resources.Load("GemOptionList") as GameObject);
                        gemOption.transform.GetChild(0).GetComponent<Text>().text = gem.gemOptionList[i].dialog;
                        gemOption.transform.SetParent(Combination.Instance.rightGemOptionContent);
                    }

                    Combination.Instance.combinationPanel.SetActive(true);
                    break;
                }
        }
    }

    public override void CancleTouch() { }
}
