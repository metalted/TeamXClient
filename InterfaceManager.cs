using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TeamXClient
{   
    /// <summary>
    /// This class is responsible for anything UI related, be it panels, playerlist or messages.
    /// </summary>
    public static class InterfaceManager
    {
        //The button in the toolbar for admins.
        public static TeamXPanelComponent teamXToolbarButton;

        //All the different panels used for TeamX admins.
        public static TeamXMainPanel mainPanel;
        public static TeamXPermissionPanel permissionPanel;
        public static TeamXSaveConfigurationPanel saveConfigurationPanel;
        public static TeamXLevelManagerPanel levelManagerPanel;

        //The overall state of all the panels.
        public static TeamXPanelState overallPanelState = TeamXPanelState.Closed;

        //Predefined colors for the UI.
        public static Color lightestGreen = new Color(0.336f, 1f, 0.766f);
        public static Color lightGreen = new Color(0f, 1f, 0.656f, 1f);
        public static Color green = new Color(0, 0.82f, 0.547f,  1f);
        public static Color darkGreen = new Color(0, 0.547f, 0.371f,  1f);
        public static Color darkestGreen = new Color(0, 0.348f, 0.238f, 1f);
        public static Color grey = new Color(0.3f, 0.3f, 0.3f, 1f);
        public static Color darkgrey = new Color(0.2f, 0.2f, 0.2f, 1f);

        /// <summary>
        /// Called when we enter the main menu to create the teamkist editor button.
        /// </summary>
        public static void SetupMainMenuUI()
        {
            GenerateLevelEditorOnlineButton();
        }

        /// <summary>
        /// Called when the level editor is opened in TeamX mode. Will setup all the required UI.
        /// </summary>
        /// <param name="management"></param>
        public static void SetupLevelEditorUI(bool management = false)
        {
            DisableLoadButton();

            if (management)
            {
                InitializePanels();
                CreateToolbarButton();
            }
        }

        /// <summary>
        /// Called when the main panel is opened.
        /// </summary>
        public static void OnPanelOpen()
        {
            Plugin.Instance.editor.Central.tool.DisableAllTools();
            Plugin.Instance.editor.Central.tool.RecolorButtons();
            Plugin.Instance.editor.Central.tool.currentTool = 3;
            Plugin.Instance.editor.Central.tool.inspectorTitle.text = "";

            overallPanelState = TeamXPanelState.Open;
        }

        /// <summary>
        /// Called when one of the panels is closed.
        /// </summary>
        public static void OnPanelClose()
        {
            Plugin.Instance.editor.Central.tool.EnableEditTool();
            Plugin.Instance.editor.Central.tool.RecolorButtons();
            Plugin.Instance.editor.Central.cam.OverrideOutsideGameView(false);

            overallPanelState = TeamXPanelState.Closed;
        }

        /// <summary>
        /// Creates the TeamX button at the top left of the tool bar.
        /// </summary>
        private static void CreateToolbarButton()
        {
            RectTransform teamXToolbarRect = GameObject.Instantiate(Plugin.Instance.editor.Central.tool.button_settings, Plugin.Instance.editor.Central.tool.button_settings.transform.parent).GetComponent<RectTransform>();
            teamXToolbarButton = new TeamXPanelComponent(TeamXPanelComponentType.Button, teamXToolbarRect);
            teamXToolbarButton.SetRectAnchors(0.005f, 0.1f, 0.025f, 0.9f);
            teamXToolbarButton.BindButton(() =>
            {
                Plugin.Instance.editor.Central.selection.DeselectAllBlocks(false, "");

                if (mainPanel != null)
                {
                    mainPanel.Open();
                }
            });           
        }

        /// <summary>
        /// Splits the regular level editor button in to two buttons, one for the regular editor and one for TeamX.
        /// </summary>
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
            editorOnlineGenericButton.normalColor = green;
            editorOnlineGenericButton.hoverColor = lightGreen;
            editorOnlineGenericButton.clickColor = lightestGreen;

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
            buttonText.text = "TeamX Editor";
        }

        /// <summary>
        /// Function that gets called when the TeamX button is clicked in the main menu.
        /// </summary>
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

        /// <summary>
        /// Greys out the load button in the level editor while in TeamX mode, because loading a level will mess up the server.
        /// </summary>
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

        /// <summary>
        /// Create the TeamX admin panels.
        /// </summary>
        private static void InitializePanels()
        {
            mainPanel = Utils.CreatePanel(Plugin.Instance.editor.Central, "TeamX").gameObject.AddComponent<TeamXMainPanel>();
            mainPanel.Initialize();

            permissionPanel = Utils.CreatePanel(Plugin.Instance.editor.Central, "TeamX Permissions").gameObject.AddComponent<TeamXPermissionPanel>();
            permissionPanel.Initialize();

            saveConfigurationPanel = Utils.CreatePanel(Plugin.Instance.editor.Central, "TeamX Save Configuration").gameObject.AddComponent<TeamXSaveConfigurationPanel>();
            saveConfigurationPanel.Initialize();

            levelManagerPanel = Utils.CreatePanel(Plugin.Instance.editor.Central, "TeamX Level Manager").gameObject.AddComponent<TeamXLevelManagerPanel>();
            levelManagerPanel.Initialize();
        }

        /// <summary>
        /// Helper function for cleaning a copied ui element.
        /// </summary>
        /// <param name="button"></param>
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

        /// <summary>
        /// Bind an action to a button click
        /// </summary>
        /// <param name="button">The button that will be clicked.</param>
        /// <param name="action">The action to perform.</param>
        public static void RebindButton(LEV_CustomButton button, UnityAction action)
        {
            button.onClick.AddListener(action);
        }

        /// <summary>
        /// Recolor a button.
        /// </summary>
        /// <param name="button">The button to recolor.</param>
        /// <param name="normalColor">The color in standard non interacted state.</param>
        /// <param name="hoverColor">The color when the user hovers over the button.</param>
        /// <param name="clickColor">The color when the user is holding mouse button over this button (:active)</param>
        /// <param name="recolorAllNormal">If true, will use the normal color for all colors, and hover and click parameters will be ignored.</param>
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

        /// <summary>
        /// Recolor a button in the standard TeamX color palette.
        /// </summary>
        /// <param name="button"></param>
        public static void StandardRecolorButton(LEV_CustomButton button)
        {
            RecolorButton(button, green, lightGreen, lightestGreen, false);
        }

        /// <summary>
        /// An OnGUI function that will show a simple list on screen with the currently connected players.
        /// </summary>
        public static void ShowPlayerList()
        {
            float startHeight = Screen.height * 0.15f;
            float boxWidth = Screen.width * 0.75f / 5f; // Width of each column
            float boxHeight = Screen.height * 0.85f / 20f; // Height of each row
            float columnSpacing = boxWidth + 10f; // Spacing between columns
            float rowSpacing = boxHeight + 5f; // Spacing between rows
            int maxRows = 20; // Max players per column

            string localPlayerName = PlayerManager.Instance.steamAchiever.GetPlayerName(false);
            string[] connectedPlayerNames = Plugin.Instance.multiplayer.GetAllPlayerNames();

            // Show the local player first.
            GUI.Box(new Rect(0, startHeight, boxWidth, boxHeight), localPlayerName);

            // Show the rest of the players underneath.
            int playerCount = connectedPlayerNames.Length;
            for (int i = 0; i < playerCount; i++)
            {
                int row = (i + 1) % maxRows; // Row position
                int column = (i + 1) / maxRows; // Column position

                float xPosition = column * columnSpacing; // Column offset
                float yPosition = startHeight + row * rowSpacing; // Row offset

                GUI.Box(new Rect(xPosition, yPosition, boxWidth, boxHeight), connectedPlayerNames[i]);
            }
        }
    }    
}
