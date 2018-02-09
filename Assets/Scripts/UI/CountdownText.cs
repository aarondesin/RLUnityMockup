// CountdownText.cs
// ©2017 Aaron Desin

using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
    /// <summary>
    /// Singleton behavior for the countdown text.
    /// </summary>
    public sealed class CountdownText : MonoBehaviour
    {
        #region Vars

        /// <summary>
        /// Active instance of the countdown text.
        /// </summary>
        public static CountdownText Instance;

        /// <summary>
        /// The Text component attached to this object.
        /// </summary>
        Text _text;

        #endregion
        #region Unity Callbacks

        private void Awake()
        {
            // Init singleton reference
            Instance = this;

            // Init component References
            _text = GetComponent<Text>();
        }

        #endregion
        #region Methods

        /// <summary>
        /// Sets the text on this countdown object.
        /// </summary>
        public void SetText(string text)
        {
            _text.text = text;
        }

        #endregion
    }
}