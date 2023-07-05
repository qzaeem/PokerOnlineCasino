using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace NetworkManagement
{
    public class NetworkUI : MonoBehaviour
    {
        [SerializeField] private GameObject connectionPanel, lobbyPanel;
        [SerializeField] private RoomButton[] roomButtons;
        [SerializeField] private Transform roomListContainer;
        [SerializeField] private TMP_InputField inputFieldTMP;
        [SerializeField] private TextMeshProUGUI userNameTMP;
        [SerializeField] private Notification notificationPefab;

        public string Nickname { get { return Globals.userInfo.username; } }

        //==================================================<REGION>========================================================
        #region Private Functions

        private void Start()
        {
            for(int i = 0; i < roomButtons.Length; i++)
            {
                RoomData roomInfo = new RoomData();
                roomInfo.mode = (byte)(i + 1);
                roomInfo.maxPlayers = Globals.maxPlayersInRoom;

                roomButtons[i].Init(roomInfo);
            }
        }   


        private void DisableAllPanels()
        {
            connectionPanel.SetActive(false);
            lobbyPanel.SetActive(false);
            LoadingCanvas.instance.ShowLoading(false);
        }

        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
        //==================================================<REGION>========================================================
        #region Public Functions

        public void ShowConnectionScreen()
        {
            gameObject.SetActive(true);

            DisableAllPanels();
            connectionPanel.SetActive(true);
        }

        public void ShowLobby(string userName)
        {
            userNameTMP.text = Globals.userInfo.first_name + " " + Globals.userInfo.last_name;

            gameObject.SetActive(true);

            DisableAllPanels();
            lobbyPanel.SetActive(true);
        }

        public void HideNetworkUI()
        {
            DisableAllPanels();

            gameObject.SetActive(false);
        }

        public void ShowLoadingScreen(string message = "")
        {
            gameObject.SetActive(true);

            if (!LoadingCanvas.instance.IsLoading)  DisableAllPanels();
            LoadingCanvas.instance.ShowLoading(true, message);
        }

        public void ShowNotification(string message, string buttonText, System.Action callback = null)
        {
            var notification = Instantiate(notificationPefab, transform);
            notification.ShowNotification(message: message, buttonText: buttonText);
        }

        public void ConnectToMaster()
        {
            NetworkManager.instance.ConnectToMaster();
        }

        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
    }
}
