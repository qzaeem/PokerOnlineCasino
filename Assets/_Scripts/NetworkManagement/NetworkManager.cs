using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using UnityEngine;

namespace NetworkManagement
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance;

        public ConnectionManager connectionManager;
        public RoomManager roomManager;
        public NetworkUI networkUI;

        //==================================================<REGION>========================================================
        #region Private Functions

        private void Awake()
        {
            if (instance)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            connectionManager = GetComponentInChildren<ConnectionManager>();
            roomManager = GetComponentInChildren<RoomManager>();
            networkUI = GetComponentInChildren<NetworkUI>();
        }

        private void Start()
        {
            if (connectionManager.IsConnectedToMaster)
            {
                networkUI.ShowLobby(connectionManager.Nickname);
            }
            else
            {
                networkUI.ShowLoadingScreen();
                if (Globals.isTesting) ConnectToMaster("Plr" + Random.Range(1, 51));
                else StartCoroutine(WaitForSocketsToConnect());
            }
        }

        private IEnumerator WaitForSocketsToConnect()
        {
            yield return new WaitUntil(() => Globals.isSocketsConnected);
            ConnectToMaster();
        }

        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
        //==================================================<REGION>========================================================
        #region Public Functions

        public void ReEstablishConnection()
        {
            connectionManager.CancelReconnection();
            // Reconnect to server and then photon
            if (Globals.isTesting) ConnectToMaster("Plr" + Random.Range(1, 51));
            else StartCoroutine(WaitForSocketsToConnect());
        }

        public void HideNetworkUI()
        {
            networkUI.HideNetworkUI();
        }

        public void ConnectToMaster(string nickName = "")
        {
            connectionManager.Connect(nickName == "" ? networkUI.Nickname : nickName);
            //connectionManager.Connect(GameData.instance.PlayerName);  
            networkUI.ShowLoadingScreen();
        }

        public void JoinRoom(RoomData roomInfo)
        {
            double requiredAmount = Globals.GetRequiredAmountForTable(roomInfo.mode);

            if(requiredAmount > Globals.userInfo.chips)
            {
                networkUI.ShowNotification("You do not have enough cash to join this table", "OK");
                return;
            }

            Globals.isSwitching = false;

            roomManager.JoinOrCreateRandomRoom(roomInfo);
            networkUI.ShowLoadingScreen();
        }

        public void SwitchRoom()
        {
            Globals.isSwitching = true;

            networkUI.ShowLoadingScreen();
            roomManager.LeaveRoom();
        }

        public void LeaveRoom()
        {
            networkUI.ShowLoadingScreen();
            roomManager.LeaveRoom();
        }

        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
        //==================================================<REGION>========================================================
        #region Callbacks

        public void OnConnectedToMaster(string userName)
        {
            if (Globals.isSwitching)
                return;

            networkUI.ShowLobby(userName);
        }

        public void OnDisconnectedFromMaster()
        {

        }

        public void OnJoinedRoom(RoomInfo roomInfo)
        {
            PhotonNetwork.IsMessageQueueRunning = false;
            SceneManager.LoadSceneAsync(Constants.GAME_SCENE_NAME);
        }

        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
    }
}
