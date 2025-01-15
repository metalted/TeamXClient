using System;
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
            WaitingOnServerRulesInMainMenu,
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
            //Initialize the plugin when we entered the main menu.
            Plugin.Instance.Initialize();

            //Create the blue button.
            InterfaceManager.SetupMainMenuUI();

            var client = Plugin.Instance.client;            

            //When connected or coming from team x editor, make sure we disconnect.
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

            //Set the new game state
            gameState = GameState.MainMenu;

            //Create the shpleeble prefab if not done so already.
            Utils.CreateShpleeblePrefabInMainMenu();

            //Clear all player data, like connected players and instantiated player models.
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
                    PlayerManager.Instance.weLoadedLevelEditorFromMainMenu = true;
                    gameState = GameState.TeamXEditor;
                }

                if (gameState == GameState.TeamXGame)
                {
                    PlayerManager.Instance.weLoadedLevelEditorFromMainMenu = false;
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

                    editor.InstantiateFromState();

                    var central = editor.Central;
                    var globalLevel = central.testMap.GlobalLevel;
                    var manager = central.manager;

                    InterfaceManager.SetupLevelEditorUI(Plugin.Instance.perms.IsAdmin());

                    if (!globalLevel.IsTestLevel)
                    {
                        return;
                    }

                    globalLevel.IsTestLevel = false;
                    manager.unsavedContent = false;

                    //Put the player back at their original location.
                    if (multiplayer.lastKnownEditorLocation.SteamID != 0)
                    {
                        //Data has been assigned previously as the SteamID has a value.
                        central.cam.transform.position = multiplayer.lastKnownEditorLocation.Position;
                    }

                    if (manager.weLoadedLevelEditorFromMainMenu)
                    {
                        return;
                    }

                    //Assign ctrl-z history list back to the game.
                    central.undoRedo.ResetUndoList(true);             
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

            if(gameState == GameState.TeamXEditor)
            {
                multiplayer.lastKnownEditorLocation = stateData;
            }

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
}
