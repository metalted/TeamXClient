using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using ZeepkistNetworking;

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
        public string chatColor;
        public int color;
        public int color_body;
        public int color_leftArm;
        public int color_leftLeg;
        public int color_rightArm;
        public int color_rightLeg;
        public int frontWheels;
        public int glasses;
        public int hat;
        public int horn;
        public string name;
        public int paraglider;
        public int rearWheels;
        public byte state;
        public ulong steamID;
        public int zeepkist;

        /// <summary>
        /// Converts player cosmetic data into a <see cref="CosmeticsV16"/> object.
        /// </summary>
        /// <returns>The converted cosmetics data.</returns>
        public CosmeticsV16 ToCosmeticsV16()
        {
            CosmeticsV16 cosmetics = new CosmeticsV16();
            cosmetics.zeepkist = (Object_Soapbox)PlayerManager.Instance.objectsList.wardrobe.GetCosmetic(CosmeticItemType.zeepkist, zeepkist, false);
            cosmetics.frontwheels = (Object_Wheel)PlayerManager.Instance.objectsList.wardrobe.GetCosmetic(CosmeticItemType.wheel, frontWheels, false);
            cosmetics.rearwheels = (Object_Wheel)PlayerManager.Instance.objectsList.wardrobe.GetCosmetic(CosmeticItemType.wheel, rearWheels, false);
            cosmetics.paraglider = (Object_Paraglider)PlayerManager.Instance.objectsList.wardrobe.GetCosmetic(CosmeticItemType.paraglider, paraglider, false);
            cosmetics.hat = (HatValues)PlayerManager.Instance.objectsList.wardrobe.GetCosmetic(CosmeticItemType.hat, hat, false);
            cosmetics.glasses = (HatValues)PlayerManager.Instance.objectsList.wardrobe.GetCosmetic(CosmeticItemType.glasses, glasses, false);
            cosmetics.color_body = (CosmeticColor)PlayerManager.Instance.objectsList.wardrobe.GetCosmetic(CosmeticItemType.skin, color_body, false);
            cosmetics.color_leftArm = (CosmeticColor)PlayerManager.Instance.objectsList.wardrobe.GetCosmetic(CosmeticItemType.skin, color_leftArm, false);
            cosmetics.color_rightArm = (CosmeticColor)PlayerManager.Instance.objectsList.wardrobe.GetCosmetic(CosmeticItemType.skin, color_rightArm, false);
            cosmetics.color_leftLeg = (CosmeticColor)PlayerManager.Instance.objectsList.wardrobe.GetCosmetic(CosmeticItemType.skin, color_leftLeg, false);
            cosmetics.color_rightLeg = (CosmeticColor)PlayerManager.Instance.objectsList.wardrobe.GetCosmetic(CosmeticItemType.skin, color_rightLeg, false);
            cosmetics.horn = (Object_Horn)PlayerManager.Instance.objectsList.wardrobe.GetCosmetic(CosmeticItemType.horn, horn, false);
            return cosmetics;
        }

        public string ToDebugString()
        {
            return $"" +
                $"Name: {name}\n" +
                $"SteamID: {steamID}\n" +
                $"ChatColor: {chatColor}\n" +
                $"Color: {color}\n" +
                $"Color Body: {color_body}\n" +
                $"Color Left Arm: {color_leftArm}\n" +
                $"Color Left Leg: {color_leftLeg}\n" +
                $"Color Right Arm: {color_rightArm}\n" +
                $"Color Right Leg: {color_rightLeg}\n" +
                $"Front Wheels: {frontWheels}\n" +
                $"Glasses: {glasses}\n" +
                $"Hat: {hat}\n" +
                $"Horn: {horn}\n" +
                $"Paraglider: {paraglider}\n" +
                $"Rear Wheels: {rearWheels}\n" +
                $"State: {state}\n" +
                $"Zeepkist: {zeepkist}";
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
        public string skybox;
        public List<string> blocks;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorStateData"/> struct with default values.
        /// </summary>
        /// <param name="b">Default constructor flag.</param>
        public EditorStateData(bool b = true)
        {
            floor = -1;
            skybox = "{\"enviro\":{\"skybox\":0,\"groundMat\":90,\"overrideFog_b\":false,\"overrideFog_f\":0,\"skyboxOverride\":null}}";
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

        public static string GetOnlinePlayerHexColor()
        {
            float h = PlayerManager.Instance.instellingen.GlobalSettings.online_name_color_H;
            float s = PlayerManager.Instance.instellingen.GlobalSettings.online_name_color_S;
            float v = PlayerManager.Instance.instellingen.GlobalSettings.online_name_color_V;

            //Convert the hsv to hex 
            return HsvToHex(h, s, v);
        }

        public static string HsvToHex(float h, float s, float v)
        {
            // Ensure HSV values are within expected ranges
            h = Math.Clamp(h, 0f, 360f);
            s = Math.Clamp(s, 0f, 1f);
            v = Math.Clamp(v, 0f, 1f);

            // Convert HSV to RGB
            int hi = (int)(h / 60) % 6;
            float f = (h / 60) - hi;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            float r = 0, g = 0, b = 0;

            switch (hi)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }

            // Convert RGB floats to integers (0-255)
            int rInt = (int)(r * 255);
            int gInt = (int)(g * 255);
            int bInt = (int)(b * 255);

            // Convert to hexadecimal string
            return $"#{rInt:X2}{gInt:X2}{bInt:X2}";
        }

        public static PlayerData GetLocalPlayerData()
         {
            ZeepkistNetworking.CosmeticIDs cosmeticIDs = ProgressionManager.Instance.GetAdventureCosmetics();

            PlayerData playerData = new PlayerData()
            {
                chatColor = GetOnlinePlayerHexColor(),
                color = cosmeticIDs.color,
                color_body = cosmeticIDs.color_body,
                color_leftArm = cosmeticIDs.color_leftArm,
                color_leftLeg = cosmeticIDs.color_leftLeg,
                color_rightArm = cosmeticIDs.color_rightArm,
                color_rightLeg = cosmeticIDs.color_rightLeg,
                frontWheels = cosmeticIDs.frontWheels,
                glasses = cosmeticIDs.glasses,
                hat = cosmeticIDs.hat,
                horn = cosmeticIDs.horn,
                name = PlayerManager.Instance.steamAchiever.GetPlayerName(false),
                paraglider = cosmeticIDs.paraglider,
                rearWheels = cosmeticIDs.rearWheels,
                state = 0,
                steamID = Plugin.Instance.client.ClientSteamID,
                zeepkist = cosmeticIDs.zeepkist
            };

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

        public static Transform CreatePanel(LEV_LevelEditorCentral central, string title)
        {
            if (central == null)
            {
                return null;
            }

            Transform panelCopy = GameObject.Instantiate<Transform>(central.saveload.transform, central.saveload.transform.parent);
            panelCopy.gameObject.name = title;
            GameObject.Destroy(panelCopy.GetComponent<LEV_SaveLoad>());
            return panelCopy;
        }

        public static Dictionary<string, List<string>> GroupTeamKistFilesByProject(IEnumerable<string> paths)
        {
            var projectFiles = new Dictionary<string, List<string>>();

            foreach (var path in paths)
            {
                Plugin.Instance.Log(path, LogType.Debug);

                // Split the path to extract the project name and file name
                string[] parts = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3) // Ensure path has enough components
                {
                    string projectName = parts[0];
                    string fileName = parts[parts.Length - 1]; // Get the last part (file name)

                    // Add to the dictionary
                    if (!projectFiles.ContainsKey(projectName))
                    {
                        projectFiles[projectName] = new List<string>();
                    }
                    projectFiles[projectName].Add(fileName);
                }
            }

            // Sort the List<string> in descending order for each project
            foreach (var key in projectFiles.Keys.ToList()) // ToList prevents modification during iteration
            {
                projectFiles[key].Sort((x, y) => string.Compare(y, x, StringComparison.Ordinal));
            }

            // Sort the dictionary keys alphabetically and return a new sorted dictionary
            return projectFiles
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .ToDictionary(entry => entry.Key, entry => entry.Value);
        }
    }
}
