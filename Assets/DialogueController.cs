using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueController : MonoBehaviour
{

    public TextMeshProUGUI DialogueText;

    public string[] sentences;

    private int Index = 0;

    public float DialogueSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnInteractPress(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            NextSentence();
        }
    }

    void NextSentence()
    {
        if (Index<=sentences.Length-1)
        {
            DialogueText.text = "";
            StartCoroutine(WriteSentence());
        }
    }
    IEnumerator WriteSentence()
    {
        foreach (char Character in sentences[Index].ToCharArray())
        {
            DialogueText.text += Character;
            yield return new WaitForSeconds(DialogueSpeed);
        }

        Index++;
    }
}
