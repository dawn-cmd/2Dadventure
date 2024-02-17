using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataDefinition : MonoBehaviour
{
    public PersistentType persistentType;
    public string ID;
    private void OnValidate() {
        if (persistentType == PersistentType.DoNotPersist) {
            ID = string.Empty;
            return;
        }
        if (string.IsNullOrEmpty(ID)) {
            ID = System.Guid.NewGuid().ToString();
        }
    }
}
