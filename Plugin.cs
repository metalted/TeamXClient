using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;
using TeamXNetwork;

//TODO
//Add a way to download the level to your local machine, when you are admin.
//Add a way to save the current level.
//Add a chat interface and the messaging required.
//Add a way to assign regions to players.

namespace TeamXClient
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        //Plugin properties
        public const string pluginGUID = "com.metalted.zeepkist.teamx";
        public const string pluginName = "TeamX";
        public const string pluginVersion = "1.1.2";

        public static Plugin Instance;
        
        //Manages multiplayer models and references.
        public MultiplayerManager multiplayer;
        //Everything related to the level editor.
        public EditorManager editor;
        //Game events.
        public GameManager game;
        //Permission system.
        public PermissionManager perms;
        //Network interaction.
        public Client client;

        //Creates the client if not initialized yet.
        private bool init = false;
        //Which messages to show in the console (0 = debug, 1 = messages)
        public int logLevel = 1;

        //Config settings for ip address and port.
        public ConfigEntry<string> cfg_serverIP;
        public ConfigEntry<int> cfg_serverPort;

        private void Awake()
        {
            Harmony harmony = new Harmony(pluginGUID);
            harmony.PatchAll();
            Instance = this;

            PacketUtility.AutoRegisterPacketsInSameNamespace();

            game = new GameManager();
            multiplayer = new MultiplayerManager();
            editor = new EditorManager();
            perms = new PermissionManager();

            cfg_serverIP = Config.Bind("Settings", "Server IP", "127.0.0.1", "The IP address of the TeamX server");
            cfg_serverPort = Config.Bind("Settings", "Server Port", 8080, "The Port of the TeamX server.");
        }

        public int GetBlockAllowance()
        {
            if(game.gameState == GameManager.GameState.TeamXEditor)
            {
                int blockAllowance = perms.GetBlockLimit() - editor.GetBlockCountBy(client.ClientSteamID);
                blockAllowance = Mathf.Max(blockAllowance, 0);
                return blockAllowance;
                
            }
            else
            {
                return -1;
            }
        }

        public bool IsTeamXEditor()
        {
            return game.gameState == GameManager.GameState.TeamXEditor;
        }

        public void Initialize()
        {
            if (init)
            {
                return;
            }

            Log("Initializing TeamX...", LogType.Message);

            ulong sid = PlayerManager.Instance.steamAchiever.GetPlayerSteamID();

            //Some debug code to generate random player ids with only one player.
            //sid += ((uint)UnityEngine.Random.Range(100, 1000));
            //Debug.LogWarning("Generated STEAM ID: " + sid);

            client = new Client(sid);
            init = true;
        }

        public void OnApplicationQuit()
        {
            if(client != null)
            {
                client.AttemptDisconnect();
            }
        }        

        public void Update()
        {
            if (init)
            {
                try
                {
                    client.ProcessIncomingMessages();
                }
                catch (InvalidOperationException ex)
                {
                    Log($"Error: {ex.Message}", LogType.Error);
                }
            }
        }

        public void Log(string message, LogType logType, bool header = true)
        {
            var previousColor = Console.ForegroundColor;

            switch (logType)
            {
                case LogType.Debug:
                    if (logLevel <= 0)
                    {
                        Logger.LogInfo($" {(header ? "[TEAMX]" : "")} {message}");
                    }
                    break;
                case LogType.Message:
                    if (logLevel <= 1)
                    {
                        Logger.LogInfo($" {(header ? "[TEAMX]" : "")} {message}");
                    }
                    break;
                case LogType.Warning:
                    if (logLevel <= 2)
                    {
                        Logger.LogWarning($" {(header ? "[TEAMX]" : "")} {message}");
                    }
                    break;
                case LogType.Error:
                    if (logLevel <= 3)
                    {
                        Logger.LogError($" {(header ? "[TEAMX]" : "")} {message}");
                    }
                    break;
            }

            Console.ForegroundColor = previousColor;
        }
    }
}
