using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;
using TeamXNetwork;
using ZeepSDK.UI;

namespace TeamXClient
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        //Plugin properties
        public const string pluginGUID = "com.metalted.zeepkist.teamx";
        public const string pluginName = "TeamX";
        public const string pluginVersion = "1.2.3";

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
        public ConfigEntry<KeyCode> cfg_showPlayerList;
        public ConfigEntry<KeyCode> cfg_openChatField;
        public ConfigEntry<bool> cfg_chatEnabled;
        public ConfigEntry<KeyCode> cfg_toggleChat;
        public ConfigEntry<int> cfg_chatHistoryLength;

        //Honk honk
        public bool isRemoteHorn = false;

        private void Awake()
        {
            //Setup harmony for modding and patching.
            Harmony harmony = new Harmony(pluginGUID);
            harmony.PatchAll();
            Instance = this;

            //Register all the network messages.
            PacketUtility.AutoRegisterPacketsInSameNamespace();

            //Setup all the teamX classes.
            game = new GameManager();
            multiplayer = new MultiplayerManager();
            editor = new EditorManager();
            perms = new PermissionManager();

            //Create all the config entries.
            cfg_serverIP = Config.Bind("Settings", "Server IP", "127.0.0.1", "The IP address of the TeamX server");
            cfg_serverPort = Config.Bind("Settings", "Server Port", 8080, "The Port of the TeamX server.");
            cfg_showPlayerList = Config.Bind("Settings", "Show Player List", KeyCode.None, "Hold this key to show the current online players in the TeamX server.");
            cfg_openChatField = Config.Bind("Settings", "Open Chat Input Field", KeyCode.None, "Button to open the chat field. Don't use Return/Enter, this will cause issues.");
            cfg_chatEnabled = Config.Bind("Settings", "Chat History Window Visible", false, "Is the chat log visible on screen");
            cfg_toggleChat = Config.Bind("Settings", "Chat History Window Visible Key Toggle", KeyCode.None, "Toggle Chat Enabled setting with a key.");
            cfg_chatHistoryLength = Config.Bind("Settings", "Chat History Length", 5, "The maximum length of the chat history.");
            cfg_chatEnabled.SettingChanged += InterfaceManager.Cfg_chatEnabled_SettingChanged;
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

        public bool IsTeamXGame()
        {
            return game.gameState == GameManager.GameState.TeamXGame;
        }

        public void OnGUI()
        {
            InterfaceManager.OnGUILoop();
        }

        public void Initialize()
        {
            if (init)
            {
                return;
            }

            Log("Initializing TeamX...", LogType.Message);
            client = new Client(PlayerManager.Instance.steamAchiever.GetPlayerSteamID());
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

                InterfaceManager.UpdateLoop();                              
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
