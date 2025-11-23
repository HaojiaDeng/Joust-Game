using UnityEngine;

[System.Serializable]
public class QTEInput
{
    public KeyCode requiredKey;
    public float windowDuration;
    public float delayBeforeThisPrompt;

    [Header("Optional Visual/Audio")]
    public Sprite promptIcon;
    public AudioClip promptSound;
}