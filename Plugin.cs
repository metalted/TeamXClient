using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;

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
        
        public MultiplayerManager multiplayer;
        public EditorManager editor;
        public GameManager game;

        public Client client;

        private bool init = false;
        public int logLevel = 1;

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
            //sid += ((uint)UnityEngine.Random.Range(100, 1000));
            //Debug.LogWarning("Generated STEAM ID: " + sid);

            client = new Client(sid);
            init = true;
        }

        public void OnApplicationQuit()
        {
            try
            {
                // Attempt to disconnect the client
                client.Disconnect();
            }
            catch (InvalidOperationException ex)
            {
                // Handle issues with client initialization or connection state
                //Console.WriteLine($"Operation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions
                //Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        public void TryToConnectToServer()
        {
            Log("Connecting...", LogType.Message);

            try
            {
                // Attempt to connect to the server
                client.Connect(cfg_serverIP.Value, cfg_serverPort.Value);
                Log("Successfully started connecting to the server.", LogType.Message);
            }
            catch (ArgumentException ex)
            {
                // Handle invalid IP address or port
                //Console.WriteLine($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                // Handle issues with client initialization or connection state
                //Console.WriteLine($"Operation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions
                //Console.WriteLine($"Unexpected error: {ex.Message}");
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
                    Plugin.Instance.Log($"Error: {ex.Message}", LogType.Error);
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
