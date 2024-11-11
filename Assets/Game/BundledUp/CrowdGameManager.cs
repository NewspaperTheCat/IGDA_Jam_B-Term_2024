using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class CrowdGameManager : MonoBehaviour
{
    public static CrowdGameManager inst;

    public Vector2 boundaries = new Vector2(17f, 8.25f);
    public Vector2 center = new Vector2(0, -7.5f/2f);
    [SerializeField] private GameObject pedestrianPrefab;
    [SerializeField] private int numPedestrians = 25;
    [SerializeField] private int numRounds = 3;
    [SerializeField] private List<CrowdPlayerPawn> players = new List<CrowdPlayerPawn>();
    private int[] playerScores;
    [SerializeField] private GameObject validationPrefab;
    
    [SerializeField] private TextMeshProUGUI roundDisplay;
    [SerializeField] private TextMeshProUGUI phaseTimerText;
    private int round = 0;

    public UnityEvent sceneChanged;

    public enum gamePhase {
        Search,
        Select,
        Score,
        Rank
    }

    public gamePhase currentGamePhase = gamePhase.Search;
    private float phaseTimer;
    [SerializeField] private float searchDuration = 15, selectDuration = 10, scoreDuration = 5, rankDuration = 5;
    private List<PedestrianBehavior> pedestrians = new List<PedestrianBehavior>();

    private void Awake() {
        inst = this;
    }

    void Start() {
        for (int i = 0; i < numPedestrians; i++) {
            Vector2 ranPos = new Vector2(
                Random.Range(-boundaries.x, boundaries.x),
                Random.Range(-boundaries.y, boundaries.y)
            ) + center;
            GameObject pedestrian = Instantiate(pedestrianPrefab, ranPos, Quaternion.identity);
            pedestrians.Add(pedestrian.GetComponent<PedestrianBehavior>());
        }

        playerScores = new int[players.Count];

        // peculiar settings so that it gets switched to Search after Start calls are done  
        currentGamePhase = gamePhase.Score;
        phaseTimer = 0;
    }

    private void Update() {
        if (phaseTimer > 0) {
            if (currentGamePhase == gamePhase.Score)
                phaseTimerText.text = "0";
            else
                phaseTimerText.text = "" + Mathf.Ceil(phaseTimer);
            phaseTimer -= Time.deltaTime;
        } else {
            phaseTimerText.text = "0";
            if (currentGamePhase == gamePhase.Search) {
                currentGamePhase = gamePhase.Select;
                phaseTimer = selectDuration;
            } else if (currentGamePhase == gamePhase.Select) {
                currentGamePhase = gamePhase.Score;
                phaseTimer = scoreDuration;

                ScoreSelections();
            } else if (currentGamePhase == gamePhase.Score) {
                round++;
                if (round > numRounds) {
                    currentGamePhase = gamePhase.Rank;
                    phaseTimer = rankDuration;
                    SubmitScores();
                } else {
                    currentGamePhase = gamePhase.Search;
                    phaseTimer = searchDuration;
                    roundDisplay.text = "Round " + round;
                }
            }
            sceneChanged.Invoke();
        }
    }

    public PedestrianBehavior ChooseRandomPlayer(int requestingPlayerIndex) {
        PedestrianBehavior foundPedestrian = null;
        while (foundPedestrian == null || foundPedestrian.IsPlayer()) {
            foundPedestrian = pedestrians[Random.Range(0, pedestrians.Count)];
        }
        foundPedestrian.MakePlayer(requestingPlayerIndex);
        if (foundPedestrian == null) Debug.Log("ERROR: no non-player found");
        return foundPedestrian;
    }

    List<CrowdPlayerPawn> correctPlayers = new List<CrowdPlayerPawn>();
    List<CrowdPlayerPawn> wrongPlayers = new List<CrowdPlayerPawn>();
    List<CrowdPlayerPawn> inactivePlayers = new List<CrowdPlayerPawn>();
    private void ScoreSelections() {
        correctPlayers = new List<CrowdPlayerPawn>();
        wrongPlayers = new List<CrowdPlayerPawn>();
        inactivePlayers = new List<CrowdPlayerPawn>();

        foreach (CrowdPlayerPawn player in players) {
            if (player.HighlightedAny()) {
                if (player.HighlightedCorrectPedestrian()) {
                    correctPlayers.Add(player);
                } else {
                    wrongPlayers.Add(player);
                }
            } else {
                inactivePlayers.Add(player);
            }
        }

        // Debug.Log(correctPlayers.Count + " > " + wrongPlayers.Count + " > " + inactivePlayers.Count);

        correctPlayers.Sort();
        wrongPlayers.Sort();
        Shuffle(inactivePlayers);

        List<CrowdPlayerPawn> orderedPlayers = new List<CrowdPlayerPawn>();
        orderedPlayers.AddRange(correctPlayers);
        orderedPlayers.AddRange(wrongPlayers);
        orderedPlayers.AddRange(inactivePlayers);

        // string output = "Round Rankings:";
        // foreach (CrowdPlayerPawn player in orderedPlayers) {
        //     output += "\t" + player.playerPawnIndex;
        // }
        // Debug.Log(output);

        for (int i = 0; i < playerScores.Length; i++) {
            playerScores[orderedPlayers[i].playerPawnIndex] += playerScores.Length - i;
        }

        StartCoroutine("DisplayAccuracy");
    }

    IEnumerator DisplayAccuracy() {
        foreach (CrowdPlayerPawn player in correctPlayers) {
            ValidationBehavior vb = Instantiate(
                validationPrefab,
                player.GetHighlightedPosition(),
                Quaternion.identity
            ).GetComponent<ValidationBehavior>();
            vb.SetVisuals(true);

            yield return new WaitForSeconds(scoreDuration / players.Count);
        }

        List<CrowdPlayerPawn> incorrectPlayers = wrongPlayers;
        incorrectPlayers.AddRange(inactivePlayers);
        foreach (CrowdPlayerPawn player in wrongPlayers) {
            ValidationBehavior vb = Instantiate(
                validationPrefab,
                player.GetHighlightedPosition(),
                Quaternion.identity
            ).GetComponent<ValidationBehavior>();
            vb.SetVisuals(false);

            yield return new WaitForSeconds(scoreDuration / players.Count);
        }
    }

    void Shuffle(List<CrowdPlayerPawn> ts) {
		var count = ts.Count;
		var last = count - 1;
		for (var i = 0; i < last; ++i) {
			var r = UnityEngine.Random.Range(i, count);
			var tmp = ts[i];
			ts[i] = ts[r];
			ts[r] = tmp;
		}
	}

    MinigameManager.Ranking ranking = new();

    void SubmitScores() {
        List<int> rearrangableScores = new List<int>();
        foreach (int score in playerScores) {
            rearrangableScores.Add(score);
        }

        for (int j = 0; j < playerScores.Length; j++) {
            int scoreMax = -1;
            int scoreMaxIndex = -1;
            for (int i = 0; i < playerScores.Length; i++) {
                int score = rearrangableScores[i];
                if (score > scoreMax) {
                    scoreMax = score;
                    scoreMaxIndex = i;
                }
            }
            ranking.SetRank(scoreMaxIndex, j + 1);
            rearrangableScores[scoreMaxIndex] = -1;
        }

        StartCoroutine("DisplayRankings");
    }

    IEnumerator DisplayRankings() {
        roundDisplay.text = "Finished";

        foreach (CrowdPlayerPawn player in players) {
            ValidationBehavior vb = Instantiate(
                validationPrefab,
                player.GetPointerPosition(),
                Quaternion.identity
            ).GetComponent<ValidationBehavior>();
            vb.UseRank(ranking.playerRanks[player.playerPawnIndex]);

            yield return new WaitForSeconds(rankDuration / players.Count);
        }

        MinigameManager.instance.EndMinigame(ranking);
    }

    public float GetRemainingPhaseTime() {
        return phaseTimer;
    }
}
