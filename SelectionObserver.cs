using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamX
{
    public class SelectionObserver : MonoBehaviour
    {
        private LEV_Selection selection;

        private HashSet<string> lastSelectionUIDs;
        private HashSet<string> currentUIDs;
        private List<string> removedUIDs;
        private List<string> addedUIDs;
        private int lastListCount = 0;

        public void Initialize(LEV_Selection selection)
        {
            this.selection = selection;

            Debug.LogWarning("Initializing SelectionObserver");

            lastSelectionUIDs = new HashSet<string>();
            currentUIDs = new HashSet<string>();
            removedUIDs = new List<string>();
            addedUIDs = new List<string>();

            Plugin.Instance.editor.selectionObserver = this;
        }

        public void Update()
        {
            if (Plugin.Instance.game.gameState != GameManager.GameState.TeamXEditor)
            {
                return;
            }

            if(selection != null)
            {
                int currentListCount = selection.list.Count;
                if (currentListCount != lastListCount)
                {
                    InspectSelection();
                    lastListCount = currentListCount;
                }
            }            
        }

        public void InspectSelection()
        {
            currentUIDs.Clear();
            foreach (BlockProperties block in selection.list)
            {
                currentUIDs.Add(block.UID);
            }

            removedUIDs = lastSelectionUIDs.Except(currentUIDs).ToList();
            addedUIDs = currentUIDs.Except(lastSelectionUIDs).ToList();

            var temp = lastSelectionUIDs;
            lastSelectionUIDs = currentUIDs;
            currentUIDs = temp;

            if (removedUIDs.Count > 0)
            {
                Plugin.Instance.editor.OnBlocksRemovedFromSelection(removedUIDs);
            }

            if (addedUIDs.Count > 0)
            {
                Plugin.Instance.editor.OnBlocksAddedToSelection(addedUIDs);
            }
        }

        public void DeselectAllBlocks(bool notify = false)
        {
            selection.DeselectAllBlocks(true, "");

            if (!notify)
            {
                lastListCount = selection.list.Count;
            }
        }

        public void DeselectBlock(string uid, bool notify = false)
        {
            int blockIndex = selection.list.FindIndex(item => item.UID == uid);

            if (blockIndex != -1)
            {
                selection.RemoveBlockAt(blockIndex, true, true);

                if (!notify)
                {
                    lastListCount = selection.list.Count;
                }
            }
        }

        public void SelectBlock(string uid, bool notify = false)
        {
            int blockIndex = selection.list.FindIndex(item => item.UID == uid);
            if (blockIndex == -1)
            {
                if (selection.central.undoRedo.allBlocksDictionary.ContainsKey(uid))
                {
                    selection.AddThisBlock(selection.central.undoRedo.allBlocksDictionary[uid]);
                }

                if (!notify)
                {
                    lastListCount = selection.list.Count;
                }
            }
        }
    }
}
