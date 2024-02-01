using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour, IInteractable
{
    [Header("广播")]
    public VoidEventSO LoadGameEvent;
    [Header("变量参数")]
    public SpriteRenderer spriteRenderer;
    public GameObject lightObj;
    public Sprite darkSprite;
    public Sprite lightSprite;
    public bool isDone;
    private void OnEnable()
    {
        spriteRenderer.sprite = isDone ? lightSprite : darkSprite;
        lightObj.SetActive(isDone);
    }
    public void TriggerAction()
    {
        if (!isDone)
        {
            isDone = true;
            Debug.Log("Save point activated!");
            // GetComponent<AudioDefinition>().PlayAudioClip();
            spriteRenderer.sprite = lightSprite;
            lightObj.SetActive(true);
            this.gameObject.tag = "Untagged";
            //TODO: Save game
            LoadGameEvent.Raise();
        }
    }
}
