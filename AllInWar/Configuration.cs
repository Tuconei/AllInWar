using Dalamud.Configuration;
using System;

namespace AllInWar;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public int DefaultBuyIn { get; set; } = 50000;
    public int RakePercent { get; set; } = 5;
    public bool IncludeDealerAsPlayer { get; set; } = true;
    public bool ConfirmManualTradesOnly { get; set; } = true;
    public string ChatCommandPrefix { get; set; } = "/party";
    public string RulesMessage { get; set; } = "All In War: buy in for {buyIn} gil. Each player rolls 1-13. Ace is high and beats 10-K. Highest card wins after {rake}% house rake. Ties go to war: match the bet again or surrender.";
    public string CollectionMessage { get; set; } = "All In War is starting. Please trade {buyIn} gil to enter.";
    public string WinnerMessage { get; set; } = "{winner} wins All In War with {card} and receives {payout} gil. House rake: {rakeAmount} gil.";

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
