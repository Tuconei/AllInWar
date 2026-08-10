using System;
using System.Collections.Generic;
using System.Linq;

namespace AllInWar.Models;

public sealed class AllInWarTable
{
    private readonly Random random = new();

    public List<TablePlayer> Players { get; } = [];
    public GamePhase Phase { get; private set; } = GamePhase.Seating;
    public int BuyIn { get; set; } = 50000;
    public int RakePercent { get; set; } = 5;
    public TablePlayer? Winner { get; private set; }
    public string Status { get; private set; } = "Seat up to 8 players, then start a round.";

    public IReadOnlyList<TablePlayer> ActivePlayers => Players.Where(player => player.IsActive).ToList();
    public IReadOnlyList<TablePlayer> WarPlayers => ActivePlayers.Where(player => player.IsInWar).ToList();
    public IReadOnlyList<TablePlayer> TiedPlayers => ActivePlayers.Where(player => CardStrength(player.Card) == HighestCardStrength && player.Card > 0).ToList();
    public int HighestCard => ActivePlayers.Count == 0 ? 0 : ActivePlayers.MaxBy(player => CardStrength(player.Card))?.Card ?? 0;
    private int HighestCardStrength => CardStrength(HighestCard);
    public int Pot => Players.Sum(player => player.Contribution);
    public int Rake => Math.Clamp(Pot * Math.Clamp(RakePercent, 0, 100) / 100, 0, Pot);
    public int WinnerPayout => Math.Max(0, Pot - Rake);

    public bool AddPlayer(string name, bool isDealer)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name) || Players.Count >= 8)
        {
            return false;
        }

        if (isDealer)
        {
            foreach (var player in Players)
            {
                player.IsDealer = false;
            }
        }

        Players.Add(new TablePlayer(name, isDealer));
        Status = "Player seated.";
        return true;
    }

    public void RemovePlayer(TablePlayer player)
    {
        Players.Remove(player);
        if (Players.Count == 0)
        {
            Phase = GamePhase.Seating;
        }
    }

    public void StartRound()
    {
        Winner = null;
        BuyIn = Math.Clamp(BuyIn, 1, 100000000);
        RakePercent = Math.Clamp(RakePercent, 0, 100);

        foreach (var player in Players)
        {
            player.ResetForRound(BuyIn);
        }

        Phase = Players.Count >= 2 ? GamePhase.CollectingAntes : GamePhase.Seating;
        Status = Players.Count >= 2
            ? "Collect buy-ins, then roll cards."
            : "Seat at least 2 players before starting.";
    }

    public void ConfirmAllAntes()
    {
        foreach (var player in Players)
        {
            player.AnteTradeStatus = TradeStatus.Confirmed;
        }

        Phase = GamePhase.DrawingCards;
        Status = "Antes confirmed. Roll or enter cards.";
    }

    public void RollActiveCards()
    {
        foreach (var player in ActivePlayers)
        {
            player.Card = random.Next(1, 14);
        }

        ResolveCards();
    }

    public void ResolveCards()
    {
        Winner = null;

        var candidates = Phase == GamePhase.WarDecision && WarPlayers.Any(player => player.Card > 0)
            ? WarPlayers
            : ActivePlayers;

        if (candidates.Count == 0)
        {
            Phase = GamePhase.Seating;
            Status = "No active players remain.";
            return;
        }

        var highestCardStrength = candidates.Max(player => CardStrength(player.Card));
        var tied = candidates.Where(player => CardStrength(player.Card) == highestCardStrength && player.Card > 0).ToList();

        if (tied.Count == 1)
        {
            CompleteWithWinner(tied[0]);
            return;
        }

        Phase = GamePhase.WarDecision;
        foreach (var player in Players)
        {
            player.IsInWar = tied.Contains(player);
            player.HasMatchedWarBet = false;
        }

        Status = $"{tied.Count} players tied at {CardName(tied[0].Card)}. Each may match the bet or surrender.";
    }

    public void AddWarBet(TablePlayer player)
    {
        if (Phase != GamePhase.WarDecision || !WarPlayers.Contains(player))
        {
            return;
        }

        player.Contribution += BuyIn;
        player.Card = 0;
        player.HasMatchedWarBet = true;
        Status = $"{player.Name} matched the war bet.";
    }

    public void Surrender(TablePlayer player)
    {
        if (Phase != GamePhase.WarDecision || !WarPlayers.Contains(player))
        {
            return;
        }

        player.Surrendered = true;
        player.IsInWar = false;
        player.Card = 0;
        Status = $"{player.Name} surrendered.";

        if (WarPlayers.Count == 1)
        {
            CompleteWithWinner(WarPlayers[0]);
        }
    }

    public void RollWarCards()
    {
        var players = WarPlayers;
        if (players.Count < 2 || players.Any(player => !player.HasMatchedWarBet))
        {
            Status = "Every remaining war player must match the bet before rolling.";
            return;
        }

        foreach (var player in players)
        {
            player.Card = random.Next(1, 14);
            player.HasMatchedWarBet = false;
        }

        ResolveCards();
    }

    private void ClearWarFlags()
    {
        foreach (var player in Players)
        {
            player.IsInWar = false;
            player.HasMatchedWarBet = false;
        }
    }

    private void CompleteWithWinner(TablePlayer winner)
    {
        Winner = winner;
        ClearWarFlags();
        Winner.PayoutTradeStatus = TradeStatus.Pending;
        Phase = GamePhase.Complete;
        Status = $"{Winner.Name} wins {WinnerPayout:n0}. Rake: {Rake:n0}.";
    }

    public static string CardName(int card)
    {
        return card switch
        {
            1 => "Ace",
            11 => "Jack",
            12 => "Queen",
            13 => "King",
            _ => card.ToString()
        };
    }

    private static int CardStrength(int card)
    {
        return card == 1 ? 14 : card;
    }
}
