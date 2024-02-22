using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "FloatEventSO", menuName = "Event/FloatEventSO", order = 0)]
public class FloatEventSO : ScriptableObject {
    public UnityAction<float> OnEventRaised;
    public void Raise(float amount) {
        OnEventRaised?.Invoke(amount);
    }
}
