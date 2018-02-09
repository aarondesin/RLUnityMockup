// CameraYBoundary.cs
// ©2017 Aaron Desin

using UnityEngine;

namespace RL.Camera
{
    /// <summary>
    /// Singleton behavior to establish minimum y-value for all camera 
    /// controllers.
    /// </summary>
    public sealed class CameraYBoundary : MonoBehaviour
    {
        #region Vars

        /// <summary>
        /// The active instance of the camera y-boundary.
        /// </summary>
        public static CameraYBoundary Instance;

        #endregion
        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;
        }

        #endregion
    }
}
