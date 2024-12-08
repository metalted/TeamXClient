using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamXClient
{
    /// <summary>
    /// Manages the game's state and transitions between different states such as MainMenu, LevelEditor, and Game.
    /// Handles actions like player spawning, state changes, and client connections.
    /// </summary>
    public class GameManager
    {
        /// <summary>
        /// Represents the various states the game can be in.
        /// </summary>
        public enum GameState
        {
            StartUp,
            MainMenu,
            WaitingForAccess,
            WaitingOnEditorDataInMainMenu,
            EnteringTeamXFromMainMenu,
            TeamXEditor,
            TeamXGame
        }

        /// <summary>
        /// The current state of the game.
        /// </summary>
        public GameState gameState;

        /// <summary>
        /// Handles actions to perform when entering the Main Menu.
        /// </summary>
        public void OnMainMenu()
        {
            Plugin.Instance.Initialize();

            var client = Plugin.Instance.client;            

            if (gameState == GameState.TeamXEditor || client.ConnectionStatus == ConnectionStatus.Connected)
            {
                try
                {
                    client.Disconnect();
                }
                catch (InvalidOperationException ex)
                {
                    Debug.LogError($"Failed to disconnect: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Unexpected error during disconnect: {ex.Message}");
                }
            }

            gameState = GameState.MainMenu;

            Utils.CreateShpleeblePrefabInMainMenu();
            Plugin.Instance.multiplayer.ClearAllData();
        }

        /// <summary>
        /// Handles actions to perform when entering the Level Editor.
        /// </summary>
        /// <param name="instance">The <see cref="LEV_LevelEditorCentral"/> instance representing the editor's central manager.</param>
        public void OnLevelEditor(LEV_LevelEditorCentral instance)
        {
            var client = Plugin.Instance.client;
            var editor = Plugin.Instance.editor;
            var multiplayer = Plugin.Instance.multiplayer;

            if (client.ConnectionStatus == ConnectionStatus.Connected)
            {
                if (gameState == GameState.EnteringTeamXFromMainMenu)
                {
                    Debug.Log("Spawning players.");
                    gameState = GameState.TeamXEditor;
                }

                if (gameState == GameState.TeamXGame)
                {
                    gameState = GameState.TeamXEditor;
                }

                if (gameState == GameState.TeamXEditor)
                {
                    multiplayer.LocalPlayerMode = CharacterMode.Build;
                    editor.SetCentral(instance);

                    var cameraTransform = instance.cam.cameraTransform;
                    if (cameraTransform.gameObject.GetComponent<PlayerObserver>() == null)
                    {
                        cameraTransform.gameObject.AddComponent<PlayerObserver>();
                    }

                    Plugin.Instance.StartCoroutine(editor.InstantiateFromState());

                    var central = editor.Central;
                    var globalLevel = central.testMap.GlobalLevel;
                    var manager = central.manager;

                    if (!globalLevel.IsTestLevel)
                    {
                        return;
                    }

                    globalLevel.IsTestLevel = false;
                    manager.unsavedContent = false;

                    if (manager.weLoadedLevelEditorFromMainMenu)
                    {
                        return;
                    }

                    central.undoRedo.historyList = manager.tempUndoList;
                }
            }
        }

        /// <summary>
        /// Handles actions to perform when entering the game.
        /// </summary>
        /// <param name="instance">The <see cref="SetupGame"/> instance representing the game setup.</param>
        public void OnGame(SetupGame instance)
        {
            var client = Plugin.Instance.client;
            var multiplayer = Plugin.Instance.multiplayer;

            if (gameState == GameState.TeamXEditor)
            {
                gameState = GameState.TeamXGame;
            }

            if (gameState == GameState.TeamXGame)
            {
                multiplayer.LocalPlayerMode = CharacterMode.Race;

                if (client.ConnectionStatus == ConnectionStatus.Connected)
                {
                    multiplayer.LocalPlayerMode = CharacterMode.Race;
                }
            }
        }

        /// <summary>
        /// Handles actions to perform when spawning players in the game.
        /// </summary>
        /// <param name="instance">The <see cref="GameMaster"/> instance responsible for managing player spawns.</param>
        public void OnSpawnPlayers(GameMaster instance)
        {
            var client = Plugin.Instance.client;

            if (gameState == GameState.TeamXGame && client.ConnectionStatus == ConnectionStatus.Connected)
            {
                var localRacer = instance.PlayersReady[0].transform;

                if (localRacer.gameObject.GetComponent<PlayerObserver>() == null)
                {
                    localRacer.gameObject.AddComponent<PlayerObserver>();
                }
            }
        }

        /// <summary>
        /// Called from a Player Observer script when the transform changes. Sends the player's updated transform to the server.
        /// </summary>
        /// <param name="stateData">The <see cref="PlayerStateData"/> containing the player's transform data.</param>
        public void OnLocalTransformChange(PlayerStateData stateData)
        {
            var multiplayer = Plugin.Instance.multiplayer;
            var client = Plugin.Instance.client;

            stateData.Mode = (byte)multiplayer.LocalPlayerMode;
            client.SendPlayerState(stateData);
        }

        /// <summary>
        /// Handles changes in the player's state while racing (e.g., Soapbox, Paraglider).
        /// </summary>
        /// <param name="state">The new state of the player.</param>
        public void OnStateChange(byte state)
        {
            var client = Plugin.Instance.client;
            var multiplayer = Plugin.Instance.multiplayer;

            if (gameState == GameState.TeamXGame && client.ConnectionStatus == ConnectionStatus.Connected)
            {
                multiplayer.LocalPlayerMode = state == 3 ? (CharacterMode)2 : (CharacterMode)1;
            }
        }

        /// <summary>
        /// Loads the editor scene from the game.
        /// </summary>
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
