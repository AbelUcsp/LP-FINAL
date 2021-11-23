using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public float _vertical;
    public float _horizontal;
    public bool brake;
    void Start()
    {
        
    }


    private void FixedUpdate()
    {
        _horizontal = Input.GetAxis("Horizontal");
        _vertical = Input.GetAxis("Vertical");
    }

    



    void Update()
    {
        
    }
}
