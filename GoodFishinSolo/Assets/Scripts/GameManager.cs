using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{

    //RESOURCES USED
    // <shttps://www.youtube.com/watch?v=C5bnWShD6ng

//----------THIS SCRIPT IS FOR TURN SEQUENCE, DRAWING CARDS, AND EVENTS----

    //-----------------INITIAL VARIABLES---------------------------
    public static GameManager instance;
    public static PlayerController playerController;
    public Card card;
    //-----------------VARIABLES FOR GAME OBJECTS-------------------
    public List<Deck> decks = new List<Deck>();

    public Transform[] cardSlots;
    
    public bool[] availableCardSlots;

    public int handIndex = 0; //for current card

    //----------------VARIABLES FOR PLAYER UI----------------------
    public TextMeshProUGUI deckSizeText;
    public TextMeshProUGUI discardPileText;

    //-----------------VARIABLES FOR TURNS-----------------------
    public TextMeshProUGUI tackleBoxText;
    public TextMeshProUGUI trophyPointsText;
    private bool isPlayerTurn = true;

    public GameObject skipDiscardButton;

    public TextMeshProUGUI eventText;

    //-----------------VARIABLES FOR WIN/LOSE-------------------
    //bait count also used to check
    public int cardCounter = 0;
    public GameObject winLosePanel;  // Win Screen
    public TextMeshProUGUI outOfBaitText;
    public TextMeshProUGUI fullHandText;

    //-------------Variables how to play----------
    public GameObject howToPanel;

    public GameObject goFishButton;


//***************************  FUNCTIONS *************************



//-------------------ON GAME START---------------------
    void Awake()
    {
        instance = this;

        skipDiscardButton.SetActive(false);//Turnn off skip discard button
        eventText.gameObject.SetActive(false);//Turn off event text


        // Check if playerController is set
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>(); // Automatically finds the PlayerController in the scene
            
        }

        // Initialize PlayerController.me from the GameManager
        if (playerController != null)
        {
            PlayerController.me = playerController;
            StartTurn(); //Lets the player take first turn
            Debug.Log("FIRST TURN");
        }
        else
        {
            Debug.LogError("PlayerController not found!");
        }
    }

//-----------------------How to play screen----------------------
    public void onGoFishClicked()
    {
        goFishButton.SetActive(false);
        howToPanel.SetActive(false);
    }

//-----------------STARTS TURN-------------------------
    public bool IsPlayerTurn()
    {
        return isPlayerTurn;  // Basically checks if player can draw.
    }

    
    
    public void StartTurn()
    {
        isPlayerTurn = true;
        Debug.Log("Player's turn started. Click the draw button to draw a card.");
        // Player can click the button to manually draw a card when ready
    }

//---------------DRAWING ACTION-----------------------------
    public void DrawFromSpecificDeck(int deckIndex)
    {
        Invoke("HideEventText", 0);
        if(isPlayerTurn == true){//If allowed to draw
            DrawCard(deckIndex);
        }
        else{
            Debug.Log("Error! Not time to draw yet!");
        }
    }

    public void DrawCard(int deckIndex)
    {
        //Make sure correct number of decks
        if(deckIndex < 0 || deckIndex >= decks.Count)
        {
            Debug.LogError("Invalid deck index.");
            return;
        }

        // Select the specified deck
        Deck selectedDeck = decks[deckIndex];

        //Make sure theres enough cards in the specified deck
        if (selectedDeck.cards.Count >= 1)
        {
            // randCard = deck[Random.Range(0,deck.Count)];
            Card randCard = selectedDeck.cards[Random.Range(0, selectedDeck.cards.Count)];

            for (int i = 0; i < availableCardSlots.Length; i++)
            {
                if (availableCardSlots[i] == true)
                {
                    int baitCost = GetBaitCost(deckIndex);

                    if(playerController.baitCount < baitCost){
                        Debug.Log("Not enough Bait :( ");
                        isPlayerTurn = true; //lets the player draw again
                        return;
                    } 
                    else
                    {
                        isPlayerTurn = false; //Doesn't let the player draw again

                        randCard.gameObject.SetActive(true);
                        randCard.handIndex = i;
                        randCard.transform.position = cardSlots[i].position;
                        availableCardSlots[i] = false;

                        selectedDeck.cards.Remove(randCard);

                        // Assign the deck to the card (so it knows its origin)
                        randCard.originDeck = selectedDeck;


                        selectedDeck.cards.Remove(randCard); // Remove from the deck
                                        // Don't add to discard pile yet

                        // Update bait count
                        playerController.baitCount -= baitCost;
                        UpdateBaitUI(playerController.baitCount);

                        cardCounter++;
                        Debug.Log(cardCounter);
                        // Update deck and discard pile UI
                        UpdateDeckUI();

                        DiscardPhase();
                        return;
                    }

                }
            }
        }
    }

//Discard "on mouse down" in card class
//-------------------SKIP DISCARD BUTTON-------------------------------------

    public void DiscardPhase()
    {
        ShowSkipDiscardButton();
    }

    public void OnSkipDiscardClicked()
    {
    HideSkipDiscardButton();
    // If skipping the discard, proceed directly to the event and end the turn.
    TriggerEvent();
    EndTurn();
    }

    public void HideSkipDiscardButton ()
    {
        skipDiscardButton.SetActive(false);//Turnn off skip discard button
    }

    public void ShowSkipDiscardButton ()
    {
        skipDiscardButton.SetActive(true);//Turnn off skip discard button
    }
//----------------------EVENT ACTION--------------------------------------------
//NEEDS WORK
    public void TriggerEvent()
    {
        int randEvent;
        if (playerController.baitCount > 2)
        {
            randEvent= Random.Range(1, 6); // Generates a number between 1 and 3
        }
        else {
            randEvent = Random.Range(1, 5);
        }

        switch (randEvent)
        {
            case 1:
                Event1();
                break;
            case 2:
                Event2();
                break;
            case 3://Not this if not enough bait, dont want to end players turn;
                Event3();
                break;
            case 4://Not this if not enough bait, dont want to end players turn;
                Event4();
                break;
            case 5://Not this if not enough bait, dont want to end players turn;
                Event5();
                break;
        }
    }

    private void Event1()
    {
        eventText.text = "Can of worms: Get 3 bait";
        playerController.baitCount += 3;
        eventText.gameObject.SetActive(true);
        UpdateBaitUI(playerController.baitCount);    
    }

    private void Event2()
    {
        eventText.text = "Holy Mackeral: Gain 3 Trophy points";
        playerController.points += 3;
        UpdateTrophyPointsUI(playerController.points);
        eventText.gameObject.SetActive(true);
    }
    private void Event3()
    {
        eventText.text = "Flop on the Deck: Lose 2 Trophy Points.";
        playerController.points -= 2;
        if (playerController.points < 0)
            playerController.points = 0; // Ensure points do not go negative
        UpdateTrophyPointsUI(playerController.points);
        eventText.gameObject.SetActive(true);
    }
    private void Event4()
    {
        eventText.text = "Lucky Catch: Draw an additional card for free!";
        eventText.gameObject.SetActive(true);
        // Allow player to draw a card without using bait
        isPlayerTurn = true; // Temporarily allow drawing
    }

    private void Event5()
    {
        if (playerController.baitCount > 2) // Assuming playerController is accessible here
        {
            eventText.text = "Hungry Guppy: Lose 1 bait";
            playerController.baitCount -= 1;
            eventText.gameObject.SetActive(true);
            UpdateBaitUI(playerController.baitCount);        
        }
        else
        {
            // Optionally, call another event or do nothing
            TriggerEvent(); // Try triggering another event if conditions aren't met
        }
    }



    private void HideEventText()
    {
        eventText.gameObject.SetActive(false);
    }

    //-------------------------UPDATE UI-----------------------------------------
    public void UpdateDeckUI()
    {
        for (int i = 0; i < decks.Count; i++)
        {
            deckSizeText.text = $"Remaining in deck {i+1}: {decks[i].cards.Count}";
            discardPileText.text = $"Discard Pile {i+1}: {decks[i].discardPile.Count}";
        }
    }

    public void UpdateTrophyPointsUI(int points)
    {
        trophyPointsText.text = "Trophy Points: " + points.ToString();
    }

    public void UpdateBaitUI(int baitCount)
    {
        tackleBoxText.text = "Bait: " + baitCount.ToString();
    }

    void IncreaseBait()
    {
        playerController.baitCount++;
        UpdateBaitUI(playerController.baitCount);
    }

    
    //Needs update Vertical Slice
    public int GetBaitCost(int deckIndex)
    {
        if (deckIndex == 0) return 1; // Deck 1 costs 1 bait
        if (deckIndex == 1) return 2; // Deck 2 costs 2 bait
        if (deckIndex == 2) return 3; // Deck 3 costs 3 bait
        return 0; // Default cost
    }

    //------------------------END AND RESET-------------------

    ///TO hide hand, just hode all 3 decks
    public void EndTurn()
    {
        if(cardCounter >= 5)
        {
            //Invoke("HideEventText", 0);
            Debug.Log("Out of hand space.");
            winLosePanel.SetActive(true);
            outOfBaitText.gameObject.SetActive(false);
            fullHandText.text = "Boy howdy, you've reached your fishing quota!\nFinal Catch: " + playerController.points + " Trophy Points!";
        }
        else if(playerController.baitCount <= 0)
        {
            //nvoke("HideEventText", 0);
            Debug.Log("Out of bait.");
            winLosePanel.SetActive(true);
            fullHandText.gameObject.SetActive(false);
            outOfBaitText.text = "Looks like them fishies emptied your tackle box.\n Final Catch: " + playerController.points + " Trophy Points!";
        } else {
            isPlayerTurn = false;  // Set the turn flag to false when ending the turn
            Debug.Log("Turn Over, starting new turn");
            StartTurn();
        }
    }


    //RESET NEVER CALLED-- Make sure to check this in Prod 4
    public void ResetGame()
    {
        foreach (var deck in decks)
        {
            deck.cards.AddRange(deck.discardPile);  // Move all discarded cards back into the deck
            deck.discardPile.Clear();  // Clear the discard pile
        }
        
        // Update UI to reflect reset
        UpdateDeckUI();
        UpdateBaitUI(playerController.baitCount);
    }
}
