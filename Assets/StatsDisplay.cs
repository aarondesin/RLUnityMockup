using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsDisplay : MonoBehaviour {

    [SerializeField] DisplayType _displayType;

    Text _text;

    PlayerController _target;

	public enum DisplayType {
        Horizontal,
        Vertical,
        Gas,
        Brake,
        Grounded,
        Flipped
    }

    void Awake () {
        _text = GetComponent<Text>();
    }

    void Update () {
        switch (_displayType) {
            case DisplayType.Horizontal:
                _text.text = "Horizontal: " + _target.Horizontal.ToString();
                break;
            case DisplayType.Vertical:
                _text.text = "Vertical: " + _target.Vertical.ToString();
                break;
            case DisplayType.Gas:
                _text.text = "Gas: " + _target.Gas.ToString();
                break;
            case DisplayType.Brake:
                _text.text = "Brake: " + _target.Brake.ToString();
                break;
            case DisplayType.Grounded:
                _text.text = "Grounded: " + _target.Grounded.ToString();
                break;
            case DisplayType.Flipped:
                _text.text = "Flipped: " + _target.Flipped.ToString();
                break;
        }
    }
}
