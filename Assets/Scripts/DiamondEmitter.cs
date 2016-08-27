using UnityEngine;
using System.Collections;

public class DiamondEmitter : MonoBehaviour {
    public ParticleSystem trail;

	// Use this for initialization
	void Start () {
        //rigidbody2D.WakeUp();
        //rigidbody2D.velocity = new Vector2(0, 8f);
        //rigidbody2D.AddForce(new Vector2(0, 5f));

        trail.renderer.sortingLayerName = "Background";
        trail.renderer.sortingOrder = -2;
	}
	
	void FixedUpdate () {
        if (rigidbody2D.IsAwake()) {
            rigidbody2D.AddForce(new Vector2(0, 1f));
        }
	}

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Destroyer") { 
            //rigidbody2D.position.Set(Random.Range(0.5f, 18.7f), -2f);
            gameObject.transform.position = new Vector3(transform.position.x, Random.Range(-8f, -1f));
            rigidbody2D.velocity = new Vector2(0, 0);
            rigidbody2D.Sleep();
        }
    }
}
