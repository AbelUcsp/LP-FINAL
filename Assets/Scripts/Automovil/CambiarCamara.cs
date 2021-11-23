using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CambiarCamara : MonoBehaviour
{

    public GameObject interiorCamara;
    public GameObject exteriorCamara;
    public bool cambiarCamara = false;
    void Start()
    {
        
    }

    
    
    void Update()
    {
        
    }

    public void _CambiarCamara()
    {
        if (cambiarCamara)
        {
            interiorCamara.SetActive(true);
            exteriorCamara.SetActive(false);
            cambiarCamara = false;
        }
        else
        {
            interiorCamara.SetActive(false);
            exteriorCamara.SetActive(true);
            cambiarCamara = true;
        }
            
    }
    
    
}
