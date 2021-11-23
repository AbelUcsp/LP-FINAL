using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Example skid script. Put this on a WheelCollider.
// Copyright 2017 Nition, BSD licence (see LICENCE file). http://nition.co
[RequireComponent(typeof(WheelCollider))]
public class WheelSkid : MonoBehaviour {

	// INSPECTOR SETTINGS

	[SerializeField]
	Rigidbody rb;
	[SerializeField]
	Skidmarks skidmarksController;

	// END INSPECTOR SETTINGS

	WheelCollider wheelCollider;
	WheelHit wheelHitInfo;

	public float SKID_FX_SPEED = 0.5f; //Velocidad mínima de deslizamiento lateral en m / s para comenzar a mostrar un deslizamiento 
	public float MAX_SKID_INTENSITY = 20.0f; // m/s donde la opacidad del deslizamiento es de máxima intensidad 
	public float WHEEL_SLIP_MULTIPLIER = 10.0f; // Para patinaje de ruedas. Ajustar la cantidad de patinazos 
	public int lastSkid = -1; // Índice de matriz para el controlador de skidmarks. Índice de la última skidmark  que usó esta rueda 
	float lastFixedUpdateTime;

	// #### UNITY INTERNAL METHODS ####

	protected void Awake() {
		wheelCollider = GetComponent<WheelCollider>();
		lastFixedUpdateTime = Time.time;
	}

	protected void FixedUpdate() {
		lastFixedUpdateTime = Time.time;
	}

	protected void LateUpdate() {
		if (wheelCollider.GetGroundHit(out wheelHitInfo))
		{
			// Compruebe la velocidad lateral

			// Da velocidad con + z como eje de avance del automóvil 
			Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
			float skidTotal = Mathf.Abs(localVelocity.x);

			// Compruebe también el giro de las ruedas 

			float wheelAngularVelocity = wheelCollider.radius * ((2 * Mathf.PI * wheelCollider.rpm) / 60);
			float carForwardVel = Vector3.Dot(rb.velocity, transform.forward);
			float wheelSpin = Mathf.Abs(carForwardVel - wheelAngularVelocity) * WHEEL_SLIP_MULTIPLIER;

			// NOTA: Esta línea adicional no debería ser necesaria y puede eliminarla si tiene una física de rueda decente
// El coche de demostración integrado de Unity en realidad patina las ruedas todo el tiempo que estás acelerando
// por lo que esto desvanece el patinaje basado en el patinaje de las ruedas a medida que aumenta la velocidad para que se vea casi bien 
			wheelSpin = Mathf.Max(0, wheelSpin * (10 - Mathf.Abs(carForwardVel)));
			

			skidTotal += wheelSpin;

			// Derrapar si debemos 
			if (skidTotal >= SKID_FX_SPEED) {
				float intensity = Mathf.Clamp01(skidTotal / MAX_SKID_INTENSITY);
				// Account for further movement since the last FixedUpdate
				Vector3 skidPoint = wheelHitInfo.point + (rb.velocity * (Time.time - lastFixedUpdateTime));
				lastSkid = skidmarksController.AddSkidMark(skidPoint, wheelHitInfo.normal, intensity, lastSkid);
			}
			else {
				lastSkid = -1;
			}
		}
		else {
			lastSkid = -1;
		}
	}

	// #### PUBLIC METHODS ####

	// #### PROTECTED/PRIVATE METHODS ####


}
