using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Gameplay
{
    public class Player : MonoBehaviour
    {
        [HideInInspector] public List<int> cards = new List<int>();
        [HideInInspector] public bool isPlaying;

        [SerializeField] TextMeshProUGUI coinsTMP, coinsOnTableTMP, playerNameTMP;
        [SerializeField] GameObject activeImg, hiddenCardsPanel;
        [SerializeField] Image card1Image, card2Image, displayPic, actionImage, timerImage;
        [SerializeField] Sprite cardBackSprite;
        [SerializeField] Sprite[] actionSprites;
        [SerializeField] Image[] canvasGroupImages;
        [SerializeField] TextMeshProUGUI[] canvasGroupTexts;

        private Animator cardsAnimator;
        private float timerVal = 0;
        private bool isPlayer, isTimerOn;
        private string playerName;

        public PlayerProfile Profile { get; private set; }
        public bool IsActive { get; private set; }


        #region Private Functions
        private void Awake()
        {
            cardsAnimator = card1Image.transform.parent.GetComponent<Animator>();
        }

        private void Update()
        {
            if (!isTimerOn) return;

            timerVal -= Time.deltaTime;
            timerVal = Mathf.Clamp(timerVal, 0, Globals.maxTimeToPerformAction);
            float halfDiff = timerVal - Globals.maxTimeToPerformAction / 2;
            float fillAmount = halfDiff > 0 ? halfDiff * 2 / Globals.maxTimeToPerformAction : 0;

            timerImage.fillAmount = fillAmount;

            if(timerVal <= 0)
            {
                isTimerOn = false;
                timerImage.gameObject.SetActive(false);
                timerVal = Globals.maxTimeToPerformAction;

                if(Profile.roomID == GameManager.instance.gameplayManager.RoomID)
                    GameManager.instance.gameplayManager.PerformAction(Globals.MoveType.Fold);
            }
        }

        private IEnumerator InactiveTimer()
        {
            float timer = Globals.playerTTL / 1000;

            while(timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            GameManager.instance.gameplayManager.RemoveInactivePlayer(Profile.roomID);
        }

        #endregion

        #region Public Functions
        public void TurnOffTimer()
        {
            isTimerOn = false;
            timerImage.gameObject.SetActive(false);
            timerVal = Globals.maxTimeToPerformAction;
        }

        public void SetCoinsOnTableTMP(TextMeshProUGUI tmp)
        {
            coinsOnTableTMP = tmp;
        }

        public void SetIsPlayer(bool isPlayer)
        {
            this.isPlayer = isPlayer;
        }

        public void SetPlayerName()
        {
            playerName = NetworkManagement.NetworkManager.instance
                .roomManager.GetPlayerFromID(Profile.roomID).NickName;
            playerNameTMP.text = playerName.Truncate(12, "...");
        }

        public void SetValues(PlayerProfile profile)
        {
            Profile = profile;
            coinsTMP.text = Profile.coins.ToString("0.00");

            coinsOnTableTMP.transform.parent.gameObject.SetActive(profile.coinsOnTable > 0);

            coinsOnTableTMP.text = profile.coinsOnTable.ToString("0.00");
        }

        public void SetCards(List<int> cards, bool show = false)
        {
            this.cards = new List<int>();
            this.cards = cards;

            card1Image.sprite = show ? GameManager.instance.gameplayManager.cardDeck.GetCardSprite(cards[0])
                : cardBackSprite;
            card2Image.sprite = show ? GameManager.instance.gameplayManager.cardDeck.GetCardSprite(cards[1])
                : cardBackSprite;

            if (show && !card1Image.transform.parent.gameObject.activeSelf)
            {
                cardsAnimator.Play("Open", 0, 0);
            }

            card1Image.transform.parent.gameObject.SetActive(show);
            hiddenCardsPanel.SetActive(!show);
        }

        public void SetIsActive(bool isActive)
        {
            activeImg.SetActive(isActive);

            if (isActive)
            {
                isTimerOn = true;
                timerImage.gameObject.SetActive(true);
                timerVal = Globals.maxTimeToPerformAction;
            }
        }

        public void SetIsOn(bool isOn) 
        {
            Profile.isOn = isOn;

            foreach(var img in canvasGroupImages)
            {
                var col = img.color;
                col.a = isOn ? 1 : 0.55f;
                img.color = col;
            }
            foreach (var txt in canvasGroupTexts)
            {
                var col = txt.color;
                col.a = isOn ? 1 : 0.55f;
                txt.color = col;
            }
        }

        public void HideActionImage()
        {
            actionImage.gameObject.SetActive(false);
        }

        public void ShowActionImage(Globals.MoveType action)
        {
            string[] texts = new string[] { "Check", "Call", "Bet", "Raise", "Fold", "All-In" };

            actionImage.gameObject.SetActive(true);
            actionImage.sprite = actionSprites[(int)action - 1];
            actionImage.GetComponentInChildren<TextMeshProUGUI>().text = texts[(int)action - 1];
        }

        public void SetAsActive()
        {
            IsActive = true;

            StopCoroutine("InactiveTimer");
        }

        public void SetAsInactive()
        {
            IsActive = false;

            StartCoroutine("InactiveTimer");
        }
        #endregion

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
