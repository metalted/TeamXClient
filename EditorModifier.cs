using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamXClient.Extensions;
using UnityEngine;

namespace TeamXClient
{
    /// <summary>
    /// Provides functionality to modify the editor's state, including creating, updating, and destroying blocks,
    /// as well as updating the floor and skybox.
    /// </summary>
    public class EditorModifier
    {
        /// <summary>
        /// The <see cref="EditorManager"/> instance that this modifier operates on.
        /// </summary>
        private readonly EditorManager editor;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorModifier"/> class.
        /// </summary>
        /// <param name="editor">The <see cref="EditorManager"/> instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided editor is null.</exception>
        public EditorModifier(EditorManager editor)
        {
            this.editor = editor ?? throw new ArgumentNullException(nameof(editor), "EditorManager cannot be null.");
        }

        /// <summary>
        /// Creates a block in the editor using a <see cref="Block"/> object.
        /// </summary>
        /// <param name="block">The block to create.</param>
        /// <exception cref="ArgumentNullException">Thrown if the block is null.</exception>
        public void CreateBlock(Block block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block), "Block cannot be null.");
            }

            BlockPropertyJSON blockPropertyJSON = block.ToBlockPropertyJSON();
            CreateBlock(blockPropertyJSON);
        }

        /// <summary>
        /// Creates a block in the editor using a <see cref="BlockPropertyJSON"/> object.
        /// </summary>
        /// <param name="blockPropertyJSON">The block properties to use for creating the block.</param>
        /// <returns>True if the block was created successfully.</returns>
        public void CreateBlock(BlockPropertyJSON blockPropertyJSON)
        {
            if (blockPropertyJSON == null)
            {
                throw new ArgumentNullException(nameof(blockPropertyJSON), "BlockPropertyJSON cannot be null.");
            }

            editor.Central.undoRedo.GenerateNewBlock(blockPropertyJSON, blockPropertyJSON.UID);
            editor.Central.validation.RecalcBlocksAndDraw(false);
        }

        /// <summary>
        /// Updates a block in the editor using a <see cref="BlockPropertyJSON"/> object.
        /// </summary>
        /// <param name="jsonBlock">The block properties to use for updating the block.</param>
        /// <exception cref="ArgumentNullException">Thrown if jsonBlock is null.</exception>
        public void UpdateBlock(BlockPropertyJSON jsonBlock)
        {
            if (jsonBlock == null)
            {
                throw new ArgumentNullException(nameof(jsonBlock), "BlockPropertyJSON cannot be null.");
            }

            BlockProperties blockProperties = editor.Central.undoRedo.TryGetBlockFromAllBlocks(jsonBlock.UID);

            if (blockProperties != null)
            {
                editor.Central.undoRedo.allBlocksDictionary.Remove(jsonBlock.UID);
                BlockPropertyJSON blockPropertyJSON = blockProperties.ConvertBlockToJSON_v15();
                GameObject.Destroy(blockProperties.gameObject);

                jsonBlock.CopyTo(blockPropertyJSON);

                editor.Central.undoRedo.GenerateNewBlock(blockPropertyJSON, blockPropertyJSON.UID);
            }
        }

        /// <summary>
        /// Destroys a block in the editor using its UID.
        /// </summary>
        /// <param name="uid">The unique identifier (UID) of the block to destroy.</param>
        /// <exception cref="ArgumentException">Thrown if UID is null or empty.</exception>
        public void DestroyBlock(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                throw new ArgumentException("UID cannot be null or empty.", nameof(uid));
            }

            BlockProperties blockProperties = editor.Central.undoRedo.TryGetBlockFromAllBlocks(uid);

            if (blockProperties != null)
            {
                editor.Central.undoRedo.allBlocksDictionary.Remove(uid);
                GameObject.Destroy(blockProperties.gameObject);
                editor.Central.validation.RecalcBlocksAndDraw(false);
            }
        }

        /// <summary>
        /// Updates the editor's floor with the specified material ID.
        /// </summary>
        /// <param name="paintID">The ID of the material to apply to the floor.</param>
        public void UpdateFloor(int paintID)
        {
            editor.Central.painter.SetLoadGroundMaterial(paintID);
        }

        /// <summary>
        /// Updates the editor's skybox with the specified skybox ID.
        /// </summary>
        /// <param name="skyboxID">The ID of the skybox to set.</param>
        public void UpdateSkybox(int skyboxID)
        {
            editor.Central.skybox.SetToSkybox(skyboxID, true);
        }

        /// <summary>
        /// Deselects all blocks in the editor.
        /// </summary>
        /// <param name="notify">Whether the deselection should cause a function call from the selection observer.</param>
        public void DeselectAllBlocks(bool notify = false)
        {
            Plugin.Instance.editor.SelectionObserver.Selection.DeselectAllBlocks(true, "");

            if (!notify)
            {
                Plugin.Instance.editor.SelectionObserver.SyncListCount();
            }
        }

        /// <summary>
        /// Deselects a specific block in the editor by its UID.
        /// </summary>
        /// <param name="uid">The unique identifier (UID) of the block to deselect.</param>
        /// <param name="notify">Whether the deselection should cause a function call from the selection observer.</param>
        /// <exception cref="ArgumentException">Thrown if UID is null or empty.</exception>
        public void DeselectBlock(string uid, bool notify = false)
        {
            if (string.IsNullOrEmpty(uid))
            {
                throw new ArgumentException("UID cannot be null or empty.", nameof(uid));
            }

            int blockIndex = Plugin.Instance.editor.SelectionObserver.Selection.list.FindIndex(item => item.UID == uid);

            if (blockIndex != -1)
            {
                Plugin.Instance.editor.SelectionObserver.Selection.RemoveBlockAt(blockIndex, true, false);

                if (!notify)
                {
                    Plugin.Instance.editor.SelectionObserver.SyncListCount();
                    Plugin.Instance.editor.SelectionObserver.InspectSelection(false);
                }
            }
        }

        /// <summary>
        /// Selects a specific block in the editor by its UID.
        /// </summary>
        /// <param name="uid">The unique identifier (UID) of the block to select.</param>
        /// <param name="notify">Whether the selection should cause a function call from the selection observer.</param>
        /// <exception cref="ArgumentException">Thrown if UID is null or empty.</exception>
        public void SelectBlock(string uid, bool notify = false)
        {
            if (string.IsNullOrEmpty(uid))
            {
                throw new ArgumentException("UID cannot be null or empty.", nameof(uid));
            }

            int blockIndex = Plugin.Instance.editor.SelectionObserver.Selection.list.FindIndex(item => item.UID == uid);

            if (blockIndex == -1 && Plugin.Instance.editor.Central.undoRedo.allBlocksDictionary.ContainsKey(uid))
            {
                Plugin.Instance.editor.SelectionObserver.Selection.AddThisBlock(
                    Plugin.Instance.editor.Central.undoRedo.allBlocksDictionary[uid]);

                if (!notify)
                {
                    Plugin.Instance.editor.SelectionObserver.SyncListCount();
                }
            }
        }
    }
}
