using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TeamXClient
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
            int count = editor.GetBlockCountBy(Plugin.Instance.client.ClientSteamID);
            bool isAllowed = Plugin.Instance.perms.CanCreate() && count < Plugin.Instance.perms.GetBlockLimit();
            bool isBanned = Plugin.Instance.perms.IsBlockBanned(afterState.blockID);
            if (isAllowed && !isBanned)
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
            //Not allowed
            else
            {
                editor.Modifier.DestroyBlock(afterState.UID);
            }
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

            if(block == null)
            {
                Plugin.Instance.Log("BlockUpdate: Block = null", LogType.Error);

                //As the block is not found in the editor anymore, destroy it (this causes issues)
                //As the block is not found in the online editor anymore, recreate it so the games are synced again.

                block = new Block()
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
                return;
            }

            //Are we the creator or have enough permission?
            bool isAllowed = (Plugin.Instance.perms.CanEdit() && block.SteamID == Plugin.Instance.client.ClientSteamID) || Plugin.Instance.perms.CanEditAll();
                        
            if (isAllowed)
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

            if(block == null)
            {
                //This can happen with blocked creations in combination with control z
                Plugin.Instance.Log("BlockDestroy: Block = null, probably ctrl-z.", LogType.Debug);
                return;
            }

            //Are we the creator or have enough permission?
            bool isAllowed = (Plugin.Instance.perms.CanDestroy() && block.SteamID == Plugin.Instance.client.ClientSteamID) || Plugin.Instance.perms.CanEditAll();

            if (isAllowed)
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
            if (Plugin.Instance.perms.CanEditFloor())
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
            if (Plugin.Instance.perms.CanEditSkybox())
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
}
