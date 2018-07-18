using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ObjPair = System.Collections.Generic.KeyValuePair<UnityEngine.GameObject, UnityEngine.GameObject>;
using Pairii = System.Collections.Generic.KeyValuePair<System.Int32, System.Int32>;
using PairTeamType = System.Collections.Generic.KeyValuePair<eTeam, eType>;

public class GamePlayManager : MonoBehaviour
{
    [SerializeField]
    private GameObject boardPrefab;
    private static eTeam currentTeam;
    private GameObject thisGame;
    private GameObject selected;
    public List<KeyValuePair<Pairii, Pairii>> Moves;
    public List<KeyValuePair<PairTeamType, PairTeamType>> Save;
    private GameObject prev;
    public static bool Mode; // true = PvP, false = PvCom
    bool AI_turn = false;
    public Button EndGame;
    public Text timeText;
    float second;
    float t;
    float min;
    bool isEnd;
    public static int[] PieceCounts;
    public static int moves;
    private void Start()
    {
        Debug.Log(AI_Script.Depth);
        isEnd = false;
        timeText.text = "00:00"; 
        thisGame = Instantiate(boardPrefab, transform.position, Quaternion.identity);
        currentTeam = eTeam.WHITE;
        Moves = new List<KeyValuePair<Pairii, Pairii>>();
        Save = new List<KeyValuePair<PairTeamType, PairTeamType>>();
        second = 0;
        t = 0;
        min = 0;
        PieceCounts = new int[12] { 8, 2, 2, 2, 1, 1, 8, 2, 2, 2, 1, 1 };
        TextCountInit();
        moves = 0;
    }

    void TextCountInit()
    {
        wPawn.text =    PieceCounts[0].ToString();
        wRook.text =    PieceCounts[1].ToString();
        wKnight.text =  PieceCounts[2].ToString();
        wBishop.text =  PieceCounts[3].ToString();
        wKing.text =    PieceCounts[4].ToString();
        wQueen.text =   PieceCounts[5].ToString();
        bPawn.text =    PieceCounts[6].ToString();
        bRook.text =    PieceCounts[7].ToString();
        bKnight.text =  PieceCounts[8].ToString();
        bBishop.text =  PieceCounts[9].ToString();
        bKing.text =    PieceCounts[10].ToString();
        bQueen.text =   PieceCounts[11].ToString();
    }

    private void Update()
    {
        #region Timer
        second += Time.deltaTime;
        if (second >= t + 1 && isEnd == false)
        {
            if (second >= 60)
            {
                second = 0;
                min += 1;
                t = 0;
                if (min < 10)
                    timeText.text = "0" + min.ToString() + ":" + "0" + ((int)second).ToString();
                else
                    timeText.text = min.ToString() + ":" + "0" + ((int)second).ToString();
            }
            else
            {
                if (second < 10)
                {
                    t += 1; ;
                    if (min < 10)
                        timeText.text = "0" + min.ToString() + ":" + "0" + ((int)second).ToString();
                    else
                        timeText.text = min.ToString() + ":" + "0" + ((int)second).ToString();
                }
                else
                {
                    t += 1; ;
                    if (min < 10)
                        timeText.text = "0" + min.ToString() + ":" + ((int)second).ToString();
                    else
                        timeText.text = min.ToString() + ":" + ((int)second).ToString();
                }
            }
        }
        #endregion

        #region Clicking
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && isEnd == false)
            {
                if (hit.collider.gameObject.GetComponent<Cell>().State == eState.FREE && selected != null)
                {
                    thisGame.GetComponent<Board>().HidePath(selected);
                    selected = null;
                }
                if (currentTeam == hit.collider.gameObject.GetComponent<Cell>().Team
                    || hit.collider.gameObject.GetComponent<Cell>().State == eState.TARGET
                    || hit.collider.gameObject.GetComponent<Cell>().State == eState.KILLABLE)
                {
                    if (hit.collider.gameObject.GetComponent<Cell>().State == eState.SELECTABLE)
                    {
                        if (selected == null) // CHUA BAM CAI GI
                        {
                            selected = hit.collider.gameObject;
                            thisGame.GetComponent<Board>().SetSelectedColor(selected, true);
                            thisGame.GetComponent<Board>().ShowPath(selected);
                        }
                        else // DA BAM ROI, GIO BAM CAI KHAC
                        {
                            thisGame.GetComponent<Board>().HidePath(selected);
                            selected = hit.collider.gameObject;
                            thisGame.GetComponent<Board>().SetSelectedColor(selected, true);
                            thisGame.GetComponent<Board>().ShowPath(selected);
                        }
                    }
                    else
                    {
                        if (hit.collider.gameObject.GetComponent<Cell>().State == eState.TARGET
                     || hit.collider.gameObject.GetComponent<Cell>().State == eState.KILLABLE)
                        {
                            GameState gameState = new GameState();
                            gameState = GetGameState(thisGame);
                            GameObject A = selected;
                            GameObject B = hit.collider.gameObject;

                            int m = B.GetComponent<Cell>().y;
                            int n = B.GetComponent<Cell>().x;
                            int y = A.GetComponent<Cell>().y;
                            int x = A.GetComponent<Cell>().x;
                            GameState nextState = AI_Script.ChangeState(gameState, y, x, m, n);
                            //Debug.Log(nextState.Cells[0][4].pType + " ... " + nextState.Cells[7][4].pType);
                            if (!IsChecked(nextState, currentTeam))
                            {
                                thisGame.GetComponent<Board>().HidePath(selected);
                                Moves.Add(new KeyValuePair<Pairii, Pairii>(new Pairii(y, x), new Pairii(m, n)));
                                Save.Add(new KeyValuePair<PairTeamType, PairTeamType>(
                                    new PairTeamType(A.GetComponent<Cell>().Team, A.GetComponent<Cell>().pType),
                                    new PairTeamType(B.GetComponent<Cell>().Team, B.GetComponent<Cell>().pType)));
                                CounterUpdate(B.GetComponent<Cell>().pType, B.GetComponent<Cell>().Team, 1);
                                thisGame.GetComponent<Board>().Move(A, B);
                                moves++;
                                gameState = new GameState();
                                gameState = GetGameState(thisGame);
                                if (Mode == true)
                                {
                                    if (IsCheckMated(gameState, NextTeam(currentTeam)))
                                    {
                                        EndGame.image.sprite = currentTeam == eTeam.BLACK ? S_BlackWin : S_WhiteWin;
                                        EndGame.gameObject.SetActive(true);
                                        isEnd = true;
                                    }
                                }
                                else
                                {
                                    if (IsCheckMated(gameState, NextTeam(currentTeam)))
                                    {
                                        EndGame.image.sprite = S_YouWin;
                                        EndGame.gameObject.SetActive(true);
                                        isEnd = true;
                                    }
                                }
                                prev = null;
                                AI_turn = true;
                                if (Mode == true)
                                    currentTeam = NextTeam(currentTeam);
                                //Debug.Log(AI_Script.NumOfLegalMoves(gameState, currentTeam));
                                selected = null;
                            }
                        }
                    }
                }
            }
        }
        #endregion 

        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && isEnd == false)
            {
                if (hit.collider.gameObject.GetComponent<Cell>().Team == currentTeam && (AI_turn == false || Mode == true))
                {
                    if (prev == null)
                    {
                        prev = hit.collider.gameObject;
                        prev.GetComponent<SpriteRenderer>().color = new Color32(40, 110, 200, 255);
                        foreach (GameObject go in thisGame.GetComponent<Board>().Path(prev))
                        {
                            if (Board.Enemy(go, prev))
                                Board.Highlight(go, "KillMask", true);
                            else
                                Board.Highlight(go, "CellMask", true);
                        }
                    }
                    else if (prev != hit.collider.gameObject && selected == null)
                    {
                        prev.GetComponent<SpriteRenderer>().color = prev.GetComponent<Cell>().TeamColor(prev.GetComponent<Cell>().Team);
                        foreach (GameObject go in thisGame.GetComponent<Board>().Path(prev))
                        {
                            if (Board.Enemy(go, prev))
                                Board.Highlight(go, "KillMask", false);
                            else
                                Board.Highlight(go, "CellMask", false);
                        }
                        prev = hit.collider.gameObject;
                        prev.GetComponent<SpriteRenderer>().color = new Color32(40, 110, 200, 255);
                        foreach (GameObject go in thisGame.GetComponent<Board>().Path(prev))
                        {
                            if (Board.Enemy(go, prev))
                                Board.Highlight(go, "KillMask", true);
                            else
                                Board.Highlight(go, "CellMask", true);
                        }
                    }
                }
                else if (hit.collider.gameObject.GetComponent<Cell>().Team != currentTeam && selected == null)
                {

                    if (prev != null)
                    {
                        prev.GetComponent<SpriteRenderer>().color = prev.GetComponent<Cell>().TeamColor(prev.GetComponent<Cell>().Team);
                        foreach (GameObject go in thisGame.GetComponent<Board>().Path(prev))
                        {
                            if (Board.Enemy(go, prev))
                                Board.Highlight(go, "KillMask", false);
                            else
                                Board.Highlight(go, "CellMask", false);
                        }
                        prev = null;
                    }
                }
            }
        }

        if (AI_turn == true && Mode == false && isEnd == false)
        {
            GameState gameState = new GameState();
            // A[y][x];
            gameState = GetGameState(thisGame);
            KeyValuePair<Pairii, Pairii> move = thisGame.GetComponent<AI_Script>().FindMove(gameState);
            GameObject A = thisGame.GetComponent<Board>().AllPieces[move.Key.Key][move.Key.Value];
            GameObject B = thisGame.GetComponent<Board>().AllPieces[move.Value.Key][move.Value.Value];
            Moves.Add(new KeyValuePair<Pairii, Pairii>(new Pairii(A.GetComponent<Cell>().y, A.GetComponent<Cell>().x),
                                                       new Pairii(B.GetComponent<Cell>().y, B.GetComponent<Cell>().x)));
            Save.Add(new KeyValuePair<PairTeamType, PairTeamType>(
                                new PairTeamType(A.GetComponent<Cell>().Team, A.GetComponent<Cell>().pType),
                                new PairTeamType(B.GetComponent<Cell>().Team, B.GetComponent<Cell>().pType)));

            CounterUpdate(B.GetComponent<Cell>().pType, B.GetComponent<Cell>().Team, 1);
            thisGame.GetComponent<Board>().Move(A, B);
            moves++;
            gameState = new GameState();
            gameState = GetGameState(thisGame);
            if (IsCheckMated(gameState, currentTeam))
            {
                EndGame.image.sprite = S_YouLose;
                EndGame.gameObject.SetActive(true);
                isEnd = true;
            }
            AI_turn = false;
        }
    }
    static eTeam NextTeam(eTeam team)
    {
        if (team == eTeam.WHITE)
            return eTeam.BLACK;
        return eTeam.WHITE;
    }

    private GameState GetGameState(GameObject board)
    {
        GameState gameState = new GameState();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                gameState.Pieces[i][j].State = board.GetComponent<Board>().AllPieces[i][j].GetComponent<Cell>().State;
                gameState.Pieces[i][j].pType = board.GetComponent<Board>().AllPieces[i][j].GetComponent<Cell>().pType;
                gameState.Pieces[i][j].Team = board.GetComponent<Board>().AllPieces[i][j].GetComponent<Cell>().Team;
            }
        }
        return gameState;
    }

    private void CounterUpdate(eType t, eTeam team, int undoFlag)
    {
        int hi = 0;
        if (t != eType.EMPTY)
        {
            switch (t)
            {
                case eType.PAWN:
                    if ((team == eTeam.WHITE))
                    {
                        hi = 0;
                        PieceCounts[hi] -= undoFlag;
                        wPawn.text = PieceCounts[hi].ToString();
                    }
                    else
                    {
                        hi = 6;
                        PieceCounts[hi] -= undoFlag;
                        bPawn.text = PieceCounts[hi].ToString();
                    }
                    break;
                case eType.ROOK:
                    if ((team == eTeam.WHITE))
                    {
                        hi = 1;
                        PieceCounts[hi] -= undoFlag;
                        wRook.text = PieceCounts[hi].ToString();
                    }
                    else
                    {
                        hi = 7;
                        PieceCounts[hi] -= undoFlag;
                        bRook.text = PieceCounts[hi].ToString();
                    }
                    break;
                case eType.KNIGHT:
                    if ((team == eTeam.WHITE))
                    {
                        hi = 2;
                        PieceCounts[hi] -= undoFlag;
                        wKnight.text = PieceCounts[hi].ToString();
                    }
                    else
                    {
                        hi = 8;
                        PieceCounts[hi] -= undoFlag;
                        bKnight.text = PieceCounts[hi].ToString();
                    }
                    break;
                case eType.BISHOP:
                    if ((team == eTeam.WHITE))
                    {
                        hi = 3;
                        PieceCounts[hi] -= undoFlag;
                        wBishop.text = PieceCounts[hi].ToString();
                    }
                    else
                    {
                        hi = 9;
                        PieceCounts[hi] -= undoFlag;
                        bBishop.text = PieceCounts[hi].ToString();
                    }
                    break;
                case eType.KING:
                    if ((team == eTeam.WHITE))
                    {
                        hi = 4;
                        PieceCounts[hi] -= undoFlag;
                        wKing.text = PieceCounts[hi].ToString();
                    }
                    else
                    {
                        hi = 10;
                        PieceCounts[hi] -= undoFlag;
                        bKing.text = PieceCounts[hi].ToString();
                    }
                    break;
                case eType.QUEEN:
                    if ((team == eTeam.WHITE))
                    {
                        hi = 5;
                        PieceCounts[hi] -= undoFlag;
                        wQueen.text = PieceCounts[hi].ToString();
                    }
                    else
                    {
                        hi = 11;
                        PieceCounts[hi] -= undoFlag;
                        bQueen.text = PieceCounts[hi].ToString();
                    }
                    break;
            }
        }
    }

    public static bool IsCheckMated(GameState game, eTeam team)
    {
        foreach (GameState gs in AI_Script.NextStates(game, team))
        {
            if (!IsChecked(gs, team))
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsChecked(GameState game, eTeam team)
    {
        // apply RookPath to King

        List<Pairii> KingOnRook;
        if (team == eTeam.WHITE)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (game.Pieces[i][j].Team == NextTeam(team) && AI_Script.Path(game.Pieces[i][j], game, i, j).Count > 0)
                    {
                        for (int k = 0; k < AI_Script.Path(game.Pieces[i][j], game, i, j).Count; k++)
                        {
                            int y = AI_Script.Path(game.Pieces[i][j], game, i, j)[k].Key;
                            int x = AI_Script.Path(game.Pieces[i][j], game, i, j)[k].Value;
                            if (game.Pieces[y][x].pType == eType.KING)
                            {
                                Debug.Log("CHECKED");
                                return true;
                            }
                        }

                    }
                }
            }
        }
        else
        {
            for (int i = 7; i >= 0; i--)
            {
                for (int j = 7; j >= 0; j--)
                {
                    if (game.Pieces[i][j].Team == NextTeam(team) && AI_Script.Path(game.Pieces[i][j], game, i, j).Count > 0)
                    {
                        for (int k = AI_Script.Path(game.Pieces[i][j], game, i, j).Count - 1; k >= 0; k--)
                        {
                            int y = AI_Script.Path(game.Pieces[i][j], game, i, j)[k].Key;
                            int x = AI_Script.Path(game.Pieces[i][j], game, i, j)[k].Value;
                            if (game.Pieces[y][x].pType == eType.KING)
                            {
                                Debug.Log("CHECKED");
                                return true;
                            }
                        }

                    }
                }
            }
        }
        return false;
    }

    #region Buttons

    public void MainMenu()
    {
        SceneManager.LoadScene("GAMEMANAGER");
    }

    public void NewGame()
    {
        Destroy(thisGame);
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Destroy(thisGame.GetComponent<Board>().AllPieces[i][j]);
            }
        }
        selected = null;
        prev = null;
        Start();
    }

    public void UndoMove()
    {
        if (selected != null)
        {
            thisGame.GetComponent<Board>().HidePath(selected);
            selected = null;
        }
        if (prev != null)
        {
            prev.GetComponent<SpriteRenderer>().color = prev.GetComponent<Cell>().TeamColor(prev.GetComponent<Cell>().Team);
            foreach (GameObject go in thisGame.GetComponent<Board>().Path(prev))
            {
                if (Board.Enemy(go, prev))
                    Board.Highlight(go, "KillMask", false);
                else
                    Board.Highlight(go, "CellMask", false);
            }
            prev = null;
        }
        if (Mode == false)
        {
            if (Save.Count != 0)
            {
                moves -= 2;
                Pairii goMay = new Pairii(Moves[Moves.Count - 1].Key.Key, Moves[Moves.Count - 1].Key.Value);
                Pairii toMay = new Pairii(Moves[Moves.Count - 1].Value.Key, Moves[Moves.Count - 1].Value.Value);
                GameObject A = thisGame.GetComponent<Board>().AllPieces[toMay.Key][toMay.Value];
                GameObject B = thisGame.GetComponent<Board>().AllPieces[goMay.Key][goMay.Value];
                thisGame.GetComponent<Board>().Move(A, B);
                if (B.GetComponent<Cell>().pType == eType.KING)
                {
                    int y = B.GetComponent<Cell>().y;
                    if (B.GetComponent<Cell>().x == 4 && A.GetComponent<Cell>().x == 6) // nhập thành phải
                    {
                        GameObject RA = thisGame.GetComponent<Board>().AllPieces[y][5];
                        GameObject RB = thisGame.GetComponent<Board>().AllPieces[y][7];
                        thisGame.GetComponent<Board>().Move(RA, RB);
                        RB.GetComponent<Cell>().didMove = false;
                    }
                    else if (B.GetComponent<Cell>().x == 4 && A.GetComponent<Cell>().x == 2) // nhap thanh trai
                    {
                        GameObject RA = thisGame.GetComponent<Board>().AllPieces[y][3];
                        GameObject RB = thisGame.GetComponent<Board>().AllPieces[y][0];
                        thisGame.GetComponent<Board>().Move(RA, RB);
                        RB.GetComponent<Cell>().didMove = false;
                    }
                }
                A.GetComponent<Cell>().Team = Save[Save.Count - 1].Value.Key;
                A.GetComponent<Cell>().pType = Save[Save.Count - 1].Value.Value;
                A.GetComponent<Cell>().State = eState.SELECTABLE;

                B.GetComponent<Cell>().Team = Save[Save.Count - 1].Key.Key;
                B.GetComponent<Cell>().pType = Save[Save.Count - 1].Key.Value;
                B.GetComponent<Cell>().State = eState.SELECTABLE;
                CounterUpdate(Save[Save.Count - 1].Value.Value, Save[Save.Count - 1].Value.Key, -1);
                B.GetComponent<Cell>().didMove = false;
                Save.RemoveAt(Save.Count - 1);
                Moves.RemoveAt(Moves.Count - 1);

                goMay = new Pairii(Moves[Moves.Count - 1].Key.Key, Moves[Moves.Count - 1].Key.Value);
                toMay = new Pairii(Moves[Moves.Count - 1].Value.Key, Moves[Moves.Count - 1].Value.Value);

                A = thisGame.GetComponent<Board>().AllPieces[toMay.Key][toMay.Value];
                B = thisGame.GetComponent<Board>().AllPieces[goMay.Key][goMay.Value];
                thisGame.GetComponent<Board>().Move(A, B);
                if (B.GetComponent<Cell>().pType == eType.KING)
                {
                    int y = B.GetComponent<Cell>().y;
                    if (B.GetComponent<Cell>().x == 4 && A.GetComponent<Cell>().x == 6) // nhập thành phải
                    {
                        GameObject RA = thisGame.GetComponent<Board>().AllPieces[y][5];
                        GameObject RB = thisGame.GetComponent<Board>().AllPieces[y][7];
                        thisGame.GetComponent<Board>().Move(RA, RB);
                        RB.GetComponent<Cell>().didMove = false;
                    }
                    else if (B.GetComponent<Cell>().x == 4 && A.GetComponent<Cell>().x == 2) // nhap thanh trai
                    {
                        GameObject RA = thisGame.GetComponent<Board>().AllPieces[y][3];
                        GameObject RB = thisGame.GetComponent<Board>().AllPieces[y][0];
                        thisGame.GetComponent<Board>().Move(RA, RB);
                        RB.GetComponent<Cell>().didMove = false;
                    }
                }
                A.GetComponent<Cell>().Team = Save[Save.Count - 1].Value.Key;
                A.GetComponent<Cell>().pType = Save[Save.Count - 1].Value.Value;
                A.GetComponent<Cell>().State = eState.SELECTABLE;

                B.GetComponent<Cell>().Team = Save[Save.Count - 1].Key.Key;
                B.GetComponent<Cell>().pType = Save[Save.Count - 1].Key.Value;
                B.GetComponent<Cell>().State = eState.SELECTABLE;
                CounterUpdate(Save[Save.Count - 1].Value.Value, Save[Save.Count - 1].Value.Key, -1);
                B.GetComponent<Cell>().didMove = false;
                Save.RemoveAt(Save.Count - 1);
                Moves.RemoveAt(Moves.Count - 1);
            }
        }
        else
        {
            if (Save.Count != 0)
            {
                moves -= 1;
                Pairii goMay = new Pairii(Moves[Moves.Count - 1].Key.Key, Moves[Moves.Count - 1].Key.Value);
                Pairii toMay = new Pairii(Moves[Moves.Count - 1].Value.Key, Moves[Moves.Count - 1].Value.Value);
                GameObject A = thisGame.GetComponent<Board>().AllPieces[toMay.Key][toMay.Value];
                GameObject B = thisGame.GetComponent<Board>().AllPieces[goMay.Key][goMay.Value];
                thisGame.GetComponent<Board>().Move(A, B);
                if (B.GetComponent<Cell>().pType == eType.KING)
                {
                    int y = B.GetComponent<Cell>().y;
                    if (B.GetComponent<Cell>().x == 4 && A.GetComponent<Cell>().x == 6) // nhập thành phải
                    {
                        GameObject RA = thisGame.GetComponent<Board>().AllPieces[y][5];
                        GameObject RB = thisGame.GetComponent<Board>().AllPieces[y][7];
                        thisGame.GetComponent<Board>().Move(RA, RB);
                        RB.GetComponent<Cell>().didMove = false;
                    }
                    else if (B.GetComponent<Cell>().x == 4 && A.GetComponent<Cell>().x == 2) // nhap thanh trai
                    {
                        GameObject RA = thisGame.GetComponent<Board>().AllPieces[y][3];
                        GameObject RB = thisGame.GetComponent<Board>().AllPieces[y][0];
                        thisGame.GetComponent<Board>().Move(RA, RB);
                        RB.GetComponent<Cell>().didMove = false;
                    }
                }
                A.GetComponent<Cell>().Team = Save[Save.Count - 1].Value.Key;
                A.GetComponent<Cell>().pType = Save[Save.Count - 1].Value.Value;
                A.GetComponent<Cell>().State = eState.SELECTABLE;

                B.GetComponent<Cell>().Team = Save[Save.Count - 1].Key.Key;
                B.GetComponent<Cell>().pType = Save[Save.Count - 1].Key.Value;
                B.GetComponent<Cell>().State = eState.SELECTABLE;
                CounterUpdate(Save[Save.Count - 1].Value.Value, Save[Save.Count - 1].Value.Key, -1);
                B.GetComponent<Cell>().didMove = false;
                Save.RemoveAt(Save.Count - 1);
                Moves.RemoveAt(Moves.Count - 1);
                currentTeam = NextTeam(currentTeam);
            }
        }

    }

    #endregion
    private KeyValuePair<string, string> piecePos(GameObject go)
    {
        int xx = go.GetComponent<Cell>().x;
        int yy = go.GetComponent<Cell>().y;
        string first = Letters[xx];
        string second = (yy + 1).ToString();

        return new KeyValuePair<string, string>(first, second);
    }
    private string[] Letters = new string[8]
    {
    "A", "B", "C", "D", "E", "F", "G", "H"
    };
    [SerializeField]
    private Sprite S_WhiteWin;
    [SerializeField]
    private Sprite S_BlackWin;
    [SerializeField]
    private Sprite S_YouWin;
    [SerializeField]
    private Sprite S_YouLose;
    [SerializeField]
    private Text wPawn;
    [SerializeField]
    private Text wRook;
    [SerializeField]
    private Text wKnight;
    [SerializeField]
    private Text wBishop;
    [SerializeField]
    private Text wKing;
    [SerializeField]
    private Text wQueen;
    [SerializeField]
    private Text bPawn;
    [SerializeField]
    private Text bRook;
    [SerializeField]
    private Text bKing;
    [SerializeField]
    private Text bBishop;
    [SerializeField]
    private Text bKnight;
    [SerializeField]
    private Text bQueen;

}
