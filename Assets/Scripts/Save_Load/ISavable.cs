using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISavable
{
    void RegisterSaveDate() => DataManager.instance.RegisterSaveData(this);
    void UnRegisterSaveDate() => DataManager.instance.UnRegisterSaveData(this);
    void GetSaveData(Data data);
    void LoadDate(Data data);
}
