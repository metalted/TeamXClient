using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TeamX
{
    public class EditorObserver
    {
        private EditorManager editor;

        public EditorObserver(EditorManager editor)
        {
            this.editor = editor;
        }

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

        public void BlockUpdated(BlockPropertyJSON beforeState, BlockPropertyJSON afterState)
        {
            //Get the block
            Block block = editor.Get(afterState.UID);
            
            //Are we the creator or have enough permission?
            if(block.SteamID == Plugin.Instance.client.ClientSteamID || (byte)Plugin.Instance.client.PermissionLevel > 1)
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
                int index = editor.central.selection.list.FindIndex(s => s.UID == afterState.UID);

                if(index > 0)
                {
                    Debug.LogError(editor.central.selection.list.Count);
                    if (editor.central.selection.list.Count == 1)
                    {
                        editor.central.selection.ClickNothing();
                        editor.central.gizmos.Deactivate();                                               
                    }
                    else
                    {
                        editor.central.selection.RemoveBlockAt(index, true, true);
                    }
                }

                editor.Modifier.UpdateBlock(beforeState);
            }
        }

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

        public void FloorUpdated(int before, int after)
        {
            if((byte) Plugin.Instance.client.PermissionLevel > 1)
            {
                editor.SetFloor(after);

                Plugin.Instance.client.SendFloorUpdate(after);
            }
            //Not allowed
            else
            {
                editor.Modifier.UpdateFloor(before);
            }
        }

        public void SkyboxUpdated(int before, int after)
        {
            if ((byte)Plugin.Instance.client.PermissionLevel > 1)
            {
                editor.SetSkybox(after);

                Plugin.Instance.client.SendSkyboxUpdate(after);
            }
            //Not allowed
            else
            {
                editor.Modifier.UpdateSkybox(before);
            }
        }
    }

    // Called when a change is made on an object.
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

    // Called when a change is undone.
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

    // Called when a change is redone.
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

    /*
    [HarmonyPatch(typeof(LEV_Selection), "RegisterManualSelectionBreakLock")]
    public class LEV_SelectionRegisterManualSelectionBreakLockPatch
    {
        public static void Postfix(ref List<string> selectionUIDs_before, ref List<string> selectionUIDs_after)
        {
            Debug.LogError("Before (" + selectionUIDs_before.Count + "): " + string.Join(',', selectionUIDs_before));
            Debug.LogError("After (" + selectionUIDs_after.Count + "): " + string.Join(',', selectionUIDs_after));
        }
    }

    [HarmonyPatch(typeof(LEV_Selection), "DeselectAllBlocks")]
    public class LEV_SelectionDeselectAllBlocksPatch
    {
        public static void Postfix()
        {
            Debug.LogError("Deselect All Blocks");
        }
    }

    [HarmonyPatch(typeof(LEV_Selection), "UndoRedoReselection")]
    public class LEV_SelectionUndoRedoReselection
    {
        public static void Postfix(ref List<BlockProperties> newSelection)
        {
            Debug.LogWarning("UndoRedoSelect (" + newSelection.Count + "): " + string.Join(',', newSelection.Select(bp => bp.UID).ToList()));
        }
    }*/
}
