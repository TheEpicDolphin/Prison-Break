using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public LinkedList<Player> players = new LinkedList<Player>();
    public List<Gunner> gunners = new List<Gunner>();
    public List<Knifer> knifers = new List<Knifer>();

    bool gameStarted = false;
    Board board;
    LinkedListNode<Player> currentNode;
    public Player currentPlayer;

    public Text TimerText;
    public Text MovesLeftText;

    public GameObject watchButton;
    public GameObject stayButton;

    public GameObject rightButton;
    public GameObject upButton;
    public GameObject leftButton;
    public GameObject downButton;

    public GameObject startPanel;
    public GameObject transitionPanel;
    public GameObject transitionPanelTextContainer;
    public GameObject endPanel;
    public GameObject endPanelTextContainer;

    public List<Tile> tileButtons;

    Coroutine gameLoop;

    public CameraController camController;

    uint waitFlags;

    private DateTime startTime;
    private static int maxTime = 300;
    public int timeLeft = maxTime;
    public string timer;

    private void Start()
    {
        watchButton.GetComponent<Button>().interactable = false;
        stayButton.GetComponent<Button>().interactable = false;
        rightButton.GetComponent<Button>().interactable = false;
        upButton.GetComponent<Button>().interactable = false;
        leftButton.GetComponent<Button>().interactable = false;
        downButton.GetComponent<Button>().interactable = false;
        transitionPanel.SetActive(false);
        endPanel.SetActive(false);
        startPanel.SetActive(true);
        board = gameObject.GetComponent<Board>();
        startTime = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Return) && !gameStarted)
        {
            ResetGame();
            startPanel.SetActive(false);
            endPanel.SetActive(false);

            foreach (Gunner gunner in gunners)
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

        timeLeft = Math.Max(0, maxTime - (int)(DateTime.Now - startTime).TotalSeconds);
        updateTimer();
        if (timeLeft == 0)
        {
            EndGame(winner:"[timeout]");
        }
        UpdateMovesLeft(currentPlayer.movesLeft);
    }

    void UpdateMovesLeft(int movesLeft)
    {
        MovesLeftText.text = String.Format("Moves Left: {0}", movesLeft);
    }

    void updateTimer()
    {
        string timer = String.Format("{0}:{1}", timeLeft / 60, (timeLeft % 60).ToString("D2"));
        TimerText.text = timer;
    }

    void ResetGame()
    {
        watchButton.GetComponent<Button>().interactable = false;
        stayButton.GetComponent<Button>().interactable = false;
        rightButton.GetComponent<Button>().interactable = false;
        upButton.GetComponent<Button>().interactable = false;
        leftButton.GetComponent<Button>().interactable = false;
        downButton.GetComponent<Button>().interactable = false;

        foreach (Gunner gunner in gunners)
        {
            Destroy(gunner.gameObject);
        }
        gunners = new List<Gunner>();

        foreach (Knifer knifer in knifers)
        {
            Destroy(knifer.gameObject);
        }
        knifers = new List<Knifer>();

        players = new LinkedList<Player>();
        board.ResetBoard();
        startTime = DateTime.Now;

        for(int i = 0; i < gunners.Count; i++)
        {
            gunners[i].playerName = "Gunner" + i.ToString();
            players.AddLast(gunners[i]);
        }
        for(int i = 0; i < knifers.Count; i++)
        {
            knifers[i].playerName = "Knifer" + i.ToString();
            players.AddLast(knifers[i]);
        }
    }

    IEnumerator GameLoop()
    {
        currentNode = players.First;
        HideKnifers();
        while (true)
        {
            waitFlags = 0b0000;
            currentPlayer = currentNode.Value;
            //Debug.Log(currentPlayer.gameObject.name + " starting turn...");
            currentPlayer.ExecuteState();
            yield return new WaitUntil(() => waitFlags == 0b0001);
            currentNode = currentNode.Next ?? currentNode.List.First;

        }
    }

    public void NextTurn()
    {
        for (int i = 0; i < gunners.Count; i++)
        {
            gunners[i].movesLeft = 2;
        }
        for (int i = 0; i < knifers.Count; i++)
        {
            knifers[i].movesLeft = 3;
        }
        this.waitFlags = 0b0001;

    }

    public void EndGame(string winner)
    {  
        //Stop game loop
        StopCoroutine(gameLoop);

        //Display winners
        endPanel.SetActive(true);
        if (winner == "[timeout]")
        {
            endPanelTextContainer.GetComponent<Text>().text = "Time ran out! Press Enter to start a new game.";
        } else
        {
            endPanelTextContainer.GetComponent<Text>().text = winner + " have won!\nPress Enter to start a new game.";
        }
        gameStarted = false;
    }

    public bool CheckIfAllGunnersDead()
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
            return true;
        }
        return false;
    }

    public bool CheckIfAllKnifersDead()
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
            return true;
        }
        return false;
    }

    public void GunnerReachedExit()
    {
        EndGame("Gunners");
    }

    public void PresentTransitionPanel(Player player)
    {
        
        LinkedListNode<Player> nextNode = currentNode.Next ?? currentNode.List.First;
        transitionPanelTextContainer.GetComponent<Text>().text = player.playerName + ", close your eyes.\n" + nextNode.Value.playerName + ", are you ready?";
        transitionPanel.SetActive(true);
    }

    public void HideKnifers()
    {
        foreach (Knifer knifer in Game.Instance.knifers)
        {
            knifer.GetComponent<Renderer>().enabled = false;
        }
    }

    public void ShowKnifers()
    {
        foreach (Knifer knifer in Game.Instance.knifers)
        {
            if (knifer.curState != PlayerState.Dead)
            {
                knifer.GetComponent<Renderer>().enabled = true;
            }

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


