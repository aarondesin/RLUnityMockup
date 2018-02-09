// RoundTimer.cs
// ©2017 Aaron Desin

using RL.Gameplay;

using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
    /// <summary>
    /// Behavior for the UI round timer.
    /// </summary>
    public sealed class RoundTimer : MonoBehaviour
    {
        #region Vars

        /// <summary>
        /// Active round timer instance.
        /// </summary>
        public static RoundTimer Instance;

        /// <summary>
        /// The text object attached to the round timer.
        /// </summary>
        Text _text;

        #endregion
        #region Unity Callbacks

        private void Awake()
        {
            // Init singleton reference
            Instance = this;

            // Init component references
            _text = GetComponentInChildren<Text>();
        }

        void Update()
        {
            if (GameManager.Instance.TimerRunning)
            {
                // Update text
                var t = GameManager.Instance.TimeLeft;
                var m = Mathf.FloorToInt(t / 60f);
                var s = (Mathf.Floor(t % 60f)).ToString("0#");
                _text.text = string.Format("{0}:{1}", m, s);
            }
        }

        #endregion
    }
}