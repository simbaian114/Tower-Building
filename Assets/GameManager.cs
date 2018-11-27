﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour {

    public static GameManager Instance;

    public GameObject BlockPrefab;
    public GameObject FirstBlock;
    public List<GameObject> Blocks = new List<GameObject>();
    public Stack<GameObject> RecoverBlocks =  new Stack<GameObject>();
    public Vector3 CurrentCenter;
    public Vector3 CurrentScale;
    public Vector3 CurrentMoveDirection;
    public int Score = 0;
    public int Combo = 0;
    public GameObject MovingBlock;
    public GameObject RecoverSample;
    public GameObject StartButton;
    public bool isFail = false;
    public Text ScoreText;

    void Awake()
    {
            Instance = this;
    }
    void Start ()
    {
        StartCoroutine(GameLoop());
	}
	
    void Init()
    {
        CurrentCenter = Vector3.up;
        CurrentScale = FirstBlock.transform.localScale;
        CurrentMoveDirection = Vector3.forward;
        RecoverSample = FirstBlock;

        Blocks.Add(FirstBlock);
        RecoverBlocks.Push(FirstBlock);
    }

    void SpawnBlock()
    {
        GameObject temp = Instantiate(BlockPrefab, CurrentCenter, Quaternion.identity);
        temp.GetComponent<Blockbehavior>().Init(CurrentCenter, CurrentMoveDirection, CurrentScale);
        MovingBlock = temp;
    }
    void CutBlockForward(GameObject newBlock, GameObject oldBlock)
    {
        Vector3 oldCenter = oldBlock.transform.position;
        Vector3 oldScale = oldBlock.transform.localScale;
        Vector3 newCenter = newBlock.transform.position;
        Vector3 newScale = newBlock.transform.localScale;

        float offset = (newCenter - oldCenter).z;

        if(Mathf.Abs(offset) > oldScale.z) //fail
        {
            newBlock.AddComponent<Rigidbody>();
            newBlock.GetComponent<Blockbehavior>().Moving = false;
            isFail = true;
            return;
        }
        
        if(Mathf.Abs(offset)<0.5f) //within error range
        {
            MovingBlock.GetComponent<Blockbehavior>().Moving = false;
            MovingBlock.transform.position = CurrentCenter;
            Blocks.Add(MovingBlock);
            CurrentCenter += Vector3.up;
            Score++;
            Combo++;

            AudioManager.Instance.PlayComboSound(Combo);
            //Do perfect tap effect
            PerfectHitManager.Instance.ShowNormalHitEffect(Blocks[Score].transform.position - Vector3.up * 0.5f, Blocks[Score].transform.localScale);
            if (Combo >= 3)
            {
                int effectCount = Mathf.Min(Combo - 2, 3);
                StartCoroutine(PerfectHitManager.Instance.ShowPerfectCombo
                    (
                        effectCount, Blocks[Score].transform.position - Vector3.up * 0.5f, Blocks[Score].transform.localScale
                    ));
            }


            return;
        }


        Vector3 ACenter = (oldCenter + newCenter) / 2f + Vector3.up * 0.5f;
        Vector3 AScale = new Vector3(oldScale.x, 1f, (oldScale.z - Mathf.Abs(offset)));

        Vector3 BCenter = Vector3.zero;

        if(offset > 0)
        {
            BCenter = oldCenter + Vector3.up + Vector3.forward * oldScale.z / 2f + Vector3.forward * offset / 2f;
        }
        if(offset <0)
        {
            BCenter = oldCenter + Vector3.up - Vector3.forward * oldScale.z / 2f + Vector3.forward * offset / 2f;
        }

        Vector3 BScale = new Vector3(oldScale.x, 1f, Mathf.Abs(offset));

        newBlock.transform.position = ACenter;
        newBlock.transform.localScale = AScale;
        newBlock.GetComponent<Blockbehavior>().Moving = false;

        GameObject b = Instantiate(BlockPrefab, BCenter, Quaternion.identity);
        b.transform.localScale = BScale;
        b.AddComponent<Rigidbody>();

        Blocks.Add(newBlock);
        RecoverBlocks.Push(newBlock);
        CurrentCenter = newBlock.transform.position + Vector3.up;
        CurrentScale = newBlock.transform.localScale;

        Score++;
        Combo = 0;
    }
    
    void CutBlockRight(GameObject newBlock,GameObject oldBlock)
    {
        Vector3 oldCenter = oldBlock.transform.position;
        Vector3 oldScale = oldBlock.transform.localScale;
        Vector3 newCenter = newBlock.transform.position;
        Vector3 newScale = newBlock.transform.localScale;

        float offset = (newCenter - oldCenter).x;

        if (Mathf.Abs(offset) > oldScale.x) //fail
        {
            newBlock.AddComponent<Rigidbody>();
            newBlock.GetComponent<Blockbehavior>().Moving = false;
            isFail = true;
            return;
        }
        if (Mathf.Abs(offset) < 0.5f) //within error range
        {
            MovingBlock.GetComponent<Blockbehavior>().Moving = false;
            MovingBlock.transform.position = CurrentCenter;
            Blocks.Add(MovingBlock);
            CurrentCenter += Vector3.up;
            Score++;
            Combo++;

            AudioManager.Instance.PlayComboSound(Combo);
            //Do perfect tap effect
            PerfectHitManager.Instance.ShowNormalHitEffect(Blocks[Score].transform.position - Vector3.up * 0.5f, Blocks[Score].transform.localScale);
            if (Combo >= 3)
            {
                int effectCount = Mathf.Min(Combo - 2, 3);
                StartCoroutine(PerfectHitManager.Instance.ShowPerfectCombo
                    (
                        effectCount, Blocks[Score].transform.position - Vector3.up * 0.5f, Blocks[Score].transform.localScale
                    ));
            }

            return;
        }
        Vector3 ACenter = (oldCenter + newCenter) / 2f + Vector3.up * 0.5f;
        Vector3 AScale = new Vector3(oldScale.x, 1f, (oldScale.z - Mathf.Abs(offset)));

        Vector3 BCenter = Vector3.zero;

        if (offset > 0)
        {
            BCenter = oldCenter + Vector3.up + Vector3.right * oldScale.x / 2f + Vector3.right * offset / 2f;
        }
        if (offset < 0)
        {
            BCenter = oldCenter + Vector3.up - Vector3.right * oldScale.x / 2f + Vector3.right * offset / 2f;
        }

        Vector3 BScale = new Vector3(Mathf.Abs(offset), 1f, oldScale.z);

        newBlock.transform.position = ACenter;
        newBlock.transform.localScale = AScale;
        newBlock.GetComponent<Blockbehavior>().Moving = false;

        GameObject b = Instantiate(BlockPrefab, BCenter, Quaternion.identity);
        b.transform.localScale = BScale;
        b.AddComponent<Rigidbody>();

        Blocks.Add(newBlock);
        RecoverBlocks.Push(newBlock);
        CurrentCenter = newBlock.transform.position + Vector3.up;
        CurrentScale = newBlock.transform.localScale;

        Score++;
        Combo = 0;
    }

    void ChangeBlockDirection()
    {
        if (CurrentMoveDirection == Vector3.forward)
            CurrentMoveDirection = Vector3.right;
        else
            CurrentMoveDirection = Vector3.forward;      
    }

    public void RecoverBlock()
    {
        if(RecoverBlocks.Count !=0)
        {
            CurrentScale = RecoverBlocks.Peek().transform.localScale;
            CurrentCenter = new Vector3
                (
                    RecoverBlocks.Peek().transform.position.x,
                    CurrentCenter.y,
                    RecoverBlocks.Peek().transform.position.z
                );
            StartCoroutine(
                MovingBlock.GetComponent<Blockbehavior>().Recover
                (
                    RecoverBlocks.Peek().transform.localScale,
                    new Vector3
                    (
                        RecoverBlocks.Peek().transform.position.x,
                        MovingBlock.transform.position.y,
                        RecoverBlocks.Peek().transform.position.z
                    )
                 )

            );

            RecoverBlocks.Pop();        
        }
        else
        {
            CurrentScale = FirstBlock.transform.localScale;
            CurrentCenter = new Vector3
                (
                    FirstBlock.transform.position.x,
                    CurrentCenter.y,
                    FirstBlock.transform.position.z
                );
            StartCoroutine(
                MovingBlock.GetComponent<Blockbehavior>().Recover
                (
                    FirstBlock.transform.localScale,
                    new Vector3
                    (
                        FirstBlock.transform.position.x,
                        MovingBlock.transform.position.y,
                        FirstBlock.transform.position.z
                    )
                )
             );

        }
    }
    
    public void Restart()
    {
        print("Restart");
    }
        
    IEnumerator GameLoop()
    {
       
        Init();

        AudioManager.Instance.PlayStartSound();

        yield return new WaitUntil(() => !StartButton.activeInHierarchy);

        while (!isFail)
        {

            SpawnBlock();

            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

            CameraManager.Instance.GoUp();

            if (CurrentMoveDirection == Vector3.forward)
                 CutBlockForward(MovingBlock,Blocks[Score]);
            if (CurrentMoveDirection == Vector3.right)
                 CutBlockRight(MovingBlock, Blocks[Score]);

            if (isFail)
                 break;

            ChangeBlockDirection(); 

            if (Combo >= 5)
                 RecoverBlock();

            yield return null;
            // if combo >= 5
                // recover block
        }

        //Print Fail UI

    }
	
	void Update ()
    {
        ScoreText.text = Score.ToString();
	}
}
