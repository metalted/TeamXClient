using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamXClient
{
    /// <summary>
    /// Harmony patch for detecting changes in the editor and invoking appropriate handlers.
    /// </summary>
    [HarmonyPatch(typeof(LEV_UndoRedo), "SomethingChanged")]
    public class LEV_UndoRedoSomethingChangedPatch
    {
        public static void Postfix(ref Change_Collection whatChanged, ref string source)
        {
            if (Plugin.Instance.game.gameState == GameManager.GameState.TeamXEditor)
            {
                foreach (Change_Single changeSingle in whatChanged.changeList)
                {
                    switch (whatChanged.changeType)
                    {
                        case Change_Collection.ChangeType.block:
                            if (changeSingle.before == null)
                            {
                                Plugin.Instance.editor.Observer.BlockCreated(Utils.GetFixedJSONBlock(changeSingle.after));
                            }
                            else if (changeSingle.after == null)
                            {
                                Plugin.Instance.editor.Observer.BlockDestroyed(Utils.GetFixedJSONBlock(changeSingle.before));
                            }
                            else
                            {
                                Plugin.Instance.editor.Observer.BlockUpdated(Utils.GetFixedJSONBlock(changeSingle.before), Utils.GetFixedJSONBlock(changeSingle.after));
                            }
                            break;
                        case Change_Collection.ChangeType.floor:
                            Plugin.Instance.editor.Observer.FloorUpdated(changeSingle.int_before, changeSingle.int_after);
                            break;
                        case Change_Collection.ChangeType.skybox:
                            Plugin.Instance.editor.Observer.SkyboxUpdated(changeSingle.int_before, changeSingle.int_after);
                            break;
                        case Change_Collection.ChangeType.selection:
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Harmony patch for detecting changes in the editor and invoking appropriate handlers.
    /// </summary>
    [HarmonyPatch(typeof(LEV_UndoRedo), "ApplyBeforeState")]
    public class LEV_UndoRedoApplyBeforeStatePatch
    {
        public static void Postfix(LEV_UndoRedo __instance)
        {
            if (Plugin.Instance.game.gameState == GameManager.GameState.TeamXEditor)
            {
                Change_Collection changes = __instance.historyList[__instance.currentHistoryPosition];

                foreach (Change_Single changeSingle in changes.changeList)
                {
                    switch (changes.changeType)
                    {
                        case Change_Collection.ChangeType.block:
                            if (changeSingle.before == null)
                            {
                                Plugin.Instance.editor.Observer.BlockDestroyed(Utils.GetFixedJSONBlock(changeSingle.after));
                            }
                            else if (changeSingle.after == null)
                            {
                                Plugin.Instance.editor.Observer.BlockCreated(Utils.GetFixedJSONBlock(changeSingle.before));
                            }
                            else
                            {
                                Plugin.Instance.editor.Observer.BlockUpdated(Utils.GetFixedJSONBlock(changeSingle.after), Utils.GetFixedJSONBlock(changeSingle.before));
                            }
                            break;
                        case Change_Collection.ChangeType.floor:
                            Plugin.Instance.editor.Observer.FloorUpdated(changeSingle.int_after, changeSingle.int_before);
                            break;
                        case Change_Collection.ChangeType.skybox:
                            Plugin.Instance.editor.Observer.SkyboxUpdated(changeSingle.int_after, changeSingle.int_before);
                            break;
                        case Change_Collection.ChangeType.selection:
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Harmony patch for detecting changes in the editor and invoking appropriate handlers.
    /// </summary>
    [HarmonyPatch(typeof(LEV_UndoRedo), "ApplyAfterState")]
    public class LEV_UndoRedoApplyAfterStatePatch
    {
        public static void Postfix(LEV_UndoRedo __instance)
        {
            if (Plugin.Instance.game.gameState == GameManager.GameState.TeamXEditor)
            {
                Change_Collection changes = __instance.historyList[__instance.currentHistoryPosition];

                foreach (Change_Single changeSingle in changes.changeList)
                {
                    switch (changes.changeType)
                    {
                        case Change_Collection.ChangeType.block:
                            if (changeSingle.before == null)
                            {
                                Plugin.Instance.editor.Observer.BlockCreated(Utils.GetFixedJSONBlock(changeSingle.after));
                            }
                            else if (changeSingle.after == null)
                            {
                                Plugin.Instance.editor.Observer.BlockDestroyed(Utils.GetFixedJSONBlock(changeSingle.before));
                            }
                            else
                            {
                                Plugin.Instance.editor.Observer.BlockUpdated(Utils.GetFixedJSONBlock(changeSingle.before), Utils.GetFixedJSONBlock(changeSingle.after));
                            }
                            break;
                        case Change_Collection.ChangeType.floor:
                            Plugin.Instance.editor.Observer.FloorUpdated(changeSingle.int_before, changeSingle.int_after);
                            break;
                        case Change_Collection.ChangeType.skybox:
                            Plugin.Instance.editor.Observer.SkyboxUpdated(changeSingle.int_before, changeSingle.int_after);
                            break;
                        case Change_Collection.ChangeType.selection:
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Harmony patch for making the Recalculate blocks function more performant in the TeamX editor.
    /// </summary>
    [HarmonyPatch(typeof(LEV_ValidationLock), "RecalculateBlocks")]
    public class LEVValidationLockRecalculateBlocks
    {
        public static bool Prefix(ref bool setDebounce, LEV_ValidationLock __instance)
        {
            if (!Plugin.Instance.IsTeamXEditor())
            {
                return true;
            }

            __instance.amountOfBlocks = Plugin.Instance.editor.GetBlockCount();
            __instance.levelTip.text = "TeamX";

            if (setDebounce)
            {
                __instance.recalcDebounce = 1;
            }

            return false;
        }
    }

    /// <summary>
    /// Harmony patch for making the Update Validation Text function more performant in the TeamX editor. Validation is kind of impossible and we dont need to iterate over all blocks as we already have the block count.
    /// </summary>
    [HarmonyPatch(typeof(LEV_ValidationLock), "UpdateValidationText")]
    public class LEVValidationUpdateValidationText
    {
        public static bool Prefix(LEV_ValidationLock __instance)
        {
            if (!Plugin.Instance.IsTeamXEditor())
            {
                return true;
            }

            __instance.debugText.text = __instance.amountOfBlocks.ToString() + " " + I2.Loc.LocalizationManager.GetTranslation("ABC_Blocks");

            return false;
        }
    }

    /// <summary>
    /// Harmony patch that's called when we enter the main menu and updates the GameManager.
    /// </summary>
    [HarmonyPatch(typeof(MainMenuUI), "Awake")]
    public class TKMainMenuUIAwakePatch
    {
        public static void Prefix()
        {
            Plugin.Instance.game.OnMainMenu();
        }
    }

    /// <summary>
    /// Harmony patch that's called when we enter the level editor and updates the GameManager.
    /// </summary>
    [HarmonyPatch(typeof(LEV_LevelEditorCentral), "Awake")]
    public class LevelEditorCentralAwakePatch
    {
        public static void Postfix(LEV_LevelEditorCentral __instance)
        {
            Plugin.Instance.game.OnLevelEditor(__instance);
        }
    }

    /// <summary>
    /// Harmony patch that's called when we enter a game mode and updates the GameManager.
    /// </summary>
    [HarmonyPatch(typeof(SetupGame), "Awake")]
    public class SetupGameAwakePatch
    {
        public static void Postfix(SetupGame __instance)
        {
            Plugin.Instance.game.OnGame(__instance);
        }
    }

    /// <summary>
    /// Harmony patch that's called when the local players gets spawned, so we can update the state and track that player.
    /// </summary>
    [HarmonyPatch(typeof(GameMaster), "SpawnPlayers")]
    public class GameMasterSpawnPlayersPatch
    {
        public static void Postfix(GameMaster __instance)
        {
            Plugin.Instance.game.OnSpawnPlayers(__instance);
        }
    }

    /// <summary>
    /// Harmony patch that's called when the local players state changes, by a gate or restart.
    /// </summary>
    [HarmonyPatch(typeof(New_ControlCar), "SetZeepkistState")]
    public class NewControlCarSetZeepkistStatePatch
    {
        public static void Prefix(ref byte newState, ref string source, ref bool playSound)
        {
            Plugin.Instance.game.OnStateChange(newState);
        }
    }

    /// <summary>
    /// Harmony patch that will make sure Zeepkist doesnt load its own file when returning to the level editor from testing.
    /// The level should always be loaded from the networked editor data.
    /// </summary>
    [HarmonyPatch(typeof(LEV_TestMap), "Start")]
    public class TKTestMapStartPatch
    {
        public static bool Prefix(LEV_TestMap __instance)
        {
            return Plugin.Instance.game.gameState != GameManager.GameState.TeamXEditor;
        }
    }

    /// <summary>
    /// Harmony patch called when blocks are duplicated in the level editor.
    /// </summary>
    [HarmonyPatch(typeof(LEV_GizmoHandler), "DuplicateSelectedObjects")]
    public class LEVGizmoHandlerDuplicateSelectedObjectsPatch
    {
        public static void Postfix()
        {
            if (Plugin.Instance.game.gameState == GameManager.GameState.TeamXEditor)
            {
                Plugin.Instance.editor.SelectionObserver.InspectSelection();
            }
        }
    }

    /// <summary>
    /// Harmony patch called when all blocks are deselected in the level editor.
    /// </summary>
    [HarmonyPatch(typeof(LEV_Selection), "DeselectAllBlocks")]
    public class LEVSelectionDeselectAllBlocksPatch
    {
        public static void Postfix()
        {
            if (Plugin.Instance.game.gameState == GameManager.GameState.TeamXEditor)
            {
                Plugin.Instance.editor.SelectionObserver.InspectSelection();
            }
        }
    }

    /// <summary>
    /// Harmony patch called when a block is selected. This is required as the observer is count based. If going from one selected to the other, it doesnt trigger.
    /// </summary>
    [HarmonyPatch(typeof(LEV_Selection), "ClickBuilding")]
    public class LEVSelectionClickBuilding
    {
        public static void Postfix()
        {
            if (Plugin.Instance.game.gameState == GameManager.GameState.TeamXEditor)
            {
                Plugin.Instance.editor.SelectionObserver.InspectSelection();
            }
        }
    }

    /// <summary>
    /// Harmony patch called whenever we press shift P. Required to not change skybox when pressing P in a panel.
    /// </summary>
    [HarmonyPatch(typeof(SkyboxManager), "PreviousCurrent")]
    public class SkyboxManager_PreviousCurrent
    {
        public static bool Prefix(SkyboxManager __instance)
        {
            if (InterfaceManager.overallPanelState == TeamXPanelState.Open)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Harmony patch called whenever we press P. Required to not change skybox when pressing P in a panel.
    /// </summary>
    [HarmonyPatch(typeof(SkyboxManager), "AdvanceCurrent")]
    public class SkyboxManager_AdvanceCurrent
    {
        public static bool Prefix(SkyboxManager __instance)
        {
            if (InterfaceManager.overallPanelState == TeamXPanelState.Open)
            {
                return false;
            }

            return true;
        }
    }
}
