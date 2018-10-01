using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleController : MonoBehaviour {

	private static readonly int WIDTH = 8;
	private static readonly int HEIGHT = 8;
	private static readonly int MIN_MATCH_NUM = 3;

	private static readonly float CHANGE_BLOCK_DURATION = 0.2f;

	[SerializeField] private GameObject blockPrefab;

	public static PuzzleController instance { get; private set; }

	public bool isSelectedBlock { get; private set; }
	private bool isAnimation;
	private bool isMatch;

	private Block[,] blockMap;
	private bool[,] matchMap;

	void Awake() {
		instance = this;

		isSelectedBlock = false;
		isAnimation = false;
		isMatch = false;

		blockMap = new Block[WIDTH, HEIGHT];
		matchMap = new bool[WIDTH, HEIGHT];
	}

	// Use this for initialization
	void Start () {
		Initialize ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void Initialize() {
		for (int y = 0; y < HEIGHT; ++y) {
			for (int x = 0; x < WIDTH; ++x) {
				InstantiateBlock (x, y);
			}
		}

		ResetMatchMap ();
		CheckMatchAll ();
	}

	public void SetSelectBlock() {
		isSelectedBlock = true;
	}

	public void ReleaseSelectBlock() {
		isSelectedBlock = false;
	}

	public void ChangeBlock(int prevX, int prevY, int nextX, int nextY) {
		if (nextX < 0 || nextX >= WIDTH || nextY < 0 || nextY >= HEIGHT)
			return;
			
		if (isAnimation)
			return;

		StartCoroutine (ChangeBlockAnimation (prevX, prevY, nextX, nextY));
	}

	private IEnumerator ChangeBlockAnimation(int prevX, int prevY, int nextX, int nextY, bool isCheckMatch = true) {
		isAnimation = true;

		var prevBlock = blockMap [prevX, prevY];
		var nextBlock = blockMap [nextX, nextY];

		var prevPos = prevBlock.transform.position;
		var nextPos = nextBlock.transform.position;

		var prevVec = nextPos - prevPos;
		var nextVec = prevPos - nextPos;

		float time = Time.time;
		while(Time.time - time < CHANGE_BLOCK_DURATION) {
			float ratio = (Time.time - time) / CHANGE_BLOCK_DURATION;

			prevBlock.transform.position = prevPos + prevVec * ratio;
			nextBlock.transform.position = nextPos + nextVec * ratio;

			yield return null;
		}

		prevBlock.transform.position = nextPos;
		nextBlock.transform.position = prevPos;

		blockMap [prevX, prevY] = nextBlock;
		blockMap [nextX, nextY] = prevBlock;
		blockMap [prevX, prevY].UpdateIndex (prevX, prevY);
		blockMap [nextX, nextY].UpdateIndex (nextX, nextY);

		if (isCheckMatch) {
			if (CheckMatchAll ()) {
				isAnimation = false;
			} else {
				yield return StartCoroutine (ChangeBlockAnimation (nextX, nextY, prevX, prevY, false));
				isAnimation = false;
			}
		} else {
			isAnimation = false;
		}
	}

	void ResetMatchMap() {
		for (int i = 0; i < matchMap.GetLength(0); ++i) {
			for (int j = 0; j < matchMap.GetLength(1); ++j) {
				matchMap [i, j] = false;
			}
		}
	}

	void InstantiateBlock(int x, int y) {
		var obj = Instantiate (blockPrefab, transform, false);
		var pos = obj.transform.position;
		pos.x = x;
		pos.y = -y;
		obj.transform.position = pos;
		var block = obj.GetComponent<Block> ();
		block.Initialize (x, y);
		blockMap [x, y] = block;
	}

	bool CheckMatchAll() {
		// 縦方向走査
		for (int y = MIN_MATCH_NUM  - 1; y < HEIGHT; y += MIN_MATCH_NUM) {
			for (int x = 0; x < WIDTH; ++x) {
				CheckMatchHeight (x, y);
			}
		}

		// 横方向走査
		for (int x = MIN_MATCH_NUM  - 1; x < WIDTH; x += MIN_MATCH_NUM) {
			for (int y = 0; y < HEIGHT; ++y) {
				CheckMatchWidth (x, y);
			}
		}

		if (isMatch) {
			isMatch = false;
			for (int i = 0; i < matchMap.GetLength(0); ++i) {
				for (int j = 0; j < matchMap.GetLength(1); ++j) {
					if (!matchMap [i, j])
						continue;

					Destroy (blockMap [i, j].gameObject);
					InstantiateBlock (i, j);
				}
			}

			ResetMatchMap ();
			CheckMatchAll ();

			return true;
		}

		return false;
	}

	void CheckMatchHeight(int x, int y) {
		var type = blockMap [x, y].type;

		int min = y;
		int max = y;

		// 上方向
		int count = 1;
		for (int i = y - 1; i >= 0; --i) {
			if (blockMap[x, i].type != type) {
				break;
			}

			++count;
			min = i;
		}

		// 下方向
		for (int i = y + 1; i < HEIGHT; ++i) {
			if (blockMap[x, i].type != type) {
				break;
			}

			++count;
			max = i;
		}

		// マッチ数が足りない
		if (count < MIN_MATCH_NUM) {
			return;
		}

		for (int i = min; i <= max; ++i) {
			matchMap [x, i] = true;
		}

		isMatch = true;
	}

	void CheckMatchWidth(int x, int y) {
		var type = blockMap [x, y].type;

		int min = x;
		int max = x;

		// 左方向
		int count = 1;
		for (int i = x - 1; i >= 0; --i) {
			if (blockMap[i, y].type != type) {
				break;
			}

			++count;
			min = i;
		}

		// 下方向
		for (int i = x + 1; i < WIDTH; ++i) {
			if (blockMap[i, y].type != type) {
				break;
			}

			++count;
			max = i;
		}

		// マッチ数が足りない
		if (count < MIN_MATCH_NUM) {
			return;
		}

		for (int i = min; i <= max; ++i) {
			matchMap [i, y] = true;
		}

		isMatch = true;
	}
}
