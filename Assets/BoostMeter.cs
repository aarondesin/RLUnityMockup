using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoostMeter : MonoBehaviour {

	Text _text;
    Image _image;

    void Awake () {
        _text = GetComponentInChildren<Text>();
        _image = GetComponentInChildren<Image>();
    }

    private void Update() {
        _text.text = PlayerController.Instance.Boost.ToString("#0");
        _image.fillAmount = PlayerController.Instance.BoostPercentage;
    }
}
