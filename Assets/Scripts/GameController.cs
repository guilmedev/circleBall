using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    [SerializeField]
    private int _record;

    [SerializeField]
    private int _bestRecord;

    private const string BESTSCORESTRING = "BEST_RECORD";

    public int BestRecord
    {
        get
        {
            return _bestRecord;
        }

        private set
        {
            _bestRecord = value;
            PlayerPrefs.SetInt( BESTSCORESTRING , _bestRecord);
            PlayerPrefs.Save();
        }
    }


    public int Record
    {
        get
        {
            return _record;
        }
        
        private set
        {
            _record = value;
        }
    }

    [Header("RULES")]
    public int ballQuantity = 40;
    [Range( .02f , .15f)]
    public float rotateSpeed = 0.032f;
    private float initialRotateSpeed;

    [SerializeField]
    public int levelChangeStep = 3;

    [Header("UI")]
    public Text score;
    public Text bestScore;
    public GameObject FASTER;



    [Header("MENUS")]
    public GameObject menuCanvas;
    public GameObject gamePlayCanvas;
    

    public Button actionButton;
    //public GameObject skinCanvas;
    //public GameObject recordCanvas;



    [Header("GAMEPLAY")]
    public GameObject gameView;
    public GameObject lightBallPrefab;
    private List<GameObject> balls = new List<GameObject>();

    [Header("AUDIO")]
    public AudioSource soundTrack;
    public AudioSource VFXTrack;

    public AudioClip buttonEffect;
    public AudioClip winVFX;
    public AudioClip LostVFX;
    public AudioClip loadCircleVFX;




    // Start is called before the first frame update
    void Start()
    {
        //Generate Balls
        GenereateBalls(ballQuantity);
        Record = 0; // fisrt time
        initialRotateSpeed = rotateSpeed;
    }

    //// Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        currentBall = 0;
    //        CheckCurrentBall();
    //    }
    //}

    private void StartGame()
    {
        //once
        BestRecord = PlayerPrefs.GetInt(BESTSCORESTRING, 0);

        UpdateBestScore(BestRecord);

        StartCoroutine (IGenerateCircle(  ()=> {
            StartRotating();
            actionButton.interactable = true; 
        }) );
    }


    #region MENU CALLBACKS


    public void PlayButton()
    {

        PlayVFX(buttonEffect);

        menuCanvas?.SetActive(false);
        gamePlayCanvas?.SetActive(true);
        gameView?.SetActive(true);

        StartGame();

    }

    public void BackButton()
    {
        PlayVFX(buttonEffect);

        StopAllCoroutines();
        menuCanvas?.SetActive(true);
        gamePlayCanvas?.SetActive(false);
        gameView?.SetActive(false);
    }

    public void ActionButton()
    {
        CheckCurrentBall();
    }

    #endregion

    #region GAMEPLAY

    private void UpdateScore(int value)
    {
        if(score)
            score.text = value.ToString();
    }

    private void UpdateBestScore(int value)
    {
        if (bestScore)
            bestScore.text = value.ToString();
    }

    private void StartRotating()
        {
            UpdateScore(Record);
            UpdateBestScore(PlayerPrefs.GetInt(BESTSCORESTRING));
            ChekLevel(Record);
            StartCoroutine(IFlashBall());
        }

    private void ChekLevel(int value)
    {
        if( Record % levelChangeStep == 0 && Record > 1)
        {
            if(rotateSpeed <= .02f)           
                rotateSpeed = .02f;
            else
                rotateSpeed -= 0.005f;

            PlayVFX(loadCircleVFX);

            if (FASTER)
                FASTER.SetActive(true);
        }
        else
        {
            if (FASTER)
                FASTER.SetActive(false);
        }
    }

    

    

        int currentBall;
        private void CheckCurrentBall()
        {

            StopAllCoroutines();
            actionButton.interactable = false; //prevent double click

            if (currentBall == 0)
            {
            //Debug.Log("YOU WIN");
            PlayVFX(winVFX);
                Record++;

                if(Record > BestRecord)
                {
                    //log new record
                    BestRecord++;
                }

                StartCoroutine( WinRountine( ()=> {
                    ResetBallColors();
                    actionButton.interactable = true;
                    StartRotating();
                } ) );
            }
            else
            {
                //Debug.Log("YOU LOSE");
                rotateSpeed = initialRotateSpeed;
                Record = 0;
                PlayVFX(LostVFX);
                StartCoroutine( LooseRountine( RestartGame ) );
            }

        }

        private void RestartGame()
        {
            //after win or lost
            StartGame();
        }

    private IEnumerator LooseRountine( Action onComplete )
        {
            yield return new WaitForSeconds(.5f);

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < balls.Count; j++)
                {
                    balls[j].GetComponent<SpriteRenderer>().color = Color.red;
                }
                yield return new WaitForSeconds(.25f);
                for (int j = 0; j < balls.Count; j++)
                {
                    balls[j].GetComponent<SpriteRenderer>().color = Color.white;
                }
                yield return new WaitForSeconds(.25f);
            }

            //Celebratrion DONE
            onComplete();
        }

        private IEnumerator WinRountine( Action onComplete )
        {
            yield return new WaitForSeconds(.5f);

            for (int i=0; i< 3; i++)
            {
                for (int j = 0; j < balls.Count; j++)
                {
                     balls[ j ].GetComponent<SpriteRenderer>().color = Color.green;                
                }
                yield return new WaitForSeconds(.5f);
                for (int j = 0; j < balls.Count; j++)
                {                
                    balls[j].GetComponent<SpriteRenderer>().color = Color.white;
                }
                yield return new WaitForSeconds(.5f);
            }

            //Celebratrion DONE
            onComplete();
        }

        IEnumerator IFlashBall()
        {

            for( int i = 0; i < balls.Count; i++)
            {
                currentBall = i;

                if( i == 0 && currentBall == 0 )
                    balls[0].GetComponent<SpriteRenderer>().color = Color.yellow;
                else
                    balls[0].GetComponent<SpriteRenderer>().color = Color.red;

                if( i != 0 )
                    balls[ i ].GetComponent<SpriteRenderer>().color = Color.green;           

                yield return new WaitForSeconds( rotateSpeed );
                balls[ i ].GetComponent<SpriteRenderer>().color = Color.white;
            }

            StartCoroutine(IFlashBall());
        }


        private IEnumerator IGenerateCircle( Action onComplete )
        {

            ResetBallsPosition();

            

            float angle = 360f / balls.Count;

            //Vector3 enemyPos = emptyEnemy.transform.position;

            for (int i = 0; i < balls.Count; i++)
            {
                //GameObject newBall = Instantiate(lightBallPrefab, gameView.transform.position * (quantity / 2), Quaternion.identity);
                GameObject newBall = balls[i];
                newBall.transform.position = 
                new Vector3(gameView.transform.position.x , 
                gameView.transform.position.y , 
                gameView.transform.position.z) 
                * ( balls.Count / 2) ;

                newBall.SetActive(true);

                gameView?.transform.RotateAround(gameView.transform.position, Vector3.forward, angle );
                yield return new WaitForSeconds(.015f);
            }

            balls[0].GetComponent<SpriteRenderer>().color = Color.red;

            //Debug.Log("START GAME");
            onComplete();
        }

    private void GenereateBalls(int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            GameObject newBall = Instantiate( lightBallPrefab, gameView.transform.position , Quaternion.identity );
            newBall.transform.SetParent(gameView?.transform);
            newBall.gameObject.name = "Ball" + i;
            newBall.SetActive(false);
            balls.Add(newBall);           
        }
    }

    private void ResetBallColors()
    {
        for (int i = 0; i < balls.Count; i++)
        {
            if(i == 0)
            {
                balls[i].GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            balls[i].GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
    private void ResetBallsPosition()
    {
        for (int i = 0; i < balls.Count; i++)
        {
            balls[ i ].transform.position =
            new Vector3(gameView.transform.position.x, gameView.transform.position.y, gameView.transform.position.z );

            balls[i].GetComponent<SpriteRenderer>().color = Color.white;
            balls[i].SetActive(false);
        }
    }

    #endregion


    #region AUDIO

    public void PlayVFX ( AudioClip clip)
    {
        VFXTrack.PlayOneShot(clip);
    }

    public void ToggleAudio(bool isMute)
    {
        isMute = !isMute;

        soundTrack.mute = isMute;
        VFXTrack.mute   = isMute;
    }
    #endregion
}
