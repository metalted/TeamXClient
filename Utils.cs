using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TeamXClient
{
    /// <summary>
    /// Represents the current connection status of a client.
    /// </summary>
    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting
    }

    public enum LogType { Debug = 0, Message = 1, Warning = 2, Error = 3 }

    /// <summary>
    /// Defines the modes a player character can be in.
    /// </summary>
    public enum CharacterMode
    {
        Build = 0,
        Race = 1,
        Paraglider = 2,
        Offroad = 3,
        Paint = 4,
        Treegun = 5,
        Read = 6,
        None = 9
    }

    /// <summary>
    /// Holds player data, including their cosmetic setup and state.
    /// </summary>
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

        /// <summary>
        /// Converts player cosmetic data into a <see cref="CosmeticsV16"/> object.
        /// </summary>
        /// <returns>The converted cosmetics data.</returns>
        public CosmeticsV16 ToCosmeticsV16()
        {
            CosmeticsV16 cosmetics = new CosmeticsV16();
            ZeepkistNetworking.CosmeticIDs cosmeticIDs = new ZeepkistNetworking.CosmeticIDs()
            {
                zeepkist = zeepkist,
                frontWheels = frontWheels,
                rearWheels = rearWheels,
                paraglider = paraglider,
                horn = horn,
                hat = hat,
                glasses = glasses,
                color_body = color_body,
                color_leftArm = color_leftArm,
                color_rightArm = color_rightArm,
                color_leftLeg = color_leftLeg,
                color_rightLeg = color_rightLeg,
                color = color
            };
            cosmetics.IDsToCosmetics(cosmeticIDs);
            return cosmetics;
        }
    }

    /// <summary>
    /// Represents a player's state in the game.
    /// </summary>
    public struct PlayerStateData
    {
        public ulong SteamID;
        public Vector3 Position;
        public Vector3 Rotation;
        public byte Mode;
    }

    /// <summary>
    /// Holds the state data of the editor, including floor and skybox settings.
    /// </summary>
    public struct EditorStateData
    {
        public int floor;
        public int skybox;
        public List<string> blocks;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorStateData"/> struct with default values.
        /// </summary>
        /// <param name="b">Default constructor flag.</param>
        public EditorStateData(bool b = true)
        {
            floor = -1;
            skybox = 0;
            blocks = new List<string>();
        }
    }   

    public static class Utils
    {
        public static Shpleeble shpleeblePrefab = null;      
        
        public static bool IsBlockSelected(string UID)
        {
            return Plugin.Instance.editor.Central.selection.list.Any(b => b.UID == UID);
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

         public static PlayerData GetLocalPlayerData()
         {
            PlayerData playerData = new PlayerData();
            playerData.state = 255;

            try
            {
                ZeepkistNetworking.CosmeticIDs cosmeticIDs = ProgressionManager.Instance.GetAdventureCosmetics();

                playerData.name = PlayerManager.Instance.steamAchiever.GetPlayerName(false);
                playerData.steamID = Plugin.Instance.client.ClientSteamID;
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
            if(shpleeblePrefab == null)
            {
                return null;
            }

            Shpleeble s = GameObject.Instantiate(shpleeblePrefab.gameObject).GetComponent<Shpleeble>();
            s.Initialize();
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

            NetworkedGhostSpawner networkedGhostSpawner = GameObject.FindObjectOfType<NetworkedGhostSpawner>(true);
            if(networkedGhostSpawner == null) { return; }

            Plugin.Instance.Log("Creating Shpleeble Prefab", LogType.Debug);

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
