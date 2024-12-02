using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamX
{
    public class MultiplayerManager
    {
        private Dictionary<ulong, PlayerData> players;
        private Dictionary<ulong, Shpleeble> playerCharacters;
        public CharacterMode LocalPlayerMode;
        
        public MultiplayerManager()
        {
            players = new Dictionary<ulong, PlayerData>();
            playerCharacters = new Dictionary<ulong, Shpleeble>();
        }

        public void AddPlayer(PlayerData playerData)
        {
            if(!players.ContainsKey(playerData.steamID))
            {
                players.Add(playerData.steamID, playerData);
                Shpleeble newPlayer = Utils.CreateShpleeble(playerData);
                playerCharacters.Add(playerData.steamID, newPlayer);
            }
        }

        public void RemovePlayer(ulong steamID)
        {
            if(players.ContainsKey(steamID))
            {
                players.Remove(steamID);
            }

            if(playerCharacters.ContainsKey(steamID))
            {
                if(playerCharacters[steamID] != null)
                {
                    GameObject.Destroy(playerCharacters[steamID].gameObject);
                }

                playerCharacters.Remove(steamID);
            }
        }

        public void UpdatePlayerState(PlayerStateData playerState)
        {
            if(playerCharacters.ContainsKey(playerState.SteamID))
            {
                Shpleeble s = playerCharacters[playerState.SteamID];
                if(s != null)
                {
                    s.UpdateTransform(playerState.Position, playerState.Rotation);
                    s.SetMode(playerState.Mode);
                }                
            }
        }
    }
}
