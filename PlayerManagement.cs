using System;
using System.Collections.Generic;
using UnityEngine;

namespace TeamX
{
    /*
    public static class PlayerManagement
    {
        
        public static Dictionary<int, Shpleeble> remotePlayers = new Dictionary<int, Shpleeble>();
        public static bool showRemotePlayers = true;

        public static void Initialize()
        {
            GameObserver.EnteredMainMenu += ProcessMainMenuEntry;
            NetworkController.ServerPlayerDataEvent += ProcessServerPlayerData;
            NetworkController.PlayerJoinedEvent += ProcessRemotePlayerJoin;
            NetworkController.PlayerLeftEvent += ProcessRemotePlayerLeft;
            NetworkController.PlayerTransformEvent += ProcessRemotePlayerTransformData;
            NetworkController.PlayerStateEvent += ProcessRemotePlayerStateData;
        }

        public static void Clear()
        {
            foreach(Shpleeble shpleeble in remotePlayers.Values)
            {
                if(shpleeble != null)
                {
                    GameObject.Destroy(shpleeble.gameObject);
                }
            }

            remotePlayers.Clear();
        }

        public static PlayerData GetLocalPlayerData()
        {
            PlayerData playerData = new PlayerData();
            playerData.state = 255;

            try
            {
                ZeepkistNetworking.CosmeticIDs cosmeticIDs = ProgressionManager.Instance.GetAdventureCosmetics();

                playerData.name = PlayerManager.Instance.steamAchiever.GetPlayerName(false);
                playerData.steamID = PlayerManager.Instance.steamAchiever.GetPlayerSteamID();
                playerData.zeepkist = cosmeticIDs.zeepkist;
                playerData.frontWheels = cosmeticIDs.frontWheels;
                playerData.rearWheels = cosmeticIDs.rearWheels;
                playerData.paraglider = cosmeticIDs.paraglider;
                playerData.horn = cosmeticIDs.horn;
                playerData.hat = cosmeticIDs.hat;
                playerData.glasses = cosmeticIDs.glasses;
                playerData.color_body = cosmeticIDs.color_body;
                playerData.color_leftArm = cosmeticIDs.color_leftArm;
                playerData.color_rightArm = cosmeticIDs.color_rightArm;
                playerData.color_leftLeg = cosmeticIDs.color_leftLeg;
                playerData.color_rightLeg = cosmeticIDs.color_rightLeg;
                playerData.color = cosmeticIDs.color;
            }
            catch (Exception e)
            {
                playerData.name = "Sphleeble";
                playerData.hat = 23000;
                playerData.color = 1000;
                playerData.zeepkist = 1000;

                playerData.zeepkist = 1000;
                playerData.frontWheels = 1000;
                playerData.rearWheels = 1000;
                playerData.paraglider = 1000;
                playerData.horn = 1000;
                playerData.hat = 23000;
                playerData.glasses = 1000;
                playerData.color_body = 1000;
                playerData.color_leftArm = 1000;
                playerData.color_rightArm = 1000;
                playerData.color_leftLeg = 1000;
                playerData.color_rightLeg = 1000;
                playerData.color = 1000;
            }

            return playerData;
        }

        public static PlayerData GetRemotePlayerData(int playerID)
        {
            if(!remotePlayers.ContainsKey(playerID))
            {
                return new PlayerData() { playerID = -1 };
            }

            return remotePlayers[playerID].GetPlayerData();
        }

        private static bool CreateRemotePlayer(PlayerData playerData)
        {
            if (remotePlayers.ContainsKey(playerData.playerID))
            {
                return false;
            }

            Shpleeble remotePlayer = GameObject.Instantiate<Shpleeble>(shpleeblePrefab);
            GameObject.DontDestroyOnLoad(remotePlayer);
            remotePlayer.SetPlayerData(playerData);
            remotePlayers.Add(playerData.playerID, remotePlayer);
            remotePlayer.gameObject.SetActive(showRemotePlayers);
            remotePlayer.Activate();

            return true;
        }       

        private static bool DestroyRemotePlayer(int playerID)
        {
            if (!remotePlayers.ContainsKey(playerID))
            {
                return false;
            }

            GameObject.Destroy(remotePlayers[playerID].gameObject);
            remotePlayers.Remove(playerID);
            return true;
        }

        private static void ProcessServerPlayerData(List<PlayerData> data)
        {
            foreach (PlayerData playerData in data)
            {
                CreateRemotePlayer(playerData);
            }
        }

        private static void ProcessRemotePlayerJoin(PlayerData data)
        {
            if (remotePlayers.ContainsKey(data.playerID))
            {
                return;
            }

            CreateRemotePlayer(data);
        }

        private static void ProcessRemotePlayerLeft(PlayerData data)
        {
            if (!remotePlayers.ContainsKey(data.playerID))
            {
                return;
            }

            DestroyRemotePlayer(data.playerID);
        }

        private static void ProcessRemotePlayerTransformData(PlayerTransformData playerTransformData)
        {
            if(!remotePlayers.ContainsKey(playerTransformData.playerID))
            {
                return;
            }

            remotePlayers[playerTransformData.playerID].UpdateTransform(playerTransformData.position, playerTransformData.euler);
        }

        private static void ProcessRemotePlayerStateData(PlayerStateData playerStateData)
        {
            if (!remotePlayers.ContainsKey(playerStateData.playerID))
            {
                return;
            }

            remotePlayers[playerStateData.playerID].SetMode(playerStateData.state);
        }
    
        private static void ProcessMainMenuEntry()
        {
            if (shpleeblePrefab == null)
            {
                shpleeblePrefab = TeamX.Utils.CreateShpleeblePrefabInMainMenu();
            }
        }
    }*/
}
