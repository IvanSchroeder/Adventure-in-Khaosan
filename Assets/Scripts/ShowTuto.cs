using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowTuto : MonoBehaviour
{
    public GameObject TextoTuto;

    // Start is called before the first frame update
    void Start()
    {
        TextoTuto.SetActive(false);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        TextoTuto.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TextoTuto.SetActive(false);
    }
}
