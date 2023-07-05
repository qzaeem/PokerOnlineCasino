using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Rules : MonoBehaviour
{
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Start()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowRules()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        DOTween.Kill(panelRect);
        DOTween.Kill(canvasGroup);
        float duration = 0.2f;
        panelRect.localScale *= 0.5f;
        panelRect.DOScale(1, duration);
        canvasGroup.DOFade(1, duration);
    }

    public void CloseRules()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float duration = 0.15f;
        panelRect.DOScale(0.5f, duration);
        canvasGroup.DOFade(0, duration).OnComplete(() =>
        {
            DOTween.Kill(panelRect);
            DOTween.Kill(canvasGroup);
        });
    }
}
