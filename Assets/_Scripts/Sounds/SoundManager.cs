using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioClip[] bet;
    [SerializeField] private AudioClip cardFlip, takePot, check, fold;

    private AudioSource audioSource;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayActionSound(Globals.MoveType action)
    {
        switch (action)
        {
            case Globals.MoveType.Check:
                PlayCheckSound();
                break;
            case Globals.MoveType.Call:
                PlayBetSound();
                break;
            case Globals.MoveType.Bet:
                PlayBetSound();
                break;
            case Globals.MoveType.Raise:
                PlayBetSound();
                break;
            case Globals.MoveType.Fold:
                PlayFoldSound();
                break;
            case Globals.MoveType.AllIn:
                PlayBetSound();
                break;
            default:
                break;
        }
    }

    public void PlayBetSound() => audioSource.PlayOneShot(bet[Random.Range(0, bet.Length)]);
    public void PlayCardFlipSound() => audioSource.PlayOneShot(cardFlip);
    public void PlayTakePotSound() => audioSource.PlayOneShot(takePot);
    public void PlayCheckSound() => audioSource.PlayOneShot(check);
    public void PlayFoldSound() => audioSource.PlayOneShot(fold);
}
