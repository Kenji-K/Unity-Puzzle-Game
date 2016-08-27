using UnityEngine;
using System.Collections;
using Assets.Scripts;
using Assets.Scripts.Util.Extensions;

public class ComplicationPreview : MonoBehaviour {
    public tk2dTileMap normalPiece;
    public tk2dTileMap resultingPiece;

    private float normalAlpha;
    private int direction;
    private bool startCrossFade;
    public bool StartCrossFade {
        get { return startCrossFade; }
        set { startCrossFade = value; }
    }
    private bool crossFading;

    public LayoutDTO normalLayout { get; set; }
    public LayoutDTO resultingLayout { get; set; }

    void Awake() {

    }

    // Use this for initialization
    void Start() {
        normalPiece.Build();
        resultingPiece.Build();

        var renderer = normalPiece.renderData.GetComponentInChildren<Renderer>();
        renderer.material.shader = Shader.Find("Custom/BlendVertexTransparentColor");

        renderer = resultingPiece.renderData.GetComponentInChildren<Renderer>();
        renderer.material.shader = Shader.Find("Custom/BlendVertexTransparentColor");

        normalAlpha = 0f;
        direction = 1;
        startCrossFade = false;
        crossFading = false;

        var normalRenderer = normalPiece.renderData.GetComponentInChildren<Renderer>();
        normalRenderer.material.color = new Color(normalRenderer.material.color.r, normalRenderer.material.color.g, normalRenderer.material.color.b, 0);
        normalPiece.renderData.transform.localScale = new Vector3(0.5f * 0.15f, 0.5f * 0.15f, 1);
        normalPiece.renderData.transform.parent = normalPiece.transform;

        var resultingRenderer = resultingPiece.renderData.GetComponentInChildren<Renderer>();
        resultingRenderer.material.color = new Color(resultingRenderer.material.color.r, resultingRenderer.material.color.g, resultingRenderer.material.color.b, 0);
        resultingPiece.renderData.transform.localScale = new Vector3(0.5f * 0.15f, 0.5f * 0.15f, 1);
        resultingPiece.renderData.transform.parent = resultingPiece.transform;

        gameObject.SetLayerRecursively(5);
    }

    // Update is called once per frame
    void Update() {
        if (startCrossFade && !crossFading) {
            normalAlpha += Time.deltaTime * direction * 0.5f;
            if (normalAlpha > 1) {
                normalAlpha = 1;
                direction *= -1;
                crossFading = true;
            }

            var normalRenderer = normalPiece.renderData.GetComponentInChildren<Renderer>();
            normalRenderer.material.color = new Color(normalRenderer.material.color.r, normalRenderer.material.color.g, normalRenderer.material.color.b, normalAlpha);

            var resultingRenderer = resultingPiece.renderData.GetComponentInChildren<Renderer>();
            resultingRenderer.material.color = new Color(resultingRenderer.material.color.r, resultingRenderer.material.color.g, resultingRenderer.material.color.b, 0);
        }

        if (crossFading) {
            normalAlpha += Time.deltaTime * direction * 0.5f;
            if (normalAlpha > 1) {
                normalAlpha = 1;
                direction *= -1;
            } else if (normalAlpha < 0) {
                normalAlpha = 0;
                direction *= -1;
            }

            var normalRenderer = normalPiece.renderData.GetComponentInChildren<Renderer>();
            normalRenderer.material.color = new Color(normalRenderer.material.color.r, normalRenderer.material.color.g, normalRenderer.material.color.b, normalAlpha);

            var resultingRenderer = resultingPiece.renderData.GetComponentInChildren<Renderer>();
            resultingRenderer.material.color = new Color(resultingRenderer.material.color.r, resultingRenderer.material.color.g, resultingRenderer.material.color.b, 1 - normalAlpha);
        }
    }

    public void Refresh() {
        for (int i = 0; i < PreviewPiece.MAX_WIDTH; i++) {
            for (int j = 0; j < PreviewPiece.MAX_HEIGHT; j++) {
                normalPiece.ClearTile(i, j, 0);
            }
        }

        for (int i = GameControl.Instance.LastShift.x; i < PreviewPiece.MAX_WIDTH; i++) {
            for (int j = GameControl.Instance.LastShift.y; j < PreviewPiece.MAX_HEIGHT; j++) {
                var ii = i - GameControl.Instance.LastShift.x;
                var jj = j - GameControl.Instance.LastShift.y;
                if (normalLayout.Layout[ii, jj] > 0) {
                    normalPiece.SetTile(i, j, 0, normalLayout.Layout[ii, jj] - 1 + normalLayout.Variant * Block.MAX_VALUE);
                }
            }
        }

        normalPiece.Build();

        for (int i = 0; i < PreviewPiece.MAX_WIDTH; i++) {
            for (int j = 0; j < PreviewPiece.MAX_HEIGHT; j++) {
                resultingPiece.ClearTile(i, j, 0);
                if (resultingLayout.Layout[i, j] > 0) {
                    resultingPiece.SetTile(i, j, 0, resultingLayout.Layout[i, j] - 1 + resultingLayout.Variant * Block.MAX_VALUE);
                }
            }
        }

        resultingPiece.Build();

        normalAlpha = 1;

        gameObject.SetLayerRecursively(5);
    }
}