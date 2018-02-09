// PlayerSpawnPoint.cs
// ©2017 Aaron Desin

using System.Collections.Generic;

using UnityEngine;

namespace RL.Gameplay
{
    /// <summary>
    /// Behavior for player spawn points.
    /// </summary>
    public sealed class PlayerSpawnPoint : MonoBehaviour
    {
        #region Vars

        /// <summary>
        /// List of all player spawn points.
        /// </summary>
        static List<PlayerSpawnPoint> _spawnPoints = new List<PlayerSpawnPoint>();

        /// <summary>
        /// The team to which this spawn point belongs.
        /// </summary>
        [Tooltip("The team to which this spawn point belongs.")]
        [SerializeField] GameManager.Team _team;

        #endregion
        #region Unity Callbacks

        private void Awake()
        {
            // Register this spawn point
            _spawnPoints.Add(this);
        }

        #endregion
        #region Properties

        /// <summary>
        /// Returns the team to which this spawn point belongs (read-only).
        /// </summary>
        public GameManager.Team Team { get { return _team; } }

        #endregion
        #region Methods

        /// <summary>
        /// Returns a spawn point for the given team.
        /// </summary>
        public static PlayerSpawnPoint GetTeamSpawnPoint(GameManager.Team team)
        {
            foreach (var spawnPoint in _spawnPoints)
                if (spawnPoint.Team == team) return spawnPoint;

            return null;
        }

        #endregion
    }
}