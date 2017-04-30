using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    const float _ROUND_START_COUNTDOWN = 3f;
    const float _ROUND_START_DELAY = 1f;
    const float _POST_GOAL_DELAY = 3f;

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

        StartCoroutine (DoEndRound());
    }

    void StartRound() {
        StartCoroutine (DoStartRound());
    }

    IEnumerator DoStartRound () {
        ResetPlayer();
        ResetBall();

        if (!Application.isEditor) {

            PlayerController.Instance.DisableMovement();

            yield return new WaitForSeconds (_ROUND_START_DELAY);

            Countdown.Instance.gameObject.SetActive (true);

            float countdown = _ROUND_START_COUNTDOWN;
            while (countdown > 0f) {
                countdown -= Time.deltaTime;
                Countdown.Instance.SetText (Mathf.CeilToInt(countdown).ToString());
                yield return null;
            }

            PlayerController.Instance.EnableMovement();

            Countdown.Instance.SetText ("GO!");

            yield return new WaitForSeconds (_ROUND_START_DELAY);

            Countdown.Instance.gameObject.SetActive(false);
        }
        yield break;
    }

    IEnumerator DoEndRound () {
        if (!Application.isEditor) {
            Countdown.Instance.gameObject.SetActive(true);
            Countdown.Instance.SetText ("You scored!");

            yield return new WaitForSeconds (_POST_GOAL_DELAY);

            Countdown.Instance.gameObject.SetActive(false);
        }
        yield return DoStartRound();
    }

    void ResetPlayer () {
        PlayerController.Instance.transform.position = PlayerSpawnPoint.Instance.transform.position;
        PlayerController.Instance.transform.rotation = PlayerSpawnPoint.Instance.transform.rotation;
        PlayerController.Instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
        PlayerController.Instance.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        PlayerController.Instance.ResetPlayer();
    }

    void ResetBall () {
        Ball.Instance.gameObject.SetActive(true);
        Ball.Instance.transform.position = BallSpawnPoint.Instance.transform.position;
        Ball.Instance.transform.rotation = Quaternion.identity;
        Ball.Instance.ResetBall();
    }
}
