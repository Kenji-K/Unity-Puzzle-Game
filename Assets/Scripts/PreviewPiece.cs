using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Util.Extensions;
using System.Linq;

public class PreviewPiece : MonoBehaviour {
	//public GameObject previewBlockPrefab;
	private tk2dTileMap tileMap;
	public int Variant { get; set; }
	public Grid<int> Layout { get; set; }
    public GameObject Cursor { get; private set; }
    public static readonly int MAX_WIDTH = 3;
    public static readonly int MAX_HEIGHT = 3;

    void Awake() {
        tileMap = GetComponent<tk2dTileMap>();

        var childRenderer = tileMap.renderData.GetComponentInChildren<Renderer>();
        childRenderer.material.shader = Shader.Find("Custom/BlendVertexTransparentColor");
        childRenderer.material.color = new Color(childRenderer.material.color.r, childRenderer.material.color.g, childRenderer.material.color.b, 0);
    }

	// Use this for initialization
	void Start () {
		iTween.ValueTo(gameObject, iTween.Hash(
			"name", "tweenAlpha",
			"from", 0f, 
			"to", 1f, 
			"time", 1f,
			"onupdate", "TweenAlpha"));

        Cursor.transform.localScale = new Vector3(0.5f, 0.5f, 1);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void TweenAlpha(float alphaValue) {
		var childRenderer = tileMap.renderData.GetComponentInChildren<Renderer>();
		childRenderer.material.color = new Color(childRenderer.material.color.r, childRenderer.material.color.g, childRenderer.material.color.b, alphaValue);
	}

	public void Init(Grid<int> layout, int variant) {
		Variant = variant;
		Layout = layout;

        tileMap = GetComponent<tk2dTileMap>();
        tileMap.renderData.transform.localScale = new Vector3(0.5f, 0.5f, 1);
        Refresh();

		tileMap.renderData.transform.parent = transform;

        var collider = GetComponent<BoxCollider2D>();
        collider.size *= tileMap.renderData.transform.localScale.x;
        collider.center *= tileMap.renderData.transform.localScale.x;

        Cursor = transform.FindChild("Selector").gameObject;
        Cursor.transform.localPosition = tileMap.data.tileSize * tileMap.renderData.transform.localScale.x;
	}

    public void Refresh() {
        for (int i = 0; i < MAX_WIDTH; i++) {
            for (int j = 0; j < MAX_HEIGHT; j++) {
                if (Layout[i, j] > 0) {
                    tileMap.SetTile(i, j, 0, Layout[i, j] - 1 + Variant * Block.MAX_VALUE);
                } else {
                    tileMap.ClearTile(i, j, 0);
                }
            }
        }

        tileMap.Build();
    }

	void OnDestroy() {
		var children = new List<GameObject>();
		foreach (Transform child in transform) children.Add(child.gameObject);
		children.ForEach(child => Destroy(child));
	}

	void Destroy() {
		Destroy(gameObject);
	}
}
