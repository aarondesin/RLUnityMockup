using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawnPoint : MonoBehaviour {

	public static BallSpawnPoint Instance;

    private void Awake() {
        Instance = this;
    }
}
