using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraYBoundary : MonoBehaviour {

	public static CameraYBoundary Instance;

    private void Awake() {
        Instance = this;
    }
}
