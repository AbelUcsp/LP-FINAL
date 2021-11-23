using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Axel
{
    Front,
    Rear
}

[Serializable]
public struct Wheel
{
    public GameObject model;
    public WheelCollider collider;
    public Axel axel;
}



public class WheelData
{
    public Wheel wheel;
    public WheelCollider collider;					// WheelCollider component for this wheel
    public Transform transform;
	
    public bool grounded = false;		// esta en suelo?
    public WheelHit hit;	
    public float forceDistance;
    //public GroundMaterial groundMaterial = null;

    public float downforce = 0.0f;

    public Vector3 velocity = Vector3.zero;
    public Vector2 localVelocity = Vector2.zero;
    public Vector2 localRigForce = Vector2.zero;		//	fuerza de la plataforma local

    public bool isBraking = false;						//	está frenando
    public float finalInput = 0.0f;
    public Vector2 tireSlip = Vector2.zero;				//	deslizamiento de llanta
    public Vector2 tireForce = Vector2.zero;			//	fuerza de los neumáticos
    public Vector2 dragForce = Vector2.zero;			//	fuerza de arrastre
    public Vector2 rawTireForce = Vector2.zero;			//	fuerza bruta del neumático

    public float angularVelocity = 0.0f;
    // Utility data
	
    public float positionRatio = 0.0f;

}

public class CarController : MonoBehaviour
{
public bool adjust = false;
	public int cantidadElementos;
	[Range(0, 2)] [SerializeField]
	private float m_SteerHelper; // 0 is raw physics , 1 the car will grip in the direction it is facing

	[Range(0, 2)] [SerializeField] private float m_TractionControl; // 0 is no traction control, 1 is full interference
	private float m_OldRotation;
	[SerializeField] private float m_SlipLimit = 0.3f;
	private float m_CurrentTorque;
	[SerializeField] public float m_FullTorqueOverAllWheels = 2500.0f;

	//Spin lateral
	[SerializeField] private float tempo; //wheelSpin pointer (angulo de drift)
	[SerializeField] private float handBreakerFrictionMultiplier = 2.0f;
	[SerializeField] private float handBreakerFriction = 0.05f;
	[SerializeField] private WheelFrictionCurve _sidewaysFriction;
	[SerializeField] private WheelFrictionCurve _forwardFriction;
	
	public bool playPauseSmoke = false;
	public float handBrakeFrictionMultiplier = 2f;
	public float KPH;
	[Range(0.5f, 5.5f)] [SerializeField] public float grip;
	private float driftFactor;
	
	
	public float topSpeed;
	public float VelZ;
	public float currSpeed;
	public float frictionMultiplier = 3f;

	public Text text_;
	public Text volcado;
	private float velocidad;

	//public Rigidbody rb;




	[SerializeField] private float maxAcceleration = 20.0f;
	[SerializeField] private float turnSensitivity = 1.0f;
	[SerializeField] private float maxSteerAngle = 45.0f;
	[SerializeField] private Vector3 _centerOfMass;
	[SerializeField] private List<Wheel> wheels;

	private float inputX, inputY;

	private Rigidbody _rb;
	private Transform _transform;


	// new script
	[Header("New Script")] public float prueba = 10.1f;
	public WheelData[] m_wheelData = new WheelData[0];
	public List<Wheel> wheelz;
	private void Start()
	{
		//_rb = GetComponent<Rigidbody>();
		_rb.centerOfMass = _centerOfMass;

		m_CurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl * m_FullTorqueOverAllWheels);
	}

	void OnEnable()
	{
		
		_transform = GetComponent<Transform>();
		_rb = GetComponent<Rigidbody>();
		
		//m_wheelData = new WheelData[wheels.Length];
		m_wheelData = new WheelData[wheels.Count];
		print(wheels.Count);
		for (int i = 0; i < m_wheelData.Length; i++)
		{
			Wheel w = wheels[i];
			WheelData wd = new WheelData();
			wheelz.Add( wheels[i]);

			//wd.isWheelChildOfCaliper = w.caliperTransform != null && w.wheelTransform != null && w.wheelTransform.IsChildOf(w.caliperTransform);

			wd.collider = w.collider;
			wd.transform = w.collider.transform;
			// if (w.handbrake) m_usesHandbrake = true;

			// Calculate the force distance for center of mass and anti-roll

			//UpdateWheelCollider(wd.collider);
			wd.forceDistance = GetWheelForceDistance(wd.collider);

			

			//VehicleFrame m_vehicleFrame;


			// Determine whether this wheel is "front" or "rear"
			float zPos = _transform.InverseTransformPoint(wd.transform.TransformPoint(wd.collider.center)).z;
			//wd.positionRatio = zPos >= m_vehicleFrame.middlePoint ? 1.0f : 0.0f;

			// Store the data

			wd.wheel = w;
			m_wheelData[i] = wd;

		}
	}

	
	// funcion nueva//
	float GetWheelForceDistance (WheelCollider col)
    {
	    return _rb.centerOfMass.y - _transform.InverseTransformPoint(col.transform.position).y 
	           + col.radius + (1.0f - col.suspensionSpring.targetPosition) * col.suspensionDistance;
    }
	
	public Vector3 GetSidewaysForceAppPoint (WheelData wd, Vector3 contactPoint)
	{
		//Vector3 sidewaysForcePoint = contactPoint + wd.transform.up * antiRoll * wd.forceDistance;

		//if (wd.wheel.steer && wd.steerAngle != 0.0f && Mathf.Sign(wd.steerAngle) != Mathf.Sign(wd.tireSlip.x))
			//sidewaysForcePoint += wd.transform.forward * (m_vehicleFrame.frontPosition - m_vehicleFrame.rearPosition) * (handlingBias - 0.5f);

//		return sidewaysForcePoint;
		return Vector3.one;
	}
	
	public static float GetBalancedValue (float value, float bias, float positionRatio)
	{
		float frontRatio = bias;
		float rearRatio = 1.0f - bias;

		return value * (positionRatio * frontRatio + (1.0f-positionRatio) * rearRatio) * 2.0f;
	}
	
	public static float GetRampBalancedValue (float value, float bias, float positionRatio)
	{
		float frontRatio = Mathf.Clamp01(2.0f * bias);
		float rearRatio = Mathf.Clamp01(2.0f * (1.0f - bias));

		return value * (positionRatio * frontRatio + (1.0f-positionRatio) * rearRatio);
	}
	
	//	end nuevas funciones
	
	
    
    private void SteerHelper()
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
        if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
        {
            var turnadjust = (transform.eulerAngles.y - m_OldRotation) * m_SteerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            _rb.velocity = velRotation * _rb.velocity;
        }
        m_OldRotation = transform.eulerAngles.y;
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
            m_CurrentTorque -= 10 * m_TractionControl;
        }
        else
        {
            m_CurrentTorque += 10 * m_TractionControl;
            if (m_CurrentTorque > m_FullTorqueOverAllWheels)
            {
                m_CurrentTorque = m_FullTorqueOverAllWheels;
            }
        }
    }
    
    
    public void applyBooster(float amount){
        float R =Mathf.Abs((currSpeed /(topSpeed * 2)) * 15000);
        if(VelZ != 0 ){
           _rb.AddForce(transform.forward * (1 + (currSpeed / topSpeed)* 5000));
           _rb.AddForce(-transform.right * amount * R *2);
        }
    }

   


    void ComputeTireForces (WheelData wd) {
	    
	    float demandedForce = 0.0f;
	    float maxBrakeForce = 3000.0f;
	    float brakeForceToMaxSlip = 1000.0f;
	    float driveForceToMaxSlip = 1000.0f;
	    
	    float wheelBrakeInput = 0.0f;
	    float wheelBrakeRatio = 0.0f;
	    float wheelBrakeSlip = 0.0f;
		float wheelMaxDriveSlip = 4.0f;
	    float defaultGroundDrag = 0.0f;
	    float groundDrag = defaultGroundDrag;
	    float brakeBalance = 0.5f;
		float maxDriveForce = 2000.0f;
		float driveBalance = 0.5f;
		
		
	    if (wd.isBraking)
	    {
		    demandedForce = wd.finalInput * GetRampBalancedValue(maxBrakeForce, brakeBalance, wd.positionRatio);
	    }
	    else
	    {
		    float balancedDriveForce = GetRampBalancedValue(maxDriveForce, driveBalance, wd.positionRatio);
		    //demandedForce = ComputeDriveForce(wd.finalInput * balancedDriveForce, balancedDriveForce, wd.grounded);
	    }
	    
		
		if (wd.grounded)
		{
			wd.tireSlip.x = wd.localVelocity.x;

			wd.tireSlip.y = wd.localVelocity.y - (wd.angularVelocity * wd.collider.radius);
		
	
		    // Get the ground properties
		    /*
		    float groundGrip;
		    float groundDrag;

		    if (wd.groundMaterial != null)
			    {
			    groundGrip = wd.groundMaterial.grip;
			    groundDrag = wd.groundMaterial.drag;
			    }
		    else
			    {
			    groundGrip = defaultGroundGrip;
			    groundDrag = defaultGroundDrag;
			    }
		    */
		    // Calculate the total tire force available
			
			
			
			
			/// BORRAR ESTAS VARIABLES
			float tireFriction = 1.0f;
			float tireFrictionBalance = 0.5f;
			float groundGrip = 1.0f;
			
			
			

			float balancedFriction = GetBalancedValue(tireFriction, tireFrictionBalance, wd.positionRatio);
			float forceMagnitude = balancedFriction * wd.downforce * groundGrip;
			//float forceMagnitude = balancedFriction * wd.downforce * groundGrip;

			// Ensure there's longitudinal slip enough for the demanded longitudinal force

			float minSlipY;

			
			if (wd.isBraking)
			{
				float wheelMaxBrakeSlip = Mathf.Max(Mathf.Abs(wd.localVelocity.y * wheelBrakeRatio),  wheelBrakeSlip);
				minSlipY = Mathf.Clamp(Mathf.Abs(demandedForce * wd.tireSlip.x) / forceMagnitude, 0.0f, wheelMaxBrakeSlip);
			}
			else
			{
				minSlipY = Mathf.Min(Mathf.Abs(demandedForce * wd.tireSlip.x) / forceMagnitude, wheelMaxDriveSlip);
				if (demandedForce != 0.0f && minSlipY < 0.1f) minSlipY = 0.1f;
			}
			
			
			if (Mathf.Abs(wd.tireSlip.y) < minSlipY) wd.tireSlip.y = minSlipY * Mathf.Sign(wd.tireSlip.y);

			// Compute combined tire forces

			wd.rawTireForce = -forceMagnitude * wd.tireSlip.normalized;
			wd.rawTireForce.x = Mathf.Abs(wd.rawTireForce.x);
			wd.rawTireForce.y = Mathf.Abs(wd.rawTireForce.y);

			// Sideways force

			wd.tireForce.x = Mathf.Clamp(wd.localRigForce.x, -wd.rawTireForce.x, +wd.rawTireForce.x);

			// Forward force

			if (wd.isBraking)
			{
				float maxFy = Mathf.Min(wd.rawTireForce.y, demandedForce);
				wd.tireForce.y = Mathf.Clamp(wd.localRigForce.y, -maxFy, +maxFy);
			}
			else
			{
				wd.tireForce.y = Mathf.Clamp(demandedForce, -wd.rawTireForce.y, +wd.rawTireForce.y);
			}

			// Drag force as for the surface resistance
			
			wd.dragForce = -(forceMagnitude * wd.localVelocity.magnitude * groundDrag * 0.001f) * wd.localVelocity;
		}
		else
		{
			wd.tireSlip = Vector2.zero;
			wd.tireForce = Vector2.zero;
			wd.dragForce = Vector2.zero;
		}
		
		// Compute angular velocity for the next step

		float slipToForce = wd.isBraking? brakeForceToMaxSlip : driveForceToMaxSlip;
		float slipRatio = Mathf.Clamp01((Mathf.Abs(demandedForce) - Mathf.Abs(wd.tireForce.y)) / slipToForce);

		float slip;

		if (wd.isBraking)
			slip = Mathf.Clamp(-slipRatio * wd.localVelocity.y * wheelBrakeRatio, -wheelBrakeSlip, wheelBrakeSlip);
		else
			slip = slipRatio * wheelMaxDriveSlip * Mathf.Sign(demandedForce);

		wd.angularVelocity = (wd.localVelocity.y + slip) / wd.collider.radius;
		
		
	}
    
    
    void ApplyTireForces (WheelData wd)
    {
	    //if (wd.grounded)
		    if (true)
        {
            //if (!disallowRuntimeChanges)
                //wd.forceDistance = GetWheelForceDistance(wd.collider);

            Vector3 forwardForce = wd.hit.forwardDir * (wd.tireForce.y + wd.dragForce.y);
            Vector3 sidewaysForce = wd.hit.sidewaysDir * (wd.tireForce.x + wd.dragForce.x);
            Vector3 sidewaysForcePoint = GetSidewaysForceAppPoint(wd, wd.hit.point);

            _rb.AddForceAtPosition(forwardForce, wd.hit.point);
            //m_rigidbody.AddForceAtPosition(sidewaysForce, sidewaysForcePoint);

			
            //Rigidbody otherRb = wd.hit.collider.attachedRigidbody;
            Rigidbody otherRb = _rb;
           // if (otherRb != null && !otherRb.isKinematic)
            {
                otherRb.AddForceAtPosition(-forwardForce, wd.hit.point);
                otherRb.AddForceAtPosition(-sidewaysForce, sidewaysForcePoint);
            }
				
        }
		
    }
    
    
    /*
    private void AdjustDrift(){
	    
        for (int i = 0; i < 2; i++)
        {
            WheelHit wheelHit;
            //2 y 3
            wheels[i].collider.GetGroundHit(out wheelHit);
            float forwardSlip = wheelHit.sidewaysSlip;

            // AdjustTorque(wheelHit.sidewaysSlip);
            if (forwardSlip < 0)
            {
                //inputY
                // inputX
                tempo = (1 + -inputX) * Mathf.Abs(forwardSlip * handBreakerFrictionMultiplier);
                if (tempo < 0.5f) tempo = 0.5f;
            }

            if (forwardSlip > 0)
            {
                tempo = (1 + inputX) * Mathf.Abs(forwardSlip * handBreakerFrictionMultiplier);
                if (tempo < 0.5f) tempo = 0.5f;
            }

            // 1 completamente de lado drift, 
            if (forwardSlip > 0.99f || forwardSlip < -0.99f)
            {
                float velocity = 0.0f;
                handBreakerFriction =
                    Mathf.SmoothDamp(handBreakerFriction, tempo * 3, ref velocity, 0.1f * Time.deltaTime);
            }
            else
                handBreakerFriction = tempo;
        }
        
		
        /*
        
        print("NO frenando..");
        //aqui va el drift
        if (VelZ < 0.0f){   //si esta frenando
            print("frenando..");
            _sidewaysFriction = wheels[0].collider.sidewaysFriction;
            _forwardFriction = wheels[0].collider.forwardFriction;

            _forwardFriction.extremumValue = _forwardFriction.asymptoteValue = ((currSpeed * frictionMultiplier) / 300) + 1;
            _sidewaysFriction.extremumValue = _sidewaysFriction.asymptoteValue = ((currSpeed * frictionMultiplier) / 300) + 1;

            for (int i = 0; i < 4; i++) {
                wheels[i].collider.forwardFriction = _forwardFriction;
                wheels[i].collider.sidewaysFriction = _sidewaysFriction;

            }
        }
        else if (VelZ >= 0.0f) //si no esta frenando
        {
            //print("frenando..");
            _sidewaysFriction = wheels[0].collider.sidewaysFriction;
            _forwardFriction = wheels[0].collider.forwardFriction;

            float velocity = 0;
            _sidewaysFriction.extremumValue = _sidewaysFriction.asymptoteValue = Mathf.SmoothDamp(_sidewaysFriction.asymptoteValue, handBreakerFriction, ref velocity, 0.05f*Time.deltaTime);
            _forwardFriction.extremumValue = _forwardFriction.asymptoteValue = Mathf.SmoothDamp(_forwardFriction.asymptoteValue, handBreakerFriction, ref velocity, 0.05f*Time.deltaTime);
            for (int i = 0; i < 2; ++i) //llantas traseras drift
            {
                wheels[i].collider.sidewaysFriction = _sidewaysFriction;
                wheels[i].collider.forwardFriction = _forwardFriction;
            }
            
            /*
            _sidewaysFriction.extremumValue = _sidewaysFriction.asymptoteValue = 1.5f;
            _forwardFriction.extremumValue = _forwardFriction.asymptoteValue = 1.5f;
            for (int i = 2; i < 4; ++i) // llantas delanteras
            {
                wheels[i].collider.sidewaysFriction = _sidewaysFriction;
                wheels[i].collider.forwardFriction = _forwardFriction;
            }
            
        }
        
    
    }//end AdjustTorque
*/


    private void AdjustDrift()
    {
	     //tine it takes to go from normal drive to drift 
        float driftSmothFactor = 1 * Time.deltaTime;

        
		//if(IM.handbrake){
            _sidewaysFriction = wheels[0].collider.sidewaysFriction;
            _forwardFriction = wheels[0].collider.forwardFriction;

            float velocity = 0;

            //if (IM.handbrake)
            if (inputY < 0.0f)
            {

                _sidewaysFriction.extremumValue = _sidewaysFriction.asymptoteValue = _forwardFriction.extremumValue =
                    _forwardFriction.asymptoteValue =
                        Mathf.SmoothDamp(_forwardFriction.asymptoteValue, driftFactor * handBrakeFrictionMultiplier,
                            ref velocity, driftSmothFactor);
            }
            else
            {

                _forwardFriction.extremumValue = _forwardFriction.asymptoteValue = _sidewaysFriction.extremumValue =
                    _sidewaysFriction.asymptoteValue =
                        ((KPH * handBrakeFrictionMultiplier) / 300) + grip; ///resetear la friccion lateral/frontal sobre la velocidad
                                                                            /// de vehiculo y al final agregamos GRIP (sujecion)

            }

            for (int i = 0; i < wheels.Count; i++) {
                wheels[i].collider.sidewaysFriction = _sidewaysFriction;
                wheels[i].collider.forwardFriction = _forwardFriction;
            }

            _sidewaysFriction.extremumValue = _sidewaysFriction.asymptoteValue = _forwardFriction.extremumValue = _forwardFriction.asymptoteValue =  1.1f;
                //extra grip for the front wheels
                //if(IM.handbrake)
                if (inputY < 0.0f)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        wheels[i].collider.sidewaysFriction = _sidewaysFriction;
                        wheels[i].collider.forwardFriction = _forwardFriction;
                    }
                }

                //}
        /*
        else{

			forwardFriction = wheels[0].forwardFriction;
			sidewaysFriction = wheels[0].sidewaysFriction;

			forwardFriction.extremumValue = forwardFriction.asymptoteValue = sidewaysFriction.extremumValue = sidewaysFriction.asymptoteValue = 
                ((KPH * handBrakeFrictionMultiplier) / 300) + grip;

			for (int i = 0; i < wheels.Length; i++) {
				wheels [i].forwardFriction = forwardFriction;
				wheels [i].sidewaysFriction = sidewaysFriction;

			}
        
        }
        */
        
            //checks the amount of slip to control the drift
		for(int i = 2; i<wheels.Count ; i++){

            WheelHit wheelHit;

            wheels[i].collider.GetGroundHit(out wheelHit);
                //smoke
            if(wheelHit.sidewaysSlip >= 0.3f || wheelHit.sidewaysSlip <= -0.3f ||wheelHit.forwardSlip >= .3f || wheelHit.forwardSlip <= -0.3f)
                playPauseSmoke = true;
            else
                playPauseSmoke = false;
                        

			if(wheelHit.sidewaysSlip < 0 )	driftFactor = (1 + -inputX) * Mathf.Abs(wheelHit.sidewaysSlip) ;

			if(wheelHit.sidewaysSlip > 0 )	driftFactor = (1 + inputX )* Mathf.Abs(wheelHit.sidewaysSlip );
		}
    }
    

    private void Update()
    {
	    cantidadElementos = wheels.Count;
	    /*
	    for (var iWheel = 0 ;  wheels.Count; ++iWheel)
	    {
		    wheelz[iWheel].model = wheels[iWheel];
	    }
	    */
        velocidad = _rb.velocity.magnitude * 3.6f;
        //speed.text = "Speed " +  velocidad.ToString() + " Km/h";
        text_.text = velocidad.ToString() + " Km/h";
        currSpeed = velocidad;
        AnimateWheels();
        GetInputs();
    }

    private void LateUpdate()
    {
        Move();
        
        //TractionControl();
        //AdjustTorque();
        if(adjust)
            AdjustDrift();
        SteerHelper();  /// helper
        Turn();
        
    }

    private void FixedUpdate()
    {
    /*
	    foreach (WheelData wd in m_wheelData)
	    {
		    ComputeTireForces(wd);
		    ApplyTireForces(wd);
	    }
	    */
    }

    private void GetInputs(){
        inputX = Input.GetAxis("Horizontal");
        inputY = Input.GetAxis("Vertical");
        VelZ = inputY;
    }

    private void Move()
    {
        foreach (var wheel in wheels)
        {
            wheel.collider.motorTorque = inputY * maxAcceleration * 500 * Time.deltaTime;
        }
        KPH = _rb.velocity.magnitude * 3.6f;
        
    }

    private void Turn()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = inputX * turnSensitivity * maxSteerAngle;
                wheel.collider.steerAngle = Mathf.Lerp(wheel.collider.steerAngle,_steerAngle,0.5f);
            }
        }
    }

    private void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion _rot;
            Vector3 _pos;
            wheel.collider.GetWorldPose(out _pos, out _rot);
            wheel.model.transform.position = _pos;
            wheel.model.transform.rotation = _rot;
        }
    }
    
    
}
