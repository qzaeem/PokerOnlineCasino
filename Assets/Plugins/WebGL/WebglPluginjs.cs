using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
  
public static class WebglPluginjs 
{
    //// Importing "CallFunction"
    //[DllImport("__Internal")]
    //public static extern void CallFunction();
    //// Importing "PassTextParam"
    [DllImport("__Internal")]
    public static extern void PassTextParam(string text);  
    //// Importing "PassNumberParam"
    //[DllImport("__Internal")]
    //public static extern void PassNumberParam(int number);
    //// Importing "GetTextValue"
    //[DllImport("__Internal")]
    //public static extern string GetTextValue();
    //// Importing "GetNumberValue"
    //[DllImport("__Internal")]
    //public static extern int GetNumberValue();
    [DllImport("__Internal")]
     public static extern string GetURLFromPage();  
    [DllImport("__Internal")]
     public static extern string GetURLFromIframe(string iframe);
    [DllImport("__Internal")]
    public static extern string getBrowserData();      
 }    
