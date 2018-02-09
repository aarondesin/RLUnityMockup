// Ball.cs
// ©2017 Aaron Desin

using UnityEngine;

namespace RL.Gameplay
{
    /// <summary>
    /// Behavior for the ball.
    /// </summary>
    public sealed class Ball : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Force of goal explosion.
        /// </summary>
        const float _EXPLOSION_FORCE = 200000f;

        /// <summary>
        /// Radius of goal explosion force.
        /// </summary>
        const float _EXPLOSION_RADIUS = 100f;

        #endregion
        #region Vars

        /// <summary>
        /// Active instance of the ball.
        /// </summary>
        public static Ball Instance;

        /// <summary>
        /// Sound to play on impact.
        /// </summary>
        [Tooltip("Sound to play on impact.")]
        [SerializeField] AudioClip _impactSound;

        /// <summary>
        /// Sound to play on goal explosion.
        /// </summary>
        [Tooltip("Sound to play on goal explosion.")]
        [SerializeField] AudioClip _explosionSound;

        /// <summary>
        /// The MeshRenderer attached to the ball.
        /// </summary>
        MeshRenderer _renderer;

        /// <summary>
        /// The AudioSource attached to the ball.
        /// </summary>
        AudioSource _audioSource;

        /// <summary>
        /// Explosion effects to play when a goal is scored.
        /// </summary>
        ParticleSystem[] _explosionEffects;

        #endregion
        #region Unity Callbacks

        private void Awake()
        {
            // Set instance
            Instance = this;

            // Init component references
            _renderer = GetComponent<MeshRenderer>();
            _explosionEffects = GetComponentsInChildren<ParticleSystem>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if entered goal
            if (other.tag == "Goal")
            {
                Explode();

                // Register goal scored
                Goal goalEntered = other.GetComponent<Goal>();
                GameManager.Team otherTeam = (GameManager.Team)(1 - (int)goalEntered.Team);
                GameManager.Instance.RegisterGoal(otherTeam);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Play impact sound
            float volume = Mathf.Clamp01(collision.relativeVelocity.magnitude / 75f);
            _audioSource.PlayOneShot(_impactSound, volume);
        }

        private void OnDrawGizmosSelected()
        {
            // Show explosion radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _EXPLOSION_RADIUS);
        }

        #endregion
        #region Methods

        /// <summary>
        /// Shows the explosion effect on this ball.
        /// </summary>
        void Explode()
        {
            // Show explosion effect
            foreach (ParticleSystem ps in _explosionEffects) ps.Play();
            _audioSource.PlayOneShot(_explosionSound, 0.75f);

            // Apply force to players
            foreach (var player in GameManager.Instance.Players)
            {
                player.GetComponent<Rigidbody>().
                    AddExplosionForce(_EXPLOSION_FORCE, transform.position, 
                    _EXPLOSION_RADIUS, 1f, ForceMode.Impulse);
            }

            // Hide ball
            _renderer.enabled = false;
        }

        /// <summary>
        /// Resets the physical properties of the ball.
        /// </summary>
        public void ResetBall()
        {
            // Reset velocity / angular velocity
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            // Show ball
            _renderer.enabled = true;
        }

        #endregion
    }
}