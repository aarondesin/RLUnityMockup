// BoostPickup.cs
// ©2017 Aaron Desin

using System.Collections;

using UnityEngine;

namespace RL.Gameplay
{
    /// <summary>
    /// Behavior for the boost pickups.
    /// </summary>
    public sealed class BoostPickup : MonoBehaviour
    {
        #region Vars

        /// <summary>
        /// Amount of boost given when a player picks up this powerup.
        /// </summary>
        [Tooltip("Amount of boost given when a player picks up this powerup.")]
        [SerializeField] float _boostValue;

        /// <summary>
        /// Length of time taken for this boost pickup to respawn (seconds).
        /// </summary>
        [Tooltip("Length of time taken for this boost pickup to respawn (seconds).")]
        [SerializeField] float _respawnTime;

        /// <summary>
        /// Boost effect to show/hide when active/inactive.
        /// </summary>
        [Tooltip("Boost effect to show/hide when active/inactive.")]
        [SerializeField] GameObject _boostEffect;

        /// <summary>
        /// Sound to play when this boost pickup is collected.
        /// </summary>
        [Tooltip("Sound to play when this boost pickup is collected.")]
        [SerializeField] AudioClip _collectSound;

        /// <summary>
        /// Is this boost pickup enabled?
        /// </summary>
        bool _enabled = true;

        /// <summary>
        /// The collider attached to this pickup.
        /// </summary>
        Collider _collider;

        /// <summary>
        /// The AudioSource attached to this pickup.
        /// </summary>
        AudioSource _audioSource;

        #endregion
        #region Unity Callbacks

        void Awake()
        {
            // Init component references
            _collider = GetComponentInChildren<Collider>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if player
            if (other.tag == "Player" && _enabled)
            {
                // Give boost to player
                var player = other.GetComponent<PlayerController>();
                player.GiveBoost(_boostValue);
                _audioSource.PlayOneShot(_collectSound, 1f);

                // Disable and wait to re-enable
                Disable();
                StartCoroutine(WaitForReenable());
            }
        }

        #endregion
        #region Methods

        /// <summary>
        /// Waits for a fixed amount of time, and then reenables.
        /// </summary>
        /// <returns></returns>
        IEnumerator WaitForReenable()
        {
            yield return new WaitForSeconds(_respawnTime);

            Enable();
            yield break;
        }

        /// <summary>
        /// Enables this boost pickup.
        /// </summary>
        void Enable()
        {
            _enabled = true;
            _boostEffect.SetActive(true);
        }

        /// <summary>
        /// Disables this boost pickup.
        /// </summary>
        void Disable()
        {
            _enabled = false;
            _boostEffect.SetActive(false);
        }

        #endregion
    }
}