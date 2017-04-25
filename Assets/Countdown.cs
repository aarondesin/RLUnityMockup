using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Countdown : MonoBehaviour {

	public static Countdown Instance;

    Text _text;

    private void Awake() {
        Instance = this;
        _text = GetComponent<Text>();
    }

    public void SetText (string text) {
        _text.text = text;
    }
}
