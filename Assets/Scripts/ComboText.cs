using UnityEngine;
using System.Collections;

public class ComboText : MonoBehaviour {
    public dfLabel number;
    public dfLabel type;
    public float TimeTillExit { get; set; }
    public bool Hidden { 
        get {
            return number.Opacity == 0 && type.Opacity == 0 && !showing;
        } 
    }

    private bool showing;
    private Vector3 StartingPosition { get; set; }

	// Use this for initialization
    void Start() {
        StartingPosition = GetComponent<dfPanel>().Position;
        TimeTillExit = 0;
        Reset();
        //StartCoroutine(AnimateIn());
	}
	
	// Update is called once per frame
	void Update () {
        if (TimeTillExit < 0) {
            if (showing) { 
                GetComponent<dfTweenVector3>().Play();
                showing = false;
            }
        } else {
            TimeTillExit -= Time.deltaTime;
        }
	}

    public void AnimateIn() {
        number.gameObject.animation.Play();
        number.gameObject.GetComponent<dfTweenFloat>().Play();
        type.gameObject.animation.Play();
        type.gameObject.GetComponent<dfTweenFloat>().Play();

        showing = true;
    }

    public void Reset() {
        number.Opacity = 0;
        type.Opacity = 0;
        showing = false;
        GetComponent<dfPanel>().Position = StartingPosition;
        var comboTextQueue = GameControl.Instance.comboTextQueue;
        if (comboTextQueue.Count > 1) {
            comboTextQueue.Dequeue();
        }
        comboTextQueue.Enqueue(this);
    }
}
