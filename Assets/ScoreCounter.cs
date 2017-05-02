using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCounter : MonoBehaviour {

	[SerializeField] GameManager.Team _team;

    Text _text;

    private void Awake() {
        _text = GetComponentInChildren<Text>();
    }

    private void Start() {
        GameManager.Instance.onGameStarted.AddListener (Refresh);
        GameManager.Instance.onGoalScored.AddListener (Refresh);
        _text.text = 0.ToString();
    }

    void Refresh () {
        _text.text = GameManager.Instance.GetScore (_team).ToString();
    }
}
