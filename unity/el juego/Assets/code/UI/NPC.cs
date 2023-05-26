using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPC : MonoBehaviour
{
    public GameObject dialogo;
    public TMP_Text DialogoTexto;
    public string[] nuevo_dialogo;
    private int indice;
    public float wordSpeed;
    public bool playerIsClose;
    public GameObject contButton;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && playerIsClose)
        {
            if(dialogo.activeInHierarchy)
            {
                zeroText();
            }
            else
            {
                dialogo.SetActive(true);
                StartCoroutine(Typing());
            }
        }

        if(DialogoTexto.text == nuevo_dialogo[indice])
        {
            contButton.SetActive(true);
        }
    }


    public void zeroText()
    {
        DialogoTexto.text = "";
        indice = 0;
        dialogo.SetActive(false);
    }

    IEnumerator Typing()
    {
        foreach(char letter in nuevo_dialogo[indice].ToCharArray())
        {
           DialogoTexto.text += letter;
           yield return new WaitForSeconds(wordSpeed); 
        }
    }

    public void NextLine()
    {
        contButton.SetActive(false);
        if (indice < nuevo_dialogo.Length - 1)
        {
            indice++;
            DialogoTexto.text = "";
            StartCoroutine(Typing());
        }
        else
        {
            zeroText();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player"))
        {
            playerIsClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
            zeroText();
        }
    }
    
}
