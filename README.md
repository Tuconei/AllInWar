# All In War

All In War is a Dalamud table assistant for running a simple high-card gambling game.

## Current Rules

- Up to 8 seated players, including the dealer if desired.
- Buy-in is entered per round in gil.
- Cards are represented as rolls from 1 to 13.
- Ace is 1; King is 13.
- Highest card wins the pot, with Ace beating 10 through King.
- Tied high cards enter war.
- War players either match the original buy-in again or surrender.
- Winner receives the pot after configurable house rake.

## Current Plugin Scope

The plugin currently tracks the table, rolls cards, resolves ties, calculates rake/payout, sends configurable rules/collection/winner announcements from explicit button clicks, and records manual trade confirmations.

It does not initiate, automate, or complete unattended trades. Dalamud's published plugin restrictions caution against automatic interactions with game servers, so settlement is presented as an explicit manual instruction and tracked with status buttons.

## Building

1. Open `AllInWar.sln` in Visual Studio or Rider.
2. Build the solution.
3. The debug plugin DLL is emitted to:

   `AllInWar/bin/x64/Debug/AllInWar.dll`

## In Game

Use `/allinwar` to open the table assistant.
