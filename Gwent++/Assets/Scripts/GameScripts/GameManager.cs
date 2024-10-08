using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public GameObject cartelManager;
    public static GameManager Instance;
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }
    }
    //public bool currentPlayer = GameManager.Instance.CurrentPlayer;
    public bool CurrentPlayer{get; private set;} //si es false el turno es d fairies, si es true el turno es d demons
    public void ChangePlayerTurn()
    {
        if(CurrentPlayer == false)
        {
            if(ContextGame.contextGame.playerFairies.GetComponent<Player>().PlayerPassed == true)
            {
                CurrentPlayer = true;
                return;
            }
        }
        else if(CurrentPlayer == true)
        {
            if(ContextGame.contextGame.playerDemons.GetComponent<Player>().PlayerPassed == true)
            {
                CurrentPlayer = false;
                return;
            }
        }
        this.CurrentPlayer = !CurrentPlayer;
    }

    void Start()
    {
        //Se asigna el primer jugdor
        CurrentPlayer = false;
        //Se cambia la carta lider si el usuario creo una
        if(CreatedCards.FairiesLeader != null)
        {
            ContextGame.contextGame.playerFairies.GetComponent<Player>().Leader = CreatedCards.FairiesLeader;
        }
        if(CreatedCards.DemonsLeader != null)
        {
            ContextGame.contextGame.playerDemons.GetComponent<Player>().Leader = CreatedCards.FairiesLeader;
        }
        //Se realizan las funciones de inicio de juego
        StartGame(ContextGame.contextGame.playerFairies.GetComponent<Player>(), CreatedCards.CreatedFairiesCards);
        StartGame(ContextGame.contextGame.playerDemons.GetComponent<Player>(), CreatedCards.CreatedDemonsCards);
    }
    void Update()
    {
        if(End)
        {
            EndRound();
        }
    }

    #region StartGame
    private void StartGame(Player player, List<Card> CreatedCards)
    {
        //Se annade el Lider a la escena
        GridLayoutGroup gridLayoutGroup = player.LeaderZone.GetComponent<GridLayoutGroup>();
        player.HandZone.GetComponent<Zone>().InstantiateCard(player.Leader, gridLayoutGroup);
        //Se actualizan las cartas con su poder original y el booleano que indica q ya se jugo la carta
        foreach(var card in player.Deck)
        {
            card.IsPlayed = false;
            card.Power = card.OriginalPower;
        }
        //Se annaden al deck las cartas creadas por el usuario
        player.Deck.AddRange(CreatedCards);
        
        ContextGame.contextGame.Shuffle(player.Deck);
        //Se roban 10 cartas q pasan a la mano
        StoleCards(player);
        //Se actualiza el frontend del juego
        ContextGame.contextGame.UpdateFront();
    }
    private void StoleCards(Player player)
    {
        for (int i = 0; i < 10 ; i++)
        {
            ContextGame.contextGame.Stole(player);
        }
    }
    #endregion

    #region EndRound
    private bool End => ((ContextGame.contextGame.playerFairies.GetComponent<Player>().HandZone.GetComponent<Zone>().Cards.Count == 0 && ContextGame.contextGame.playerDemons.GetComponent<Player>().HandZone.GetComponent<Zone>().Cards.Count == 0)
                            || (ContextGame.contextGame.playerFairies.GetComponent<Player>().PlayerPassed == true && ContextGame.contextGame.playerDemons.GetComponent<Player>().PlayerPassed == true)
                            || (ContextGame.contextGame.playerFairies.GetComponent<Player>().PlayerPassed == true && ContextGame.contextGame.playerDemons.GetComponent<Player>().HandZone.GetComponent<Zone>().Cards.Count == 0)
                            || (ContextGame.contextGame.playerFairies.GetComponent<Player>().HandZone.GetComponent<Zone>().Cards.Count == 0 && ContextGame.contextGame.playerDemons.GetComponent<Player>().PlayerPassed == true));
    private void EndRound()
    {
        CardFaction winner = CardFaction.Fairies;

        //Se define quien gano
        if(ContextGame.contextGame.playerFairies.GetComponent<Player>().Points > ContextGame.contextGame.playerDemons.GetComponent<Player>().Points)
        {
            cartelManager.GetComponent<CartelManager>().MostrarCartel("El jugador Fairies gano la ronda");
            ContextGame.contextGame.playerFairies.GetComponent<Player>().RoundsWon ++ ;
        }
        else if(ContextGame.contextGame.playerFairies.GetComponent<Player>().Points < ContextGame.contextGame.playerDemons.GetComponent<Player>().Points)
        {
            winner = CardFaction.Demons;
            cartelManager.GetComponent<CartelManager>().MostrarCartel("El jugador Demons gano la ronda");
            ContextGame.contextGame.playerDemons.GetComponent<Player>().RoundsWon ++ ;
        }
        else if(ContextGame.contextGame.playerFairies.GetComponent<Player>().Points == ContextGame.contextGame.playerDemons.GetComponent<Player>().Points)
        {
            cartelManager.GetComponent<CartelManager>().MostrarCartel("Empate");
            ContextGame.contextGame.playerFairies.GetComponent<Player>().RoundsWon ++ ;
            ContextGame.contextGame.playerDemons.GetComponent<Player>().RoundsWon ++ ;
        }
        
        //Se limpia el tablero
        ContextGame.contextGame.CleanTheBoard();

        //Se verifica cual es el siguiente estado del juego segun las rondas ganadas de cada jugador
        int FairiesWinnedRounds = ContextGame.contextGame.playerFairies.GetComponent<Player>().UpdateRounds();
        int DemonsWinnedRounds = ContextGame.contextGame.playerDemons.GetComponent<Player>().UpdateRounds();
        Check(FairiesWinnedRounds, DemonsWinnedRounds, winner);
    }
    private void Check(int FWR, int DWR, CardFaction winner)
    {
        if((FWR == 0 && DWR  == 1) || (FWR == 1 && DWR == 0) || (FWR == 1 && DWR == 1))
        {
            NewRound(winner);
        }
        else if((FWR == 2 && DWR == 0) || (FWR == 2 && DWR == 1))
        { 
            PlayerFairiesWin();
        }
        else if((FWR == 0 && DWR == 2) || (FWR == 1 && DWR == 2))
        {
            PlayerDemonsWin();
        }
        else if(FWR == 2 && DWR == 2)
        {
            SceneManager.LoadScene("Draw");
        }
    }
    private void NewRound(CardFaction winner)
    {
        //Ambos jugadores roban dos cartas
        StoleTwoCards(ContextGame.contextGame.playerFairies.GetComponent<Player>());
        StoleTwoCards(ContextGame.contextGame.playerDemons.GetComponent<Player>());
        ContextGame.contextGame.UpdateFront();
        //Actualizar los booleanos de los turnos para la nueva ronda 
        if(winner == CardFaction.Demons)
        {
            CurrentPlayer = true;
        } 
        else if(winner == CardFaction.Fairies)
        {
            CurrentPlayer = false;
        }  
        ContextGame.contextGame.playerFairies.GetComponent<Player>().PlayerPassed = false;
        ContextGame.contextGame.playerDemons.GetComponent<Player>().PlayerPassed = false;
    }
    private void StoleTwoCards(Player player)
    {
        for (int i = 0; i < 2; i++)
        {
            ContextGame.contextGame.Stole(player);
        }
        //Si la mano tiene mas de 10 cartas despues del robo se envian al cementerio las cartas sobrantes
        int n = player.HandZone.GetComponent<Zone>().Cards.Count;
        if(n > 10)
        {
            List<Card> CardsToRemove = new List<Card>();
            for (int i = n - 1 ; i >= 10; i--)
            {
                CardsToRemove.Add(player.HandZone.GetComponent<Zone>().Cards[i]);
            }
            foreach(Card card in CardsToRemove)
            {
                player.Cementery.Add(card);
                player.HandZone.GetComponent<Zone>().Cards.Remove(card);
                card.IsPlayed = true;
            }
        }
    }

    #endregion

    #region EndGame
    private void PlayerFairiesWin()
    {
        SceneManager.LoadScene("PlayerFairiesWin");
    }
    private void PlayerDemonsWin()
    {
        SceneManager.LoadScene("PlayerDemonsWin");
    }
    #endregion
}