using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPickup : MonoBehaviour {

    Collider _collider;

    [SerializeField] float _boostValue;
    [SerializeField] float _respawnTime;

    bool _enabled = true;

    [SerializeField] GameObject _boostEffect;

	void Awake () {
        _collider = GetComponentInChildren<Collider>();
    }

    IEnumerator WaitForReenable () {
        yield return new WaitForSeconds (_respawnTime);

        Enable();
        yield break;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player" && _enabled) {
            var player = other.GetComponent<PlayerController>();
            player.GiveBoost (_boostValue);
            Disable();
            StartCoroutine(WaitForReenable());
        }
    }

    void Enable() {
        _enabled = true;
        _boostEffect.SetActive(true);
    }

    void Disable () {
        _enabled = false;
        _boostEffect.SetActive(false);
    }
}
