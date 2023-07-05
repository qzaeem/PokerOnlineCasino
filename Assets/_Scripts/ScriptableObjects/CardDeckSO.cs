using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "Card Deck", menuName = "ScriptableObjects/NewCardDeck", order = 1)]
public class CardDeckSO : ScriptableObject
{
    [SerializeField] private Sprite back;
    [SerializeField] private Sprite[] diamonds, clubs, hearts, spades;

    public List<int> GetNewCardDeck()
    {
        int[] arr = new int[52];

        for (int i = 0; i < 52; i++) arr[i] = i;

        return arr.ToList();
    }

    public Sprite GetCardSprite(int card)
    {
        if (card / 13 == 0)
            return diamonds[card % 13];
        if (card / 13 == 1)
            return clubs[card % 13];
        if (card / 13 == 2)
            return hearts[card % 13];

        return spades[card % 13];
    }

    public Sprite GetCardBackSprite()
    {
        return back;
    }

    public static HandCards GetResult(int[] cards)
    {
        // For Testing
        //PrintCardValues(cards);

        HandCards result = new HandCards();
        CardInfo[] cardInfos = new CardInfo[cards.Length];

        for(int i = 0; i < cards.Length; i++)
        {
            cardInfos[i] = new CardInfo();
            cardInfos[i].card = cards[i];
            cardInfos[i].rank = cards[i] % 13;
            cardInfos[i].suit = cards[i] / 13;
            cardInfos[i].points = cardInfos[i].rank == 0 ? 13 : cardInfos[i].rank;
        }

        cardInfos = cardInfos.OrderByDescending(i => i.rank).ToArray();

        Dictionary<int, List<CardInfo>> sameCards = cardInfos.GroupBy(x => x.rank).Where(g => g.Count() > 1)
            .ToDictionary(x => x.Key, y => y.ToList());

        Dictionary<int, List<CardInfo>> flush = cardInfos.GroupBy(x => x.suit).Where(g => g.Count() >= 5)
            .ToDictionary(x => x.Key, y => y.ToList());

        var straights = GetStraights(cardInfos);

        Globals.HandRank handRank = Globals.HandRank.HighCard;
        int points = 0;

        foreach(List<CardInfo> straight in straights)
        {
            int suit = straight[0].suit;
            bool containsKing = straight.Any(strt => strt.rank == 12);
            bool sameSuit = straight.All(strt => strt.suit == suit);
            int sum = straight.Sum(strt => strt.points);
            if (sum > points) points = sum;

            if (sameSuit)
            {
                points = straight.Sum(strt => strt.points);

                if (containsKing)
                {
                    handRank = Globals.HandRank.RoyalFlush;
                    result.rank = handRank;
                    result.points = points;
                    return result;
                }

                handRank = Globals.HandRank.StraightFlush;
                result.rank = handRank;
                result.points = points;
                return result;
            }

            handRank = Globals.HandRank.Straight;
        }

        var fourOfAKind = sameCards.Where(kvp => kvp.Value.Count == 4).ToDictionary(x => x.Key, x => x.Value);
        if (fourOfAKind.Count > 0)
        {
            int pts = 0;
            foreach(KeyValuePair<int, List<CardInfo>> kvp in fourOfAKind)
            {
                int sum = kvp.Value.Sum(card => card.points);
                if (sum > pts)
                    pts = sum;
            }

            points = pts;
            handRank = Globals.HandRank.FourOfAKind;
            result.rank = handRank;
            result.points = points;
            return result;
        }

        var threeOfAKind = sameCards.Where(kvp => kvp.Value.Count == 3).ToDictionary(x => x.Key, x => x.Value);
        var pairs = sameCards.Where(kvp => kvp.Value.Count == 2).ToDictionary(x => x.Key, x => x.Value);
        if (threeOfAKind.Count > 0 && pairs.Count > 0)
        {
            int pts = 0;
            foreach (KeyValuePair<int, List<CardInfo>> kvp in threeOfAKind)
            {
                int sum = kvp.Value.Sum(card => card.points);
                if (sum > pts)
                    pts = sum;
            }

            int pts2 = 0;
            foreach (KeyValuePair<int, List<CardInfo>> kvp in pairs)
            {
                int sum = kvp.Value.Sum(card => card.points);
                if (sum > pts2)
                    pts2 = sum;
            }

            points = pts + pts2;
            handRank = Globals.HandRank.FullHouse;
            result.rank = handRank;
            result.points = points;
            return result;
        }

        if(flush.Count > 0)
        {
            var combos = flush.First().Value.DifferentCombinations(5).ToList();
            int pts = 0;

            foreach(var combo in combos)
            {
                int sum = combo.Sum(card => card.points);
                if (sum > pts)
                    pts = sum;
            }

            points = pts;
            handRank = Globals.HandRank.Flush;
            result.rank = handRank;
            result.points = points;
            return result;
        }

        if(handRank == Globals.HandRank.Straight)
        {
            handRank = Globals.HandRank.Straight;
            result.rank = handRank;
            result.points = points;
            return result;
        }

        if(threeOfAKind.Count > 0)
        {
            var cardsTemp = cardInfos.ToList();
            List<CardInfo> tok = new List<CardInfo>();

            int pts = 0;
            foreach (KeyValuePair<int, List<CardInfo>> kvp in threeOfAKind)
            {
                int sum = kvp.Value.Sum(card => card.points);
                if (sum > pts)
                {
                    pts = sum;
                    tok = kvp.Value;
                }
            }

            int pts2 = 0;
            foreach (CardInfo card in tok) cardsTemp.RemoveAll(c => c.card == card.card);
            if(cardsTemp.Count > 1)
            {
                var remainingList = cardsTemp.DifferentCombinations(2).ToList();

                foreach(var combo in remainingList)
                {
                    int sum = combo.Sum(card => card.points);
                    if (sum > pts2)
                        pts2 = sum;
                }
            }

            points = pts + pts2;
            handRank = Globals.HandRank.ThreeOfAKind;
            result.rank = handRank;
            result.points = points;
            return result;
        }

        if(pairs.Count > 1)
        {
            var cardsTemp = cardInfos.ToList();
            var pairsTemp = pairs;
            int key1 = 0;
            List<CardInfo> pair1 = new List<CardInfo>();
            List<CardInfo> pair2 = new List<CardInfo>();

            int pts = 0;
            foreach (KeyValuePair<int, List<CardInfo>> kvp in pairsTemp)
            {
                int sum = kvp.Value.Sum(card => card.points);
                if (sum > pts)
                {
                    pts = sum;
                    pair1 = kvp.Value;
                    key1 = kvp.Key;
                }
            }
            foreach (CardInfo card in pair1) cardsTemp.RemoveAll(c => c.card == card.card);
            pairsTemp.Remove(key1);

            int pts2 = 0;
            foreach (KeyValuePair<int, List<CardInfo>> kvp in pairsTemp)
            {
                int sum = kvp.Value.Sum(card => card.points);
                if (sum > pts2)
                {
                    pts2 = sum;
                    pair2 = kvp.Value;
                }
            }
            foreach (CardInfo card in pair2) cardsTemp.RemoveAll(c => c.card == card.card);

            int pts3 = 0;
            foreach (CardInfo card in cardsTemp) if (card.points > pts3) pts3 = card.points;

            points = pts + pts2 + pts3;
            handRank = Globals.HandRank.TwoPair;
            result.rank = handRank;
            result.points = points;
            return result;
        }

        if(pairs.Count > 0)
        {
            var cardsTemp = cardInfos.ToList();
            var pair = pairs.First().Value;
            int pts = pair.Sum(card => card.points);
            foreach (CardInfo card in pair) cardsTemp.RemoveAll(c => c.card == card.card);

            int pts2 = 0;
            var combos = cardsTemp.DifferentCombinations(3);
            foreach(var combo in combos)
            {
                int sum = combo.Sum(card => card.points);
                if (sum > pts2)
                    pts2 = sum;
            }

            points = pts + pts2;
            handRank = Globals.HandRank.OnePair;
            result.rank = handRank;
            result.points = points;
            return result;
        }

        var combinations = cardInfos.DifferentCombinations(5).ToList();
        foreach (var combo in combinations)
        {
            int sum = combo.Sum(card => card.points);
            if (sum > points)
                points = sum;
        }

        handRank = Globals.HandRank.HighCard;
        result.rank = handRank;
        result.points = points;
        return result;
    }

    static List<List<CardInfo>> GetStraights(CardInfo[] cardInfos)
    {
        var combos = cardInfos.DifferentCombinations(5).ToList();
        List<List<CardInfo>> straights = new List<List<CardInfo>>();

        if (combos.Count == 0)
            return straights;

        foreach(var combination in combos)
        {
            CardInfo[] combo = combination.ToArray();
            List<CardInfo> straight = new List<CardInfo>();

            for(int i = 0; i < combo.Length; i++)
            {
                if(i + 1 >= combo.Length)
                {
                    if(combo[i - 1].rank == combo[i].rank + 1)
                    {
                        straight.Add(combo[i]);
                    }
                    break;
                }

                if(combo[i].rank == combo[i + 1].rank + 1)
                {
                    straight.Add(combo[i]);
                }
                else if (combo[i].rank == 9 && combo[i + 1].rank == 0)
                {
                    straight.Add(combo[i]);
                    straight.Add(combo[i + 1]);
                }
                else 
                {
                    break;
                }
            }

            if(straight.Count == 5)
            {
                straights.Add(straight);
            }
        }

        return straights;
    }

    static void PrintCardValues(int[] cards)
    {
        List<int> diamondCards = new List<int>(), clubCards = new List<int>(),
            heartCards = new List<int>(), spadeCards = new List<int>();

        diamondCards = cards.Where(i => i / 13 == (int)Globals.Suit.Diamonds).OrderByDescending(i => i)
            .Select(i => i %= 13).ToList();
        clubCards = cards.Where(i => i / 13 == (int)Globals.Suit.Clubs).OrderByDescending(i => i)
            .Select(i => i %= 13).ToList();
        heartCards = cards.Where(i => i / 13 == (int)Globals.Suit.Hearts).OrderByDescending(i => i)
            .Select(i => i %= 13).ToList();
        spadeCards = cards.Where(i => i / 13 == (int)Globals.Suit.Spades).OrderByDescending(i => i)
            .Select(i => i %= 13).ToList();

        diamondCards.ForEach((int i) => Debug.Log("Diamonds: " + i));
        clubCards.ForEach((int i) => Debug.Log("Clubs: " + i));
        heartCards.ForEach((int i) => Debug.Log("Hearts: " + i));
        spadeCards.ForEach((int i) => Debug.Log("Spades: " + i));
    }
}

public class HandCards
{
    public Globals.HandRank rank;
    public int points;
    public int playerID;
}

public class CardInfo
{
    public int card;
    public int rank;
    public int suit;
    public int points;
}
