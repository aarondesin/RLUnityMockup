using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {

    const float _ROUND_START_COUNTDOWN = 3f;
    const float _ROUND_START_DELAY = 1f;
    const float _POST_GOAL_DELAY = 3f;
    const float _MATCH_TIME = 300f;

    public enum Team {
        Orange = 0,
        Blue = 1
    }

	Dictionary<Team, int> _score = new Dictionary<Team, int>();

    List<PlayerController> _players = new List<PlayerController>();

    public static GameManager Instance;

    public UnityEvent onGoalScored = new UnityEvent();
    public UnityEvent onGameStarted = new UnityEvent();

    float _matchTimer;
    bool _timerRunning = false;

    private void Awake() {
        Instance= this;
        _score.Add (Team.Orange, 0);
        _score.Add (Team.Blue, 0);
    }

    private void Start() {
        var players = FindObjectsOfType<PlayerController>();
        foreach (var player in players) {
            _players.Add (player);
            int id = _players.Count;
            player.AssignPlayerID (id);
            player.Camera.AssignPlayerID (id);
        }

        foreach (var controller in Input.GetJoystickNames())
            Debug.Log (controller);

        //_players[0].Camera.AssignScreenRect (new Rect (0f, 0.5f, 1f, 0.5f));
        //_players[1].Camera.AssignScreenRect (new Rect (0f, 0f, 1f, 0.5f));

        Countdown.Instance.gameObject.SetActive(false);
        StartMatch();
    } 

    void Update () {
        //if (Input.GetButtonDown("Reset")) StartRound();
        if (_timerRunning) {
            _matchTimer -= Time.deltaTime;
            if (_matchTimer <= 0f) {
                EndMatch();
            }
        }
    }

    public List<PlayerController> Players { get { return _players; } }

    public bool TimerRunning { get { return _timerRunning; } }

    public float TimeLeft { get { return _matchTimer; } }

    public void RegisterGoal (Team team) {
        _score[team]++;
        onGoalScored.Invoke();

        StartCoroutine (DoEndRound(team));
    }

    void StartMatch () {
        _matchTimer = _MATCH_TIME;
        _score[Team.Blue] = 0;
        _score[Team.Orange] = 0;

        onGameStarted.Invoke();

        StartRound();
    }

    void StartRound() {
        StopAllCoroutines();
        StartCoroutine (DoStartRound());
    }

    IEnumerator DoStartRound () {
        ResetPlayers();
        ResetBall();

        RoundTimer.Instance.gameObject.SetActive (false);

        if (!Application.isEditor) {

            foreach (var player in _players)
                player.DisableMovement();

            yield return new WaitForSeconds (_ROUND_START_DELAY);

            Countdown.Instance.gameObject.SetActive (true);

            float countdown = _ROUND_START_COUNTDOWN;
            while (countdown > 0f) {
                countdown -= Time.deltaTime;
                Countdown.Instance.SetText (Mathf.CeilToInt(countdown).ToString());
                yield return null;
            }

            foreach (var player in _players)
                player.EnableMovement();

            Countdown.Instance.SetText ("GO!");
            _timerRunning = true;

            yield return new WaitForSeconds (_ROUND_START_DELAY);
        }

        RoundTimer.Instance.gameObject.SetActive (true);
        Countdown.Instance.gameObject.SetActive(false);

        _timerRunning = true;
        yield break;
    }


    IEnumerator DoEndRound (Team winner) {
        if (!Application.isEditor) {
            RoundTimer.Instance.gameObject.SetActive(false);

            Countdown.Instance.gameObject.SetActive(true);
            Countdown.Instance.SetText (winner.ToString() + " scored!");

            yield return new WaitForSeconds (_POST_GOAL_DELAY);

            Countdown.Instance.gameObject.SetActive(false);
        }

        _timerRunning = false;
        yield return DoStartRound();
    }

    void EndMatch () {
        _timerRunning = false;

        StartCoroutine(DoEndMatch());
    }

    IEnumerator DoEndMatch () {
        RoundTimer.Instance.gameObject.SetActive(false);

        var blueScore = _score[Team.Blue];
        var orangeScore = _score[Team.Orange];
        string countdownText;
        if (blueScore > orangeScore) countdownText = "Blue won!";
        else if (orangeScore > blueScore) countdownText = "Orange won!";
        else countdownText = "Draw!";

        Countdown.Instance.gameObject.SetActive(true);
        Countdown.Instance.SetText (countdownText);

        while (!Input.GetButton ("Jump1") && !Input.GetButton("Jump2")) yield return null;
        StartMatch();
    }

    void ResetPlayers () {
        foreach (var player in _players) {
            var spawnPoint = PlayerSpawnPoint.GetTeamSpawnPoint (player.Team);

            player.transform.position = spawnPoint.transform.position;
            player.transform.rotation = spawnPoint.transform.rotation;
            player.ResetPlayer();
        }
    }

    void ResetBall () {
        Ball.Instance.gameObject.SetActive(true);
        Ball.Instance.transform.position = BallSpawnPoint.Instance.transform.position;
        Ball.Instance.transform.rotation = Quaternion.identity;
        Ball.Instance.ResetBall();
    }

    public int GetScore (Team team) {
        return _score[team];
    }
}
