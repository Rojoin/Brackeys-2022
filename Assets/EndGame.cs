using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{

    [SerializeField] private Transform targetPos;

    private bool playerInPortal = false;
    private Transform charTransform;

    public void ChangeScene(InputAction.CallbackContext context)
    {
        if (playerInPortal && context.performed)
            SceneManager.LoadScene("EndScreen");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform.CompareTag("Player"))
        {
            playerInPortal = true;
            
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.transform.CompareTag("Player"))
        {
            playerInPortal = false;
        
        }
    }
}

