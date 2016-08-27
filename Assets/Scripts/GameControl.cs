using UnityEngine;
using Assets.Scripts.Util.Extensions;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.GameModes;
using Assets.Scripts;
using System.Linq;
using Parse;
using Assets.Scripts.Util;
using System.Globalization;
using System.Threading;

public class GameControl : MonoBehaviour {
    public const int TOTAL_PIECES = 6;

    public tk2dCamera mainCamera;
    public Board boardPrefab;
    public Piece piecePrefab;
    public MultiplierBar bonusBarPrefab;
    public PreviewPiece previewPiecePrefab;
    public EffectManager textFxPrefab;
    public EffectManager shimmerTextFxPrefab;
    public tk2dTextMesh tk2dTextPrefab;
    public GameObject squareSolvedEffectPrefab;
    public EffectManager messageTextPrefab;
    public GameUIController uiController;
    public dfPanel menuPanel;
    public PauseMenuController pauseMenu;
    public dfLabel targetPlayerLbl;
    public dfLabel targetScoreLbl;
    public dfLabel scoreLbl;
    public ComboText comboTextPrefab;
    public Queue<ComboText> comboTextQueue;
    public dfLabel boardClearText;
    public LifeCount lifeCounter;
    public dfLabel bottomTimer;
    public bool processNextMove;

    private float prevAspectRatio;
    private Board gameBoard;
    private bool musicOn;
    private float waitUntilGameOverDismissal;

    private const float gameStartDelay = 1.6f;

    private Bounds gameplayBounds;
    public Bounds GameplayBounds {
        get { return gameplayBounds; }
        set { gameplayBounds = value; }
    }

    private GameMode currentGameMode;
    internal GameMode CurrentGameMode {
        get {
            return currentGameMode;
        }
        private set {
            currentGameMode = value;
        }
    }

    //private EffectManager gameOverText;
    //public EffectManager GameOverText {
    //    get { return gameOverText; }
    //    set { gameOverText = value; }
    //}
    
    public Grid<Slot> SlotGrid {
        get { return gameBoard.SlotGrid; }
    }

    private Piece currentPiece;
    public Piece CurrentPiece {
        get {
            return currentPiece;
        }
        set {
            currentPiece = value;
        }
    }

    private List<Grid<int>> pieceLayouts;
    public List<Grid<int>> PieceLayouts {
        get { return pieceLayouts; }
        set { pieceLayouts = value; }
    }

    //Singleton
    private static GameControl instance;
    public static GameControl Instance {
        get {
            return instance; 
        }
    }

    private bool gameStarted;
    public bool GameStarted {
        get { return gameStarted; }
        set { gameStarted = value; }
    }

    private bool paused;
    public bool Paused {
        get { return paused; }
        set { paused = value; }
    }

    private bool gameOver;
    public bool GameOver {
        get { return gameOver; }
        set { gameOver = value; }
    }

    //The shifting that had to be done to the last piece complication
    private IntVector2 lastShift;
    public IntVector2 LastShift {
        get { return lastShift; }
        set { lastShift = value; }
    }

    public bool ClearAnimationOngoing { get; set; }

    public PlaylistController simpleSongController { get; set; }
    public PlaylistController enhancedSongController { get; set; }

    void Awake() {
        GameControl.instance = this;
        Paused = false;
        GameStarted = false;
        ClearAnimationOngoing = false;
        GameOver = false;
        musicOn = true;
        processNextMove = true;
        InitializeCamera();
        lastShift = new IntVector2(0, 0);
        comboTextQueue = new Queue<ComboText>();
        foreach (var comboText in FindObjectsOfType(typeof(ComboText))) {
            comboTextQueue.Enqueue((ComboText)comboText);
        }

        simpleSongController = PlaylistController.InstanceByName("PC Simple Music");
        enhancedSongController = PlaylistController.InstanceByName("PC Enhanced Music");

        if (GlobalData.Instance.FriendsScores == null) { 
            GlobalData.Instance.OnFriendsScoresSet += ShowSuperiorPlayer;
        }

        if (GlobalData.Instance.GlobalTopScores == null) {
            GlobalData.Instance.OnGlobalTopScoresSet += ShowSuperiorPlayer;
        }
    }

    void OnDestroy() {
        GlobalData.Instance.OnFriendsScoresSet -= ShowSuperiorPlayer; 
        GlobalData.Instance.OnGlobalTopScoresSet -= ShowSuperiorPlayer;
    }

	// Use this for initialization
	void Start() {
        //TODO -> Change this to a different, more secure system if needed
        if (!PlayerPrefs.HasKey("Version")) {
            PlayerPrefs.SetString("Version", GlobalData.Instance.version);
        }

        if (PlayerPrefs.GetString("Version").CompareTo(GlobalData.Instance.version) < 0) {
            //Do something if version has changed from last time we played.
            PlayerPrefs.SetString("Version", GlobalData.Instance.version);
        }

        SelectGameMode(GlobalData.Instance.GameMode);
	}
    
	// Update is called once per frame
	void Update () {
        if (GameStarted && !GameOver && Input.GetKeyUp(KeyCode.Escape)) {
            //Fade music to volume
            pauseMenu.Toggle();
        }

        if (GameOver && waitUntilGameOverDismissal <= 0 && !CameraFade.Fading) {
            if (musicOn) {
                foreach (var playlistController in PlaylistController.Instances) {
                    var localPlaylist = playlistController;
                    localPlaylist.FadeToVolume(0f, 1f, () => {
                        localPlaylist.StopPlaylist();
                        localPlaylist.PlaylistVolume = 1f;
                    });
                }
                musicOn = false;
            }

            if (Input.GetMouseButtonUp((int)Mouse.LeftButton) || Input.anyKey) {
                CameraFade.StartAlphaFade(Color.white, false, 1.2f, 0f, () => { PersistentUtility.Instance.GoToScene("ScoreScene"); });                
            }
        }

        if (waitUntilGameOverDismissal > 0) {
            waitUntilGameOverDismissal -= Time.deltaTime;
        }

        if (!Mathf.Approximately(prevAspectRatio, mainCamera.camera.aspect)) {
            InitializeCamera();
        }
	}

    /********************** Custom functions **************************/

    public void SelectGameMode(string gameMode) {
        switch (gameMode) {
            case "GrowthMode":
                CurrentGameMode = gameObject.AddComponent<GrowthGameMode>();
                CurrentGameMode.GameController = this;
                break;
            case "TutorialMode":
                CurrentGameMode = gameObject.AddComponent<TutorialGameMode>();
                CurrentGameMode.GameController = this;
                break;
            case "ClassicMode":
                CurrentGameMode = gameObject.AddComponent<ClassicGameMode>();
                CurrentGameMode.GameController = this;
                break;
            case "MiniMode":
                CurrentGameMode = gameObject.AddComponent<MiniGameMode>();
                CurrentGameMode.GameController = this;
                break;
            case "PeriodicMode":
                CurrentGameMode = gameObject.AddComponent<PeriodicGameMode>();
                CurrentGameMode.GameController = this;
                break;
            default:
                throw new System.ArgumentException("Mode is " + gameMode);
        }

        GlobalData.Instance.GameMode = gameMode;

        CurrentGameMode.ScoreChangedEvent += UpdateScoreText;
        
        InitializeGameBoard();
        InitializePieceLayouts();
        InitializeUI();
        CurrentGameMode.Initialize(PieceLayouts);
        StartCoroutine(CurrentGameMode.StartGame(gameStartDelay));
    }

    private void InitializeCamera() {
        if (mainCamera.camera.aspect >= 16f / 9) {
            GameplayBounds = new Bounds(new Vector3(9.6f, 5.4f, 0f), new Vector3(10.8f * mainCamera.camera.aspect /*19.2f*/, 10.8f, 0f));
        } else {
            GameplayBounds = new Bounds(new Vector3(9.6f, 5.4f, 0f), new Vector3(19.2f, 19.2f / mainCamera.camera.aspect /*10.8f*/, 0f));
        }

        if (!mainCamera.camera.isOrthoGraphic) {
            float distance;
            float height = GameplayBounds.size.y;
            float fov = mainCamera.camera.fieldOfView;

            distance = 0.5f * height / (Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad));
            mainCamera.camera.transform.position = GameplayBounds.center + (Vector3.back * (GameplayBounds.extents.z + distance));
            mainCamera.camera.transform.LookAt(GameplayBounds.center);

            var gameplayBounds = GameplayBounds;
            gameplayBounds.size = new Vector3(19.2f, 10.8f, 0f);
            GameplayBounds = gameplayBounds;
        }

        prevAspectRatio = mainCamera.camera.aspect;
    }

    private void InitializePieceLayouts() {
        pieceLayouts = new List<Grid<int>>();

        var layout = new Grid<int>();
        layout[0, 0] = 1; layout[1, 0] = 1;
        pieceLayouts.Add(layout);

        layout = new Grid<int>();
        layout[0, 0] = 1; layout[1, 0] = 1;
        pieceLayouts.Add(layout);

        layout = new Grid<int>();
        layout[0, 0] = 1; layout[1, 0] = 1;
        pieceLayouts.Add(layout);

        layout = new Grid<int>();
        layout[0, 0] = 1; layout[1, 0] = 1;
        pieceLayouts.Add(layout);

        layout = new Grid<int>();
        layout[0, 0] = 1; /*layout[1, 0] = 0;*/
        /*layout[0, 1] = 0;*/ layout[1, 1] = 1;
        pieceLayouts.Add(layout);

        layout = new Grid<int>();
        layout[0, 0] = 1; /*layout[1, 0] = 0;*/
        /*layout[0, 1] = 0;*/ layout[1, 1] = 1;
        pieceLayouts.Add(layout);

    }

    private void InitializeGameBoard() {
        gameBoard = Instantiate(boardPrefab) as Board;
        gameBoard.gameObject.name = boardPrefab.name;
    }

    private void InitializeUI() {
        var opacityTween = uiController.GetComponent<dfTweenFloat>();
        opacityTween.Play();

        scoreLbl.Text = "0";

        if (CurrentGameMode.ModeName == "TutorialMode") {
            targetPlayerLbl.Text = "Tutorial Mode";
            targetScoreLbl.Text = "0";
        } else {
            ShowSuperiorPlayer();
        }

        string modeName = CurrentGameMode.ModeName;


        var labels = FindObjectsOfType<dfLabel>();
        foreach (var label in labels) {
            if (label.Color.r == 14 && label.Color.g == 103 && label.Color.b == 163) {
                label.Color = GlobalData.Instance.SolidColors[modeName];
            }
        }

        var buttons = FindObjectsOfType<dfButton>();
        foreach (var button in buttons) {
            if (button.NormalBackgroundColor.r == 14 && button.NormalBackgroundColor.g == 103 && button.NormalBackgroundColor.b == 163) {
                button.NormalBackgroundColor = GlobalData.Instance.SolidColors[modeName];
                button.HoverBackgroundColor = GlobalData.Instance.HighlightColors[modeName];
                button.FocusBackgroundColor = GlobalData.Instance.SolidColors[modeName];
                button.DisabledColor = GlobalData.Instance.SolidColors[modeName];
                button.PressedBackgroundColor = GlobalData.Instance.SolidColors[modeName];
            }
        }

        var progressBars = FindObjectsOfType<dfProgressBar>();
        foreach (var progressBar in progressBars) {
            if (progressBar.ProgressColor.r == 14 && progressBar.ProgressColor.g == 103 && progressBar.ProgressColor.b == 163) {
                progressBar.ProgressColor = GlobalData.Instance.SolidColors[modeName];
            }
        }

        //foreach (var slot in lifeCounter.slots) {
        //    var currentColor = GlobalData.Instance.SolidColors[CurrentGameMode.ModeName];
        //    slot.Color = new Color32(currentColor.r, currentColor.g, currentColor.b, 128);
        //}
        lifeCounter.Count = 0;

        uiController.ChangeImages(CurrentGameMode.ModeName);
        if (modeName == "ClassicMode") modeName = "NormalMode"; //KLUDGE: los nombres de los sprites dicen Normal en lugar de Classic
        string strippedString = modeName.Replace("Mode", string.Empty);
        pauseMenu.pauseIndicator.SpriteName = strippedString + "PausedBig";
        //pauseMenu.pauseButton.BackgroundSprite = strippedString + "Pause";
    }

    internal IEnumerator ResolveBoardChange(float blockDestructionDelay) {
        yield return StartCoroutine(CurrentGameMode.ClearBoard(blockDestructionDelay));

        if (!processNextMove) {
            StartCoroutine(ResolveBoardChange(blockDestructionDelay));
            processNextMove = true;
            yield break;
        }

        if (CurrentGameMode.IsGameOver()) {
            this.DestroyAll(currentPiece.gameObject);
            //gameOverText.Text = "Game Over\r\n";

            if (FB.IsLoggedIn) {
                //Try to save high score
                var result = PersistentUtility.Instance.SaveHighScore(CurrentGameMode);
            }

            //Send playthrough information to server
            var scoreParse = new ParseObject("Score");
            scoreParse["score"] = currentGameMode.Score;
            scoreParse["player"] = GlobalData.Instance.playerName;
            scoreParse["facebookUserID"] = GlobalData.Instance.playerFacebookID;
            scoreParse["gameVersion"] = GlobalData.Instance.version;
            scoreParse["gameMode"] = currentGameMode.ModeName;
            scoreParse["squaresCleared"] = currentGameMode.SquaresCleared;
            scoreParse["blocksCleared"] = currentGameMode.BlocksCleared;
            scoreParse["piecesPlaced"] = currentGameMode.PiecesPlaced;
            scoreParse["levelReached"] = currentGameMode.Level;
            scoreParse["maxChain"] = currentGameMode.MaxChain;
            scoreParse["maxCombo"] = currentGameMode.MaxCombo;
            scoreParse["totalChains"] = currentGameMode.TotalChains;
            scoreParse["totalCombos"] = currentGameMode.TotalCombos;
            scoreParse["timePlayed"] = currentGameMode.TimePlayed;
            scoreParse["hintsUsed"] = currentGameMode.HintsUsed;
            if (currentGameMode.ModeName == "PeriodicMode") {
                scoreParse["seedUsed"] = GlobalData.Instance.periodicModeSeed;
                scoreParse["rawSeedUsed"] = GlobalData.Instance.rawSeed;
            }

            scoreParse.SaveAsync();
            GlobalData.Instance.GameStats = scoreParse;

            //gameOverText.PlayAnimation();
            GameUIController.Instance.noMoreMovesMessage.GetComponents<dfTweenGroup>().Single(tg => tg.TweenName == "TweenCombined").Play();
            GameUIController.Instance.gameOverMessage.GetComponents<dfTweenVector3>().Single(t => t.TweenName == "TweenIn").Play();
            GameOver = true;
            waitUntilGameOverDismissal = 1.25f;
        } else {
            CurrentGameMode.UserHasPlayed = true;
            if (currentPiece.gameObject.activeSelf) { 
                LayoutDTO nextPieceInfo = CurrentGameMode.GetNextPiece();
                if (nextPieceInfo.Variant == Block.TOTAL_VARIANTS + 1) {
                    GameUIController.Instance.NoMoreMovesTweenAlone.Play();
                    CurrentPiece.gameObject.SetActive(false);
                }
                if (nextPieceInfo != null) {
                    CurrentPiece.InitPiece(nextPieceInfo.Layout, nextPieceInfo.Variant);
                } else {
                    this.DestroyAll(CurrentPiece.gameObject);
                }
            }
        }
    }

    /* private void PostScoreCallback(FBResult result) {
        if (string.IsNullOrEmpty(result.Text) || result.Text == "false") {
            Debug.Log("An error occurred while posting score to fb: " + result.Error);
        } else {
            Debug.Log("Score posted successfully.");
        }
    }
    */

    private void UpdateScoreText(int previousScore, int updatedScore) {
        if (CurrentGameMode.ModeName != "TutorialMode") {
            ShowSuperiorPlayer();
        }

        iTween.StopByName(gameObject, "iTweenScore");

        var previouslyDisplayedScore = int.Parse(scoreLbl.Text);

        iTween.ValueTo(gameObject, iTween.Hash(
            "name", "iTweenScore",
            "from", previouslyDisplayedScore, 
            "to", updatedScore, 
            "time", 0.7f, 
            "easetype", "easeOutCirc",
            "onupdate", "TweenScore"));
    }

    private void ShowSuperiorPlayer() {
        //Debug.Log("Called ShowSuperiorPlayer!");
        var updatedScore = CurrentGameMode.Score;
        Leaderboard.DTOUserScore nextHighScore = null;
        IEnumerable<Leaderboard.DTOUserScore> higherScores = null;
        if (GlobalData.Instance.FriendsScores != null) {
            higherScores = (from score in GlobalData.Instance.FriendsScores
                            where score.GameMode == CurrentGameMode.ModeName &&
                                    score.Score > updatedScore
                            orderby score.Score ascending
                            select score);

            nextHighScore = higherScores.FirstOrDefault();
        }

        //foreach (var highScore in GlobalData.Instance.FriendsScores) {
        //    Debug.Log("Name: " + highScore.Name + " - Score: " + highScore.Score + " - GameMode: " + highScore.GameMode);
        //}

        if (higherScores != null && nextHighScore != null) {
            targetScoreLbl.Text = nextHighScore.Score.ToString();
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            targetPlayerLbl.Text = string.Format("{0} - {1} {2}", nextHighScore.Name.Substring(0, Mathf.Min(nextHighScore.Name.Length, 18)), textInfo.ToTitleCase(I2.Loc.ScriptLocalization.Get("RANK")), higherScores.Count());
        } else {
            //Fetch the global high score
            if (GlobalData.Instance.GlobalTopScores != null) {
                var globalTopScore = GlobalData.Instance.GlobalTopScores.SingleOrDefault(s => s.Score > updatedScore && s.GameMode == CurrentGameMode.ModeName);
                if (globalTopScore != null) {
                    targetPlayerLbl.Text = I2.Loc.ScriptLocalization.Get("GlobalScoreChallenge");
                    targetScoreLbl.Text = globalTopScore.Score.ToString();
                } else {
                    targetPlayerLbl.Text = I2.Loc.ScriptLocalization.Get("GlobalRecord");
                    targetScoreLbl.Text = CurrentGameMode.Score.ToString();
                }
            } else {
                targetPlayerLbl.Text = "Fetching Top Score";
                targetScoreLbl.Text = "0";
            }
        }
    }

    private void TweenScore(int score) {
        scoreLbl.Text = score.ToString();
    }

    internal Grid<int> ComplicateLayout(int index) {
        return ComplicateLayout(pieceLayouts[index]);
    }

    internal Grid<int> ComplicateLayout(Grid<int> originalLayout) {
        var layout = originalLayout.Copy();
        var width = layout.Width;
        var height = layout.Height;
        var candidateList = new List<IntVector2>();
        var extraCol = width < 3 ? 1 : 0;
        var extraRow = height < 3 ? 1 : 0;

        for (int i = -extraCol; i < width + extraCol; i++) {
            for (int j = -extraRow; j < height + extraRow; j++) {
                if (IsComplicationCandidate(layout, i, j)) {
                    candidateList.Add(new IntVector2(i, j));
                }
            }
        }

        if (candidateList.Count == 0) return layout;

        var pickedIndex = Random.Range(0, candidateList.Count);
        var chosenCoord = candidateList[pickedIndex];

        int xShift = 0;
        if (chosenCoord.x < 0) {
            xShift = 1;
        }

        int yShift = 0;
        if (chosenCoord.y < 0) {
            yShift = 1;
        }

        lastShift.x = xShift;
        lastShift.y = yShift;
        layout.ShiftPositions(xShift, yShift);
        layout[chosenCoord.x + xShift, chosenCoord.y + yShift]++;
        return layout;
    }
    
    internal bool SimplifyLayout(int index) {
        var layout = pieceLayouts[index];
        var width = layout.Width;
        var height = layout.Height;

        var layoutTotalValue = layout.Sum();
        if (layoutTotalValue <= 2) return false;

        var candidateSet = new HashSet<IntVector2>();
        for (int c = 0; c < width; c++) {
            for (int r = 0; r < height; r++) {
                if (layout[c, r] >= 1) {
                    candidateSet.Add(new IntVector2(c, r));
                }
            }
        }

        while (candidateSet.Any()) {
            var testLayout = layout.Copy();
            var chosenPosition = candidateSet.RandomElement();

            testLayout[chosenPosition.x, chosenPosition.y]--;
            if (HasOneIsle(testLayout)) {
                layout[chosenPosition.x, chosenPosition.y]--;
                if (layout[chosenPosition.x, chosenPosition.y] == 0) {
                    layout.Remove(chosenPosition);
                    //Shift columns and/or rows if they are left empty

                    var xShift = 0;
                    var yShift = 0;
                    if (chosenPosition.x == 0 && layout.GetColumn(chosenPosition.x).Sum() == 0) {
                        xShift = -1;
                    }

                    if (chosenPosition.y == 0 && layout.GetRow(chosenPosition.y).Sum() == 0) {
                        yShift = -1;
                    }

                    layout.ShiftPositions(xShift, yShift);
                }
                return true;
            } else {
                candidateSet.Remove(chosenPosition);
            }
        }

        return false;
    }

    private bool IsComplicationCandidate(Grid<int> layout, int x, int y) {
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                var outOfBounds = x + i < 0 || x + i >= gameBoard.TotalColumns || y + j < 0 || y + j >= gameBoard.TotalRows;
                if (outOfBounds) continue;

                if (i == 0 & j == 0 && layout[x, y] < Block.MAX_VALUE && layout[x, y] != 0) return true;
                var selfOutOfBounds = x < 0 || x >= gameBoard.TotalColumns || y < 0 || y >= gameBoard.TotalRows;
                if ((i != 0 || j != 0) && layout[x + i, y + j] > 0) {
                    if (!selfOutOfBounds && layout[x, y] < Block.MAX_VALUE) return true;
                    else if (selfOutOfBounds) return true;
                }
            }
        }

        return false;
    }

    private bool HasOneIsle(Grid<int> layout) {
        var width = layout.Width;
        var height = layout.Height;
        var occupiedCoordinates = layout.GetPositionsWhere(p => p > 0);

        var visitedCoords = new Grid<bool>();
        for (int c = 0; c < width; c++) {
            for (int r = 0; r < height; r++) {
                if (layout[c, r] > 0) {
                    var isle = GetLayoutIsle(layout, new IntVector2(c, r), visitedCoords);
                    //Returns true if the isle found by expanding on (c, r) contains all the positive elements of the layout
                    return isle.SetEquals(occupiedCoordinates);
                }
            }
        }

        return false;
    }

    private HashSet<IntVector2> GetLayoutIsle(Grid<int> layout, IntVector2 position, Grid<bool> visited) {
        if (position.x < 0 ||
            position.y < 0 ||
            position.x >= layout.Width ||
            position.y >= layout.Height ||
            layout[position.x, position.y] <= 0 ||
            visited[position.x, position.y]) {
            return null;
        }

        visited[position.x, position.y] = true;

        var set = new HashSet<IntVector2>();
        set.Add(position);
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 && y == 0) continue;

                var result = GetLayoutIsle(layout, position + new IntVector2(x, y), visited);
                if (result != null) {
                    set.UnionWith(result);
                }
            }
        }

        return set;
    }

    public void ActivatePiece() {
        CurrentPiece.gameObject.SetActive(true);
    }
}
