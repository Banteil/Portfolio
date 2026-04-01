using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageSelectPopup : MonoBehaviour
{
    [SerializeField]
    int worldIndex;
    public int WorldIndex
    {
        set
        {
            worldIndex = value;
            worldBackground.sprite = LobbyResourceManager.Instance.WorldBackgroundSprite[worldIndex];
            int maxIndex = 15;
            //월드맵 인덱스에 따라 스테이지 세팅, 데모에선 그냥 배치
            for (int i = 0; i < maxIndex; i++)
            {
                GameObject stageObj = Instantiate(LobbyResourceManager.Instance.StageButtonObj, stageTr, false);
                StageSelectButton stageButton = stageObj.GetComponent<StageSelectButton>();
                stageButton.SetIndexInfo(worldIndex, i);
                stageSelectButtons.Add(stageButton);
                SetButtonPosition(i);
                //중간보스 처리
                if (((i + 1) % 5).Equals(0))
                {
                    stageButton.ButtonImage.sprite = Resources.Load<Sprite>("Sprites/UI/Lobby/StageMenu/MidBoss");
                    stageButton.transform.localScale = new Vector3(1.5f, 1.5f);
                }
            }

            //보스 스테이지는 별도 설정
            StageSelectButton bossButton = stageSelectButtons[stageSelectButtons.Count - 1];
            bossButton.ButtonImage.sprite = Resources.Load<Sprite>("Sprites/UI/Logo/Boss/" + bossID + "_BossLogo");
            RectTransform bossRT = bossButton.GetComponent<RectTransform>();
            bossRT.anchoredPosition = new Vector3(0f, bossRT.anchoredPosition.y);
            bossRT.localScale = new Vector3(3f, 3f);

            //버튼들 사이에 라인 그림
            DrawStageLine();
            //스테이지 꾸미기 이미지 추가
            SetDecoration();
            //스테이지 오픈 여부 체크
            StageOpenCheck();

            //스테이지 스크롤 크기 조절
            stageTr.sizeDelta = new Vector2(stageTr.sizeDelta.x, 400f + bossButton.GetComponent<RectTransform>().anchoredPosition.y);
        }
    }

    int bossID;
    public int BossID { set { bossID = value; } }

    [SerializeField]
    RectTransform content;
    [SerializeField]
    RectTransform stageTr;
    [SerializeField]
    Image worldBackground;
    [SerializeField]
    TextMeshProUGUI worldNameText;
    public TextMeshProUGUI WorldNameText { get { return worldNameText; } }

    List<StageSelectButton> stageSelectButtons = new List<StageSelectButton>();
    public List<StageSelectButton> StageSelectButtons { get { return stageSelectButtons; } }

    void SetButtonPosition(int index)
    {
        if (index.Equals(0))
            stageSelectButtons[index].GetComponent<RectTransform>().anchoredPosition = new Vector2(Random.Range(-200f, 50f), 80f);
        else
        {
            float angle = 0;
            switch (index % 2)
            {
                case 0:
                    angle = Random.Range(20f, 50f);
                    break;
                case 1:
                    angle = Random.Range(-50f, -20f);
                    break;
            }
            Quaternion quaternion = Quaternion.Euler(0, 0, angle);
            Vector3 prevPos = stageSelectButtons[index - 1].GetComponent<RectTransform>().anchoredPosition;
            RectTransform buttonRT = stageSelectButtons[index].GetComponent<RectTransform>();
            buttonRT.anchoredPosition = prevPos + (quaternion * Vector3.up * 150f);
            if (buttonRT.anchoredPosition.x < -200f)
                buttonRT.anchoredPosition = new Vector2(-200f, buttonRT.anchoredPosition.y);
            else if (buttonRT.anchoredPosition.x > 200)
                buttonRT.anchoredPosition = new Vector2(200f, buttonRT.anchoredPosition.y);
        }
    }

    void DrawStageLine()
    {
        for (int i = 0; i < stageSelectButtons.Count - 1; i++)
        {
            RectTransform startButtonRT = stageSelectButtons[i].GetComponent<RectTransform>();
            RectTransform endButtonRT = stageSelectButtons[i + 1].GetComponent<RectTransform>();
            float startButtonRadius = startButtonRT.rect.height / 2f;
            float endButtonRadius = endButtonRT.rect.height / 2f;

            GameObject lineObj = Instantiate(LobbyResourceManager.Instance.LineObj, stageTr, false);
            lineObj.transform.SetAsFirstSibling();
            RectTransform lineRT = lineObj.GetComponent<RectTransform>();
            lineRT.anchoredPosition = new Vector2(startButtonRT.anchoredPosition.x, startButtonRT.anchoredPosition.y + startButtonRadius);
            float dis = Vector2.Distance(startButtonRT.anchoredPosition, endButtonRT.anchoredPosition);
            lineRT.sizeDelta = new Vector2(100f, dis * 4);
            float angle = Mathf.Atan2((endButtonRT.anchoredPosition.y + endButtonRadius) - (startButtonRT.anchoredPosition.y + startButtonRadius),
                endButtonRT.anchoredPosition.x - startButtonRT.anchoredPosition.x) * Mathf.Rad2Deg;
            lineRT.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        }
    }

    void SetDecoration()
    {
        for (int i = 0; i < stageSelectButtons.Count; i++)
        {
            GameObject leftObj = new GameObject("Decoration_Left", typeof(RectTransform));
            leftObj.AddComponent<Image>().sprite = LobbyResourceManager.Instance.WorldDecorationSprites[worldIndex][Random.Range(0, LobbyResourceManager.Instance.WorldDecorationSprites[worldIndex].Length)];
            leftObj.GetComponent<Image>().color = new Color32(255, 255, 255, 200);
            leftObj.transform.SetParent(stageTr, false);
            leftObj.transform.SetAsFirstSibling();
            RectTransform leftRT = leftObj.GetComponent<RectTransform>();
            leftRT.pivot = new Vector2(0.5f, 0f);
            leftRT.anchorMax = new Vector2(0.5f, 0f);
            leftRT.anchorMin = new Vector2(0.5f, 0f);
            leftRT.anchoredPosition = new Vector2(Random.Range(-350, -150f), stageSelectButtons[i].GetComponent<RectTransform>().anchoredPosition.y + Random.Range(-20, 20f));

            GameObject rightObj = new GameObject("Decoration_Right", typeof(RectTransform));
            rightObj.AddComponent<Image>().sprite = LobbyResourceManager.Instance.WorldDecorationSprites[worldIndex][Random.Range(0, LobbyResourceManager.Instance.WorldDecorationSprites[worldIndex].Length)];
            rightObj.GetComponent<Image>().color = new Color32(255, 255, 255, 200);
            rightObj.transform.SetParent(stageTr, false);
            rightObj.transform.SetAsFirstSibling();
            RectTransform rightRT = rightObj.GetComponent<RectTransform>();
            rightRT.pivot = new Vector2(0.5f, 0f);
            rightRT.anchorMax = new Vector2(0.5f, 0f);
            rightRT.anchorMin = new Vector2(0.5f, 0f);
            rightRT.anchoredPosition = new Vector2(Random.Range(150, 350f), stageSelectButtons[i].GetComponent<RectTransform>().anchoredPosition.y + Random.Range(-20, 20f));
        }
    }

    void StageOpenCheck()
    {
        for (int i = 0; i < TableDatabase.Instance.UserStageTable.infoList.Count; i++)
        {
            if (TableDatabase.Instance.StageTable.infoList[TableDatabase.Instance.UserStageTable.infoList[i].stageID].world.Equals(worldIndex))
            {
                if(TableDatabase.Instance.UserStageTable.infoList[i].isClear)
                {
                    stageSelectButtons[TableDatabase.Instance.StageTable.infoList[TableDatabase.Instance.UserStageTable.infoList[i].stageID].stage].IsOpen = true;
                    if(stageSelectButtons.Count > TableDatabase.Instance.StageTable.infoList[TableDatabase.Instance.UserStageTable.infoList[i].stageID].stage + 1)
                        stageSelectButtons[TableDatabase.Instance.StageTable.infoList[TableDatabase.Instance.UserStageTable.infoList[i].stageID].stage + 1].IsOpen = true;
                }
                else
                {
                    stageSelectButtons[TableDatabase.Instance.StageTable.infoList[TableDatabase.Instance.UserStageTable.infoList[i].stageID].stage].IsOpen = true;
                }
            }
        }
    }

    private void OnDisable()
    {
        content.anchoredPosition = Vector2.zero;
        for (int i = 0; i < stageTr.childCount; i++)
        {
            Destroy(stageTr.GetChild(i).gameObject);
        }
        stageSelectButtons = new List<StageSelectButton>();
    }
}
