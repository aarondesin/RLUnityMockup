using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour {

    const float _ACCELERATION = 1f;
    const float _BRAKE_STRENGTH = 1f;
    const float _STEERING_STRENGTH = 1f;
    const float _MAX_VELOCITY_UNBOOSTED = 1f;
    const float _PITCH_SPEED = 1f;
    const float _ROLL_SPEED = 1f;
    const float _YAW_SPEED = 1f;

    const float _JUMP_FORCE = 10f;

    public static PlayerController Instance;

    float _boost = 30f;

    UnityEvent onGround = new UnityEvent();

    bool _grounded = false;

    int _jumpsLeft = 2;

    float _h;
    float _v;
    float _g;
    float _b;

    Rigidbody _rb;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
        Instance = this;
    }

    private void Update() {
        HandleInputs();
    }

    private void FixedUpdate() {
        float throttleValue = Input.GetAxis ("Gas") * _ACCELERATION + -Input.GetAxis("Brake") * _BRAKE_STRENGTH;
        float steeringValue = Input.GetAxis ("Horizontal") * _STEERING_STRENGTH * Mathf.Clamp01(_rb.velocity.magnitude / _MAX_VELOCITY_UNBOOSTED);
        if (_grounded) {
            _rb.AddForce (transform.forward * throttleValue, ForceMode.Impulse);
            //if (_rb.velocity.magnitude > _MAX_VELOCITY_UNBOOSTED)
                //_rb.velocity *= _rb.velocity.magnitude / _MAX_VELOCITY_UNBOOSTED;
            _rb.MoveRotation (transform.rotation * Quaternion.Euler (0f, steeringValue, 0f));
        } else {
            _rb.AddTorque (transform.up * _YAW_SPEED  * _h);
            _rb.AddTorque (transform.right * _PITCH_SPEED * _v);
        }
    }

    public float Horizontal { get { return _h; } }

    public float Vertical { get { return _v; } }

    public float Gas { get { return _g; } }

    public float Brake { get { return _b; } }

    void HandleInputs () {
        if (Input.GetButtonDown("Jump")) AttemptJump();
        if (Input.GetButton("Boost")) AttemptBoost();
        _h = Input.GetAxis("Horizontal");
        _v = -Input.GetAxis("Vertical");
        _g = Input.GetAxis ("Gas");
        _b = Input.GetAxis ("Brake");
    }

    void AttemptJump () {
        Debug.Log ("AttemptJump");
        if (_grounded) {
            Jump();
        }
    }

    void Jump () {
        Debug.Log ("Jump");
        _rb.AddForce (transform.up * _JUMP_FORCE, ForceMode.VelocityChange);
    }

    void AttemptBoost () {
        Debug.Log ("Boost");
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.tag == "Ground") {
            _grounded = true;
        }
    }

    private void OnCollisionExit(Collision collision) {
        if (collision.collider.tag == "Ground") {
            _grounded = false;
        }
    }
}
