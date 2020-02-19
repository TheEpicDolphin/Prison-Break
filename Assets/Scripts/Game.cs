using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Globalization;


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

    // Start is called before the first frame update
    void Start()
    {
        
        
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

    
}
