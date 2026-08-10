using System;
using System.Numerics;
using AllInWar.Models;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

namespace AllInWar.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private readonly AllInWarTable table = new();
    private string newPlayerName = string.Empty;

    public MainWindow(Plugin plugin)
        : base("All In War###AllInWarMain")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(560, 420),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
        table.BuyIn = plugin.Configuration.DefaultBuyIn;
        table.RakePercent = plugin.Configuration.RakePercent;
    }

    public void Dispose() { }

    public override void Draw()
    {
        SyncConfigDefaults();
        DrawRoundControls();
        ImGui.Separator();
        DrawSeatControls();
        ImGui.Separator();
        DrawPlayers();
        ImGui.Separator();
        DrawSettlement();
    }

    private void SyncConfigDefaults()
    {
        if (table.Phase == GamePhase.Seating)
        {
            table.BuyIn = plugin.Configuration.DefaultBuyIn;
            table.RakePercent = plugin.Configuration.RakePercent;
        }
    }

    private void DrawRoundControls()
    {
        ImGui.TextUnformatted($"Phase: {table.Phase}");
        ImGui.TextUnformatted(table.Status);

        ImGui.Spacing();

        var buyIn = table.BuyIn;
        ImGui.SetNextItemWidth(140 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputInt("Buy-in", ref buyIn, 1000, 10000))
        {
            table.BuyIn = Math.Clamp(buyIn, 1, 100000000);
        }

        ImGui.SameLine();
        var rake = table.RakePercent;
        ImGui.SetNextItemWidth(130 * ImGuiHelpers.GlobalScale);
        if (ImGui.SliderInt("Rake %", ref rake, 0, 25))
        {
            table.RakePercent = Math.Clamp(rake, 0, 25);
        }

        ImGui.Spacing();

        if (ImGui.Button("New Round"))
        {
            table.StartRound();
        }

        ImGui.SameLine();
        if (ImGui.Button("Send Rules"))
        {
            SendConfiguredMessage(plugin.Configuration.RulesMessage);
        }

        ImGui.SameLine();
        if (ImGui.Button("Send Collection"))
        {
            SendConfiguredMessage(plugin.Configuration.CollectionMessage);
        }

        ImGui.SameLine();
        if (ImGui.Button("Confirm Antes"))
        {
            table.ConfirmAllAntes();
        }

        ImGui.SameLine();
        if (ImGui.Button("Roll Cards"))
        {
            table.RollActiveCards();
        }

        ImGui.SameLine();
        if (ImGui.Button("Resolve Entered Cards"))
        {
            table.ResolveCards();
        }
    }

    private void DrawSeatControls()
    {
        ImGui.TextUnformatted($"Players: {table.Players.Count}/8");

        ImGui.SetNextItemWidth(260 * ImGuiHelpers.GlobalScale);
        ImGui.InputText("Name", ref newPlayerName, 64);

        ImGui.SameLine();
        if (ImGui.Button("Seat Player"))
        {
            table.AddPlayer(newPlayerName, false);
            newPlayerName = string.Empty;
        }

        if (plugin.Configuration.IncludeDealerAsPlayer)
        {
            ImGui.SameLine();
            if (ImGui.Button("Seat Dealer"))
            {
                table.AddPlayer(newPlayerName, true);
                newPlayerName = string.Empty;
            }
        }
    }

    private void DrawPlayers()
    {
        if (!ImGui.BeginTable("players", 8, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
        {
            return;
        }

        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Role");
        ImGui.TableSetupColumn("Ante");
        ImGui.TableSetupColumn("Card");
        ImGui.TableSetupColumn("Contribution");
        ImGui.TableSetupColumn("War");
        ImGui.TableSetupColumn("Payout");
        ImGui.TableSetupColumn("Remove");
        ImGui.TableHeadersRow();

        foreach (var player in table.Players.ToArray())
        {
            ImGui.PushID(player.Name);
            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(player.Name);

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(player.IsDealer ? "Dealer" : "Player");

            ImGui.TableNextColumn();
            DrawTradeStatus($"ante-{player.Name}", player.AnteTradeStatus, status => player.AnteTradeStatus = status);

            ImGui.TableNextColumn();
            var card = player.Card;
            ImGui.SetNextItemWidth(72 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputInt("##card", ref card))
            {
                player.Card = Math.Clamp(card, 0, 13);
            }

            if (player.Card > 0)
            {
                ImGui.TextUnformatted(AllInWarTable.CardName(player.Card));
            }

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{player.Contribution:n0}");

            ImGui.TableNextColumn();
            if (player.IsInWar)
            {
                if (ImGui.Button("Match"))
                {
                    table.AddWarBet(player);
                }

                ImGui.SameLine();
                if (ImGui.Button("Surrender"))
                {
                    table.Surrender(player);
                }
            }
            else
            {
                ImGui.TextUnformatted(player.Surrendered ? "Out" : "-");
            }

            ImGui.TableNextColumn();
            DrawTradeStatus($"payout-{player.Name}", player.PayoutTradeStatus, status => player.PayoutTradeStatus = status);

            ImGui.TableNextColumn();
            if (ImGui.SmallButton("X"))
            {
                table.RemovePlayer(player);
            }

            ImGui.PopID();
        }

        ImGui.EndTable();

        if (table.Phase == GamePhase.WarDecision && ImGui.Button("Roll War Cards"))
        {
            table.RollWarCards();
        }
    }

    private static void DrawTradeStatus(string id, TradeStatus value, Action<TradeStatus> setValue)
    {
        ImGui.PushID(id);
        if (ImGui.SmallButton(value.ToString()))
        {
            var next = value switch
            {
                TradeStatus.Pending => TradeStatus.Confirmed,
                TradeStatus.Confirmed => TradeStatus.Waived,
                _ => TradeStatus.Pending
            };
            setValue(next);
        }

        ImGui.PopID();
    }

    private void DrawSettlement()
    {
        ImGui.TextUnformatted($"Pot: {table.Pot:n0}");
        ImGui.SameLine();
        ImGui.TextUnformatted($"Rake: {table.Rake:n0}");
        ImGui.SameLine();
        ImGui.TextUnformatted($"Winner payout: {table.WinnerPayout:n0}");

        if (table.Winner is not null)
        {
            ImGui.TextUnformatted($"Winner: {table.Winner.Name}");
            ImGui.TextUnformatted($"Manual settlement: trade {table.WinnerPayout:n0} to {table.Winner.Name}; keep {table.Rake:n0} house rake.");

            if (ImGui.Button("Send Winner"))
            {
                SendConfiguredMessage(plugin.Configuration.WinnerMessage);
            }
        }

        if (plugin.Configuration.ConfirmManualTradesOnly)
        {
            ImGui.TextWrapped("Trade controls are confirmation trackers. This plugin does not initiate or complete unattended trades.");
        }
    }

    private void SendConfiguredMessage(string template)
    {
        foreach (var line in FormatMessage(template).Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            var message = line.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                continue;
            }

            var command = BuildChatCommand(message);
            if (string.IsNullOrWhiteSpace(command))
            {
                Plugin.ChatGui.PrintError("All In War message was not sent. Configure a chat command such as /yell, or start the message line with a slash command.");
                continue;
            }

            Plugin.CommandManager.ProcessCommand(command);
        }
    }

    private string BuildChatCommand(string message)
    {
        if (message.StartsWith('/'))
        {
            return message;
        }

        var prefix = plugin.Configuration.ChatCommandPrefix.Trim();
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return string.Empty;
        }

        if (!prefix.StartsWith('/'))
        {
            prefix = "/" + prefix;
        }

        return $"{prefix} {message}";
    }

    private string FormatMessage(string template)
    {
        var winner = table.Winner;
        var winnerCard = winner is null || winner.Card == 0 ? string.Empty : AllInWarTable.CardName(winner.Card);

        return template
            .Replace("{buyIn}", table.BuyIn.ToString("n0"), StringComparison.OrdinalIgnoreCase)
            .Replace("{rake}", table.RakePercent.ToString("n0"), StringComparison.OrdinalIgnoreCase)
            .Replace("{pot}", table.Pot.ToString("n0"), StringComparison.OrdinalIgnoreCase)
            .Replace("{rakeAmount}", table.Rake.ToString("n0"), StringComparison.OrdinalIgnoreCase)
            .Replace("{payout}", table.WinnerPayout.ToString("n0"), StringComparison.OrdinalIgnoreCase)
            .Replace("{winner}", winner?.Name ?? "Winner", StringComparison.OrdinalIgnoreCase)
            .Replace("{card}", winnerCard, StringComparison.OrdinalIgnoreCase)
            .Replace("{players}", table.Players.Count.ToString("n0"), StringComparison.OrdinalIgnoreCase);
    }
}
