using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private AutoController _autoController;
    public GameObject Player;
    public GameObject Camera;
    public GameObject LookAt;
    public float speed;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        Camera = Player.transform.Find("camera").gameObject;
        LookAt = Player.transform.Find("LookAt").gameObject;
        _autoController = Player.GetComponent<AutoController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        follow();
        speed = (_autoController.velocidad >= 50) ? 20 : _autoController.velocidad / 4;
       
    }

    private void follow()
    {

        speed = Mathf.Lerp(speed, _autoController.velocidad / 4, Time.deltaTime);
        gameObject.transform.position =
            Vector3.Lerp(transform.position, Camera.transform.position, Time.deltaTime * speed);
        gameObject.transform.LookAt(LookAt.gameObject.transform.position);
    }
}
