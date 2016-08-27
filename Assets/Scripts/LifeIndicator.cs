using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Util.Extensions;

public class LifeIndicator : MonoBehaviour {
	// Start is called just before any of the
	// Update methods is called the first time.
	void Start () {
        iTween.MoveFrom(gameObject, iTween.Hash("y", transform.position.y - 1f, "time", 1f, "easetype", "easeOutBack"));
        iTween.ColorFrom(gameObject, iTween.Hash("a", 0, "time", 1f, "easetype", "easeOutCubic"));
    }
	
	// Update is called every frame, if the
	// MonoBehaviour is enabled.
	void Update () {
		
	}

    public IEnumerator Consume() {
        var leftPiece = transform.Search("LeftPiece").gameObject;
        var rightPiece = transform.Search("RightPiece").gameObject;

        leftPiece.SetActive(true);
        rightPiece.SetActive(true);

        renderer.enabled = false;

        var animLength = 0.5f;

        leftPiece.GetComponent<Rigidbody2D>().velocity = new Vector2(-0.3f, 5f);
        leftPiece.GetComponent<Rigidbody2D>().angularVelocity = 10;
        rightPiece.GetComponent<Rigidbody2D>().velocity = new Vector2(0.3f, 5f);
        rightPiece.GetComponent<Rigidbody2D>().angularVelocity = -10;
        iTween.ColorTo(leftPiece, iTween.Hash("a", 0, "time", animLength, "easetype", "easeOutQuad"));
        iTween.ColorTo(rightPiece, iTween.Hash("a", 0, "time", animLength, "easetype", "easeOutQuad"));

        yield return new WaitForSeconds(animLength);
        gameObject.DestroyAll();
    }
}
