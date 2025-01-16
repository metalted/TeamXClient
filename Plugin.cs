using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;
using TeamXNetwork;
using ZeepSDK.Utilities;
using ZeepSDK.Controls;

namespace TeamXClient
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        //Plugin properties
        public const string pluginGUID = "com.metalted.zeepkist.teamx";
        public const string pluginName = "TeamX";
        public const string pluginVersion = "1.2.1";

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

        //A simple bool that when true will show a gui player list.
        private bool playerListShouldBeVisible = false;

        //A bool that keeps track if the chat input box is open or not.
        private bool chatInputIsOpen = false;
        public bool chatInputNeedsFocus = false;

        //Blocker used for blocking inputs when the chat input is open.
        private DisposableBag currentBlocker;

        //Honk honk
        public bool isRemoteHorn = false;

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
            cfg_showPlayerList = Config.Bind("Settings", "Show Player List", KeyCode.None, "Hold this key to show the current online players in the TeamX server.");
            cfg_openChatField = Config.Bind("Settings", "Open Chat Input Field", KeyCode.None, "Button to open the chat field. Don't use Return/Enter, this will cause issues.");
            cfg_chatEnabled = Config.Bind("Settings", "Chat Enabled", false, "Is the chat log visible on screen");
            cfg_toggleChat = Config.Bind("Settings", "Toggle Chat", KeyCode.None, "Toggle Chat Enabled setting with a key.");

            cfg_chatEnabled.SettingChanged += Cfg_chatEnabled_SettingChanged;
        }

        /// <summary>
        /// Called when the chat enabled setting has changed so we can enable or disable the game object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cfg_chatEnabled_SettingChanged(object sender, EventArgs e)
        {
            if (IsTeamXEditor())
            {
                if (cfg_chatEnabled.Value)
                {
                    InterfaceManager.chatText.gameObject.SetActive(true);
                }
                else
                {
                    InterfaceManager.chatText.gameObject.SetActive(false);
                }
            }
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

        public void OnGUI()
        {
            if(IsTeamXEditor())
            {
                if(playerListShouldBeVisible)
                {
                    InterfaceManager.ShowPlayerList();
                }

                if(chatInputIsOpen)
                {
                    InterfaceManager.ShowChatInput();
                }
            }
        }

        public void Initialize()
        {
            if (init)
            {
                return;
            }

            Log("Initializing TeamX...", LogType.Message);

            ulong sid = PlayerManager.Instance.steamAchiever.GetPlayerSteamID();
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

                if(Input.GetKeyDown(cfg_showPlayerList.Value))
                {
                    playerListShouldBeVisible = true;
                }

                if(Input.GetKeyUp(cfg_showPlayerList.Value))
                {
                    playerListShouldBeVisible = false;
                }
                
                //If the open chat box is pressed
                if(Input.GetKeyUp(cfg_openChatField.Value))
                {
                    if (!chatInputIsOpen)
                    {
                        chatInputIsOpen = true;
                        chatInputNeedsFocus = true;

                        //Block input 
                        currentBlocker = ControlsApi.DisableAllInput();
                    }
                }

                //Toggling chat
                if(Input.GetKeyDown(cfg_toggleChat.Value))
                {
                    cfg_chatEnabled.Value = !cfg_chatEnabled.Value;

                    if(IsTeamXEditor())
                    {
                        if(cfg_chatEnabled.Value)
                        {
                            InterfaceManager.chatText.gameObject.SetActive(true);
                        }
                        else
                        {
                            InterfaceManager.chatText.gameObject.SetActive(false);
                        }
                    }
                }                  
            }
        }

        public void ProcessChatInput()
        {
            //Get Text
            string currentText = InterfaceManager.chatInputText;

            //Clear field
            InterfaceManager.chatInputText = "";

            //Close chat field
            chatInputIsOpen = false;

            if(!string.IsNullOrEmpty(currentText))
            {
                client.SendChatMessage(currentText);
            }

            //Make sure to release inputs
            currentBlocker.Dispose();
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
