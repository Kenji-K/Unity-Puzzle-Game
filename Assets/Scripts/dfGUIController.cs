using UnityEngine;
using System.Collections;

public class dfGUIController : MonoBehaviour {
    public dfPanel globalPanel;
    dfGUIManager dfManager;

	// Use this for initialization
	void Start () {
        dfManager = GetComponent<dfGUIManager>();
		dfManager.UIScaleLegacyMode = false;
	}
	
	// Update is called once per frame
	void Update () {
        var screenSize = dfManager.GetScreenSize();
        if (screenSize.x / screenSize.y <= 16f / 9 && !Mathf.Approximately(screenSize.x, globalPanel.Size.x)) {
            dfManager.UIScale = dfManager.UIScale * screenSize.x / globalPanel.Size.x;
        } else if (screenSize.x / screenSize.y > 16f / 9) {
            dfManager.UIScale = 1;
        }
	}
}
