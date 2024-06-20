using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Database;
using System.Collections.Generic;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("GamblingStats", "herbs.acab", "1.1.0")]
    [Description("Records and displays gambling statistics for players.")]

    class GamblingStats : CovalencePlugin
    {
        class PlayerStats
        {
            public int ScrapSpent { get; set; }
            public int ScrapLost { get; set; }
            public int ScrapEarned { get; set; }
        }

        private Dictionary<string, PlayerStats> playerStats = new Dictionary<string, PlayerStats>();

        private bool useMySql;
        private MySql sql;
        private Connection connection;

        protected override void LoadDefaultConfig()
        {
            Config["StorageMethod"] = "internal"; // options: internal, mysql
            Config["MySqlHost"] = "localhost";
            Config["MySqlPort"] = 3306;
            Config["MySqlUser"] = "root";
            Config["MySqlPass"] = "password";
            Config["MySqlDb"] = "rust_gambling_stats";
            SaveConfig();
        }

        void Init()
        {
            useMySql = Config["StorageMethod"].ToString().ToLower() == "mysql";
            if (useMySql)
            {
                InitMySql();
            }
            LoadData();
        }

        void OnServerSave()
        {
            SaveData();
        }

        void OnServerShutdown()
        {
            SaveData();
        }

        // Commands
        [Command("mystats")]
        void MyStatsCommand(IPlayer player, string command, string[] args)
        {
            var stats = GetPlayerStats(player.Id);
            player.Message($"You have spent {stats.ScrapSpent} scrap, lost {stats.ScrapLost} scrap, and earned {stats.ScrapEarned} scrap through gambling.");
        }

        [Command("topstats")]
        void TopStatsCommand(IPlayer player, string command, string[] args)
        {
            var topStats = GetTopStats();
            foreach (var stat in topStats)
            {
                var targetPlayer = covalence.Players.FindPlayerById(stat.Key);
                if (targetPlayer != null)
                {
                    player.Message($"{targetPlayer.Name}: Spent {stat.Value.ScrapSpent}, Lost {stat.Value.ScrapLost}, Earned {stat.Value.ScrapEarned}");
                }
            }
        }

        [Command("searchstats")]
        void SearchStatsCommand(IPlayer player, string command, string[] args)
        {
            if (args.Length == 0)
            {
                player.Message("Usage: /searchstats <PlayerName or Steam64ID>");
                return;
            }

            var targetPlayer = FindPlayer(args[0]);
            if (targetPlayer == null)
            {
                player.Message("Player not found.");
                return;
            }

            var stats = GetPlayerStats(targetPlayer.Id);
            player.Message($"{targetPlayer.Name} has spent {stats.ScrapSpent} scrap, lost {stats.ScrapLost} scrap, and earned {stats.ScrapEarned} scrap through gambling.");
        }

        // Hook into specific gambling events
        void OnScrapSpent(BasePlayer player, int amount)
        {
            var stats = GetPlayerStats(player.UserIDString);
            stats.ScrapSpent += amount;
            SaveData();
        }

        void OnScrapLost(BasePlayer player, int amount)
        {
            var stats = GetPlayerStats(player.UserIDString);
            stats.ScrapLost += amount;
            SaveData();
        }

        void OnScrapEarned(BasePlayer player, int amount)
        {
            var stats = GetPlayerStats(player.UserIDString);
            stats.ScrapEarned += amount;
            SaveData();
        }

        // Helper methods to load, save, and get player stats
        void LoadData()
        {
            if (useMySql)
            {
                LoadDataFromMySql();
            }
            else
            {
                playerStats = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<string, PlayerStats>>(Name);
            }
        }

        void SaveData()
        {
            if (useMySql)
            {
                SaveDataToMySql();
            }
            else
            {
                Interface.Oxide.DataFileSystem.WriteObject(Name, playerStats);
            }
        }

        PlayerStats GetPlayerStats(string playerId)
        {
            if (!playerStats.TryGetValue(playerId, out var stats))
            {
                stats = new PlayerStats();
                playerStats[playerId] = stats;
            }
            return stats;
        }

        Dictionary<string, PlayerStats> GetTopStats()
        {
            return playerStats.OrderByDescending(s => s.Value.ScrapEarned).Take(10).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        IPlayer FindPlayer(string identifier)
        {
            return covalence.Players.FindPlayer(identifier);
        }

        // MySQL Integration
        void InitMySql()
        {
            sql = Interface.Oxide.GetLibrary<MySql>();
            var host = Config["MySqlHost"].ToString();
            var port = int.Parse(Config["MySqlPort"].ToString());
            var user = Config["MySqlUser"].ToString();
            var pass = Config["MySqlPass"].ToString();
            var db = Config["MySqlDb"].ToString();

            connection = sql.OpenDb($"{host}:{port}", user, pass, db, this);

            sql.Query(@"
                CREATE TABLE IF NOT EXISTS GamblingStats (
                    PlayerId VARCHAR(17) PRIMARY KEY,
                    ScrapSpent INT DEFAULT 0,
                    ScrapLost INT DEFAULT 0,
                    ScrapEarned INT DEFAULT 0
                )", connection);
        }

        void LoadDataFromMySql()
        {
            sql.Query("SELECT * FROM GamblingStats", connection, list =>
            {
                foreach (var entry in list)
                {
                    var playerId = entry["PlayerId"].ToString();
                    var stats = new PlayerStats
                    {
                        ScrapSpent = int.Parse(entry["ScrapSpent"].ToString()),
                        ScrapLost = int.Parse(entry["ScrapLost"].ToString()),
                        ScrapEarned = int.Parse(entry["ScrapEarned"].ToString())
                    };
                    playerStats[playerId] = stats;
                }
            });
        }

        void SaveDataToMySql()
        {
            foreach (var entry in playerStats)
            {
                sql.Insert(@$"
                    REPLACE INTO GamblingStats (PlayerId, ScrapSpent, ScrapLost, ScrapEarned) 
                    VALUES ('{entry.Key}', {entry.Value.ScrapSpent}, {entry.Value.ScrapLost}, {entry.Value.ScrapEarned})", connection);
            }
        }
    }
}
