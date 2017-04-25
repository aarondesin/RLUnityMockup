using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    const float _ROUND_START_COUNTDOWN = 3f;
    const float _ROUND_START_DELAY = 1f;

    public enum Team {
        Orange,
        Blue
    }

	Dictionary<Team, int> _score = new Dictionary<Team, int>();

    public static GameManager Instance;

    private void Awake() {
        Instance= this;
        _score.Add (Team.Orange, 0);
        _score.Add (Team.Blue, 0);
    }

    private void Start() {
        Countdown.Instance.gameObject.SetActive(false);
        StartRound();
    } 

    public void RegisterGoal (Team team) {
        _score[team]++;
    }

    void StartRound() {
        StartCoroutine (DoStartRound());
    }

    IEnumerator DoStartRound () {
        PlayerController.Instance.DisableMovement();

        float delay = _ROUND_START_DELAY;
        while (delay > 0f) {
            delay -= Time.deltaTime;
            yield return null;
        }

        Countdown.Instance.gameObject.SetActive (true);

        float countdown = _ROUND_START_COUNTDOWN;
        while (countdown > 0f) {
            countdown -= Time.deltaTime;
            Countdown.Instance.SetText (Mathf.CeilToInt(countdown).ToString());
            yield return null;
        }

        PlayerController.Instance.EnableMovement();

        Countdown.Instance.SetText ("GO!");
        float postDelay = _ROUND_START_DELAY;
        while (postDelay > 0f) {
            postDelay -= Time.deltaTime;
            yield return null;
        }

        Countdown.Instance.gameObject.SetActive(false);
        yield break;
    }
}
