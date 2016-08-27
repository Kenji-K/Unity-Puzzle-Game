using UnityEngine;
using System.Collections;

public class LeaderboardEntry : MonoBehaviour {
    public dfLabel position;
    public dfLabel name;
    public dfLabel score;
    public dfLabel rankName;
    public dfWebSprite photo;

	// Use this for initialization
	void Start () {
        rankName.IsVisible = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
