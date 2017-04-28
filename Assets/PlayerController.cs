using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour {

    const float _ACCELERATION = 200f;
    const float _BRAKE_STRENGTH = 200f;
    const float _STEERING_STRENGTH_GROUNDED = 2.25f;
    const float _STEERING_STRENGTH_INAIR = 1f;
    const float _MAX_VELOCITY_UNBOOSTED = 40f;
    const float _PITCH_SPEED = 10f;
    const float _ROLL_SPEED = 10f;
    const float _YAW_SPEED = 10f;

    const float _JUMP_FORCE = 15f;
    const float _DODGEFLIP_THRESHOLD = 0.5f;
    const float _DODGEFLIP_FORCE = 15f;
    const float _DODGEFLIP_TORQUE = 10f;
    const float _FLIP_FORCE = 10f;
    const float _FLIP_TORQUE = 1f;
    const float _FLIPPED_THRESHOLD = 0.98f;
    const float _DODGE_FLIP_DURATION = 0.75f;
    const float _DODGE_FLIP_ANGULAR_VELOCITY_DECAY = 0.995f;
    const float _ANGULAR_VELOCITY_DECAY = 0.96f;

    const int _JUMPS_ALLOWED = 2;

    public static PlayerController Instance;

    float _boost = 30f;

    UnityEvent onGround = new UnityEvent();

    bool _grounded = false;
    bool _flipped = false;

    Vector3 _groundNormal;

    int _jumpsLeft = 2;

    float _dodgeFlipDuration;

    bool _movementDisabled = false;

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

        if (_movementDisabled) return;

        float forwardValue = _g * _ACCELERATION;
        float backwardValue = _b * _BRAKE_STRENGTH;

        float throttleValue = forwardValue - backwardValue;
        
        float steeringMultiplier = _grounded ? (_flipped ? 0f : _STEERING_STRENGTH_GROUNDED) : _STEERING_STRENGTH_INAIR;
        float steeringValue = Input.GetAxis("Horizontal") * steeringMultiplier * Mathf.Clamp01(_rb.velocity.magnitude / _MAX_VELOCITY_UNBOOSTED);
        if (_grounded) {
            if (!_flipped) {
                //_rb.MovePosition (transform.forward * throttleValue * Time.fixedDeltaTime);

                var forward = Vector3.ProjectOnPlane(transform.forward, _groundNormal).normalized;

                var v = _rb.velocity.magnitude;
                if (Vector3.Dot (_rb.velocity, transform.forward) < 0f) v *= -1f;
                _rb.MoveRotation(transform.rotation * Quaternion.Euler(0f, steeringValue, 0f));
                _rb.velocity = forward * v;
                var force = forward * throttleValue;

                _rb.AddForce(force, ForceMode.Acceleration);
                if (_rb.velocity.magnitude > _MAX_VELOCITY_UNBOOSTED)
                    _rb.velocity *= _MAX_VELOCITY_UNBOOSTED / _rb.velocity.magnitude;
                //_rb.MovePosition (transform.position + transform.forward * (_rb.velocity.magnitude + throttleValue * Time.fixedDeltaTime));
                //_rb.velocity = transform.forward * (_rb.velocity.magnitude + throttleValue * Time.fixedDeltaTime);
            }
        } else {
            if (_dodgeFlipDuration > 0f) {
                _dodgeFlipDuration -= Time.fixedDeltaTime;
                _rb.angularVelocity *= _DODGE_FLIP_ANGULAR_VELOCITY_DECAY;

                if (_dodgeFlipDuration <= 0f) {
                    _rb.useGravity = true;
                }
            } else {
                _rb.AddTorque(transform.up * _YAW_SPEED * _h, ForceMode.Acceleration);
                _rb.AddTorque(transform.right * _PITCH_SPEED * _v, ForceMode.Acceleration);

                _rb.angularVelocity *= _ANGULAR_VELOCITY_DECAY;
            }
        }




        if (!_rb.detectCollisions) _rb.detectCollisions = true;
    }

    public float Horizontal { get { return _h; } }

    public float Vertical { get { return _v; } }

    public float Gas { get { return _g; } }

    public float Brake { get { return _b; } }

    public bool Grounded { get { return _grounded; } }

    public bool Flipped { get { return _flipped; } }

    void HandleInputs() {
        if (Input.GetButtonDown("Jump")) AttemptJump();
        if (Input.GetButton("Boost")) AttemptBoost();
        _h = Input.GetAxis("Horizontal");
        _v = -Input.GetAxis("Vertical");
        _g = Input.GetAxis("Gas");
        _b = Input.GetAxis("Brake");
    }

    void AttemptJump() {
        Debug.Log("AttemptJump");
        if (_grounded) {
            if (_flipped) Flip();
            else Jump();
        } else if (_jumpsLeft > 0) {
            if (Mathf.Abs(_h) > _DODGEFLIP_THRESHOLD ||
                Mathf.Abs(_v) > _DODGEFLIP_THRESHOLD)
                DodgeFlip();
            else Jump();
        }
    }

    void Jump() {
        Debug.Log("Jump");
        _rb.AddForce(transform.up * _JUMP_FORCE, ForceMode.VelocityChange);
        _rb.MovePosition(transform.position + transform.up * _JUMP_FORCE * Time.fixedDeltaTime);
        _rb.detectCollisions = false;
        _jumpsLeft--;
    }

    void Flip() {
        Debug.Log("Flip");
        _rb.AddForce(_groundNormal * _FLIP_FORCE, ForceMode.VelocityChange);

        var forward = Vector3.ProjectOnPlane(transform.forward, _groundNormal);

        var flipFactor = 0.5f - 0.5f * Vector3.Dot(transform.up, _groundNormal);

        _rb.AddTorque(forward * _FLIP_FORCE * flipFactor, ForceMode.VelocityChange);
        _rb.detectCollisions = false;
        _jumpsLeft--;
    }

    void DodgeFlip() {
        Debug.Log("DodgeFlip");
        var forward = Vector3.ProjectOnPlane(transform.forward, _groundNormal).normalized;
        var right = Vector3.ProjectOnPlane(transform.right, _groundNormal).normalized;
        var verticalVelocity = Vector3.Project(_rb.velocity, _groundNormal);

        _rb.AddTorque(forward * -_h * _DODGEFLIP_TORQUE, ForceMode.VelocityChange);
        _rb.AddTorque(right * _v * _DODGEFLIP_TORQUE, ForceMode.VelocityChange);
        _rb.AddForce(right * _h * _DODGEFLIP_FORCE, ForceMode.VelocityChange);
        _rb.AddForce(forward * _v * _DODGEFLIP_FORCE, ForceMode.VelocityChange);

        _rb.velocity -= verticalVelocity;
        _rb.detectCollisions = false;
        _rb.useGravity = false;
        _dodgeFlipDuration = _DODGE_FLIP_DURATION;
        _jumpsLeft--;
    }

    void AttemptBoost() {
        Debug.Log("Boost");
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.tag == "Ground" || collision.collider.tag == "Goal") {
            _flipped = Vector3.Dot(collision.contacts[0].normal, transform.up) < _FLIPPED_THRESHOLD;
            _grounded = true;
            _jumpsLeft = _JUMPS_ALLOWED;
            _groundNormal = collision.contacts[0].normal;
        }
    }

    private void OnCollisionStay(Collision collision) {
        if (_grounded) {
            _flipped = Vector3.Dot(collision.contacts[0].normal, transform.up) < _FLIPPED_THRESHOLD;
        }
    }

    private void OnCollisionExit(Collision collision) {
        if (collision.collider.tag == "Ground" || collision.collider.tag == "Goal") {
            _grounded = false;
        }
    }

    public void DisableMovement() {
        _movementDisabled = true;
    }

    public void EnableMovement() {
        _movementDisabled = false;
    }
}
