using UnityEngine;
using System.Collections;
using Assets.Scripts.Util.Extensions;

public class ControlHint : MonoBehaviour {

	// Use this for initialization
	void Start () {
        iTween.MoveFrom(gameObject, iTween.Hash("position", transform.position - new Vector3(0, 0.5f), "time", 0.4f, "easetype", "easeinoutcubic"));
        iTween.ColorFrom(gameObject, iTween.Hash("a", 0, "time", 0.4f));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void FadeOut() {
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine() {
        iTween.MoveTo(gameObject, iTween.Hash("position", transform.position - new Vector3(0, 0.5f), "time", 0.4f, "easetype", "easeinoutcubic"));
        iTween.ColorTo(gameObject, iTween.Hash("a", 0, "time", 0.4f));

        yield return new WaitForSeconds(0.4f);

        this.DestroyAll(gameObject);
    }
}
