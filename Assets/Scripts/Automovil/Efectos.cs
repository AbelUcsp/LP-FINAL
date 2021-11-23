using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Efectos : MonoBehaviour
{
    public ParticleSystem[] _humo = new ParticleSystem[4];

    private AutoController _autoController;
    private bool _controladorHumo;
    
    
    
    [Header("Skidmarks")] 
    private InputManager _inputManager;
    private bool marcasFlag;
    public TrailRenderer[] _marcas;
    public AudioSource _clipDerrape;

    public float delat_ = 0.15f;
    void Start()
    {
        _autoController = gameObject.GetComponent<AutoController>();
        _inputManager = gameObject.GetComponent<InputManager>();

    }

    public void IniciarHumo()
    {
        if (_controladorHumo) return;
        for (int i = 0; i < _humo.Length; ++i)
        {
            _humo[i].Play();

        }

        _controladorHumo = true;
    }
    public void DetenerHumo()
    {
        if (!_controladorHumo) return;
        for (int i = 0; i < _humo.Length; ++i)
        {
            _humo[i].Stop();
        }

        _controladorHumo = false;
    }
    

    void FixedUpdate()
    {
        if(_autoController.playPauseSmoke)
            IniciarHumo();
        else
            DetenerHumo();

        if (_controladorHumo)
        {
            for (int i=0; i<_humo.Length; ++i)
            {
                var emission = _humo[i].emission;
                //emission.rateOverTime = ((int) _autoController.velocidad * 10 <= 2000)
                //        ? (int) _autoController.velocidad * 10
                //        : 2000;

                emission.rateOverDistance = ((int) _autoController.velocidad <= 60)
                    ? 5
                    : (int) _autoController.velocidad * 0.075f;
            }
        }

        EstaDeslizandose();
    }
    
    //skidmarks
    private void EstaDeslizandose()
    {
        if (_inputManager.brake)
        {
            IniciarMarcas();
        }
        else
        {
            DeternerMarcas();
        }
    }

    void IniciarMarcas()
    {// las banderas se utilizan para emitir una sola vez en lugar de cada fotograma
        if (marcasFlag) return;
        foreach (TrailRenderer T in _marcas)
        {
            T.emitting = true;
        }
        _clipDerrape.PlayDelayed(delat_);
        //_clipDerrape.Play();
        
        marcasFlag = true;
    }

    void DeternerMarcas()
    {
        if (!marcasFlag) return;
        foreach (TrailRenderer T in _marcas)
        {
            T.emitting = false;
            
        }
        _clipDerrape.Stop();

        marcasFlag = false;
    }
    
}
