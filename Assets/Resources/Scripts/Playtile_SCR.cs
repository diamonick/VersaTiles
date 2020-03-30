using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playtile_SCR : MonoBehaviour
{
    public enum Tile
    {
        ATKPlus1,       // Attack +1
        ATKPlus2,       // Attack +2
        ATKPlus3,       // Attack +3
        ATKPlus4,       // Attack +4
        MulATKPlus1,    // Multi-Attack +1
        MulATKPlus2,    // Multi-Attack +2
        HPPlus5,        // Heart Points +5
        CPPlus1,        // Command Points +1
        CmdItem,        // Command Item
        Mul2,           // x2
        Mul3,           // x3
        ElementSwap    // Element Swap
    };

    private GameObject Obj;
    private Shader GreyscaleShader;
    private const string Resource = "Sprites/GameplayUI/PlayTiles";
    private Tile tileType = Tile.ATKPlus1;
    private bool isNullified = false;
    [SerializeField] private int ID = 0;
    private int frameIndex = 0;
    private float grayVal = 0f;

    private void Awake()
    {
        Obj = this.gameObject;
        GreyscaleShader = Resources.Load<Shader>("Greyscale_SHD");
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isNullified)
        {
            if (grayVal < 1f)
            {
                grayVal += 0.2f;
                Obj.GetComponent<SpriteRenderer>().material.SetFloat("_EffectAmount", grayVal);
            }
        }
    }

    public void EnableGreyscale()
    {
        Obj.GetComponent<SpriteRenderer>().material.shader = GreyscaleShader;
        isNullified = true;
    }

    public bool isTileNullified() { return isNullified; }

    public int GetID() { return ID; }
    public void SetID(int id)
    {
        ID = id;
        Obj.name = $"Playtile {id}";
    }
    public int GetFrameIndex() { return frameIndex; }
    public Tile GetTileType() { return tileType; }
    public void SetTileType(Tile t) { tileType = t; }
    public int GetElementIndex() { return frameIndex; }

    public void AssignTileType(int typeNum, int frameNum = -1)
    {
        OtherFunctions.ChangeSprite(Obj, Resource, typeNum);
        switch (typeNum)
        {
            //Attack +1 Tile
            case 0:
                {
                    tileType = Tile.ATKPlus1;
                    OtherFunctions.ChangeSprite(Obj, Resource, 0);
                    break;
                }
            //Attack +2 Tile
            case 1:
                {
                    tileType = Tile.ATKPlus2;
                    OtherFunctions.ChangeSprite(Obj, Resource, 1);
                    break;
                }
            //Attack +3 Tile
            case 2:
                {
                    tileType = Tile.ATKPlus3;
                    OtherFunctions.ChangeSprite(Obj, Resource, 2);
                    break;
                }
            //Attack +4 Tile
            case 3:
                {
                    tileType = Tile.ATKPlus4;
                    OtherFunctions.ChangeSprite(Obj, Resource, 3);
                    break;
                }
            //Multi-Attack +1 Tile
            case 4:
                {
                    tileType = Tile.MulATKPlus1;
                    OtherFunctions.ChangeSprite(Obj, Resource, 4);
                    break;
                }
            //Multi-Attack +2 Tile
            case 5:
                {
                    tileType = Tile.MulATKPlus2;
                    OtherFunctions.ChangeSprite(Obj, Resource, 5);
                    break;
                }
            //Heart Points +5 Tile
            case 6:
                {
                    tileType = Tile.HPPlus5;
                    OtherFunctions.ChangeSprite(Obj, Resource, 8);
                    break;
                }
            //Command Points +1 Tile
            case 7:
                {
                    tileType = Tile.CPPlus1;
                    OtherFunctions.ChangeSprite(Obj, Resource, 9);
                    break;
                }
            //Command Tile
            case 8:
                {
                    tileType = Tile.CmdItem;
                    OtherFunctions.ChangeSprite(Obj, Resource, 10);
                    break;
                }
            //x2 Tile
            case 9:
                {
                    tileType = Tile.Mul2;
                    OtherFunctions.ChangeSprite(Obj, Resource, 11);
                    break;
                }
            //x3 Tile
            case 10:
                {
                    tileType = Tile.Mul3;
                    OtherFunctions.ChangeSprite(Obj, Resource, 12);
                    break;
                }
            //Element Swap Tile
            case 11:
                {
                    tileType = Tile.ElementSwap;
                    if (frameNum == -1) { frameIndex = Random.Range(13, 20); }
                    else { frameIndex = frameNum; }
                    OtherFunctions.ChangeSprite(Obj, Resource, frameIndex);
                    break;
                }
        }
    }
}
