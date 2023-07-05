using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using NetworkManagement;
using UnityEngine.SceneManagement;

public class GetBrowserData : MonoBehaviour
{
    public static GetBrowserData instance;
    public string playerToken;
    public string PlayerID;
    public string PublicUserName;
    public string PlayerName;
  //  public Text text1;
  //  public Text textID;
  //  public Text textName;
    public bool IsLive;

    [HideInInspector] public WebConnectivety webConnectivety;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        webConnectivety = GetComponent<WebConnectivety>();
         instance = this;
         DontDestroyOnLoad(gameObject);    
    } 
    // Start is called before the first frame update
    void Start()
    {
        Globals.isSocketsConnected = Globals.isTesting;

       if(Application.platform == RuntimePlatform.WebGLPlayer && IsLive)
        {
            StartCoroutine(miniWait());
         } 
       else
        {
            webConnectivety.enabled = true;
        }
    }     
    IEnumerator miniWait()
    {
        yield return new WaitForSeconds(.1f);
        GetLocalStorage();
    }
    public void GetLocalStorage()
    {
        string getStorage = WebglPluginjs.getBrowserData();
         ExtractingbrowserData dataobj = new ExtractingbrowserData();
        dataobj = JsonUtility.FromJson<ExtractingbrowserData>(getStorage);  
        Debug.Log("token: " + dataobj.token);
        Debug.Log("first name: " + dataobj.first_name);
        Debug.Log("last name : " + dataobj.last_name);
         playerToken = dataobj.token;
        PlayerName = dataobj.first_name + " " + dataobj.last_name;
        //  text1.text = "storage is : " + getStorage.ToString();     
        //textID.text = PlayerID;
        //textName.text = PlayerName;
         webConnectivety.enabled = true;  
    }                            
     [System.Serializable]
    public class ExtractingbrowserData
    {
        public string token;
        public string first_name;
        public string last_name;
    }  
  }  
