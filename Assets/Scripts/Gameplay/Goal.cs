// Goal.cs
// ©2017 Aaron Desin

using UnityEngine;

namespace RL.Gameplay
{
    /// <summary>
    /// Behavior for a goal.
    /// </summary>
    public sealed class Goal : MonoBehaviour
    {
        #region Vars

        /// <summary>
        /// The team to which this goal belongs.
        /// </summary>
        [Tooltip("The team to which this goal belongs.")]
        [SerializeField] GameManager.Team _team;

        #endregion
        #region Properties

        /// <summary>
        /// Returns the team to which this goal belongs (read-only).
        /// </summary>
        public GameManager.Team Team { get { return _team; } }

        #endregion
    }
}