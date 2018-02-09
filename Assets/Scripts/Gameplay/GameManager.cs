// GameManager.cs
// ©2017 Aaron Desin

using RL.UI;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace RL.Gameplay
{
    /// <summary>
    /// Singleton behavior to manager game time and score.
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Countdown time before each round (seconds).
        /// </summary>
        const float _ROUND_START_COUNTDOWN = 3f;

        /// <summary>
        /// Delay before each round countdown (seconds).
        /// </summary>
        const float _ROUND_START_DELAY = 1f;

        /// <summary>
        /// Time to wait after each goal before starting a new round (seconds).
        /// </summary>
        const float _POST_GOAL_DELAY = 3f;

        /// <summary>
        /// Total length of match regulation (seconds).
        /// </summary>
        const float _MATCH_TIME = 300f;

        #endregion
        #region Vars

        /// <summary>
        /// Active GameManager instance.
        /// </summary>
        public static GameManager Instance;

        /// <summary>
        /// The sound to play with each countdown tick.
        /// </summary>
        [Tooltip("The sound to play with each countdown tick.")]
        [SerializeField] AudioClip _roundCountdownSound;

        /// <summary>
        /// The sound to play when a round starts.
        /// </summary>
        [Tooltip("The sound to play when a round starts.")]
        [SerializeField] AudioClip _roundStartSound;

        /// <summary>
        /// Scores for each team.
        /// </summary>
        Dictionary<Team, int> _score = new Dictionary<Team, int>();

        /// <summary>
        /// All players currently in the game.
        /// </summary>
        List<PlayerController> _players = new List<PlayerController>();

        /// <summary>
        /// Current match time.
        /// </summary>
        float _matchTimer;

        /// <summary>
        /// Is the match timer currently running?
        /// </summary>
        bool _timerRunning = false;

        /// <summary>
        /// The AudioSource attached to this object.
        /// </summary>
        AudioSource _audioSource;

        /// <summary>
        /// Invoked when a new game is started.
        /// </summary>
        public UnityEvent onGameStarted = new UnityEvent();
        
        /// <summary>
        /// Invoked when a goal is scored.
        /// </summary>
        public UnityEvent onGoalScored = new UnityEvent();

        #endregion
        #region Unity Callbacks

        private void Awake()
        {
            // Init singleton reference
            Instance = this;

            // Init component references
            _audioSource = GetComponent<AudioSource>();

            // Init score dict
            _score.Add(Team.Orange, 0);
            _score.Add(Team.Blue, 0);
        }

        private void Start()
        {
            // Add all players to game
            var players = FindObjectsOfType<PlayerController>();
            foreach (var player in players)
            {
                _players.Add(player);
                int id = _players.Count;
                player.AssignPlayerID(id);
                player.Camera.AssignPlayerID(id);
            }

            //foreach (var controller in Input.GetJoystickNames())
            //    Debug.Log(controller);

            // Hide countdown text initially
            CountdownText.Instance.gameObject.SetActive(false);

            StartMatch();
        }

        void Update()
        {
            // Check match time
            if (_timerRunning)
            {
                _matchTimer -= Time.deltaTime;
                if (_matchTimer <= 0f)
                {
                    EndMatch();
                }
            }
        }

        #endregion
        #region Properties

        /// <summary>
        /// Returns a list of all players in the current game (read-only).
        /// </summary>
        public List<PlayerController> Players { get { return _players; } }

        /// <summary>
        /// Returns true if the match timer is running (read-only).
        /// </summary>
        public bool TimerRunning { get { return _timerRunning; } }

        /// <summary>
        /// Returns the amount of time left in the match (seconds) (read-only).
        /// </summary>
        public float TimeLeft { get { return _matchTimer; } }

        #endregion
        #region Methods

        /// <summary>
        /// Starts a new match.
        /// </summary>
        void StartMatch()
        {
            _matchTimer = _MATCH_TIME;
            _score[Team.Blue] = 0;
            _score[Team.Orange] = 0;

            onGameStarted.Invoke();

            StartRound();
        }

        /// <summary>
        /// Starts a new round.
        /// </summary>
        void StartRound()
        {
            StopAllCoroutines();
            StartCoroutine(DoStartRound());
        }

        /// <summary>
        /// Round start coroutine.
        /// </summary>
        IEnumerator DoStartRound()
        {
            // Reset players and the ball
            ResetPlayers();
            ResetBall();

            RoundTimer.Instance.gameObject.SetActive(false);

            if (!Application.isEditor)
            {
                // Disable player movement
                foreach (var player in _players)
                    player.DisableMovement();

                yield return new WaitForSeconds(_ROUND_START_DELAY);

                CountdownText.Instance.gameObject.SetActive(true);

                // Do round countdown
                float countdown = _ROUND_START_COUNTDOWN;
                int rounded = 4;
                while (countdown > 0f)
                {
                    countdown -= Time.deltaTime;
                    var newRounded = Mathf.CeilToInt(countdown);
                    if (newRounded != rounded)
                        _audioSource.PlayOneShot(_roundCountdownSound, 0.5f);
                    rounded = newRounded;
                    CountdownText.Instance.SetText(rounded.ToString());
                    yield return null;
                }

                // Enable player movement
                foreach (var player in _players)
                    player.EnableMovement();

                // Show round start
                CountdownText.Instance.SetText("GO!");
                _audioSource.PlayOneShot(_roundStartSound, 0.5f);
                _timerRunning = true;

                yield return new WaitForSeconds(_ROUND_START_DELAY);
            }

            RoundTimer.Instance.gameObject.SetActive(true);
            CountdownText.Instance.gameObject.SetActive(false);

            _timerRunning = true;
            yield break;
        }

        /// <summary>
        /// Registers a goal for the given team.
        /// </summary>
        public void RegisterGoal(Team team)
        {
            _score[team]++;
            onGoalScored.Invoke();

            StartCoroutine(DoEndRound(team));
        }

        /// <summary>
        /// Round end coroutine.
        /// </summary>
        IEnumerator DoEndRound(Team winner)
        {
            if (!Application.isEditor)
            {
                RoundTimer.Instance.gameObject.SetActive(false);

                // Show score text
                CountdownText.Instance.gameObject.SetActive(true);
                CountdownText.Instance.SetText(winner.ToString() + " scored!");

                yield return new WaitForSeconds(_POST_GOAL_DELAY);

                CountdownText.Instance.gameObject.SetActive(false);
            }

            _timerRunning = false;
            yield return DoStartRound();
        }

        /// <summary>
        /// Ends the current match.
        /// </summary>
        void EndMatch()
        {
            _timerRunning = false;

            StartCoroutine(DoEndMatch());
        }

        /// <summary>
        /// Match end coroutine.
        /// </summary>
        IEnumerator DoEndMatch()
        {
            RoundTimer.Instance.gameObject.SetActive(false);

            // Show match winner text
            var blueScore = _score[Team.Blue];
            var orangeScore = _score[Team.Orange];
            string countdownText;
            if (blueScore > orangeScore) countdownText = "Blue won!";
            else if (orangeScore > blueScore) countdownText = "Orange won!";
            else countdownText = "Draw!";

            CountdownText.Instance.gameObject.SetActive(true);
            CountdownText.Instance.SetText(countdownText);

            // Wait for button press from either player before starting a 
            // new match
            while (!Input.GetButton("Jump1") && !Input.GetButton("Jump2"))
                yield return null;

            StartMatch();
        }

        /// <summary>
        /// Resets all players to their spawn points.
        /// </summary>
        void ResetPlayers()
        {
            foreach (var player in _players)
            {
                var spawnPoint = PlayerSpawnPoint.GetTeamSpawnPoint(player.Team);

                player.transform.position = spawnPoint.transform.position;
                player.transform.rotation = spawnPoint.transform.rotation;
                player.ResetPlayer();
            }
        }

        /// <summary>
        /// Resets the ball to its spawn point.
        /// </summary>
        void ResetBall()
        {
            Ball.Instance.gameObject.SetActive(true);
            Ball.Instance.transform.position = BallSpawnPoint.Instance.transform.position;
            Ball.Instance.transform.rotation = Quaternion.identity;
            Ball.Instance.ResetBall();
        }

        /// <summary>
        /// Returns the current score for the given team.
        /// </summary>
        /// <returns></returns>
        public int GetScore(Team team) { return _score[team]; }

        #endregion
        #region Enums

        public enum Team
        {
            Orange = 0,
            Blue = 1
        }

        #endregion
    }
}