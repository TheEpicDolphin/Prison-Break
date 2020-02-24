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
        DisableMovementOptionButtons();
        Game.Instance.currentPlayer.ProcessAction(PlayerAction.Watch);
    }

    public void Skip()
    {
        DisableMovementOptionButtons();
        Game.Instance.currentPlayer.ProcessAction(PlayerAction.Skip);
    }

    public void FaceRight()
    {
        DisableRotationOptionButtons();
        Game.Instance.currentPlayer.ProcessAction(PlayerAction.Rotate, new Dictionary<string, object> { {"dir" , Vector2.right}});
    }

    public void FaceUp()
    {
        DisableRotationOptionButtons();
        Game.Instance.currentPlayer.ProcessAction(PlayerAction.Rotate, new Dictionary<string, object> { { "dir", Vector2.down } });
    }

    public void FaceLeft()
    {
        DisableRotationOptionButtons();
        Game.Instance.currentPlayer.ProcessAction(PlayerAction.Rotate, new Dictionary<string, object> { { "dir", Vector2.left } });
    }

    public void FaceDown()
    {
        DisableRotationOptionButtons();
        Game.Instance.currentPlayer.ProcessAction(PlayerAction.Rotate, new Dictionary<string, object> { { "dir", Vector2.up } });
    }

    void DisableMovementOptionButtons()
    {
        foreach (Tile neighborTile in Game.Instance.tileButtons)
        {
            neighborTile.gameObject.layer = 2;

            if (neighborTile.isExit)
            {
                neighborTile.gameObject.GetComponent<MeshRenderer>().material.color = new Color(255 / 255.0f, 225 / 255.0f, 0 / 255.0f);
            }
            else
            {
                neighborTile.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }
        Game.Instance.watchButton.GetComponent<Button>().interactable = false;
        Game.Instance.stayButton.GetComponent<Button>().interactable = false;
    }

    void DisableRotationOptionButtons()
    {
        Game.Instance.rightButton.GetComponent<Button>().interactable = false;
        Game.Instance.upButton.GetComponent<Button>().interactable = false;
        Game.Instance.leftButton.GetComponent<Button>().interactable = false;
        Game.Instance.downButton.GetComponent<Button>().interactable = false;

    }

    public void TransitionToNextTurn()
    {
        Game.Instance.transitionPanel.SetActive(false);
        Game.Instance.NextTurn();
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
