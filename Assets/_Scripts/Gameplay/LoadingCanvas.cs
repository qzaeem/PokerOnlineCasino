using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCanvas : MonoBehaviour
{
    public static LoadingCanvas instance;

    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingTMP;

    public bool IsLoading { get { if (loadingPanel != null) return loadingPanel.activeSelf; else return false; } }

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowLoading(bool show, string txt = "")
    {
        loadingPanel.SetActive(show);
        loadingTMP.text = txt;
    }
}
