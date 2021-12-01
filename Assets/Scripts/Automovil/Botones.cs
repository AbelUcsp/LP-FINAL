using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;// Required when using Event data.

public class Botones : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    //public AutoController _AutoController;
    public InputManager _inputManager;
    public string _tipoBtn;
    void Start()
    {
        
    }

    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_tipoBtn == "adelante")
        {
            _inputManager._vertical = 1;
        }
        else if (_tipoBtn == "atras")
        {
            _inputManager._vertical = -1;
        }
        else if (_tipoBtn == "drift")
        {
            
            _inputManager.brake = true;
        }
        //_inputManager._vertical = 1;
    }
    
    
    public void OnPointerUp (PointerEventData eventData)
    {
        if (_tipoBtn == "adelante")
        {
            _inputManager._vertical = 0;
        }
        else if (_tipoBtn == "atras")
        {
            _inputManager._vertical = 0;
        }
        else if (_tipoBtn == "drift")
        {
            _inputManager.brake = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        //_inputManager._horizontal = _inputManager._horizontal * AutoController.steervalue;
        //_inputManager._horizontal = Input.acceleration.x * AutoController.steervalue;
    }
}
