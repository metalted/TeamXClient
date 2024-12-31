using I2.Loc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TeamXClient
{
   
    public static class InterfaceManager
    {
        public static TeamXPanelComponent teamXToolbarButton;

        public static TeamXMainPanel mainPanel;
        public static TeamXPermissionPanel permissionPanel;
        public static TeamXSaveConfigurationPanel saveConfigurationPanel;
        public static TeamXLevelManagerPanel levelManagerPanel;

        public static LEV_LevelEditorCentral _central;

        public static Color lightestGreen = new Color(0.336f, 1f, 0.766f); //86.196.255
        public static Color lightGreen = new Color(0f, 1f, 0.656f, 1f); //0.168.255
        public static Color green = new Color(0, 0.82f, 0.547f,  1f); //0.140.210
        public static Color darkGreen = new Color(0, 0.547f, 0.371f,  1f);
        public static Color darkestGreen = new Color(0, 0.348f, 0.238f, 1f);
        public static Color grey = new Color(0.3f, 0.3f, 0.3f, 1f);
        public static Color darkgrey = new Color(0.2f, 0.2f, 0.2f, 1f);

        public static void SetupMainMenuUI()
        {
            GenerateLevelEditorOnlineButton();
        }

        public static void SetupLevelEditorUI(LEV_LevelEditorCentral central, bool management = false)
        {
            _central = central;
            DisableLoadButton();

            if (management)
            {
                InitializePanels(central);
                CreateToolbarButtons(central);
            }
        }

        public static void OnPanelOpen()
        {
            _central.tool.DisableAllTools();
            _central.tool.RecolorButtons();
            _central.tool.currentTool = 3;
            _central.tool.inspectorTitle.text = "";
        }

        public static void OnPanelClose()
        {
            _central.tool.EnableEditTool();
            _central.tool.RecolorButtons();
            _central.cam.OverrideOutsideGameView(false);
        }

        private static void CreateToolbarButtons(LEV_LevelEditorCentral central)
        {
            RectTransform teamXToolbarRect = GameObject.Instantiate(central.tool.button_settings, central.tool.button_settings.transform.parent).GetComponent<RectTransform>();
            teamXToolbarButton = new TeamXPanelComponent(TeamXPanelComponentType.Button, teamXToolbarRect);
            teamXToolbarButton.SetRectAnchors(0.005f, 0.1f, 0.025f, 0.9f);
            teamXToolbarButton.BindButton(() =>
            {
                central.selection.DeselectAllBlocks(false, "");

                if (mainPanel != null)
                {
                    mainPanel.Open();
                }
            });           
        }

        //This function will split the regular level editor button in to two buttons, one for regular and one for teamkist.
        private static void GenerateLevelEditorOnlineButton()
        {
            //Get the two current buttons.
            OpenUIOnStart mainmenu_canvas = GameObject.FindObjectOfType<OpenUIOnStart>();
            Transform levelEditorGUI = mainmenu_canvas.transform.Find("LevelEditorGUI").transform;
            RectTransform workshopButton = levelEditorGUI.Find("Workshop Button").GetComponent<RectTransform>();
            RectTransform levelEditorButton = levelEditorGUI.Find("Start Level Editor Button").GetComponent<RectTransform>();

            //Calculate the current spacing between the two buttons.
            float buttonSpacing = Mathf.Abs(workshopButton.anchorMax.x - levelEditorButton.anchorMin.x);
            float buttonHeight = Mathf.Abs(levelEditorButton.anchorMax.y - levelEditorButton.anchorMin.y);

            //Create a copy of the level editor button.
            RectTransform editorOnlineButton = GameObject.Instantiate(levelEditorButton.transform, levelEditorButton.transform.parent).GetComponent<RectTransform>();

            //Make the level editor button half size with half spacing.
            levelEditorButton.anchorMin = new Vector2(levelEditorButton.anchorMin.x, levelEditorButton.anchorMin.y + (buttonHeight / 2) + (buttonSpacing / 2));

            //Set the new button on the other half
            editorOnlineButton.anchorMax = new Vector2(editorOnlineButton.anchorMax.x, editorOnlineButton.anchorMax.y - (buttonHeight / 2) - (buttonSpacing / 2));

            //Remove the listener of the new button.
            GenericButton editorOnlineGenericButton = editorOnlineButton.GetComponent<GenericButton>();
            editorOnlineGenericButton.normalColor = new Color(0, 0.547f, 0.82f, 1f);
            editorOnlineGenericButton.buttonImage.color = editorOnlineGenericButton.normalColor;
            editorOnlineGenericButton.onClick.RemoveAllListeners();
            for (int i = editorOnlineGenericButton.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                editorOnlineGenericButton.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
            }

            //Add a new listener
            editorOnlineGenericButton.onClick.AddListener(OnEditorOnlineButton);

            //Get the text component, remove the localizer and change the text
            TextMeshProUGUI buttonText = editorOnlineGenericButton.GetComponentInChildren<TextMeshProUGUI>();
            GameObject.Destroy(buttonText.GetComponent<I2.Loc.Localize>());
            buttonText.text = "Teamkist Editor";
        }

        //When the custom button is clicked.
        private static void OnEditorOnlineButton()
        {
            try
            {
                Plugin.Instance.client.AttemptToConnectToServer();
                PlayerManager.Instance.weLoadedLevelEditorFromMainMenu = true;
            }
            catch (Exception e)
            {
                Plugin.Instance.Log(e.Message, LogType.Error);

                if (Plugin.Instance.client != null)
                {
                    Plugin.Instance.client.AttemptDisconnect();
                }
            }
        }

        //Grey out the load button in the level editor while in teamkist mode, because loading a level will mess up the server.
        private static void DisableLoadButton()
        {
            LEV_CustomButton loadButton = Plugin.Instance.editor.Central.tool.button_load;
            loadButton.normalColor = Color.grey;
            loadButton.hoverColor = Color.grey;
            loadButton.clickColor = Color.grey;
            loadButton.overrideAllColor = true;
            loadButton.overrideNormalColor = true;
            loadButton.onClick.RemoveAllListeners();

            for (int i = loadButton.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                loadButton.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
            }
        }

        private static void InitializePanels(LEV_LevelEditorCentral central)
        {
            mainPanel = Utils.CreatePanel(central, "TeamX").gameObject.AddComponent<TeamXMainPanel>();
            mainPanel.Initialize();

            permissionPanel = Utils.CreatePanel(central, "TeamX Permissions").gameObject.AddComponent<TeamXPermissionPanel>();
            permissionPanel.Initialize();

            saveConfigurationPanel = Utils.CreatePanel(central, "TeamX Save Configuration").gameObject.AddComponent<TeamXSaveConfigurationPanel>();
            saveConfigurationPanel.Initialize();

            levelManagerPanel = Utils.CreatePanel(central, "TeamX Level Manager").gameObject.AddComponent<TeamXLevelManagerPanel>();
            levelManagerPanel.Initialize();
        }

        public static void UnbindButton(LEV_CustomButton button)
        {
            button.onClick.RemoveAllListeners();

            for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                button.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
            }

            //Disable the hotkey script.
            LEV_HotkeyButton hotkeybutton = button.GetComponent<LEV_HotkeyButton>();
            if (hotkeybutton != null)
            {
                hotkeybutton.enabled = false;
            }
        }

        public static void RebindButton(LEV_CustomButton button, UnityAction action)
        {
            button.onClick.AddListener(action);
        }

        public static void RecolorButton(LEV_CustomButton button, Color normalColor, Color hoverColor, Color clickColor, bool recolorAllNormal = false)
        {
            button.normalColor = normalColor;
            button.overrideNormalColor = true;
            button.buttonImage.color = normalColor;
            button.hoverColor = hoverColor;
            button.clickColor = clickColor;
            button.isDisabled_clickColor = clickColor;
            button.isDisabled_hoverColor = hoverColor;
            button.isDisabled_normalColor = normalColor;

            if (recolorAllNormal)
            {
                button.clickColor = normalColor;
                button.hoverColor = normalColor;
                button.normalColor = normalColor;
                button.selectedColor = normalColor;
                button.isDisabled_clickColor = normalColor;
                button.isDisabled_hoverColor = normalColor;
                button.isDisabled_normalColor = normalColor;
                button.isDisabled_selectedColor = normalColor;
            }
        }

        public static void StandardRecolorButton(LEV_CustomButton button)
        {
            RecolorButton(button, green, lightGreen, lightestGreen, false);
        }
    }    
}
