using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals 
{
    #region Enums
    public enum HandRank { HighCard = 0, OnePair = 1, TwoPair = 2, ThreeOfAKind = 3, Straight = 4,
    Flush = 5, FullHouse = 6, FourOfAKind = 7, StraightFlush = 8, RoyalFlush = 9 }
    public enum Suit { Diamonds = 0, Clubs = 1, Hearts = 2, Spades = 3 }
    public enum MoveType { None = 0, Check = 1, Call = 2, Bet = 3, Raise = 4, Fold = 5, AllIn = 6 }
    public enum Round { PreFlop = 0, Flop = 1, Turn = 2, River = 3, Showdown = 4 }
    #endregion

    #region Events
    public static event Action<double> getCoinsEvent;
    public static event Action<string> createGameEvent;
    #endregion

    #region Booleans
    public static bool isTesting = false;
    public static bool isSwitching = false;
    public static bool isSocketsConnected = false;
    #endregion

    #region Numbers
    public static byte maxPlayersInRoom = 10;
    public static int maxPlayersOnTable = 6;
    public static int roomTTL = 30000; //15000;   // 15 Seconds
    public static int playerTTL = 30000; //15000; // 15 Seconds
    public static int timeToStartGame = 5;
    public static int maxTimeToPerformAction = 15;
    public static double mode1RequiredAmount = 50;
    public static double mode2RequiredAmount = 100;
    public static double mode3RequiredAmount = 200;
    public static double mode4RequiredAmount = 500;
    #endregion

    #region Strings
    public static string gameID = "";
    public static string[] modeNames = new string[] { "50", "100", "200", "500" };
    #endregion

    #region Objects
    public static ExtractingGetUserInfo userInfo;
    #endregion

    public static double GetRequiredAmountForTable(int mode)
    {
        double amount = 50;
        switch (mode)
        {
            case 1:
                amount = mode1RequiredAmount;
                break;
            case 2:
                amount = mode2RequiredAmount;
                break;
            case 3:
                amount = mode3RequiredAmount;
                break;
            case 4:
                amount = mode4RequiredAmount;
                break;
            default:
                break;
        }

        return amount;
    }

    public static (double sb, double bb) GetBlindValues(int mode)
    {
        double sb = 0, bb = 0;
        switch (mode)
        {
            case 1:
                sb = 0.5f;
                bb = 1;
                break;
            case 2:
                sb = 1;
                bb = 2;
                break;
            case 3:
                sb = 2;
                bb = 4;
                break;
            case 4:
                sb = 5;
                bb = 10;
                break;
            default:
                break;
        }

        return (sb, bb);
    }

    public static void OnGetCoins(double coins)
    {
        userInfo.chips = coins;
        Debug.Log("<color=green>After: </color> " + userInfo.chips);
        getCoinsEvent?.Invoke(coins);
    }

    public static void OnCreatedGame(string game_id)
    {
        createGameEvent?.Invoke(game_id);
    }

    public static void CreateDummyUserInfo()
    {
        userInfo = new ExtractingGetUserInfo();
        userInfo.profile_image = "https://drive.google.com/uc?export=download&id=1_Yu5el4AQ7WSCy0jRobsITZQ9pR3esA2";
        userInfo._id = "64242d0dc319ff3af49ae0dd";
        userInfo.first_name = "Irfan";
        userInfo.last_name = "Shah";
        userInfo.email = "mzaeem3@gmail.com";
        userInfo.username = "shah g";
        userInfo.login_type = "Google";
        userInfo.social_id = "23702dummy2u32390";
        userInfo.role = "Admin";
        userInfo.wins = 10;
        userInfo.loose = 20;
        userInfo.chips = 2000;
        userInfo.__v = "1";
    }
}


