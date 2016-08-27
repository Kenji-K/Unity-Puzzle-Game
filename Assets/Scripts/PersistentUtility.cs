using UnityEngine;
using System.Collections;
using Parse;
using Assets.Scripts.GameModes;
using Assets.Scripts.Util;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.IO;
using OpenSslCompat;
using System.Text;
using Facebook.MiniJSON;

public class PersistentUtility : MonoBehaviour {
    private static PersistentUtility instance;
    public static PersistentUtility Instance {
        get {
            return instance; 
        }
    }

    private float musicVolume;
    public float MusicVolume {
        get { return musicVolume; }
        set { 
            musicVolume = value;
            if (MasterAudio.Instance != null) {
                MasterAudio.PlaylistMasterVolume = value;
                PlayerPrefs.SetFloat("BGMVolume", value);
            }
        }
    }

    private float effectsVolume;
    public float EffectsVolume {
        get { return effectsVolume; }
        set {
            effectsVolume = value;
            if (MasterAudio.Instance != null) {
                MasterAudio.GrabBusByName("Sound Effects").volume = value;
                PlayerPrefs.SetFloat("SFXVolume", value);
            }
        }
    }

    private bool isFetchingInfo;

    void Awake() {
        DontDestroyOnLoad(this);
        instance = this;
    }

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (FB.IsLoggedIn && !isFetchingInfo) {
            GetFriendsInfo();
            isFetchingInfo = true;
        }
	}

    void OnLevelWasLoaded(int level) {
        if (!PlayerPrefs.HasKey("BGMVolume") || !PlayerPrefs.HasKey("SFXVolume")) {
            MusicVolume = 0.5f;
            EffectsVolume = 0.5f;
            PlayerPrefs.Save();
        } else {
            MusicVolume = PlayerPrefs.GetFloat("BGMVolume");
            EffectsVolume = PlayerPrefs.GetFloat("SFXVolume");
        }
        var netRandom = new System.Random();
        UnityEngine.Random.seed = netRandom.Next();

        //Debug.Log("Loaded Scene: " + Application.loadedLevelName);
        if (Application.loadedLevelName == "MainMenuScene" || Application.loadedLevelName == "ScoreScene") {
            isFetchingInfo = false;
            StartCoroutine(GetGlobalTopScores());
        }
    }

    IEnumerator GetGlobalTopScores() {
        var parameters = new Dictionary<string, object> {
            {"gameVersion", GlobalData.Instance.version},
            {"gameModes", new List<string> {"ClassicMode", "GrowthMode", "PeriodicMode"}}
        };

        var globalTopScoresTask = ParseCloud.CallFunctionAsync<IEnumerable>("GlobalTopScores", parameters);

        while (!globalTopScoresTask.IsCompleted) yield return null;

        if (globalTopScoresTask.IsFaulted) {
            foreach (var e in globalTopScoresTask.Exception.InnerExceptions) {
                ParseException parseException = (ParseException)e;
                Debug.Log("Error getting global scores.\r\nError message: " + parseException.Message + "\r\nErrorCode: " + parseException.Code);
            }
        } else {
            IEnumerable<ParseObject> globalTopScores = globalTopScoresTask.Result.OfType<ParseObject>();
            GlobalData.Instance.GlobalTopScores = globalTopScores.Select(s => new Leaderboard.DTOUserScore { Score = s.Get<int>("score"), GameMode = s.Get<string>("gameMode") }).ToList();
        }
    }

    internal TaskResult SaveHighScore(GameMode currentGameMode) {
        var taskResult = new TaskResult();
        StartCoroutine(SaveHighScore(currentGameMode, taskResult));
        return taskResult;
    }

    internal IEnumerator SaveHighScore(GameMode currentGameMode, TaskResult result) {
        if (!FB.IsLoggedIn) yield break;

        var query = ParseObject.GetQuery("HighScore")
                        .WhereEqualTo("facebookUserID", FB.UserId)
                        .WhereEqualTo("gameMode", currentGameMode.ModeName);

        var task = query.FirstOrDefaultAsync();

        while (!task.IsCompleted) yield return null;
        
        if (task.IsFaulted) {
            foreach (var e in task.Exception.InnerExceptions) {
                ParseException parseException = (ParseException)e;
                Debug.Log("Error getting high score.\r\nError message: " + parseException.Message + "\r\nErrorCode: " + parseException.Code);
            }

            result.IsCompleted = true;
            result.IsFaulted = true;

            yield break;
        }

        if (task.Result == null) {
            //Create and send new score
            var highScore = new ParseObject("HighScore");
            highScore["gameVersion"] = GlobalData.Instance.version;
            highScore["score"] = currentGameMode.Score;
            highScore["facebookUserID"] = FB.UserId;
            highScore["gameMode"] = currentGameMode.ModeName;
            if (currentGameMode.ModeName == "PeriodicMode") {
                highScore["seedUsed"] = GlobalData.Instance.periodicModeSeed;
                highScore["rawSeedUsed"] = GlobalData.Instance.rawSeed;
            }
            var saveTask = highScore.SaveAsync();

            while (!saveTask.IsCompleted) yield return null;

            if (saveTask.IsFaulted) {
                foreach (var e in task.Exception.InnerExceptions) {
                    ParseException parseException = (ParseException)e;
                    Debug.Log("Error saving high score.\r\nError message: " + parseException.Message + "\r\nErrorCode: " + parseException.Code);
                }

                result.IsCompleted = true;
                result.IsFaulted = true;

                yield break;
            }

            result.IsCompleted = true;
            PlayerPrefs.SetInt(currentGameMode.ModeName + "HighScore", currentGameMode.Score);
            GlobalData.Instance.highScores[currentGameMode.ModeName] = currentGameMode.Score;

        } else {
            //Update high score if recorded one is lower than the obtained one (or if the game changed versions in the meantime)
            var previousHighScore = task.Result;
            if (previousHighScore.Get<string>("gameVersion") != GlobalData.Instance.version || previousHighScore.Get<long>("score") < currentGameMode.Score ||
                (previousHighScore.Get<string>("gameMode") == "PeriodicMode" && previousHighScore.Get<string>("rawSeedUsed") != GlobalData.Instance.rawSeed)) { 
                previousHighScore["gameVersion"] = GlobalData.Instance.version;
                previousHighScore["score"] = currentGameMode.Score;
                if (currentGameMode.ModeName == "PeriodicMode") {
                    previousHighScore["seedUsed"] = GlobalData.Instance.periodicModeSeed;
                    previousHighScore["rawSeedUsed"] = GlobalData.Instance.rawSeed;
                }
                var saveTask = previousHighScore.SaveAsync();

                if (saveTask.IsFaulted) {
                    foreach (var e in task.Exception.InnerExceptions) {
                        ParseException parseException = (ParseException)e;
                        Debug.Log("Error updating high score.\r\nError message: " + parseException.Message + "\r\nErrorCode: " + parseException.Code);
                    }

                    result.IsCompleted = true;
                    result.IsFaulted = true;

                    yield break;
                }

                result.IsCompleted = true;
                PlayerPrefs.SetInt(currentGameMode.ModeName + "HighScore", currentGameMode.Score);
                GlobalData.Instance.highScores[currentGameMode.ModeName] = currentGameMode.Score;
            }
        }

        /* Facebook score saving was here
        var formData = new Dictionary<string, string>();
        formData.Add("score", currentGameMode.Score.ToString());
        FB.API("/me/scores", Facebook.HttpMethod.POST, PostScoreCallback, formData); //TODO-> Add callback and error handling*/
    }

    #region Getting info from Facebook and Parse
    string lastResponse;
    private Dictionary<string, Leaderboard.DTOUserScore> friendsInfo;
    void GetFriendsInfo() {
        var apiQuery = "/fql?q=" + WWW.EscapeURL(@"
            SELECT uid, first_name, last_name, name_format, pic_square FROM user 
            WHERE uid IN (SELECT uid FROM user WHERE is_app_user=1 
		            AND (uid IN (SELECT uid2 FROM friend WHERE uid1 = me()) OR uid = me()))");
        FB.API(apiQuery, Facebook.HttpMethod.GET, FriendsCallback);
    }

    void FriendsCallback(FBResult result) {
        if (!String.IsNullOrEmpty(result.Error)) {
            lastResponse = "Error Response 01:\r\n" + result.Error;
            Invoke("GetFriendsInfo", 5f);
        } else {
            var dict = Json.Deserialize(result.Text) as Dictionary<string, object>;

            var deserializedFriendList = new List<object>();
            deserializedFriendList = (List<object>)(((Dictionary<string, object>)dict)["data"]);
            friendsInfo = new Dictionary<string, Leaderboard.DTOUserScore>();
            if (deserializedFriendList.Count > 0) {
                foreach (var friendDict in deserializedFriendList) {
                    var userID = ((Dictionary<string, object>)friendDict)["uid"].ToString();
                    var firstName = (string)((Dictionary<string, object>)friendDict)["first_name"];
                    var lastName = (string)((Dictionary<string, object>)friendDict)["last_name"];
                    var nameFormat = (string)((Dictionary<string, object>)friendDict)["name_format"];
                    var pic = (string)((Dictionary<string, object>)friendDict)["pic_square"];
                    var name = nameFormat.Replace("{first}", firstName).Replace("{last}", lastName);

                    friendsInfo.Add(userID, new Leaderboard.DTOUserScore { UserID = userID.ToString(), Name = name, Pic = pic });
                }
            }

            GlobalData.Instance.FriendsInfo = friendsInfo;

            StartCoroutine(GetFriendsScores());
        }

        if (lastResponse != null) {
            Debug.Log(lastResponse);
        }
    }

    IEnumerator GetFriendsScores() {
        HashSet<string> friendsIDs = new HashSet<string>();
        foreach (var friend in friendsInfo) {
            friendsIDs.Add(friend.Key);
        }

        var parameters = new Dictionary<string, object> {
            {"gameVersion", GlobalData.Instance.version}, 
            {"friendsIDs", friendsIDs.ToList()},
            {"gameModes", new List<string> {"ClassicMode", "GrowthMode", "PeriodicMode"}}
        };

        var friendsHighScoresTask = ParseCloud.CallFunctionAsync<IEnumerable>("FriendsHighScores", parameters);

        while (!friendsHighScoresTask.IsCompleted) yield return null;

        if (friendsHighScoresTask.IsFaulted) {
            foreach (var e in friendsHighScoresTask.Exception.InnerExceptions) {
                ParseException parseException = (ParseException)e;
                Debug.Log("Error getting friends scores.\r\nError message: " + parseException.Message + "\r\nErrorCode: " + parseException.Code);
            }
        } else {
            IEnumerable<ParseObject> friendsHighScores = friendsHighScoresTask.Result.OfType<ParseObject>();

            var friendsScores = new List<Leaderboard.DTOUserScore>();

            foreach (var friendHighScore in friendsHighScores) {
                var facebookID = friendHighScore.Get<string>("facebookUserID");
                var friendScore = new Leaderboard.DTOUserScore {
                    Name = friendsInfo[facebookID].Name,
                    UserID = facebookID,
                    Score = friendHighScore.Get<int>("score"),
                    GameMode = friendHighScore.Get<string>("gameMode")
                };
                friendsScores.Add(friendScore);
            }

            GlobalData.Instance.FriendsScores = friendsScores;
        }
    }
    #endregion

    public void GoToMainMenu() {
        if (!CameraFade.Fading) { 
            Time.timeScale = 1f;
            var startingTime = Time.time;
            foreach (var playlistController in PlaylistController.Instances) {
                var localPlaylist = playlistController;
                localPlaylist.FadeToVolume(0f, 1f, () => {
                    localPlaylist.StopPlaylist();
                    //Debug.Log("Playlist " + localPlaylist.PlaylistName + " stopped. Elapsed time: " + (Time.time - startingTime));
                    localPlaylist.PlaylistVolume = 1f;
                });
            }
            CameraFade.StartAlphaFade(Color.white, false, 1.5f, 0f, () => { GoToScene("MainMenuScene"); /*Debug.Log("Scene changed. Elapsed time: " + (Time.time - startingTime));*/ });
        }
    }

    public void RestartScene() {
        if (!CameraFade.Fading) {
            Time.timeScale = 1f;

            foreach (var playlistController in PlaylistController.Instances) {
                var localPlaylist = playlistController;
                localPlaylist.FadeToVolume(0f, 1f, () => {
                    localPlaylist.StopPlaylist();
                    //Debug.Log("Playlist " + localPlaylist.PlaylistName + " stopped.");
                    localPlaylist.PlaylistVolume = 1f;
                });
            }
            CameraFade.StartAlphaFade(Color.white, false, 1.5f, 0f, () => { Application.LoadLevel(Application.loadedLevel); /*Debug.Log("Restarted scene.");*/ });
        }
    }

    internal void GoToScene(string scene) {
        Application.LoadLevel(scene);
    }

    public IEnumerator GetPeriodHash() {
        var task = ParseCloud.CallFunctionAsync<string>("PeriodicHash", new Dictionary<string, object>());

        while (!task.IsCompleted) yield return null;

        if (task.IsFaulted) {
            foreach (var e in task.Exception.InnerExceptions) {
                Debug.Log("Exception type: " + e.GetType());
                //Exception parseException = e;
                ParseException parseException = (ParseException)e;
                Debug.Log("Error message " + parseException.Message);
                Debug.Log("Error code: " + parseException.Code);
            }
            var ea = new PeriodHashEventArgs();
            ea.Seed = 0;
            ea.Success = false;
            OnPeriodHashObtained(ea);
        } else { 
            if (task.Result == null) {
                var ea = new PeriodHashEventArgs();
                ea.Seed = 0;
                ea.Success = false;
                OnPeriodHashObtained(ea);
            } else {
                var seed = task.Result.GetHashCode();
                GlobalData.Instance.rawSeed = task.Result.ToString();
                GlobalData.Instance.periodicModeSeed = seed;
                var ea = new PeriodHashEventArgs();
                ea.Seed = seed;
                ea.Success = true;
                OnPeriodHashObtained(ea);
            }
        }

        yield return null;
    }

    public event EventHandler<PeriodHashEventArgs> PeriodHashObtained;

    private void OnPeriodHashObtained(PeriodHashEventArgs e) {
        EventHandler<PeriodHashEventArgs> handler = PeriodHashObtained;
        if (handler != null) {
            handler(this, e);
        }
    }
}
