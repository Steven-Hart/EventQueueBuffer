using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.XR;
using Vector2 = UnityEngine.Vector2;

public class Puzzle : MonoBehaviour
{
    public Texture2D image;
    public ParticleSystem particles;
    public int blocksPerLine = 4;
    public int suffleLength = 20;
    public float defaultMoveDuration = .2f;
    public float shuffleMoveDuration = .1f;
    
    public Player player;

    
    private Block emptyBlock;
    private Block[,] blocks;
    private bool blockIsMoving;
    private int suffleMovesRemaining;
    private Vector2Int prevSuffleOffset;
    
    private Queue<Block> inputs;
    private Stack<Block> reverseInputs;



    private void Start()
    {
        CreatePuzzle();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartSuffle();
        }
        if (Input.GetKey(KeyCode.X))
        {
            Reverse();
        }
        
    }

    void CreatePuzzle()
    {
        blocks = new Block[blocksPerLine,blocksPerLine];
        Texture2D[,] imageSlices = ImageSlicer.GetSlices(image, blocksPerLine);
        for (int y = 0; y < blocksPerLine; y++)
        {
            for (int x = 0; x < blocksPerLine; x++)
            {
                GameObject blockObject = GameObject.CreatePrimitive((PrimitiveType.Quad));
                blockObject.transform.position = -Vector2.one * (blocksPerLine - 1) * 0.5f + new Vector2(x, y);
                blockObject.transform.parent = transform;

                Block block = blockObject.AddComponent<Block>();
                block.OnBlockPressed += player.PlayerMoveBlockInput;
                block.OnFinishedMoving += OnBlockFinishedMoving;
                block.Init(new Vector2Int(x,y), imageSlices[x,y], particles);
                blocks[x, y] = block;

                if (y == 0 && x == blocksPerLine - 1)
                {
                    blockObject.SetActive(false);
                    emptyBlock = block;
                }
            }
        }

        Camera.main.orthographicSize = blocksPerLine * .55f;
        inputs = new Queue<Block>();
        reverseInputs = new Stack<Block>();
    }

    
    
    // Queue Methods

    public void UndoAll()
    {
        while(inputs.Count > 0)
        {
            inputs.Dequeue()._particles.Stop();
        }
    }
    

    public void AddInput(Block block)
    {
        inputs.Enqueue(block);
    }
    
    void Reverse()
    {
        while(reverseInputs.Count > 0 && !blockIsMoving)
        {
            MoveBlock(reverseInputs.Pop(), shuffleMoveDuration);
        }
    }
    
    //Player and Shuffle Input Handler

    public void MakeNextMove(string s)
    {
        if (s == "shuffle")
        {
            FindNextShuffleMove();
            while(inputs.Count > 0 && !blockIsMoving)
            {
                Block temp = inputs.Dequeue();
                reverseInputs.Push(temp);
                MoveBlock(temp, shuffleMoveDuration);
            }
        }
        else
        {
            while(inputs.Count > 0 && !blockIsMoving)
            {
                Block temp = inputs.Dequeue();
                temp._particles.Stop();
                reverseInputs.Push(temp);
                MoveBlock(temp, defaultMoveDuration);
            }
        }
    }
    
    //Move Block Methods
    
    void MoveBlock(Block blockToMove, float duration)
    {
        if ((blockToMove.coord - emptyBlock.coord).sqrMagnitude == 1)
        {
            blocks[blockToMove.coord.x, blockToMove.coord.y] = emptyBlock;
            blocks[emptyBlock.coord.x, emptyBlock.coord.y] = blockToMove;
            
            Vector2Int targetCoord = emptyBlock.coord;
            emptyBlock.coord = blockToMove.coord;
            blockToMove.coord = targetCoord;
            
            Vector2 targetPosition = emptyBlock.transform.position;
            emptyBlock.transform.position = blockToMove.transform.position;
            blockToMove.MoveToPosition(targetPosition, duration);
            blockIsMoving = true;
        }
    }

    void OnBlockFinishedMoving()
    {
        blockIsMoving = false;
        MakeNextMove("player");

        if (suffleMovesRemaining > 0)
        {
            MakeNextMove("shuffle");
        }
    }

    // Shuffle Methods
    
    void StartSuffle()
    {
        suffleMovesRemaining = suffleLength;
        MakeNextMove("shuffle");
    }

    

    void FindNextShuffleMove()
    {
        Vector2Int[] offsets =
            {new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1)};
        int randomIndex = Random.Range(0, offsets.Length);

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector2Int offset = offsets[(randomIndex + i) % offsets.Length];
            if (offset != prevSuffleOffset * -1)
            {
                Vector2Int moveBlockCoord = emptyBlock.coord + offset;

                if (moveBlockCoord.x >= 0 && moveBlockCoord.x < blocksPerLine && moveBlockCoord.y >= 0 &&
                    moveBlockCoord.y < blocksPerLine)
                {
                    AddInput(blocks[moveBlockCoord.x, moveBlockCoord.y]);
                    suffleMovesRemaining--;
                    prevSuffleOffset = offset;
                    Debug.Log(suffleMovesRemaining);
                    break;
                }
            }
        }
    }

    
}
