using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {

	public enum Type {
		Red = 0,
		Blue,
		Green,
		Yellow,

		Max
	}

	private static readonly Dictionary<Type, Color> TYPE_COLOR_TABLE = new Dictionary<Type, Color>() {
		{ Type.Red, Color.red },
		{ Type.Blue, Color.blue },
		{ Type.Green, Color.green },
		{ Type.Yellow, Color.yellow },
	};

	private SpriteRenderer spriteRenderer;

	private int x;
	private int y;
	public Type type { get; private set; }

	void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer> ();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Initialize(int x, int y) {
		UpdateIndex (x, y);

		type = (Type)Random.Range (0, (int)Type.Max);
		spriteRenderer.color = TYPE_COLOR_TABLE[type];
	}

	public void UpdateIndex(int x, int y) {
		this.x = x;
		this.y = y;
	}

	public void OnPointerDown() {
		if (PuzzleController.instance.isSelectedBlock)
			return;

		PuzzleController.instance.SetSelectBlock ();

		Debug.Log (x.ToString () + ":" + y.ToString ());
	}

	public void OnPointerUp() {
		if (!PuzzleController.instance.isSelectedBlock)
			return;

		PuzzleController.instance.ReleaseSelectBlock ();
	}

	public void OnPointerExit() {
		if (!PuzzleController.instance.isSelectedBlock)
			return;

		var pos = transform.position;
		var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		var vec = pos - mousePos;
		var absVec = new Vector2 (Mathf.Abs (vec.x), Mathf.Abs (vec.y));

		int nextX = x;
		int nextY = y;

		if (absVec.x > absVec.y) {
			// 横方向
			if (vec.x > 0) {
				// 左
				--nextX;
			} else {
				// 右
				++nextX;
			}
		} else {
			// 縦方向
			if (vec.y < 0) {
				// 上
				--nextY;
			} else {
				// 下
				++nextY;
			}
		}

		Debug.Log (x.ToString () + ":" + y.ToString ());
		PuzzleController.instance.ChangeBlock (x, y, nextX, nextY);
		PuzzleController.instance.ReleaseSelectBlock ();
	}
}
