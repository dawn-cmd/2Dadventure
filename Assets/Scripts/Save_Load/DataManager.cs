using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour {
    public static DataManager instance;
    public List<ISavable> savableList = new List<ISavable>();
    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(this.gameObject);
        }
    }
    public void RegisterSaveData(ISavable savable) {
        if (savableList.Contains(savable)) {
            return;
        }
        savableList.Add(savable);
    }
    public void UnRegisterSaveData(ISavable savable) {
        savableList.Remove(savable);
    }
}
