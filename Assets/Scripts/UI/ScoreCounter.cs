// ScoreCounter.cs
// ©2017 Aaron Desin

using RL.Gameplay;

using UnityEngine;
using UnityEngine.UI;

namespace RL.UI
{
    /// <summary>
    /// Behavior for the score counter.
    /// </summary>
    public sealed class ScoreCounter : MonoBehaviour
    {
        #region Vars

        /// <summary>
        /// The team that this score counter represents.
        /// </summary>
        [Tooltip("The team that this score counter represents.")]
        [SerializeField] GameManager.Team _team;

        /// <summary>
        /// The text object attached to this score counter.
        /// </summary>
        Text _text;

        #endregion
        #region Unity Callbacks

        private void Awake()
        {
            // Init component references
            _text = GetComponentInChildren<Text>();
        }

        private void Start()
        {
            // Init event listeners
            GameManager.Instance.onGameStarted.AddListener(Refresh);
            GameManager.Instance.onGoalScored.AddListener(Refresh);

            // Init text
            _text.text = 0.ToString();
        }

        #endregion
        #region Methods

        /// <summary>
        /// Refreshes this score counter.
        /// </summary>
        void Refresh()
        {
            _text.text = GameManager.Instance.GetScore(_team).ToString();
        }

        #endregion
    }
}