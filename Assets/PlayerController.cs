using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour {

    const float _ACCELERATION = 75f;
    const float _BRAKE_STRENGTH = 75f;
    const float _STEERING_STRENGTH_GROUNDED = 3f;
    const float _STEERING_STRENGTH_INAIR = 1f;
    const float _MAX_VELOCITY_UNBOOSTED = 50f;
    const float _MAX_VELOCITY_BOOSTED = 75f;
    const float _PITCH_SPEED = 8f;
    const float _ROLL_SPEED = 20f;
    const float _YAW_SPEED = 8f;

    const float _JUMP_FORCE = 20f;
    const float _DODGEFLIP_THRESHOLD = 0.5f;
    const float _DODGEFLIP_FORCE = 15f;
    const float _DODGEFLIP_TORQUE = 10f;
    const float _FLIP_FORCE = 15f;
    const float _FLIP_TORQUE = 1f;
    const float _FLIPPED_THRESHOLD = 0.98f;
    const float _DODGE_FLIP_DURATION = 0.75f;
    const float _DODGE_FLIP_ANGULAR_VELOCITY_DECAY = 0.995f;
    const float _ANGULAR_VELOCITY_DECAY = 0.96f;

    const float _MAX_BOOST = 100f;
    const float _INITIAL_BOOST = 30f;
    const float _BOOST_USED_PER_SECOND = 33.33f;
    const float _BOOST_FORCE = 2500f;

    const float _GROUND_RAYCAST_DIST = 0.2f;

    const float _ENGINE_MAX_PITCH = 1.1f;
    const float _ENGINE_MIN_PITCH = 0.0f;

    [SerializeField] ParticleSystem _boostPS;
    [SerializeField] Light _boostLight;
    AudioSource _boostAudio;
    AudioSource _engineAudio;

    [SerializeField] GameManager.Team _team;

    [SerializeField] AudioClip _impactSound;
    [SerializeField] AudioClip _jumpSound;

    const int _JUMPS_ALLOWED = 2;

    float _boost = 30f;
    bool _boosting = false;

    UnityEvent onGround = new UnityEvent();

    bool _grounded = false;
    bool _flipped = false;

    Vector3 _groundNormal;

    int _jumpsLeft = 2;

    float _dodgeFlipDuration;

    bool _movementDisabled = false;

    int _playerID;
    string _playerIDStr;

    float _h;
    float _v;
    float _g;
    float _b;

    Rigidbody _rb;

    CameraController _camera;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
        _boostPS.Stop();
        _boostLight.gameObject.SetActive(false);
        _camera = GetComponentInChildren<CameraController>();
        _engineAudio = GetComponent<AudioSource>();
        _boostAudio = _boostPS.GetComponent<AudioSource>();
        _boostAudio.Stop();
    }

    private void FixedUpdate() {
        if (_movementDisabled) return;

        HandleInputs();

        float forwardValue = _g * _ACCELERATION;
        float backwardValue = _b * _BRAKE_STRENGTH;

        float throttleValue = forwardValue - backwardValue;

        float speedCap = _boosting ? _MAX_VELOCITY_BOOSTED : _MAX_VELOCITY_UNBOOSTED;
        
        float steeringMultiplier = _grounded ? (_flipped ? 0f : _STEERING_STRENGTH_GROUNDED) : _STEERING_STRENGTH_INAIR;
        float steeringValue = Input.GetAxis("Horizontal" + _playerIDStr) * steeringMultiplier * Mathf.Sqrt (Mathf.Clamp01(_rb.velocity.magnitude / speedCap));

        //if (!_grounded && Physics.Raycast(transform.position, -transform.up, _GROUND_RAYCAST_DIST)) _grounded = true;


        if (_grounded) {
            if (!_flipped) {
                //_rb.MovePosition (transform.forward * throttleValue * Time.fixedDeltaTime);

                var forward = Vector3.ProjectOnPlane(transform.forward, _groundNormal).normalized;

                var v = _rb.velocity.magnitude;

                var reverse = Vector3.Dot (_rb.velocity, transform.forward) < 0f;
                if (reverse) {
                    v *= -1f;
                    steeringValue *= -1;
                }
                _rb.MoveRotation(transform.rotation * Quaternion.Euler(0f, steeringValue, 0f));
                _rb.velocity = forward * v;
                var force = forward * throttleValue;

                if (_rb.velocity.magnitude < speedCap - force.magnitude * Time.fixedDeltaTime) _rb.AddForce(force, ForceMode.Acceleration);
                /*if (_rb.velocity.magnitude > speedCap) {
                    Debug.Log ("Above speed cap!");
                    float dSpeed = _rb.velocity.magnitude - speedCap * Time.fixedDeltaTime;
                    _rb.velocity -= _rb.velocity.normalized * dSpeed;
                }*/
                //_rb.MovePosition (transform.position + transform.forward * (_rb.velocity.magnitude + throttleValue * Time.fixedDeltaTime));
                //_rb.velocity = transform.forward * (_rb.velocity.magnitude + throttleValue * Time.fixedDeltaTime);
            } else {
                _rb.angularVelocity *= _ANGULAR_VELOCITY_DECAY;
            }
        } else {
            if (_dodgeFlipDuration > 0f) {
                _dodgeFlipDuration -= Time.fixedDeltaTime;
                _rb.angularVelocity *= _DODGE_FLIP_ANGULAR_VELOCITY_DECAY;

                if (_dodgeFlipDuration <= 0f) {
                    _rb.useGravity = true;
                }
            } else {
                _rb.AddTorque(transform.right * _PITCH_SPEED * _v, ForceMode.Acceleration);
                if (Input.GetButton ("Roll" + _playerIDStr))
                    _rb.AddTorque (transform.forward * _ROLL_SPEED * -_h, ForceMode.Acceleration);
                else 
                    _rb.AddTorque(transform.up * _YAW_SPEED * _h, ForceMode.Acceleration);
                

                _rb.angularVelocity *= _ANGULAR_VELOCITY_DECAY;
            }
        }

        _engineAudio.pitch = _rb.velocity.magnitude / _MAX_VELOCITY_UNBOOSTED * (_ENGINE_MAX_PITCH - _ENGINE_MIN_PITCH) + _ENGINE_MIN_PITCH;

        if (!_rb.detectCollisions) _rb.detectCollisions = true;
    }

    public float Horizontal { get { return _h; } }

    public float Vertical { get { return _v; } }

    public float Gas { get { return _g; } }

    public float Brake { get { return _b; } }

    public bool Grounded { get { return _grounded; } }

    public bool Flipped { get { return _flipped; } }

    public float Boost { get { return _boost; } }

    public float BoostPercentage { get { return _boost / _MAX_BOOST; } }

    public Vector3 Velocity { get { return _rb.velocity; } }

    public CameraController Camera { get { return _camera; } }

    public GameManager.Team Team { get { return _team; } }

    void HandleInputs() {
        if (Input.GetButtonDown("Jump" + _playerIDStr)) AttemptJump();
        if (Input.GetButton("Boost" + _playerIDStr)) AttemptBoost();
        else if (_boosting) EndBoost();
        _h = Input.GetAxis("Horizontal" + _playerIDStr);
        _v = -Input.GetAxis("Vertical" + _playerIDStr);
        _g = Input.GetAxis("Gas" + _playerIDStr);
        _b = Input.GetAxis("Brake" + _playerIDStr);

        //Debug.Log (_playerIDStr + " " + _g.ToString() + " " + _b.ToString());
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
        _engineAudio.PlayOneShot (_jumpSound);
    }

    void Flip() {
        Debug.Log("Flip");
        _rb.AddForce(_groundNormal * _FLIP_FORCE, ForceMode.VelocityChange);

        var forward = Vector3.ProjectOnPlane(transform.forward, _groundNormal);

        var flipFactor = 0.5f - 0.5f * Vector3.Dot(transform.up, _groundNormal);

        _rb.AddTorque(forward * _FLIP_FORCE * flipFactor, ForceMode.VelocityChange);
        _rb.detectCollisions = false;
        _jumpsLeft--;
        _engineAudio.PlayOneShot (_jumpSound);
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
        _engineAudio.PlayOneShot (_jumpSound);
    }

    void AttemptBoost() {
        Debug.Log("AttemptBoost");
        if (_boost > 0f) DoBoost();
        else if (_boosting) EndBoost();
    }

    void DoBoost () {
        Debug.Log ("Boost");
        _boostPS.Play();
        if (!_boostAudio.isPlaying)
            _boostAudio.Play();
        _boosting = true;
        float dBoost = _BOOST_USED_PER_SECOND * Time.fixedDeltaTime;
        _boost = Mathf.Clamp (_boost - dBoost, 0f, _MAX_BOOST);
        _boostLight.gameObject.SetActive(true);
        var boostForce = transform.forward * _BOOST_FORCE * Time.fixedDeltaTime;
        _rb.AddForce (boostForce, ForceMode.Acceleration);
    }

    void EndBoost () {
        Debug.Log ("EndBoost");
        _boostPS.Stop();
        _boosting = false;
        _boostLight.gameObject.SetActive(false);
        _boostAudio.Stop();
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.tag == "Ground" || collision.collider.tag == "Goal") {
            _flipped = Vector3.Dot(collision.contacts[0].normal, transform.up) < _FLIPPED_THRESHOLD;
            _grounded = true;
            _jumpsLeft = _JUMPS_ALLOWED;
            _groundNormal = AverageContactNormal(collision.contacts);
            Debug.Log ("Landed");
            _engineAudio.PlayOneShot (_impactSound);

        }
    }

    private void OnCollisionStay(Collision collision) {
        if (_grounded) {
            _flipped = Vector3.Dot(collision.contacts[0].normal, transform.up) < _FLIPPED_THRESHOLD;
            _groundNormal = AverageContactNormal(collision.contacts);

        }
    }

    private void OnCollisionExit(Collision collision) {
        if (collision.collider.tag == "Ground" || collision.collider.tag == "Goal") {
            _grounded = false;
            Debug.Log ("TookOff");
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawRay (transform.position, _groundNormal * 5f);
        Gizmos.DrawRay (transform.position, Vector3.ProjectOnPlane (transform.forward, _groundNormal) * 5f);
        Gizmos.DrawRay (transform.position, -transform.up * _GROUND_RAYCAST_DIST);
    }

    public void DisableMovement() {
        _movementDisabled = true;
    }

    public void EnableMovement() {
        _movementDisabled = false;
    }

    public void ResetPlayer () {
        _boost = _INITIAL_BOOST;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    public void GiveBoost (float boost) {
        _boost = Mathf.Clamp (_boost + boost, 0f, _MAX_BOOST);
    }

    Vector3 AverageContactNormal (ContactPoint[] contacts) {
        Vector3 result = Vector3.zero;
        for (int i = 0; i < contacts.Length; i++) {
            result += contacts[i].normal;
        }
        return result /= (float)contacts.Length;
    }

    public void AssignPlayerID (int id) {
        _playerID = id;
        _playerIDStr = id.ToString();
    }
}
