using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace NetworkManagement
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        private List<RoomInfo> cachedRoomList;
        //private List<PlayerProfile> cachedProfiles;
        private int currentMode;
        private bool isInRoom;

        public Room lastJoinedRoom;

        public int PlayersCount { get { return PhotonNetwork.CurrentRoom.PlayerCount; } }
        public int ActorNumber { get { return PhotonNetwork.LocalPlayer.ActorNumber; } }
        public int CurrentMode { get { return currentMode; } }
        public bool IsMaster { get { return PhotonNetwork.IsMasterClient; } }
        public bool IsInRoom { get { return isInRoom; } }

        //==================================================<REGION>========================================================
        #region Private Functions

        private void Start()
        {
            //CustomStream.Register();
        }

        private void SwitchRoom()
        {
            var roomsList = cachedRoomList.FindAll(roomInfo =>
            roomInfo.Name != lastJoinedRoom.Name
            && System.Convert.ToByte(roomInfo.CustomProperties[Constants.GAME_MODE])
                    == System.Convert.ToByte(lastJoinedRoom.CustomProperties[Constants.GAME_MODE]));

            if (roomsList.Count > 0)
            {
                PhotonNetwork.JoinRoom(roomsList[new System.Random().Next(roomsList.Count)].Name);
            }
            else
            {
                CreateNewRoomToSwitch();
            }
        }

        private void CreateNewRoomToSwitch()
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = lastJoinedRoom.MaxPlayers;
            roomOptions.EmptyRoomTtl = Globals.roomTTL;
            roomOptions.PlayerTtl = Globals.playerTTL;
            roomOptions.CustomRoomPropertiesForLobby = lastJoinedRoom.PropertiesListedInLobby;
            roomOptions.CustomRoomProperties =
                new ExitGames.Client.Photon.Hashtable
                { { Constants.GAME_MODE,
                        System.Convert.ToByte(lastJoinedRoom.CustomProperties[Constants.GAME_MODE]) } };
            PhotonNetwork.CreateRoom(null, roomOptions);
        }

        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
        //==================================================<REGION>========================================================
        #region Public Functions

        public Room GetCurrentRoom() => PhotonNetwork.CurrentRoom;
        public Player[] GetAllPlayersInCurrentRoom() => PhotonNetwork.PlayerList;

        public void SetRoomForNewPlayers(bool isOpen)
        {
            PhotonNetwork.CurrentRoom.IsOpen = isOpen;
        }

        public Player GetPlayerFromID(int id)
        {
            var players = GetAllPlayersInCurrentRoom();
            Player player = null;

            foreach (Player plr in players)
            {
                if (plr.ActorNumber == id)
                {
                    player = plr;
                    break;
                }
            }

            return player;
        }

        public byte GetSlotsOnTable()
        {
            var roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

            if (!roomProperties.ContainsKey(Constants.SLOTS_KEY))
            {
                roomProperties = new ExitGames.Client.Photon.Hashtable { { Constants.GAME_MODE,
                        System.Convert.ToByte(lastJoinedRoom.CustomProperties[Constants.GAME_MODE]) },
                        { Constants.SLOTS_KEY, 0 }};

                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
            }

            return System.Convert.ToByte(roomProperties[Constants.SLOTS_KEY]);
        }

        public void UpdateSlotOnTable(int index, bool val)
        {
            var roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

            var bitArray = BinaryByteConversion
                .ConvertByteToBoolArray(System.Convert.ToByte(roomProperties[Constants.SLOTS_KEY]));

            bitArray[index] = val;

            roomProperties = new ExitGames.Client.Photon.Hashtable { { Constants.GAME_MODE,
                        System.Convert.ToByte(lastJoinedRoom.CustomProperties[Constants.GAME_MODE]) },
                        { Constants.SLOTS_KEY, BinaryByteConversion.ConvertBoolArrayToByte(bitArray) }};

            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }

        public void JoinOrCreateRandomRoom(RoomData roomInfo)
        {
            isInRoom = false;

            currentMode = roomInfo.mode;

            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = roomInfo.maxPlayers;
            roomOptions.EmptyRoomTtl = Globals.roomTTL;
            roomOptions.PlayerTtl = Globals.playerTTL;
            var roomProperties = new ExitGames.Client.Photon.Hashtable { { Constants.GAME_MODE, roomInfo.mode } };
            roomOptions.CustomRoomProperties = roomProperties;
            roomOptions.CustomRoomPropertiesForLobby = new string[] { Constants.GAME_MODE }; 
            PhotonNetwork.JoinRandomOrCreateRoom(
                roomProperties, roomInfo.maxPlayers,
                MatchmakingMode.FillRoom, null, null, null, roomOptions);
        }

        public void LeaveRoom()
        {
            if(PlayersCount == 1)
            {
                PhotonNetwork.CurrentRoom.EmptyRoomTtl = 0;
            }

            PhotonNetwork.LeaveRoom(false);
            isInRoom = false;
        }

        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
        //==================================================<REGION>========================================================
        #region Callbacks

        public override void OnCreatedRoom()
        {
            print("Room Created!");
        }

        public override void OnJoinedRoom()
        {
            if (isInRoom)
            {
                NetworkManager.instance.HideNetworkUI();

                var gameManager = Gameplay.GameManager.instance;

                if (!gameManager)
                    return;

                gameManager.gameplayManager.ResumePlayerActive(PhotonNetwork.LocalPlayer.ActorNumber);

                return;
            }

            isInRoom = true;

            lastJoinedRoom = PhotonNetwork.CurrentRoom;
            print("Joined room: " + lastJoinedRoom.Name);
            NetworkManager.instance.OnJoinedRoom(lastJoinedRoom);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            isInRoom = false;
            Debug.LogError(message);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            isInRoom = false;
            Debug.LogError(message);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            isInRoom = false;
            Debug.LogError(message);
        }

        public override void OnPlayerEnteredRoom(Player otherPlayer)
        {
            print("Other Player has entered!");

            var gameManager = Gameplay.GameManager.instance;

            if (!gameManager)
                return;

            gameManager.gameplayManager.ResumePlayerActive(otherPlayer.ActorNumber);
        }

        public override void OnLeftRoom()
        {
            print("I Left Room!");
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            print("Other player left!");

            var gameManager = Gameplay.GameManager.instance;

            if (!gameManager)
                return;

            gameManager.gameplayManager.SetPlayerInactive(otherPlayer.ActorNumber);
            //gameManager.gameplayManager.RemovePlayer(otherPlayer.ActorNumber);
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            cachedRoomList = roomList;

            if (Globals.isSwitching)
            {
                Globals.isSwitching = false;
                SwitchRoom();
            }
        }

        #endregion
        //==================================================<END_REGION>========================================================
        //======================================================================================================================
        //======================================================================================================================
    }
}


