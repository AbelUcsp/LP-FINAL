using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AutoController : MonoBehaviour
{

    internal enum TipoDeAuto
    {
        TraccionDelantera,
        TraccionTrasera,
        TraccionCompleta
    }
    
    
    public  static float steervalue = 0.5f;

    public Text texto;
    [SerializeField] private TipoDeAuto _tipoDeAuto;
    [SerializeField] private  List<Llanta> Ruedas = new List<Llanta>(4); //serializar para poder usar en el "Inspector"
    //[SerializeField] public WheelCollider[] wheels = new WheelCollider[4];
    //public GameObject[] wheelMesh = new GameObject[4];
    
    public int motorTorque = 100;
    public float steerAngle = 4;
    public float brakeValue = 9000;

    private InputManager _inputManager;
    private Rigidbody _rb;
    public float velocidad;
    public float KPH;
    public float FuerzaHaciaAbajo = 250.0f;
    public float radioAckearman = 6.0f;   //  Ackerman

    public GameObject CenterMass;

    [Header("Friccion")] //public float algo;
    public float SegundosPasarDifrt = 0.7f;
    public float[] _deslizar = new float[4];
    private float driftFactor;
    public float handBrakeFrictionMultiplier = 2f;  // fuerza de drift del auto
    private WheelFrictionCurve  forwardFriction,sidewaysFriction;
    [HideInInspector] public bool playPauseSmoke = false;       //para Humo

    
    [Header("Engine")] public AnimationCurve Motor;
    public float RPMruedas;         //wheelsRPM
    public float PoderTotalMotor;
    public float motorRPM;          //engineRPM
    private float smoothTime = 0.09f;
    public float MaxRPM = 5600.0f;
    public float MinRPM = 6000.0f;
    
    
    //public float wheelsRPM;
    //public float engineRPM;
    public float[] gears;
    //public float[] gearChangeSpeed;
    public int gearNum = 0;
    [Header("Speedimetro")] public Speedometro _speedometro;


    
    public bool _reversa;
    
    void Start()
    {
        _inputManager = GetComponent<InputManager>();
        _rb = GetComponent<Rigidbody>();
        CenterMass = GameObject.Find("CenterOfMass");
        //_rb.centerOfMass = CenterMass.transform.position;
        // no deberia voltearse/rodar solo driftear
        _rb.centerOfMass = CenterMass.transform.localPosition; //lo mas bajo posible para evitar dar vueltas al voltear a gran velocidad
    }

    private void moveAuto()
    {
        float totalPower;
        if (_tipoDeAuto == TipoDeAuto.TraccionCompleta)
        {
            for (int i = 0; i < Ruedas.Count; ++i)
            {
                Ruedas[i].collider.motorTorque = PoderTotalMotor/4;
            }
        }else if (_tipoDeAuto == TipoDeAuto.TraccionDelantera)
        {
            Ruedas[0].collider.motorTorque = Ruedas[1].collider.motorTorque = PoderTotalMotor/2;
        }else if (_tipoDeAuto == TipoDeAuto.TraccionTrasera)
        {
            Ruedas[2].collider.motorTorque = Ruedas[3].collider.motorTorque = PoderTotalMotor/2;
        }

        

        for (int i = 0; i < Ruedas.Count-2; ++i)
        {
            Ruedas[i].collider.steerAngle = _inputManager._horizontal * steerAngle;
        }
    }

    private void brake()
    {
        if (_inputManager.brake)
        {
            Ruedas[2].collider.brakeTorque = Ruedas[3].collider.brakeTorque = brakeValue;
        }
        else
        {
            Ruedas[2].collider.brakeTorque = Ruedas[3].collider.brakeTorque = 0;
        }
    }
    
    private void AnimacionRuedas()
    {
        foreach (var wheel in Ruedas)
        {
            Quaternion _root;
            Vector3 _pos;
            wheel.collider.GetWorldPose(out _pos, out _root);
            wheel.modelo.transform.position = _pos;
            wheel.modelo.transform.rotation = _root;
        }
    }

    //ackerman formula
    private void Ackerman()
    {
        /*
        _targetRotationx = transform.rotation;
        _targetRotationx.x = 0;
        _targetRotationz = transform.rotation;
        _targetRotationz.z = 0;

        if(transform.rotation.x > .15f || transform.rotation.x < -.15f){
            transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotationx, Time.deltaTime * turningRate);	
        }
        if(transform.rotation.z > .10f || transform.rotation.z < -.10f){
            transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotationz, Time.deltaTime * turningRate);
        }
*/
        
        var horizontalInput = _inputManager._horizontal;

        //acerman steering formula
        //steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * horizontalInput;
        
        if (horizontalInput > 0 ) {
            //rear tracks size is set to 1.5f       wheel base has been set to 2.55f
            Ruedas[0].collider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radioAckearman + (1.5f / 2))) * horizontalInput;
            Ruedas[1].collider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radioAckearman - (1.5f / 2))) * horizontalInput;
        } else if (horizontalInput < 0 ) {
            Ruedas[0].collider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radioAckearman - (1.5f / 2))) * horizontalInput;
            Ruedas[1].collider.steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radioAckearman + (1.5f / 2))) * horizontalInput;
            //transform.Rotate(Vector3.up * steerHelping);

        } else {
            Ruedas[0].collider.steerAngle =0;
            Ruedas[1].collider.steerAngle =0;
        }

    }

    void AddDownForce()
    {
        _rb.AddForce(-transform.up*(FuerzaHaciaAbajo*_rb.velocity.magnitude));
    }

    private void ObtenerFriccion()
    {
        for (int i = 0; i < Ruedas.Count; ++i)
        {
            WheelHit _wheelHit;
            Ruedas[i].collider.GetGroundHit(out _wheelHit);
            
            // "fraccion/valor" perdida de la friccion lateral
            _deslizar[i] = _wheelHit.sidewaysSlip;
        }
        
    }
    

//begin drift
    private void adjustTraction(){
            //tiempo que se tarda en pasar de la conducción normal a la deriva 
        float driftSmothFactor = SegundosPasarDifrt * Time.deltaTime;

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
            //agarre extra para las ruedas delanteras 
            for (int i = 0; i < 2; i++) {
                Ruedas[i].collider.sidewaysFriction = sidewaysFriction;
                Ruedas[i].collider.forwardFriction = forwardFriction;
            }
            //_rb.AddForce(transform.forward * (KPH / 400) * 40000 );
            //_rb.AddForce(Vector3.forward * thrust);               //agregar bost      thrust = 10000
            
            
		}
            //ejecutado cuando no se sostiene el input
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

            //comprueba la cantidad de deslizamiento para controlar el drift
            //aqui evitamos perder traccion
            
		for(int i = 0;i<4 ;i++){

            WheelHit wheelHit;

            Ruedas[i].collider.GetGroundHit(out wheelHit);   
            if(wheelHit.sidewaysSlip >= 0.3f || wheelHit.sidewaysSlip <= -0.3f ||wheelHit.forwardSlip >= .3f || wheelHit.forwardSlip <= -0.3f)
                playPauseSmoke = true;
            else
                playPauseSmoke = false;

			if(wheelHit.sidewaysSlip < 0 )	driftFactor = (1 + -_inputManager._horizontal) * Mathf.Abs(wheelHit.sidewaysSlip) ;

			if(wheelHit.sidewaysSlip > 0 )	driftFactor = (1 + _inputManager._horizontal) * Mathf.Abs(wheelHit.sidewaysSlip );
		}	
        
		
	}

    
    private IEnumerator timedLoop(){            // girar menos a velocidades altas
        while(true){
            yield return new WaitForSeconds(.7f);
            radioAckearman = 6 + KPH / 20;
            
        }
    }
    
    //end drift


    private void PotenciaMotor()
    {
        RPM();
        PoderTotalMotor = Motor.Evaluate(motorRPM) * (gears[gearNum]) * _inputManager._vertical;
        float velocity = 0.0f;
        motorRPM = Mathf.SmoothDamp(motorRPM, 1000 + (Mathf.Abs(RPMruedas) *3.6f* (gears[gearNum])), ref velocity, smoothTime);
    }

    private void RPM()
    {
        float temp = 0;
        int inc = 0;
        for (int i = 0; i<4; ++i)
        {
            temp += Ruedas[i].collider.rpm;
            inc++;
        }
        // POSITIVO SI forward, NEGATIVO si retrocede xD
        RPMruedas = (inc != 0) ? temp/inc : 0;

        if (RPMruedas < 0 && !_reversa) //!_reverse si boleano ya cambio o toavia
        {                               // comprobamos si ya cambio para no actualizar la UI a cada frame porque es costoso
            
            _reversa = true;
            _speedometro.updateGear();
        }
        else if (RPMruedas > 0 && _reversa)
        {
            
            _reversa = false;
            _speedometro.updateGear();
        }
    }

    bool Esta_enAire()
    {
        if (!Ruedas [0].collider.isGrounded && !Ruedas [1].collider.isGrounded && !Ruedas [2].collider.isGrounded && !Ruedas [3].collider.isGrounded) {
            return true;
        } else
            return false;
    }

    private void cajaCambio()
    {

        if (Esta_enAire()) return;
        
        //automatico mode
        if (motorRPM > MaxRPM && gearNum < gears.Length-1)
        {
            gearNum++;
            _speedometro.updateGear();
        }
        if (motorRPM < MinRPM && gearNum > 0)
        {
            gearNum--;
            _speedometro.updateGear();
        }
        
    }
    
    /*
    void Cambio ()
    {
        if ((gearNum < gearRatios.length - 1 && motorRPM >= MaxRPM || (gearNum == 0 && (fwdInput > 0 || backInput < 0))) && !isFlying () && checkGearSpeed ()) {
            gearNum++;
        }
        if (gearNum > 1 && engineRPM <= minGearChangeRPM)
            gearNum--;
        if (checkStandStill () && backInput < 0)
            gearNum = -1;
        if (gearNum == -1 && checkStandStill () && fwdInput > 0)
            gearNum = 1;
    }
    private bool checkSpeeds = true;
    bool checkGearSpeed ()
    {
        if (gearNum != -1) {
            if (checkSpeeds) {
                return velocidad >= gears [gearNum - 1];
            } else
                return true;
        } else
            return false;
    }
    */
    
    
    private void FixedUpdate()
    {
        velocidad = _rb.velocity.magnitude * 3.6f;
        KPH = velocidad;
        AddDownForce();
        AnimacionRuedas();
        moveAuto();
        Ackerman();
        //brake();
        PotenciaMotor();
        ObtenerFriccion();
        adjustTraction();
        cajaCambio();   //automatico
    }

    void Update()
    {
        //cajaCambio();     //manual
        texto.text = velocidad.ToString();
    }
    
}
