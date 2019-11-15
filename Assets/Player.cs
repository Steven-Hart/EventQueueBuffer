using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Puzzle puzzle;
    public bool delay;

    private void Start()
    {
        delay = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (delay)
            {
                delay = false;
                puzzle.MakeNextMove("player");
            }
            else
            {
                delay = true;
            }
        }

        
        if (Input.GetKeyDown(KeyCode.Z))
        {
            PlayerUndoAll();
        }
    }

    public void PlayerMoveBlockInput(Block blockToMove)
    {
        puzzle.AddInput(blockToMove);
        blockToMove._particles.Play();
        if (!delay)
        {
            puzzle.MakeNextMove("player");
        }
    }

    
    private void PlayerUndoAll()
    {
        puzzle.UndoAll();
    }
}
