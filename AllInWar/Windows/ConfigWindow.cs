using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace AllInWar.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("All In War Settings###AllInWarSettings")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(330, 150);
        SizeCondition = ImGuiCond.FirstUseEver;

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        if (configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        var defaultBuyIn = configuration.DefaultBuyIn;
        if (ImGui.InputInt("Default buy-in", ref defaultBuyIn, 1000, 10000))
        {
            configuration.DefaultBuyIn = Math.Clamp(defaultBuyIn, 1, 100000000);
            configuration.Save();
        }

        var rakePercent = configuration.RakePercent;
        if (ImGui.SliderInt("House rake", ref rakePercent, 0, 25, "%d%%"))
        {
            configuration.RakePercent = Math.Clamp(rakePercent, 0, 25);
            configuration.Save();
        }

        var includeDealer = configuration.IncludeDealerAsPlayer;
        if (ImGui.Checkbox("Dealer can be seated as a player", ref includeDealer))
        {
            configuration.IncludeDealerAsPlayer = includeDealer;
            configuration.Save();
        }

        var manualTradesOnly = configuration.ConfirmManualTradesOnly;
        if (ImGui.Checkbox("Manual trade confirmations only", ref manualTradesOnly))
        {
            configuration.ConfirmManualTradesOnly = manualTradesOnly;
            configuration.Save();
        }

        var movable = configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable settings window", ref movable))
        {
            configuration.IsConfigWindowMovable = movable;
            configuration.Save();
        }
    }
}
