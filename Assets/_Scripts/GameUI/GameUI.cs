using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gameplay;
using DG.Tweening;
using TMPro;

namespace Gameplay
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private UI_Manager uiManager;
        [SerializeField] private RectTransform bottomPanelRect, sliderPanelRect;
        [SerializeField] private GameObject betButton, checkButtonGO, callButtonGO, allInButtonGO, foldButtonGO,
            betAmountGO;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI totalCoinsTMP, betButtonTMP, betAmountTMP;
        [SerializeField] private GameObject[] addToSlotGO;

        private double betOrRaiseAmount;
        private float sliderPanelWidth, bottomPanelHeight;

        private void Start()
        {
            sliderPanelWidth = sliderPanelRect.rect.width;
            bottomPanelHeight = bottomPanelRect.rect.height;
            sliderPanelRect.anchoredPosition = new Vector2(sliderPanelWidth, 0);
            bottomPanelRect.anchoredPosition = new Vector2(0, -bottomPanelHeight);
        }

        public void MoveControls(bool inside)
        {
            if (sliderPanelWidth == 0 && bottomPanelHeight == 0) return;

            float duration = 0.2f;
            var sliderTarget = inside ? new Vector2(0, 0) : new Vector2(sliderPanelWidth, 0);
            var bottomPanelTarget = inside ? new Vector2(0, 0) : new Vector2(0, -bottomPanelHeight); ;

            sliderPanelRect.DOAnchorPos(sliderTarget, duration);
            bottomPanelRect.DOAnchorPos(bottomPanelTarget, duration);
        }

        public void UpdateTotalCoins()
        {
            totalCoinsTMP.text = Globals.userInfo.chips.ToString("0.00");
        }

        public void DisableAllControls()
        {
            DisableBetOrRaise();
            SetControls(false, false, false, false);
            MoveControls(false);
        }

        public void OnSliderValueChanged()
        {
            betOrRaiseAmount = slider.value;
            betAmountTMP.text = betOrRaiseAmount.ToString("0.00");
        }

        public void EnableBetOrRaise(double minVal, double maxVal, string text = "Bet")
        {
            betButton.SetActive(true);
            betAmountGO.SetActive(true);
            slider.gameObject.SetActive(true);
            slider.minValue = (float)minVal;
            slider.maxValue = (float)maxVal;
            slider.value = (float)minVal;
            betOrRaiseAmount = minVal;
            betButtonTMP.text = text;
        }
        public void DisableBetOrRaise()
        {
            betButton.SetActive(false);
            slider.gameObject.SetActive(false);
            betAmountGO.SetActive(false);
        }

        public void SetControls(bool check, bool call, bool fold, bool allIn)
        {
            checkButtonGO.SetActive(check);
            callButtonGO.SetActive(call);
            foldButtonGO.SetActive(fold);
            allInButtonGO.SetActive(allIn);
        }

        public void BetOrRaise()
        {
            Globals.MoveType move;
            move = slider.value == slider.maxValue ? Globals.MoveType.AllIn
                : betButtonTMP.text == "Bet" ? Globals.MoveType.Bet : Globals.MoveType.Raise;
            GameManager.instance.gameplayManager.PerformAction(move, slider.value);
        }

        public void Fold()
        {
            GameManager.instance.gameplayManager.PerformAction(Globals.MoveType.Fold);
        }

        public void AllIn()
        {
            GameManager.instance.gameplayManager.PerformAction(Globals.MoveType.AllIn);
        }

        public void Check()
        {
            GameManager.instance.gameplayManager.PerformAction(Globals.MoveType.Check);
        }

        public void Call()
        {
            GameManager.instance.gameplayManager.PerformAction(Globals.MoveType.Call);
        }

        public void DisableAllAddButtons()
        {
            foreach(var go in addToSlotGO)
            {
                go.SetActive(false);
            }
        }

        public void ShowAddToSlotButton(int index, bool show)
        {
            addToSlotGO[index].SetActive(show);
        }

        public void AddPlayerToSlot(int slot)
        {
            GameManager.instance.gameplayManager.AddPlayerToTable(slot);
        }
    }
}
