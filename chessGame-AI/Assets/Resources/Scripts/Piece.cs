using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece
{
    
    public bool Empty()
    {
        if (Team != eTeam.MID)
            return false;
        if (State != eState.FREE)
            return false;
        if (pType != eType.EMPTY)
            return false;
        return true;
    }
    private eTeam team;
    private eState state;
    private eType ptype;

    public eTeam Team
    {
        get
        {
            return team;
        }
        set
        {
            team = value;
        }
    }
    public eState State
    {
        get
        {
            return state;
        }
        set
        {
            state = value;
        }
    }
    public eType pType
    {
        get
        {
            return ptype;
        }
        set
        {
            ptype = value;
        }
    }
     public bool didMove
    {
        get; set;
    }
}
