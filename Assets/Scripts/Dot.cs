using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board variables.")]
    public Vector2 firstTouchPos;
    public Vector2 lastTouchPos;
    public int row;
    public int col;
    public int prevRow;
    public int prevCol;
    public MatchSearcher matchSearcher;

    private GameObject otherDot;
    private Board board;
    private Vector2 tmpPos;

    public bool isMatched = false;

    public float swapAngle = 0;
    public float minSwapDistance = .5f;
    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        matchSearcher = FindObjectOfType<MatchSearcher>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isMatched) {
            //Change the color of the sprite when matched
            SpriteRenderer selfSprite = GetComponent<SpriteRenderer>();
            selfSprite.color = new Color(1f, 1f, 1f, 0.1f);
        }

        //TODO Idea: we can make the targets shine before we leave the mouse? 
        //TODO: this part should be split into appropriate functions
        
        if (Mathf.Abs(row - transform.position.y) > .1) {
            //Start movement towards target
            tmpPos = new Vector2(transform.position.x, row);
            transform.position = Vector2.Lerp(transform.position, tmpPos, 0.6f);
            if (board.allDots[col, row] != this.gameObject) {
                board.allDots[col, row] = this.gameObject;
            }
            //Start match search
            matchSearcher.FindAllMatches();
        }
        else {
            //End movement
            tmpPos = new Vector2(transform.position.x, row);
            transform.position = tmpPos;
        }

        if (Mathf.Abs(col - transform.position.x) > .1) {
            //Start horizontal movement
            tmpPos = new Vector2(col, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tmpPos, 0.6f);
            if (board.allDots[col, row] != this.gameObject) {
                board.allDots[col, row] = this.gameObject;
            }
            matchSearcher.FindAllMatches();

        }
        else {
            //End movement
            tmpPos = new Vector2(col, transform.position.y);
            transform.position = tmpPos;
        }

    }

    private void OnMouseDown()
    {
        if (board.gameState != GameState.move) {
            return;
        }

        // Since the regular mouse position gives pixel coordinates, we change them to world points
        firstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (board.gameState != GameState.move) {
            return;
        }
        lastTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isValidSwipe;
        (isValidSwipe, swapAngle) = CalculateAngle();
        if (!isValidSwipe) {
            board.gameState = GameState.move;
            return;
        }
        board.gameState = GameState.wait;
        SwapPieces();
        //Sometimes there are impossible matches and we check it in the coroutine.
        StartCoroutine(CheckMoveCo());
    }

    private (bool isValidSwipe, float angle) CalculateAngle()
    {
        Vector2 swapVector = new Vector2(lastTouchPos.x - firstTouchPos.x, lastTouchPos.y - firstTouchPos.y);
        if(swapVector.magnitude < minSwapDistance) {
            return (false, 0);
        }
        board.gameState = GameState.wait;
        // Arctan returns radians so we multiply it by 180/pi to convert it to degrees. 
        // Unity handles the undefined results such as arctan90 etc.
        return (true, Mathf.Atan2(lastTouchPos.y - firstTouchPos.y, lastTouchPos.x - firstTouchPos.x) * 180 / Mathf.PI);
    }

    public IEnumerator CheckMoveCo()
    {
        yield return new WaitForSeconds(.3f);
        if (!otherDot) {
            yield break;
        }

        if(!isMatched && !otherDot.GetComponent<Dot>().isMatched) {
            otherDot.GetComponent<Dot>().col = col;
            otherDot.GetComponent<Dot>().row = row;
            col = prevCol;
            row = prevRow;

            yield return new WaitForSeconds(.4f);
            board.gameState = GameState.move;
        }
        else {
            board.DestroyMatches();
        }
        otherDot = null;
    }

    void SwapPieces()
    {
        if (swapAngle < 45 && swapAngle >= -45 && col < (board.width -1)){
            //Right swap
            otherDot = board.allDots[col + 1, row];
            otherDot.GetComponent<Dot>().col -= 1;
               
            //We dont need to set the other dot's prevrow/col since checkmoveCo sets the otherdots row to this row/col.
            prevCol = col;
            prevRow = row;
            col += 1;
        } else if (swapAngle >= 45 && swapAngle < 135 && row < (board.height - 1)){
            //Up swap
            otherDot = board.allDots[col, row + 1];
            otherDot.GetComponent<Dot>().row -= 1;
            prevCol = col;
            prevRow = row;

            row += 1;
        } else if ((swapAngle >= 135 || swapAngle < -135) && col > 0){
            //Left swap
            otherDot = board.allDots[col - 1, row];
            otherDot.GetComponent<Dot>().col += 1;
            prevCol = col;
            prevRow = row;

            col -= 1;
        } else if ((swapAngle < -45 && swapAngle >= -135) && row > 0){
            //Down swap
            otherDot = board.allDots[col, row - 1];
            otherDot.GetComponent<Dot>().row += 1;
            prevCol = col;
            prevRow = row;
            row -= 1;
        }
    }

}
