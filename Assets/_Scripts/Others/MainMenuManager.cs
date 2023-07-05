using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalCoinsTMP;

    private void OnEnable()
    {
        Globals.getCoinsEvent += UpdateTotalCoins;
    }
    private void OnDisable()
    {
        Globals.getCoinsEvent -= UpdateTotalCoins;
    }

    // Start is called before the first frame update
    private void Start()
    {
        totalCoinsTMP.text = "";
        if (!Globals.isTesting) StartCoroutine(GetCoins());
        else Globals.CreateDummyUserInfo();
    }

    private IEnumerator GetCoins()
    {
        yield return new WaitUntil(() => Globals.isSocketsConnected);
        GetBrowserData.instance.webConnectivety.GetCoins();
    }

    private void UpdateTotalCoins(double totalCoins)
    {
        Globals.userInfo.chips = totalCoins;
        totalCoinsTMP.text = totalCoins.ToString("0.00");
    }
}
