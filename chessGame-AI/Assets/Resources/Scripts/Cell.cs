using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {
    [SerializeField]
    private eTeam team;
    [SerializeField]
    private eState state;
    [SerializeField]
    private eType ptype;
    public eState prevState
    {
        get; set;
    }
    public eTeam prevTeam
    {
        get; set;
    }
    public eType prevType
    {
        get; set;
    }


    public bool didMove
    {
        get; set;
    }


    public int x;
    public int y;

    public Color32 TeamColor(eTeam t)
    {
        if (t == eTeam.BLACK)
        {
            return new Color32(0, 0, 0, 255);
        }
        return new Color32(255, 255, 255, 255);
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
            switch(value)
            {
                case eType.PAWN:
                    this.GetComponent<SpriteRenderer>().sprite = S_Pawn;
                    break;
                case eType.ROOK:
                    this.GetComponent<SpriteRenderer>().sprite = S_Rook;
                    break;
                case eType.KNIGHT:
                    this.GetComponent<SpriteRenderer>().sprite = S_Knight;
                    break;
                case eType.BISHOP:
                    this.GetComponent<SpriteRenderer>().sprite = S_Bishop;
                    break;
                case eType.KING:
                    this.GetComponent<SpriteRenderer>().sprite = S_King;
                    break;
                case eType.QUEEN:
                    this.GetComponent<SpriteRenderer>().sprite = S_Queen;
                    break;
                case eType.EMPTY:
                    this.GetComponent<SpriteRenderer>().sprite = S_Free;
                    break;
            }
        }
    }

    public eTeam Team
    {
        get 
        {
            return team;
        }
        set
        {
            team = value;
            switch(value)
            {
                case eTeam.BLACK:
                    this.GetComponent<SpriteRenderer>().color = TeamColor(value);
                    break;
                case eTeam.WHITE:
                    this.GetComponent<SpriteRenderer>().color = TeamColor(value);
                    break;
            }
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
            switch(value)
            {
                case eState.TARGET:
                    GameObject go = this.transform.Find("CellMask").gameObject;
                    go.GetComponent<SpriteRenderer>().enabled = true;
                    break;
                case eState.KILLABLE:
                    GameObject go1 = this.transform.Find("KillMask").gameObject;
                    go1.GetComponent<SpriteRenderer>().enabled = true;
                    break;
                case eState.FREE:
                    this.GetComponent<SpriteRenderer>().sprite = S_Free;
                    break;
            }
        }
    }

    public Sprite S_Pawn;
    public Sprite S_Rook;
    public Sprite S_Knight;
    public Sprite S_Bishop;
    public Sprite S_King;
    public Sprite S_Queen;
    public Sprite S_Free;
}
