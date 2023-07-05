using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (!animator)
        {
            gameObject.SetActive(false);
            return;
        }

        animator.SetBool("On", false);
        animator.Play("Close", 0, 1);
    }

    public void OpenMenu()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (!animator)
        {
            gameObject.SetActive(true);
            return;
        }

        animator.SetBool("On", true);
    }

    public void CloseMenu()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (!animator)
        {
            gameObject.SetActive(false);
            return;
        }

        animator.SetBool("On", false);
    }
}
