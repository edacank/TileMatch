using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public int borderSize;
    public GameObject tilePrefab;
    public GameObject[] gamePiecePrefabs;
    public float  swapTime = 0.5f;
    Tile[,] allTiles;
    GamePiece[,] allGamePieces;

    public Tile clickedTile;
   public Tile targetTile;
    // Start is called before the first frame update
    void Start()
    {
        allTiles = new Tile[width, height];
        allGamePieces = new GamePiece[width, height];
        SetUpTiles();
        SetUpCamera();
        FillRandom();
    }

    
    void SetUpTiles()
    {
        for(int i =0; i < width; i++){
            for(int j=0; j < height; j++){
                GameObject tile= Instantiate(tilePrefab, new Vector3(i,j,0),Quaternion.identity) as GameObject;
                tile.name = "Tile(" + i +"," +j+ ")";
                allTiles [i,j] =tile.GetComponent<Tile>();
                tile.transform.parent =  transform;
                allTiles[i,j].Init(i,j,this);
            }
        }
    }
    void SetUpCamera()
    {
        Camera.main.transform.position= new Vector3((float)(width-1)/2f,(float)(height-1)/2f,-10f);
        float aspectRatio = (float) Screen.width /(float) Screen.height;
        float verticalSize = (float) height/2f +(float)borderSize;
        float horizontalSize = ( (float) width /2f + (float) borderSize ) /aspectRatio;
        Camera.main.orthographicSize =(verticalSize > horizontalSize) ? verticalSize: horizontalSize;
    }
    GameObject GetRandomPiece()
    {
        int randomIdx = Random.Range(0,gamePiecePrefabs.Length);
        if(gamePiecePrefabs[randomIdx] == null)
        {
            Debug.LogWarning("Board:"+randomIdx+"does not contain a valid GamePiece prefab");
        }
        return gamePiecePrefabs[randomIdx];
    }
   public void PlaceGamePiece(GamePiece gamePiece,int x,int y)
    {
        if(gamePiece == null)
        {
            Debug.LogWarning("Board: Invalid GamePiece!!!");
            return;
        }
        gamePiece.transform.position = new Vector3(x,y,0);
        gamePiece.transform.rotation = Quaternion.identity;
        if(IsWithinBounds(x,y))
        {   //corresponding
            allGamePieces[x,y] = gamePiece;
        }
        
        
        gamePiece.SetCoord(x,y);
    }
    bool IsWithinBounds(int x,int y)
    {
        return(x>= 0 && x< width && y>=0 && y<height);
    }
    void FillRandom()
    {
        for(int i=0; i<width;i++)
        {
            for(int j =0; j<height;j++)
            {
                GameObject randomPiece = Instantiate(GetRandomPiece(), Vector3.zero, Quaternion.identity) as GameObject;
                if(randomPiece != null)
                {
                    randomPiece.GetComponent<GamePiece>().Init(this);
                    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(),i,j);
                    randomPiece.transform.parent=transform;
                }
            }
        }
    }
    public void ClickTile(Tile tile)
    {
        //if
        if(clickedTile == null)
        {
            clickedTile =tile;
            Debug.Log("clickedTile:"+tile.name);
        }
    }
    public void DragToTile(Tile tile)
    {
        if(clickedTile !=null && IsNextTo(tile,clickedTile))
        {
            targetTile = tile;
        }
    }
    public void ReleaseTile()
    {
        if(clickedTile != null && targetTile != null)
        {
            SwitchTiles(clickedTile,targetTile);
        }
    }
    void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        GamePiece clickedPiece= allGamePieces[clickedTile.xIndex,clickedTile.yIndex];
        GamePiece targetPiece = allGamePieces[targetTile.xIndex,targetTile.yIndex];

        clickedPiece.Move(targetTile.xIndex,targetTile.yIndex,swapTime);
        targetPiece.Move(clickedTile.xIndex,clickedTile.yIndex,swapTime);
        //add code to switch corresponding GamePieces
        //clickedTile = null;
        //targetTile=null;
    }
    bool IsNextTo(Tile start, Tile end)
    {
        if(Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }
        if(Mathf.Abs(start.yIndex -end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }
        return false;
    }
}
