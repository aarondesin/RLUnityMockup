// StatsDisplay.cs
// ©2017 Aaron Desin

using RL.Gameplay;

using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

namespace RL.Debugging
{
    /// <summary>
    /// Behavior for debug stats display.
    /// </summary>
    public sealed class StatsDisplay : MonoBehaviour
    {
        #region Vars

        /// <summary>
        /// The type of information to show.
        /// </summary>
        [Tooltip("The type of information to show.")]
        [SerializeField] DisplayType _displayType;

        /// <summary>
        /// The text object attached to this stats display.
        /// </summary>
        Text _text;

        /// <summary>
        /// The target player from which to draw information.
        /// </summary>
        PlayerController _target;

        #endregion
        #region Unity Callbacks

        void Awake()
        {
            // Init component references
            _text = GetComponent<Text>();
        }

        void Update()
        {
            switch (_displayType)
            {
                case DisplayType.Horizontal:
                    _text.text = "Horizontal: " + _target.Horizontal.ToString();
                    break;
                case DisplayType.Vertical:
                    _text.text = "Vertical: " + _target.Vertical.ToString();
                    break;
                case DisplayType.Gas:
                    _text.text = "Gas: " + _target.Gas.ToString();
                    break;
                case DisplayType.Brake:
                    _text.text = "Brake: " + _target.Brake.ToString();
                    break;
                case DisplayType.Grounded:
                    _text.text = "Grounded: " + _target.Grounded.ToString();
                    break;
                case DisplayType.Flipped:
                    _text.text = "Flipped: " + _target.Flipped.ToString();
                    break;
            }
        }

        #endregion
        #region Enums

        public enum DisplayType
        {
            Horizontal,
            Vertical,
            Gas,
            Brake,
            Grounded,
            Flipped
        }

        #endregion
    }
}