using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO3;
using BestHTTP.SocketIO3.Events;

public class WebConnectivety : MonoBehaviour
{
    [SerializeField] private GetBrowserData browserData;
    
    //WebSocket ws;
    public string mainURL;
    SocketManager manager;

    // Start is called before the first frame update
    void Start()
    {
        if (Globals.isTesting) return;
        print(BestHTTP.HTTPManager.UserAgent); 
         SocketOptions options = new SocketOptions();
        options.AutoConnect = false;
          manager = new SocketManager(new Uri(mainURL), options);
         //var root = manager.Socket;  
         manager.Socket.On("connect", () =>
         {
             UpdateUserInfo();
             Debug.Log(manager.Handshake.Sid);
         });
         manager.Open();
         manager.Socket.On<string>("CREATE_GAME", OnCreateGameReceived);
         manager.Socket.On<string>("ON_WIN", ONWINGameReceived);
         manager.Socket.On<string>("ON_LOSE", ONLoseGameReceived);
         manager.Socket.On<string>("CHECK_COINS", ONCheckCoinsReceived);
         manager.Socket.On<string>("ADD_COINS", ONADDCoinsReceived);  
         manager.Socket.On<string>("SUBTRACT_COINS", ONSUBTRACTCoinsReceived);  
         manager.Socket.On<string>("GET_USER_INFO", ONGETUSERINFOReceived);  
         manager.Socket.On<string>("GET_USER_BY_USERNAME ", ONGETINFOUSERNAMEReceived);  
     }   
      // Responses
    private void OnCreateGameReceived(string buffer)  
    {
        var result = JsonUtility.FromJson(buffer, typeof(ExtractingGameID)) as ExtractingGameID;
        Globals.gameID = result.game_id;
        Globals.OnCreatedGame(result.game_id);
        GetCoins();
    }     
    private void ONWINGameReceived(string buffer)
    {
        var result = JsonUtility.FromJson(buffer, typeof(ExtractingONWIN)) as ExtractingONWIN;
        Debug.Log($"<color=yellow>Win status: {result.status}</color>");
        Debug.Log("<color=green>Win Added: </color> " + result.chips);
        Debug.Log("<color=green>Total coins: </color> " + result.current_chips);
        if (result.status == "success") GetCoins();
    }
    private void ONLoseGameReceived(string buffer)
    {
        var result = JsonUtility.FromJson(buffer, typeof(ExtractingONLoose)) as ExtractingONLoose;
        if (result.status == "success") GetCoins();
    }
    private void ONCheckCoinsReceived(string buffer)
    {
        var coinsInfo = JsonUtility.FromJson(buffer, typeof(ExtractingCheckCoins)) as ExtractingCheckCoins;
        Globals.OnGetCoins(coinsInfo.chips);
     }
    private void ONADDCoinsReceived(string buffer)
    {
        var result = JsonUtility.FromJson(buffer, typeof(ExtractingAddCoins)) as ExtractingAddCoins;
        Debug.Log($"<color=yellow>Add status: {result.status}</color>");
        Debug.Log("<color=green>Coins Added: </color> " + result.chips);
        Debug.Log("<color=green>Total coins: </color> " + result.current_chips);
        if (result.status == "success") GetCoins();
    }
    private void ONSUBTRACTCoinsReceived(string buffer)
    {
        var result = JsonUtility.FromJson(buffer, typeof(ExtractingSubtractCoins)) as ExtractingSubtractCoins;
        Debug.Log($"<color=yellow>Subtract status: {result.status}</color>");
        Debug.Log("<color=green>Coins Subtracted: </color> " + result.chips);
        Debug.Log("<color=green>Total coins: </color> " + result.current_chips);
        if (result.status == "success") GetCoins();
    }
    private void ONGETUSERINFOReceived(string buffer)
    {
        var userInfo = JsonUtility.FromJson(buffer, typeof(ExtractingGetUserInfo)) as ExtractingGetUserInfo;
        Globals.userInfo = userInfo;
        browserData.PlayerID = userInfo._id;
        Globals.isSocketsConnected = true;
    }
    private void ONGETINFOUSERNAMEReceived(string buffer)
    {
        Debug.Log("output: " + buffer);
    }
          
    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            GetUserByUserName myObject = new GetUserByUserName();
            //TableCoins GetdataFromClass(string _playerID, int _chips)
            myObject = myObject.GetdataFromClass(GetBrowserData.instance.PublicUserName);
            string bodyJson = JsonUtility.ToJson(myObject);
            string mydata = "data:" + bodyJson;
            print(mydata);
            manager.Socket.Emit("GET_USER_BY_USERNAME", mydata);
        }
        */
    }

    public void CreateGame(string gameName)
    {
        CreatingGameID myObject = new CreatingGameID();
        myObject = myObject.GetdataFromClass(gameName);
        string bodyJson = JsonUtility.ToJson(myObject);
        manager.Socket.Emit("CREATE_GAME", bodyJson);
        Debug.Log("<color=yellow>Creating Game ID</color>");
    }

    public void UpdateUserInfo()
    {
        GetUserInfo myObject = new GetUserInfo();
        myObject = myObject.GetdataFromClass(GetBrowserData.instance.playerToken);
        string bodyJson = JsonUtility.ToJson(myObject);
        manager.Socket.Emit("GET_USER_INFO", bodyJson);
    }

    public void GetCoins()
    {
        CheckCoins myObject = new CheckCoins();
        myObject = myObject.GetdataFromClass(GetBrowserData.instance.PlayerID);
        string bodyJson = JsonUtility.ToJson(myObject);
        string mydata = "data:" + bodyJson;
        print("Check Coins");
        manager.Socket.Emit("CHECK_COINS", bodyJson);
    }

    public void SubtractCoins(double coins)
    {
        SubtractCoins myObject = new SubtractCoins();
        myObject = myObject.GetdataFromClass(Globals.gameID, GetBrowserData.instance.PlayerID, coins);
        string bodyJson = JsonUtility.ToJson(myObject);
        manager.Socket.Emit("SUBTRACT_COINS", bodyJson);
        Debug.Log("<color=yellow>Subtract JSON: </color>" + bodyJson);
        Debug.Log("<color=green>Subtract: </color>" + coins);
    }

    public void AddCoins(double coins)
    {
        AddCoins myObject = new AddCoins();
        myObject = myObject.GetdataFromClass(Globals.gameID, GetBrowserData.instance.PlayerID, coins);
        string bodyJson = JsonUtility.ToJson(myObject);
        manager.Socket.Emit("ADD_COINS", bodyJson);
    }

    public void AddWin(string gameID, double winningPool)
    {
        CreatingONWIN myObject = new CreatingONWIN();
        myObject = myObject.GetdataFromClass(gameID, GetBrowserData.instance.PlayerID, winningPool);
        string bodyJson = JsonUtility.ToJson(myObject);
        manager.Socket.Emit("ON_WIN", bodyJson);
    }

    public void AddLoss(string gameID, double lostAmount)
    {
        CreatingONLoose myObject = new CreatingONLoose();
        myObject = myObject.GetdataFromClass(gameID, GetBrowserData.instance.PlayerID, lostAmount);
        string bodyJson = JsonUtility.ToJson(myObject);
        manager.Socket.Emit("ON_LOSE", bodyJson);
    }
}

/// <Creating GameMode>
[System.Serializable]
public class CreatingGameID
{
    public String game_mode;
    public CreatingGameID GetdataFromClass(string name)
    {
        CreatingGameID myObj = new CreatingGameID();
        myObj.game_mode = name;
        return myObj;
    }
}
[System.Serializable]
public class ExtractingGameID
{
    public string game_id;
}
/// </GameMode




/// <ONWIN>

[System.Serializable]
public class CreatingONWIN
{
    public String game_id;
    public String player_id;
    public double wining_poll;
    public CreatingONWIN GetdataFromClass(string _gameid, string _playerID, double _winningPool)
    {
        CreatingONWIN myObj = new CreatingONWIN();
        myObj.game_id = _gameid;
        myObj.player_id = _playerID;
        myObj.wining_poll = _winningPool;
        return myObj;
    }
}

public class ExtractingONWIN
{
    public string status;
    public double chips;
    public double current_chips;
}

/// <END ONWIN>


/// <LOOSE>
[System.Serializable]
public class CreatingONLoose
{
    public String game_id;
    public String player_id;
    public double coins;
    public CreatingONLoose GetdataFromClass(string _gameid, string _playerID, double _loosingPool)
    {
        CreatingONLoose myObj = new CreatingONLoose();
        myObj.game_id = _gameid;
        myObj.game_id = _playerID;
        myObj.coins = _loosingPool;
        return myObj;
    }
}
public class ExtractingONLoose
{
    public string status;
}
/// <END ONLOOSE>

/// <CheckCoins>
[System.Serializable]
public class CheckCoins
{
    public String player_id;
    public CheckCoins GetdataFromClass(string _playerID)
    {
        CheckCoins myObj = new CheckCoins();
        myObj.player_id = _playerID;
        return myObj;
    }
}

public class ExtractingCheckCoins
{
    public string _id;
    public double chips;
}
/// <END CheckCoins>


/// <AddCoins>
[System.Serializable]
public class AddCoins
{
    public String game_id;
    public String player_id;
    public double chips;
    public AddCoins GetdataFromClass(string _gameID, string _playerID, double _chips)
    {
        AddCoins myObj = new AddCoins();
        myObj.game_id = _gameID;
        myObj.player_id = _playerID;
        myObj.chips = _chips;
        return myObj;
    }
}

public class ExtractingAddCoins
{
    public string status;
    public double chips;
    public double current_chips;
}

/// <END AddCoins>

/// <SubtractCoins>
[System.Serializable]
public class SubtractCoins
{
    public String game_id;
    public String player_id;
    public double chips;
    public SubtractCoins GetdataFromClass(string _gameID, string _playerID, double _chips)
    {
        SubtractCoins myObj = new SubtractCoins();
        myObj.game_id = _gameID;
        myObj.player_id = _playerID;
        myObj.chips = _chips;
        return myObj;
    }
}
public class ExtractingSubtractCoins
{
    public string status;
    public double chips;
    public double current_chips;
}
/// <END SubtractCoins>


/// <GetUserInfo>

[System.Serializable]
public class GetUserInfo
{
    public String token;
    public GetUserInfo GetdataFromClass(string _playerID)
    {
        GetUserInfo myObj = new GetUserInfo();
        myObj.token = _playerID;
        return myObj;
    }
}
public class ExtractingGetUserInfo
{
    public string profile_image;
    public string _id;
    public string first_name;
    public string last_name;
    public string email;
    public string username;
    public string login_type;
    public string social_id;
    public string role;
    public double wins;
    public double loose;
    public double chips;
    public string __v;
}

/// <END GetUserInfo>


/// <GET_USER_BY_USERNAME>

[System.Serializable]
public class GetUserByUserName
{
    public String username;
    public GetUserByUserName GetdataFromClass(string _username)
    {
        GetUserByUserName myObj = new GetUserByUserName();
        myObj.username = _username;
        return myObj;
    }
}
public class ExtractingGetUserByUserName
{
    public string profile_image;
    public string _id;
    public string first_name;
    public string last_name;
}

/// <END GET_USER_BY_USERNAME>
