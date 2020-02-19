using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Globalization;


public class Game : MonoBehaviour
{
    public int round = -1;
    public int currentPlayer;
    public int nPlayers = 3;
    public Player[] players = new Player[nPlayers]();
    public Player player { get => this.players[this.currentPlayer]}
    public int nMovesLeft = 3;
    public int timeLeft = 0;
    public final int maxTime = 300;  // game length (s)
    public DateTime startTime;


    // Start is called before the first frame update
    void Start()
    {   
        this.currentPlayer = this.players.Length - 1;
        this.startTime = DateTime.Now;
    }

    void startGame(string[] names) {
        players[0] = Player(name=names[0], type=PlayerTypes.Gunner)
	    for (int i=1; i < names.Length; i++) {
            players[i] = Player(name=names[i], type=PlayerTypes.Knifer)
		}
        this.nextTurn();
	}

    void endGame(int winner=-1) {
        EndPanel(winner=winner)
	}

    void startTransition() {
        transitionPanel.show(player.name)
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
        nMovesLeft = 3;
        currentPlayer = (currentPlayer + 1) % players.Length;
        if (currentPlayer == 0) {
            round += 1;
        }
        moveCamera(player);  // follow the player
        this.updateView(player);
        if (player.extendedView) {
            extendView(player);
		}
        if (player.type == PlayerTypes.gunner) {
            watchButton.setActive(true);
            player.extendedView = false;
		} else {
            watchButton.setActive(false);  
		}
        startTransition();
	}

    void updateView() {
        //show the tiles the user can see (just 4 tiles)
        //stretch: highlight possible moves
	}

    void extendView() {
        //show full cone view (infinite)
	}

    void watch()

    // Update is called once per frame
    void Update()
    {
        this.updateTimer()
    }

    
}
