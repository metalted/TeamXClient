using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamX
{
    /*
    public static class EditorState
    {
        private static Dictionary<string, BlockPropertyJSON> blocks = new Dictionary<string, BlockPropertyJSON>();
        private static int skybox;
        private static int floor;

        public static bool Clear()
        {
            blocks.Clear();
            skybox = 0;
            floor = -1;
            return true;
        }

        public static bool AddBlock(string blockJSON)
        {
            BlockPropertyJSON blockPropertyJSON = LEV_UndoRedo.GetJSONblock(blockJSON);
            if(blocks.ContainsKey(blockPropertyJSON.UID))
            {
                return false;
            }
            blocks.Add(blockPropertyJSON.UID, blockPropertyJSON);
            return true;
        }

        public static bool UpdateBlock(string blockUID, string properties)
        {
            if (!blocks.ContainsKey(blockUID))
            {
                return false;
            }

            TeamX.Utils.AssignPropertyListToBlockPropertyJSON(properties, blocks[blockUID]);
            return true;
        }

        public static bool RemoveBlock(string blockUID)
        {
            if (!blocks.ContainsKey(blockUID))
            {
                return false;
            }

            blocks.Remove(blockUID);
            return true;
        }

        public static bool SetSkybox(int skyboxID)
        {
            skybox = skyboxID;
            return true;
        }

        public static bool SetFloor(int floorID)
        {
            floor = floorID;
            return true;
        }

        public static bool SetState(EditorStateData data)
        {
            Clear();

            foreach(string s in data.blocks)
            {
                AddBlock(s);
            }

            SetSkybox(data.skybox);
            SetFloor(data.floor);

            return true;
        }

        public static EditorStateData GetState()
        {
            EditorStateData state = new EditorStateData();
            foreach(BlockPropertyJSON blockPropertyJSON in blocks.Values)
            {
                string blockJSON = LEV_UndoRedo.GetJSONstring(blockPropertyJSON);
                state.blocks.Add(blockJSON);
            }
            state.skybox = GetSkybox();
            state.floor = GetFloor();
            return state;
        }

        public static BlockPropertyJSON GetBlock(string blockUID)
        {
            if (blocks.ContainsKey(blockUID))
            {
                return blocks[blockUID];
            }
            return null;
        }

        public static int GetSkybox() { return skybox; }

        public static int GetFloor() { return floor; }
    }*/
}
