using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TeamX
{
    /// <summary>
    /// Observes and handles changes made in the editor, including block creation, updates, destruction,
    /// and changes to the floor or skybox.
    /// </summary>
    public class EditorObserver
    {
        /// <summary>
        /// The <see cref="EditorManager"/> instance that this modifier operates on.
        /// </summary>
        private readonly EditorManager editor;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorObserver"/> class.
        /// </summary>
        /// <param name="editor">The <see cref="EditorManager"/> instance associated with this observer.</param>
        public EditorObserver(EditorManager editor)
        {
            this.editor = editor ?? throw new ArgumentNullException(nameof(editor), "EditorManager cannot be null.");
        }

        /// <summary>
        /// Handles the creation of a new block in the editor.
        /// </summary>
        /// <param name="afterState">The state of the block after creation.</param>
        public void BlockCreated(BlockPropertyJSON afterState)
        {
            Block block = new Block()
            {
                ID = afterState.blockID,
                PositionX = afterState.position.x,
                PositionY = afterState.position.y,
                PositionZ = afterState.position.z,
                EulerAnglesX = afterState.eulerAngles.x,
                EulerAnglesY = afterState.eulerAngles.y,
                EulerAnglesZ = afterState.eulerAngles.z,
                LocalScaleX = afterState.localScale.x,
                LocalScaleY = afterState.localScale.y,
                LocalScaleZ = afterState.localScale.z,
                Properties = afterState.properties,
                UID = afterState.UID,
                SteamID = Plugin.Instance.client.ClientSteamID
            };

            //Store the block
            editor.Add(block);

            Plugin.Instance.client.SendBlockCreate(block);
        }

        /// <summary>
        /// Handles the update of an existing block in the editor.
        /// </summary>
        /// <param name="beforeState">The state of the block before the update.</param>
        /// <param name="afterState">The state of the block after the update.</param>
        public void BlockUpdated(BlockPropertyJSON beforeState, BlockPropertyJSON afterState)
        {
            //Get the block
            Block block = editor.Get(afterState.UID);
            
            //Are we the creator or have enough permission?
            if(block != null && (block.SteamID == Plugin.Instance.client.ClientSteamID || (byte)Plugin.Instance.client.PermissionLevel > 1))
            {
                //Save the change
                block.PositionX = afterState.position.x;
                block.PositionY = afterState.position.y;
                block.PositionZ = afterState.position.z;
                block.EulerAnglesX = afterState.eulerAngles.x;
                block.EulerAnglesY = afterState.eulerAngles.y;
                block.EulerAnglesZ = afterState.eulerAngles.z;
                block.LocalScaleX = afterState.localScale.x;
                block.LocalScaleY = afterState.localScale.y;
                block.LocalScaleZ = afterState.localScale.z;
                block.Properties = afterState.properties;

                Plugin.Instance.client.SendBlockUpdate(block);
            }
            //We cant edit this block, revert to before.
            else
            {
                //Remove the block from the selection if its in there
                int index = editor.Central.selection.list.FindIndex(s => s.UID == afterState.UID);

                if(index > 0)
                {
                    Debug.LogError(editor.Central.selection.list.Count);
                    if (editor.Central.selection.list.Count == 1)
                    {
                        editor.Central.selection.ClickNothing();
                        editor.Central.gizmos.Deactivate();                                               
                    }
                    else
                    {
                        editor.Central.selection.RemoveBlockAt(index, true, true);
                    }
                }

                editor.Modifier.UpdateBlock(beforeState);
            }
        }

        /// <summary>
        /// Handles the destruction of a block in the editor.
        /// </summary>
        /// <param name="beforeState">The state of the block before destruction.</param>
        public void BlockDestroyed(BlockPropertyJSON beforeState)
        {
            //Get the block
            Block block = editor.Get(beforeState.UID);

            //Are we the creator or have enough permission?
            if (block.SteamID == Plugin.Instance.client.ClientSteamID || (byte)Plugin.Instance.client.PermissionLevel > 1)
            {
                //Remove the block and send the update
                string uid = block.UID;

                editor.Remove(uid);

                Plugin.Instance.client.SendBlockDestroy(uid);
            }
            //We cant edit this block, revert to before.
            else
            {
                editor.Modifier.CreateBlock(beforeState);
            }
        }

        /// <summary>
        /// Handles updates to the editor's floor.
        /// </summary>
        /// <param name="before">The floor state before the update.</param>
        /// <param name="after">The floor state after the update.</param>
        public void FloorUpdated(int before, int after)
        {
            if((byte) Plugin.Instance.client.PermissionLevel > 1)
            {
                editor.Floor = after;

                Plugin.Instance.client.SendFloorUpdate(after);
            }
            //Not allowed
            else
            {
                editor.Modifier.UpdateFloor(before);
            }
        }

        /// <summary>
        /// Handles updates to the editor's skybox.
        /// </summary>
        /// <param name="before">The skybox state before the update.</param>
        /// <param name="after">The skybox state after the update.</param>
        public void SkyboxUpdated(int before, int after)
        {
            if ((byte)Plugin.Instance.client.PermissionLevel > 1)
            {
                editor.Skybox = after;

                Plugin.Instance.client.SendSkyboxUpdate(after);
            }
            //Not allowed
            else
            {
                editor.Modifier.UpdateSkybox(before);
            }
        }
    }

    /// <summary>
    /// Harmony patch for detecting changes in the editor and invoking appropriate handlers.
    /// </summary>
    [HarmonyPatch(typeof(LEV_UndoRedo), "SomethingChanged")]
    public class LEV_UndoRedoSomethingChangedPatch
    {
        public static void Postfix(ref Change_Collection whatChanged, ref string source)
        {
            if(Plugin.Instance.game.gameState == GameManager.GameState.TeamXEditor)
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
}
