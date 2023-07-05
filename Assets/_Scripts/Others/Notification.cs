using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Notification : MonoBehaviour
{
    [SerializeField] private RectTransform notificationPanel;
    [SerializeField] private TextMeshProUGUI messageTMP;
    [SerializeField] private Button button1, button2;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private int minHeight;

    private Action button1Callback, button2Callback;

    private void Start()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = true;
        StartCoroutine(RescaleToFitText());
    }

    private IEnumerator RescaleToFitText()
    {
        int frames = 0;

        while(frames < 5)
        {
            frames++;
            yield return null;
        }

        float totalHeight = messageTMP.GetPreferredValues().y + minHeight;
        notificationPanel.sizeDelta = new Vector2(notificationPanel.sizeDelta.x, totalHeight);

        float duration = 0.2f;
        notificationPanel.localScale *= 0.5f;
        notificationPanel.DOScale(1, duration);
        canvasGroup.DOFade(1, duration);
    }

    private IEnumerator Close()
    {
        yield return new WaitForSeconds(0);
        canvasGroup.interactable = false;

        float duration = 0.15f;
        notificationPanel.DOScale(0.5f, duration);
        canvasGroup.DOFade(0, duration).OnComplete(() =>
        {
            DOTween.Kill(notificationPanel);
            DOTween.Kill(canvasGroup);
            Destroy(gameObject);
        });
    }

    public void ShowNotification(string message, string buttonText, Action callback = null)
    {
        messageTMP.text = message;
        button1.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
        button1.gameObject.SetActive(true);
        button2.gameObject.SetActive(false);

        button1Callback = callback;
        button2Callback = null;
    }

    public void ShowNotification(string message, string button1Text, string button2Text,
        Action button1Callback = null, Action button2Callback = null)
    {
        messageTMP.text = message;
        button1.GetComponentInChildren<TextMeshProUGUI>().text = button1Text;
        button2.GetComponentInChildren<TextMeshProUGUI>().text = button2Text;
        button1.gameObject.SetActive(true);
        button2.gameObject.SetActive(true);

        this.button1Callback = button1Callback;
        this.button2Callback = button2Callback;
    }

    public void OnTappedButton(int index)
    {
        if(index == 0)
        {
            button1Callback?.Invoke();
        }
        else
        {
            button2Callback?.Invoke();
        }

        CloseNotification();
    }

    public void CloseNotification()
    {
        StartCoroutine(Close());
    }
}
