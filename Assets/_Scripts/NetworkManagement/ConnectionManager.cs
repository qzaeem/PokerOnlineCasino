using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace NetworkManagement
{
    public class ConnectionManager : MonoBehaviourPunCallbacks
    {
        public bool IsConnectedToMaster { get { return PhotonNetwork.IsConnected; } }
        public string Nickname { get { return PhotonNetwork.NickName; } }

        //==================================================<REGION>========================================================
        #region Private Functions

        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = false;
        }

        private void Reconnect()
        {
            bool isReconnecting = PhotonNetwork.Reconnect();

            if (isReconnecting)
            {

            }
            else
            {
                Invoke("Reconnect", 0.5f);
            }
        }

        private void ReconnectAndRejoin()
        {
            bool isReconnecting = PhotonNetwork.ReconnectAndRejoin();

            if (isReconnecting)
            {

            }
            else
            {
                Invoke("ReconnectAndRejoin", 0.5f);
            }
        }

        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
        //==================================================<REGION>========================================================
        #region Public Functions

        public void Connect(string nickeName)
        {
            if (PhotonNetwork.IsConnected)
            {
                
            }
            else
            {
                PhotonNetwork.NickName = nickeName;
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = "1"; // TODO
            }
        }

        public void CancelReconnection()
        {
            CancelInvoke("Reconnect");
            CancelInvoke("ReconnectAndRejoin");
        }

        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
        //==================================================<REGION>========================================================
        #region Callbacks

        public override void OnConnectedToMaster()
        {
            print("Connected to Master!");
            PhotonNetwork.JoinLobby();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            print("Disconnected!");

            if (NetworkManager.instance.roomManager.IsInRoom)
            {
                Invoke("ReconnectAndRejoin", 0.5f);
            }
            else
            {
                Invoke("Reconnect", 0.5f);
            }

            NetworkManager.instance.networkUI.ShowLoadingScreen("Reconnecting");

            var gameManager = Gameplay.GameManager.instance;

            if (!gameManager)
                return;

            gameManager.gameplayManager.SetPlayerInactive(PhotonNetwork.LocalPlayer.ActorNumber);
        }

        public override void OnJoinedLobby()
        {
            NetworkManager.instance.OnConnectedToMaster(PhotonNetwork.NickName);
        }

        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
    }
}

