using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TeamX
{
    public enum ConnectionStatus
    {
        NotConnected, Connecting, Connected, Disconnecting
    }
    public enum CharacterMode 
    { 
        Build = 0, Race = 1, Paraglider = 2, Offroad = 3, Paint = 4, Treegun = 5, Read = 6, None = 9
    };

    public enum NetworkMessageType
    {
        LogIn = 10,
        JoinedPlayerData = 11,
        ServerPlayerData = 12,
        PlayerTransformData = 13,
        PlayerStateData = 14,
        PlayerLeft = 15,
        ServerData = 20,
        LevelEditorChangeEvents = 100,
        BlockCreateEvent = 101,
        BlockDestroyEvent = 102,
        BlockChangeEvent = 103,
        EditorFloorEvent = 104,
        EditorSkyboxEvent = 105,
        CustomMessage = 200
    }

    public struct LevelEditorChange
    {
        public enum ChangeType { BlockCreate, BlockUpdate, BlockDestroy, Floor, Skybox };
        public ChangeType changeType;
        public string UID;
        public string string_data;
        public int int_data;
    }

    public struct PlayerData
    {
        public ulong steamID;
        public string name;
        public byte state;
        public int zeepkist;
        public int frontWheels;
        public int rearWheels;
        public int paraglider;
        public int horn;
        public int hat;
        public int glasses;
        public int color_body;
        public int color_leftArm;
        public int color_rightArm;
        public int color_leftLeg;
        public int color_rightLeg;
        public int color;

        public CosmeticsV16 ToCosmeticsV16()
        {
            CosmeticsV16 cosmetics = new CosmeticsV16();
            ZeepkistNetworking.CosmeticIDs cosmeticIDs = new ZeepkistNetworking.CosmeticIDs();
            cosmeticIDs.zeepkist = zeepkist;
            cosmeticIDs.frontWheels = frontWheels;
            cosmeticIDs.rearWheels = rearWheels;
            cosmeticIDs.paraglider = paraglider;
            cosmeticIDs.horn = horn;
            cosmeticIDs.hat = hat;
            cosmeticIDs.glasses = glasses;
            cosmeticIDs.color_body = color_body;
            cosmeticIDs.color_leftArm = color_leftArm;
            cosmeticIDs.color_rightArm = color_rightArm;
            cosmeticIDs.color_leftLeg = color_leftLeg;
            cosmeticIDs.color_rightLeg = color_rightLeg;
            cosmeticIDs.color = color;
            cosmetics.IDsToCosmetics(cosmeticIDs);
            return cosmetics;
        }
    }
    public struct PlayerStateData
    {
        public ulong SteamID;
        public Vector3 Position;
        public Vector3 Rotation;
        public byte Mode;
    }

    public struct EditorStateData
    {
        public int floor;
        public int skybox;
        public List<string> blocks;

        public EditorStateData(bool b = true)
        {
            floor = -1;
            skybox = 0;
            blocks = new List<string>();
        }
    }

    public static class Utils
    {
        public static Shpleeble shpleeblePrefab;

        public static BlockPropertyJSON BlockToBlockPropertyJSON(Block block)
        {
            BlockPropertyJSON json = new BlockPropertyJSON()
            {
                blockID = block.ID,
                eulerAngles = new Vector3(block.EulerAnglesX, block.EulerAnglesY, block.EulerAnglesZ),
                localScale = new Vector3(block.LocalScaleX, block.LocalScaleY, block.LocalScaleZ),
                position = new Vector3(block.PositionX, block.PositionY, block.PositionZ),
                properties = block.Properties,
                UID = block.UID
            };

            return json;
        }

        public static List<float> PropertyStringToList(string properties)
        {
            return properties.Split('|').Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToList();
        }

        public static string PropertyListToString(List<float> properties)
        {
            return string.Join("|", properties.Select(p => p.ToString(CultureInfo.InvariantCulture)));
        }

        public static void AssignPropertyListToBlockPropertyJSON(string properties, BlockPropertyJSON blockPropertyJSON)
        {
            List<float> propertyList = PropertyStringToList(properties);

            blockPropertyJSON.position.x = propertyList[0];
            blockPropertyJSON.position.y = propertyList[1];
            blockPropertyJSON.position.z = propertyList[2];
            blockPropertyJSON.eulerAngles.x = propertyList[3];
            blockPropertyJSON.eulerAngles.y = propertyList[4];
            blockPropertyJSON.eulerAngles.z = propertyList[5];
            blockPropertyJSON.localScale.x = propertyList[6];
            blockPropertyJSON.localScale.y = propertyList[7];
            blockPropertyJSON.localScale.z = propertyList[8];
            blockPropertyJSON.properties = propertyList;
        }

        public static string FixMissingJSONProperties(string blockJSON)
        {
            BlockPropertyJSON block = LEV_UndoRedo.GetJSONblock(blockJSON);
            block.properties[0] = block.position.x;
            block.properties[1] = block.position.y;
            block.properties[2] = block.position.z;
            block.properties[3] = block.eulerAngles.x;
            block.properties[4] = block.eulerAngles.y;
            block.properties[5] = block.eulerAngles.z;
            block.properties[6] = block.localScale.x;
            block.properties[7] = block.localScale.y;
            block.properties[8] = block.localScale.z;
            return LEV_UndoRedo.GetJSONstring(block);
        }

        public static BlockPropertyJSON GetFixedJSONBlock(string blockJSON)
        {
            BlockPropertyJSON block = LEV_UndoRedo.GetJSONblock(blockJSON);
            block.properties[0] = block.position.x;
            block.properties[1] = block.position.y;
            block.properties[2] = block.position.z;
            block.properties[3] = block.eulerAngles.x;
            block.properties[4] = block.eulerAngles.y;
            block.properties[5] = block.eulerAngles.z;
            block.properties[6] = block.localScale.x;
            block.properties[7] = block.localScale.y;
            block.properties[8] = block.localScale.z;
            return block;
        }

        public static string FixedPropertyListToString(string blockJSON)
        {
            BlockPropertyJSON block = LEV_UndoRedo.GetJSONblock(blockJSON);
            block.properties[0] = block.position.x;
            block.properties[1] = block.position.y;
            block.properties[2] = block.position.z;
            block.properties[3] = block.eulerAngles.x;
            block.properties[4] = block.eulerAngles.y;
            block.properties[5] = block.eulerAngles.z;
            block.properties[6] = block.localScale.x;
            block.properties[7] = block.localScale.y;
            block.properties[8] = block.localScale.z;
            return PropertyListToString(block.properties);
        }

        public static void CopyFromTo(BlockPropertyJSON from, BlockPropertyJSON to)
        {
            to.blockID = from.blockID;
            to.position = from.position;
            to.eulerAngles = from.eulerAngles;
            to.localScale = from.localScale;
            to.properties = from.properties.ToList();
            to.UID = from.UID;
        }

         public static PlayerData GetLocalPlayerData()
        {
            PlayerData playerData = new PlayerData();
            playerData.state = 255;

            try
            {
                ZeepkistNetworking.CosmeticIDs cosmeticIDs = ProgressionManager.Instance.GetAdventureCosmetics();

                playerData.name = PlayerManager.Instance.steamAchiever.GetPlayerName(false);
                playerData.steamID = PlayerManager.Instance.steamAchiever.GetPlayerSteamID();
                playerData.zeepkist = cosmeticIDs.zeepkist;
                playerData.frontWheels = cosmeticIDs.frontWheels;
                playerData.rearWheels = cosmeticIDs.rearWheels;
                playerData.paraglider = cosmeticIDs.paraglider;
                playerData.horn = cosmeticIDs.horn;
                playerData.hat = cosmeticIDs.hat;
                playerData.glasses = cosmeticIDs.glasses;
                playerData.color_body = cosmeticIDs.color_body;
                playerData.color_leftArm = cosmeticIDs.color_leftArm;
                playerData.color_rightArm = cosmeticIDs.color_rightArm;
                playerData.color_leftLeg = cosmeticIDs.color_leftLeg;
                playerData.color_rightLeg = cosmeticIDs.color_rightLeg;
                playerData.color = cosmeticIDs.color;
            }
            catch (Exception e)
            {
                playerData.name = "Sphleeble";
                playerData.hat = 23000;
                playerData.color = 1000;
                playerData.zeepkist = 1000;

                playerData.zeepkist = 1000;
                playerData.frontWheels = 1000;
                playerData.rearWheels = 1000;
                playerData.paraglider = 1000;
                playerData.horn = 1000;
                playerData.hat = 23000;
                playerData.glasses = 1000;
                playerData.color_body = 1000;
                playerData.color_leftArm = 1000;
                playerData.color_rightArm = 1000;
                playerData.color_leftLeg = 1000;
                playerData.color_rightLeg = 1000;
                playerData.color = 1000;
            }

            return playerData;
        }

        public static Shpleeble CreateShpleeble(PlayerData playerData)
        {
            if(shpleeblePrefab != null)
            {
                return null;
            }

            Shpleeble s = GameObject.Instantiate<Shpleeble>(shpleeblePrefab);
            GameObject.DontDestroyOnLoad(s.gameObject);
            s.SetPlayerData(playerData);
            s.Activate();
            return s;
        }

        public static void CreateShpleeblePrefabInMainMenu()
        {
            if(shpleeblePrefab != null)
            {
                return;
            }

            NetworkedGhostSpawner networkedGhostSpawner = GameObject.FindObjectOfType<NetworkedGhostSpawner>();
            if(networkedGhostSpawner == null) { return; }

            Shpleeble shpleeble = new GameObject("Shpleeble").AddComponent<Shpleeble>();
            GameObject.DontDestroyOnLoad(shpleeble);

            //SOAPBOX
            SetupModelCar soapbox = GameObject.Instantiate(networkedGhostSpawner.zeepkistGhostPrefab.ghostModel.transform, shpleeble.transform).GetComponent<SetupModelCar>();
            //Remove ghost wheel scripts
            Ghost_AnimateWheel[] animateWheelScripts = soapbox.transform.GetComponentsInChildren<Ghost_AnimateWheel>();
            foreach (Ghost_AnimateWheel gaw in animateWheelScripts)
            {
                GameObject.Destroy(gaw);
            }
            //Attach the left and right arm to the top of the armature
            Transform armatureTopSX = soapbox.transform.Find("Character/Armature/Top");
            Transform leftArmSX = soapbox.transform.Find("Character/Left Arm");
            Transform rightArmSX = soapbox.transform.Find("Character/Right Arm");
            leftArmSX.parent = armatureTopSX;
            leftArmSX.localPosition = new Vector3(-0.25f, 0, 1.25f);
            leftArmSX.localEulerAngles = new Vector3(0, 240, 0);
            rightArmSX.parent = armatureTopSX;
            rightArmSX.localPosition = new Vector3(-0.25f, 0, -1.25f);
            rightArmSX.localEulerAngles = new Vector3(0, 120, 0);

            //CAMERA MAN
            SetupModelCar cameraMan = GameObject.Instantiate(networkedGhostSpawner.zeepkistGhostPrefab.cameraManModel.transform, shpleeble.transform).GetComponent<SetupModelCar>();
            GameObject camera = cameraMan.transform.Find("Character/Right Arm/Camera").gameObject;
            camera.SetActive(false);

            //Attach the left and right arm to the top of the armature
            Transform armatureTop = cameraMan.transform.Find("Character/Armature/Top");
            Transform leftArm = cameraMan.transform.Find("Character/Left Arm");
            Transform rightArm = cameraMan.transform.Find("Character/Right Arm");
            leftArm.parent = armatureTop;
            leftArm.localPosition = new Vector3(-0.25f, 0, 1.25f);
            leftArm.localEulerAngles = new Vector3(0, 240, 0);
            rightArm.parent = armatureTop;
            rightArm.localPosition = new Vector3(-0.25f, 0, -1.25f);
            rightArm.localEulerAngles = new Vector3(0, 120, 0);

            //DISPLAY NAME
            TextMeshPro displayName = GameObject.Instantiate(networkedGhostSpawner.zeepkistGhostPrefab.nameDisplay.transform, shpleeble.transform).GetComponent<TextMeshPro>();
            GameObject.Destroy(displayName.transform.GetComponent<DisplayPlayerName>());
            GameObject.Destroy(displayName.transform.Find("hoethouder").gameObject);
            displayName.transform.localScale = new Vector3(-1, 1, 1);
            

            //OTHER
            GameObject hornModel = soapbox.transform.Find("Visible Horn").gameObject;
            hornModel.SetActive(false);

            GameObject paragliderModel = soapbox.transform.Find("Glider").gameObject;
            foreach (Transform t in paragliderModel.transform)
            {
                t.gameObject.SetActive(true);
            }
            paragliderModel.SetActive(false);

            shpleeble.SetObjects(soapbox, cameraMan, displayName, hornModel, paragliderModel, camera, armatureTop);
            shpleeble.gameObject.SetActive(false);

            shpleeblePrefab = shpleeble;
        }        
    }
}
