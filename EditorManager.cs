using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamX
{
    public class EditorManager
    {
        private int Floor;
        private int Skybox;
        private Dictionary<string, Block> Blocks;

        public EditorModifier Modifier;
        public EditorObserver Observer;

        public LEV_LevelEditorCentral central;

        public SelectionObserver selectionObserver;

        public bool InLevelEditor()
        {
            return central != null;
        }

        public EditorManager()
        {
            Floor = -1;
            Skybox = 0;
            Blocks = new Dictionary<string, Block>();
            Modifier = new EditorModifier(this);
            Observer = new EditorObserver(this);
        }

        public void OnBlocksRemovedFromSelection(List<string> removed)
        {
            foreach(string uid in removed)
            {
                //We gotta check if we own this block or if we have sufficient permissions for the request.
                Block block = Get(uid);

                //Are we the creator or have enough permission?
                if (block.SteamID == Plugin.Instance.client.ClientSteamID || (byte)Plugin.Instance.client.PermissionLevel > 1)
                {
                    Plugin.Instance.client.SendDeselection(uid);
                }
            }
        }

        public void OnBlocksAddedToSelection(List<string> added)
        {
            foreach (string uid in added)
            {
                Debug.Log(uid);

                //We gotta check if we own this block or if we have sufficient permissions for the request.
                Block block = Get(uid);

                //Are we the creator or have enough permission?
                if (block.SteamID == Plugin.Instance.client.ClientSteamID || (byte)Plugin.Instance.client.PermissionLevel > 1)
                {
                    Plugin.Instance.client.SendSelection(uid);
                }
                //We cant edit this block, revert to before.
                else
                {
                    Modifier.DeselectBlock(uid);
                }
            }
        }

        public IEnumerator InstantiateFromState()
        {
            yield return new WaitForEndOfFrame();
            Modifier.UpdateSkybox(Skybox);
            Modifier.UpdateFloor(Floor);

            //60 fps, 1 second
            int blockCount = Blocks.Count;
            int batchCount = Mathf.Max(10, Mathf.FloorToInt(blockCount / 60f));
            int counter = 0;

            foreach (KeyValuePair<string, Block> block in Blocks)
            {
                Modifier.CreateBlock(block.Value);
                counter++;
                if(counter % batchCount == 0)
                {
                    yield return new WaitForEndOfFrame();
                }
            }

            central.validation.RecalcBlocksAndDraw(false);
        }

        public void SetState(EditorStateData stateData)
        {
            Blocks.Clear();

            Floor = stateData.floor;
            Skybox = stateData.skybox;
            foreach(string s in stateData.blocks)
            {
                Block block = JSONToBlock(s);
                Blocks.Add(block.UID, block);
            }
        }

        public void Add(string blockString)
        {
            Block block = JSONToBlock(blockString);
            if(!Blocks.ContainsKey(block.UID))
            {
                Blocks.Add(block.UID, block);
            }
        }

        public void Add(Block block)
        {
            if (!Blocks.ContainsKey(block.UID))
            {
                Blocks.Add(block.UID, block);
            }
        }

        public void Remove(string uid)
        {
            if(Blocks.ContainsKey(uid))
            {
                Blocks.Remove(uid);
            }
        }

        public Block Get(string uid)
        {
            if(Blocks.ContainsKey(uid))
            {
                return Blocks[uid];
            }

            return null;
        }

        public void Update(string uid, string properties)
        {
            if(Blocks.ContainsKey(uid))
            {
                SetBlockProperties(Blocks[uid], properties);
            }
        }

        public void Update(Block block)
        {
            if (Blocks.ContainsKey(block.UID))
            {
                Blocks[block.UID] = block;
            }           
        }

        public void SetFloor(int floor)
        {
            Floor = floor;
        }

        public void SetSkybox(int skybox)
        {
            Skybox = skybox;
        }

        public Block JSONToBlock(string json)
        {
            Block block = JsonConvert.DeserializeObject<Block>(json);
            return block;
        }

        public string BlockToJSON(Block block)
        {
            return JsonConvert.SerializeObject(block);
        }

        private void SetBlockProperties(Block block, string properties)
        {
            List<float> props = PropertyStringToList(properties);
            block.PositionX = props[0];
            block.PositionY = props[1];
            block.PositionZ = props[2];
            block.EulerAnglesX = props[3];
            block.EulerAnglesY = props[4];
            block.EulerAnglesZ = props[5];
            block.LocalScaleX = props[6];
            block.LocalScaleY = props[7];
            block.LocalScaleZ = props[8];
            block.Properties = props;
        }

        private List<float> PropertyStringToList(string properties)
        {
            return properties.Split('|').Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToList();
        }
    }
}
