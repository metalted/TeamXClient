using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;
using TeamXNetwork;

/* Overview of scripts and to do's 
 * Plugin.cs                Done 
 * Block.cs                 Done
 * Client.cs                Done
 * EditorManager.cs         Done
 * EditorModifier.cs        Done
 * EditorObserver.cs        Done
 * GameManager.cs           Done
 * MultiplayerManager.cs    Done
 * PlayerObserver.cs        Done
 * SelectionObserver.cs     Done
 * Shpleeble.cs             Done
 * Utils.cs                 Done
 * 
 * We need to create an openable ui, in which we can set permissions for players
 * This needs to be a window similar to blueprintsX. We create a scrollable list that shows all the players.
 * It would be nice if we could somehow create checkboxes for each permission level, so you can simple select one of them for each player.
 * Then after we set the permissions we click the apply button.
 * 
 * We also need to be able to reload a save somehow. Like request a list of save names, select one and load it. Or maybe even make saves with names.
 * 
 */

namespace TeamXClient
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        //Plugin properties
        public const string pluginGUID = "com.metalted.zeepkist.teamx";
        public const string pluginName = "TeamX";
        public const string pluginVersion = "1.0";

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
