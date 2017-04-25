using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    const float _CAMERA_HEIGHT = 5f;
    const float _CAMERA_XZ_OFFSET = 12f;
    const float _MIN_CAMERA_HEIGHT = 5f;

    Mode _mode = Mode.BallCam;

    Vector3 _ballPos;

	public enum Mode {
        Free,
        BallCam
    }

    private void Update() {
        HandleInputs();

        switch (_mode) {
            case Mode.Free:
                Vector3 pos = PlayerController.Instance.transform.position -transform.forward * _CAMERA_XZ_OFFSET + transform.up * _CAMERA_HEIGHT;
                pos.y = Mathf.Clamp (pos.y, _MIN_CAMERA_HEIGHT, Mathf.Infinity);

                transform.position = pos;

                var parent = transform.parent.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler (0f, parent.y, 0f);
                break;
            case Mode.BallCam:

                if (Ball.Instance != null)
                    _ballPos = Ball.Instance.transform.position;
                Vector3 playerPos = PlayerController.Instance.transform.position;
                
                Vector3 ballToPlayer = playerPos - _ballPos;
                //ballToPlayer.y = 0f;
                ballToPlayer = ballToPlayer.normalized;
                Vector3 newPos = new Vector3 (
                    playerPos.x + ballToPlayer.x * _CAMERA_XZ_OFFSET,
                    //playerPos.y + _CAMERA_HEIGHT,
                    Mathf.Clamp(playerPos.y + ballToPlayer.y * _CAMERA_XZ_OFFSET, _MIN_CAMERA_HEIGHT, Mathf.Infinity),
                    playerPos.z + ballToPlayer.z * _CAMERA_XZ_OFFSET);
                transform.position = newPos;
                transform.LookAt (_ballPos, Vector3.up);
                break;
        }
    }

    void HandleInputs () {
        if (Input.GetButtonDown("ChangeCamera")) {
            _mode = (Mode)(1 - (int)_mode);
        }
    }
}
