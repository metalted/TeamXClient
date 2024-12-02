using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TeamX
{
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

        public void Activate()
        {
            active = true;
        }

        public void Deactivate()
        {
            active = false;
        }

        public bool IsActive()
        {
            return active;
        }

        public void SetPlayerData(PlayerData playerData)
        {
            this.playerData = playerData;

            SetName(playerData.name);
            SetCosmetics(playerData.ToCosmeticsV16());
            SetMode(playerData.state);
        }

        public PlayerData GetPlayerData()
        {
            return playerData;
        }

        public void SetObjects(SetupModelCar soapbox, SetupModelCar cameraMan, TextMeshPro displayName, GameObject hornModel, GameObject paragliderModel, GameObject camera, Transform armatureTop)
        {
            this.soapbox = soapbox;
            this.cameraMan = cameraMan;
            this.displayName = displayName;
            this.hornModel = hornModel;
            this.paragliderModel = paragliderModel;
            this.camera = camera;
            this.armatureTop = armatureTop;
        }

        public void SetName(string name)
        {
            displayName.text = name;
        }

        public void SetCosmetics(CosmeticsV16 cosmetics)
        {
            soapbox.DoCarSetup(cosmetics, false, false, true);
            cameraMan.DoCarSetup(cosmetics, false, false, true);
        }

        public void SetMode(byte mode)
        {
            if(mode == (byte) currentMode)
            {
                return;
            }

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
                    foreach (Transform t in paragliderModel.gameObject.transform)
                    {
                        t.gameObject.SetActive(true);
                    }
                    currentMode = CharacterMode.Paraglider;
                    break;
            }
        }

        public void MoveTowards(Vector3 position, bool instant = false)
        {
            targetPosition = position;

            if(instant)
            {
                transform.position = position;
            }
        }

        public void RotateTowards(Vector3 euler, bool instant = false)
        {
            Quaternion rotation = Quaternion.Euler(euler);
            targetRotation = rotation;

            if(instant)
            {
                transform.rotation = rotation;
            }
        }

        public void RotateFullBodyTowards(float angle, bool instant = false) 
        {
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            targetBodyRotation = rotation;

            if(instant)
            {
                transform.rotation = rotation;
            }
        }

        public void RotateUpperBodyTowards(float angle, bool instant = false) 
        {
            Quaternion rotation = Quaternion.Euler(0, 270f, 180f - angle);
            targetArmatureRotation = rotation;

            if(instant)
            {
                armatureTop.localRotation = rotation;
            }
        }

        private void Update()
        {
            if (!active)
            {
                return;
            }

            //Make display name visible to camera
            try
            {
                displayName.transform.LookAt(Camera.main.transform.position);
            }
            catch { }

            //Move towards Position
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
                    //Armature Rotation
                    if (targetArmatureRotation != armatureTop.localRotation)
                    {
                        float angle = Quaternion.Angle(armatureTop.localRotation, targetArmatureRotation);
                        float rotateDuration = angle / maxRotateDuration;
                        armatureTop.localRotation = Quaternion.RotateTowards(armatureTop.localRotation, targetArmatureRotation, rotateDuration * Time.deltaTime);
                    }

                    //Body Rotation
                    if (targetBodyRotation != transform.rotation)
                    {
                        float angle = Quaternion.Angle(transform.rotation, targetBodyRotation);
                        float rotateDuration = angle / maxRotateDuration;
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetBodyRotation, rotateDuration * Time.deltaTime);
                    }
                    break;
                case CharacterMode.Race:
                case CharacterMode.Paraglider:
                case CharacterMode.Offroad:
                    //Soapbox
                    if (targetRotation != transform.rotation)
                    {
                        float angle = Quaternion.Angle(transform.rotation, targetRotation);
                        float rotateDuration = angle / maxRotateDuration;
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateDuration * Time.deltaTime);
                    }
                    break;
            }
        }

        public void UpdateTransform(Vector3 pos, Vector3 eul)
        {
            switch (currentMode)
            {
                case CharacterMode.Build:
                case CharacterMode.Paint:
                case CharacterMode.Treegun:
                case CharacterMode.Read:
                    MoveTowards(pos);
                    RotateFullBodyTowards(eul.y);
                    RotateUpperBodyTowards(eul.x);
                    break;
                case CharacterMode.Race:
                case CharacterMode.Paraglider:
                case CharacterMode.Offroad:
                    MoveTowards(pos);
                    RotateTowards(eul);
                    break;
            }
        }
    }
}
