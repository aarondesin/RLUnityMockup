using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour {

    const float _ACCELERATION = 1f;
    const float _BRAKE_STRENGTH = 1f;
    const float _STEERING_STRENGTH_GROUNDED = 1.5f;
    const float _STEERING_STRENGTH_INAIR = 1f;
    const float _MAX_VELOCITY_UNBOOSTED = 1f;
    const float _PITCH_SPEED = 1f;
    const float _ROLL_SPEED = 1f;
    const float _YAW_SPEED = 1f;

    const float _JUMP_FORCE = 10f;
    const float _DODGEFLIP_THRESHOLD = 0.5f;
    const float _DODGEFLIP_FORCE = 12f;
    const float _DODGEFLIP_TORQUE = 3.5f;
    const float _FLIP_FORCE = 10f;
    const float _FLIP_TORQUE = 1f;
    const float _FLIPPED_THRESHOLD = 0.1f;

    const int _JUMPS_ALLOWED = 2;

    public static PlayerController Instance;

    float _boost = 30f;

    UnityEvent onGround = new UnityEvent();

    bool _grounded = false;
    bool _flipped = false;

    Vector3 _groundNormal;

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

    private void FixedUpdate() {
        HandleInputs();

        float throttleValue = Input.GetAxis ("Gas") * _ACCELERATION + -Input.GetAxis("Brake") * _BRAKE_STRENGTH;
        float steeringMultiplier = _grounded ? (_flipped ? 0f : _STEERING_STRENGTH_GROUNDED) : _STEERING_STRENGTH_INAIR;
        float steeringValue = Input.GetAxis ("Horizontal") * steeringMultiplier * Mathf.Clamp01(_rb.velocity.magnitude / _MAX_VELOCITY_UNBOOSTED);
        if (_grounded) {
            //_rb.MovePosition (transform.forward * throttleValue * Time.fixedDeltaTime);
            _rb.AddForce (transform.forward * throttleValue, ForceMode.Impulse);
            _rb.MoveRotation (transform.rotation * Quaternion.Euler (0f, steeringValue, 0f));
            _rb.velocity = transform.forward * _rb.velocity.magnitude;
        } else {
            _rb.AddTorque (transform.up * _YAW_SPEED  * _h);
            _rb.AddTorque (transform.right * _PITCH_SPEED * _v);
        }

        if (!_rb.detectCollisions) _rb.detectCollisions = true;
    }

    public float Horizontal { get { return _h; } }

    public float Vertical { get { return _v; } }

    public float Gas { get { return _g; } }

    public float Brake { get { return _b; } }

    public bool Grounded { get { return _grounded; } } 

    public bool Flipped { get { return _flipped; } }

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
            if (_flipped) Flip();
            else Jump();
        } else if (_jumpsLeft > 0) {
            if (Mathf.Abs (_h) > _DODGEFLIP_THRESHOLD ||
                Mathf.Abs (_v) > _DODGEFLIP_THRESHOLD)
            DodgeFlip();
            else Jump();
        }
    }

    void Jump () {
        Debug.Log ("Jump");
        _rb.AddForce (transform.up * _JUMP_FORCE, ForceMode.VelocityChange);
        _rb.MovePosition (transform.position + transform.up * _JUMP_FORCE * Time.fixedDeltaTime);
        _rb.detectCollisions = false;
        _jumpsLeft--;
    }

    void Flip () {
        Debug.Log ("Flip");
        _rb.AddForce (_groundNormal * _FLIP_FORCE, ForceMode.VelocityChange);
        _rb.AddTorque (transform.forward * _FLIP_FORCE, ForceMode.VelocityChange);
        _rb.detectCollisions = false;
        _jumpsLeft--;
    }

    void DodgeFlip () {
        Debug.Log ("DodgeFlip");
        _rb.AddTorque (transform.forward * -_h * _DODGEFLIP_TORQUE, ForceMode.VelocityChange);
        _rb.AddTorque (transform.right * _v * _DODGEFLIP_TORQUE, ForceMode.VelocityChange);
        _rb.AddForce (transform.right * _h * _DODGEFLIP_FORCE, ForceMode.VelocityChange);
        _rb.AddForce (transform.forward * _v * _DODGEFLIP_FORCE, ForceMode.VelocityChange);
        _rb.detectCollisions = false;
        _jumpsLeft--;
    }

    void AttemptBoost () {
        Debug.Log ("Boost");
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.tag == "Ground" || collision.collider.tag == "Goal") {
            _flipped = Vector3.Dot (collision.contacts[0].normal, transform.up) < _FLIPPED_THRESHOLD;
            _grounded = true;
            _jumpsLeft = _JUMPS_ALLOWED;
            _groundNormal = collision.contacts[0].normal;
        }
    }

    private void OnCollisionExit(Collision collision) {
        if (collision.collider.tag == "Ground" || collision.collider.tag == "Goal") {
            _grounded = false;
        }
    }
}
