using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {

    

	[SerializeField] GameManager.Team _team;

    public GameManager.Team Team { get { return _team; } }
}
