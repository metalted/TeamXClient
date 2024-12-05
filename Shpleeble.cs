using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TeamX
{
    /// <summary>
    /// Represents a Shpleeble character in the game, capable of moving, rotating, and updating its state based on player data and mode.
    /// </summary>
    public class Shpleeble : MonoBehaviour
    {
        //Data
        private PlayerData playerData;
        private CharacterMode currentMode = CharacterMode.None;

        //Components
        private SetupModelCar soapbox;
        private SetupModelCar cameraMan;
        private TextMeshPro displayName;
        private GameObject hornModel;
        private GameObject paragliderModel;
        private GameObject camera;
        private Transform armatureTop;

        //Control
        private bool active;
        private float maxMoveDuration = 0.3f;
        private float maxRotateDuration = 0.2f;
        private Vector3 targetPosition = Vector3.zero;
        private Quaternion targetRotation = Quaternion.identity;
        private Quaternion targetArmatureRotation = Quaternion.identity;
        private Quaternion targetBodyRotation = Quaternion.identity;

        /// <summary>
        /// Activates the Shpleeble, enabling its functionality.
        /// </summary>
        public void Activate()
        {
            active = true;
        }

        /// <summary>
        /// Deactivates the Shpleeble, disabling its functionality.
        /// </summary>
        public void Deactivate()
        {
            active = false;
        }

        /// <summary>
        /// Checks if the Shpleeble is active.
        /// </summary>
        /// <returns>True if active; otherwise, false.</returns>
        public bool IsActive()
        {
            return active;
        }

        /// <summary>
        /// Sets the player data associated with this Shpleeble and updates its appearance and mode.
        /// </summary>
        /// <param name="playerData">The player data to set.</param>
        public void SetPlayerData(PlayerData playerData)
        {
            Debug.LogWarning("Setting player data in Shpleeble.");
            this.playerData = playerData;
            Debug.LogWarning("Setting name in Shpleeble.");
            SetName(playerData.name);
            Debug.LogWarning("Setting cosmetics in Shpleeble.");
            SetCosmetics(playerData.ToCosmeticsV16());
            Debug.LogWarning("Setting mode in Shpleeble.");
            SetMode(playerData.state);
        }

        /// <summary>
        /// Retrieves the player data associated with this Shpleeble.
        /// </summary>
        /// <returns>The player data.</returns>

        public PlayerData GetPlayerData()
        {
            return playerData;
        }

        /// <summary>
        /// Configures the Shpleeble's components, such as models and UI elements.
        /// </summary>
        /// <param name="soapbox">The soapbox model.</param>
        /// <param name="cameraMan">The camera man model.</param>
        /// <param name="displayName">The display name text.</param>
        /// <param name="hornModel">The horn model.</param>
        /// <param name="paragliderModel">The paraglider model.</param>
        /// <param name="camera">The camera object.</param>
        /// <param name="armatureTop">The top of the armature transform.</param>
        public void SetObjects(
            SetupModelCar soapbox,
            SetupModelCar cameraMan,
            TextMeshPro displayName,
            GameObject hornModel,
            GameObject paragliderModel,
            GameObject camera,
            Transform armatureTop)
        {
            this.soapbox = soapbox;
            this.cameraMan = cameraMan;
            this.displayName = displayName;
            this.hornModel = hornModel;
            this.paragliderModel = paragliderModel;
            this.camera = camera;
            this.armatureTop = armatureTop;
        }


        /// <summary>
        /// Sets the display name for the Shpleeble.
        /// </summary>
        /// <param name="name">The name to display.</param>
        public void SetName(string name)
        {
            displayName.text = name;
        }

        /// <summary>
        /// Updates the Shpleeble's cosmetics based on the provided cosmetic data.
        /// </summary>
        /// <param name="cosmetics">The cosmetic data.</param>
        public void SetCosmetics(CosmeticsV16 cosmetics)
        {
            soapbox.DoCarSetup(cosmetics, false, false, true);
            cameraMan.DoCarSetup(cosmetics, false, false, true);
        }

        /// <summary>
        /// Sets the Shpleeble's mode using a mode identifier.
        /// </summary>
        /// <param name="mode">The mode identifier (byte).</param>
        public void SetMode(byte mode)
        {
            if (mode == (byte)currentMode)
                return;

            switch (mode)
            {
                case 0:
                    SetMode(CharacterMode.Build);
                    break;
                case 1:
                    SetMode(CharacterMode.Race);
                    break;
                case 2:
                    SetMode(CharacterMode.Paraglider);
                    break;
            }
        }

        /// <summary>
        /// Sets the Shpleeble's mode using a <see cref="CharacterMode"/> value.
        /// </summary>
        /// <param name="mode">The mode to set.</param>
        public void SetMode(CharacterMode mode)
        {
            switch (mode)
            {
                case CharacterMode.Build:
                case CharacterMode.Paint:
                case CharacterMode.Treegun:
                case CharacterMode.Read:
                    soapbox.gameObject.SetActive(false);
                    cameraMan.gameObject.SetActive(true);
                    currentMode = CharacterMode.Build;
                    break;

                case CharacterMode.Race:
                case CharacterMode.Offroad:
                    cameraMan.gameObject.SetActive(false);
                    soapbox.gameObject.SetActive(true);
                    paragliderModel.gameObject.SetActive(false);
                    currentMode = CharacterMode.Race;
                    break;

                case CharacterMode.Paraglider:
                    cameraMan.gameObject.SetActive(false);
                    soapbox.gameObject.SetActive(true);
                    paragliderModel.gameObject.SetActive(true);
                    foreach (Transform t in paragliderModel.transform)
                    {
                        t.gameObject.SetActive(true);
                    }
                    currentMode = CharacterMode.Paraglider;
                    break;
            }
        }

        /// <summary>
        /// Moves the Shpleeble towards a specified position.
        /// </summary>
        /// <param name="position">The target position.</param>
        /// <param name="instant">If true, moves instantly; otherwise, interpolates over time.</param>
        public void MoveTowards(Vector3 position, bool instant = false)
        {
            targetPosition = position;

            if (instant)
            {
                transform.position = position;
            }
        }

        /// <summary>
        /// Rotates the Shpleeble towards a specified Euler rotation.
        /// </summary>
        /// <param name="euler">The target Euler angles.</param>
        /// <param name="instant">If true, rotates instantly; otherwise, interpolates over time.</param>
        public void RotateTowards(Vector3 euler, bool instant = false)
        {
            targetRotation = Quaternion.Euler(euler);

            if (instant)
            {
                transform.rotation = targetRotation;
            }
        }

        /// <summary>
        /// Rotates the Shpleeble's full body towards a specified angle.
        /// </summary>
        /// <param name="angle">The target angle (degrees).</param>
        /// <param name="instant">If true, rotates instantly; otherwise, interpolates over time.</param>
        public void RotateFullBodyTowards(float angle, bool instant = false)
        {
            targetBodyRotation = Quaternion.Euler(0, angle, 0);

            if (instant)
            {
                transform.rotation = targetBodyRotation;
            }
        }

        /// <summary>
        /// Rotates the Shpleeble's upper body towards a specified angle.
        /// </summary>
        /// <param name="angle">The target angle (degrees).</param>
        /// <param name="instant">If true, rotates instantly; otherwise, interpolates over time.</param>
        public void RotateUpperBodyTowards(float angle, bool instant = false)
        {
            targetArmatureRotation = Quaternion.Euler(0, 270f, 180f - angle);

            if (instant)
            {
                armatureTop.localRotation = targetArmatureRotation;
            }
        }

        /// <summary>
        /// Updates the Shpleeble's state every frame, handling movement, rotation, and UI alignment.
        /// </summary>
        private void Update()
        {
            if (!active)
                return;

            try
            {
                displayName.transform.LookAt(Camera.main.transform.position);
            }
            catch { }

            // Movement
            if (targetPosition != transform.position)
            {
                float distance = Vector3.Distance(transform.position, targetPosition);
                float moveDuration = distance / maxMoveDuration;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveDuration * Time.deltaTime);
            }

            switch (currentMode)
            {
                case CharacterMode.Build:
                case CharacterMode.Paint:
                case CharacterMode.Treegun:
                case CharacterMode.Read:
                    HandleBuildModeRotation();
                    break;

                case CharacterMode.Race:
                case CharacterMode.Paraglider:
                case CharacterMode.Offroad:
                    HandleRaceModeRotation();
                    break;
            }
        }

        /// <summary>
        /// Updates the Shpleeble's transform based on the given position and rotation.
        /// </summary>
        /// <param name="pos">The target position.</param>
        /// <param name="eul">The target Euler rotation.</param>
        public void UpdateTransform(Vector3 pos, Vector3 eul)
        {
            MoveTowards(pos);
            if (currentMode == CharacterMode.Build)
            {
                RotateFullBodyTowards(eul.y);
                RotateUpperBodyTowards(eul.x);
            }
            else
            {
                RotateTowards(eul);
            }
        }

        /// <summary>
        /// Handles rotation for build-related modes.
        /// </summary>
        private void HandleBuildModeRotation()
        {
            if (targetArmatureRotation != armatureTop.localRotation)
            {
                float angle = Quaternion.Angle(armatureTop.localRotation, targetArmatureRotation);
                float rotateDuration = angle / maxRotateDuration;
                armatureTop.localRotation = Quaternion.RotateTowards(armatureTop.localRotation, targetArmatureRotation, rotateDuration * Time.deltaTime);
            }

            if (targetBodyRotation != transform.rotation)
            {
                float angle = Quaternion.Angle(transform.rotation, targetBodyRotation);
                float rotateDuration = angle / maxRotateDuration;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetBodyRotation, rotateDuration * Time.deltaTime);
            }
        }

        /// <summary>
        /// Handles rotation for race-related modes.
        /// </summary>
        private void HandleRaceModeRotation()
        {
            if (targetRotation != transform.rotation)
            {
                float angle = Quaternion.Angle(transform.rotation, targetRotation);
                float rotateDuration = angle / maxRotateDuration;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateDuration * Time.deltaTime);
            }
        }
    }
}
