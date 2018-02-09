// BoostMeter.cs
// ©2017 Aaron Desin

using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
    /// <summary>
    /// Behavior for the player boost meter.
    /// </summary>
    public sealed class BoostMeter : MonoBehaviour
    {
        #region Vars

        /// <summary>
        /// Index of the player to which this boost meter belongs.
        /// </summary>
        [Tooltip("Index of the player to which this boost meter belongs.")]
        [SerializeField] int _player;

        /// <summary>
        /// Text attached to the boost meter.
        /// </summary>
        Text _text;

        /// <summary>
        /// Image attached to the boost meter.
        /// </summary>
        Image _image;

        /// <summary>
        /// Target player.
        /// </summary>
        PlayerController _target;

        #endregion
        #region Unity Callbacks

        void Awake()
        {
            // Init component references
            _text = GetComponentInChildren<Text>();
            _image = GetComponentInChildren<Image>();
        }

        private void Update()
        {
            // Init target player reference if necessary
            if (_target == null)
                _target = GameManager.Instance.Players[_player];

            // Update text/image
            _text.text = _target.Boost.ToString("#0");
            _image.fillAmount = _target.BoostPercentage;
        }

        #endregion
    }
}