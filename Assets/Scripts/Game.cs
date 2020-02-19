using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Globalization;
using UnityEngine.UI;


/*
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
        //Board()
        this.presentStartingScreen()
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
*/


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
    public Player[] players = new Player[1];
    bool gameStarted = false;
    public Player currentPlayer;

    public GameObject watchButton;
    public GameObject stayButton;
    public List<Tile> tileButtons;

    /*
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
    {
    }
    */

    private void Start()
    {
        watchButton.GetComponent<Button>().interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A) && !gameStarted)
        {
            StartCoroutine(TurnLoop());
            gameStarted = true;
        }
    }

    IEnumerator TurnLoop()
    {
        int i = 0;
        uint waitFlags;
        while (true)
        {
            waitFlags = 0b0000;
            currentPlayer = this.players[i];
            StartCoroutine(currentPlayer.StartTurn());
            yield return new WaitUntil(() => waitFlags == 0b0001);
            
            i = (i + 1) % players.Length;
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


