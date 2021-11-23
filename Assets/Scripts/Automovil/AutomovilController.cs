
using System;
using System.Collections.Generic;
using UnityEngine;


public class AutomovilController : MonoBehaviour
{
    [Header("Vehicle Setup")] //public float algo;

    public TrailRenderer[] MarcasDrift;
    private bool _marcaFlag;


    private bool handbrake;
    public float fwdInput, backInput, horizontalInput;
    public float topSpeed;
    //public float currSpeed;
    
    private float tempo;   	//wheelSpin pointer 
    public float handBrakeFrictionMultiplier = 2;
    //public  float frictionMultiplier = 3f;
    private float handBrakeFriction  = 0.05f;
    //end Drift

    
 
    public bool Frenando;

    public bool adjust;
    
    
    [Range(0, 1)] [SerializeField] private float _AyudaTimon; // 0 is raw physics , 1 the car will grip in the direction it is facing
    //[Range(0, 2)] [SerializeField] private float _ControlDtraccion ;
    private Rigidbody _rb;
    private Transform _transform;
    private float _RotacionAntigua;
    
    [Range(0, 2)] [SerializeField] private float _TraccionControl; // 0 is no traction control, 1 is full interference
    
    
    //private float _TorqueActual;
    public float velocidad;
    [SerializeField] private float m_SlipLimit = 0.3f;
    private float m_CurrentTorque;
    private float driftFactor;
    public bool playPauseSmoke = false;
    //public float handBrakeFrictionMultiplier = 2f;
    public float KPH;
    
    [SerializeField] public float _TorqueEnRuedas = 2500.0f;
    [Range(0.5f, 1.5f)] [SerializeField] public float grip;
    [SerializeField] private float _AceleracionMax = 20.0f;
    [SerializeField] private float _SensibilidadGiro = 1.0f;
    [SerializeField] private float _AnguloDireccionMax = 45.0f;
    [SerializeField] private Vector3 _CentroMasa;
    [SerializeField] private  List<Llanta> wheels;
    
    //Spin lateral
    [SerializeField] private WheelFrictionCurve _sidewaysFriction;
    [SerializeField] private WheelFrictionCurve _forwardFriction;
    
    

    //Inputs
    private float inputX;
    private float inputY;
    private bool inputSpace;

    private void Inputs(){
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");
        
        handbrake = Convert.ToBoolean(Input.GetAxisRaw("Jump"));
        
        inputSpace = Input.GetKeyDown(KeyCode.F);
        //VelZ = inputY;
    }
    
    private void AnimacionRuedas()
    {
        foreach (var wheel in wheels)
        {
            Quaternion _root;
            Vector3 _pos;
            wheel.collider.GetWorldPose(out _pos, out _root);
            wheel.modelo.transform.position = _pos;
            wheel.modelo.transform.rotation = _root;
        }
    }
    
    private void MovimientoAuto()
    {
        foreach (var wheel in wheels)
        {
            wheel.collider.motorTorque = inputY * _AceleracionMax * 500 * Time.deltaTime;
        }
        KPH = _rb.velocity.magnitude * 3.6f;
    }

    
   
    
    private void Freno()
    {
        
    }
    private void DireccionAuto()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.eje == Tipo_eje.Delantero)
            {
                var anguloDireccion = inputX * _SensibilidadGiro * _AnguloDireccionMax;
                wheel.collider.steerAngle = Mathf.Lerp(wheel.collider.steerAngle,anguloDireccion,0.5f);
            }
        }
    }
    
    private void AyudanteTimon()
    {
        foreach (var wheel in wheels)
        {
            WheelHit wheelhit;
            wheel.collider.GetGroundHit(out wheelhit);
            if (wheelhit.normal == Vector3.zero)
                return; // las ruedas no están en el suelo, así que no realinee la velocidad del cuerpo rígido
        }

        // esto si es necesario para evitar problemas de bloqueo del cardán que harán
        // que el automóvil cambie de dirección repentinamente
        if (Mathf.Abs(_RotacionAntigua - transform.eulerAngles.y) < 10f)
        {
            var turnadjust = (transform.eulerAngles.y - _RotacionAntigua) * _AyudaTimon;
            Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            _rb.velocity = velRotation * _rb.velocity;
        }
        _RotacionAntigua = transform.eulerAngles.y;
    }

    //control de tracción crudo que reduce la potencia a la rueda si el coche gira demasiado
    private void TractionControl() {
        WheelHit wheelHit;
        //CarDriveType.RearWheelDrive:          2 y 3
        wheels[0].collider.GetGroundHit(out wheelHit);
        AdjustTorque(wheelHit.forwardSlip);
        

        wheels[1].collider.GetGroundHit(out wheelHit);
        AdjustTorque(wheelHit.forwardSlip);
    }
    
    private void AdjustTorque(float forwardSlip)
    {
        if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0)
        {
            m_CurrentTorque -= 10 * _TraccionControl;
        }
        else
        {
            m_CurrentTorque += 10 * _TraccionControl;
            if (m_CurrentTorque > _TorqueEnRuedas)
            {
                m_CurrentTorque = _TorqueEnRuedas;
            }
        }
    }//end traccion
    
    
     private void adjustTraction(){
            //tine it takes to go from normal drive to drift 
        float driftSmothFactor = 0.7f * Time.deltaTime;

        
		if(handbrake){
            _sidewaysFriction = wheels[0].collider.sidewaysFriction;
            _forwardFriction = wheels[0].collider.forwardFriction;

            float velocity = 0;
            _sidewaysFriction.extremumValue = _sidewaysFriction.asymptoteValue = _forwardFriction.extremumValue =  _forwardFriction.asymptoteValue =
                        Mathf.SmoothDamp(_forwardFriction.asymptoteValue, driftFactor * handBrakeFrictionMultiplier, ref velocity, driftSmothFactor);
            
        /*
            else
            {

                _forwardFriction.extremumValue = _forwardFriction.asymptoteValue = _sidewaysFriction.extremumValue =
                    _sidewaysFriction.asymptoteValue =
                        ((KPH * handBrakeFrictionMultiplier) / 300) + grip; ///resetear la friccion lateral/frontal sobre la velocidad
                                                                            /// de vehiculo y al final agregamos GRIP (sujecion)

            }
*/
            for (int i = 0; i < wheels.Count; i++) {
                wheels[i].collider.sidewaysFriction = _sidewaysFriction;
                wheels[i].collider.forwardFriction = _forwardFriction;
            }

            _sidewaysFriction.extremumValue = _sidewaysFriction.asymptoteValue = _forwardFriction.extremumValue = _forwardFriction.asymptoteValue =  1.1f;
                //extra grip for the front wheels
             
            for (int i = 0; i < 2; i++)
            {
                wheels[i].collider.sidewaysFriction = _sidewaysFriction;
                wheels[i].collider.forwardFriction = _forwardFriction;
            }
            _rb.AddForce(transform.forward*(velocidad/400)*40000);
        }

                //}
        
        else{

			//forwardFriction = wheels[0].forwardFriction;
			//sidewaysFriction = wheels[0].sidewaysFriction;
            _sidewaysFriction = wheels[0].collider.sidewaysFriction;
            _forwardFriction = wheels[0].collider.forwardFriction;

            _forwardFriction.extremumValue = _forwardFriction.asymptoteValue = _sidewaysFriction.extremumValue = _sidewaysFriction.asymptoteValue = 
                ((KPH * handBrakeFrictionMultiplier) / 300) + grip;

			for (int i = 0; i < wheels.Count; i++) {
				wheels[i].collider.forwardFriction = _forwardFriction;
				wheels[i].collider.sidewaysFriction = _sidewaysFriction;

			}
        
        }
        
        
            //checks the amount of slip to control the drift
		for(int i = 2; i<wheels.Count ; i++){

            WheelHit wheelHit;

            wheels[i].collider.GetGroundHit(out wheelHit);
                //smoke
                /*
            if(wheelHit.sidewaysSlip >= 0.3f || wheelHit.sidewaysSlip <= -0.3f ||wheelHit.forwardSlip >= .3f || wheelHit.forwardSlip <= -0.3f)
                playPauseSmoke = true;
            else
                playPauseSmoke = false;
                     */   

			if(wheelHit.sidewaysSlip < 0 )	driftFactor = (1 + -inputX) * Mathf.Abs(wheelHit.sidewaysSlip) ;

			if(wheelHit.sidewaysSlip > 0 )	driftFactor = (1 + inputX )* Mathf.Abs(wheelHit.sidewaysSlip );
		}
     }
     
     
    //private float driftFactor;
    //public float handBrakeFrictionMultiplier = 2f;  // fuerza de drift del auto
    //private WheelFrictionCurve  forwardFriction,sidewaysFriction;

    /*
    private void adjustTraction(){
            //tine it takes to go from normal drive to drift 
        float driftSmothFactor = .7f * Time.deltaTime;

		if(_inputManager.brake){
            sidewaysFriction = Ruedas[0].collider.sidewaysFriction;
            forwardFriction = Ruedas[0].collider.forwardFriction;

            float velocity = 0;
            sidewaysFriction.extremumValue =sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue =
                Mathf.SmoothDamp(forwardFriction.asymptoteValue,driftFactor * handBrakeFrictionMultiplier,ref velocity ,driftSmothFactor );

            for (int i = 0; i < 4; i++) {
                Ruedas[i].collider.sidewaysFriction = sidewaysFriction;
                Ruedas[i].collider.forwardFriction = forwardFriction;
            }

            sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = forwardFriction.extremumValue = forwardFriction.asymptoteValue =  1.1f;
                //extra grip for the front wheels
            for (int i = 0; i < 2; i++) {
                Ruedas[i].collider.sidewaysFriction = sidewaysFriction;
                Ruedas[i].collider.forwardFriction = forwardFriction;
            }
            _rb.AddForce(transform.forward * (KPH / 400) * 40000 );
		}
            //executed when handbrake is being held
        else{

			forwardFriction = Ruedas[0].collider.forwardFriction;
			sidewaysFriction = Ruedas[0].collider.sidewaysFriction;

			forwardFriction.extremumValue = forwardFriction.asymptoteValue = sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = 
                ((KPH * handBrakeFrictionMultiplier) / 300) + 1;

			for (int i = 0; i < 4; i++) {
                Ruedas[i].collider.forwardFriction = forwardFriction;
                Ruedas[i].collider.sidewaysFriction = sidewaysFriction;

			}
        }

            //checks the amount of slip to control the drift
		for(int i = 2;i<4 ;i++){

            WheelHit wheelHit;

            Ruedas[i].collider.GetGroundHit(out wheelHit);            

			if(wheelHit.sidewaysSlip < 0 )	driftFactor = (1 + -_inputManager._horizontal) * Mathf.Abs(wheelHit.sidewaysSlip) ;

			if(wheelHit.sidewaysSlip > 0 )	driftFactor = (1 + _inputManager._horizontal) * Mathf.Abs(wheelHit.sidewaysSlip );
		}	
		
	}
    */
     
    void OnEnable()
    {

        _transform = GetComponent<Transform>();
        _rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        _rb.centerOfMass = _CentroMasa;
        //_TorqueActual = _TorqueEnRuedas - (_ControlDtraccion * _TorqueEnRuedas);
    }
    
    
    void Update()
    {
        velocidad = _rb.velocity.magnitude * 3.6f;
       // currSpeed = car.velocity.magnitude *3.6f;
        
        
        
        Inputs();
        AnimacionRuedas();
        Freno();
        //adjustTraction();
        

        //ControladorMarcas();
    }

    private void FixedUpdate()
    {
       //fwdInput = im.forward;
        //checkWheelSpin();
        //adjustTraction();
    }

    private void LateUpdate()
    {
        MovimientoAuto();
        AyudanteTimon();
        DireccionAuto();
    }


    void checkWheelSpin(){

        float blind = 0.28f;

        if(Input.GetKey(KeyCode.LeftShift))
            _rb.AddForce(transform.forward * 15000);
        if (handbrake){
        //if(handBrake){
            for(int i = 0;i<4 ;i++){
                WheelHit wheelHit;
                wheels[i].collider.GetGroundHit(out wheelHit);
                //wheelColliders[i].GetGroundHit(out wheelHit);
                if(wheelHit.sidewaysSlip > blind || wheelHit.sidewaysSlip < -blind){
                    applyBooster(wheelHit.sidewaysSlip);
                }
            }

        }

        
        
        //tempo es el angulo de drift
        for(int i = 2;i<4 ;i++){
            WheelHit wheelHit;

            wheels[i].collider.GetGroundHit(out wheelHit);

            if(wheelHit.sidewaysSlip < 0 )	//lado derecho
                tempo = (1 + -inputX) * Mathf.Abs(wheelHit.sidewaysSlip *handBrakeFrictionMultiplier) ;
            if(tempo < 0.5) tempo = 0.5f;
            if(wheelHit.sidewaysSlip > 0 )	//lado izquierdo
                tempo = (1 + inputX)* Mathf.Abs(wheelHit.sidewaysSlip *handBrakeFrictionMultiplier);
            if(tempo < 0.5) tempo = 0.5f;
            if(wheelHit.sidewaysSlip > .99f || wheelHit.sidewaysSlip < -.99f){
                //handBrakeFriction = tempo * 3;
                float velocity = 0;
                handBrakeFriction = Mathf.SmoothDamp(handBrakeFriction,tempo* 3,ref velocity ,0.1f * Time.deltaTime);
            }
            else

                handBrakeFriction = tempo;
        }

		

    }
    
    
    public void applyBooster(float amount){
        float R =Mathf.Abs((velocidad /(topSpeed * 2)) * 15000);
        if(fwdInput != 0 ){
            _rb.AddForce(transform.forward * (1 + (velocidad / topSpeed)* 5000));
            _rb.AddForce(-transform.right * amount * R *2);
        }
    }
    
    
/*
    void brakeCar ()
    {
        for (int i = 0; i < 4; i++) {
            wheelColliders [i].brakeTorque = (backInput < 0 )?brakingPower: 0;
            if(backInput < 0 && fwdInput ==0)car.AddForce(-transform.forward * 1000);
            if(velocidad < 0 && backInput == 0 )wheelColliders[i].brakeTorque = brakingPower * 18000;
            else wheelColliders[i].brakeTorque = 0;
        }


    }
    
    /*
    private void ControladorMarcas()
    {
        if (inputSpace)
        {
            EmitirEfectoDrift();
        }
        else
        {
            PararEfectoDrift();
        }
        
    }

    private void EmitirEfectoDrift()
    {
        if (_marcaFlag) return;
        foreach (var i in MarcasDrift)
        {
            i.emitting = true;
        }

        _marcaFlag = true;
    }
    private void PararEfectoDrift()
    {
        if (!_marcaFlag) return;
        foreach (var i in MarcasDrift)
        {
            i.emitting = false;
        }

        _marcaFlag = !true;
    }
    */
}
