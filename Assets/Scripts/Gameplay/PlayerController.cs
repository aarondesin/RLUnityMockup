// PlayerController.cs
// ©2017 Aaron Desin

using RL.Camera;

using UnityEngine;
using UnityEngine.Events;

#pragma warning disable 0414

namespace RL.Gameplay
{
    /// <summary>
    /// Behavior for the player arcade car controller.
    /// </summary>
    public sealed class PlayerController : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Acceleration (UU/s/s).
        /// </summary>
        const float _ACCELERATION = 75f;

        /// <summary>
        /// 
        /// </summary>
        const float _BRAKE_STRENGTH = 75f;

        /// <summary>
        /// Turnability while on the ground.
        /// </summary>
        const float _STEERING_STRENGTH_GROUNDED = 3f;

        /// <summary>
        /// Turnability while in the air.
        /// </summary>
        const float _STEERING_STRENGTH_INAIR = 1f;

        /// <summary>
        /// Maximum unboosted velocity (UU/s).
        /// </summary>
        const float _MAX_VELOCITY_UNBOOSTED = 50f;

        /// <summary>
        /// Maximum velocity while boosted (UU/s).
        /// </summary>
        const float _MAX_VELOCITY_BOOSTED = 75f;

        /// <summary>
        /// Pitch adjustment speed (deg/s).
        /// </summary>
        const float _PITCH_SPEED = 8f;

        /// <summary>
        /// Roll adjustment speed (deg/s).
        /// </summary>
        const float _ROLL_SPEED = 20f;

        /// <summary>
        /// Yaw adjustment speed (deg/s).
        /// </summary>
        const float _YAW_SPEED = 8f;

        /// <summary>
        /// Percentage decay of angular velocity per update.
        /// </summary>
        const float _ANGULAR_VELOCITY_DECAY = 0.96f;

        /// <summary>
        /// Number of jumps allowed until the ground is next touched.
        /// </summary>
        const int _JUMPS_ALLOWED = 2;

        /// <summary>
        /// Physics force from jumping.
        /// </summary>
        const float _JUMP_FORCE = 20f;

        /// <summary>
        /// Angle threshold to the ground within which to dodge-flip.
        /// </summary>
        const float _DODGEFLIP_THRESHOLD = 0.5f;

        /// <summary>
        /// Physical force of dodge-flipping.
        /// </summary>
        const float _DODGEFLIP_FORCE = 15f;

        /// <summary>
        /// Physical torque of dodge-flipping.
        /// </summary>
        const float _DODGEFLIP_TORQUE = 10f;

        /// <summary>
        /// Duration of dodge flip (s).
        /// </summary>
        const float _DODGE_FLIP_DURATION = 0.75f;

        /// <summary>
        /// Percentage decay of dodge flip angular velocity per update.
        /// </summary>
        const float _DODGE_FLIP_ANGULAR_VELOCITY_DECAY = 0.995f;

        /// <summary>
        /// Physical force of flipping.
        /// </summary>
        const float _FLIP_FORCE = 15f;

        /// <summary>
        /// Physical torque of flipping.
        /// </summary>
        const float _FLIP_TORQUE = 1f;

        /// <summary>
        /// Dot threshold to ground to be considered flipped.
        /// </summary>
        const float _FLIPPED_THRESHOLD = 0.98f;
        
        /// <summary>
        /// Maximum held boost.
        /// </summary>
        const float _MAX_BOOST = 100f;

        /// <summary>
        /// Initial boost value when starting a round.
        /// </summary>
        const float _INITIAL_BOOST = 30f;

        /// <summary>
        /// Boost consumption per second.
        /// </summary>
        const float _BOOST_USED_PER_SECOND = 33.33f;

        /// <summary>
        /// Physical force of boosting.
        /// </summary>
        const float _BOOST_FORCE = 2500f;

        /// <summary>
        /// Maximum distance to raycast towards ground.
        /// </summary>
        const float _GROUND_RAYCAST_DIST = 0.2f;

        /// <summary>
        /// Maximum engine pitch.
        /// </summary>
        const float _ENGINE_MAX_PITCH = 1.1f;

        /// <summary>
        /// Minimum engine pitch.
        /// </summary>
        const float _ENGINE_MIN_PITCH = 0.0f;

        #endregion
        #region Vars

        /// <summary>
        /// The team to which this player belongs.
        /// </summary>
        [Tooltip("The team to which this player belongs.")]
        [SerializeField] GameManager.Team _team;

        /// <summary>
        /// Sound to use for car impacts.
        /// </summary>
        [Tooltip("Sound to use for car impacts.")]
        [SerializeField] AudioClip _impactSound;

        /// <summary>
        /// Sound to use when jumping.
        /// </summary>
        [Tooltip("Sound to use when jumping.")]
        [SerializeField] AudioClip _jumpSound;

        /// <summary>
        /// Particle system to use when boosting.
        /// </summary>
        [Tooltip("Particle system to use when boosting.")]
        [SerializeField] ParticleSystem _boostPS;

        /// <summary>
        /// Light to use when boosting.
        /// </summary>
        [Tooltip("Light to use when boosting.")]
        [SerializeField] Light _boostLight;
        AudioSource _boostAudio;

        /// <summary>
        /// The ID index of this player.
        /// </summary>
        int _playerID;

        /// <summary>
        /// The ID index of this player, as a string.
        /// </summary>
        string _playerIDStr;

        /// <summary>
        /// Horizontal/vertical input values for this player.
        /// </summary>
        float _h, _v;

        /// <summary>
        /// Gas/brake input values for this player.
        /// </summary>
        float _g, _b;

        /// <summary>
        /// Is movement currently disabled?
        /// </summary>
        bool _movementDisabled = false;

        /// <summary>
        /// Current boost value.
        /// </summary>
        float _boost = 30f;

        /// <summary>
        /// Is the player currently boosting?
        /// </summary>
        bool _boosting = false;

        /// <summary>
        /// Number of jumps the player can currently do.
        /// </summary>
        int _jumpsLeft = 2;

        /// <summary>
        /// Is the player currently on the ground?
        /// </summary>
        bool _grounded = false;

        /// <summary>
        /// Is the player currently flipped?
        /// </summary>
        bool _flipped = false;

        /// <summary>
        /// Duration of current dodge-flip.
        /// </summary>
        float _dodgeFlipDuration;

        /// <summary>
        /// Engine AudioSource.
        /// </summary>
        AudioSource _engineAudio;

        /// <summary>
        /// The normal vector of the last ground collision.
        /// </summary>
        Vector3 _groundNormal;

        /// <summary>
        /// The Rigidbody attached to this player.
        /// </summary>
        Rigidbody _rb;

        /// <summary>
        /// The camera controller attached to this player.
        /// </summary>
        CameraController _camera;
        
        /// <summary>
        /// Invoked when the player touches the ground.
        /// </summary>
        UnityEvent onGround = new UnityEvent();

        #endregion
        #region Unity Callbacks

        private void Awake()
        {
            // Init component references
            _rb = GetComponent<Rigidbody>();
            _camera = GetComponentInChildren<CameraController>();
            _engineAudio = GetComponent<AudioSource>();
            _boostAudio = _boostPS.GetComponent<AudioSource>();

            // Stop boost effect
            _boostPS.Stop();
            _boostLight.gameObject.SetActive(false);
            _boostAudio.Stop();
        }

        private void FixedUpdate()
        {
            if (_movementDisabled) return;

            // Get updated input values
            HandleInputs();

            // Calculate forward/backward movement
            float forwardValue = _g * _ACCELERATION;
            float backwardValue = _b * _BRAKE_STRENGTH;
            float throttleValue = forwardValue - backwardValue;

            float speedCap = _boosting ? _MAX_VELOCITY_BOOSTED : _MAX_VELOCITY_UNBOOSTED;

            // Calculate steering values
            float steeringMultiplier = _grounded ? (_flipped ? 0f : _STEERING_STRENGTH_GROUNDED) : _STEERING_STRENGTH_INAIR;
            float steeringValue = Input.GetAxis("Horizontal" + _playerIDStr) * steeringMultiplier * Mathf.Sqrt(Mathf.Clamp01(_rb.velocity.magnitude / speedCap));

            if (_grounded)
            {
                // Grounded and upright
                if (!_flipped)
                {
                    var forward = Vector3.ProjectOnPlane(transform.forward, _groundNormal).normalized;

                    var v = _rb.velocity.magnitude;

                    var reverse = Vector3.Dot(_rb.velocity, transform.forward) < 0f;
                    if (reverse)
                    {
                        v *= -1f;
                        steeringValue *= -1;
                    }
                    _rb.MoveRotation(transform.rotation * Quaternion.Euler(0f, steeringValue, 0f));
                    _rb.velocity = forward * v;
                    var force = forward * throttleValue;

                    if (_rb.velocity.magnitude < speedCap - force.magnitude * Time.fixedDeltaTime) _rb.AddForce(force, ForceMode.Acceleration);
                } 
                
                // Grounded and flipped
                else
                {
                    _rb.angularVelocity *= _ANGULAR_VELOCITY_DECAY;
                }
            } 
            
            else
            {
                // Dodge-flipping
                if (_dodgeFlipDuration > 0f)
                {
                    _dodgeFlipDuration -= Time.fixedDeltaTime;
                    _rb.angularVelocity *= _DODGE_FLIP_ANGULAR_VELOCITY_DECAY;

                    if (_dodgeFlipDuration <= 0f)
                    {
                        _rb.useGravity = true;
                    }
                } 
                
                // In-air
                else
                {
                    _rb.AddTorque(transform.right * _PITCH_SPEED * _v, ForceMode.Acceleration);

                    // Add roll torque
                    if (Input.GetButton("Roll" + _playerIDStr))
                        _rb.AddTorque(transform.forward * _ROLL_SPEED * -_h, ForceMode.Acceleration);
                    else
                        _rb.AddTorque(transform.up * _YAW_SPEED * _h, ForceMode.Acceleration);

                    _rb.angularVelocity *= _ANGULAR_VELOCITY_DECAY;
                }
            }

            // Adjust engine audio pitch
            _engineAudio.pitch = _rb.velocity.magnitude / _MAX_VELOCITY_UNBOOSTED * 
                (_ENGINE_MAX_PITCH - _ENGINE_MIN_PITCH) + _ENGINE_MIN_PITCH;

            if (!_rb.detectCollisions) _rb.detectCollisions = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Check if player is grounded
            if (collision.collider.tag == "Ground" || collision.collider.tag == "Goal")
            {
                _flipped = Vector3.Dot(collision.contacts[0].normal, transform.up) < _FLIPPED_THRESHOLD;
                _grounded = true;
                _jumpsLeft = _JUMPS_ALLOWED;
                _groundNormal = AverageContactNormal(collision.contacts);
                _engineAudio.PlayOneShot(_impactSound);

            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (_grounded)
            {
                // Calculate if player is flipped
                _flipped = Vector3.Dot(collision.contacts[0].normal, transform.up) < _FLIPPED_THRESHOLD;
                _groundNormal = AverageContactNormal(collision.contacts);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.collider.tag == "Ground" || collision.collider.tag == "Goal")
            {
                _grounded = false;
                Debug.Log("TookOff");
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, _groundNormal * 5f);
            Gizmos.DrawRay(transform.position, Vector3.ProjectOnPlane(transform.forward, _groundNormal) * 5f);
            Gizmos.DrawRay(transform.position, -transform.up * _GROUND_RAYCAST_DIST);
        }

        #endregion
        #region Properties

        /// <summary>
        /// Returns the current horizontal steering value (read-only).
        /// </summary>
        public float Horizontal { get { return _h; } }

        /// <summary>
        /// Returns the current vertical steering value (read-only).
        /// </summary>
        public float Vertical { get { return _v; } }

        /// <summary>
        /// Returns the current gas value (read-only).
        /// </summary>
        public float Gas { get { return _g; } }

        /// <summary>
        /// Returns the current brake value (read-only).
        /// </summary>
        public float Brake { get { return _b; } }

        /// <summary>
        /// Returns true if this player is grounded (read-only).
        /// </summary>
        public bool Grounded { get { return _grounded; } }

        /// <summary>
        /// Returs true if this player is flipped (read-only).
        /// </summary>
        public bool Flipped { get { return _flipped; } }

        /// <summary>
        /// Returns the amount of boost this player has (read-only).
        /// </summary>
        public float Boost { get { return _boost; } }

        /// <summary>
        /// Returns the percentage of boost this player has (read-only).
        /// </summary>
        public float BoostPercentage { get { return _boost / _MAX_BOOST; } }

        /// <summary>
        /// Returns the current velocity of this player (read-only).
        /// </summary>
        public Vector3 Velocity { get { return _rb.velocity; } }

        /// <summary>
        /// Returns this player's camera controller (read-only).
        /// </summary>
        public CameraController Camera { get { return _camera; } }

        /// <summary>
        /// Returns the team to which this player belongs (read-only).
        /// </summary>
        public GameManager.Team Team { get { return _team; } }

        #endregion
        #region Methods

        /// <summary>
        /// Updates all input values.
        /// </summary>
        void HandleInputs()
        {
            if (Input.GetButtonDown("Jump" + _playerIDStr)) AttemptJump();
            if (Input.GetButton("Boost" + _playerIDStr)) AttemptBoost();
            else if (_boosting) EndBoost();
            _h = Input.GetAxis("Horizontal" + _playerIDStr);
            _v = -Input.GetAxis("Vertical" + _playerIDStr);
            _g = Input.GetAxis("Gas" + _playerIDStr);
            _b = Input.GetAxis("Brake" + _playerIDStr);
        }

        /// <summary>
        /// Attempts to jump.
        /// </summary>
        void AttemptJump()
        {
            if (_grounded)
            {
                if (_flipped) Flip();
                else Jump();
            } 
            
            else if (_jumpsLeft > 0)
            {
                if (Mathf.Abs(_h) > _DODGEFLIP_THRESHOLD ||
                    Mathf.Abs(_v) > _DODGEFLIP_THRESHOLD)
                    DodgeFlip();
                else Jump();
            }
        }

        /// <summary>
        /// Performs a jump.
        /// </summary>
        void Jump()
        {
            // Apply force
            _rb.AddForce(transform.up * _JUMP_FORCE, ForceMode.VelocityChange);
            _rb.MovePosition(transform.position + transform.up * _JUMP_FORCE * Time.fixedDeltaTime);
            _rb.detectCollisions = false;

            _jumpsLeft--;

            _engineAudio.PlayOneShot(_jumpSound);
        }

        /// <summary>
        /// Performs a flip.
        /// </summary>
        void Flip()
        {
            // Get forward vector
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, _groundNormal);

            // Apply force and torque
            var flipFactor = 0.5f - 0.5f * Vector3.Dot(transform.up, _groundNormal);
            _rb.AddForce(_groundNormal * _FLIP_FORCE, ForceMode.VelocityChange);
            _rb.AddTorque(forward * _FLIP_FORCE * flipFactor, ForceMode.VelocityChange);

            _rb.detectCollisions = false;

            _jumpsLeft--;

            _engineAudio.PlayOneShot(_jumpSound);
        }

        /// <summary>
        /// Performs a dodge-flip.
        /// </summary>
        void DodgeFlip()
        {
            // Get forward / right vectors
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, _groundNormal).normalized;
            Vector3 right = Vector3.ProjectOnPlane(transform.right, _groundNormal).normalized;

            // Get vertical vector relative to ground
            Vector3 verticalVelocity = Vector3.Project(_rb.velocity, _groundNormal);

            // Apply force and torque
            _rb.AddTorque(forward * -_h * _DODGEFLIP_TORQUE, ForceMode.VelocityChange);
            _rb.AddTorque(right * _v * _DODGEFLIP_TORQUE, ForceMode.VelocityChange);
            _rb.AddForce(right * _h * _DODGEFLIP_FORCE, ForceMode.VelocityChange);
            _rb.AddForce(forward * _v * _DODGEFLIP_FORCE, ForceMode.VelocityChange);

            _rb.velocity -= verticalVelocity;
            _rb.detectCollisions = false;
            _rb.useGravity = false;

            _dodgeFlipDuration = _DODGE_FLIP_DURATION;
            _jumpsLeft--;

            _engineAudio.PlayOneShot(_jumpSound);
        }

        /// <summary>
        /// Attempts to use boost.
        /// </summary>
        void AttemptBoost()
        {
            if (_boost > 0f) DoBoost();
            else if (_boosting) EndBoost();
        }

        /// <summary>
        /// Shows the boost effect and applies boost physics.
        /// </summary>
        void DoBoost()
        {
            // Add boost force
            Vector3 boostForce = transform.forward * _BOOST_FORCE * Time.fixedDeltaTime;
            _rb.AddForce(boostForce, ForceMode.Acceleration);

            // Consume boost
            float dBoost = _BOOST_USED_PER_SECOND * Time.fixedDeltaTime;
            _boost = Mathf.Clamp(_boost - dBoost, 0f, _MAX_BOOST);

            // Show boost effect
            _boostPS.Play();
            if (!_boostAudio.isPlaying)
                _boostAudio.Play();
            _boosting = true;
            _boostLight.gameObject.SetActive(true);
        }

        /// <summary>
        /// Called when boosting stops.
        /// </summary>
        void EndBoost()
        {
            _boostPS.Stop();
            _boosting = false;
            _boostLight.gameObject.SetActive(false);
            _boostAudio.Stop();
        }

        /// <summary>
        /// Disables this player's movement.
        /// </summary>
        public void DisableMovement()
        {
            _movementDisabled = true;
        }

        /// <summary>
        /// Enables this player's movement.
        /// </summary>
        public void EnableMovement()
        {
            _movementDisabled = false;
        }

        /// <summary>
        /// Resets this player's properties.
        /// </summary>
        public void ResetPlayer()
        {
            // Reset boost
            _boost = _INITIAL_BOOST;

            // Reset velocity / angular velocity
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        /// <summary>
        /// Gives the specified amount of boost to this player.
        /// </summary>
        public void GiveBoost(float boost)
        {
            _boost = Mathf.Clamp(_boost + boost, 0f, _MAX_BOOST);
        }

        /// <summary>
        /// Returns the average normal of contact between a set iof physics
        /// contacts.
        /// </summary>
        Vector3 AverageContactNormal(ContactPoint[] contacts)
        {
            Vector3 result = Vector3.zero;
            for (int i = 0; i < contacts.Length; i++)
            {
                result += contacts[i].normal;
            }
            return result /= (float)contacts.Length;
        }

        /// <summary>
        /// Assigns a player ID to this controller.
        /// </summary>
        public void AssignPlayerID(int id)
        {
            _playerID = id;
            _playerIDStr = id.ToString();
        }

        #endregion
    }
}