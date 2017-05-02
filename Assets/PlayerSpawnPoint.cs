using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour {

    static List<PlayerSpawnPoint> _spawnPoints = new List<PlayerSpawnPoint>();

	[SerializeField] GameManager.Team _team;

    private void Awake() {
        _spawnPoints.Add (this);
    }

    public static PlayerSpawnPoint GetTeamSpawnPoint (GameManager.Team team) {
        foreach (var spawnPoint in _spawnPoints)
            if (spawnPoint.Team == team) return spawnPoint;

        return null;
    }

    public GameManager.Team Team { get { return _team; } }
}
