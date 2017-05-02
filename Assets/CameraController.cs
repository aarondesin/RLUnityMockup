using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    const float _CAMERA_HEIGHT = 4f;
    const float _CAMERA_XZ_OFFSET = 12f;
    const float _MIN_CAMERA_HEIGHT = 4f;
    const float _LAG = 0.9f;

    Mode _mode = Mode.BallCam;

    Vector3 _ballPos;

    bool _inverted = false;

    Camera _camera;

    int _playerID;
    string _playerIDStr;

    PlayerController _player;

	public enum Mode {
        Free,
        BallCam
    }

    private void Awake() {
        _camera = GetComponent<Camera>();
        _player = GetComponentInParent<PlayerController>();
    }

    private void Update() {
        HandleInputs();

        Vector3 lerpedPos;

        switch (_mode) {
            case Mode.Free:
                Vector3 pos = _player.transform.position -transform.forward * _CAMERA_XZ_OFFSET + transform.up * _CAMERA_HEIGHT;
                pos.y = Mathf.Clamp (pos.y, _MIN_CAMERA_HEIGHT, Mathf.Infinity);
                lerpedPos = Vector3.Lerp (pos, transform.position, _LAG);
                transform.position = lerpedPos;

                var parent = transform.parent.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler (0f, parent.y + (_inverted ? 180f : 0f), 0f);
                //Quaternion rot = Quaternion.LookRotation (PlayerController.Instance.Velocity, Vector3.up);
                //transform.rotation = rot;
                break;
            case Mode.BallCam:

                if (Ball.Instance != null)
                    _ballPos = Ball.Instance.transform.position;
                Vector3 playerPos = _player.transform.position;
                
                Vector3 ballToPlayer = playerPos - _ballPos;
                ballToPlayer.y = 0f;
                ballToPlayer = ballToPlayer.normalized;
                float raycastY = -Mathf.Infinity;
                RaycastHit hit;
                if (Physics.Raycast (transform.position, Vector3.down, out hit, Mathf.Infinity)) {
                    raycastY = hit.point.y + _CAMERA_HEIGHT;
                }

                float extrapolatedY = playerPos.y + ballToPlayer.y * _CAMERA_XZ_OFFSET;
                float y = extrapolatedY;
                //float y = Mathf.Max (extrapolatedY, raycastY);

                //float yBound = Mathf.Max (CameraYBoundary.Instance.transform.position.y, );
                float yBound = Mathf.Max (extrapolatedY + _CAMERA_HEIGHT, raycastY);

                Vector3 newPos = new Vector3 (
                    playerPos.x + ballToPlayer.x * _CAMERA_XZ_OFFSET,
                    //playerPos.y + _CAMERA_HEIGHT,
                    Mathf.Clamp(extrapolatedY, yBound, Mathf.Infinity),
                    playerPos.z + ballToPlayer.z * _CAMERA_XZ_OFFSET);
                lerpedPos = Vector3.Lerp (transform.position, newPos, _LAG);
                transform.position = lerpedPos;
                var lookVector = _ballPos - transform.position * (_inverted ? -1f : 1f);
                Quaternion newRot= Quaternion.LookRotation (lookVector);
                Quaternion lerpedRot = Quaternion.Lerp (transform.rotation, newRot, _LAG);
                //transform.LookAt (_ballPos, Vector3.up);
                transform.rotation = lerpedRot;
                break;
        }
    }

    void HandleInputs () {
        if (Input.GetButtonDown("ChangeCamera" + _playerIDStr)) {
            _mode = (Mode)(1 - (int)_mode);
        }

        _inverted = Input.GetButton ("InvertCamera" + _playerIDStr);
    }

    public void AssignScreenRect (Rect screenRect) {
        _camera.pixelRect = screenRect;
    }

    public void AssignPlayerID (int id) {
        _playerID = id;
        _playerIDStr = id.ToString();
    }
}
