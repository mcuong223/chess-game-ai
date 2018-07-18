using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityScript;
using Pairii = System.Collections.Generic.KeyValuePair<System.Int32, System.Int32>;


public class AI_Script : MonoBehaviour
{
    [SerializeField]
    public static int Depth;

    public static bool CellEqual(Piece v1, Piece v2)
    {
        if (v1.pType != v2.pType)
            return false;
        if (v1.Team != v2.Team)
            return false;
        if (v1.State != v2.State)
            return false;
        return true;
    }

    GameState Chuyen(GameState game)
    {
        GameState newGame = new GameState();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                newGame.Pieces[i][j] = ChuyenCell(game.Pieces[i][j]);
            }
        }
        return newGame;
    }

    public KeyValuePair<Pairii, Pairii> FindMove(GameState Sta)
    {
        int alpha = -999999999;
        GameState move = new GameState();
        foreach (GameState gs in NextStates(Sta, eTeam.BLACK))
        {
            int eval = Minimax(gs, Depth - 1, -999999999, 999999999, eTeam.WHITE);
            Debug.Log(eval);
            if (eval > alpha)
            {
                alpha = eval;
                move = Chuyen(gs);
            }
        }
        int flag = 0;
        int a = 0, b = 0, c = 0, d = 0;
        for (int i = 0; i < 8; i++)
        {
            if (flag == 2)
                break;
            for (int j = 0; j < 8; j++)
            {
                if (!CellEqual(Sta.Pieces[i][j], move.Pieces[i][j]))
                {
                    if (move.Pieces[i][j].Empty())
                    {
                        a = i;
                        b = j;
                        flag++;
                    }
                    if (!move.Pieces[i][j].Empty())
                    {
                        c = i;
                        d = j;
                        flag++;
                    }
                }
            }
        }
        return new KeyValuePair<Pairii, Pairii>(new Pairii(a, b), new Pairii(c, d));
    }



    private int Minimax(GameState gameState, int d, int alpha, int beta, eTeam team)
    {
        if (d == 0 || IsEndState(gameState)) 
        {
            int eval = Evaluation(gameState);
            return eval;
        }
        if (team == eTeam.BLACK) // MAX - NGUOI 
        {
            foreach (GameState gs in NextStates(gameState, eTeam.BLACK))
            {
                int e = Minimax(gs, d - 1, alpha, beta, eTeam.WHITE);
                if (e > alpha)
                    alpha = e;
                if (alpha >= beta)
                    break;
            }
            return alpha;
        }
        if (team == eTeam.WHITE) // MIN - MAY
        {
            foreach (GameState gs in NextStates(gameState, eTeam.WHITE))
            {
                int e = Minimax(gs, d - 1, alpha, beta, eTeam.BLACK);
                if (e < beta)
                    beta = e;
                if (alpha >= beta)
                    break;
            }
            return beta;
        }
        return 0;
    }

    eTeam SwitchTeam(eTeam team)
    {
        return (team == eTeam.WHITE) ? eTeam.BLACK : eTeam.WHITE;
    }

    bool IsEndState(GameState game)
    {
        int k = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (game.Pieces[i][j].pType == eType.KING)
                    k++;
                if (k == 2)
                    return false;
            }
        }
        return true;
    }

    public static int NumOfLegalMoves(GameState gameState, eTeam team)
    {
        if (IsCheckedState(gameState, team))
        {
            int a = 0;
        }
        return 20;
    }

    public static List<GameState> NextStates(GameState gameState, eTeam team)
    {
        List<GameState> L = new List<GameState>();
        int c;
        if (team == eTeam.BLACK)
        {
            c = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (gameState.Pieces[i][j].Team == team)
                    {
                        c++;
                        foreach (Pairii pair in Path(gameState.Pieces[i][j], gameState, i, j))
                        {
                            int m = pair.Key;
                            int n = pair.Value;
                            L.Add(ChangeState(gameState, i, j, m, n));
                        }
                        if (c >= 16)
                            return L;
                    }
                }
            }
        }
        else
        {
            c = 0;
            for (int i = 7; i >= 0; i--)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (gameState.Pieces[i][j].Team == team)
                    {
                        c++;
                        foreach (Pairii pair in Path(gameState.Pieces[i][j], gameState, i, j))
                        {
                            int m = pair.Key;
                            int n = pair.Value;
                            L.Add(ChangeState(gameState, i, j, m, n));
                        }
                        if (c >= 16)
                            return L;
                    }
                }
            }
        }

        return L;
    }

    static Piece ChuyenCell(Piece vir)
    {
        Piece cell = new Piece();
        cell.pType = vir.pType;
        cell.State = vir.State;
        cell.Team = vir.Team;
        return cell;
    }

    public static GameState ChangeState(GameState game, int y, int x, int m, int n)
    {
        GameState newState = new GameState();
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                newState.Pieces[i][j] = ChuyenCell(game.Pieces[i][j]);
        if ((m == 7 || m == 0) && game.Pieces[y][x].pType == eType.PAWN)
        {
            newState.Pieces[m][n] = ChuyenCell(game.Pieces[y][x]);
            newState.Pieces[m][n].pType = eType.QUEEN;
        }
        else
            newState.Pieces[m][n] = ChuyenCell(game.Pieces[y][x]);

        newState.Pieces[y][x].pType = eType.EMPTY;
        newState.Pieces[y][x].State = eState.FREE;
        newState.Pieces[y][x].Team = eTeam.MID;

        return newState;
    }

    public static int Evaluation(GameState gameState)
    {
        int eval = -MaterialEval(gameState);
        if (GameManager.isHard) eval += PositionEval(gameState);
        return eval;
    }

    public static int MaterialEval(GameState gameState)
    {
        int eval = 0;
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (gameState.Pieces[i][j].Team == eTeam.BLACK)
                {
                    count++;
                    switch (gameState.Pieces[i][j].pType)
                    {
                        case eType.PAWN:
                            eval -= pawnMaterial;
                            break;
                        case eType.ROOK:
                            eval -= rookMaterial;
                            break;
                        case eType.KNIGHT:
                            eval -= knightMaterial;
                            break;
                        case eType.BISHOP:
                            eval -= bishopMaterial;
                            break;
                        case eType.KING:
                            eval -= kingMaterial;
                            break;
                        case eType.QUEEN:
                            eval -= queenMaterial;
                            break;
                    }
                }
                else if (gameState.Pieces[i][j].Team == eTeam.WHITE)
                {
                    count++;
                    switch (gameState.Pieces[i][j].pType)
                    {
                        case eType.PAWN:
                            eval += pawnMaterial;
                            break;
                        case eType.ROOK:
                            eval += rookMaterial;
                            break;
                        case eType.KNIGHT:
                            eval += knightMaterial;
                            break;
                        case eType.BISHOP:
                            eval += bishopMaterial;
                            break;
                        case eType.KING:
                            eval += kingMaterial;
                            break;
                        case eType.QUEEN:
                            eval += queenMaterial;
                            break;
                    }
                }
                if (count >= 32)
                    break;
            }
            if (count >= 32)
                break;
        }
        if (GamePlayManager.PieceCounts[3] > 1)
            eval += 50 * GamePlayManager.PieceCounts[3];
        if (GamePlayManager.PieceCounts[9] > 1)
            eval -= 50 * GamePlayManager.PieceCounts[3];
        return eval * 5;
    }

    public static int SumArr(int[] a)
    {
        int s = 0;
        for (int i = 0; i < a.Length; i++)
        {
            s += a[i];
        }
        return s;
    }

    public static int PositionEval(GameState gameState)
    {
        int black = 0;
        int white = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (gameState.Pieces[i][j].Team == eTeam.BLACK)
                {
                    switch (gameState.Pieces[i][j].pType)
                    {
                        case eType.PAWN:
                            black += PawnPosEval[i][j];
                            break;
                        case eType.ROOK:
                            black += RookPosEval[i][j];
                            break;
                        case eType.KNIGHT:
                            black += KnightPosEval[i][j];
                            break;
                        case eType.BISHOP:
                            black += BishopPosEval[i][j];
                            break;
                        case eType.KING:
                            if (SumArr(GamePlayManager.PieceCounts) > 16) // Đầu và giữa game
                            {
                                black += KingPosEval_Early[i][j];
                            }
                            else
                            {
                                black += KingPosEval_Lately[i][j];
                            }
                            break;
                        case eType.QUEEN:
                            black += QueenPosEval[i][j];
                            break;
                    }
                }
                if (gameState.Pieces[i][j].Team == eTeam.WHITE)
                {
                    switch (gameState.Pieces[i][j].pType)
                    {
                        case eType.PAWN:
                            white += PawnPosEval[7 - i][j];
                            break;
                        case eType.ROOK:
                            white += RookPosEval[7 - i][j];
                            break;
                        case eType.KNIGHT:
                            white += KnightPosEval[7 - i][j];
                            break;
                        case eType.BISHOP:
                            white += BishopPosEval[7 - i][j];
                            break;
                        case eType.KING:
                            if (SumArr(GamePlayManager.PieceCounts) > 16) // Đầu và giữa game
                            {
                                white += KingPosEval_Early[7 - i][j];
                            }
                            else
                            {
                                white += KingPosEval_Lately[7 - i][j];
                            }
                            break;
                        case eType.QUEEN:
                            white += QueenPosEval[7 - i][j];
                            break;
                    }
                }
            }
        }
        return (black - white) * 2;
    }

    public static int MobilityEval(GameState gameState)
    {
        int eval = 0;
        int black = 0, white = 0;
        black += NumOfLegalMoves(gameState, eTeam.BLACK);
        white += NumOfLegalMoves(gameState, eTeam.WHITE);
        eval = (black - white) * 75;
        if (black == 0) // black bi chieu bi
        {
            eval += 200000;
        }
        if (white == 0)
        {
            eval -= 200000;
        }
        return eval;
    }

    public static bool IsCheckedState(GameState gameState, eTeam team)
    {
        int y = 0, x = 0;
        if (team == eTeam.WHITE)
        {
            for (int i = 0; i < 8; i++)
            {
                bool done = false;
                for (int j = 0; j < 8; j++)
                {
                    if (gameState.Pieces[i][j].pType == eType.KING)
                    {
                        y = i;
                        x = j;
                        done = true;
                        break;
                    }
                }
                if (done) break;
            }
        }
        else
        {
            for (int i = 7; i >= 0; i--)
            {
                bool done = false;
                for (int j = 0; j < 8; j++)
                {
                    if (gameState.Pieces[i][j].pType == eType.KING)
                    {
                        y = i;
                        x = j;
                        done = true;
                        break;
                    }
                }
                if (done) break;
            }
        }
        // King on Rook Path
        List<Pairii> KingOnRook = RookPath(gameState.Pieces[y][x], gameState, y, x);
        foreach (Pairii p in KingOnRook)
        {
            if (gameState.Pieces[p.Key][p.Value].pType == eType.ROOK || gameState.Pieces[p.Key][p.Value].pType == eType.QUEEN)
                return true;
        }
        // King on Bishop path
        List<Pairii> KingOnBishop = BishopPath(gameState.Pieces[y][x], gameState, y, x);
        foreach (Pairii p in KingOnBishop)
        {
            if (gameState.Pieces[p.Key][p.Value].pType == eType.BISHOP || gameState.Pieces[p.Key][p.Value].pType == eType.QUEEN)
                return true;
        }
        // King on Knight path
        List<Pairii> KingOnKnight = KnightPath(gameState.Pieces[y][x], gameState, y, x);
        foreach (Pairii p in KingOnKnight)
        {
            if (gameState.Pieces[p.Key][p.Value].pType == eType.KNIGHT)
                return true;
        }
        if (team == eTeam.WHITE)
        {
            // Pawn
            if (gameState.Pieces[y + 1][x + 1].pType == eType.PAWN)
                return true;

            if (gameState.Pieces[y + 1][x - 1].pType == eType.PAWN)
                return true;
        }
        else
        {
            if (!OutOfBound(y - 1, x + 1))
                if (gameState.Pieces[y - 1][x + 1].pType == eType.PAWN)
                    return true;

            if (!OutOfBound(y - 1, x - 1))
                if (gameState.Pieces[y - 1][x - 1].pType == eType.PAWN)
                    return true;

        }
        // Enemy King
        if (!OutOfBound(y + 1, x - 1))
            if (gameState.Pieces[y + 1][x - 1].pType == eType.KING)
                return true;
        if (!OutOfBound(y + 1, x + 1))
            if (gameState.Pieces[y + 1][x + 1].pType == eType.KING)
                return true;
        if (!OutOfBound(y - 1, x - 1))
            if (gameState.Pieces[y - 1][x - 1].pType == eType.KING)
                return true;
        if (!OutOfBound(y - 1, x + 1))
            if (gameState.Pieces[y - 1][x + 1].pType == eType.KING)
                return true;
        if (!OutOfBound(y, x - 1))
            if (gameState.Pieces[y][x - 1].pType == eType.KING)
                return true;
        if (!OutOfBound(y, x + 1))
            if (gameState.Pieces[y][x + 1].pType == eType.KING)
                return true;
        if (!OutOfBound(y + 1, x))
            if (gameState.Pieces[y + 1][x].pType == eType.KING)
                return true;
        if (!OutOfBound(y - 1, x))
            if (gameState.Pieces[y - 1][x].pType == eType.KING)
                return true;
        return false;
    }

    #region ********_EVALUATIONS_********

    private static int pawnMaterial = 100;
    private static int knightMaterial = 320;
    private static int bishopMaterial = 330;
    private static int rookMaterial = 500;
    private static int queenMaterial = 900;
    private static int kingMaterial = 20000;

    private static int[][] PawnPosEval = new int[8][]
    {
        new int[8] {0, 0, 0, 0, 0, 0, 0, 0},
        new int[8] {50, 50, 50, 50, 50, 50, 50, 50},
        new int[8] {10, 10, 20, 30, 30, 20, 10, 10},
        new int[8] {5,   5, 10, 25, 25, 10,  5,  5},
        new int[8] {0,   0,  0, 20, 20,  0,  0,  0},
        new int[8] {5,  -5,-10,  0,  0,-10, -5,  5},
        new int[8] {5,  10, 10,-20,-20, 10, 10, 5},
        new int[8] {0,   0,  0,  0,   0,  0, 0, 0}
    };

    private static int[][] RookPosEval = new int[8][]
    {
        new int[8] {0, 0, 0, 0, 0, 0, 0, 0},
        new int[8] {5, 10, 10, 10, 10, 10, 10, 5},
        new int[8] {-5, 0, 0, 0, 0, 0, 0, -5},
        new int[8] {-5, 0, 0, 0, 0, 0, 0, -5},
        new int[8] {-5, 0, 0, 0, 0, 0, 0, -5},
        new int[8] {-5, 0, 0, 0, 0, 0, 0, -5},
        new int[8] {-5, 0, 0, 0, 0, 0, 0, -5},
        new int[8] {0, 0, 0, 5, 5, 0, 0, 0}
    };
    private static int[][] KnightPosEval = new int[8][]
    {
        new int[8] {-50, -40, -30, -30, -30, -30, -40, -50},
        new int[8] {-40, -20, 0, 0, 0, 0, -20, -40},
        new int[8] {-30, 0, 10, 15, 15, 10, 0, -30},
        new int[8] {-30, 5, 15, 20, 20, 15, 5, -30},
        new int[8] {-30, 0, 15, 20, 20, 15, 0, -30},
        new int[8] {-30, 5, 10, 15, 15, 10, 5, -30},
        new int[8] {-40, -20, 0, 5, 5, 0, -20, -40},
        new int[8] {-50, -40, -30, -30, -30, -30, -40, -50}
    };
    private static int[][] BishopPosEval = new int[8][]
    {
        new int[8] {-20, -10, -10, -10, -10, -10, -10, -20},
        new int[8] {-10, 0, 0, 0, 0, 0, 0, -10},
        new int[8] {-10, 0, 5, 10, 10, 5, 0, -10},
        new int[8] {-10, 5, 5, 10, 10, 5, 5, -10},
        new int[8] {-10, 0, 10, 10, 10, 10, 0, -10},
        new int[8] {-10, 10, 10, 10, 10, 10, 10, -10},
        new int[8] {-10, 5, 0, 0, 0, 0, 5, -10},
        new int[8] {-20, -10, -10, -10, -10, -10, -10, -20}
    };
    private static int[][] KingPosEval_Early = new int[8][]
    {
        new int[8] {-30, -40, -40, -50, -50, -40, -40, -30},
        new int[8] {-30, -40, -40, -50, -50, -40, -40, -30},
        new int[8] {-30, -40, -40, -50, -50, -40, -40, -30},
        new int[8] {-30, -40, -40, -50, -50, -40, -40, -30},
        new int[8] {-20, -30, -30, -40, -40, -30, -30, -20},
        new int[8] {-10, -20, -20, -20, -20, -20, -20, -10},
        new int[8] {20, 20, 0, 0, 0, 0, 20, 20},
        new int[8] {20, 30, 10, 0, 0, 10, 30, 20}
    };

    private static int[][] KingPosEval_Lately = new int[8][]
    {
        new int[8] {-50, -40, -30, -20, -20, -30, -40, -50},
        new int[8] {-30, -20, -10, 0, 0, -10, -20, -30},
        new int[8] {-30, -10, 20, 30, 30, 20, -10, -30},
        new int[8] {-30, -10, 30, 40, 40, 30, -10, -30},
        new int[8] {-30, -10, 30, 40, 40, 30, -10, -30},
        new int[8] {-30, -10, 20, 30, 30, 20, -10, -30},
        new int[8] {-30, -30, 0, 0, 0, 0, -30, -30},
        new int[8] {-50, -30, -30, -30, -30, -30, -30, -50}
    };

    private static int[][] QueenPosEval = new int[8][]
    {
        new int[8] {-20, -10, -10, -5, -5, -10, -10, -20},
        new int[8] {-10, 0, 0, 0, 0, 0, 0, -10},
        new int[8] {-10, 0, 5, 5, 5, 5, 0, -10},
        new int[8] {-5, 0, 5, 5, 5, 5, 0, -5},
        new int[8] {0, 0, 5, 5, 5, 5, 0, -5},
        new int[8] {-10, 5, 5, 5, 5, 5, 0, -10},
        new int[8] {-10, 0, 5, 0, 0, 0, 0, -10},
        new int[8] {-20, -10, -10, -5, -5, -10, -10, -20}
    };
    #endregion

    #region ****************************************CELLPATHS****************************************
    static bool CellEnemy(Piece v1, Piece v2)
    {
        if (v1.Team == eTeam.BLACK && v2.Team == eTeam.WHITE)
            return true;
        if (v1.Team == eTeam.WHITE && v2.Team == eTeam.BLACK)
            return true;
        return false;
    }
    // Pairii = KeyValuePair<int, int>
    public static List<Pairii> Path(Piece piece, GameState gameState, int xx, int yy)
    {
        List<Pairii> L = new List<Pairii>();
        switch (piece.pType)
        {
            case eType.PAWN:
                L = PawnPath(piece, gameState, xx, yy);
                break;
            case eType.ROOK:
                L = RookPath(piece, gameState, xx, yy);
                break;
            case eType.BISHOP:
                L = BishopPath(piece, gameState, xx, yy);
                break;
            case eType.KNIGHT:
                L = KnightPath(piece, gameState, xx, yy);
                break;
            case eType.QUEEN:
                L = QueenPath(piece, gameState, xx, yy);
                break;
            case eType.KING:
                L = KingPath(piece, gameState, xx, yy);
                break;
        }
        return L;
    }

    private static List<Pairii> PawnPath(Piece piece, GameState gameState, int yy, int xx)
    {
        List<Pairii> L = new List<Pairii>();
        if ((yy == 1 && piece.Team == eTeam.WHITE) || (yy == 6 && piece.Team == eTeam.BLACK)) // CHUA DI
        {
            if (piece.Team == eTeam.WHITE && yy < 7)
            {
                if (gameState.Pieces[yy + 1][xx].pType == eType.EMPTY)
                    L.Add(new Pairii(yy + 1, xx));

                if (gameState.Pieces[yy + 2][xx].pType == eType.EMPTY && gameState.Pieces[yy + 1][xx].pType == eType.EMPTY)
                    L.Add(new Pairii(yy + 2, xx));
            }
            if (piece.Team == eTeam.BLACK && yy > 0)
            {
                if (gameState.Pieces[yy - 1][xx].pType == eType.EMPTY)
                    L.Add(new Pairii(yy - 1, xx));

                if (gameState.Pieces[yy - 2][xx].pType == eType.EMPTY && gameState.Pieces[yy - 1][xx].pType == eType.EMPTY)
                    L.Add(new Pairii(yy - 2, xx));
            }
        }
        else // Di roi
        {
            if (piece.Team == eTeam.WHITE && yy < 7)
                if (gameState.Pieces[yy + 1][xx].pType == eType.EMPTY)
                    L.Add(new Pairii(yy + 1, xx));
            if (piece.Team == eTeam.BLACK && yy > 0)
                if (gameState.Pieces[yy - 1][xx].pType == eType.EMPTY)
                    L.Add(new Pairii(yy - 1, xx));
        }

        if (piece.Team == eTeam.WHITE) // GIET
            if (yy <= 6)
            {
                if (xx >= 1 && CellEnemy(gameState.Pieces[yy + 1][xx - 1], piece))
                    L.Add(new Pairii(yy + 1, xx - 1));
                if (xx <= 6 && CellEnemy(gameState.Pieces[yy + 1][xx + 1], piece))
                    L.Add(new Pairii(yy + 1, xx + 1));
            }
        else if (piece.Team == eTeam.BLACK)
            if (yy >= 1)
            {
                if (xx >= 1 && CellEnemy(gameState.Pieces[yy - 1][xx - 1], piece))
                    L.Add(new Pairii(yy - 1, xx - 1));
                if (xx <= 6 && CellEnemy(gameState.Pieces[yy - 1][xx + 1], piece))
                    L.Add(new Pairii(yy - 1, xx + 1));
            }
        return L;
    }

    public static List<Pairii> RookPath(Piece piece, GameState gameState, int Y, int X)
    {
        List<Pairii> L = new List<Pairii>();
        for (int i = Y - 1; i >= 0; i--) // Xuống dưới 
        {
            if (CellEnemy(piece, gameState.Pieces[i][X]))
            {
                L.Add(new Pairii(i, X));
                break;
            }
            if (gameState.Pieces[i][X].pType != eType.EMPTY)
                break;
            else
                L.Add(new Pairii(i, X));

        }
        for (int i = Y + 1; i < 8; i++) // Lên trên
        {
            if (CellEnemy(piece, gameState.Pieces[i][X]))
            {
                L.Add(new Pairii(i, X));
                break;
            }
            if (gameState.Pieces[i][X].pType != eType.EMPTY)
                break;
            else
                L.Add(new Pairii(i, X));
        }
        for (int i = X - 1; i >= 0; i--) // Sang trái 
        {
            if (CellEnemy(piece, gameState.Pieces[Y][i]))
            {
                L.Add(new Pairii(Y, i));
                break;
            }
            if (gameState.Pieces[Y][i].pType != eType.EMPTY)
                break;
            else
                L.Add(new Pairii(Y, i));
        }
        for (int i = X + 1; i < 8; i++) // sang phải
        {
            if (CellEnemy(piece, gameState.Pieces[Y][i]))
            {
                L.Add(new Pairii(Y, i));
                break;
            }
            if (gameState.Pieces[Y][i].pType != eType.EMPTY)
                break;
            else
                L.Add(new Pairii(Y, i));
        }
        return L;
    }

    private static bool OutOfBound(int y, int x)
    {
        if (y > 7 || y < 0 || x > 7 || x < 0)
        {
            return true;
        }
        return false;
    }
    public static List<Pairii> BishopPath(Piece piece, GameState gameState, int Y, int X)
    {
        List<Pairii> L = new List<Pairii>();
        // TOP LEFT
        for (int iY = Y + 1, iX = X - 1; ; iY++, iX--)
        {
            if (!OutOfBound(iY, iX))
            {
                if (CellEnemy(piece, gameState.Pieces[iY][iX]))
                    L.Add(new Pairii(iY, iX));
                if (gameState.Pieces[iY][iX].pType != eType.EMPTY)
                    break;
                L.Add(new Pairii(iY, iX));
            }
            else
                break;
        }
        // TOP RIGHT
        for (int iY = Y + 1, iX = X + 1; ; iY++, iX++)
        {
            if (!OutOfBound(iY, iX))
            {
                if (CellEnemy(piece, gameState.Pieces[iY][iX]))
                    L.Add(new Pairii(iY, iX));
                if (gameState.Pieces[iY][iX].pType != eType.EMPTY)
                    break;
                L.Add(new Pairii(iY, iX));
            }
            else
                break;
        }
        // BOTTOM LEFT
        for (int iY = Y - 1, iX = X - 1; ; iY--, iX--)
        {
            if (!OutOfBound(iY, iX))
            {
                if (CellEnemy(piece, gameState.Pieces[iY][iX]))
                    L.Add(new Pairii(iY, iX));
                if (gameState.Pieces[iY][iX].pType != eType.EMPTY)
                    break;
                L.Add(new Pairii(iY, iX));
            }
            else
                break;
        }
        // BOTTOM RIGHT
        for (int iY = Y - 1, iX = X + 1; ; iY--, iX++)
        {
            if (!OutOfBound(iY, iX))
            {
                if (CellEnemy(piece, gameState.Pieces[iY][iX]))
                    L.Add(new Pairii(iY, iX));
                if (gameState.Pieces[iY][iX].pType != eType.EMPTY)
                    break;
                L.Add(new Pairii(iY, iX));
            }
            else
                break;
        }

        return L;
    }

    public static List<Pairii> KnightPath(Piece piece, GameState gameState, int Y, int X)
    {
        List<Pairii> L = new List<Pairii>();

        // TĂNG CỘT 2, GIẢM HÀNG 1
        if (!OutOfBound(Y + 2, X - 1))
            if (CellEnemy(gameState.Pieces[Y + 2][X - 1], piece) || gameState.Pieces[Y + 2][X - 1].pType == eType.EMPTY)
                L.Add(new Pairii(Y + 2, X - 1));
        // TĂNG CỘT 2, TĂNG HÀNG 1
        if (!OutOfBound(Y + 2, X + 1))
            if (CellEnemy(gameState.Pieces[Y + 2][X + 1], piece) || gameState.Pieces[Y + 2][X + 1].pType == eType.EMPTY)
                L.Add(new Pairii(Y + 2, X + 1));

        // GIẢM CỘT 2, GIẢM HÀNG 1
        if (!OutOfBound(Y - 2, X - 1))
            if (CellEnemy(gameState.Pieces[Y - 2][X - 1], piece) || gameState.Pieces[Y - 2][X - 1].pType == eType.EMPTY)
                L.Add(new Pairii(Y - 2, X - 1));
        // GIẢM CỘT 2, TĂNG HÀNG 1
        if (!OutOfBound(Y - 2, X + 1))
            if (CellEnemy(gameState.Pieces[Y - 2][X + 1], piece) || gameState.Pieces[Y - 2][X + 1].pType == eType.EMPTY)
                L.Add(new Pairii(Y - 2, X + 1));

        // TĂNG CỘT 1 GIẢM HÀNG 2
        if (!OutOfBound(Y + 1, X - 2))
            if (CellEnemy(gameState.Pieces[Y + 1][X - 2], piece) || gameState.Pieces[Y + 1][X - 2].pType == eType.EMPTY)
                L.Add(new Pairii(Y + 1, X - 2));
        // GIẢM CỘT 1, GIẢM HÀNG 2
        if (!OutOfBound(Y - 1, X - 2))
            if (CellEnemy(gameState.Pieces[Y - 1][X - 2], piece) || gameState.Pieces[Y - 1][X - 2].pType == eType.EMPTY)
                L.Add(new Pairii(Y - 1, X - 2));

        // TĂNG CỘT 1, TĂNG HÀNG 2
        if (!OutOfBound(Y + 1, X + 2))
            if (CellEnemy(gameState.Pieces[Y + 1][X + 2], piece) || gameState.Pieces[Y + 1][X + 2].pType == eType.EMPTY)
                L.Add(new Pairii(Y + 1, X + 2));
        // GIẢM CỘT 1, TĂNG HÀNG 2
        if (!OutOfBound(Y - 1, X + 2))
            if (CellEnemy(gameState.Pieces[Y - 1][X + 2], piece) || gameState.Pieces[Y - 1][X + 2].pType == eType.EMPTY)
                L.Add(new Pairii(Y - 1, X + 2));
        return L;
    }

    public static List<Pairii> QueenPath(Piece piece, GameState gameState, int X, int Y)
    {
        List<Pairii> L = RookPath(piece, gameState, X, Y);
        foreach (Pairii g in BishopPath(piece, gameState, X, Y))
            L.Add(g);
        return L;
    }


    private static List<Pairii> KingPath(Piece piece, GameState gameState, int Y, int X)
    {
        List<Pairii> L = new List<Pairii>();
        if (!OutOfBound(Y + 1, X))
            if (CellEnemy(gameState.Pieces[Y + 1][X], piece) || gameState.Pieces[Y + 1][X].pType == eType.EMPTY)
                L.Add(new Pairii(Y + 1, X));
        if (!OutOfBound(Y - 1, X))
            if (CellEnemy(gameState.Pieces[Y - 1][X], piece) || gameState.Pieces[Y - 1][X].pType == eType.EMPTY)
                L.Add(new Pairii(Y - 1, X));
        if (!OutOfBound(Y, X - 1))
            if (CellEnemy(gameState.Pieces[Y][X - 1], piece) || gameState.Pieces[Y][X - 1].pType == eType.EMPTY)
                L.Add(new Pairii(Y, X - 1));
        if (!OutOfBound(Y, X + 1))
            if (CellEnemy(gameState.Pieces[Y][X + 1], piece) || gameState.Pieces[Y][X + 1].pType == eType.EMPTY)
                L.Add(new Pairii(Y, X + 1));
        if (!OutOfBound(Y + 1, X + 1))
            if (CellEnemy(gameState.Pieces[Y + 1][X + 1], piece) || gameState.Pieces[Y + 1][X + 1].pType == eType.EMPTY)
                L.Add(new Pairii(Y + 1, X + 1));
        if (!OutOfBound(Y + 1, X - 1))
            if (CellEnemy(gameState.Pieces[Y + 1][X - 1], piece) || gameState.Pieces[Y + 1][X - 1].pType == eType.EMPTY)
                L.Add(new Pairii(Y + 1, X - 1));
        if (!OutOfBound(Y - 1, X + 1))
            if (CellEnemy(gameState.Pieces[Y - 1][X + 1], piece) || gameState.Pieces[Y - 1][X + 1].pType == eType.EMPTY)
                L.Add(new Pairii(Y - 1, X + 1));
        if (!OutOfBound(Y - 1, X - 1))
            if (CellEnemy(gameState.Pieces[Y - 1][X - 1], piece) || gameState.Pieces[Y - 1][X - 1].pType == eType.EMPTY)
                L.Add(new Pairii(Y - 1, X - 1));
        return L;
    }
    #endregion

}
