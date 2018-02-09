// BallSpawnPoint.cs
// ©2017 Aaron Desin

using UnityEngine;

namespace RL.Gameplay
{
    /// <summary>
    /// Behavior to mark the ball spawn point.
    /// </summary>
    public sealed class BallSpawnPoint : MonoBehaviour
    {
        #region Vars

        /// <summary>
        /// The active ball spawn point instance.
        /// </summary>
        public static BallSpawnPoint Instance;

        #endregion
        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;
        }

        #endregion
    }
}