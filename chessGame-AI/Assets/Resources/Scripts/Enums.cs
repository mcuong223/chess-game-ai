using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eType
{
    PAWN, KNIGHT, BISHOP, ROOK, KING, QUEEN, EMPTY
}

public enum eState
{
    TARGET, FREE, SELECTED, SELECTABLE, KILLABLE
}

public enum eTeam
{
    BLACK, WHITE, MID
}

public enum eMinimax
{
    MIN, MAX
}