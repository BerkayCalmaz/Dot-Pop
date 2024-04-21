using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.UI;

public enum GameState {
    wait,
    move
}

public class Board : MonoBehaviour {

    public GameState gameState = GameState.move;

    public int width;
    public int height;
    public int offset;
    public GameObject tilePrefab;
    public GameObject[] dots;
    private TileBackground[,] tiles;
    public GameObject[,] allDots;
    // Start is called before the first frame update  
    void Start()
    {
        //Create a 2D array of tiles with given w/h
        tiles = new TileBackground[width, height];
        allDots = new GameObject[width, height];
        SetBoard();
    }

    private void SetBoard()
    {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                Vector2 bgPos = new Vector2(i, j);
                Vector2 dotPos = new Vector2(i, j + offset);
                GameObject backgroundTile = Instantiate(tilePrefab, bgPos, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "(" + i + "," + j + ")";

                int dotToUse = Random.Range(0, dots.Length);
                int retryCount = 0;
                while (retryCount < 50 && MatchesAt(i, j, dots[dotToUse])) {
                    dotToUse = Random.Range(0, dots.Length);
                    retryCount += 1;
                }

                GameObject dot = Instantiate(dots[dotToUse], dotPos, Quaternion.identity);
                dot.GetComponent<Dot>().row = j;
                dot.GetComponent<Dot>().col = i;

                dot.transform.parent = this.transform;
                dot.transform.name = "dot: " + backgroundTile.name;
                allDots[i, j] = dot;
            }
        }
    }

    private bool MatchesAt(int col, int row, GameObject piece)
    {
        Dot dotPiece = piece.GetComponent<Dot>();
        if (col > 1) {
            //if the pieces to my left (already generated) are both of the same type as me then ...
            if (dotPiece.CompareTag(allDots[col - 1, row].GetComponent<Dot>().tag) && dotPiece.CompareTag(allDots[col - 2, row].GetComponent<Dot>().tag)) {
                return true;
            }
        }

        if (row > 1) {
            //if the pieces below me (already generated) are both of the same type as me then ...
            if (dotPiece.CompareTag(allDots[col, row - 1].GetComponent<Dot>().tag) && dotPiece.CompareTag(allDots[col, row - 2].GetComponent<Dot>().tag)) {
                return true;
            }
        }
        return false;
    }

    private void DestroyMatchesAt(int col, int row)
    {
        if (allDots[col, row].GetComponent<Dot>().isMatched) {
            Destroy(allDots[col, row]);
            allDots[col, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (!allDots[i, j]) {
                    continue;
                }
                DestroyMatchesAt(i, j);
            }
        }
        StartCoroutine(DecreaseRowCo());
    }

    private IEnumerator DecreaseRowCo()
    {
        int nullCounter = 0;
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (!allDots[i, j]) {
                    nullCounter += 1;
                }
                else {
                    allDots[i, j].GetComponent<Dot>().prevRow = allDots[i, j].GetComponent<Dot>().row;
                    allDots[i, j].GetComponent<Dot>().prevCol = allDots[i, j].GetComponent<Dot>().col;

                    if (nullCounter > 0) {
                        allDots[i, j].GetComponent<Dot>().row -= nullCounter;
                        allDots[i, j] = null;
                    }
                }
            }
            nullCounter = 0;
        }
        yield return new WaitForSeconds(.4f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard()
    {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (allDots[i, j]) {
                    continue;
                }
                Vector2 tempPos = new Vector2(i, j + offset);
                int dotToUse = Random.Range(0, dots.Length);
                GameObject piece = Instantiate(dots[dotToUse], tempPos, Quaternion.identity);
                piece.GetComponent<Dot>().row = j;
                piece.GetComponent<Dot>().col = i;

                allDots[i, j] = piece;
                piece.transform.parent = this.transform;
                piece.name = "( " + i + ", " + j + " )";
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (!allDots[i, j]) {
                    continue;
                }
                if (allDots[i, j].GetComponent<Dot>().isMatched) {
                    return true;
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(0.4f);
        while (MatchesOnBoard()) {
            yield return new WaitForSeconds(0.4f);
            DestroyMatches();
        }
        yield return new WaitForSeconds(0.4f);
        gameState = GameState.move;
    }
}
