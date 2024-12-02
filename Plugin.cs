using BepInEx;
using HarmonyLib;
using System;
using UnityEngine;

namespace TeamX
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGUID = "com.metalted.zeepkist.teamx";
        public const string pluginName = "TeamX";
        public const string pluginVersion = "1.0";

        public static Plugin Instance;
        public Client client;
        public MultiplayerManager multiplayer;
        public EditorManager editor;
        public GameManager game;

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
            client = new Client(PlayerManager.Instance.steamAchiever.GetPlayerSteamID());                    
            init = true;
        }

        public void OnApplicationQuit()
        {
            client.Disconnect();
        }

        public void Update()
        {
            if (init)
            {
                client.Run();

                if (Input.GetKeyDown(KeyCode.P))
                {
                    Log("Connecting...");
                    client.Connect("127.0.0.1", 8080);
                }
            }
        }

        public void Log(string message)
        {
            Logger.LogInfo(message);
        }
    }
}
