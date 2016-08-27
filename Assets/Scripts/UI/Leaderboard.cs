using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Util;
using System.Linq;
using Parse;
using Facebook.MiniJSON;

public class Leaderboard : MonoBehaviour {
    public LeaderboardEntry leaderboardEntryPrefab;
    public List<PlayerNameplate> playerNameplates;
    public dfLabel playerRank;
    public List<dfPanel> friendsLeaderboardPanels;
    public List<dfPanel> globalLeaderboardPanels;
    public List<dfSpriteAnimation> waitAnimations;
    public dfLabel noScoresPrefab;

    bool loading;
    private List<DTOSelfScoreRank> selfScoresRanks;
    //private List<DTOUserScore> globalScores;
    private string lastResponse;
    private string currentMode;

    void Awake() {
        selfScoresRanks = new List<DTOSelfScoreRank>();
        currentMode = "ClassicMode";
        GlobalData.Instance.OnFriendsScoresSet += OnFriendsScoresSetHandler;
    }

	// Use this for initialization
	void Start () {
        loading = false;
        foreach (var playerNameplate in playerNameplates) {
            playerNameplate.name.Text = String.Empty;
            playerNameplate.score.Text = String.Empty;
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (!loading) {
            if (FB.IsLoggedIn) {
                GetSelfInfo();
                loading = true;
            }
        }
	}

    void OnDestroy() {
        GlobalData.Instance.OnFriendsScoresSet -= OnFriendsScoresSetHandler;
    }

    void OnFriendsScoresSetHandler() {
        HashSet<string> friendsIDs = new HashSet<string>();
        foreach (var friend in GlobalData.Instance.FriendsInfo) {
            friendsIDs.Add(friend.Key);
        }

        DisplayScores(GlobalData.Instance.FriendsScores, friendsIDs, "ClassicMode");
        DisplayScores(GlobalData.Instance.FriendsScores, friendsIDs, "GrowthMode");
        DisplayScores(GlobalData.Instance.FriendsScores, friendsIDs, "PeriodicMode");
    }

    void GetSelfInfo() {
        var apiQuery = "/fql?q=" + WWW.EscapeURL(@"
            SELECT first_name, last_name, name_format FROM user 
            WHERE uid = me()");
        FB.API(apiQuery, Facebook.HttpMethod.GET, SelfCallback);
    }

    void SelfCallback(FBResult result) {
        if (!String.IsNullOrEmpty(result.Error)) {
            lastResponse = "Error Response 02:\r\n" + result.Error;
            Invoke("GetSelfInfo", 5f);
        } else {
            var dict = Json.Deserialize(result.Text) as Dictionary<string, object>;

            var deserializedList = new List<object>();
            deserializedList = (List<object>)(((Dictionary<string, object>)dict)["data"]);
            string name = string.Empty;
            if (deserializedList.Any()) {
                var firstName = (string)((Dictionary<string, object>)deserializedList[0])["first_name"];
                var lastName = (string)((Dictionary<string, object>)deserializedList[0])["last_name"];
                var nameFormat = (string)((Dictionary<string, object>)deserializedList[0])["name_format"];
                //var pic = (string)((Dictionary<string, object>)deserializedList[0])["pic_square"];
                name = nameFormat.Replace("{first}", firstName).Replace("{last}", lastName);
            }

            GlobalData.Instance.playerName = name;
            GlobalData.Instance.playerFacebookID = FB.UserId;

            foreach (var playerNameplate in playerNameplates) {
                playerNameplate.name.Text = name;
            }

            StartCoroutine(GetSelfScores());
        }

        if (lastResponse != null) {
            Debug.Log(lastResponse);
        }
    }

    IEnumerator GetSelfScores() {
        if (!FB.IsLoggedIn) {
            yield break;
        }

        while (GlobalData.Instance.FriendsInfo == null) yield return null;

        HashSet<string> friendsIDs = new HashSet<string>();
        foreach (var friend in GlobalData.Instance.FriendsInfo) {
            friendsIDs.Add(friend.Key);
        }

        var gameModes = new List<string> { "ClassicMode", "GrowthMode", "PeriodicMode" };
        var parameters = new Dictionary<string, object> {
            {"userID", FB.UserId},
            {"gameVersion", GlobalData.Instance.version}, 
            {"friendsIDs", friendsIDs.ToList()},
            {"gameModes", gameModes}
        };
        var playerScoresAndRanksTask = ParseCloud.CallFunctionAsync<Dictionary<string, object>>("PlayerScoresAndRanks", parameters);

        while (!playerScoresAndRanksTask.IsCompleted) yield return null;

        if (playerScoresAndRanksTask.IsFaulted) {
            foreach (var e in playerScoresAndRanksTask.Exception.InnerExceptions) {
                ParseException parseException = (ParseException)e;
                Debug.Log("Error getting your score.\r\nError message: " + parseException.Message + "\r\nErrorCode: " + parseException.Code);
            }
        } else {

            Dictionary<string, object> playerScoresAndRanks = playerScoresAndRanksTask.Result;

            foreach (var gameMode in gameModes) {
                selfScoresRanks.Add(new DTOSelfScoreRank { GameMode = gameMode, Rank = Convert.ToInt32(playerScoresAndRanks[gameMode + "Rank"]), Score = (long)playerScoresAndRanks[gameMode + "Score"] });
                playerNameplates[GetModeIndex(gameMode)].score.Text = playerScoresAndRanks[gameMode + "Score"].ToString();
                GlobalData.Instance.highScores[gameMode] = (long)playerScoresAndRanks[gameMode + "Score"];
            }
            try {
                playerRank.Text = playerScoresAndRanks[currentMode + "Rank"].ToString();
            } catch {
                Debug.Log("Tried to access " + currentMode + "Rank");
            }
        }
    }

    private void DisplayScores(IEnumerable<DTOUserScore> parseScores, HashSet<string> friendsIDs, string gameMode) {
        foreach (var waitAnimation in waitAnimations) {
            waitAnimation.Stop();
            waitAnimation.gameObject.SetActive(false);
        }

        var currentModeScores = new List<DTOUserScore>();
        int page = GetModeIndex(gameMode);

        var specificModeScores = parseScores.Where(s => s.GameMode == gameMode).ToList();
        foreach (var score in specificModeScores) {
            var userID = score.UserID;
            var currentFriendInfo = GlobalData.Instance.FriendsInfo[userID];
            currentFriendInfo.Score = score.Score;
            currentModeScores.Add(currentFriendInfo);
        }

        currentModeScores = currentModeScores.OrderByDescending(s => s.Score).ToList();
        var totalEntries = UnityEngine.Mathf.Min(currentModeScores.Count, 10);

        //int rank = 1;
        //if (currentModeScores.Any(x => x.UserID == FB.UserId)) {
        //    rank = currentModeScores.FindIndex(x => x.UserID == FB.UserId);
        //} else {
        //    //FindPlayerRank(gameMode, , friendsIDs);
        //}

        //playerRank.Text = rank.ToString();

        if (totalEntries == 0) {
            var noScores = friendsLeaderboardPanels[page].AddPrefab(noScoresPrefab.gameObject) as dfLabel;
            noScores.BackgroundColor = GlobalData.Instance.SolidColors[gameMode];
            noScores.Anchor = dfAnchorStyle.CenterHorizontal | dfAnchorStyle.CenterVertical;
            noScores.IsVisible = true;
        }

        //Insert values into page
        for (int i = 0; i < totalEntries; i++) {
            var entryGameObject = friendsLeaderboardPanels[page].AddPrefab(leaderboardEntryPrefab.gameObject);
            var entry = entryGameObject.GetComponent<LeaderboardEntry>();
            var entryPanel = entry.GetComponent<dfPanel>();

            //entryPanel.Anchor = dfAnchorStyle.Top | dfAnchorStyle.Left;
            entryPanel.RelativePosition = new Vector3(10, i * (entryPanel.Size.y + 10) + 12, 0);
            entryPanel.IsEnabled = true;
            entryPanel.IsVisible = true;
            entry.name.Text = currentModeScores[i].Name;
            entry.score.Text = currentModeScores[i].Score.ToString();
            entry.position.Text = (i + 1).ToString();
            entry.photo.URL = currentModeScores[i].Pic;
            entry.photo.LoadImage();
        }
    }

    public void OnModeSelected(string modeName) {
        var modeIndex = GetModeIndex(modeName);

        if (modeIndex != -1) {
            friendsLeaderboardPanels[modeIndex].ZOrder = GlobalData.TOTAL_GAME_MODES - 1;
            playerNameplates[modeIndex].GetComponent<dfPanel>().ZOrder = GlobalData.TOTAL_GAME_MODES - 1;
            if (selfScoresRanks != null && selfScoresRanks.Any()) {
                playerRank.Text = selfScoresRanks[modeIndex].Rank.ToString();
            }
        }

        currentMode = modeName;
    }

    private int GetModeIndex(string gameModeName) {
        switch (gameModeName) {
            case "TutorialMode":
            case "ClassicMode":
                return 0;
            case "GrowthMode":
                return 1;
            //case "MiniMode":
            case "PeriodicMode":
                return 2;
            default:
                return -1;
        }
    }

    public class DTOUserScore {
        public string UserID { get; set; }
        public string Name { get; set; }
        public string Pic { get; set; }
        public string GameMode { get; set; }
        public long Score { get; set; }
    }

    public class DTOSelfScoreRank {
        public string GameMode { get; set; }
        public long Score { get; set; }
        public int Rank { get; set; }
    }
}
