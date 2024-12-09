using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamXClient
{
    /// <summary>
    /// Manages multiplayer functionality, including players, their states, and their in-game representations (Shpleeble).
    /// </summary>
    public class MultiplayerManager
    {
        /// <summary>
        /// Dictionary of players, keyed by their SteamID.
        /// </summary>
        private readonly Dictionary<ulong, PlayerData> players;

        /// <summary>
        /// Dictionary of in-game player representations (Shpleeble), keyed by their SteamID.
        /// </summary>
        private readonly Dictionary<ulong, Shpleeble> playerCharacters;

        /// <summary>
        /// The mode of the local player (e.g., Build, Race).
        /// </summary>
        public CharacterMode LocalPlayerMode;

        public int MaxBlockCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiplayerManager"/> class.
        /// </summary>
        public MultiplayerManager()
        {
            players = new Dictionary<ulong, PlayerData>();
            playerCharacters = new Dictionary<ulong, Shpleeble>();
        }

        /// <summary>
        /// Adds a new player to the multiplayer session.
        /// </summary>
        /// <param name="playerData">The data of the player to add.</param>
        public void AddPlayer(PlayerData playerData)
        {
            if (players.ContainsKey(playerData.steamID))
            {
                Plugin.Instance.Log($"Player with SteamID {playerData.steamID} already exists.", LogType.Warning);
                return;
            }

            players.Add(playerData.steamID, playerData);

            Shpleeble newPlayer = Utils.CreateShpleeble(playerData);

            if (newPlayer != null)
            {
                playerCharacters.Add(playerData.steamID, newPlayer);
                Plugin.Instance.Log($"Added player with SteamID {playerData.steamID} and created their Shpleeble.", LogType.Message);
            }
            else
            {
                Plugin.Instance.Log($"Failed to create Shpleeble for player with SteamID {playerData.steamID}.", LogType.Warning);
            }
        }

        /// <summary>
        /// Removes a player from the multiplayer session.
        /// </summary>
        /// <param name="steamID">The SteamID of the player to remove.</param>
        public void RemovePlayer(ulong steamID)
        {
            if (players.Remove(steamID))
            {
                Plugin.Instance.Log($"Player with SteamID {steamID} removed from the players dictionary.", LogType.Message);
            }
            else
            {
                Plugin.Instance.Log($"Player with SteamID {steamID} not found in the players dictionary.", LogType.Warning);
            }

            if (playerCharacters.TryGetValue(steamID, out Shpleeble shpleeble))
            {
                if (shpleeble != null)
                {
                    GameObject.Destroy(shpleeble.gameObject);
                }

                playerCharacters.Remove(steamID);
                Plugin.Instance.Log($"Shpleeble with SteamID {steamID} removed from the game.", LogType.Debug);
            }
            else
            {
                Plugin.Instance.Log($"Shpleeble with SteamID {steamID} not found in the playerCharacters dictionary.", LogType.Warning);
            }
        }

        /// <summary>
        /// Updates the state of an existing player in the multiplayer session.
        /// </summary>
        /// <param name="playerState">The state data of the player to update.</param>
        public void UpdatePlayerState(PlayerStateData playerState)
        {
            if (playerCharacters.TryGetValue(playerState.SteamID, out Shpleeble shpleeble))
            {
                if (shpleeble != null)
                {
                    shpleeble.UpdateTransform(playerState.Position, playerState.Rotation);
                    shpleeble.SetMode(playerState.Mode);
                    Plugin.Instance.Log($"Updated state for player with SteamID {playerState.SteamID}.", LogType.Debug);
                }
                else
                {
                    Plugin.Instance.Log($"Shpleeble with SteamID {playerState.SteamID} is null and cannot be updated.", LogType.Warning);
                }
            }
            else
            {
                Plugin.Instance.Log($"Shpleeble with SteamID {playerState.SteamID} not found in the playerCharacters dictionary.", LogType.Warning);
            }
        }

        public void ClearAllData()
        {
            List<ulong> ids = players.Keys.ToList();
            foreach(ulong id in ids)
            {
                RemovePlayer(id);
            }
        }
    }
}
