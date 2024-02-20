using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data
{
    public string sceneToSave;
    public Dictionary<string, SerializeVector3> characterPosDist = new();
    public Dictionary<string, float> floatSavedData = new();
    public Dictionary<string, bool> interactableSavedData = new();
    public void SaveGameScene(GameSceneSO scene)
    {
        sceneToSave = JsonUtility.ToJson(scene);
        Debug.Log(sceneToSave);
    }
    public GameSceneSO GetSavedScene() 
    {
        var newScene = ScriptableObject.CreateInstance<GameSceneSO>();
        JsonUtility.FromJsonOverwrite(sceneToSave, newScene);
        return newScene;
    }
}
public class SerializeVector3 {
    public float x;
    public float y;
    public float z;
    public SerializeVector3(Vector3 vec) {
        x = vec.x;
        y = vec.y;
        z = vec.z;
    }
    public Vector3 ToVector3() {
        return new Vector3(x, y, z);
    }
}
