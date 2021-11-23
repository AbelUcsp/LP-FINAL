using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Events;
using UnityEngine.EventSystems;// Required when using Event data.

public class Speedometro : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public GameObject medidor;
    public Text KPH;
    public Text gearNum;
    public float InicioPos = 212.0f, FinPos = -36.0f;
    public float ActualPos;
    public AutoController _AutoController;
    public InputManager _inputManager;
    public float velocidad;
    void Start()
    {
        
    }
    
    
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        KPH.text = _AutoController.KPH.ToString("0");
        //velocidad = _AutoController.KPH;
        updateMedidor();
    }

    private void updateMedidor()
    {
        ActualPos = InicioPos - FinPos;
        //float tmp = velocidad / 180;
        float tmp = _AutoController.motorRPM /12000;
        medidor.transform.eulerAngles = new Vector3(0, 0, (InicioPos - (tmp * ActualPos)));
    }

    public void updateGear()
    {
        gearNum.text = (!_AutoController._reversa) ? (_AutoController.gearNum + 1).ToString() : "R";
    }

    public void aceleracionTrue()
    {
        _inputManager._vertical = 1;
    }

    public void aceleracionFlase()
    {
        _inputManager._vertical = 0;
    }
    public void DriftTrue()
    {
        _inputManager.brake = true;
    }

    public void DriftFlase()
    {
        _inputManager.brake = false;
    }
    
    
    /*
    
    public void OnPointerEnter(PointerEventData p)
    {
        _inputManager._vertical = 1;
        Debug.Log("entern");
    }

    public void OnPointerExit(PointerEventData p)
    {
        _inputManager._vertical = 0;
        Debug.Log("exit");
    }
    
    
    public void OnPointerUp (PointerEventData eventData)
    {
        //aceleracionTrue();
        _inputManager._vertical = 0;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //aceleracionFlase();
        _inputManager._vertical = 1;
    }
*/
   
    
}
