# GamblingStats Plugin

GamblingStats is a Rust plugin for the Oxide modding framework that records and displays gambling statistics for players. The plugin keeps track of scrap spent, scrap lost, and scrap earned through gambling activities.

## Features

- Records gambling statistics for each player.
- Commands for players to view their own stats, top gamblers, and search for other players' stats.
- Admin command to delete a player's data.
- Periodic data saving and backup creation.
- Displays a welcome message with the player's profit/loss when they join the server.

## Commands

### Player Commands

- **/mystats**: View your gambling statistics.
  - **Description**: Shows the total scrap spent, lost, and earned through gambling.

- **/topstats**: View the top 10 gamblers.
  - **Description**: Displays the top 10 players with the most scrap earned through gambling.

- **/searchstats <PlayerName or Steam64ID>**: View gambling statistics for a specific player.
  - **Description**: Shows the gambling statistics for the specified player.

### Admin Commands

- **/deldata <Steam64ID>**: Delete a player's gambling data.
  - **Description**: Removes the gambling statistics of the specified player.
  - **Permission**: Requires `gamblingstats.admin` permission.

### Help Command

- **/help**: View the list of available commands with descriptions.
  - **Description**: Displays the list of player commands in red color.

## Permissions

- **gamblingstats.admin**: Required to use the `/deldata` command.

## Installation

1. Download the `GamblingStats.cs` file.
2. Place the file in your `oxide/plugins` directory.
3. Restart your Rust server or load the plugin using the `oxide.reload GamblingStats` command.

## Configuration

No configuration is needed for local storage. The plugin automatically saves and loads data from a local data file.

## Data Backup

- The plugin saves data every 10 minutes and creates backups of the data.
- A maximum of 5 backups are kept, with older backups being deleted automatically.

## Event Hooks

- **OnScrapSpent(BasePlayer player, int amount)**: Triggered when a player spends scrap on gambling.
- **OnScrapLost(BasePlayer player, int amount)**: Triggered when a player loses scrap in gambling.
- **OnScrapEarned(BasePlayer player, int amount)**: Triggered when a player earns scrap through gambling.

## Welcome Message

- When a player joins the server, the plugin displays a welcome message with their current gambling profit/loss.

## Credits

- **Author**: herbs.acab
- **Version**: 1.3.0

## Changelog

- **1.3.0**:
  - Added a welcome message displaying the player's profit/loss.
  - Added the `/help` command to display available commands.
  - Added the `/deldata` admin command to delete a player's data.
  - Highlighted commands in the help menu in red color.

- **1.2.0**:
  - Added periodic data saving and backup creation.
  - Improved data handling and storage.

- **1.1.0**:
  - Initial release with basic gambling statistics tracking and commands.
