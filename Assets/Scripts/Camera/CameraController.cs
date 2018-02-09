// CameraController.cs
// ©2017 Aaron Desin

using RL.Gameplay;

using UnityEngine;

namespace RL.Camera
{
    /// <summary>
    /// Behavior for the camera controller.
    /// </summary>
    public sealed class CameraController : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Height above players at which the camera stays.
        /// </summary>
        const float _CAMERA_HEIGHT = 2f;

        /// <summary>
        /// XZ plane distance from player at which camera stays.
        /// </summary>
        const float _CAMERA_XZ_OFFSET = 10f;

        /// <summary>
        /// Minimum world-space camera height above which camera stays.
        /// </summary>
        const float _MIN_CAMERA_HEIGHT = 4f;

        /// <summary>
        /// Camera movement lag/lerp value.
        /// </summary>
        const float _LAG = 0.9f;

        #endregion
        #region Vars

        /// <summary>
        /// Current mode in which this camera is operating.
        /// </summary>
        Mode _mode = Mode.BallCam;

        /// <summary>
        /// Last ball position.
        /// </summary>
        Vector3 _ballPos;

        /// <summary>
        /// Is the camera inverted?
        /// </summary>
        bool _inverted = false;

        /// <summary>
        /// The Unity camera attached to this object.
        /// </summary>
        UnityEngine.Camera _camera;

        /// <summary>
        /// ID of the player to follow.
        /// </summary>
        int _playerID;

        /// <summary>
        /// String of the ID of the player to follow.
        /// </summary>
        string _playerIDStr;

        /// <summary>
        /// Player to follow.
        /// </summary>
        PlayerController _player;

        #endregion
        #region Unity Callbacks

        private void Awake()
        {
            // Init component references
            _camera = GetComponent<UnityEngine.Camera>();
            _player = GetComponentInParent<PlayerController>();
        }

        private void Update()
        {
            // Get inputs
            HandleInputs();

            Vector3 lerpedPos;
            switch (_mode)
            {
                // Do free-cam movement
                case Mode.Free:
                    // Place camera relative to player
                    Vector3 pos = _player.transform.position - 
                        transform.forward * _CAMERA_XZ_OFFSET + 
                        transform.up * _CAMERA_HEIGHT;

                    // Clamp camera height
                    pos.y = Mathf.Clamp(pos.y, _MIN_CAMERA_HEIGHT, Mathf.Infinity);

                    // Lerp towards position
                    lerpedPos = Vector3.Lerp(pos, transform.position, _LAG);
                    transform.position = lerpedPos;

                    // Use car Y rotation
                    Vector3 parentRotation = transform.parent.rotation.eulerAngles;
                    transform.rotation = Quaternion.Euler(0f, parentRotation.y + (_inverted ? 180f : 0f), 0f);
    
                    break;

                // Do ball-cam movement
                case Mode.BallCam:

                    // Update ball position
                    if (Ball.Instance != null)
                        _ballPos = Ball.Instance.transform.position;

                    Vector3 playerPos = _player.transform.position;

                    // Get normalized XZ direction from player to ball
                    Vector3 ballToPlayer = playerPos - _ballPos;
                    ballToPlayer.y = 0f;
                    ballToPlayer = ballToPlayer.normalized;

                    // Raycast down to floor
                    float raycastY = -Mathf.Infinity;
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity))
                    {
                        raycastY = hit.point.y + _CAMERA_HEIGHT;
                    }

                    // Bind y position between player and floor
                    float extrapolatedY = playerPos.y + ballToPlayer.y * _CAMERA_XZ_OFFSET;
                    float y = extrapolatedY;
                    float yBound = Mathf.Max(extrapolatedY + _CAMERA_HEIGHT, raycastY);

                    // Calculate new camera position
                    Vector3 newPos = new Vector3(
                        playerPos.x + ballToPlayer.x * _CAMERA_XZ_OFFSET,
                        Mathf.Clamp(extrapolatedY, yBound, Mathf.Infinity),
                        playerPos.z + ballToPlayer.z * _CAMERA_XZ_OFFSET);

                    // Lerp towards new position
                    lerpedPos = Vector3.Lerp(transform.position, newPos, _LAG);
                    transform.position = lerpedPos;

                    // Lerp towards new rotation
                    var lookVector = _ballPos - transform.position * (_inverted ? -1f : 1f);
                    Quaternion newRot = Quaternion.LookRotation(lookVector);
                    Quaternion lerpedRot = Quaternion.Lerp(transform.rotation, newRot, _LAG);
                    transform.rotation = lerpedRot;
                    break;
            }
        }

        void HandleInputs()
        {
            // Check if camera mode was switched
            if (Input.GetButtonDown("ChangeCamera" + _playerIDStr))
            {
                _mode = (Mode)(1 - (int)_mode);
            }

            // Check if camera is inverted
            _inverted = Input.GetButton("InvertCamera" + _playerIDStr);
        }

        /// <summary>
        /// Assigns a particular screen rect to this camera.
        /// </summary>
        public void AssignScreenRect(Rect screenRect)
        {
            _camera.pixelRect = screenRect;
        }

        /// <summary>
        /// Assigns a particular player ID to this camera controller.
        /// </summary>
        public void AssignPlayerID(int id)
        {
            _playerID = id;
            _playerIDStr = id.ToString();
        }

        #endregion
        #region Enums

        public enum Mode
        {
            Free,   // Faces directly forward relative to the car
            BallCam // Always faces towards the ball
        }

        #endregion
    }
}