using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAcontroller : MonoBehaviour
{
    public float velocidad = 7.0f;
    public Transform objetivo;
    public int metaa;
    void Start()
    {
        
        transform.LookAt(new Vector3(objetivo.position.x, transform.position.y, objetivo.position.z));
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(0,0,velocidad*Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.tag.CompareTo(waypoints))
        if (other.tag == "waypoints")
        {
            objetivo = other.GetComponent<waypoints>().siguiente;
            transform.LookAt(new Vector3(objetivo.position.x, transform.position.y, objetivo.position.z));
        }
        if (other.tag == "meta")
        {
            //objetivo = other.GetComponent<waypoints>().siguiente;
            // transform.LookAt(new Vector3(objetivo.position.x, transform.position.y, objetivo.position.z));
            metaa += 1;
        }
    }
}
