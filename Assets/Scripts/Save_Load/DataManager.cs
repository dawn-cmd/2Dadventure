using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;
    [Header("Event Capture")]
    public VoidEventSO saveGameEvent;
    public VoidEventSO loadGameEvent;
    public List<ISavable> savableList = new List<ISavable>();
    private Data saveData;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        saveData = new Data();
    }
    private void OnEnable()
    {
        saveGameEvent.OnEventRaised += Save;
        loadGameEvent.OnEventRaised += Load;
    }
    private void OnDisable()
    {
        saveGameEvent.OnEventRaised -= Save;
        loadGameEvent.OnEventRaised -= Load;
    }
    private void Update()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            Load();
        }
    }
    public void RegisterSaveData(ISavable savable)
    {
        if (savableList.Contains(savable))
        {
            return;
        }
        savableList.Add(savable);
    }
    public void UnRegisterSaveData(ISavable savable)
    {
        savableList.Remove(savable);
    }
    public void Save()
    {
        foreach (var savable in savableList)
        {
            savable.GetSaveData(saveData);
        }

        foreach (var item in saveData.characterPosDist)
        {
            Debug.Log(item.Key + " " + item.Value);
        }
    }
    public void Load()
    {
        foreach (var savable in savableList)
        {
            savable.LoadData(saveData);
        }
    }
}
