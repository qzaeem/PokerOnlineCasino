using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using NetworkManagement;
using System.Linq;

namespace Gameplay
{
    public class GameNetwork : MonoBehaviourPun
    {
        #region Private Functions
        private void Awake()
        {
            PhotonNetwork.IsMessageQueueRunning = true;
        }
        #endregion

        #region Public Functions
        public void StartGameTimer()
        {
            photonView.RPC("BeginStartTimer", RpcTarget.AllViaServer);
        }

        public void SpawnPlayer(PlayerProfile playerProfile)
        {
            var dict = PlayerProfile.PlayerProfileToDictionary(playerProfile);
            this.photonView.RPC("SpawnPlayer", RpcTarget.All, dict);
        }

        public void SendSetGameID(string gameID)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            dict.Add(0, gameID);
            photonView.RPC("SetGameID", RpcTarget.All, dict);
        }

        public void GetNewPlayerReady(int id, List<PlayerProfile> playerProfiles)
        {
            Photon.Realtime.Player player = NetworkManager.instance.roomManager.GetPlayerFromID(id);

            if (player == null)
                return;

            playerProfiles.RemoveAll(p => p.roomID == id);
            
            Dictionary<byte, double> playerProfilesDict = PlayerProfile.PlayerProfilesToDictionary(playerProfiles);

            photonView.RPC("GetReadyForGame", player, playerProfilesDict);
        }

        public void SendUpdateBlind(int id, double bet)
        {
            var dict = new Dictionary<byte, double> { { 0, id }, { 1, bet} };

            photonView.RPC("UpdateBlind", RpcTarget.All, dict);
        }

        public void SendUpdatePlayersInHand(List<int> playersInHandIds, int targetID = -1)
        {
            Dictionary<byte, int> dict = new Dictionary<byte, int>();

            playersInHandIds.ForEach((int id) =>
            {
                dict.Add(System.Convert.ToByte(id), id);
            });

            if(targetID == -1)
                photonView.RPC("UpdatePlayersInHand", RpcTarget.Others, dict);
            else
                photonView.RPC("UpdatePlayersInHand", NetworkManager.instance.roomManager.GetPlayerFromID(targetID),
                    dict);
        }

        public void SendSetCards(int card1, int card2, int playerID, int targetID = -1)
        {
            var dict = new Dictionary<byte, int>();
            int key = 0;
            dict.Add(System.Convert.ToByte(key++), card1);
            dict.Add(System.Convert.ToByte(key++), card2);
            dict.Add(System.Convert.ToByte(key++), playerID);

            if(targetID < 0)
                photonView.RPC("SetCards", RpcTarget.All, dict);
            else
                photonView.RPC("SetCards", NetworkManager.instance.roomManager.GetPlayerFromID(targetID), dict);
        }

        public void SendSetCommunityCards(Dictionary<int, int> cards, int targetID = -1)
        {
            if (targetID < 0)
                photonView.RPC("SetCommunityCards", RpcTarget.All, cards);
            else
                photonView.RPC("SetCommunityCards", NetworkManager.instance.roomManager.GetPlayerFromID(targetID),
                    cards);
        }

        public void SendSetRound(int round, int targetID = -1)
        {
            if (targetID < 0)
                photonView.RPC("SetRound", RpcTarget.All, round);
            else
                photonView.RPC("SetRound", NetworkManager.instance.roomManager.GetPlayerFromID(targetID),
                    round);
        }

        public void SendSetActivePlayer(int id, int targetID = -1)
        {
            if (targetID < 0)
                photonView.RPC("SetActivePlayer", RpcTarget.All, id);
            else
                photonView.RPC("SetActivePlayer", NetworkManager.instance.roomManager.GetPlayerFromID(targetID),
                    id);
        }

        public void SendStartNextMove(PokerAction pokerAction)
        {
            var dict = PokerAction.PokerActionToDictionary(pokerAction);
            photonView.RPC("StartNextMove", RpcTarget.All, dict);
        }

        public void SendUpdatePot(Dictionary<int, double> coinsInPot, int targetID = -1)
        {
            if (targetID < 0)
                photonView.RPC("UpdatePot", RpcTarget.All, coinsInPot);
            else
                photonView.RPC("UpdatePot", NetworkManager.instance.roomManager.GetPlayerFromID(targetID),
                    coinsInPot);
        }

        public void SendSetIsOn(int id, bool val, int targetID = -1)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            dict.Add(0, id);
            dict.Add(1, val ? 1 : 0);
            if (targetID < 0)
                photonView.RPC("SetIsPlayerOn", RpcTarget.All, dict);
            else
                photonView.RPC("SetIsPlayerOn", NetworkManager.instance.roomManager.GetPlayerFromID(targetID),
                    dict);
        }

        public void SendPlayersPerformedAction(Dictionary<int, int> dict, int targetID = -1)
        {
            if (targetID < 0)
                photonView.RPC("SetPlayersPerformedAction", RpcTarget.All, dict);
            else
                photonView.RPC("SetPlayersPerformedAction",
                    NetworkManager.instance.roomManager.GetPlayerFromID(targetID),
                    dict);
        }

        public void SendTakeCoinsForTable(int id)
        {
            var player = NetworkManager.instance.roomManager.GetPlayerFromID(id);
            photonView.RPC("TakeCoinsForTable", player);
        }

        public void SendUpdateCoinsForHand(int id, double coins)
        {
            photonView.RPC("UpdateCoinsForHand", RpcTarget.All,
                new Dictionary<int, double> { { 0, id }, { 1, coins } });
        }

        public void SendShowdown()
        {
            photonView.RPC("ShowDown", RpcTarget.All);
        }

        public void SendSetIsPlaying(int id, bool val, int targetID = -1)
        {
            if(targetID < 0)
                photonView.RPC("SetIsPlaying", RpcTarget.All,
                    new Dictionary<int, int> { { 0, id}, { 1, val ? 1 : 0 } });
            else
                photonView.RPC("SetIsPlaying", NetworkManager.instance.roomManager.GetPlayerFromID(targetID),
                    new Dictionary<int, int> { { 0, id }, { 1, val ? 1 : 0 } });
        }

        public void SendNextPlayerNotFound()
        {
            photonView.RPC("NextPlayerNotFound", RpcTarget.All);
        }

        public void SendRemovePlayer(int id)
        {
            photonView.RPC("RemovePlayer", RpcTarget.All, id);
        }

        public void SendResetTableCoins(int targetID = -1)
        {
            if (targetID < 0)
                photonView.RPC("ResetTableCoins", RpcTarget.All);
            else
                photonView.RPC("ResetTableCoins", NetworkManager.instance.roomManager.GetPlayerFromID(targetID));
        }

        public void SendUpdatePlayerCoins(int id, double coins, double coinsOnTable)
        {
            var dict = new Dictionary<int, double>();
            dict.Add(0, id);
            dict.Add(1, coins);
            dict.Add(2, coinsOnTable);
            photonView.RPC("UpdatePlayerCoins", RpcTarget.All, dict);
        }

        public void SendPlayerPerformedAction(int id, int action)
        {
            var dict = new Dictionary<int, int>();
            dict.Add(id, action);
            photonView.RPC("PlayerPerformedAction", RpcTarget.All, dict);
        }
        #endregion

        #region RPCs

        [PunRPC]
        private void SetGameID(Dictionary<int, string> dict)
        {
            Globals.gameID = dict[0];
            Debug.Log("<color=red>Game ID: </color>" + Globals.gameID);
        }

        [PunRPC]
        private void PlayerPerformedAction(Dictionary<int, int> dict)
        {
            GameManager.instance.gameplayManager.PlayerPerformedAction(dict.ElementAt(0).Key,
                dict.ElementAt(0).Value);
        }

        [PunRPC]
        private void UpdatePlayerCoins(Dictionary<int, double> info)
        {
            int id = (int)info[0];
            double coins = info[1];
            double coinsOnTable = info[2];
            GameManager.instance.gameplayManager.UpdatePlayerCoins(id, coins, coinsOnTable);
        }

        [PunRPC]
        private void ResetTableCoins()
        {
            GameManager.instance.gameplayManager.ResetTableCoins();
        }

        [PunRPC]
        private void RemovePlayer(int id)
        {
            GameManager.instance.gameplayManager.RemovePlayer(id);
        }

        [PunRPC]
        private void NextPlayerNotFound()
        {
            GameManager.instance.gameplayManager.NextPlayerNotFound();
        }

        [PunRPC]
        private void SetIsPlaying(Dictionary<int, int> dict)
        {
            GameManager.instance.gameplayManager.SetIsPlaying(dict[0], dict[1] > 0 ? true : false);
        }

        [PunRPC]
        private void ShowDown()
        {
            GameManager.instance.gameplayManager.Showdown();
        }

        [PunRPC]
        private void UpdateCoinsForHand(Dictionary<int, double> dict)
        {
            GameManager.instance.gameplayManager.UpdateCoinsForHand((int)dict[0], dict[1]);
        }

        [PunRPC]
        private void TakeCoinsForTable()
        {
            GameManager.instance.gameplayManager.TakeCoinsForTable();
        }

        [PunRPC]
        private void SetPlayersPerformedAction(Dictionary<int, int> dict)
        {
            GameManager.instance.gameplayManager.SetPlayersPerformedAction(dict);
        }

        [PunRPC]
        private void SetIsPlayerOn(Dictionary<int, int> dict)
        {
            int id = dict[0];
            bool val = dict[1] > 0 ? true : false;
            GameManager.instance.gameplayManager.SetPlayerOn(id, val);
        }

        [PunRPC]
        private void UpdatePot(Dictionary<int, double> coinsInPot)
        {
            GameManager.instance.gameplayManager.UpdatePot(coinsInPot);
        }

        [PunRPC]
        private void StartNextMove(Dictionary<byte, double> dict)
        {
            var pokerAction = PokerAction.DictionaryToPlayerProfile(dict);
            GameManager.instance.gameplayManager.StartNextMove(pokerAction);
        }

        [PunRPC]
        private void SetActivePlayer(int id)
        {
            GameManager.instance.gameplayManager.SetActivePlayer(id);
        }

        [PunRPC]
        private void SetRound(int round)
        {
            GameManager.instance.gameplayManager.SetRound(round);
        }

        [PunRPC]
        private void SetCommunityCards(Dictionary<int, int> dict)
        {
            GameManager.instance.gameplayManager.SetCommunityCards(dict);
        }

        [PunRPC]
        private void SetCards(Dictionary<byte, int> dict)
        {
            var list = dict.Values.ToList();
            int card1 = list[0];
            int card2 = list[1];
            int playerID = list[2];

            GameManager.instance.gameplayManager.SetCards(card1, card2, playerID);
        }

        [PunRPC]
        private void UpdatePlayersInHand(Dictionary<byte, int> dict)
        {
            GameManager.instance.gameplayManager.UpdatePlayersInHand(dict.Values.ToList());
        }

        [PunRPC]
        private void UpdateBlind(Dictionary<byte, double> dict)
        {
            int id = (int)dict[0];
            double bet = dict[1];

            GameManager.instance.gameplayManager.UpdateBlind(id, bet);
        }

        [PunRPC]
        private void BeginStartTimer()
        {
            GameManager.instance.uI_Manager.StartGameTimer();
        }

        [PunRPC]
        private void GetReadyForGame(Dictionary<byte, double> playerProfiles)
        {
            var playerProfilesList = PlayerProfile.DictionaryToPlayerProfiles(playerProfiles);
            playerProfilesList.ForEach((PlayerProfile profile) =>
            {
                GameManager.instance.gameplayManager.SpawnPlayer(profile);
            });
        }

        [PunRPC]
        private void SpawnPlayer(Dictionary<byte, double> dict)
        {
            var playerProfile = PlayerProfile.DictionaryToPlayerProfile(dict);
            GameManager.instance.gameplayManager.SpawnAndGetReady(playerProfile);
        }
        #endregion
    }
}
