using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {

    const float _EXPLOSION_FORCE = 100000f;
    const float _EXPLOSION_RADIUS = 100f;

    public static Ball Instance;

    MeshRenderer _renderer;

    private void Awake() {
        Instance = this;
        _renderer = GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Goal") {
            var otherTeam = (GameManager.Team)(1 - (int)other.GetComponent<Goal>().Team);
            GameManager.Instance.RegisterGoal (otherTeam);
            Explode();
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere (transform.position, _EXPLOSION_RADIUS);
    }

    void Explode () {
        PlayerController.Instance.GetComponent<Rigidbody>().AddExplosionForce (_EXPLOSION_FORCE, transform.position, _EXPLOSION_RADIUS);
        //GetComponent<MeshRenderer>().enabled = false;
        gameObject.SetActive(false);
    }

    public void ResetBall() {
        //_renderer.enabled = true;
        //gameObject.SetActive(true);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        
        //Debug.Log ("reset");
    } 
}
