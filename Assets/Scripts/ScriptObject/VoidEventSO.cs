using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "VoidEventSO", menuName = "Event/VoidEventSO", order = 0)]
public class VoidEventSO : ScriptableObject {
    public UnityAction OnEventRaised;
    public void Raise() {
        OnEventRaised?.Invoke();
    }
}
