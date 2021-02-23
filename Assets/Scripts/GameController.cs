using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [NonSerialized]
    public Canvas SetupMenu;
    [NonSerialized]
    public Canvas RulesMenu;
    [NonSerialized]
    public Canvas CardGame;
    [NonSerialized] 
    public Canvas FinishedGame;

    public string PlayerOne { get; set; }
    public string PlayerTwo { get; set; }
    private int _currentPlayer = 1; // meaning who's turn it currently is
    private bool _canDraw = false;
    
    private List<Card> _cards;
    private List<Card> _p1Cards;
    private List<Card> _p2Cards;

    public bool gameActive = false;

    public GameObject p1Gizmo;
    public GameObject p2Gizmo;

    public TMP_Text roundWinText;
    public Button continueButton;
    public TMP_Text cardsLeftText;

    public GameObject cardPrefab;

    private GameObject _p1CardObj;
    private GameObject _p2CardObj;
    private Card _p1CardDraw;
    private Card _p2CardDraw;
    
    // Start is called before the first frame update
    private void Start()
    {
        Instance = this;
        
        SetupMenu = GameObject.Find("SetupMenu").GetComponent<Canvas>();
        SetupMenu.enabled = true;

        RulesMenu = GameObject.Find("RulesMenu").GetComponent<Canvas>();
        RulesMenu.enabled = false;

        CardGame = GameObject.Find("GameCanvas").GetComponent<Canvas>();
        CardGame.enabled = false;

        FinishedGame = GameObject.Find("GameOverCanvas").GetComponent<Canvas>();
        FinishedGame.enabled = false;

        roundWinText.enabled = false;
        continueButton.gameObject.SetActive(false);
        
        _cards = new List<Card>();
        for (int c = 1; c <= 3; c++)
        {
            for (int n = 1; n <= 10; n++)
            {
                Card card = new Card
                {
                    Colour = c,
                    Number = n
                };
                _cards.Add(card);
            }
        }
        Debug.Log("Cards have been added -> " + _cards.Count);
    }

    public void BeginGame()
    {
        _p1Cards = new List<Card>();
        _p2Cards = new List<Card>();
        
        UpdateUINames(true, 0);
        UpdateCardsLeft();

        gameActive = true;
        CardGame.enabled = true;
        _canDraw = true;
    }

    private Card Draw()
    {
        Card card = _cards[Random.Range(0, _cards.Count)];
        _cards.Remove(card);
        return card;
    }

    private void ReturnCard(Card card)
    {
        _cards.Add(card);
    }

    private void AddCardToPlayer(int player, Card card)
    {
        if (player == 1)
            _p1Cards.Add(card);
        else
            _p2Cards.Add(card);
    }

    private void TakeTurn()
    {
        Debug.Log("Taking turn");
        Card draw = Draw();
        CreateCard(draw, _currentPlayer, true);
        UpdateCardsLeft();

        if (_currentPlayer == 2)
        {
            _canDraw = false;
            
            int winner = 0;
            
            // Calc same colour first
            if (_p1CardDraw.Colour == _p2CardDraw.Colour)
            {
                if (_p1CardDraw.Number > _p2CardDraw.Number)
                    winner = 1;
                else
                    winner = 2;
            }
            else
            {
                // 1 = red, 2 = black, 3 = yellow
                int p1 = _p1CardDraw.Colour;
                int p2 = _p2CardDraw.Colour;
                if (p1 == 1 && p2 == 2)
                    winner = 1;
                else if (p1 == 3 && p2 == 1)
                    winner = 1;
                else if (p1 == 2 && p2 == 3)
                    winner = 1;
                else
                    winner = 2;
            }

            AddCardToPlayer(winner, _p1CardDraw);
            AddCardToPlayer(winner, _p2CardDraw);
            
            roundWinText.text = (winner == 1 ? PlayerOne : PlayerTwo) + " has won the round!";
            roundWinText.enabled = true;
            
            continueButton.gameObject.SetActive(true);
            UpdateUINames(false, winner);
        }
        else
        {
            _currentPlayer = 2;
            UpdateUINames(true, 0);
        }
    }

    public void ClickContinueButton()
    {
        _currentPlayer = 1;
        
        Destroy(_p1CardObj);
        _p1CardDraw = null;
        
        Destroy(_p2CardObj);
        _p2CardDraw = null;

        roundWinText.enabled = false;
        continueButton.gameObject.SetActive(false);

        if (_cards.Count <= 0)
        {
            GameOver();
            return;
        }

        _canDraw = true;
        UpdateUINames(true, 0);
    }

    private void GameOver()
    {
        gameActive = false;
        CardGame.enabled = false;
        Debug.Log("Game over");
        
        // Win text
        TMP_Text winnerText = FinishedGame.transform.Find("WinnerText").GetComponent<TMP_Text>();

        int winner = 0; // 0 = draw, 1 = p1, 2 = p2
        
        if (_p1Cards.Count == _p2Cards.Count)
        {
            winnerText.text = "It's a tie!";
        }
        else
        {
            if (_p1Cards.Count > _p2Cards.Count)
                winner = 1;
            else
                winner = 2;

            winnerText.text = (winner == 1 ? PlayerOne : PlayerTwo) + " has won the game!";
        }
        
        // Scores part
        TMP_Text p1Score = FinishedGame.transform.Find("Player1Score").GetComponent<TMP_Text>();
        TMP_Text p2Score = FinishedGame.transform.Find("Player2Score").GetComponent<TMP_Text>();

        p1Score.text = PlayerOne + ": " + _p1Cards.Count;
        p2Score.text = PlayerTwo + ": " + _p2Cards.Count;

        FinishedGame.enabled = true;
    }

    private void CreateCard(Card card, int player, bool real)
    {
        GameObject c = Instantiate(cardPrefab, (player == 1 ? p1Gizmo.transform.localPosition : p2Gizmo.transform.localPosition), Quaternion.identity);
        c.transform.SetParent(CardGame.transform);
        c.transform.localScale = new Vector3(1, 1, 1);

        TMP_Text top = c.transform.Find("TopNum").GetComponent<TMP_Text>();
        TMP_Text bottom = c.transform.Find("BottomNum").GetComponent<TMP_Text>();

        top.text = card.Number.ToString();
        bottom.text = card.Number.ToString();

        if (card.Colour == 3) // i had to use an ass shade of yellow since unity sucks
        {
            top.color = Color.black;
            bottom.color = Color.black;
        }

        c.GetComponent<Image>().color = ColourFromCard(card);
        
        Debug.Log("created card with colour " + ColourNameFromCard(card) + " number " + card.Number);

        if (real)
        {
            if (player == 1)
            {
                _p1CardObj = c;
                _p1CardDraw = card;
            }
            else
            {
                _p2CardObj = c;
                _p2CardDraw = card;
            }   
        }
        else
        {
            Destroy(this, 5);
        }

    }

    private void UpdateUINames(bool colour, int winner)
    {
        TMP_Text p1 = GameObject.Find("PlayerOneText").GetComponent<TMP_Text>();
        TMP_Text p2 = GameObject.Find("PlayerTwoText").GetComponent<TMP_Text>();
        
        p1.text = GenerateUIName(1);
        p2.text = GenerateUIName(2);

        if (winner != 0)
        {
            if (winner == 1)
            {
                p1.color = Color.yellow;
                p2.color = Color.white;
            }
            else
            {
                p2.color = Color.yellow;
                p1.color = Color.white;
            }

            return;
        }
        
        if (colour)
        {
            if (_currentPlayer == 1)
            {
                p1.color = Color.cyan;
                p2.color = Color.white;
            }
            else
            {
                p2.color = Color.cyan;
                p1.color = Color.white;
            }
                
        }
        else
        {
            p1.color = Color.white;
            p2.color = Color.white;
        }
    } 
    
    private string GenerateUIName(int player)
    {
        string plr = (player == 1 ? PlayerOne : PlayerTwo);
        int amount = (player == 1 ? _p1Cards.Count : _p2Cards.Count);
        return plr + " (" + amount + ")";
    }

    private void UpdateCardsLeft()
    {
        cardsLeftText.text = "Cards Left: " + _cards.Count;
    }

    public void PileClick()
    {
        if (!_canDraw || !gameActive)
            return;
        
        TakeTurn();
    }

    private Color ColourFromCard(Card card)
    {
        switch (card.Colour)
        {
            case 1:
            {
                return Color.red;
            }
            case 2:
            {
                return Color.black;
            }
            case 3:
            {
                return Color.yellow;
            }
            default:
            {
                return Color.magenta;
            }
        }
    }

    private String ColourNameFromCard(Card card)
    {
        switch (card.Colour)
        {
            case 1:
            {
                return "Red";
            }
            case 2:
            {
                return "Black";
            }
            case 3:
            {
                return "Yellow";
            }
            default:
            {
                return "Error";
            }
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
