using System.Collections;
using System.Collections.Generic;
using NetworkManagement;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;

namespace Gameplay
{
    public class GameplayManager : MonoBehaviour
    {
        public CardDeckSO cardDeck;

        [SerializeField] private RectTransform gameCanvasRect;
        [SerializeField] private GameNetwork gameNetwork;
        [SerializeField] private Player playerPrefab;
        [SerializeField] private Transform[] slotTransforms;
        [SerializeField] private TextMeshProUGUI[] slotCoinsOnTableTMPs;
        [SerializeField] private GameObject cardsParent, potPrefab, chipPrefab;
        [SerializeField] private Transform potPanelTransform, chipsPanelTransform;
        [SerializeField] private Image[] cardSpriteRenderers;
        [SerializeField] private TextMeshProUGUI resultTMP;

        private Dictionary<int, Player> playersOnTable = new Dictionary<int, Player>();
        private Dictionary<int, Player> playersInHand = new Dictionary<int, Player>();
        private Dictionary<int, int> playersPerformedAction = new Dictionary<int, int>();
        private Dictionary<int, bool> communityCards;
        private Dictionary<int, double> potWithShares = new Dictionary<int, double>();
        private Dictionary<int, PotData> pots = new Dictionary<int, PotData>();
        private Player thisPlayer;
        private PokerAction previousAction;
        private Globals.Round round;
        private int firstPlayerSlotInLastHand;
        private int myActorNumber;
        private double totalCoinsWon;
        private bool selfSpawned = false, gameStarted = false;

        public int RoomID { get { return thisPlayer.Profile.roomID; } }

        #region Private Functions

        private void Awake()
        {
            foreach (var tmp in slotCoinsOnTableTMPs) tmp.transform.parent.gameObject.SetActive(false);
            foreach (var card in cardSpriteRenderers) card.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            Globals.getCoinsEvent += UpdateTotalCoins;
            Globals.createGameEvent += SendSetGameID;
        }
        private void OnDisable()
        {
            Globals.getCoinsEvent -= UpdateTotalCoins;
            Globals.createGameEvent -= SendSetGameID;
        }

        private void SendSetGameID(string gameID)
        {
            gameNetwork.StartGameTimer();
            gameNetwork.SendSetGameID(gameID);
        }

        private void CheckPlayersOnTable()
        {
            if (!NetworkManager.instance.roomManager.IsMaster) return;

            NetworkManager.instance.roomManager.SetRoomForNewPlayers(playersOnTable.Count < Globals.maxPlayersOnTable);
        }

        private void UpdateTotalCoins(double totalCoins)
        {
            GameManager.instance.uI_Manager.gameUI.UpdateTotalCoins();
        }

        private void AddCoinsToPot(double coins, int id)
        {
            if(pots.Count == 0)
            {
                PotData potData = new PotData();
                var go = Instantiate(potPrefab, potPanelTransform);
                potData.go = go;
                potData.coinsInPot = 0;
                potData.tmp = go.GetComponentInChildren<TextMeshProUGUI>();
                potData.tmp.text = potData.coinsInPot.ToString("0.00");
                potData.playersInPot = new List<int>();

                pots.Add(pots.Count, potData);
            }
            potWithShares.Add(id, coins);
            pots[0].coinsInPot = potWithShares.Values.Sum();
            pots[0].tmp.text = pots[0].coinsInPot.ToString("0.00");
        }

        private void RetractCoins()
        {
            thisPlayer.Profile.coins = 0;
            GameManager.instance.uI_Manager.gameUI.UpdateTotalCoins();
        }

        private List<int> GetCardsToDeal(int numberOfCards, List<int> deck)
        {
            List<int> result = new List<int>();

            while(result.Count < numberOfCards)
            {
                int card = deck[Random.Range(0, deck.Count)];

                result.Add(card);
                deck.Remove(card);
            }

            return result;
        }

        private void TakePositionAtTheTable(int index)
        {
            GameData.instance.playerProfile.slot = index;
            GameData.instance.playerProfile.isOn = false;
            NetworkManager.instance.roomManager.UpdateSlotOnTable(index, true);
            gameNetwork.SpawnPlayer(GameData.instance.playerProfile);
        }

        private void GetNewPlayerReady(PlayerProfile playerProfile)
        {
            if (!NetworkManager.instance.roomManager.IsMaster || playerProfile.roomID == myActorNumber)
                return;
            
            var playerProfiles = playersOnTable.Values.Select(p => p.Profile).ToList();

            gameNetwork.GetNewPlayerReady(playerProfile.roomID, playerProfiles);

            if(playersOnTable.Count > 1 && !gameStarted)
            {
                if (!Globals.isTesting)
                    GetBrowserData.instance.webConnectivety
                        .CreateGame(Globals.modeNames[NetworkManager.instance.roomManager.CurrentMode - 1]);
            }

            if (playersInHand.Count == 0)
                return;

            gameNetwork.SendUpdatePlayersInHand(playersInHand.Values.Select(p => p.Profile.roomID).ToList(),
                playerProfile.roomID);

            foreach (Player player in playersInHand.Values.ToList())
            {
                gameNetwork.SendSetIsOn(player.Profile.roomID, player.Profile.isOn, playerProfile.roomID);

                gameNetwork.SendSetIsPlaying(player.Profile.roomID, player.isPlaying, playerProfile.roomID);

                gameNetwork.SendSetCards(player.cards[0], player.cards[1], player.Profile.roomID,
                    playerProfile.roomID);
            }

            var commCards = new Dictionary<int, int>();
            foreach (KeyValuePair<int, bool> card in communityCards) commCards.Add(card.Key,
                card.Value ? 1 : 0);
            gameNetwork.SendSetCommunityCards(commCards, playerProfile.roomID);

            gameNetwork.SendSetRound((int)round, playerProfile.roomID);
            gameNetwork.SendPlayersPerformedAction(playersPerformedAction, playerProfile.roomID);
            gameNetwork.SendUpdatePot(potWithShares, playerProfile.roomID);
        }

        private int GetNextActiveSlotInHand(int slot, bool firstTime = false)
        {
            int nextSlot = slot + 1 >= Globals.maxPlayersOnTable ? 0 : slot + 1;

            for (int i = nextSlot; i < Globals.maxPlayersOnTable; i++)
            {
                if (i == slot)
                {
                    return -1;
                }

                bool isActive = playersInHand.Values.ToList().Any(p =>
                p.Profile.slot == i && (p.Profile.isOn || firstTime));
                if (isActive)
                    return i;

                if (i + 1 >= Globals.maxPlayersOnTable)
                    i = -1;
            }

            return -1;
        }

        private int GetNextSlotInHand(int slot)
        {
            int nextSlot = slot + 1 >= Globals.maxPlayersOnTable ? 0 : slot + 1;

            for (int i = nextSlot; i < Globals.maxPlayersOnTable; i++)
            {
                if (i == slot)
                {
                    return -1;
                }

                bool isActive = playersInHand.Values.ToList().Any(p => p.Profile.slot == i);
                if (isActive)
                    return i;

                if (i + 1 >= Globals.maxPlayersOnTable)
                    i = -1;
            }

            return -1;
        }

        private void CheckForNoActiveSlot() // In case GetNextActiveSlotInHand returns -1
        {
            var players = playersInHand.Values.Where(p => p.isPlaying).ToList();
            GameManager.instance.uI_Manager.gameUI.DisableAllControls();
            if (players != null)
            {
                StopAllCoroutines();
                if (players.Count == 1)
                {
                    StartCoroutine(DirectlyDeclareWinner(players[0].Profile.roomID));
                    return;
                }
                else if(players.Count > 1)
                {
                    StartCoroutine(ChangeRound(true));
                    return;
                }
            }

            StopAllCoroutines();
            ResetValues();
            gameStarted = false;
        }

        private void UpdateAddButtons()
        {
            if (playersOnTable.Keys.Contains(GameData.instance.playerProfile.slot))
            {
                GameManager.instance.uI_Manager.gameUI.DisableAllAddButtons();
                return;
            }

            for (int i = 0; i < Globals.maxPlayersOnTable; i++)
            {
                GameManager.instance.uI_Manager.gameUI.ShowAddToSlotButton(i, !playersOnTable.Keys.Contains(i));
            }
        }

        private void UpdateMoveValues()
        {
            PlayerProfile playerProfile = new PlayerProfile();
            playerProfile.roomID = previousAction.playerID;
            playerProfile.coins = previousAction.playerCoins;
            playerProfile.coinsOnTable = previousAction.coinsOnTable;
            playerProfile.slot = previousAction.playerSlot;

            var playerToUpdate = playersInHand.Values.FirstOrDefault(p => p.Profile.roomID == playerProfile.roomID);
            playerProfile.isOn = playerToUpdate.Profile.isOn;
            if (playerToUpdate) playerToUpdate.SetValues(playerProfile);

            CalculatePots();
        }

        private void CalculatePots()
        {
            foreach(var pot in pots)
            {
                Destroy(pot.Value.go);
            }
            pots.Clear();

            var allIns = playersInHand.Values.Where(p => p.isPlaying && !p.Profile.isOn)
                .Select(p => p.Profile.roomID).ToList();
            potWithShares = potWithShares.OrderBy(p => p.Value).ToDictionary(x => x.Key, y => y.Value);
            var shares = new Dictionary<int, double>();
            foreach (var share in potWithShares) shares.Add(share.Key, share.Value);
            double potThresholdReached = 0;

            foreach(var shareInPot in potWithShares)
            {
                if (!allIns.Contains(shareInPot.Key) || potWithShares.Values.Max() == shareInPot.Value
                    || shares[shareInPot.Key] == 0) continue;

                double currentThreshold = shareInPot.Value - potThresholdReached;
                PotData potData = new PotData();
                potData.go = Instantiate(potPrefab, potPanelTransform);
                potData.tmp = potData.go.GetComponentInChildren<TextMeshProUGUI>();
                potData.playersInPot = new List<int>();
                potData.coinsInPot = 0;

                foreach(var share in potWithShares)
                {
                    if (shares[share.Key] == 0) continue;

                    double amount = shares[share.Key] >= currentThreshold ? currentThreshold : shares[share.Key];
                    shares[share.Key] = shares[share.Key] - amount;
                    potData.coinsInPot += amount;
                    potData.playersInPot.Add(share.Key);
                }

                potData.tmp.text = potData.coinsInPot.ToString("0.00");
                pots.Add(pots.Count, potData);
                potThresholdReached += currentThreshold;
            }

            if (shares.Values.Sum() == 0) return;

            PotData potData1 = new PotData();
            potData1.go = Instantiate(potPrefab, potPanelTransform);
            potData1.tmp = potData1.go.GetComponentInChildren<TextMeshProUGUI>();
            potData1.playersInPot = new List<int>();
            potData1.coinsInPot = 0;

            foreach (var share in potWithShares)
            {
                if (shares[share.Key] == 0) continue;

                double amount = shares[share.Key];
                shares[share.Key] = shares[share.Key] - amount;
                potData1.coinsInPot += amount;
                potData1.playersInPot.Add(share.Key);
            }
            potData1.tmp.text = potData1.coinsInPot.ToString("0.00");
            pots.Add(pots.Count, potData1);
        }

        private bool CheckRound()
        {
            if (playersInHand.Count == 0)
                return false;

            var idList = playersPerformedAction.Values.ToList();
            var activePlayersInHand = playersInHand.Values.Where(p => p.isPlaying).ToList();
            foreach (var player in activePlayersInHand)
            {
                if (!idList.Contains(player.Profile.roomID))
                {
                    return false;
                }
            }

            double amountOnTable = previousAction.coinsOnTable;
            foreach(Player player in activePlayersInHand)
            {
                double playerCoinsOnTable = player.Profile.roomID == previousAction.playerID
                    ? previousAction.coinsOnTable : player.Profile.coinsOnTable;

                if (playerCoinsOnTable != amountOnTable)
                {
                    return false;
                }
            }

            UpdateMoveValues();

            StartCoroutine(ChangeRound());
            return true;
        }

        private IEnumerator ChangeRound(bool dontReturnToAction = false)
        {
            if (NetworkManager.instance.roomManager.IsMaster)
            {
                var newRound = (Globals.Round)((int)round + 1);
                gameNetwork.SendSetRound((int)newRound);

                playersPerformedAction.Clear();
                gameNetwork.SendPlayersPerformedAction(playersPerformedAction);
                gameNetwork.SendResetTableCoins();
            }

            yield return new WaitForSeconds(2);
            if (NetworkManager.instance.roomManager.IsMaster)
            {
                var commCards = new Dictionary<int, int>();

                if (round == Globals.Round.Flop)
                {
                    communityCards[communityCards.ElementAt(0).Key] = true;
                    communityCards[communityCards.ElementAt(1).Key] = true;
                    communityCards[communityCards.ElementAt(2).Key] = true;
                }
                else if (round == Globals.Round.Turn)
                {
                    communityCards[communityCards.ElementAt(0).Key] = true;
                    communityCards[communityCards.ElementAt(1).Key] = true;
                    communityCards[communityCards.ElementAt(2).Key] = true;
                    communityCards[communityCards.ElementAt(3).Key] = true;
                }
                else if (round == Globals.Round.River)
                {
                    communityCards[communityCards.ElementAt(0).Key] = true;
                    communityCards[communityCards.ElementAt(1).Key] = true;
                    communityCards[communityCards.ElementAt(2).Key] = true;
                    communityCards[communityCards.ElementAt(3).Key] = true;
                    communityCards[communityCards.ElementAt(4).Key] = true;
                }

                foreach (KeyValuePair<int, bool> kvp in communityCards)
                {
                    commCards.Add(kvp.Key, kvp.Value ? 1 : 0);
                }

                gameNetwork.SendSetCommunityCards(commCards);

                if (round == Globals.Round.River)
                {
                    round = Globals.Round.Showdown;
                    gameNetwork.SendSetRound((int)round);
                    gameNetwork.SendShowdown();
                    yield break;
                }
            }

            yield return new WaitForSeconds(2);

            if (NetworkManager.instance.roomManager.IsMaster)
            {
                if (dontReturnToAction)
                {
                    CheckForNoActiveSlot();
                    yield break;
                }

                PokerAction pokerAction = previousAction;
                var nextPlayer = playersInHand.Values.FirstOrDefault(p => p.Profile.slot == firstPlayerSlotInLastHand);
                if (nextPlayer == null)
                {
                    int slot = GetNextActiveSlotInHand(firstPlayerSlotInLastHand);
                    if (slot == -1)
                    {
                        CheckForNoActiveSlot(); // TODO Complete this logic
                        yield break;
                    }
                    nextPlayer = playersInHand.Values.FirstOrDefault(p => p.Profile.slot == slot);
                }
                pokerAction.nextPlayerID = nextPlayer.Profile.roomID;
                pokerAction.coinsOnTable = 0;
                pokerAction.moveType = (int)Globals.MoveType.None;

                gameNetwork.SendStartNextMove(pokerAction);
            }
        }

        private IEnumerator Ending()
        {
            yield return new WaitForSeconds(2);
            CalculateWinner();
            ResetValues();

            yield return new WaitForSeconds(2);
            CheckCoinsForTable();

            yield return new WaitForSeconds(5);

            if (NetworkManager.instance.roomManager.IsMaster)
            {
                foreach (Player player in playersOnTable.Values)
                {
                    gameNetwork.SendTakeCoinsForTable(player.Profile.roomID);
                }

                if (playersOnTable.Count > 1)
                {
                    if (!Globals.isTesting)
                        GetBrowserData.instance.webConnectivety
                            .CreateGame(Globals.modeNames[NetworkManager.instance.roomManager.CurrentMode - 1]);
                }
                else gameStarted = false;
            }
        }

        private void CheckCoinsForTable()
        {
            double requiredCoins = Globals.GetRequiredAmountForTable(NetworkManager.instance.roomManager.CurrentMode);

            double coinsToGet = requiredCoins - thisPlayer.Profile.coins;
            bool hasEnoughCoins = Globals.userInfo.chips >= coinsToGet;

            if (!hasEnoughCoins)
            {
                GameManager.instance.uI_Manager.SetSpectateButton(false);
                StartSpectating();
            }
        }

        private void ResetValues()
        {
            playersInHand.Clear();
            playersPerformedAction.Clear();
            potWithShares.Clear();
            foreach (var player in playersOnTable.Values)
            {
                player.SetIsOn(false);
                player.isPlaying = false;
                player.Profile.coinsOnTable = 0;
                player.SetValues(player.Profile);
                //player.HideActionImage();
                player.TurnOffTimer();
            }

            foreach (var pot in pots)
            {
                Destroy(pot.Value.go);
            }
            pots.Clear();
        }

        private void CalculateWinner() // Each pot can have different winners
        {
            totalCoinsWon = 0;
            foreach(var pot in pots)
            {
                WinnerForPot(pot.Value);
            }
            AddToTotalCoins();
        }

        private void WinnerForPot(PotData pot)
        {
            HandCards handCards = new HandCards();
            handCards.playerID = 0;
            handCards.points = 0;
            handCards.rank = Globals.HandRank.HighCard;

            var playersInPot = playersInHand.Values.Where(p => pot.playersInPot.Contains(p.Profile.roomID));
            List<Player> players = playersInPot.Where(p => p.isPlaying).ToList();
            List<HandCards> results = new List<HandCards>();

            foreach (var player in players)
            {
                List<int> totalCards = communityCards.Keys.ToList();
                totalCards.AddRange(player.cards);
                var result = CardDeckSO.GetResult(totalCards.ToArray());
                result.playerID = player.Profile.roomID;

                results.Add(result);
            }

            var rankedResult = results.OrderByDescending(r => (int)r.rank).ToList();
            var topRanks = rankedResult.Where(r => (int)r.rank == (int)rankedResult[0].rank).ToList();

            if (topRanks.Count == 1)
            {
                //GameManager.instance.uI_Manager.DeclareWinner(topRanks[0].playerID, topRanks[0].rank.ToString());
                AddPotToWinner(new int[] { topRanks[0].playerID }, pot);
                return;
            }

            var topPoints = topRanks.OrderByDescending(r => r.points).ToList();
            topPoints = topPoints.Where(tp => tp.points == topPoints[0].points).ToList();
            var ids = topPoints.Select(tp => tp.playerID).ToArray();
            AddPotToWinner(ids, pot);
            //GameManager.instance.uI_Manager.DeclareWinner(topPoints[0].playerID, topPoints[0].rank.ToString());
        }

        private void AddPotToWinner(int[] winnerIDs, PotData pot)
        {
            double share = pot.coinsInPot / winnerIDs.Length;

            foreach(var id in winnerIDs)
            {
                var player = playersInHand.Values.FirstOrDefault(p => p.Profile.roomID == id);
                if (player == null) continue;
                player.Profile.coins += share;
                player.SetValues(player.Profile);
                PlayChipsAnimation(id, pot.go.GetComponent<RectTransform>());

                if(id == thisPlayer.Profile.roomID)
                {
                    totalCoinsWon += share;
                }
            }
            SoundManager.instance.PlayTakePotSound();
            Destroy(pot.go);
        }

        private void LoseGame()
        {
            double lostAmount = potWithShares.FirstOrDefault(kvp => kvp.Key == thisPlayer.Profile.roomID).Value;
            if (lostAmount <= 0) return;
            if (!Globals.isTesting) GetBrowserData.instance.webConnectivety.AddLoss(Globals.gameID, lostAmount);
        }

        private void AddToTotalCoins()
        {
            if (totalCoinsWon <= 0)
            {
                LoseGame();
                return;
            } 

            double totalCoinsPlayed = potWithShares.FirstOrDefault(kvp => kvp.Key == thisPlayer.Profile.roomID).Value;
            double totalProfit = totalCoinsWon - totalCoinsPlayed;

            Debug.Log("<color=yellow>Coins Played</color> " + totalCoinsPlayed);
            Debug.Log("<color=yellow>Coins Won</color> " + totalProfit);

            if (!Globals.isTesting)
            {
                if (totalProfit > 0)
                {
                    GetBrowserData.instance.webConnectivety.AddCoins(totalCoinsPlayed);
                    GetBrowserData.instance.webConnectivety.AddWin(Globals.gameID, totalProfit);
                    GameManager.instance.uI_Manager.ActivateWinnerPanel();
                }
                else
                {
                    GetBrowserData.instance.webConnectivety.AddCoins(totalCoinsWon);
                }
            }
        }

        private void PlayChipsAnimation(int targetID, RectTransform potRect)
        {
            var potPosX = (potRect.parent as RectTransform).anchorMin.x * gameCanvasRect.sizeDelta.x
                + potRect.anchoredPosition.x;
            var potPosY = (potRect.parent as RectTransform).anchorMax.y * gameCanvasRect.sizeDelta.y
                + potRect.anchoredPosition.y;
            var startingPosition = new Vector2(potPosX, potPosY);

            int slot = playersInHand.Values.FirstOrDefault(p => p.Profile.roomID == targetID).Profile.slot;
            var slotRect = slotTransforms[slot] as RectTransform;
            var slotPosX = (slotRect.anchorMin.x + (slotRect.anchorMax.x - slotRect.anchorMin.x) / 2)
                * gameCanvasRect.sizeDelta.x;
            var slotPosY = (slotRect.anchorMin.y + (slotRect.anchorMax.y - slotRect.anchorMin.y) / 2)
                * gameCanvasRect.sizeDelta.y;
            var targetPosition = new Vector2(slotPosX, slotPosY);

            for(int i = 0; i < 6; i++)
            {
                var chipRect = Instantiate(chipPrefab, chipsPanelTransform).transform as RectTransform;
                chipRect.anchoredPosition = startingPosition;
                DOTween.Sequence().AppendInterval(0.025f * i).Append(chipRect.DOAnchorPos(targetPosition, 0.2f)
                    .OnComplete(() => {
                        Destroy(chipRect.gameObject);
                    }));
            }
        }

        private IEnumerator DirectlyDeclareWinner(int winnerID)
        {
            totalCoinsWon = 0;
            foreach (var pot in pots)
            {
                AddPotToWinner(new int[] { winnerID }, pot.Value);
            }
            AddToTotalCoins();
            ResetValues();

            yield return new WaitForSeconds(2);
            CheckCoinsForTable();

            //GameManager.instance.uI_Manager.DeclareWinner(winnerID);

            yield return new WaitForSeconds(5);

            if (NetworkManager.instance.roomManager.IsMaster)
            {
                foreach (Player player in playersOnTable.Values)
                {
                    gameNetwork.SendTakeCoinsForTable(player.Profile.roomID);
                }

                if (playersOnTable.Count > 1)
                {
                    if (!Globals.isTesting)
                        GetBrowserData.instance.webConnectivety
                            .CreateGame(Globals.modeNames[NetworkManager.instance.roomManager.CurrentMode - 1]);
                } 
                else gameStarted = false;
            }
        }

        private IEnumerator CheckCoinsWithDelay()
        {
            yield return new WaitForSeconds(1);
            if(!Globals.isTesting) GetBrowserData.instance.webConnectivety.GetCoins();
        }

        #endregion

        #region Public Functions
        // Gameplay Methods
        /// <summary>
        /// Initializes player and takes a slot at the table. 
        /// </summary>
        /// <param name="slots">Occupied slots before this player takes a position at the table.</param>
        /// <returns></returns>
        public void InitializePlayer(byte slots)
        {
            myActorNumber = NetworkManager.instance.roomManager.ActorNumber;
            GameData.instance.playerProfile.roomID =
                System.Convert.ToByte(myActorNumber);
            GameManager.instance.uI_Manager.gameUI.DisableAllControls();

            double requiredCoins = Globals.GetRequiredAmountForTable(NetworkManager.instance.roomManager.CurrentMode);

            GameData.instance.playerProfile.coins = requiredCoins;
            GameManager.instance.uI_Manager.gameUI.UpdateTotalCoins();

            var slotsArr = BinaryByteConversion.ConvertByteToBoolArray(slots);

            for(int i = 0; i < slotsArr.Length; i++)
            {
                if (!slotsArr[i])
                {
                    TakePositionAtTheTable(i);
                    break;
                }
            }
        }

        public void StartHand()
        {
            if (gameStarted && playersInHand.Count > 0)
                return;

            if(playersOnTable.Count < 2)
            {
                gameStarted = false;
                return;
            }

            if (!NetworkManager.instance.roomManager.IsMaster)
                return;

            if (!gameStarted)
            {
                firstPlayerSlotInLastHand = -1;
            }
            gameStarted = true;

            foreach(var player in playersOnTable.Values)
            {
                if(player.IsActive)
                    playersInHand.Add(player.Profile.slot, player);
            }
            
            int smallBlind = firstPlayerSlotInLastHand == -1
                ? (playersInHand.Values.Any(p => p.Profile.slot == 0) ? 0 : GetNextActiveSlotInHand(0, true))
                : GetNextActiveSlotInHand(firstPlayerSlotInLastHand, true);
            int bigBlind = GetNextActiveSlotInHand(smallBlind, true);
            int firstToAction = GetNextActiveSlotInHand(bigBlind, true);

            firstPlayerSlotInLastHand = smallBlind;

            smallBlind = playersInHand.Values.FirstOrDefault(p => p.Profile.slot == smallBlind).Profile.roomID;
            bigBlind = playersInHand.Values.FirstOrDefault(p => p.Profile.slot == bigBlind).Profile.roomID;
            firstToAction = playersInHand.Values.FirstOrDefault(p => p.Profile.slot == firstToAction).Profile.roomID;
            
            List<int> cards = GetCardsToDeal((playersInHand.Count * 2) + 5, cardDeck.GetNewCardDeck());
            gameNetwork.SendUpdatePlayersInHand(playersInHand.Values.Select(p => p.Profile.roomID).ToList());

            var blinds = Globals.GetBlindValues(NetworkManager.instance.roomManager.CurrentMode);

            gameNetwork.SendUpdateBlind(smallBlind, blinds.sb);
            gameNetwork.SendUpdateBlind(bigBlind, blinds.bb);

            foreach(Player player in playersInHand.Values.ToList())
            {
                if (player.Profile.roomID != smallBlind && player.Profile.roomID != bigBlind)
                    gameNetwork.SendSetIsOn(player.Profile.roomID, true);

                gameNetwork.SendSetIsPlaying(player.Profile.roomID, true);

                int card1 = cards[Random.Range(0, cards.Count)];
                cards.Remove(card1);
                int card2 = cards[Random.Range(0, cards.Count)];
                cards.Remove(card2);

                gameNetwork.SendSetCards(card1, card2, player.Profile.roomID);
            }

            var commCards = new Dictionary<int, int>();
            foreach (int card in cards) commCards.Add(card, 0);
            gameNetwork.SendSetCommunityCards(commCards);

            gameNetwork.SendSetRound((int)Globals.Round.PreFlop);

            PokerAction pokerAction = new PokerAction();
            var plr = playersInHand.Values.FirstOrDefault(p => p.Profile.roomID == bigBlind);
            pokerAction.playerID = bigBlind;
            pokerAction.playerSlot = plr.Profile.slot;
            pokerAction.nextPlayerID = firstToAction;
            pokerAction.playerCoins = plr.Profile.coins;
            pokerAction.coinsOnTable = blinds.bb;
            pokerAction.betOrRaise = blinds.bb;
            pokerAction.moveType = (int)Globals.MoveType.None;

            gameNetwork.SendStartNextMove(pokerAction);
            // TODO Send these to new joining player too
        }

        public void StartNextMove(PokerAction pokerAction)
        {
            StartCoroutine(CheckCoinsWithDelay());
            foreach (var plr in playersOnTable.Values)
            {
                plr.TurnOffTimer();
            }

            if (!gameStarted)
                return;

            if (pots.Count == 1)
                if (pots[0].playersInPot.Count == 0)
                    pots[0].playersInPot = playersInHand.Values.Select(p => p.Profile.roomID).ToList();

            previousAction = pokerAction;

            var nextPlayer = playersOnTable.Values.FirstOrDefault(p => p.Profile.roomID == pokerAction.nextPlayerID);
            if(nextPlayer == null || !nextPlayer.Profile.isOn)
            {
                int nextSlot = GetNextActiveSlotInHand(pokerAction.playerSlot);

                if(nextSlot == -1)
                {
                    UpdateMoveValues();
                    CheckForNoActiveSlot(); 
                    return;
                }
                else
                {
                    nextPlayer = playersOnTable.Values
                        .FirstOrDefault(p => p.Profile.slot == nextSlot);
                }
            }

            int nextActiveSlot = GetNextActiveSlotInHand(nextPlayer.Profile.slot);
            var playing = playersInHand.Values.Where(p => p.isPlaying).ToList();

            if (nextActiveSlot == -1)
            {
                if(playing.Count > 1)
                {
                    bool allIn = true;
                    foreach (Player player in playing)
                    {
                        double playerCoins = player.Profile.roomID == previousAction.playerID
                            ? previousAction.playerCoins : player.Profile.coins;

                        if (playerCoins != 0)
                        {
                            if (previousAction.nextPlayerID == player.Profile.roomID
                                && potWithShares.Values.Max() == potWithShares[previousAction.nextPlayerID])
                                break;
                            else
                            {
                                allIn = false;
                                break;
                            }
                        }
                    }

                    if (allIn)
                    {
                        UpdateMoveValues();
                        CheckForNoActiveSlot();
                        return;
                    }
                }
                else
                {
                    UpdateMoveValues();
                    CheckForNoActiveSlot();
                    return;
                }
            }

            if (CheckRound())
                return;

            if (NetworkManager.instance.roomManager.IsMaster)
                gameNetwork.SendSetActivePlayer(nextPlayer.Profile.roomID);

            if(thisPlayer.Profile.roomID == nextPlayer.Profile.roomID)
            {
                if(pokerAction.coinsOnTable - thisPlayer.Profile.coinsOnTable >= thisPlayer.Profile.coins)
                {
                    GameManager.instance.uI_Manager.gameUI.SetControls(false, false, true, true);
                    GameManager.instance.uI_Manager.gameUI.MoveControls(true);
                }
                else if(pokerAction.coinsOnTable > thisPlayer.Profile.coinsOnTable)
                {
                    double minAmount = pokerAction.betOrRaise + pokerAction.coinsOnTable
                        - thisPlayer.Profile.coinsOnTable;
                    if(thisPlayer.Profile.coins >= minAmount)
                        GameManager.instance.uI_Manager.gameUI.EnableBetOrRaise(minAmount,
                        thisPlayer.Profile.coins, "Raise");

                    GameManager.instance.uI_Manager.gameUI.SetControls(false,
                        thisPlayer.Profile.coins > pokerAction.coinsOnTable - thisPlayer.Profile.coinsOnTable,
                        true, true);
                    GameManager.instance.uI_Manager.gameUI.MoveControls(true);
                }
                else
                {
                    var blinds = Globals.GetBlindValues(NetworkManager.instance.roomManager.CurrentMode);
                    if (thisPlayer.Profile.coins >= blinds.bb)
                        GameManager.instance.uI_Manager.gameUI.EnableBetOrRaise(blinds.bb,
                        thisPlayer.Profile.coins, "Bet");

                    GameManager.instance.uI_Manager.gameUI.SetControls(true, false, true, true);
                    GameManager.instance.uI_Manager.gameUI.MoveControls(true);
                }
            }

            if (pokerAction.moveType == (int)Globals.MoveType.None)
                return;

            UpdateMoveValues();
        }

        public void PerformAction(Globals.MoveType action, double betOrRaise = 0)
        {
            if (!gameStarted)
                return;

            PokerAction pokerAction = new PokerAction();
            if (betOrRaise == thisPlayer.Profile.coins)
                action = Globals.MoveType.AllIn;
            pokerAction.moveType = (int)action;
            thisPlayer.TurnOffTimer();

            gameNetwork.SendPlayerPerformedAction(thisPlayer.Profile.roomID, pokerAction.moveType);

            if(action == Globals.MoveType.Call)
            {
                double amount = previousAction.coinsOnTable - thisPlayer.Profile.coinsOnTable;
                if (!Globals.isTesting) GetBrowserData.instance.webConnectivety.SubtractCoins(amount);
                thisPlayer.Profile.coins -= amount;
                thisPlayer.Profile.coinsOnTable += amount;
                if (potWithShares.ContainsKey(thisPlayer.Profile.roomID))
                    potWithShares[thisPlayer.Profile.roomID] += amount;
                else
                    potWithShares.Add(thisPlayer.Profile.roomID, amount);
                betOrRaise = previousAction.betOrRaise;
            }
            else if(action == Globals.MoveType.Bet)
            {
                thisPlayer.Profile.coins -= betOrRaise;
                if (!Globals.isTesting) GetBrowserData.instance.webConnectivety.SubtractCoins(betOrRaise);
                thisPlayer.Profile.coinsOnTable += betOrRaise;
                if (potWithShares.ContainsKey(thisPlayer.Profile.roomID))
                    potWithShares[thisPlayer.Profile.roomID] += betOrRaise;
                else
                    potWithShares.Add(thisPlayer.Profile.roomID, betOrRaise);
            }
            else if (action == Globals.MoveType.Raise)
            {
                thisPlayer.Profile.coins -= betOrRaise;
                if (!Globals.isTesting) GetBrowserData.instance.webConnectivety.SubtractCoins(betOrRaise);
                thisPlayer.Profile.coinsOnTable += betOrRaise;
                if (potWithShares.ContainsKey(thisPlayer.Profile.roomID))
                    potWithShares[thisPlayer.Profile.roomID] += betOrRaise;
                else
                    potWithShares.Add(thisPlayer.Profile.roomID, betOrRaise);
            }
            else if (action == Globals.MoveType.AllIn)
            {
                double amount = thisPlayer.Profile.coins;
                thisPlayer.Profile.coins -= amount;
                if (!Globals.isTesting) GetBrowserData.instance.webConnectivety.SubtractCoins(amount);
                thisPlayer.Profile.coinsOnTable += amount;
                if (potWithShares.ContainsKey(thisPlayer.Profile.roomID))
                    potWithShares[thisPlayer.Profile.roomID] += amount;
                else
                    potWithShares.Add(thisPlayer.Profile.roomID, amount);
                gameNetwork.SendSetIsOn(thisPlayer.Profile.roomID, false);
                betOrRaise = previousAction.betOrRaise;
            }
            else if (action == Globals.MoveType.Fold)
            {
                gameNetwork.SendSetIsOn(thisPlayer.Profile.roomID, false);
                gameNetwork.SendSetIsPlaying(thisPlayer.Profile.roomID, false);

                double lostAmount = potWithShares.FirstOrDefault(kvp => kvp.Key == thisPlayer.Profile.roomID).Value;
                if (!Globals.isTesting) GetBrowserData.instance.webConnectivety.AddLoss(Globals.gameID, lostAmount);
            }

            GameManager.instance.uI_Manager.gameUI.DisableAllControls();
            gameNetwork.SendUpdatePot(potWithShares);

            if(action == Globals.MoveType.Fold ||
                (action == Globals.MoveType.AllIn && thisPlayer.Profile.coinsOnTable < previousAction.coinsOnTable))
            {
                gameNetwork.SendUpdatePlayerCoins(thisPlayer.Profile.roomID, thisPlayer.Profile.coins,
                    thisPlayer.Profile.coinsOnTable);
                pokerAction = previousAction;
            }
            else 
            {
                pokerAction.playerID = thisPlayer.Profile.roomID;
                pokerAction.playerSlot = thisPlayer.Profile.slot;
                pokerAction.playerCoins = thisPlayer.Profile.coins;
                pokerAction.coinsOnTable = thisPlayer.Profile.coinsOnTable;
                pokerAction.betOrRaise = betOrRaise;
            }

            int nextActiveSlot = GetNextSlotInHand(thisPlayer.Profile.slot);
            if (nextActiveSlot == -1)
            {
                playersPerformedAction[thisPlayer.Profile.slot] = thisPlayer.Profile.roomID;
                gameNetwork.SendPlayersPerformedAction(playersPerformedAction);
                gameNetwork.SendNextPlayerNotFound();
                return;
            }
            pokerAction.nextPlayerID = playersInHand.Values.FirstOrDefault(p => p.Profile.slot == nextActiveSlot)
                .Profile.roomID;

            playersPerformedAction[thisPlayer.Profile.slot] = thisPlayer.Profile.roomID;
            gameNetwork.SendPlayersPerformedAction(playersPerformedAction);
            gameNetwork.SendStartNextMove(pokerAction);
        }

        public void PlayerPerformedAction(int id, int action)
        {
            SoundManager.instance.PlayActionSound((Globals.MoveType)action);

            var player = playersInHand.Values.FirstOrDefault(p => p.Profile.roomID == id);

            if (!player) return;

            player.ShowActionImage((Globals.MoveType)action);
        }

        public void UpdatePlayerCoins(int id, double coins, double coinsOnTable)
        {
            var player = playersInHand.Values.FirstOrDefault(p => p.Profile.roomID == id);

            if (player == null) return;

            player.Profile.coins = coins;
            player.Profile.coinsOnTable = coinsOnTable;
            player.SetValues(player.Profile);
        }

        public void TakeCoinsForTable()
        {
            foreach (var player in playersOnTable.Values) player.SetCards(player.cards);

            double requiredCoins = Globals.GetRequiredAmountForTable(NetworkManager.instance.roomManager.CurrentMode);
            
            if (thisPlayer.Profile.coins > 0)
                return;

            double coinsToGet = requiredCoins - thisPlayer.Profile.coins; 
            bool hasEnoughCoins = Globals.userInfo.chips >= coinsToGet;

            if (!hasEnoughCoins)
            {
                GameManager.instance.uI_Manager.SetSpectateButton(false);
                StartSpectating();
            }


            thisPlayer.Profile.coins = requiredCoins;
            GameManager.instance.uI_Manager.gameUI.UpdateTotalCoins();
            gameNetwork.SendUpdateCoinsForHand(thisPlayer.Profile.roomID, thisPlayer.Profile.coins);
        }

        public void UpdateCoinsForHand(int id, double coins)
        {
            var player = playersOnTable.Values.FirstOrDefault(p => p.Profile.roomID == id);

            player.Profile.coins = coins;

            player.SetValues(player.Profile);
        }

        public void SetPlayersPerformedAction(Dictionary<int, int> dict)
        {
            playersPerformedAction = dict;
        }

        public void SetIsPlaying(int id, bool val)
        {
            var player = playersOnTable.Values.FirstOrDefault(p => p.Profile.roomID == id);

            if (player != null)
                player.isPlaying = val;
        }

        public void SetPlayerOn(int id, bool val)
        {
            var player = playersOnTable.Values.FirstOrDefault(p => p.Profile.roomID == id);

            if (player != null)
                player.SetIsOn(val);
        }

        public void SetActivePlayer(int playerID)
        {
            foreach(Player player in playersOnTable.Values)
            {
                player.SetIsActive(player.Profile.roomID == playerID);
            }
        }

        public void NextPlayerNotFound()
        {
            CheckForNoActiveSlot();
        }

        public void SetRound(int round)
        {
            this.round = (Globals.Round)round;
        }

        public void SetCards(int card1, int card2, int playerID)
        {
            var player = playersOnTable.Values.FirstOrDefault(p => p.Profile.roomID == playerID);

            if (!player)
                return;

            player.SetCards(new List<int> { card1, card2 }, playerID == GameData.instance.playerProfile.roomID);
            if (playerID == GameData.instance.playerProfile.roomID) SoundManager.instance.PlayCardFlipSound();
        }

        public void Showdown()
        {
            if (playersInHand.Count == 0)
                return;

            foreach(var player in playersInHand.Values)
            {
                player.SetCards(player.cards, true);
            }

            StartCoroutine(Ending());
        }

        public void SetCommunityCards(Dictionary<int, int> cards)
        {
            bool soundPlayed = false;
            cardsParent.SetActive(true);
            communityCards = new Dictionary<int, bool>();

            foreach (KeyValuePair<int, int> kvp in cards)
            {
                communityCards.Add(kvp.Key, kvp.Value > 0);
            }

            for(int i = 0; i < communityCards.Count; i++)
            {
                var kvp = communityCards.ElementAt(i);

                if (kvp.Value)
                {
                    cardSpriteRenderers[i].sprite = cardDeck.GetCardSprite(kvp.Key);
                    if (!cardSpriteRenderers[i].gameObject.activeSelf)
                    {
                        cardSpriteRenderers[i].GetComponent<Animator>().Play("Open", 0, 0);
                        if (!soundPlayed)
                        {
                            soundPlayed = true;
                            SoundManager.instance.PlayCardFlipSound();
                        }
                    }
                    cardSpriteRenderers[i].gameObject.SetActive(true);
                }
                else
                {
                    cardSpriteRenderers[i].sprite = cardDeck.GetCardBackSprite();
                    cardSpriteRenderers[i].gameObject.SetActive(false);
                }
            }

            List<int> playerCards = thisPlayer.cards;
            if (playerCards == null)
                return;
            if (playerCards.Count == 0)
                return;

            var totalCards = communityCards.Where(c => c.Value).ToDictionary(x => x.Key, y => y.Value)
                .Keys.ToList();
            totalCards.AddRange(playerCards);

            resultTMP.text = CardDeckSO.GetResult(totalCards.ToArray()).rank.ToString();
        }

        public void UpdatePlayersInHand(List<int> playersInHandIDs)
        {
            gameStarted = true;
            foreach(int id in playersInHandIDs)
            {
                Player player = playersOnTable.Values.FirstOrDefault(p => p.Profile.roomID == id);

                if (player)
                {
                    playersInHand.Add(player.Profile.slot, player);
                }
            }
        }

        public void UpdateBlind(int id, double bet)
        {
            foreach(var plr in playersOnTable.Values)
            {
                plr.HideActionImage();
            }

            var player = playersOnTable.Values.FirstOrDefault(p => p.Profile.roomID == id);
            if (!player)
                return;

            PlayerProfile profile = new PlayerProfile();
            profile.roomID = id;
            profile.coins = player.Profile.coins - bet;
            profile.slot = player.Profile.slot;
            profile.coinsOnTable = bet;
            profile.isOn = true;
            player.SetValues(profile);
            player.SetIsOn(true);

            if (!Globals.isTesting && thisPlayer.Profile.roomID == id)
                GetBrowserData.instance.webConnectivety.SubtractCoins(bet);

            AddCoinsToPot(bet, id);

            var blinds = Globals.GetBlindValues(NetworkManager.instance.roomManager.CurrentMode);
            if (bet == blinds.sb)
                firstPlayerSlotInLastHand = player.Profile.slot;
        }

        public void UpdatePot(Dictionary<int, double> potShares)
        {
            potWithShares = potShares;
        }

        public void SpawnAndGetReady(PlayerProfile playerProfile)
        {
            SpawnPlayer(playerProfile);
            GetNewPlayerReady(playerProfile);
        }

        public void SpawnPlayer(PlayerProfile playerProfile)
        {
            if(playersOnTable.Count > 0)
            {
                bool alreadySpawned = playersOnTable.Values.Any(p => p.Profile.roomID == playerProfile.roomID);
                if (alreadySpawned) return;
            }

            bool isPlayer = GameData.instance.playerProfile.roomID == playerProfile.roomID;
            if (isPlayer)
            {
                if (selfSpawned)
                    return;

                selfSpawned = true;
            }

            var player = Instantiate(playerPrefab, slotTransforms[playerProfile.slot]);
            player.SetCoinsOnTableTMP(slotCoinsOnTableTMPs[playerProfile.slot]);
            player.HideActionImage();
            player.TurnOffTimer();
            player.SetValues(playerProfile);
            player.SetIsPlayer(isPlayer);
            player.SetPlayerName();
            if (isPlayer) thisPlayer = player;
            playersOnTable.Add(playerProfile.slot, player);
            GameManager.instance.uI_Manager.SetSpectateButton(true);
            GameManager.instance.uI_Manager.gameUI.DisableAllAddButtons();
            UpdateAddButtons();
            player.isPlaying = false;
            player.SetAsActive();

            CheckPlayersOnTable();
        }

        public void AddPlayerToTable(int slot)
        {
            if (playersOnTable.Keys.Contains(slot))
                return;

            GameManager.instance.uI_Manager.gameUI.DisableAllAddButtons();
            TakePositionAtTheTable(slot);
            TakeCoinsForTable();
            GameManager.instance.uI_Manager.SetSpectateButton(true);
        }

        public void StartSpectating()
        {
            gameNetwork.SendRemovePlayer(thisPlayer.Profile.roomID);
        }

        public void ResetTableCoins()
        {
            foreach (var tmp in slotCoinsOnTableTMPs) tmp.transform.parent.gameObject.SetActive(false);

            foreach(var player in playersInHand.Values)
            {
                player.Profile.coinsOnTable = 0;
                player.SetValues(player.Profile);
            }
        }

        private void TransferMoveToNextPlayer(int leavingPlayerSlot)
        {
            if (!NetworkManager.instance.roomManager.IsMaster)
                return;

            PokerAction pokerAction = previousAction;
            pokerAction.moveType = (int)Globals.MoveType.None;
            int nextActiveSlot = GetNextActiveSlotInHand(leavingPlayerSlot);
            if (nextActiveSlot == -1)
            {
                playersPerformedAction[leavingPlayerSlot] = previousAction.nextPlayerID;
                gameNetwork.SendPlayersPerformedAction(playersPerformedAction);
                gameNetwork.SendNextPlayerNotFound();
                return;
            }

            playersPerformedAction[leavingPlayerSlot] = previousAction.nextPlayerID;
            gameNetwork.SendPlayersPerformedAction(playersPerformedAction);
            pokerAction.nextPlayerID = playersInHand.Values.FirstOrDefault(p => p.Profile.slot == nextActiveSlot)
                .Profile.roomID;
            gameNetwork.SendStartNextMove(pokerAction);
        }

        public void SetPlayerInactive(int playerID)
        {
            var player = playersOnTable.Values.FirstOrDefault(p => p.Profile.roomID == playerID);

            if (player == null) return;

            player.SetAsInactive();
        }

        public void ResumePlayerActive(int playerID)
        {
            var player = playersOnTable.Values.FirstOrDefault(p => p.Profile.roomID == playerID);

            if (player == null) return;

            player.SetAsActive();
        }

        public void PlayerLeaving()
        {
            gameNetwork.SendRemovePlayer(thisPlayer.Profile.roomID);
        }

        public void RemoveInactivePlayer(int playerID)
        {
            RemovePlayer(playerID);
            if (playerID == thisPlayer.Profile.roomID)
            {
                GameManager.instance.LeaveRoom();
                NetworkManager.instance.ReEstablishConnection();
            }
        }

        public void RemovePlayer(int playerID)
        {
            int slotNumber = -1;
            foreach(KeyValuePair<int, Player> kv in playersOnTable)
            {
                if(kv.Value.Profile.roomID == playerID)
                {
                    slotNumber = kv.Key;
                    break;
                }
            }

            if (slotNumber == -1)
                return;

            if (playerID == GameData.instance.playerProfile.roomID)
            {
                RetractCoins();
                selfSpawned = false;
                LoseGame();
            }

            if (previousAction != null)
            {
                if (previousAction.nextPlayerID == playerID)
                {
                    TransferMoveToNextPlayer(slotNumber);
                }
            }

            Destroy(playersOnTable[slotNumber].gameObject);
            playersOnTable.Remove(slotNumber);
            playersInHand.Remove(slotNumber);
            slotCoinsOnTableTMPs[slotNumber].text = "";
            slotCoinsOnTableTMPs[slotNumber].transform.parent.gameObject.SetActive(false);
            UpdateAddButtons();

            var players = playersInHand.Values.Where(p => p.isPlaying).ToList();

            if (players != null)
            {
                if(players.Count <= 1)
                    CheckForNoActiveSlot();
            }
            else
            {
                CheckForNoActiveSlot();
            }

            if (playersInHand.Count <= 1) gameStarted = false;
            NetworkManager.instance.roomManager.UpdateSlotOnTable(slotNumber, false);

            CheckPlayersOnTable();
        }
        #endregion
    }
}



public class PotData
{
    public GameObject go;
    public TextMeshProUGUI tmp;
    public double coinsInPot;
    public List<int> playersInPot;
}
