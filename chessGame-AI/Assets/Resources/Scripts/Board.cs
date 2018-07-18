using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System;

public class Board : MonoBehaviour
{
    [SerializeField]
    private GameObject cellPrefab;

    public GameObject[][] AllPieces;

    void Start()
    {
        this.GetComponent<SpriteRenderer>().enabled = true;
        Initiate();
        SettingUp();
    }

    public void SetNothing(GameObject go)
    {
        go.GetComponent<Cell>().State = eState.FREE;
        go.GetComponent<Cell>().Team = eTeam.MID;
        go.GetComponent<Cell>().pType = eType.EMPTY;
    }

    public void SetSelectedColor(GameObject go, bool e)
    {
        if (e)
        {
            go.GetComponent<SpriteRenderer>().color = new Color32(40, 110, 200, 255);
            Highlight(go, "FrameMask", true);
        }
        else
        {
            go.GetComponent<SpriteRenderer>().color = go.GetComponent<Cell>().TeamColor(go.GetComponent<Cell>().Team);
            Highlight(go, "FrameMask", false);
        }
    }
    GameObject a, b;
    public void Move2(GameObject go, GameObject to)
    {
        a = go;
        b = to;
        Show2Masks();
        to.GetComponent<Cell>().pType = go.GetComponent<Cell>().pType;
        to.GetComponent<Cell>().Team = go.GetComponent<Cell>().Team;
        to.GetComponent<Cell>().State = go.GetComponent<Cell>().State;
        go.GetComponent<Cell>().pType = eType.EMPTY;
        go.GetComponent<Cell>().State = eState.FREE;
        go.GetComponent<Cell>().Team = eTeam.MID;
        go.GetComponent<Cell>().didMove = true;
        if ((to.GetComponent<Cell>().y == 7 || to.GetComponent<Cell>().y == 0) && to.GetComponent<Cell>().pType == eType.PAWN)
            PawnToQueen(to);
        Invoke("Hide2Masks", 0.6f);
    }

    public void Move(GameObject go, GameObject to)
    {
        to.GetComponent<Cell>().pType = go.GetComponent<Cell>().pType;
        to.GetComponent<Cell>().Team = go.GetComponent<Cell>().Team;
        to.GetComponent<Cell>().State = go.GetComponent<Cell>().State;
        go.GetComponent<Cell>().pType = eType.EMPTY;
        go.GetComponent<Cell>().State = eState.FREE;
        go.GetComponent<Cell>().Team = eTeam.MID;
        to.GetComponent<Cell>().didMove = true;
        if ((to.GetComponent<Cell>().y == 7 || to.GetComponent<Cell>().y == 0) && to.GetComponent<Cell>().pType == eType.PAWN)
            PawnToQueen(to);
        if (to.GetComponent<Cell>().pType == eType.KING)
        {
            int xTo = to.GetComponent<Cell>().x;
            if (xTo == 2 || xTo == 6) // nhap thanh
            {
                if (xTo == 2) // nhap thanh trai
                    Move(AllPieces[to.GetComponent<Cell>().y][0], AllPieces[to.GetComponent<Cell>().y][3]);
                else // nhap thanh phai
                    Move(AllPieces[to.GetComponent<Cell>().y][7], AllPieces[to.GetComponent<Cell>().y][5]);
            }
        }
    }

    void Show2Masks()
    {
        Highlight(a, "YellowMask", true);
        Highlight(b, "YellowMask", true);
    }
    void Hide2Masks()
    {
        Highlight(a, "YellowMask", false);
        Highlight(b, "YellowMask", false);
    }

    public static void MyDelay(int seconds)
    {
        DateTime ts = DateTime.Now + TimeSpan.FromSeconds(seconds);
        do
        {
        } while (DateTime.Now < ts);
    }


    public static void Highlight(GameObject go, string str, bool on)
    {
        if (go != null)
        {
            go.transform.Find(str).gameObject.GetComponent<SpriteRenderer>().enabled = on;
        }
    }


    public void SelectSetup(GameObject go)
    {
        go.GetComponent<SpriteRenderer>().color = new Color32(40, 110, 200, 255);
        go.GetComponent<Cell>().State = eState.SELECTED;
    }
    #region KhoiTao
    public void Initiate()
    {
        AllPieces = new GameObject[8][];
        for (int y = 0; y < 8; y++)
        {
            AllPieces[y] = new GameObject[8];
            for (int x = 0; x < 8; x++)
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(
                    new Vector3(69 + (90 * x), 69 + 90 * y, Camera.main.nearClipPlane));
                pos.z = -1;
                GameObject go = Instantiate(cellPrefab, pos, Quaternion.identity);
                go.GetComponent<Cell>().State = eState.FREE;
                go.GetComponent<Cell>().pType = eType.EMPTY;
                go.GetComponent<Cell>().Team = eTeam.MID;
                go.GetComponent<Cell>().x = x;
                go.GetComponent<Cell>().y = y;
                go.GetComponent<Cell>().didMove = false;
                //go.transform.localScale = new Vector3(10 / 9, 10 / 9, 1);
                AllPieces[y][x] = go;
            }
        }
    }

    public void SettingUp()
    {
        for (int j = 0; j < 8; j++)
        {
            AllPieces[1][j].GetComponent<Cell>().Team = eTeam.WHITE;
            AllPieces[1][j].GetComponent<Cell>().pType = pawnRow[j];
            AllPieces[1][j].GetComponent<Cell>().State = eState.SELECTABLE;

            AllPieces[0][j].GetComponent<Cell>().Team = eTeam.WHITE;
            AllPieces[0][j].GetComponent<Cell>().pType = royaltyRow[j];
            AllPieces[0][j].GetComponent<Cell>().State = eState.SELECTABLE;

            AllPieces[6][j].GetComponent<Cell>().Team = eTeam.BLACK;
            AllPieces[6][j].GetComponent<Cell>().pType = pawnRow[j];
            AllPieces[6][j].GetComponent<Cell>().State = eState.SELECTABLE;

            AllPieces[7][j].GetComponent<Cell>().Team = eTeam.BLACK;
            AllPieces[7][j].GetComponent<Cell>().pType = royaltyRow[j];
            AllPieces[7][j].GetComponent<Cell>().State = eState.SELECTABLE;
        }
    }
    #endregion

    #region Đường đi
    public void ShowPath(GameObject go)
    {
        foreach (GameObject g in Path(go))
        {
            if (Enemy(g, go))
            {
                g.GetComponent<Cell>().prevState = g.GetComponent<Cell>().State;
                g.GetComponent<Cell>().State = eState.KILLABLE;
            }
            else
            {
                g.GetComponent<Cell>().prevState = g.GetComponent<Cell>().State;
                g.GetComponent<Cell>().State = eState.TARGET;
            }
        }
    }
    public void HidePath(GameObject go)
    {
        SetSelectedColor(go, false);
        foreach (GameObject g in Path(go))
        {
            g.GetComponent<Cell>().State = g.GetComponent<Cell>().prevState;
            g.GetComponent<Cell>().pType = g.GetComponent<Cell>().pType;
            Highlight(g, "CellMask", false);
            Highlight(g, "KillMask", false);
        }
    }

    public List<GameObject> Path(GameObject go)
    {
        List<GameObject> L = new List<GameObject>();
        switch (go.GetComponent<Cell>().pType)
        {
            case eType.PAWN:
                L = PawnPath(go);
                break;
            case eType.ROOK:
                L = RookPath(go);
                break;
            case eType.BISHOP:
                L = BishopPath(go);
                break;
            case eType.KNIGHT:
                L = KnightPath(go);
                break;
            case eType.QUEEN:
                L = QueenPath(go);
                break;
            case eType.KING:
                L = KingPath(go);
                break;
        }
        return L;
    }


    void PawnToQueen(GameObject go)
    {
        go.GetComponent<Cell>().pType = eType.QUEEN;
    }

    private bool OutOfBound(int y, int x)
    {
        if (y > 7 || y < 0 || x > 7 || x < 0)
        {
            return true;
        }
        return false;
    }

    #region ****************************************PIECEPATH****************************************
    private List<GameObject> PawnPath(GameObject go)
    {
        List<GameObject> L = new List<GameObject>();
        int xx = go.GetComponent<Cell>().x;
        int yy = go.GetComponent<Cell>().y;
        if ((yy == 1 && go.GetComponent<Cell>().Team == eTeam.WHITE) || (yy == 6 && go.GetComponent<Cell>().Team == eTeam.BLACK))
        {
            if (go.GetComponent<Cell>().Team == eTeam.WHITE && yy < 7)
            {
                if (AllPieces[yy + 1][xx].GetComponent<Cell>().pType == eType.EMPTY)
                    L.Add(AllPieces[yy + 1][xx]);

                if (yy < 6)
                {
                    if (AllPieces[yy + 2][xx].GetComponent<Cell>().pType == eType.EMPTY
                 && AllPieces[yy + 1][xx].GetComponent<Cell>().pType == eType.EMPTY)
                        L.Add(AllPieces[yy + 2][xx]);
                }
            }
            else if (go.GetComponent<Cell>().Team == eTeam.BLACK && yy > 0)
            {
                if (AllPieces[yy - 1][xx].GetComponent<Cell>().pType == eType.EMPTY)
                    L.Add(AllPieces[yy - 1][xx]);

                if (yy > 1)
                {
                    if (AllPieces[yy - 2][xx].GetComponent<Cell>().pType == eType.EMPTY
                 && AllPieces[yy - 1][xx].GetComponent<Cell>().pType == eType.EMPTY)
                        L.Add(AllPieces[yy - 2][xx]);
                }
            }
        }
        else
        {
            if (go.GetComponent<Cell>().Team == eTeam.WHITE && yy < 7)
            {
                if (AllPieces[yy + 1][xx].GetComponent<Cell>().pType == eType.EMPTY)
                    L.Add(AllPieces[yy + 1][xx]);
            }
            else if (go.GetComponent<Cell>().Team == eTeam.BLACK && yy > 0)
            {
                if (AllPieces[yy - 1][xx].GetComponent<Cell>().pType == eType.EMPTY)
                    L.Add(AllPieces[yy - 1][xx]);
            }
        }

        if (go.GetComponent<Cell>().Team == eTeam.WHITE) // GIET
        {
            if (yy <= 6)
            {
                if (xx >= 1 && Enemy(AllPieces[yy + 1][xx - 1], go))
                {
                    L.Add(AllPieces[yy + 1][xx - 1]);
                }
                if (xx <= 6 && Enemy(AllPieces[yy + 1][xx + 1], go))
                {
                    L.Add(AllPieces[yy + 1][xx + 1]);
                }
            }

        }
        else if (go.GetComponent<Cell>().Team == eTeam.BLACK)
        {
            if (yy >= 1)
            {
                if (xx >= 1 && Enemy(AllPieces[yy - 1][xx - 1], go))
                {
                    L.Add(AllPieces[yy - 1][xx - 1]);
                }
                if (xx <= 6 && Enemy(AllPieces[yy - 1][xx + 1], go))
                {
                    L.Add(AllPieces[yy - 1][xx + 1]);
                }
            }
        }
        return L;
    }

    public List<GameObject> RookPath(GameObject go)
    {
        List<GameObject> L = new List<GameObject>();
        int X = go.GetComponent<Cell>().x;
        int Y = go.GetComponent<Cell>().y;
        for (int i = Y - 1; i >= 0; i--) // Xuống dưới 
        {
            if (Enemy(go, AllPieces[i][X]))
            {
                L.Add(AllPieces[i][X]);
                break;
            }
            if (AllPieces[i][X].GetComponent<Cell>().pType != eType.EMPTY)
            {
                break;
            }
            else
                L.Add(AllPieces[i][X]);

        }
        for (int i = Y + 1; i < 8; i++) // Lên trên
        {
            if (Enemy(go, AllPieces[i][X]))
            {
                L.Add(AllPieces[i][X]);
                break;
            }
            if (AllPieces[i][X].GetComponent<Cell>().pType != eType.EMPTY)
            {
                break;
            }
            L.Add(AllPieces[i][X]);
        }
        for (int i = X - 1; i >= 0; i--) // Sang trái 
        {
            if (Enemy(go, AllPieces[Y][i]))
            {
                L.Add(AllPieces[Y][i]);
                break;
            }
            if (AllPieces[Y][i].GetComponent<Cell>().pType != eType.EMPTY)
            {
                break;
            }
            L.Add(AllPieces[Y][i]);
        }
        for (int i = X + 1; i < 8; i++) // sang phải
        {
            if (Enemy(go, AllPieces[Y][i]))
            {
                L.Add(AllPieces[Y][i]);
                break;
            }
            if (AllPieces[Y][i].GetComponent<Cell>().pType != eType.EMPTY)
            {
                break;
            }
            L.Add(AllPieces[Y][i]);
        }
        return L;
    }
    public List<GameObject> BishopPath(GameObject go)
    {
        List<GameObject> L = new List<GameObject>();
        int X = go.GetComponent<Cell>().x;
        int Y = go.GetComponent<Cell>().y;

        // TOP LEFT
        for (int iY = Y + 1, iX = X - 1; ; iY++, iX--)
        {
            if (!OutOfBound(iY, iX))
            {
                if (Enemy(go, AllPieces[iY][iX]))
                    L.Add(AllPieces[iY][iX]);
                if (AllPieces[iY][iX].GetComponent<Cell>().pType != eType.EMPTY)
                    break;
                L.Add(AllPieces[iY][iX]);
            }
            else
                break;
        }
        // TOP RIGHT
        for (int iY = Y + 1, iX = X + 1; ; iY++, iX++)
        {
            if (!OutOfBound(iY, iX))
            {
                if (Enemy(go, AllPieces[iY][iX]))
                    L.Add(AllPieces[iY][iX]);
                if (AllPieces[iY][iX].GetComponent<Cell>().pType != eType.EMPTY)
                    break;
                L.Add(AllPieces[iY][iX]);
            }
            else
                break;
        }
        // BOTTOM LEFT
        for (int iY = Y - 1, iX = X - 1; ; iY--, iX--)
        {
            if (!OutOfBound(iY, iX))
            {
                if (Enemy(go, AllPieces[iY][iX]))
                    L.Add(AllPieces[iY][iX]);
                if (AllPieces[iY][iX].GetComponent<Cell>().pType != eType.EMPTY)
                    break;
                L.Add(AllPieces[iY][iX]);
            }
            else
                break;
        }
        // BOTTOM RIGHT
        for (int iY = Y - 1, iX = X + 1; ; iY--, iX++)
        {
            if (!OutOfBound(iY, iX))
            {
                if (Enemy(go, AllPieces[iY][iX]))
                    L.Add(AllPieces[iY][iX]);
                if (AllPieces[iY][iX].GetComponent<Cell>().pType != eType.EMPTY)
                    break;
                L.Add(AllPieces[iY][iX]);
            }
            else
                break;
        }

        return L;
    }
    public List<GameObject> KnightPath(GameObject go)
    {

        List<GameObject> L = new List<GameObject>();
        int X = go.GetComponent<Cell>().x;
        int Y = go.GetComponent<Cell>().y;

        // TOP
        //LEFT OF TOP
        if (!OutOfBound(Y + 2, X - 1))
            if (Enemy(AllPieces[Y + 2][X - 1], go) || AllPieces[Y + 2][X - 1].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y + 2][X - 1]);
        // RIGHT OF TOP
        if (!OutOfBound(Y + 2, X + 1))
            if (Enemy(AllPieces[Y + 2][X + 1], go) || AllPieces[Y + 2][X + 1].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y + 2][X + 1]);

        // BOTTOM
        //LEFT OF BOTTOM
        if (!OutOfBound(Y - 2, X - 1))
            if (Enemy(AllPieces[Y - 2][X - 1], go) || AllPieces[Y - 2][X - 1].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y - 2][X - 1]);
        // RIGHT OF BOTTOM
        if (!OutOfBound(Y - 2, X + 1))
            if (Enemy(AllPieces[Y - 2][X + 1], go) || AllPieces[Y - 2][X + 1].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y - 2][X + 1]);

        // LEFT
        //TOP OF LEFT
        if (!OutOfBound(Y + 1, X - 2))
            if (Enemy(AllPieces[Y + 1][X - 2], go) || AllPieces[Y + 1][X - 2].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y + 1][X - 2]);
        // BOTTOM OF LEFT
        if (!OutOfBound(Y - 1, X - 2))
            if (Enemy(AllPieces[Y - 1][X - 2], go) || AllPieces[Y - 1][X - 2].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y - 1][X - 2]);

        // RIGHT
        //TOP OF RIGHT
        if (!OutOfBound(Y + 1, X + 2))
            if (Enemy(AllPieces[Y + 1][X + 2], go) || AllPieces[Y + 1][X + 2].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y + 1][X + 2]);
        // BOTTOM OF RIGHT
        if (!OutOfBound(Y - 1, X + 2))
            if (Enemy(AllPieces[Y - 1][X + 2], go) || AllPieces[Y - 1][X + 2].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y - 1][X + 2]);

        return L;
    }

    public List<GameObject> QueenPath(GameObject go)
    {
        List<GameObject> L = RookPath(go);
        foreach (GameObject g in BishopPath(go))
            L.Add(g);
        return L;
    }

    private List<GameObject> KingPath(GameObject go)
    {
        List<GameObject> L = new List<GameObject>();
        int X = go.GetComponent<Cell>().x;
        int Y = go.GetComponent<Cell>().y;
        if (!OutOfBound(Y + 1, X))
            if (Enemy(AllPieces[Y + 1][X], go) || AllPieces[Y + 1][X].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y + 1][X]);
        if (!OutOfBound(Y - 1, X))
            if (Enemy(AllPieces[Y - 1][X], go) || AllPieces[Y - 1][X].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y - 1][X]);
        if (!OutOfBound(Y, X - 1))
            if (Enemy(AllPieces[Y][X - 1], go) || AllPieces[Y][X - 1].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y][X - 1]);
        if (!OutOfBound(Y, X + 1))
            if (Enemy(AllPieces[Y][X + 1], go) || AllPieces[Y][X + 1].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y][X + 1]);
        if (!OutOfBound(Y + 1, X + 1))
            if (Enemy(AllPieces[Y + 1][X + 1], go) || AllPieces[Y + 1][X + 1].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y + 1][X + 1]);
        if (!OutOfBound(Y + 1, X - 1))
            if (Enemy(AllPieces[Y + 1][X - 1], go) || AllPieces[Y + 1][X - 1].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y + 1][X - 1]);
        if (!OutOfBound(Y - 1, X + 1))
            if (Enemy(AllPieces[Y - 1][X + 1], go) || AllPieces[Y - 1][X + 1].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y - 1][X + 1]);
        if (!OutOfBound(Y - 1, X - 1))
            if (Enemy(AllPieces[Y - 1][X - 1], go) || AllPieces[Y - 1][X - 1].GetComponent<Cell>().pType == eType.EMPTY)
                L.Add(AllPieces[Y - 1][X - 1]);
        if (go.GetComponent<Cell>().didMove == false)
        {
            if (AllPieces[Y][0].GetComponent<Cell>().pType == eType.ROOK && AllPieces[Y][0].GetComponent<Cell>().didMove == false &&
                AllPieces[Y][1].GetComponent<Cell>().pType == eType.EMPTY && AllPieces[Y][2].GetComponent<Cell>().pType == eType.EMPTY 
                && AllPieces[Y][3].GetComponent<Cell>().pType == eType.EMPTY)
            {
                L.Add(AllPieces[Y][X - 2]);
            }
            if (AllPieces[Y][7].GetComponent<Cell>().pType == eType.ROOK && AllPieces[Y][7].GetComponent<Cell>().didMove == false && 
                AllPieces[Y][6].GetComponent<Cell>().pType == eType.EMPTY && AllPieces[Y][5].GetComponent<Cell>().pType == eType.EMPTY)
            {
                L.Add(AllPieces[Y][X + 2]);
            }
        }
        return L;
    }
    #endregion

    #endregion


    public static bool Enemy(GameObject g1, GameObject g2)
    {
        if (g1.GetComponent<Cell>().Team == eTeam.BLACK && g2.GetComponent<Cell>().Team == eTeam.WHITE)
            return true;
        if (g2.GetComponent<Cell>().Team == eTeam.BLACK && g1.GetComponent<Cell>().Team == eTeam.WHITE)
            return true;
        return false;
    }

    #region RowsOrder
    private eType[] pawnRow = new eType[8]
    {
        eType.PAWN, eType.PAWN, eType.PAWN, eType.PAWN,
        eType.PAWN, eType.PAWN, eType.PAWN, eType.PAWN
    };
    private eType[] royaltyRow = new eType[8]
    {
        eType.ROOK, eType.KNIGHT, eType.BISHOP, eType.QUEEN,
        eType.KING, eType.BISHOP, eType.KNIGHT, eType.ROOK
    };
    #endregion
}
