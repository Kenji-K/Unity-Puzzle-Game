using UnityEngine;
using System.Collections;
using Assets.Scripts.Util;

public class LoadingControllerObject : MonoBehaviour {
    public dfSprite LoadingAnimation;
    public dfLabel Message;

	// Use this for initialization
	void Start () {
        LoadingAnimation.Color = GlobalData.Instance.SolidColors[GlobalData.Instance.GameMode];
        PersistentUtility.Instance.PeriodHashObtained += OnPeriodHashObtained;
        CameraFade.StartAlphaFade(Color.white, true, 1f, 0, () => {
            StartCoroutine(PersistentUtility.Instance.GetPeriodHash());
        });
	}
	
	// Update is called once per frame
	void Update () {

	}

    void OnPeriodHashObtained(object sender, PeriodHashEventArgs e) {
        if (e.Success) {
            GlobalData.Instance.periodicModeSeed = e.Seed;
            Message.Text = I2.Loc.ScriptLocalization.Get("SeedObtained");
            CameraFade.StartAlphaFade(Color.white, false, 1f, 2f, () => {
                PersistentUtility.Instance.GoToScene("GameScene");
            });
        } else {
            var animation = LoadingAnimation.GetComponent<dfSpriteAnimation>();
            animation.Stop();
            LoadingAnimation.IsVisible = false;
            Message.Text = I2.Loc.ScriptLocalization.Get("ErrorTryLater");
            CameraFade.StartAlphaFade(Color.white, false, 1f, 2f, () => {
                PersistentUtility.Instance.GoToScene("MainMenuScene");
            });
        }
    }
}
