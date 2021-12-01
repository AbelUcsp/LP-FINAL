using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public float _vertical;
    public float _horizontal;
    public float stearr = 60.0f;

    public float rotation;
    public bool brake;
    void Start()
    {
        
    }


    private void FixedUpdate()
    {
        _horizontal = Input.GetAxis("Horizontal");
        _vertical = Input.GetAxis("Vertical");
        brake = Convert.ToBoolean(Input.GetAxis("Jump"));
        rotation = Input.GetAxis("Horizontal") * 100.0f;
    }

    



    void Update()
    {
        
    }
}
