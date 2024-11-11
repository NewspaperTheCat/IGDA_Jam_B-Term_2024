using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class CrowdGameManager : MonoBehaviour
{
    public static CrowdGameManager inst;

    public Vector2 screenBoundaries = new Vector2(17f, 8.25f);
    [SerializeField] private GameObject pedestrianPrefab;
    [SerializeField] private int numPedestrians = 25;
    [SerializeField] private int numRounds = 3;
    [SerializeField] private List<CrowdPlayerPawn> players = new List<CrowdPlayerPawn>();
    private int[] playerScores;
    
    [SerializeField] private TextMeshProUGUI roundDisplay;
    [SerializeField] private TextMeshProUGUI phaseTimerText;
    private int round = 0;

    public UnityEvent sceneChanged;

    public enum gamePhase {
        Search,
        Select,
        Score
    }

    public gamePhase currentGamePhase = gamePhase.Search;
    private float phaseTimer;
    [SerializeField] private float searchDuration = 15, selectDuration = 10, scoreDuration = 5;
    private List<PedestrianBehavior> pedestrians = new List<PedestrianBehavior>();

    private void Awake() {
        inst = this;
    }

    void Start() {
        for (int i = 0; i < numPedestrians; i++) {
            Vector2 ranPos = new Vector2(
                Random.Range(-screenBoundaries.x, screenBoundaries.x),
                Random.Range(-screenBoundaries.y, screenBoundaries.y)
            );
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
                currentGamePhase = gamePhase.Search;
                phaseTimer = searchDuration;

                round++;
                if (round > numRounds) {
                    SubmitScores();
                } else {
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

    private void ScoreSelections() {
        List<CrowdPlayerPawn> correctPlayers = new List<CrowdPlayerPawn>();
        List<CrowdPlayerPawn> wrongPlayers = new List<CrowdPlayerPawn>();
        List<CrowdPlayerPawn> inactivePlayers = new List<CrowdPlayerPawn>();

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

    void SubmitScores() {
        MinigameManager.Ranking ranking = new();

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
            Debug.Log(scoreMaxIndex + " had a score of " + scoreMax);
            ranking.SetRank(scoreMaxIndex, j + 1);
            rearrangableScores[scoreMaxIndex] = -1;
        }

        for (int i = 0; i < 4; i++) {
            Debug.Log(i + " got a rank of " + ranking.playerRanks[i]);
        }

        MinigameManager.instance.EndMinigame(ranking);
    }
}
