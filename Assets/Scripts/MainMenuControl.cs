using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Assets.Scripts.Util;
using System;
using Facebook.MiniJSON;
using I2.Loc;

public class MainMenuControl : MonoBehaviour {
    public tk2dCamera uiCamera;
    public dfLabel errorMessage;
    public List<dfButton> mainButtons;
    public Leaderboard leaderboard;
    public dfButton languageButton;

    private dfControl currentlySelectedMode;
    private HashSet<string> errorCalls;
    private bool showingErrorMessage;
    private string lastResponse;
    private bool FBisInit;
    
    void Awake() {
        Application.targetFrameRate = 60;
        FBisInit = false;
        errorCalls = new HashSet<string>();
        currentlySelectedMode = null;
    }

    // Use this for initialization
    void Start() {
        if (!FB.IsLoggedIn) {
            FB.Init(OnInitComplete, OnHideUnity);
        }

        //SelectButton(mainButtons[0], "ClassicMode"); //Should be classic mode

        leaderboard.GetComponent<dfTweenVector3>().Start();
    }

    // Update is called once per frame
    void Update() {
        if (errorCalls.Any() && !showingErrorMessage) {
            ShowErrorMessage();
        } else if (errorCalls.Count == 0 && showingErrorMessage) {
            HideErrorMessage();
        }
    }

    private void OnInitComplete() {
        FBisInit = true;

        FB.Login("email, publish_actions", LoginCallback);
    }

    void LoginCallback(FBResult result) {
        if (result.Error != null) {
            lastResponse = "Error Response:\n" + result.Error;
            ShowErrorMessage("There was an error retrieving your facebook information.");
            Invoke("OnInitComplete", 5f);
        } else if (!FB.IsLoggedIn) {
            lastResponse = "Login cancelled by Player";
        } else {
            lastResponse = "Login was successful!";
        }
    }

    private void OnHideUnity(bool isGameShown) {
        /*
        if (isGameShown) {
            Time.timeScale = 1;
        } else {
            Time.timeScale = 0;
        }
        //If false, should pause. Else should unpause
        Debug.Log("Is game showing? " + isGameShown);*/
    }

    private void ShowErrorMessage(string p = null) {
        showingErrorMessage = true;
        if (!String.IsNullOrEmpty(p)) {
            errorMessage.Text = p;
        }

        var tween = errorMessage.GetComponent<dfTweenFloat>();
        tween.StartValue = errorMessage.Opacity;
        tween.EndValue = 1;
        if (!tween.IsPlaying && tween.EndValue != errorMessage.Opacity) {
            tween.Play();
        }
    }

    private void HideErrorMessage() {
        showingErrorMessage = false;
        var tween = errorMessage.GetComponent<dfTweenFloat>();
        tween.StartValue = errorMessage.Opacity;
        tween.EndValue = 0;
        if (tween.EndValue != errorMessage.Opacity) {
            tween.Play();
        }
    }

    public void ClassicModeSelectd(dfControl control, dfMouseEventArgs mouseEvent) {
        if (currentlySelectedMode != control) {
            SelectButton((dfButton)control, "ClassicMode");
        } else {
            SelectMode((dfButton)control, "ClassicMode");
        }
    }

    public void GrowthModeSelected(dfControl control, dfMouseEventArgs mouseEvent) {
        if (currentlySelectedMode != control) {
            SelectButton((dfButton)control, "GrowthMode");
        } else {
            SelectMode((dfButton)control, "GrowthMode");
        }
    }

    public void MiniModeSelected(dfControl control, dfMouseEventArgs mouseEvent) {
        if (currentlySelectedMode != control) {
            SelectButton((dfButton)control, "MiniMode");
        } else {
            SelectMode((dfButton)control, "MiniMode");
        }
    }

    public void PeriodicModeSelected(dfControl control, dfMouseEventArgs mouseEvent) {
        if (currentlySelectedMode != control) {
            SelectButton((dfButton)control, "PeriodicMode");
        } else {
            SelectMode((dfButton)control, "PeriodicMode");
        }
    }

    public void TutorialModeSelected(dfControl control, dfMouseEventArgs mouseEvent) {
        if (currentlySelectedMode != control) {
            SelectButton((dfButton)control, "TutorialMode");
        } else {
            SelectMode((dfButton)control, "TutorialMode");
        }
    }

    private void SelectButton(dfButton control, string gameMode) {
        //foreach (var button in mainButtons) {
        //    button.BackgroundSprite = button.BackgroundSprite.Replace("Highlight", String.Empty);
        //}
        //control.BackgroundSprite = control.BackgroundSprite + "Highlight";

        foreach (var button in mainButtons) {
            button.GetComponents<dfTweenVector3>().Single(t => t.TweenName == "TweenSelect").EndValue = new Vector3(-470, button.Position.y);
        }
        control.GetComponents<dfTweenVector3>().Single(t => t.TweenName == "TweenSelect").EndValue = new Vector3(-400, control.Position.y);

        foreach (var button in mainButtons) {
            button.GetComponents<dfTweenVector3>().Single(t => t.TweenName == "TweenSelect").Play();
        }

        currentlySelectedMode = control;

        if (gameMode != "TutorialMode") { 
            leaderboard.OnModeSelected(gameMode);
        }
    }

    private void SelectMode(dfButton control, string gameMode) {
        control.IsInteractive = false;
        var dfTween = control.GetComponents<dfTweenVector3>().Single(t => t.TweenName == "TweenIn");
        var endValue = dfTween.EndValue;
        dfTween.EndValue = dfTween.StartValue;
        dfTween.StartValue = endValue;
        dfTween.Play();
        CameraFade.StartAlphaFade(Color.white, false, 1.8f, 0f, () => {
            GlobalData.Instance.GameMode = gameMode;
            if (gameMode != "PeriodicMode") {
                Application.LoadLevel("GameScene");
            } else {
                Application.LoadLevel("LoadingScene");
            }
        });
    }

    public void ChangeLanguage(dfControl control, dfMouseEventArgs mouseEvent) {
        var button = (dfButton)control;

        if (control.name == "Spanish Button") {
            LocalizationManager.CurrentLanguage = "Español";
            PlayerPrefs.SetString("Language", "Español");
        } else if (control.name == "English Button") {
            LocalizationManager.CurrentLanguage = "English";
            PlayerPrefs.SetString("Language", "English");
        }
    }
}
