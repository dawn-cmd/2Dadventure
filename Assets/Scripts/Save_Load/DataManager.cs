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
        ReadSavedData();
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
        Debug.Log("Saving");
        Debug.Log(savableList.Count);
        foreach (var savable in savableList)
        {
            Debug.Log(savable.GetDataID().ID + " is saving!");
            savable.GetSaveData(saveData);
        }
        Debug.Log("Saved");
        var resultPath = jsonFolder + "SaveData.sav";
        var jsonData = JsonConvert.SerializeObject(saveData);
        if (!File.Exists(resultPath))
        {
            Directory.CreateDirectory(jsonFolder);
        }
        File.WriteAllText(resultPath, jsonData);
    }
    public void Load()
    {
        Debug.Log("Loading");
        Debug.Log(savableList.Count);
        foreach (var savable in savableList)
        {
            Debug.Log(savable.GetDataID().ID + " is loading!");
            savable.LoadData(saveData);
        }
        Debug.Log("Loaded");
    }
    private void ReadSavedData()
    {
        var resultPath = jsonFolder + "SaveData.sav";
        if (File.Exists(resultPath))
        {
            var jsonData = File.ReadAllText(resultPath);
            saveData = JsonConvert.DeserializeObject<Data>(jsonData);
        }
    }
}
