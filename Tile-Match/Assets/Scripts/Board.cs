using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject tilePrefab;
    Tile[,] allTiles;
    // Start is called before the first frame update
    void Start()
    {
        allTiles = new Tile[width, height];
        SetUpTiles();
    }

    
    void SetUpTiles(){
        for(int i =0; i < width; i++){
            for(int j=0; j < height; j++){
                GameObject tile= Instantiate(tilePrefab, new Vector3(i,j,0),Quaternion.identity) as GameObject;
                tile.name = "Tile(" + i +"," +j+ ")";
                allTiles [i,j] =tile.GetComponent<Tile>();
                tile.transform.parent =  transform;
            }
        }
    }
}
