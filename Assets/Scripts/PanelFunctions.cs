using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelFunctions : MonoBehaviour
{
    public GameObject Canvas;
    
    public GameObject TransitionPanel;
    public Text TransitionText;    
    private string baseText;

    public GameObject StartPanel;

    public GameObject EndPanel;

    public GameObject ErrorPanel;
    public Text ErrorPanelText;

    public Text Input1;
    public Text Input2;
    public Text Input3;

    void Start()
    {
        //float height = Canvas.GetComponent<RectTransform>().rect.height;
        //TransitionPanel.transform.position = Canvas.transform.position;
        //TransitionPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTranform.Axis.Vertical, height);
        baseText = "{0}, make sure your eyes are closed.\n{1}, are you ready?";
        TransitionText.text = string.Format(baseText, "player 1", "player 2");
        //TransitionPanel.SetActive(false);
    }

    void Update() {}

    public void Watch()
    {
        foreach (Tile neighborTile in Game.Instance.tileButtons)
        {
            neighborTile.gameObject.layer = 2;
            neighborTile.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
        }
        StartCoroutine(Game.Instance.currentPlayer.Watch());
    }

    public void transitionReadyButton()
    {
        TransitionPanel.SetActive(false);
	}

    public void showError(string error) {
        ErrorPanelText.text = error;
        ErrorPanel.SetActive(true);
	}

    public void startReadyButton() {
        /*
        if (Input1.text == "") {
            showError("Missing player 1 name");
		} else if (Input2.text == "") {
            showError("Missing player 2 name");
		} else if (Input3.text == "") {
            showError("Missing player 3 name");
		} else {
            StartPanel.SetActive(false);
            // Game.startGame(names=new string[]{Input1.text, Input2.text, Input3.text});  
		} */
        StartPanel.SetActive(false);
        // Game.startGame(names=new string[]{Input1.text, Input2.text, Input3.text});  
	}
}
