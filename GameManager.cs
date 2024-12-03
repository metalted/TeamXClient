using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamX
{
    public class GameManager
    {
        public enum GameState { StartUp, MainMenu, WaitingForAccess, WaitingOnEditorDataInMainMenu, EnteringTeamXFromMainMenu, TeamXEditor, TeamXGame };
        public GameState gameState;
        public Action<PlayerStateData> TransformChange;
        public void OnMainMenu()
        {
            Plugin.Instance.Initialize();

            if(gameState == GameState.TeamXEditor || Plugin.Instance.client.ConnectionStatus == ConnectionStatus.Connected)
            {
                try
                {
                    // Attempt to disconnect the client
                    Plugin.Instance.client.Disconnect();
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

            gameState = GameState.MainMenu;

            Utils.CreateShpleeblePrefabInMainMenu();
        }

        public void OnLevelEditor(LEV_LevelEditorCentral instance)
        {
            if (Plugin.Instance.client.ConnectionStatus == ConnectionStatus.Connected)
            {
                if (gameState == GameState.EnteringTeamXFromMainMenu)
                {
                    //Spawn players.
                    Debug.Log("Spawn players and shit");
                    gameState = GameState.TeamXEditor;
                }

                if(gameState == GameState.TeamXGame)
                {
                    gameState = GameState.TeamXEditor;
                }
                
                if (gameState == GameState.TeamXEditor)
                {
                    Plugin.Instance.multiplayer.LocalPlayerMode = CharacterMode.Build;

                    Plugin.Instance.editor.central = instance;


                    Plugin.Instance.multiplayer.LocalPlayerMode = CharacterMode.Build;

                    if (instance.cam.cameraTransform.gameObject.GetComponent<PlayerObserver>() == null)
                    {
                        instance.cam.cameraTransform.gameObject.AddComponent<PlayerObserver>();
                    }

                    Plugin.Instance.StartCoroutine(Plugin.Instance.editor.InstantiateFromState());

                    if (instance.gameObject.GetComponent<SelectionObserver>() == null)
                    {
                        SelectionObserver observer = instance.gameObject.AddComponent<SelectionObserver>();
                        observer.Initialize(Plugin.Instance.editor.central.selection);
                    }

                    if (!Plugin.Instance.editor.central.testMap.GlobalLevel.IsTestLevel)
                    {
                        return;
                    }

                    Plugin.Instance.editor.central.testMap.GlobalLevel.IsTestLevel = false;
                    Plugin.Instance.editor.central.manager.unsavedContent = false;


                    if (Plugin.Instance.editor.central.manager.weLoadedLevelEditorFromMainMenu)
                    {
                        return;
                    }

                    Plugin.Instance.editor.central.undoRedo.historyList = Plugin.Instance.editor.central.manager.tempUndoList;                    
                }
            }
        }

        public void OnGame(SetupGame instance)
        {
            if(gameState == GameState.TeamXEditor)
            {
                gameState = GameState.TeamXGame;
            }

            if (gameState == GameState.TeamXGame)
            {
                Plugin.Instance.multiplayer.LocalPlayerMode = CharacterMode.Race;

                if (Plugin.Instance.client.ConnectionStatus == ConnectionStatus.Connected)
                {
                    Plugin.Instance.multiplayer.LocalPlayerMode = CharacterMode.Race;
                }
            }
        }

        public void OnSpawnPlayers(GameMaster instance)
        {
            if (gameState == GameState.TeamXGame)
            {
                if (Plugin.Instance.client.ConnectionStatus == ConnectionStatus.Connected)
                {
                    Transform localRacer = instance.PlayersReady[0].transform;
                    if (localRacer.gameObject.GetComponent<PlayerObserver>() == null)
                    {
                        localRacer.gameObject.AddComponent<PlayerObserver>();
                    }
                }
            }
        }

        public void OnLocalTransformChange(PlayerStateData stateData)
        {
            stateData.Mode = (byte)Plugin.Instance.multiplayer.LocalPlayerMode;
            Plugin.Instance.client.SendPlayerState(stateData);
        }

        public void OnStateChange(byte state)
        {
            if (gameState == GameState.TeamXGame)
            {
                if (Plugin.Instance.client.ConnectionStatus == ConnectionStatus.Connected)
                {
                    if (state == (byte)3)
                    {
                        Plugin.Instance.multiplayer.LocalPlayerMode = (CharacterMode)2;
                    }
                    else
                    {
                        Plugin.Instance.multiplayer.LocalPlayerMode = (CharacterMode)1;
                    }
                }
            }
        }

        public void LoadIntoEditorX()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LevelEditor2");
        }
    }

    //Called when we enter the main menu.
    [HarmonyPatch(typeof(MainMenuUI), "Awake")]
    public class TKMainMenuUIAwakePatch
    {
        public static void Prefix()
        {
            Plugin.Instance.game.OnMainMenu();
        }
    }

    [HarmonyPatch(typeof(LEV_LevelEditorCentral), "Awake")]
    public class LevelEditorCentralAwakePatch
    {
        public static void Postfix(LEV_LevelEditorCentral __instance)
        {
            Plugin.Instance.game.OnLevelEditor(__instance);
        }
    }

    [HarmonyPatch(typeof(SetupGame), "Awake")]
    public class SetupGameAwakePatch
    {
        public static void Postfix(SetupGame __instance)
        {
            Plugin.Instance.game.OnGame(__instance);
        }
    }

    [HarmonyPatch(typeof(GameMaster), "SpawnPlayers")]
    public class GameMasterSpawnPlayersPatch
    {
        public static void Postfix(GameMaster __instance)
        {
            Plugin.Instance.game.OnSpawnPlayers(__instance);
        }
    }

    //Called when a players state changes
    [HarmonyPatch(typeof(New_ControlCar), "SetZeepkistState")]
    public class NewControlCarSetZeepkistStatePatch
    {
        public static void Prefix(ref byte newState, ref string source, ref bool playSound)
        {
            Plugin.Instance.game.OnStateChange(newState);
        }
    }

    //This patch will make sure Zeepkist doesnt load its own file when returning to the level editor from testing.
    //The level should always be loaded from the storage script.
    [HarmonyPatch(typeof(LEV_TestMap), "Start")]
    public class TKTestMapStartPatch
    {
        public static bool Prefix(LEV_TestMap __instance)
        {
            return Plugin.Instance.game.gameState != GameManager.GameState.TeamXEditor;
        }
    }
}
