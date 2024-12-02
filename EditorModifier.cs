using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamX
{
    public class EditorModifier
    {
        private EditorManager editor;
        
        public EditorModifier(EditorManager editor)
        {
            this.editor = editor;
        }

        public void CreateBlock(Block block)
        {
            BlockPropertyJSON json = Utils.BlockToBlockPropertyJSON(block);
            editor.central.undoRedo.GenerateNewBlock(json, json.UID);
            editor.central.validation.RecalcBlocksAndDraw(false);
        }

        public bool CreateBlock(BlockPropertyJSON blockPropertyJSON)
        {
            editor.central.undoRedo.GenerateNewBlock(blockPropertyJSON, blockPropertyJSON.UID);
            editor.central.validation.RecalcBlocksAndDraw(false);
            return true;
        }

        public bool CreateBlock(string blockString)
        {
            BlockPropertyJSON blockPropertyJSON = LEV_UndoRedo.GetJSONblock(blockString);
            CreateBlock(blockPropertyJSON);
            return true;
        }

        public void UpdateBlock(string uid, string properties)
        {
            BlockProperties blockProperties = editor.central.undoRedo.TryGetBlockFromAllBlocks(uid);

            if (blockProperties != null)
            {
                editor.central.undoRedo.allBlocksDictionary.Remove(uid);
                BlockPropertyJSON blockPropertyJSON = blockProperties.ConvertBlockToJSON_v15();
                GameObject.Destroy(blockProperties.gameObject);

                Utils.AssignPropertyListToBlockPropertyJSON(properties, blockPropertyJSON);
                editor.central.undoRedo.GenerateNewBlock(blockPropertyJSON, blockPropertyJSON.UID);
            }
        }

        public void UpdateBlock(BlockPropertyJSON jsonBlock)
        {
            BlockProperties blockProperties = editor.central.undoRedo.TryGetBlockFromAllBlocks(jsonBlock.UID);

            if (blockProperties != null)
            {
                editor.central.undoRedo.allBlocksDictionary.Remove(jsonBlock.UID);
                BlockPropertyJSON blockPropertyJSON = blockProperties.ConvertBlockToJSON_v15();
                GameObject.Destroy(blockProperties.gameObject);

                Utils.CopyFromTo(jsonBlock, blockPropertyJSON);

                editor.central.undoRedo.GenerateNewBlock(blockPropertyJSON, blockPropertyJSON.UID);
            }
        }

        public void DestroyBlock(string uid)
        {
            BlockProperties blockProperties = editor.central.undoRedo.TryGetBlockFromAllBlocks(uid);

            if (blockProperties != null)
            {
                editor.central.undoRedo.allBlocksDictionary.Remove(uid);
                GameObject.Destroy(blockProperties.gameObject);
                editor.central.validation.RecalcBlocksAndDraw(false);
            }
        }     
        
        public void UpdateFloor(int paintID)
        {
            editor.central.painter.SetLoadGroundMaterial(paintID);
        }

        public void UpdateSkybox(int skyboxID)
        {
            editor.central.skybox.SetToSkybox(skyboxID, true);
        }
    }
}
