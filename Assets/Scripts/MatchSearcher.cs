using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSearcher : MonoBehaviour
{
    private Board board;
    public List<GameObject> currentMatches;
    void Start()
    {
        board = FindObjectOfType<Board>();
        currentMatches = new List<GameObject>();
    }
    
    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }
    private IEnumerator FindAllMatchesCo()
    {
        yield return new WaitForSeconds(.2f);
        for (int col = 0; col < board.width; col++) {
            for (int row = 0; row < board.height; row++) {
                _CheckHorizontalMatches(col, row);
                _CheckVerticalMatches(col, row);
            }
        }
    }

    private void _CheckHorizontalMatches(int col, int row)
    {
        GameObject currentDot = board.allDots[col, row];
        if (!currentDot || col <= 0 || col >= board.width - 1) {
            return;
        }
        GameObject leftDot = board.allDots[col- 1, row];
        GameObject rightDot = board.allDots[col + 1, row];
        if (!leftDot || !rightDot) {
            return;
        }
        // If there isn't a match, continue:
        if (!leftDot.CompareTag(currentDot.tag) || !rightDot.CompareTag(currentDot.tag)) {
            return;
        }
        MarkAsMatched(leftDot);
        MarkAsMatched(rightDot);
        MarkAsMatched(currentDot);
        }

    private void _CheckVerticalMatches(int col, int row)
    {
        GameObject currentDot = board.allDots[col, row];
        if (!currentDot || row <= 0 || row >= board.height- 1) {
            return;
        }
        GameObject upDot = board.allDots[col, row + 1];
        GameObject downDot = board.allDots[col, row - 1];
        if (!upDot|| !downDot) {
            return;
        }
        // If there isn't a match, continue:
        if (!upDot.CompareTag(currentDot.tag) || !downDot.CompareTag(currentDot.tag)) {
            return;
        }
        MarkAsMatched(upDot);
        MarkAsMatched(downDot);
        MarkAsMatched(currentDot);
    }
    private void MarkAsMatched(GameObject dot)
    {
        dot.GetComponent<Dot>().isMatched = true;
        if(!currentMatches.Contains(dot)) {
            currentMatches.Add(dot);
        }
    }
}
