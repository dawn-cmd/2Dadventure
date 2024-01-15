using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;

public class Sign : MonoBehaviour
{
    private PlayerInputControl playerInput;
    private Animator anim;
    public Transform playerTrans;
    public GameObject signSprite;
    private IInteractable objItem;
    private bool canPress;
    private void Awake()
    {
        anim = signSprite.GetComponent<Animator>();
        playerInput = new PlayerInputControl();
        playerInput.Gameplay.Confirm.started += OnConfirm;
        playerInput.Enable();
    }

    private void OnConfirm(InputAction.CallbackContext context)
    {
        if (canPress)
        {
            objItem.TriggerAction();
        }
    }

    private void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
    }

    private void OnDisable()
    {
        canPress = false;
    }
    private void OnActionChange(object obj, InputActionChange actionChange)
    {
        if (actionChange == InputActionChange.ActionStarted)
        {
            // Debug.Log(((InputAction)obj).activeControl.device);
            var d = ((InputAction)obj).activeControl.device;
            switch (d.device)
            {
                case Keyboard:
                    anim.Play("keyboard");
                    break;
                case XInputController:
                    anim.Play("xbox");
                    break;
            }
        }
    }

    private void Update()
    {
        signSprite.GetComponent<SpriteRenderer>().enabled = canPress;
        signSprite.transform.localScale = playerTrans.localScale;
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            canPress = true;
            objItem = other.GetComponent<IInteractable>();
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        canPress = false;
    }
}

