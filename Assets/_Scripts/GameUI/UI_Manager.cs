using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Gameplay
{
    public class UI_Manager : MonoBehaviour
    {
        public GameUI gameUI;

        [SerializeField] private RectTransform uiCanvasRect;
        [SerializeField] private Menu gameMenu;
        [SerializeField] private TextMeshProUGUI roomGUID_TMP, modeTMP, maxPlayersTMP, currentPlayersTMP,
            timerTMP;
        [SerializeField] private Button spectateButton;
        [SerializeField] private GameObject timerPanel, winnerPanel;
        [SerializeField] private Rules rulesPrefab;

        private Rules rulesPanel;
        private bool isTimerRunning = false;

        //==================================================<REGION>========================================================
        #region Private Functions

        private void Start()
        {
            winnerPanel.SetActive(false);

            rulesPanel = Instantiate(rulesPrefab, uiCanvasRect);
        }

        private IEnumerator StartGameAfterTimer()
        {
            float timerVal = Globals.timeToStartGame;

            while(timerVal > 0)
            {
                timerTMP.text = timerVal.ToString("00");

                timerVal -= Time.deltaTime;

                yield return null;
            }

            timerTMP.text = "00";
            isTimerRunning = false;
            timerPanel.SetActive(false);
            GameManager.instance.gameplayManager.StartHand();
        }

        IEnumerator ShowWinnerPanel()
        {
            yield return new WaitForSeconds(1);

            winnerPanel.SetActive(true);

            yield return new WaitForSeconds(2);

            winnerPanel.SetActive(false);
        }

        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
        //==================================================<REGION>========================================================
        #region Public Functions

        public void ActivateWinnerPanel()
        {
            if (!gameObject.activeInHierarchy) return;

            StartCoroutine("ShowWinnerPanel");
        }

        public void StartGameTimer()
        {
            if (isTimerRunning)
                return;

            isTimerRunning = true;
            timerPanel.SetActive(true);
            StartCoroutine(StartGameAfterTimer());
        }

        public void DeclareWinner(int playerID, string hand = "")
        {
            timerPanel.SetActive(true);
            timerTMP.text = "Player " + playerID + " Wins" + "\n" + hand.ToString();
            StartCoroutine(CloseWinPanel());
        }

        IEnumerator CloseWinPanel()
        {
            yield return new WaitForSeconds(3);
            timerPanel.SetActive(false);
        }

        public void InitRoomInfo(Photon.Realtime.Room roomInfo)
        {
            roomGUID_TMP.text = roomInfo.Name;
            modeTMP.text = System.Convert.ToByte(roomInfo.CustomProperties[Constants.GAME_MODE]).ToString();
            maxPlayersTMP.text = roomInfo.MaxPlayers.ToString();
            currentPlayersTMP.text = roomInfo.PlayerCount.ToString();
        }

        public void StartSpectateMode()
        {
            SetSpectateButton(false);
            GameManager.instance.gameplayManager.StartSpectating();
            gameMenu.CloseMenu();
        }

        public void SetSpectateButton(bool val)
        {
            spectateButton.interactable = val;
        }

        public void UpdatePlayersInRoom(string playerCount)
        {
            currentPlayersTMP.text = playerCount;
        }

        public void SwitchRoom()
        {
            GameManager.instance.SwitchRoom();
            gameMenu.CloseMenu();
        }

        public void LeaveRoom()
        {
            GameManager.instance.LeaveRoom();
            gameMenu.CloseMenu();
        }

        public void OpenMenu()
        {
            gameMenu.OpenMenu();
        }

        public void CloseMenu()
        {
            gameMenu.CloseMenu();
        }

        public void OpenRules()
        {
            rulesPanel.ShowRules();
        }
        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
        //==================================================<REGION>========================================================
    }
}
