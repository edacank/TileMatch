using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GamePiece : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    bool isMoving = false;
    public InterpType interpolation = InterpType.SmootherStep;
    Board m_board;
    public enum InterpType{
        Linear,
        EaseOut,
        EaseIn,
        SmoothStep,
        SmootherStep
    };
    public MatchValue matchValue;
    public enum MatchValue
    {
        BabyBlue,
        Pink,
        Yellow,
        Orange,
        Purple,
        Blue,
        Green,
        DarkPink

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
       /* if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move((int)transform.position.x +2, (int) transform.position.y, 0.5f);
        }
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move((int)transform.position.x -2, (int) transform.position.y, 0.5f);
        }*/
    }
    // Update is called once per frame
    public void Init(Board board)
    {
        
        m_board = board;
    }
   public void SetCoord(int x, int y)
    {
        xIndex =x;
        yIndex=y;

    }
    public void Move(int destX, int destY, float timeToMove)
    {
        if(!isMoving)
        { 
            StartCoroutine(MoveRoutine(new Vector3(destX,destY,0),timeToMove));
        }
       
    }
    IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        Vector3 startPosition = transform.position;
        bool reachedDestination= false;
        float elapsedTime = 0f;
        isMoving=true;
        while(!reachedDestination)
        {
            //almost there
            if(Vector3.Distance(transform.position,destination)< 0.01f)
            {
                reachedDestination = true;
                //round our position to the final destination on integer values
                //transform.position = destination;
                //set the xIndex and yIndex of the GamePiece
                //SetCoord((int) destination.x, (int)destination.y);
                //replace them with this
                if(m_board != null)
                {
                    m_board.PlaceGamePiece(this,(int)destination.x,(int) destination.y);
                }
                break;
            }
            //track the total running time
            elapsedTime += Time.deltaTime;
            //calculate the lerp value
            float t = Mathf.Clamp(elapsedTime/timeToMove,0f,1f);

            switch(interpolation)
            {
                case InterpType.Linear:
                    break;
                case InterpType.EaseOut:
                    t = Mathf.Sin(t*Mathf.PI*0.5f);
                    break;
                case InterpType.EaseIn:
                    t= 1-Mathf.Cos(t*Mathf.PI*0.5f);
                    break;
                case InterpType.SmoothStep:
                    t=t*t*(3-2*t);
                    break;
                case InterpType.SmootherStep:
                    t=t*t*t*(t*(t*6-15)+10);
                    break;
            }
            
           //move the game piece
            transform.position = Vector3.Lerp(startPosition,destination,t);
            //wait until nextframe
            yield return null;
        }
        isMoving =false;

    }
}
