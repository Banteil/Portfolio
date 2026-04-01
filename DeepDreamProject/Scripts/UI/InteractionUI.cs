using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    const string InteractionKey = "E";

    CanvasGroup _canvasGroup;
    [SerializeField]
    protected Text _keyInfoText;
    [SerializeField]
    protected Text _actInfoText;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        SetKeyInfo(InteractionKey);
    }

    public void SetActive(bool isActive)
    {
        if (isActive)
            _canvasGroup.alpha = 1;
        else
        {
            _canvasGroup.alpha = 0;
            SetKeyInfo(InteractionKey);
            SetActInfo("");
        }
    }

    public void SetKeyInfo(string info) => _keyInfoText.text = info;
    public void SetActInfo(string info) => _actInfoText.text = info;
}
