using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CamaraYSteer : MonoBehaviour
{

    public Text steer;
    public AutoController _inputManager;
    public Botones _botones;
    public Camera inter;
    public Camera _out;
    void Start()
    {
        
    }

    public void mas()
    {
        AutoController.steervalue += 0.5f;
        
    }
    
    public void menos()
    {

        
        AutoController.steervalue -= 0.5f;
    }

    public void cambiarCamara()
    {
        
    }
    
    void Update()
    {
        //Debug.Log(AutoController.steervalue);
        steer.text = AutoController.steervalue.ToString();
        
    }
}
