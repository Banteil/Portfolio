using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

///<summary>
///상점에 관련된 변수 및 함수가 저장된 클래스
///</summary>
public class Shop : Singleton<Shop>
{

    ////////////////////////////////////////////
    int PotionPrice
    {
        get { return PlayerState.Instance.potionGrade * 300; }
    }
    int FoodPrice
    {
        get { return PlayerState.Instance.foodGrade * 200; }
    }
    int BombPrice
    {
        get { return PlayerState.Instance.bombGrade * 500; }
    }
    //////////////////////소모품 가격//////////////////////

    ////////////////////////////////////////////
    int PotionGradeUpPrice
    {
        get { return PlayerState.Instance.potionGrade * (PlayerState.Instance.potionGrade * 500); }
    }

    int FoodGradeUpPrice
    {
        get { return PlayerState.Instance.foodGrade * (PlayerState.Instance.foodGrade * 500); }
    }

    int BombGradeUpPrice
    {
        get { return PlayerState.Instance.bombGrade * (PlayerState.Instance.bombGrade * 500); }
    }
    //////////////////////소모품 등급 업 가격//////////////////////

    ////////////////////////////////////////////
    public Text potionPrice, foodPrice, bombPrice;
    public Text potionGrade, foodGrade, bombGrade;
    public Text haveMoneyValue, havePotionValue, havefoodValue, haveBombValue;
    public Text buyReslutText;
    public Image potionSlot, foodSlot, bombSlot;
    public GameObject potionGradeUp, foodGradeUp, bombGradeUp;
    public GameObject itemInfoPanel;
    Coroutine buyDirection;
    //////////////////////UI 객체를 저장한 변수

    [HideInInspector]
    public bool touchPotion, touchFood, touchBomb;
    //어떤 객체를 터치했는지 여부를 판단하는 bool

    void Start()
    {
        SettingTextValue();
    }

    ///<summary>
    ///상점 내 텍스트 UI 정보를 업데이트하는 함수
    ///</summary>
    public void SettingTextValue()
    {
        potionPrice.text = PotionPrice.ToString();
        foodPrice.text = FoodPrice.ToString();
        bombPrice.text = BombPrice.ToString();

        switch (PlayerState.Instance.potionGrade)
        {
            case 1:
                potionGrade.text = "최하급 포션";
                break;
            case 2:
                potionGrade.text = "하급 포션";
                break;
            case 3:
                potionGrade.text = "중급 포션";
                break;
            case 4:
                potionGrade.text = "상급 포션";
                break;
            case 5:
                potionGrade.text = "최상급 포션";
                break;
        }

        switch (PlayerState.Instance.foodGrade)
        {
            case 1:
                foodGrade.text = "최하급 음식";
                break;
            case 2:
                foodGrade.text = "하급 음식";
                break;
            case 3:
                foodGrade.text = "중급 음식";
                break;
            case 4:
                foodGrade.text = "상급 음식";
                break;
            case 5:
                foodGrade.text = "최상급 음식";
                break;
        }

        switch (PlayerState.Instance.bombGrade)
        {
            case 1:
                bombGrade.text = "최하급 폭탄";
                break;
            case 2:
                bombGrade.text = "하급 폭탄";
                break;
            case 3:
                bombGrade.text = "중급 폭탄";
                break;
            case 4:
                bombGrade.text = "상급 폭탄";
                break;
            case 5:
                bombGrade.text = "최상급 폭탄";
                break;
        }

        haveMoneyValue.text = PlayerState.Instance.Money.ToString();
        havePotionValue.text = PlayerState.Instance.GetPotion.ToString();
        havefoodValue.text = PlayerState.Instance.GetFood.ToString();
        haveBombValue.text = PlayerState.Instance.GetBomb.ToString();
    }

    ///<summary>
    ///상점 내 실제 정보를 초기화하는 함수
    ///</summary>
    public void ResetInfo()
    {
        touchPotion = false;
        touchFood = false;
        touchBomb = false;
        potionSlot.color = new Color32(255, 255, 255, 255);
        foodSlot.color = new Color32(255, 255, 255, 255);
        bombSlot.color = new Color32(255, 255, 255, 255);
        itemInfoPanel.SetActive(false);

        if (PlayerState.Instance.potionGrade > 4) potionGradeUp.SetActive(false);
        if (PlayerState.Instance.foodGrade > 4) foodGradeUp.SetActive(false);
        if (PlayerState.Instance.bombGrade > 4) bombGradeUp.SetActive(false);
    }

    ///<summary>
    ///포션을 구매할 때 실행되는 함수
    ///</summary>
    public void BuyPotion()
    {
        if (!touchPotion) //첫번째 터치일 때
        {
            touchPotion = true;
            touchFood = false;
            touchBomb = false;

            potionSlot.color = new Color32(255, 143, 0, 255);
            foodSlot.color = new Color32(255, 255, 255, 255);
            bombSlot.color = new Color32(255, 255, 255, 255);

            itemInfoPanel.SetActive(true);
            Text itemInfo = itemInfoPanel.GetComponentInChildren<Text>();
            itemInfo.text = "사용 시 HP를 " + Expendables.PotionValue + "%만큼 회복시켜주는 아이템";
        }
        else //두번째 터치일 때
        {
            touchPotion = false;
            potionSlot.color = new Color32(255, 255, 255, 255);
            itemInfoPanel.SetActive(false);

            if (PlayerState.Instance.Money < PotionPrice)
            {
                buyReslutText.text = "돈이 부족하다..";
                if (buyDirection != null) StopCoroutine(buyDirection);
                buyDirection = StartCoroutine(BuyReslut());
                return;
            }

            if (PlayerState.Instance.GetPotion == 9)
            {
                buyReslutText.text = "더 이상 소지할 수 없다";
                if (buyDirection != null) StopCoroutine(buyDirection);
                buyDirection = StartCoroutine(BuyReslut());
                return;
            }

            PlayerState.Instance.Money -= PotionPrice;
            PlayerState.Instance.GetPotion++;
            SettingTextValue();

            buyReslutText.text = "구매 완료";
            if (buyDirection != null) StopCoroutine(buyDirection);
            buyDirection = StartCoroutine(BuyReslut());
        }
    }

    ///<summary>
    ///음식을 구매할 때 실행되는 함수
    ///</summary>
    public void BuyFood()
    {
        if (!touchFood)
        {
            touchPotion = false;
            touchFood = true;
            touchBomb = false;

            potionSlot.color = new Color32(255, 255, 255, 255);
            foodSlot.color = new Color32(255, 143, 0, 255);
            bombSlot.color = new Color32(255, 255, 255, 255);

            itemInfoPanel.SetActive(true);
            Text itemInfo = itemInfoPanel.GetComponentInChildren<Text>();
            itemInfo.text = "던전에서 사용 시 스태미나를 " + Expendables.FoodValue + "%만큼 회복시켜주며,\n" +
                "전투 시 사용하면 AP를 " + Expendables.BattleUseFood + "만큼 증가시켜주는 아이템";
        }
        else
        {
            touchFood = false;
            foodSlot.color = new Color32(255, 255, 255, 255);
            itemInfoPanel.SetActive(false);

            if (PlayerState.Instance.Money < FoodPrice)
            {
                buyReslutText.text = "돈이 부족하다..";
                if (buyDirection != null) StopCoroutine(buyDirection);
                buyDirection = StartCoroutine(BuyReslut());
                return;
            }

            if (PlayerState.Instance.GetFood == 9)
            {
                buyReslutText.text = "더 이상 소지할 수 없다";
                if (buyDirection != null) StopCoroutine(buyDirection);
                buyDirection = StartCoroutine(BuyReslut());
                return;
            }

            PlayerState.Instance.Money -= FoodPrice;
            PlayerState.Instance.GetFood++;
            SettingTextValue();

            buyReslutText.text = "구매 완료";
            if (buyDirection != null) StopCoroutine(buyDirection);
            buyDirection = StartCoroutine(BuyReslut());
        }
    }

    ///<summary>
    ///폭탄을 구매할 때 실행되는 함수
    ///</summary>
    public void BuyBomb()
    {
        if (!touchBomb)
        {
            touchPotion = false;
            touchFood = false;
            touchBomb = true;

            potionSlot.color = new Color32(255, 255, 255, 255);
            foodSlot.color = new Color32(255, 255, 255, 255);
            bombSlot.color = new Color32(255, 143, 0, 255);

            itemInfoPanel.SetActive(true);
            Text itemInfo = itemInfoPanel.GetComponentInChildren<Text>();
            itemInfo.text = "던전에서 사용 시 바라보고 있는 벽을 한번만에 부숴주며,\n" +
                "전투 시 사용하면 적의 HP를 고정값으로 " + Expendables.AttackBomb + "만큼 감소시켜주는 아이템";
        }
        else
        {
            touchBomb = false;
            bombSlot.color = new Color32(255, 255, 255, 255);
            itemInfoPanel.SetActive(false);

            if (PlayerState.Instance.Money < BombPrice)
            {
                buyReslutText.text = "돈이 부족하다..";
                if (buyDirection != null) StopCoroutine(buyDirection);
                buyDirection = StartCoroutine(BuyReslut());
                return;
            }

            if (PlayerState.Instance.GetBomb == 9)
            {
                buyReslutText.text = "더 이상 소지할 수 없다";
                if (buyDirection != null) StopCoroutine(buyDirection);
                buyDirection = StartCoroutine(BuyReslut());
                return;
            }

            PlayerState.Instance.Money -= BombPrice;
            PlayerState.Instance.GetBomb++;
            SettingTextValue();

            buyReslutText.text = "구매 완료";
            if (buyDirection != null) StopCoroutine(buyDirection);
            buyDirection = StartCoroutine(BuyReslut());
        }
    }

    ///<summary>
    ///포션의 등급을 상승시킬 때 실행되는 함수
    ///</summary>
    public void PotionGradeUp()
    {
        if (PlayerState.Instance.Money < PotionGradeUpPrice)
        {
            buyReslutText.text = PotionGradeUpPrice + "골드가 필요하다";
            if (buyDirection != null) StopCoroutine(buyDirection);
            buyDirection = StartCoroutine(BuyReslut());
            return;
        }

        PlayerState.Instance.Money -= PotionGradeUpPrice;
        PlayerState.Instance.potionGrade++;
        SettingTextValue();

        buyReslutText.text = "등급 상승 완료";
        if (buyDirection != null) StopCoroutine(buyDirection);
        buyDirection = StartCoroutine(BuyReslut());

        ResetInfo();
    }

    ///<summary>
    ///음식의 등급을 상승시킬 때 실행되는 함수
    ///</summary>
    public void FoodGradeUp()
    {       
        if (PlayerState.Instance.Money < FoodGradeUpPrice)
        {
            buyReslutText.text = FoodGradeUpPrice + "골드가 필요하다";
            if (buyDirection != null) StopCoroutine(buyDirection);
            buyDirection = StartCoroutine(BuyReslut());
            return;
        }

        PlayerState.Instance.Money -= FoodGradeUpPrice;
        PlayerState.Instance.foodGrade++;
        SettingTextValue();

        buyReslutText.text = "등급 상승 완료";
        if (buyDirection != null) StopCoroutine(buyDirection);
        buyDirection = StartCoroutine(BuyReslut());

        ResetInfo();
    }

    ///<summary>
    ///폭탄의 등급을 상승시킬 때 실행되는 함수
    ///</summary>
    public void BombGradeUp()
    {
        if (PlayerState.Instance.Money < BombGradeUpPrice)
        {
            buyReslutText.text = BombGradeUpPrice + "골드가 필요하다";
            if (buyDirection != null) StopCoroutine(buyDirection);
            buyDirection = StartCoroutine(BuyReslut());
            return;
        }

        PlayerState.Instance.Money -= BombGradeUpPrice;
        PlayerState.Instance.bombGrade++;
        SettingTextValue();

        buyReslutText.text = "등급 상승 완료";
        if (buyDirection != null) StopCoroutine(buyDirection);
        buyDirection = StartCoroutine(BuyReslut());

        ResetInfo();
    }

    ///<summary>
    ///구매 결과에 대한 텍스트 연출 코루틴
    ///</summary>
    IEnumerator BuyReslut()
    {
        yield return StartCoroutine(EffectManager.Instance.FadeOut(buyReslutText, 0.5f));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(EffectManager.Instance.FadeIn(buyReslutText, 0.5f));
    }


}
