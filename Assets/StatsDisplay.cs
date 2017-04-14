using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsDisplay : MonoBehaviour {

    [SerializeField] DisplayType _displayType;

    Text _text;

	public enum DisplayType {
        Horizontal,
        Vertical,
        Gas,
        Brake
    }

    void Awake () {
        _text = GetComponent<Text>();
    }

    void Update () {
        switch (_displayType) {
            case DisplayType.Horizontal:
                _text.text = "Horizontal: " + PlayerController.Instance.Horizontal.ToString();
                break;
            case DisplayType.Vertical:
                _text.text = "Vertical: " + PlayerController.Instance.Vertical.ToString();
                break;
            case DisplayType.Gas:
                _text.text = "Gas: " + PlayerController.Instance.Gas.ToString();
                break;
            case DisplayType.Brake:
                _text.text = "Brake: " + PlayerController.Instance.Brake.ToString();
                break;
        }
    }
}
