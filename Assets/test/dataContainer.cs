using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using NetworkManagement;
using UnityEngine.SceneManagement;

public class dataContainer : MonoBehaviour
{
     // public TMP_InputField field;
    public Text _textIframeID;
    public Text _textURL;
    public Text _textUserID;
    public Text _textUserF_name;
    public Text _textUserL_name;
    public Text _textUserFull_name;
     // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetLocalStorage()
    {
        string getStorage = WebglPluginjs.getBrowserData();
        _textURL.text = "storage is : " + getStorage.ToString();
    }      
      
    public void CallFunction()
    {  
                     // field.text = "game_iframe";
                    // string _iframe = "game_iframe";
                    //_textIframeID.text = _iframe;  
                    // string getURL = WebglPluginjs.GetURLFromIframe(_iframe);  
                     string getURL = WebglPluginjs.GetURLFromPage();      
                    _textURL.text = "iframe : " + getURL.ToString();  
                    string id = "";   
                    string fname = "";
                    string lname = "";
                    string Fullname = "";
                 //   string url = "http://localhost/buildgetIframe?id=123444&fname=irfan&lname=shah";
                    string url = _textURL.text;   
                     // Parse the query string to get the values of id, fname, and lname
                    string[] parts = url.Split('?');   
                    if (parts.Length > 1)  
                    {  
                        string[] parameters = parts[1].Split('&');
                        foreach (string parameter in parameters)
                        {
                            print(parameter);
                            string[] nameValue = parameter.Split('=');

                            if (nameValue[0] == "id")
                            {
                                id = nameValue[1];
                                print("ID" + id);
                            }
                            else if (nameValue[0] == "fname")
                            {
                                fname = nameValue[1];
                                print("fname " + fname);

                            }
                            else if (nameValue[0] == "lname")
                            {
                                lname = nameValue[1];
                                print("lname " + lname);
                            }
                        }

                        // Now you can use the values of id, fname, and lname as needed
                    }
                      Fullname = fname + " " + lname;
                     _textUserID.text = id;  
                    _textUserF_name.text = fname;
                    _textUserL_name.text = lname;
                    _textUserFull_name.text = Fullname;     
                    //   SceneManager.LoadScene(1);  
      

    }

}
