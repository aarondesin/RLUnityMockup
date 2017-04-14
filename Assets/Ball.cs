using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Goal") {
            var otherTeam = (GameManager.Team)(1 - (int)other.GetComponent<Goal>().Team);
            GameManager.Instance.RegisterGoal (otherTeam);
            Explode();
        }
    }

    void Explode () {
        Destroy(gameObject);
    }
}
