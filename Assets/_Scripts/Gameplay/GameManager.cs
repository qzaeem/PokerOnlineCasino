using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkManagement;
using UnityEngine.SceneManagement;

namespace Gameplay
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        public GameplayManager gameplayManager;

        public UI_Manager uI_Manager;

        //==================================================<REGION>========================================================
        #region Private Functions
        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            Observer.RegisterCustomEvent(Constants.PLAYER_LEFT_EVENT, PlayerLeftRoom);
        }

        private void OnDisable()
        {
            Observer.RemoveCustomEvent(Constants.PLAYER_LEFT_EVENT, PlayerLeftRoom);
        }

        private void Start()
        {
            PlayerEnteredRoom();

            NetworkManager.instance.HideNetworkUI();
            uI_Manager.InitRoomInfo(NetworkManager.instance.roomManager.lastJoinedRoom);
        }

        private void PlayerEnteredRoom()
        {
            byte slots = NetworkManager.instance.roomManager.GetSlotsOnTable();
            gameplayManager.InitializePlayer(slots);
        }

        private void PlayerLeftRoom()
        {

        }

        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
        //==================================================<REGION>========================================================
        #region Public Functions

        public void SwitchRoom()
        {
            gameplayManager.PlayerLeaving();

            if (!NetworkManager.instance)
                return;

            NetworkManager.instance.SwitchRoom();
        }

        public void LeaveRoom()
        {
            gameplayManager.PlayerLeaving();

            if (!NetworkManager.instance)
                return;

            NetworkManager.instance.LeaveRoom();
            SceneManager.LoadSceneAsync(Constants.MAIN_SCENE_NAME);
        }

        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
        //==================================================<REGION>========================================================
    }
}
