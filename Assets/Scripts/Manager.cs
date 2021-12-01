using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    public AutoController controlador;
    public GameObject vancas;
    public Text texto;
    public int cantidad;
    public IAcontroller IAcar;
    public AudioSource fondo;
    void Start()
    {
        
    }

    
    void Update()
    {
        if (controlador.metaa >= cantidad)
        {
            vancas.SetActive(true);
            Time.timeScale = 0.0F;
            texto.text = "Ganaste";
           // fondo.clip. 
        }
        else if (IAcar.metaa>=cantidad)
        {
            vancas.SetActive(true);
            Time.timeScale = 0.0F;
            texto.text = "Perdiste";
        }
    }
    
    
    
}
