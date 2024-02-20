using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json;

[DefaultExecutionOrder(-100)]
public class DataManager : MonoBehaviour
{
    public static DataManager instance;
    [Header("Event Capture")]
    public VoidEventSO saveGameEvent;
    public VoidEventSO loadGameEvent;
    public List<ISavable> savableList = new List<ISavable>();
    private Data saveData;
    private string jsonFolder;
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
        jsonFolder = Application.persistentDataPath + "/SaveData/";
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
        var resultPath = jsonFolder + "SaveData.sav";
        var jsonData = JsonConvert.SerializeObject(saveData);
    }
    public void Load()
    {
        foreach (var savable in savableList)
        {
            savable.LoadData(saveData);
        }
    }
}
