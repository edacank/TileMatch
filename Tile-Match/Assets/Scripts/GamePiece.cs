using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move((int)transform.position.x +1, (int) transform.position.y, 0.5f);
        }
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move((int)transform.position.x -1, (int) transform.position.y, 0.5f);
        }
    }
    // Update is called once per frame
   public void SetCoord(int x, int y)
    {
        xIndex =x;
        yIndex=y;

    }
    public void Move(int destX, int destY, float timeToMove)
    {
        StartCoroutine(MoveRoutine(new Vector3(destX,destY,0),timeToMove));
    }
    IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        Vector3 startPosition = transform.position;
        bool reachedDestination= false;
        float elapsedTime = 0f;
        while(!reachedDestination)
        {
            //almost there
            if(Vector3.Distance(transform.position,destination)< 0.01f)
            {
                reachedDestination = true;
                transform.position = destination;
            }
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime/timeToMove,0f,1f);
            transform.position = Vector3.Lerp(startPosition,destination,t);

            yield return null;
        }

    }
}
