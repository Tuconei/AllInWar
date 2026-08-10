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

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
