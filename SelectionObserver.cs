using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;

namespace TeamXClient
{
    /// <summary>
    /// Observes changes in the current block selection within the editor and notifies the editor manager of any additions or removals.
    /// </summary>
    public class SelectionObserver : MonoBehaviour
    {
        /// <summary>
        /// The selection object representing the currently selected blocks.
        /// </summary>
        public LEV_Selection Selection { get; private set; }

        /// <summary>
        /// Stores the UIDs of the last recorded selection.
        /// </summary>
        private HashSet<string> lastSelectionUIDs;

        /// <summary>
        /// Stores the UIDs of the current selection.
        /// </summary>
        private HashSet<string> currentUIDs;

        /// <summary>
        /// Stores the UIDs of blocks that have been removed from the selection.
        /// </summary>
        private List<string> removedUIDs;

        /// <summary>
        /// Stores the UIDs of blocks that have been added to the selection.
        /// </summary>
        private List<string> addedUIDs;

        /// <summary>
        /// Stores the last recorded count of the selection list.
        /// </summary>
        private int lastListCount = 0;

        /// <summary>
        /// Initializes the selection observer with the given selection object.
        /// </summary>
        /// <param name="selection">The selection object to observe.</param>
        public void Initialize(LEV_Selection selection)
        {
            Selection = selection;

            lastSelectionUIDs = new HashSet<string>();
            currentUIDs = new HashSet<string>();
            removedUIDs = new List<string>();
            addedUIDs = new List<string>();

            Plugin.Instance.editor.SelectionObserver = this;
        }

        /// <summary>
        /// Synchronizes the list count with the current selection's list count.
        /// </summary>
        public void SyncListCount()
        {
            lastListCount = Selection.list.Count;
        }

        /// <summary>
        /// Updates the selection observer, inspecting the selection if changes are detected.
        /// </summary>
        private void Update()
        {
            // Ensure this only runs in the TeamXEditor state.
            if (Plugin.Instance.game.gameState != GameManager.GameState.TeamXEditor)
            {
                return;
            }

            if (Selection != null)
            {
                int currentListCount = Selection.list.Count;

                // Check if the selection list count has changed.
                if (currentListCount != lastListCount)
                {
                    InspectSelection();
                    lastListCount = currentListCount;
                }
            }
        }

        /// <summary>
        /// Inspects the current selection, detecting additions and removals compared to the last selection.
        /// </summary>
        public void InspectSelection(bool notify = true)
        {
            // Clear current UIDs and populate with the current selection.
            currentUIDs.Clear();
            foreach (BlockProperties block in Selection.list)
            {
                currentUIDs.Add(block.UID);
            }

            // Determine removed and added UIDs by comparing the current selection to the last recorded selection.
            removedUIDs = lastSelectionUIDs.Except(currentUIDs).ToList();
            addedUIDs = currentUIDs.Except(lastSelectionUIDs).ToList();

            // Swap the last selection UIDs with the current UIDs for the next update.
            var temp = lastSelectionUIDs;
            lastSelectionUIDs = currentUIDs;
            currentUIDs = temp;

            // Notify the editor of changes.
            if (removedUIDs.Count > 0)
            {
                Plugin.Instance.Log("InspectSelection: Removed UIDs: " + removedUIDs.Count, LogType.Debug);
                if (notify)
                {
                    Plugin.Instance.editor.OnBlocksRemovedFromSelection(new List<string>(removedUIDs));
                }
                    
                removedUIDs.Clear();
            }

            if (addedUIDs.Count > 0)
            {
                Plugin.Instance.Log("InspectSelection: Added UIDs: " + addedUIDs.Count, LogType.Debug);
                if (notify)
                {
                    Plugin.Instance.editor.OnBlocksAddedToSelection(new List<string>(addedUIDs));
                }
                addedUIDs.Clear();
            }
        }
    }    
}
