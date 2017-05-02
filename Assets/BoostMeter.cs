using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoostMeter : MonoBehaviour {

	Text _text;
    Image _image;

    PlayerController _target;

    [SerializeField] int _player;

    void Awake () {
        _text = GetComponentInChildren<Text>();
        _image = GetComponentInChildren<Image>();
    }

    private void Update() {
        if (_target == null)
            _target = GameManager.Instance.Players[_player];

        _text.text = _target.Boost.ToString("#0");
        _image.fillAmount = _target.BoostPercentage;
    }
}
