using System.Collections.Generic;
using System.Linq;

public class PlayerProfile
{
    const int numberOfProperties = 5; // The total number of properties of this class that are to be sent through the network
    public double coins;
    public int roomID;
    public int slot;
    public double coinsOnTable;
    public bool isOn;

    public static Dictionary<byte, double> PlayerProfileToDictionary(PlayerProfile playerProfile)
    {
        Dictionary<byte, double> dict = new Dictionary<byte, double>();
        dict.Add(Constants.PLAYERPROFILE_ROOMID, playerProfile.roomID);
        dict.Add(Constants.PLAYERPROFILE_COINS, playerProfile.coins);
        dict.Add(Constants.PLAYERPROFILE_SLOT, playerProfile.slot);
        dict.Add(Constants.PLAYERPROFILE_COINSONTABLE, playerProfile.coinsOnTable);
        dict.Add(Constants.PLAYERPROFILE_ISON, playerProfile.isOn ? 1 : 0);

        return dict;
    }

    public static PlayerProfile DictionaryToPlayerProfile(Dictionary<byte, double> dict)
    {
        PlayerProfile playerProfile = new PlayerProfile();
        playerProfile.roomID = (int)dict[Constants.PLAYERPROFILE_ROOMID];
        playerProfile.coins = dict[Constants.PLAYERPROFILE_COINS];
        playerProfile.slot = (int)dict[Constants.PLAYERPROFILE_SLOT];
        playerProfile.coinsOnTable = dict[Constants.PLAYERPROFILE_COINSONTABLE];
        playerProfile.isOn = dict[Constants.PLAYERPROFILE_ISON] > 0 ? true : false;

        return playerProfile;
    }

    public static Dictionary<byte, double> PlayerProfilesToDictionary(List<PlayerProfile> playerProfiles)
    {
        Dictionary<byte, double> playerProfilesDict = new Dictionary<byte, double>();

        for(int i = 0; i < playerProfiles.Count; i++)
        {
            int j = numberOfProperties * i;

            playerProfilesDict.Add(System.Convert.ToByte(j), playerProfiles[i].coins);
            playerProfilesDict.Add(System.Convert.ToByte(j + 1), playerProfiles[i].roomID);
            playerProfilesDict.Add(System.Convert.ToByte(j + 2), playerProfiles[i].slot);
            playerProfilesDict.Add(System.Convert.ToByte(j + 3), playerProfiles[i].coinsOnTable);
            playerProfilesDict.Add(System.Convert.ToByte(j + 4), playerProfiles[i].isOn ? 1 : 0);
        }

        return playerProfilesDict;
    }

    public static List<PlayerProfile> DictionaryToPlayerProfiles(Dictionary<byte, double> playerProfilesDict)
    {
        List<PlayerProfile> playerProfiles = new List<PlayerProfile>();
        var playerProfilesList = playerProfilesDict.Values.ToList();

        for (int i = 0; i < playerProfilesDict.Count / numberOfProperties; i++)
        {
            PlayerProfile playerProfile = new PlayerProfile();
            int j = numberOfProperties * i;
            playerProfile.coins = playerProfilesList[j];
            playerProfile.roomID = (int)playerProfilesList[j + 1];
            playerProfile.slot = (int)playerProfilesList[j + 2];
            playerProfile.coinsOnTable = playerProfilesList[j + 3];
            playerProfile.isOn = playerProfilesList[j + 4] > 0 ? true : false;

            playerProfiles.Add(playerProfile);
        }

        return playerProfiles;
    }
}


public class PokerAction
{
    public int playerID;
    public int nextPlayerID;
    public int playerSlot;
    public double coinsOnTable;
    public double playerCoins;
    public double betOrRaise;
    public int moveType; // Defined enum in Globals

    public static Dictionary<byte, double> PokerActionToDictionary(PokerAction action)
    {
        Dictionary<byte, double> dict = new Dictionary<byte, double>();
        dict.Add(Constants.POKERACTION_PLAYERID, action.playerID);
        dict.Add(Constants.POKERACTION_NEXTPLAYERID, action.nextPlayerID);
        dict.Add(Constants.POKERACTION_PLAYERCOINS, action.playerCoins);
        dict.Add(Constants.POKERACTION_COINSONTABLE, action.coinsOnTable);
        dict.Add(Constants.POKERACTION_MOVETYPE, action.moveType);
        dict.Add(Constants.POKERACTION_PLAYERSLOT, action.playerSlot);
        dict.Add(Constants.POKERACTION_BETORRAISE, action.betOrRaise);

        return dict;
    }

    public static PokerAction DictionaryToPlayerProfile(Dictionary<byte, double> dict)
    {
        PokerAction pokerAction = new PokerAction();
        pokerAction.playerID = (int)dict[Constants.POKERACTION_PLAYERID];
        pokerAction.nextPlayerID = (int)dict[Constants.POKERACTION_NEXTPLAYERID];
        pokerAction.playerCoins = dict[Constants.POKERACTION_PLAYERCOINS];
        pokerAction.coinsOnTable = dict[Constants.POKERACTION_COINSONTABLE];
        pokerAction.moveType = (int)dict[Constants.POKERACTION_MOVETYPE];
        pokerAction.playerSlot = (int)dict[Constants.POKERACTION_PLAYERSLOT];
        pokerAction.betOrRaise = dict[Constants.POKERACTION_BETORRAISE];

        return pokerAction;
    }
}
