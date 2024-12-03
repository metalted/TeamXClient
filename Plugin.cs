using BepInEx;
using HarmonyLib;
using System;
using UnityEngine;

namespace TeamX
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

        private void Awake()
        {
            Harmony harmony = new Harmony(pluginGUID);
            harmony.PatchAll();

            Instance = this;

            PacketUtility.AutoRegisterPacketsInSameNamespace();

            game = new GameManager();
            multiplayer = new MultiplayerManager();
            editor = new EditorManager();
        }

        public void Initialize()
        {
            if (init)
            {
                return;
            }

            Log("Initializing TeamX...");
            ulong sid = PlayerManager.Instance.steamAchiever.GetPlayerSteamID();
            sid += ((uint)UnityEngine.Random.Range(100, 1000));
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
                Console.WriteLine($"Operation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        public void Update()
        {
            if (init)
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    Log("Connecting...");
                    try
                    {
                        // Attempt to connect to the server
                        client.Connect("127.0.0.1", 8080);
                        Console.WriteLine("Successfully connecting to the server.");
                    }
                    catch (ArgumentException ex)
                    {
                        // Handle invalid IP address or port
                        Console.WriteLine($"Invalid input: {ex.Message}");
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Handle issues with client initialization or connection state
                        Console.WriteLine($"Operation failed: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        // Catch any unexpected exceptions
                        Console.WriteLine($"Unexpected error: {ex.Message}");
                    }
                }

                try
                {
                    client.ProcessIncomingMessages();
                }
                catch (InvalidOperationException ex)
                {
                    Plugin.Instance.Log($"Error: {ex.Message}");
                }
            }
        }

        public void Log(string message)
        {
            Logger.LogInfo(message);
        }
    }
}
