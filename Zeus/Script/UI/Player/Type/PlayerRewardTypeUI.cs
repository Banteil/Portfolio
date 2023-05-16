using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zeus;

public class PlayerRewardTypeUI : PlayerUIType
{
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private float _duration;

    public struct RewardInfoText
    {
        public string Name;
        public Sprite Icon;
    }

    private bool _isPlaying;
    private List<RewardInfoText> _rewardInfos = new List<RewardInfoText>();

    private void Awake()
    {
        UIType = TypePlayerUI.REWARD;
    }
    protected override void Start()
    {
        base.Start();
        _canvas.alpha = 0f;
        _isPlaying = false;
    }
    private void Update()
    {
        if (_rewardInfos.Count == 0) return;
        if (_isPlaying) return;

        StartCoroutine(AppearCO(_rewardInfos[0]));
    }
    private IEnumerator AppearCO(RewardInfoText rewardInfo)
    {
        _isPlaying = true;

        SetItem(rewardInfo.Name, rewardInfo.Icon);

        bool isDone = false;
        _canvas.DOFade(1, 2f).SetEase(Ease.OutCubic).onComplete += () =>
        {
            isDone = true;
        };
        yield return new WaitUntil(() => isDone);

        yield return new WaitForSeconds(_duration);

        isDone = false;
        _canvas.DOFade(0, 2f).SetEase(Ease.InOutCubic).onComplete += () =>
        {
            isDone = true;
        };
        yield return new WaitUntil(() => isDone);

        _rewardInfos.Remove(rewardInfo);
        _isPlaying = false;
    }

    public void SetVisible(bool visiabled, Ease ease, TweenCallback onComplete = null)
    {
        _canvas.DOFade(visiabled ? 1 : 0, _duration).SetEase(ease).onComplete = onComplete;
    }

    public void SetItem(string itemName, Sprite itemIcon)
    {
        _itemName.SetText(itemName);
        //_itemIcon.sprite = itemIcon;
    }
    public void AddItem(string itemName, Sprite itemIcon)
    {
        var rewardInfo = new RewardInfoText();
        rewardInfo.Name = itemName;
        rewardInfo.Icon = itemIcon;
        _rewardInfos.Add(rewardInfo);
    }
}
