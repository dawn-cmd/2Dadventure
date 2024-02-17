using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISavable
{
    DataDefinition GetDataID();
    void RegisterSaveDate() => DataManager.instance.RegisterSaveData(this);
    void UnRegisterSaveDate() => DataManager.instance.UnRegisterSaveData(this);
    void GetSaveData(Data data);
    void LoadData(Data data);
}
