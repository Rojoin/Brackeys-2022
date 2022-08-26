using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TPPlayer : MonoBehaviour
{
    [SerializeField] private Transform targetPos;

    private bool playerInPortal = false;
    private Transform charTransform;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void TeleportPlayer(InputAction.CallbackContext context)
    {
        if (playerInPortal && context.performed)
            charTransform.position = targetPos.position;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform.CompareTag("Player"))
        {
            playerInPortal = true;
            charTransform = col.transform;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.transform.CompareTag("Player"))
        {
            playerInPortal = false;
            charTransform = null;
        }
    }
}
