namespace AllInWar.Models;

public sealed class TablePlayer
{
    public TablePlayer(string name, bool isDealer = false)
    {
        Name = name;
        IsDealer = isDealer;
    }

    public string Name { get; set; }
    public bool IsDealer { get; set; }
    public int Card { get; set; }
    public int Contribution { get; set; }
    public bool Surrendered { get; set; }
    public bool IsInWar { get; set; }
    public bool HasMatchedWarBet { get; set; }
    public TradeStatus AnteTradeStatus { get; set; } = TradeStatus.Pending;
    public TradeStatus PayoutTradeStatus { get; set; } = TradeStatus.Pending;

    public bool IsActive => !Surrendered;

    public void ResetForRound(int buyIn)
    {
        Card = 0;
        Contribution = buyIn;
        Surrendered = false;
        IsInWar = false;
        HasMatchedWarBet = false;
        AnteTradeStatus = TradeStatus.Pending;
        PayoutTradeStatus = TradeStatus.Pending;
    }
}
