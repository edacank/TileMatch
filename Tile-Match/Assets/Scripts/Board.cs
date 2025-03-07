using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public int borderSize;
    public GameObject tileNormalPrefab;
    public GameObject tileObstaclePrefab;
    public GameObject[] gamePiecePrefabs;
    public float  swapTime = 0.5f;
    Tile[,] allTiles;
    GamePiece[,] allGamePieces;

    public Tile clickedTile;
   public Tile targetTile;
    bool playerInputEnabled= true;
    public StartingTile[] startingTiles;
    [System.Serializable]
    public class StartingTile{
        public GameObject tilePrefab;
        public int x;
        public int y;
        public int z;
    }
    void Start()
    {
        allTiles = new Tile[width, height];
        allGamePieces = new GamePiece[width, height];
        SetUpTiles();
        SetUpCamera();
        FillBoard(10, 0.5f);
        //HighlightMatches();
        //ClearPieceAt(1,4);
       // ClearPieceAt(3,5);
    }

    
    void SetUpTiles()
    {
        foreach(StartingTile sTile in startingTiles)
        {
            if(sTile != null)
            {
                MakeTile(sTile.tilePrefab, sTile.x, sTile.y, sTile.z);
            }
        }
        for(int i =0; i < width; i++)
        {
            for(int j=0; j < height; j++)
            {
                if(allTiles[i,j] == null)
                {
                MakeTile(tileNormalPrefab, i, j);
                }
            }
        }
    }

     void MakeTile(GameObject prefab, int x, int y,int z = 0)
    {
        if(prefab != null)
        {
            GameObject tile = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
            tile.name = "Tile(" + x + "," + y + ")";
            allTiles[x, y] = tile.GetComponent<Tile>();
            tile.transform.parent = transform;
            allTiles[x, y].Init(x,y, this);
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
    void FillBoard(int falseYOffSet = 0,float moveTime=0.1f)
    {
        int maxIterations =100;
        int iterations = 0;
        for(int i=0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            { 
                if(allGamePieces[i,j] == null && allTiles[i,j].tileType != TileType.Obstacle)
                {
                    GamePiece piece = FillRandomAt(i, j,falseYOffSet,moveTime);
                    iterations = 0;
                    while(HasMatchOnFill(i,j))
                    {
                        ClearPieceAt(i,j);
                        piece=FillRandomAt(i,j,falseYOffSet,moveTime);
                        iterations++;
                        if(iterations >= maxIterations)
                        {
                             Debug.Log("break---------");
                             break;
                        }
                    }
                }
            }
        }
    }

    GamePiece FillRandomAt(int x, int y, int falseYOffSet =0, float moveTime=0.1f)
    {
        GameObject randomPiece = Instantiate(GetRandomPiece(), Vector3.zero, Quaternion.identity) as GameObject;
        if (randomPiece != null)
        {
            randomPiece.GetComponent<GamePiece>().Init(this);
            PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), x, y);
            if(falseYOffSet !=0)
            {
                randomPiece.transform.position = new Vector3(x,y+falseYOffSet,0);
                randomPiece.GetComponent<GamePiece>().Move(x,y,moveTime);
            }
            randomPiece.transform.parent = transform;
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }

    bool  HasMatchOnFill(int x, int y, int minLength =3)
    {
        List<GamePiece> leftMatches = FindMatches(x,y, new Vector2(-1,0),minLength);
        List<GamePiece> downwardMatches = FindMatches(x,y, new Vector2(0,-1),minLength);
        if(leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }
        if(downwardMatches == null)
        {
            downwardMatches= new List<GamePiece>();
        }
        return (leftMatches.Count> 0 || downwardMatches.Count> 0);
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
        clickedTile = null;
        targetTile = null;
    }
    void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile,targetTile));
    }
    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
     if(playerInputEnabled)
     {


        
          GamePiece clickedPiece= allGamePieces[clickedTile.xIndex,clickedTile.yIndex];
          GamePiece targetPiece = allGamePieces[targetTile.xIndex,targetTile.yIndex];
          if(targetPiece !=null && clickedPiece !=null)
          {
             clickedPiece.Move(targetTile.xIndex,targetTile.yIndex,swapTime);
             targetPiece.Move(clickedTile.xIndex,clickedTile.yIndex,swapTime);
             //add small pause in here for swapTime seconds
             yield return new WaitForSeconds(swapTime);
            List<GamePiece> clickedPieceMatches =FindMatchesAt(clickedTile.xIndex,clickedTile.yIndex);
            List<GamePiece> targetPieceMatches =FindMatchesAt(targetTile.xIndex,targetTile.yIndex);
            //if they do not match remove them to their first positions.
            if(targetPieceMatches.Count == 0 &&  clickedPieceMatches.Count == 0)
            {
             clickedPiece.Move(clickedTile.xIndex,clickedTile.yIndex,swapTime);
                targetPiece.Move(targetTile.xIndex,targetTile.yIndex,swapTime);
            }
            // yield return new WaitForSeconds(swapTime);
            else
            {
                 yield return new WaitForSeconds(swapTime);
                 ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
           // ClearPieceAt(clickedPieceMatches);
           // ClearPieceAt(targetPieceMatches);

            //CollapseColumn(clickedPieceMatches);
            //CollapseColumn(targetPieceMatches);
           // HighlightMatchesAt(clickedTile.xIndex,clickedTile.yIndex);
           // HighlightMatchesAt(targetTile.xIndex,targetTile.yIndex);
            //add code to switch corresponding GamePieces
            //clickedTile = null;

            }
          }
        
            
    }
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
   
   //general search method; specify a starting coordinate (startX,startY) and use a Vector2 for direction 
   //i.e (1,0) -->right (-1,0) --> left (0,1) --> up (0,-1) --> down
   //minLength is the min number to be considered a match
    List<GamePiece> FindMatches(int startX,int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece =null;

        if(IsWithinBounds(startX,startY))
        {
            startPiece = allGamePieces[startX,startY];
        }
        if(startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }
        int nextX;
        int nextY;
        int maxValue =(width > height) ? width: height;
        for(int i=1; i < maxValue -1; i++)
        {
            nextX = startX +(int) Mathf.Clamp(searchDirection.x,-1,1) * i;
            nextY = startY + (int) Mathf.Clamp(searchDirection.y,-1,1) * i;

            if(!IsWithinBounds(nextX,nextY))
            {
                break;
            }
            GamePiece nextPiece = allGamePieces[nextX,nextY];
            //???
            if(nextPiece == null)
            {
                break;
            }
            else
            {
                if(nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece)) 
                {
                    matches.Add(nextPiece);
                }
                else
                {
                   break;
                }
            }

            
        }
            if(matches.Count >= minLength)
            {
                return matches;
            }
            return null;
        
    }

    List<GamePiece> FindVerticalMatches(int startX,int startY,int minLength = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX,startY, new Vector2(0,1),2);
        List<GamePiece> downwardMatches = FindMatches(startX,startY, new Vector2(0,-1),2);
        
        if(upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }
        if(downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }
        /*
        foreach(GamePiece piece in downwardMatches)
        {
            if(!upwardMatches.Contains(piece))
            {
                upwardMatches.Add(piece);
            }

        }*/
       // return (upwardMatches.Count >= minLength) ? upwardMatches : null;
        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();
        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    
    
    }
    List<GamePiece> FindHorizontalMatches(int startX,int startY,int minLength = 3)
    {
        List<GamePiece> rightMatches = FindMatches(startX,startY, new Vector2(1,0),2);
        List<GamePiece> leftMatches = FindMatches(startX,startY, new Vector2(-1,0),2);
        
        if(rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }
        if(leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }
        
        var combinedMatches = rightMatches.Union(leftMatches).ToList();
        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    
    
    }
    List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();
        for(int i =0; i<width;i++)
        {
            for(int j = 0; j<height; j++)
            {
                List<GamePiece> matches = FindMatchesAt(i,j);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }
        return combinedMatches;
    }
    void HighlightTileOff(int x,int y)
    {
        if(allTiles[x,y].tileType != TileType.Breakable){
             SpriteRenderer spriteRenderer = allTiles[x, y].GetComponent<SpriteRenderer>();
             spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }
    }
    void HighlightTileOn(int x, int y , Color col)
    {
       SpriteRenderer spriteRenderer = allTiles[x,y].GetComponent<SpriteRenderer>();
       spriteRenderer.color = col;
    }

    void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x,y);
                
                var combinedMatches = FindMatchesAt(x, y);
                if (combinedMatches.Count > 0)
                {
                    foreach (GamePiece piece in combinedMatches)
                    {
                       HighlightTileOn(piece.xIndex,piece.yIndex,piece.GetComponent<SpriteRenderer>().color);
                    }
                }
    }


    void HighlightMatches()
    {
        for(int i =0; i< width; i++)
        {
            for(int j=0; j< height; j++)
            {
                HighlightMatchesAt(i,j);
            }
        }
    }
void HighlightPieces(List<GamePiece> gamePieces)
{
    foreach(GamePiece piece in gamePieces)
    {
        if(piece != null)
        {
            HighlightTileOn(piece.xIndex,piece.yIndex,piece.GetComponent<SpriteRenderer>().color);
        }
    }
}
    List<GamePiece> FindMatchesAt(int x, int y, int minLength=3)
    {
        List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLength);
        List<GamePiece> vertMatches = FindVerticalMatches(x, y, minLength);
        if (horizMatches == null)
        {
            horizMatches = new List<GamePiece>();
        }

        if (vertMatches == null)
        {
            vertMatches = new List<GamePiece>();
        }
        //always returns a list, return value is not nullable.
        var combinedMatches = horizMatches.Union(vertMatches).ToList();
        return combinedMatches;
    }
    List<GamePiece>FindMatchesAt(List<GamePiece> gamePieces, int minLength=3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        foreach(GamePiece piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
        }
        return matches;
    }
void ClearPieceAt(int x, int y)
{
    GamePiece pieceToClear = allGamePieces[x,y];
    if(pieceToClear != null)
    {
        allGamePieces[x,y]=null;
        Destroy(pieceToClear.gameObject);
    }
    HighlightTileOff(x,y);
}
void BreakTile(int x, int y)
{
    Tile tileToBreak = allTiles[x,y];
    if(tileToBreak != null)
    {
        tileToBreak.BreakTile();
    }

}
void BreakTileAt(List<GamePiece> gamePieces)
{
    foreach(GamePiece piece in gamePieces)
    {
        if(piece != null)
        {
            BreakTile(piece.xIndex, piece.yIndex);
        }
    }

}
void ClearBoard()
{
    for(int i =0; i<width;i++)
    {
        for(int j=0; j<height;j++)
        {   
            ClearPieceAt(i,j);

        }
    }
}

void ClearPieceAt(List<GamePiece> gamePieces)
{
    foreach(GamePiece piece in gamePieces)
    {
        if(piece != null){
            ClearPieceAt(piece.xIndex, piece.yIndex);
        }
        ClearPieceAt(piece.xIndex,piece.yIndex);
    }

}
List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
{
    List<GamePiece> movingPieces = new List<GamePiece>();
    for(int i=0; i < height -1; i++)
    {
        if(allGamePieces[column,i] == null && allTiles[column,i].tileType != TileType.Obstacle)
        {
            for(int j = i+1; j<height; j++ )
            {
                if(allGamePieces[column,j] != null)
                {
                    allGamePieces[column,j].Move(column,i,collapseTime*(j-i));
                    allGamePieces[column, i] =allGamePieces[column,j];
                    allGamePieces[column,i].SetCoord(column,i);
                    if(!movingPieces.Contains(allGamePieces[column,i]))
                    {
                        movingPieces.Add(allGamePieces[column, i]);
                    }
                    allGamePieces[column,j] = null;
                    break;
                }
            }
        }
    }
    return movingPieces;
}
List<int> GetColumns(List<GamePiece> gamePieces)
{
    List<int> columns = new List<int>();
    foreach(GamePiece piece in gamePieces)
    {
        if(!columns.Contains(piece.xIndex))
        {
            columns.Add(piece.xIndex);
        }
    }
    return columns;
}
List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
{
    List<GamePiece> movingPieces = new List<GamePiece>();
    List<int> columnsToCollapse = GetColumns(gamePieces);
    foreach(int column in columnsToCollapse)
    {
        movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
    }
    return movingPieces;
}

void ClearAndRefillBoard(List<GamePiece> gamePieces)
{
    StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
}
IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
{
    
    playerInputEnabled = false;
    List<GamePiece> matches = gamePieces;
    do{
    //clear and collapse
    yield return StartCoroutine(ClearAndCollapseRoutine(gamePieces));
    yield return null;
    //refill
    yield return StartCoroutine(RefillRoutine());
    
    matches = FindAllMatches();
    yield return new WaitForSeconds(0.5f);
    }
    while(matches.Count != 0);
    playerInputEnabled = true;
}
IEnumerator RefillRoutine()
{
    FillBoard(10,0.5f);
    yield return null;
}
IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
{
    List<GamePiece> movingPieces = new List<GamePiece>();
    List<GamePiece> matches = new List<GamePiece>();
    HighlightPieces(gamePieces);

    yield return new WaitForSeconds(0.25f);
    bool isFinished = false;
    while(!isFinished)
    {
        ClearPieceAt(gamePieces);
        BreakTileAt(gamePieces);

        yield return new WaitForSeconds(0.25f);
        movingPieces = CollapseColumn(gamePieces);
        //
        while(!IsCollapsed(movingPieces))
        {
            yield return null;

        }
        yield return new WaitForSeconds(0.2f);
        matches = FindMatchesAt(movingPieces);
        if(matches.Count == 0)
        {
            isFinished = true;
            break;
        }
        else 
        {
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
        }
    }
    yield return null;
}
bool IsCollapsed(List<GamePiece> gamePieces)
{
    foreach(GamePiece piece in gamePieces)
    {
        if(piece != null)
        {
            //if the piece is not null we check if the piece has reached its destination.
            if(piece.transform.position.y -(float)piece.yIndex > 0.001f)
            {
                return false;
            }
        }
    }
    return true;
}
}

