using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;
using System;
public class CameraControl : MonoBehaviour
{
    private CinemachineConfiner2D confiner2D;
    public CinemachineImpulseSource impulseSource;
    public VoidEventSO cameraShakeEvent;
    public VoidEventSO afterSceneLoadedEvent;
    private void Awake()
    {
        confiner2D = GetComponent<CinemachineConfiner2D>();
    }
    private void OnEnable()
    {
        cameraShakeEvent.OnEventRaised += OnCameraShakeEvent;
        afterSceneLoadedEvent.OnEventRaised += OnAfterSceneLoadedEvent;
    }

    private void OnAfterSceneLoadedEvent()
    {
        GetNewCameraBounds();
    }

    private void OnDisable()
    {
        cameraShakeEvent.OnEventRaised -= OnCameraShakeEvent;
        afterSceneLoadedEvent.OnEventRaised -= OnAfterSceneLoadedEvent;
    }

    private void OnCameraShakeEvent()
    {
        impulseSource.GenerateImpulse();
    }

    // private void Start()
    // {
    //     GetNewCameraBounds();
    // }
    private void GetNewCameraBounds()
    {
        var obj = GameObject.FindGameObjectWithTag("Bounds");
        if (obj == null) return;
        confiner2D.InvalidateCache();
        confiner2D.m_BoundingShape2D = obj.GetComponent<Collider2D>();
    }
}
