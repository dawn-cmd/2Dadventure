using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable, ISavable
{
    private SpriteRenderer spriteRenderer;
    public Sprite openSprite;
    public Sprite closedSprite;
    public bool isDone;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        ISavable savable = this;
        savable.RegisterSaveDate();
        Debug.Log("Chest is registered!");
        spriteRenderer.sprite = isDone ? openSprite : closedSprite;
    }
    private void OnDisable()
    {
        ISavable savable = this;
        savable.UnRegisterSaveDate();
        Debug.Log("Chest is unregistered!");
    }
    public void TriggerAction()
    {
        if (!isDone)
        {
            Debug.Log("Chest opened!");
            GetComponent<AudioDefinition>().PlayAudioClip();
            OpenChest();
        }
    }

    private void OpenChest()
    {
        spriteRenderer.sprite = openSprite;
        isDone = true;
        this.gameObject.tag = "Untagged";
    }

    public DataDefinition GetDataID()
    {
        return GetComponent<DataDefinition>();
    }

    public void GetSaveData(Data data)
    {
        if (data.interactableSavedData.ContainsKey(GetDataID().ID))
        {
            data.interactableSavedData[GetDataID().ID] = isDone;
        }
        else
        {
            data.interactableSavedData.Add(GetDataID().ID, isDone);
        }
    }

    public void LoadData(Data data)
    {
        Debug.Log("Loading chest data");
        if (!data.interactableSavedData.ContainsKey(GetDataID().ID)) { return; }
        isDone = data.interactableSavedData[GetDataID().ID];
        if (isDone)
        {
            OpenChest();
            Debug.Log("Chest opened by initialized!");
        }
    }
}
