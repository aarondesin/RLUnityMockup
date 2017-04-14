using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public enum Team {
        Orange,
        Blue
    }

	Dictionary<Team, int> _score = new Dictionary<Team, int>();

    public static GameManager Instance;

    private void Awake() {
        Instance= this;
        _score.Add (Team.Orange, 0);
        _score.Add (Team.Blue, 0);
    }

    public void RegisterGoal (Team team) {
        _score[team]++;
    }
}
