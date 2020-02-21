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


    public int knifersLeft = 2;
    public Player[] players;
    public Gunner[] gunners;
    public Knifer[] knifers;
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
            foreach (Player player in players)
            {
                player.Setup();
            }
            //Start game loop
            gameLoop = StartCoroutine(GameLoop());
            gameStarted = true;
        }
    }

    IEnumerator GameLoop()
    {
        int i = 0;
        while (true)
        {
            waitFlags = 0b0000;
            currentPlayer = this.players[i];
            currentPlayer.ExecuteState();
            yield return new WaitUntil(() => waitFlags == 0b0001);
            i = (i + 1) % players.Length;
        }
    }

    public void NextTurn()
    {
        this.waitFlags = 0b0001;
    }

    public void EndGame(Player winner)
    {  
        //display current player as winner
        StopCoroutine(gameLoop);
        //Game ended
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


