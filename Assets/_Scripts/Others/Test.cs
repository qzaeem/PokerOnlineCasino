using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] private RectTransform rect1, rect2;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(CheckPos());
        GetUserInfo getUser = new GetUserInfo();
        getUser = getUser.GetdataFromClass("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjY0MjQyZDBkYzMxOWZmM2FmNDlhZTBkZCIsImlhdCI6MTY4MDA5MjQyOX0.FKbyGi_yVX4X4G5FCMgJmzFMZ4FbB7b69wjyLzuDoNI");
        string bodyJson = JsonUtility.ToJson(getUser);
        print(bodyJson);
    }

    IEnumerator CheckPos()
    {
        yield return new WaitForSeconds(1);
        float posX = (rect1.parent as RectTransform).anchoredPosition.x + rect1.anchoredPosition.x;
        float posY = (rect1.parent as RectTransform).anchoredPosition.y + rect1.anchoredPosition.y;
        print("Rect 1: " + new Vector2(posX, posY));
    }
}
