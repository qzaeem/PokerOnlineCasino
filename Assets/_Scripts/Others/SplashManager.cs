using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LoadingCanvas.instance.ShowLoading(false);
        StartCoroutine(LoadMainScene());
    }
    
    IEnumerator LoadMainScene()
    {
        yield return new WaitForSeconds(5);
        yield return new WaitUntil(() => Globals.isSocketsConnected);
        SceneManager.LoadScene(Constants.MAIN_SCENE_NAME);
    }
}   
