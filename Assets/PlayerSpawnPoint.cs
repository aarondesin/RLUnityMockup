using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour {

	public static PlayerSpawnPoint Instance;

    private void Awake() {
        Instance = this;
    }
}
