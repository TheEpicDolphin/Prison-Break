using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Globalization;
using UnityEngine.UI;


public class Game : MonoBehaviour
{
    private static Game _instance;
    public static Game Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    //public Gunner[] gunners;
    //public Knifer[] knifers;
    public List<Gunner> gunners = new List<Gunner>();
    public List<Knifer> knifers = new List<Knifer>();

    bool gameStarted = false;
    public Player currentPlayer;

    public GameObject watchButton;
    public GameObject stayButton;

    public GameObject rightButton;
    public GameObject upButton;
    public GameObject leftButton;
    public GameObject downButton;

    public List<Tile> tileButtons;

    Coroutine gameLoop;

    uint waitFlags;

    private void Start()
    {
        watchButton.GetComponent<Button>().interactable = false;
        stayButton.GetComponent<Button>().interactable = false;
        rightButton.GetComponent<Button>().interactable = false;
        upButton.GetComponent<Button>().interactable = false;
        leftButton.GetComponent<Button>().interactable = false;
        downButton.GetComponent<Button>().interactable = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A) && !gameStarted)
        {
            foreach(Gunner gunner in gunners)
            {
                gunner.Setup();
            }

            foreach (Knifer knifer in knifers)
            {
                knifer.Setup();
            }
            //Start game loop
            gameLoop = StartCoroutine(GameLoop());
            gameStarted = true;
        }
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            
            foreach (Gunner gunner in gunners)
            {
                waitFlags = 0b0000;
                currentPlayer = gunner;
                Debug.Log(currentPlayer.gameObject.name + " starting turn...");
                currentPlayer.ExecuteState();
                yield return new WaitUntil(() => waitFlags == 0b0001);
            }

            foreach (Knifer knifer in knifers)
            {
                waitFlags = 0b0000;
                currentPlayer = knifer;
                Debug.Log(currentPlayer.gameObject.name + " starting turn...");
                currentPlayer.ExecuteState();
                yield return new WaitUntil(() => waitFlags == 0b0001);
            }
        }
    }

    public void NextTurn()
    {
        this.waitFlags = 0b0001;
    }

    public void EndGame(string winner)
    {  
        //display current player as winner
        StopCoroutine(gameLoop);
        Debug.Log(winner + " have won!");
        //Game ended
    }

    public void CheckIfAllGunnersDead()
    {
        int numGunners = gunners.Count;
        int gunnersDead = 0;
        foreach(Gunner gunner in gunners)
        {
            if(gunner.curState == PlayerState.Dead)
            {
                gunnersDead += 1;
            }
        }
        if(gunnersDead == numGunners)
        {
            EndGame("Knifers");
        }
    }

    public void CheckIfAllKnifersDead()
    {
        int numKnifers = knifers.Count;
        int knifersDead = 0;
        foreach (Knifer knifer in knifers)
        {
            if (knifer.curState == PlayerState.Dead)
            {
                knifersDead += 1;
            }
        }
        if (knifersDead == numKnifers)
        {
            EndGame("Gunners");
        }
    }

    /*
    move(tileLoc)
        If tileLoc in Board.getValidMoves(self.Player[playerTurn]): player.move(tileLoc), updateView(), self.movesLeft -= 1 else raise error
        For i, player in enumerate(self.players):
        If i != self.playerTurn and self.players[self.playerTurn].loc == player.loc: end_game(winner=self.playerTurn
        For i, player in enumerate(self.players):
        If player.extendedView and 
        If self.movesLeft == 0: self.showEndTurn()
        showEndTurn()
        self.endTurnButton.show()
    */


}


