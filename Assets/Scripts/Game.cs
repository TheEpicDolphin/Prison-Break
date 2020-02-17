using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Globalization;


public class Game : MonoBehaviour
{
    public int round = -1;
    public int currentPlayer;
    public Player[3] players;
    public Player player { get => this.players[this.currentPlayer]}
    public int nMovesLeft = 3;
    public int timeLeft = 0;
    public final int maxTime = 300;  // game length (s)
    public DateTime startTime;


    // Start is called before the first frame update
    void Start()
    {   
        this.players = new Players[]{Player(), Player(), Player()}; 
        this.currentPlayer = this.players.Length - 1;
        this.startTime = DateTime.Now;
        this.initializeBoard()
        this.presentStartingScreen()
    }

    void initializeBoard() {
        Board()
	}

    void presentStartingScreen() {
        StartScreen()
	}

    void startGame() {
        this.nextTurn();
	}

    void endGame(int winner=-1) {
        EndPanel(winner=winner)
	}

    void startTransition() {
        this.transition.show(this.players[this.currentPlayer].name)
	}

    void endTransition() {
        this.transition.close()
	}

    void updateTimer() {
        this.timeLeft = this.maxTime - (DateTime.Now - this.startTime).TotalSeconds;
        if (this.timeLeft <= 0) {
            this.timerRanOut();
		}
	}

    void timerRanOut() {
        this.endGame(winner=-1);
	}

    void nextTurn() {
        this.nMovesLeft = 3;
        this.currentPlayer = (this.currentPlayer + 1) % this.players.Length;
        if (this.currentPlayer == 0) {
            this.round += 1;
        }
        moveCamera(this.player);  // follow the player
        this.updateView();
        if (this.player.extendedView) {
            this.extendView();
		}
        if (this.player.type == PlayerTypes.gunner) {
            this.watchButton.show();
            this.player.extendedView = false;
		}
        this.startTransition();
	}

    void watch()

    // Update is called once per frame
    void Update()
    {
        this.updateTimer()
    }

    
}
