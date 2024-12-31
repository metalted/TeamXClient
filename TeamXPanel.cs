using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamXClient
{
    public enum TeamXPanelComponentName { Background, Save, Home, AutoSaves, Backups, UpOneLevel, NewFolder, Sort, Upload, OpenFolder, Exit, ScrollView, URL, Load, LoadHere, LoadFile, Reload, LoadPreview, SavePreview, FileName, TypeText, SearchBar, Download, Search, PreviousPage, NextPage, PageCounter, SelectedName, SearchResultScrollView, PermissionEntryUser, PermissionEntryBanned, PermissionEntryGuest, PermissionEntryDefault, PermissionEntryTrusted, PermissionEntryAdmin, AutoSaveIntervalLabel, AutoSaveIntervalInput, BackupCountLabel, BackupCountInput, KeepBackupWithNoEditorsLabel, KeepBackupWithNoEditorsButton, LevelNameLabel, LevelNameInput, LoadBackupOnStartLabel, LoadBackupOnStartButton };
    public enum TeamXPanelComponentType { Button, Image, Text, ScrollView, TextInput };
    public enum TeamXPanelState { Closed, Open };

    public class TeamXPanel : MonoBehaviour
    {
        private Dictionary<TeamXPanelComponentName, TeamXPanelComponent> panelComponents;
        public TeamXPanelState currentState = TeamXPanelState.Closed;

        public Dictionary<string, TeamXPanelComponent> elements;

        public void Initialize()
        {
            GetPanelComponents();
            elements = new Dictionary<string, TeamXPanelComponent>();
            elements.Add("ScrollView", panelComponents[TeamXPanelComponentName.ScrollView]);
            OnInitialize(gameObject.name);
        }

        public virtual void OnInitialize(string title)
        {

        }

        public virtual void Open(bool notifyManager = true)
        {
            currentState = TeamXPanelState.Open;
            gameObject.SetActive(true);

            if(notifyManager)
            {
                InterfaceManager.OnPanelOpen();
            }
            
            OnOpen();
        }

        public virtual void OnOpen()
        {

        }


        private void GetPanelComponents()
        {
            panelComponents = new Dictionary<TeamXPanelComponentName, TeamXPanelComponent>();

            RectTransform innerPanel = transform.GetChild(1).GetComponent<RectTransform>();
            panelComponents.Add(TeamXPanelComponentName.Background, new TeamXPanelComponent(TeamXPanelComponentType.Image, innerPanel));

            foreach (RectTransform rt in innerPanel)
            {
                switch (rt.gameObject.name)
                {
                    case "Save Panel Save Button":
                        panelComponents.Add(TeamXPanelComponentName.Save, new TeamXPanelComponent(TeamXPanelComponentType.Button, rt));
                        panelComponents[TeamXPanelComponentName.Save].Disable();
                        break;
                    case "Home Button":
                        panelComponents.Add(TeamXPanelComponentName.Home, new TeamXPanelComponent(TeamXPanelComponentType.Button, rt));
                        panelComponents[TeamXPanelComponentName.Home].Disable();
                        break;
                    case "Autosaves Button":
                        panelComponents.Add(TeamXPanelComponentName.AutoSaves, new TeamXPanelComponent(TeamXPanelComponentType.Button, rt));
                        panelComponents[TeamXPanelComponentName.AutoSaves].Disable();
                        break;
                    case "Backups Button":
                        panelComponents.Add(TeamXPanelComponentName.Backups, new TeamXPanelComponent(TeamXPanelComponentType.Button, rt));
                        panelComponents[TeamXPanelComponentName.Backups].Disable();
                        break;
                    case "Up One Level Button":
                        panelComponents.Add(TeamXPanelComponentName.UpOneLevel, new TeamXPanelComponent(TeamXPanelComponentType.Button, rt));
                        panelComponents[TeamXPanelComponentName.UpOneLevel].Disable();                       
                        break;
                    case "New Folder Button":
                        panelComponents.Add(TeamXPanelComponentName.NewFolder, new TeamXPanelComponent(TeamXPanelComponentType.Button, rt));
                        panelComponents[TeamXPanelComponentName.NewFolder].Disable();
                        break;
                    case "Sort Regular Levels Button":
                        panelComponents.Add(TeamXPanelComponentName.Sort, new TeamXPanelComponent(TeamXPanelComponentType.Button, rt));
                        panelComponents[TeamXPanelComponentName.Sort].Disable();
                        break;
                    case "Open Folder Button":
                        panelComponents.Add(TeamXPanelComponentName.OpenFolder, new TeamXPanelComponent(TeamXPanelComponentType.Button, rt));
                        panelComponents[TeamXPanelComponentName.OpenFolder].Disable();
                        break;
                    case "Exit Saving":
                        panelComponents.Add(TeamXPanelComponentName.Exit, new TeamXPanelComponent(TeamXPanelComponentType.Button, rt));
                        panelComponents[TeamXPanelComponentName.Exit].BindButton(() =>
                        {
                            OnCloseButton();
                        });
                        break;
                    case "URL":
                        panelComponents.Add(TeamXPanelComponentName.URL, new TeamXPanelComponent(TeamXPanelComponentType.Text, rt));
                        panelComponents[TeamXPanelComponentName.URL].SetRectAnchors(0.06f, 0.88f, 0.5f, 0.95f);
                        break;
                    case "Scroll View":
                        panelComponents.Add(TeamXPanelComponentName.ScrollView, new TeamXPanelComponent(TeamXPanelComponentType.ScrollView, rt));
                        panelComponents[TeamXPanelComponentName.ScrollView].Disable();
                        break;
                    case "TextMeshPro - InputField":
                        panelComponents.Add(TeamXPanelComponentName.FileName, new TeamXPanelComponent(TeamXPanelComponentType.TextInput, rt));
                        panelComponents[TeamXPanelComponentName.FileName].Disable();
                        break;
                    case "Medal Times Warning":
                        panelComponents.Add(TeamXPanelComponentName.TypeText, new TeamXPanelComponent(TeamXPanelComponentType.Text, rt));
                        panelComponents[TeamXPanelComponentName.TypeText].Disable();
                        break;
                    case "AreYouSure (false)":
                        //confirmPanel = new TeamXConfirmPanel(this, rt);
                        break;
                    case "Create New Folder Panel (false)":
                        //folderPanel = new TeamXFolderPanel(this, rt);
                        break;
                }
            }
        }

        protected virtual void OnCloseButton()
        {
            Close();
        }

        public virtual void Close(bool notifyManager = true)
        {
            gameObject.SetActive(false);
            currentState = TeamXPanelState.Closed;

            if (notifyManager)
            {
                InterfaceManager.OnPanelClose();
            }
        }

        protected void SetBackgroundColor(Color color)
        {
            panelComponents[TeamXPanelComponentName.Background].Image.color = color;
        }

        protected void SetTitle(string title)
        {
            panelComponents[TeamXPanelComponentName.URL].SetText(title);
        }

        protected TeamXPanelComponent CreateImageTextButton(string name, string icon, string text, bool addToElements = true)
        {
            RectTransform rect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.Home].Rect.gameObject, panelComponents[TeamXPanelComponentName.Home].Rect.transform.parent).GetComponent<RectTransform>();
            TeamXPanelComponent c = new TeamXPanelComponent(TeamXPanelComponentType.Button, rect);
            
            if(addToElements)
            {
                elements.Add(name, c);
            }
            
            c.SetGameObjectName(name);

            //Implement icon

            c.SetButtonText(text);
            c.Enable();

            return c;
        }

        protected TeamXPanelComponent CreateImageButton(string name, string icon, bool addToElements = true)
        {
            RectTransform rect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.Save].Rect.gameObject, panelComponents[TeamXPanelComponentName.Home].Rect.transform.parent).GetComponent<RectTransform>();
            TeamXPanelComponent c = new TeamXPanelComponent(TeamXPanelComponentType.Button, rect);

            if(addToElements)
            {
                elements.Add(name, c);
            }
            
            c.SetGameObjectName(name);
            c.Enable();

            //Implement icon

            return c;
        }

        protected TeamXPanelComponent CreateTextButton(string name, string text, bool addToElements = true)
        {
            RectTransform rect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.Home].Rect.gameObject, panelComponents[TeamXPanelComponentName.Home].Rect.transform.parent).GetComponent<RectTransform>();
            TeamXPanelComponent c = new TeamXPanelComponent(TeamXPanelComponentType.Button, rect);

            if(addToElements)
            {
                elements.Add(name, c);
            }

            c.SetGameObjectName(name);
            c.Enable();

            c.HideButtonImage();
            c.SetButtonText(text);
            c.SetButtonTextRectAnchors(0.05f, 0.05f, 0.95f, 0.95f);

            return c;
        }

        protected TeamXPanelComponent CreateLabel(string name, string text, bool addToElements = true)
        {
            RectTransform rect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.URL].Rect.gameObject, panelComponents[TeamXPanelComponentName.URL].Rect.transform.parent).GetComponent<RectTransform>();
            TeamXPanelComponent c = new TeamXPanelComponent(TeamXPanelComponentType.Text, rect);

            if(addToElements)
            {
                elements.Add(name, c);
            }

            c.SetGameObjectName(name);
            c.Enable();
            c.SetText(text);

            return c;
        }

        protected TeamXPanelComponent CreateTextInput(string name, bool addToElements = true)
        {
            RectTransform rect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.FileName].Rect.gameObject, panelComponents[TeamXPanelComponentName.FileName].Rect.transform.parent).GetComponent<RectTransform>();
            TeamXPanelComponent c = new TeamXPanelComponent(TeamXPanelComponentType.TextInput, rect);

            if(addToElements)
            {
                elements.Add(name, c);
            }

            c.SetGameObjectName(name);
            c.Enable();

            return c;
        }
    }
}
