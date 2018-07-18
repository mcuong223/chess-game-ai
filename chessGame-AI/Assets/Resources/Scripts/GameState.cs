using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    public GameState()
    {
        Pieces = new Piece[8][];
        for (int i = 0; i < 8; i++)
        {
            Pieces[i] = new Piece[8];
            for (int j = 0; j < 8; j++)
            {
                Pieces[i][j] = new Piece();
            }
        }
    }
    public Piece[][] Pieces;
}
