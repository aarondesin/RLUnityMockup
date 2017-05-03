﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {

    const float _EXPLOSION_FORCE = 200000f;
    const float _EXPLOSION_RADIUS = 100f;

    public static Ball Instance;

    MeshRenderer _renderer;

    ParticleSystem[] _explosionEffects;

    [SerializeField] AudioClip _impactSound;
    [SerializeField] AudioClip _explosionSound;
    AudioSource _audioSource;

    private void Awake() {
        Instance = this;
        _renderer = GetComponent<MeshRenderer>();
        _explosionEffects = GetComponentsInChildren<ParticleSystem>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Goal") {
            Explode();
            var otherTeam = (GameManager.Team)(1 - (int)other.GetComponent<Goal>().Team);
            GameManager.Instance.RegisterGoal (otherTeam);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        _audioSource.PlayOneShot (_impactSound, Mathf.Clamp01(collision.relativeVelocity.magnitude / 75f));
        Debug.Log ("bap");
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere (transform.position, _EXPLOSION_RADIUS);
    }

    void Explode () {
        foreach (var player in GameManager.Instance.Players)
            player.GetComponent<Rigidbody>().AddExplosionForce (_EXPLOSION_FORCE, transform.position, _EXPLOSION_RADIUS, 1f, ForceMode.Impulse);
        _renderer.enabled = false;
        foreach (var ps in _explosionEffects) ps.Play();
        _audioSource.PlayOneShot (_explosionSound, 0.75f);
        //gameObject.SetActive(false);

    }

    public void ResetBall() {
        _renderer.enabled = true;
        //gameObject.SetActive(true);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        
        //Debug.Log ("reset");
    } 
}
