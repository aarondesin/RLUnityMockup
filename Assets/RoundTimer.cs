using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundTimer : MonoBehaviour {

    Text _text;

    public static RoundTimer Instance;

    private void Awake() {
        Instance = this;
        _text = GetComponentInChildren<Text>();
    }

    void Update () {
		if (GameManager.Instance.TimerRunning) {
            var t = GameManager.Instance.TimeLeft;
            var m = Mathf.FloorToInt (t / 60f);
            var s = (Mathf.Floor(t % 60f)).ToString("0#");
            _text.text = string.Format ("{0}:{1}", m, s);
        }
	}
}
