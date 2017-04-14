using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    Mode _mode = Mode.Free;

	public enum Mode {
        Free,
        BallCam
    }

    private void Update() {
        switch (_mode) {
            case Mode.Free:
                var parent = transform.parent.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler (0f, parent.y, 0f);
                break;
        }
    }
}
