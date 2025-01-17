using System;
using System.Collections.Generic;
using TeamXNetwork;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ZeepSDK.Utilities;
using ZeepSDK.Controls;
using System.Linq;

namespace TeamXClient
{   
    public struct ChatLine
    {
        public string userName;
        public string userColor;
        public string message;
    }

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

        public static void UpdateLoop()
        {
            //When in the online editor, toggle the player list when holding down the key.
            if (Plugin.Instance.IsTeamXEditor())
            {                
                if (Input.GetKeyDown(Plugin.Instance.cfg_showPlayerList.Value))
                {
                    ShowPlayerList();
                }

                if (Input.GetKeyUp(Plugin.Instance.cfg_showPlayerList.Value))
                {
                    HidePlayerList();
                }
            }

            //When in the online editor, and we press the open chat input key, show the chat input.
            if(Plugin.Instance.IsTeamXEditor())
            {
                if (Input.GetKeyDown(Plugin.Instance.cfg_openChatField.Value))
                {
                    if (!chatInputIsOpen)
                    {
                        OpenChatInput();
                    }
                }
            }

            //When in the online editor, toggle the chat visibility bool with a key.
            if(Plugin.Instance.IsTeamXEditor())
            {
                if(Input.GetKeyDown(Plugin.Instance.cfg_toggleChat.Value))
                {
                    Plugin.Instance.cfg_chatEnabled.Value = !Plugin.Instance.cfg_chatEnabled.Value;
                    ApplyChatWindowVisibilitySetting();
                }
            }
        }

        public static void OnGUILoop()
        {
            if(chatInputIsOpen)
            {
                RenderChatInputGUI();
            }
        }

        public static void BlockInput()
        {
            currentBlocker = ControlsApi.DisableAllInput();
        }

        public static void UnblockInput()
        {
            if (currentBlocker.HasValue)
            {
                currentBlocker.Value.Dispose();
                currentBlocker = null;
            }

            //As this is only really used for chat, its ok to put this in here, to make sure it absolutely doesnt show.
            chatInputIsOpen = false;
            chatInputNeedsFocus = false;
        }

        #region MainMenu
        /// <summary>
        /// Called when we enter the main menu to create the teamkist editor button.
        /// </summary>
        public static void SetupMainMenuUI()
        {
            GenerateLevelEditorOnlineButton();
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

            // Find the splitscreen image and apply it to the teamx button.
            Image targetImage = FindImageByName("Players Icon 3");

            if (targetImage != null)
            {
                // Get the second child named "Image" from the editorOnlineGenericButton
                Transform secondChild = editorOnlineGenericButton.transform.GetChild(1); // 1 for the second child (0-based index)
                if (secondChild != null && secondChild.name == "Image")
                {
                    Image teamxButtonImage = secondChild.GetComponent<Image>();

                    if (teamxButtonImage != null)
                    {
                        teamxButtonImage.sprite = targetImage.sprite;
                    }
                }
            }
        }

        public static Image FindImageByName(string imageName)
        {
            // Find all Image components in the scene
            Image[] allImages = GameObject.FindObjectsOfType<Image>(true);

            // Find the one with the specific name
            return allImages.FirstOrDefault(image => image.name == imageName);
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
        #endregion

        #region LevelEditor

        /// <summary>
        /// Called when the level editor is opened in TeamX mode. Will setup all the required UI.
        /// </summary>
        /// <param name="management">Indicates if this client needs administrative panels.</param>
        public static void SetupLevelEditorUI(bool management = false)
        {
            DisableLoadButton();

            if (management)
            {
                InitializePanels();
                CreateToolbarButton();
            }

            SetupChatUI();
            SetupPlayerListUI();
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

        #endregion

        #region Helpers
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
        #endregion

        #region PlayerList
        public static TextMeshProUGUI playerListText;
        /// <summary>
        /// Will copy the Tooltips TextMesh to be used for the player list.
        /// </summary>
        
        public static void SetupPlayerListUI()
        {
            //Find the LEV_Tooltips
            LEV_Tooltips tooltips = GameObject.FindObjectOfType<LEV_Tooltips>();

            if (tooltips == null)
            {
                Plugin.Instance.Log("Tooltips UI not found.", LogType.Error);
                return;
            }

            //Copy the tooltips to the player list window
            playerListText = GameObject.Instantiate(tooltips.gameObject, tooltips.transform.parent).GetComponent<TextMeshProUGUI>();
            playerListText.gameObject.name = "PlayerListWindow";

            //Make the PlayerListWindow moveable.
            ZeepSDK.UI.UIApi.AddToConfigurator(playerListText.GetComponent<RectTransform>());

            //Get the index of the tooltip and place the playerlist window there
            int tooltipIndex = tooltips.transform.GetSiblingIndex();
            playerListText.transform.SetSiblingIndex(tooltipIndex);
            playerListText.enableWordWrapping = false;

            //Destroy the unwanted components
            GameObject.Destroy(playerListText.GetComponent<LEV_Tooltips>());

            //Hide the playerlist as it is only visible if a key is held
            playerListText.gameObject.SetActive(false);
        }

        /// <summary>
        /// Fills the player list with the current players and makes it visible.
        /// </summary>
        public static void ShowPlayerList()
        {
            Debug.LogWarning("Showing");
            if(playerListText == null)
            {
                Debug.LogWarning("Player list text is null wtf");
                return;
            }

            //Get the current players.
            string localPlayerName = PlayerManager.Instance.steamAchiever.GetPlayerName(false);
            string[] connectedPlayerNames = Plugin.Instance.multiplayer.GetAllPlayerNames();

            // Combine the localPlayerName and connectedPlayerNames into a single array
            string[] allPlayerNames = new string[connectedPlayerNames.Length + 2];
            allPlayerNames[0] = "Online Players:";
            allPlayerNames[1] = localPlayerName;
            connectedPlayerNames.CopyTo(allPlayerNames, 2);

            //Create the text
            string playerListContent = string.Join('\n', allPlayerNames);

            playerListText.text = playerListContent;
            playerListText.gameObject.SetActive(true);       
        }

        /// <summary>
        /// Hides the player list if available.
        /// </summary>
        public static void HidePlayerList()
        {
            if(playerListText == null)
            {
                return;
            }

            playerListText.gameObject.SetActive(false);
        }
        #endregion

        #region Chat
        public static TextMeshProUGUI chatText;
        public static string chatInputText = "";
        public static List<ChatLine> chatHistory = new List<ChatLine>();

        //A bool that keeps track if the chat input box is open or not.
        private static bool chatInputIsOpen = false;
        public static bool chatInputNeedsFocus = false;

        //Blocker used for blocking inputs when the chat input is open.
        private static DisposableBag? currentBlocker = null;

        /// <summary>
        /// Will copy the Tooltips TextMesh to be used for the chat log.
        /// </summary>
        public static void SetupChatUI()
        {
            //Find the LEV_Tooltips
            LEV_Tooltips tooltips = GameObject.FindObjectOfType<LEV_Tooltips>();

            if (tooltips == null)
            {
                Plugin.Instance.Log("Tooltips UI not found.", LogType.Error);
                return;
            }

            //Add the tooltips to the configurator
            ZeepSDK.UI.UIApi.AddToConfigurator(tooltips.GetComponent<RectTransform>());            

            //Copy the tooltips to the chat window
            chatText = GameObject.Instantiate(tooltips.gameObject, tooltips.transform.parent).GetComponent<TextMeshProUGUI>();
            chatText.gameObject.name = "ChatWindow";

            //Make the ChatWindow moveable.
            ZeepSDK.UI.UIApi.AddToConfigurator(chatText.GetComponent<RectTransform>());

            //Get the index of the tooltip and place the chat there
            int tooltipIndex = tooltips.transform.GetSiblingIndex();
            chatText.transform.SetSiblingIndex(tooltipIndex);
            chatText.enableWordWrapping = true;

            //Destroy the unwanted components
            GameObject.Destroy(chatText.GetComponent<LEV_Tooltips>());

            //Show or hide the chat based on the config.
            ApplyChatWindowVisibilitySetting();

            //Add the current history to the chat.
            RefreshChatBox();
        }

        /// <summary>
        /// Hide or show the chat based on the config setting.
        /// </summary>
        public static void ApplyChatWindowVisibilitySetting()
        {
            if(chatText == null)
            {
                return;
            }

            if (Plugin.Instance.cfg_chatEnabled.Value)
            {
                chatText.gameObject.SetActive(true);
            }
            else
            {
                chatText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Updates the chat history and chat window when a message is received. Local messages will also be send to this function.
        /// </summary>
        /// <param name="chatMessage">The <see cref="ChatMessagePacket"/> received from the server, or created from local chat input. </param>
        public static void ReceivedChat(ChatMessagePacket chatMessage)
        {
            //Create the chatline
            ChatLine line = new ChatLine()
            {
                message = chatMessage.Message,
                userName = chatMessage.Username,
                userColor = chatMessage.Color
            };

            //Add the chat line to the history
            chatHistory.Add(line);

            if (chatHistory.Count > Plugin.Instance.cfg_chatHistoryLength.Value)
            {
                chatHistory.RemoveAt(0);
            }

            RefreshChatBox();
        }

        /// <summary>
        /// Clears chat box and refills it with the latest chat.
        /// </summary>
        public static void RefreshChatBox()
        {
            if (chatText != null)
            {
                List<string> chatLines = new List<string>();

                foreach (ChatLine c in chatHistory)
                {
                    chatLines.Add($"<color={c.userColor}>{c.userName}</color>: <color=#ffffff>{c.message}</color>");
                }

                string fullChat = string.Join("\n", chatLines.ToArray());

                chatText.text = fullChat;
            }
        }

        /// <summary>
        /// Opens the chat input in the level editor
        /// </summary>
        public static void OpenChatInput()
        {
            //Set a bool so OnGUI knows to show the input.
            chatInputIsOpen = true;

            //When opening let the OnGUI take focus on the input.
            chatInputNeedsFocus = true;

            BlockInput();
        }

        public static void RenderChatInputGUI()
        {
            //Detect the enter press in the GUI for sending the message.
            bool enterPressed = false;
            Event e = Event.current;
            if (e.isKey)
            {
                string name = e.keyCode.ToString().ToLower();
                if (name.Contains("return"))
                {
                    enterPressed = true;
                }
            }

            GUI.SetNextControlName("ChatInputField");
            chatInputText = GUI.TextField(new Rect(0, Screen.height - 30f, Screen.width * 0.25f, 30f), chatInputText);

            if(chatInputNeedsFocus)
            {
                GUI.FocusControl("ChatInputField");
                chatInputNeedsFocus = false;
            }

            if (enterPressed)
            {
                ProcessChatInput();
            }
        }

        /// <summary>
        /// Called when the chat enabled setting has changed so we can enable or disable the game object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Cfg_chatEnabled_SettingChanged(object sender, EventArgs e)
        {
            ApplyChatWindowVisibilitySetting();
        }

        public static void ProcessChatInput()
        {
            //Get Text
            string currentText = chatInputText;

            //Clear field
            chatInputText = "";

            //Close chat field
            chatInputIsOpen = false;

            if (!string.IsNullOrEmpty(currentText))
            {
                Plugin.Instance.client.SendChatMessage(currentText);
            }

            UnblockInput();
        }
        #endregion
    }    
}
