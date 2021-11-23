using UnityEngine;
using System;

public enum Tipo_eje
{
    Delantero,
    Trasero,
    Ambos
}


[Serializable]
public class Llanta 
{

    [Header("Propiedades")] 
    public GameObject modelo;
    public WheelCollider collider;
    public Tipo_eje eje;
    
    
    void Start()
    {
        
    }
    
    
    void Update()
    {
        
    }

}
