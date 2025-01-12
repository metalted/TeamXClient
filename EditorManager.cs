using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TeamXClient.Extensions;
using UnityEngine;

namespace TeamXClient
{
    /// <summary>
    /// Manages the editor's state, including blocks, floors, and skyboxes, 
    /// and provides methods to interact with and modify the editor's contents.
    /// </summary>
    public class EditorManager
    {
        /// <summary>
        /// The current floor ID in the editor.
        /// </summary>
        public int Floor { get; set; }

        /// <summary>
        /// The current skybox ID in the editor.
        /// </summary>
        public int Skybox { get; set; }

        /// <summary>
        /// A dictionary of blocks managed by the editor, indexed by their unique UIDs.
        /// </summary>
        private Dictionary<string, Block> Blocks { get; set; }

        /// <summary>
        /// Modifier for performing actions on the editor's contents.
        /// </summary>
        public EditorModifier Modifier { get; private set; }

        /// <summary>
        /// Observer for monitoring changes in the editor.
        /// </summary>
        public EditorObserver Observer { get; private set; }

        /// <summary>
        /// Reference to Zeepkists central editor script.
        /// </summary>
        public LEV_LevelEditorCentral Central;

        /// <summary>
        /// Observer for selection-related changes in the editor.
        /// </summary>
        public SelectionObserver SelectionObserver;        

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorManager"/> class.
        /// </summary>
        public EditorManager()
        {
            Floor = -1;
            Skybox = 0;
            Blocks = new Dictionary<string, Block>();
            Modifier = new EditorModifier(this);
            Observer = new EditorObserver(this);
        }

        /// <summary>
        /// Sets the central level editor object and initializes the selection observer.
        /// </summary>
        /// <param name="central">The <see cref="LEV_LevelEditorCentral"/> instance to set.</param>
        /// <remarks>
        /// This method associates the editor manager with a specific central level editor,
        /// enabling further interactions and modifications through the central object.
        /// It also ensures that a <see cref="SelectionObserver"/> is created and initialized for managing block selections.
        /// </remarks>
        public void SetCentral(LEV_LevelEditorCentral central)
        {
            Central = central;
            CreateSelectionObserver();
        }

        /// <summary>
        /// Creates and initializes a <see cref="SelectionObserver"/> on the central object if one does not already exist.
        /// </summary>
        /// <remarks>
        /// The <see cref="SelectionObserver"/> is responsible for observing and handling selection-related events in the editor.
        /// If the central object's GameObject already contains a <see cref="SelectionObserver"/>, this method does nothing.
        /// Otherwise, it adds the component and initializes it with the selection data from the editor's central object.
        /// </remarks>
        private void CreateSelectionObserver()
        {
            if (Central.gameObject.GetComponent<SelectionObserver>() == null)
            {
                SelectionObserver observer = Central.gameObject.AddComponent<SelectionObserver>();
                observer.Initialize(Plugin.Instance.editor.Central.selection);
            }
        }

        /// <summary>
        /// Checks if the editor is currently active.
        /// </summary>
        /// <returns>True if the editor is active; otherwise, false.</returns>
        public bool InLevelEditor()
        {
            return Central != null;
        }

        /// <summary>
        /// Updates the editor state with the specified data.
        /// </summary>
        /// <param name="stateData">The editor state data.</param>
        public void SetState(EditorStateData stateData)
        {
            Blocks.Clear();
            Floor = stateData.floor;
            Skybox = stateData.skybox;

            foreach (string s in stateData.blocks)
            {
                Block block = s.FromJson();
                Blocks.Add(block.UID, block);
            }
        }

        /// <summary>
        /// Instantiates blocks in the editor from the current state in batches.
        /// </summary>
        public IEnumerator InstantiateFromState()
        {
            yield return new WaitForEndOfFrame();
            Modifier.UpdateSkybox(Skybox);
            Modifier.UpdateFloor(Floor);

            /*int blockCount = Blocks.Count;
            int batchCount = Mathf.Max(10, Mathf.FloorToInt(blockCount / 60f));
            int counter = 0;*/

            foreach (KeyValuePair<string, Block> block in Blocks)
            {
                Modifier.CreateBlock(block.Value, false);
                /*counter++;
                if (counter % batchCount == 0)
                {
                    yield return new WaitForEndOfFrame();
                }*/
            }

            Central.validation.RecalcBlocksAndDraw(false);
        }

        /// <summary>
        /// Adds a new block to the editor state.
        /// </summary>
        /// <param name="block">The block to add.</param>
        public void Add(Block block)
        {
            if (!Blocks.ContainsKey(block.UID))
            {
                Blocks.Add(block.UID, block);
            }
        }

        /// <summary>
        /// Gets a block by its UID.
        /// </summary>
        /// <param name="uid">The UID of the block to retrieve.</param>
        /// <returns>The block with the specified UID, or null if not found.</returns>
        public Block Get(string uid)
        {
            Blocks.TryGetValue(uid, out Block block);
            return block;
        }

        public int GetBlockCountBy(ulong steamID)
        {
            int count = Blocks.Values.Count(block => block.SteamID == steamID);
            return count;
        }

        public int GetBlockCount()
        {
            return Blocks.Count;
        }

        /// <summary>
        /// Updates the data of an existing block.
        /// </summary>
        /// <param name="block">The block with updated data.</param>
        public void Update(Block block)
        {
            if (Blocks.ContainsKey(block.UID))
            {
                Blocks[block.UID] = block;
            }
        }

        /// <summary>
        /// Removes a block from the editor state by UID.
        /// </summary>
        /// <param name="uid">The UID of the block to remove.</param>
        public void Remove(string uid)
        {
            Blocks.Remove(uid);
        }

        /// <summary>
        /// Handles blocks added to the selection.
        /// Sends selection requests for blocks the client owns or has permission to modify.
        /// Reverts selection for blocks the client cannot modify.
        /// </summary>
        /// <param name="added">A list of block UIDs that were added to the selection.</param>
        public void OnBlocksAddedToSelection(List<string> added)
        {
            foreach (string uid in added)
            {
                Block block = Get(uid);
                if (block == null) continue;

                //Are we allowed to select this?
                bool isAllowed = (block.SteamID == Plugin.Instance.client.ClientSteamID || Plugin.Instance.perms.CanEditAll());

                if (isAllowed)
                {
                    Plugin.Instance.client.SendSelection(uid);
                }
                else
                {
                    Modifier.DeselectBlock(uid);
                }
            }
        }

        /// <summary>
        /// Handles blocks removed from the selection.
        /// Sends deselection requests for blocks the client owns or has permission to modify.
        /// </summary>
        /// <param name="removed">A list of block UIDs that were removed from the selection.</param>
        public void OnBlocksRemovedFromSelection(List<string> removed)
        {
            foreach (string uid in removed)
            {
                Block block = Get(uid);
                if (block == null) continue;

                //Are we allowed to select this?
                bool isAllowed = (block.SteamID == Plugin.Instance.client.ClientSteamID || Plugin.Instance.perms.CanEditAll());

                if (isAllowed)
                {
                    Plugin.Instance.client.SendDeselection(uid);
                }
            }
        }
    }

    [HarmonyPatch(typeof(LEV_ValidationLock), "RecalculateBlocks")]
    public class LEVValidationLockRecalculateBlocks
    {
        public static bool Prefix(ref bool setDebounce, LEV_ValidationLock __instance)
        {
            if(!Plugin.Instance.IsTeamXEditor())
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
}
