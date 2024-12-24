using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamXClient
{
    public class PermissionWindowEntry
    {
        public Dictionary<TeamXPanelComponentName, TeamXPanelComponent> entryComponents;

        public string user;
        public ulong steamID;
        public bool banned;
        public bool guest;
        public bool dfault;
        public bool trusted;
        public bool admin;

        public PermissionWindowEntry()
        {
            entryComponents = new Dictionary<TeamXPanelComponentName, TeamXPanelComponent>();
        }

        public void DestroyComponents()
        {
            foreach (KeyValuePair<TeamXPanelComponentName, TeamXPanelComponent> kvp in entryComponents)
            {
                if (kvp.Value != null)
                {
                    if (kvp.Value.Rect != null)
                    {
                        GameObject.Destroy(kvp.Value.Rect.gameObject);
                    }
                }
            }

            entryComponents.Clear();
        }

        public void ColorCurrentState()
        {
            InterfaceManager.RecolorButton(entryComponents[TeamXPanelComponentName.PermissionEntryBanned].Button, banned ? Color.green : InterfaceManager.darkgrey, Color.black, Color.black, true);
            InterfaceManager.RecolorButton(entryComponents[TeamXPanelComponentName.PermissionEntryGuest].Button, guest ? Color.green : InterfaceManager.darkgrey, Color.black, Color.black, true);
            InterfaceManager.RecolorButton(entryComponents[TeamXPanelComponentName.PermissionEntryDefault].Button, dfault ? Color.green : InterfaceManager.darkgrey, Color.black, Color.black, true);
            InterfaceManager.RecolorButton(entryComponents[TeamXPanelComponentName.PermissionEntryTrusted].Button, trusted ? Color.green : InterfaceManager.darkgrey, Color.black, Color.black, true);
            InterfaceManager.RecolorButton(entryComponents[TeamXPanelComponentName.PermissionEntryAdmin].Button, admin ? Color.green : InterfaceManager.darkgrey, Color.black, Color.black, true);
        }

        public void ResetChoice()
        {
            banned = false;
            guest = false;
            dfault = false;
            trusted = false;
            admin = false;
        }

        public void SetBanned()
        {
            ResetChoice();
            banned = true;
            ColorCurrentState();
        }

        public void SetGuest()
        {
            ResetChoice();
            guest = true;
            ColorCurrentState();
        }

        public void SetDefault()
        {
            ResetChoice();
            dfault = true;
            ColorCurrentState();
        }

        public void SetTrusted()
        {
            ResetChoice();
            trusted = true;
            ColorCurrentState();
        }

        public void SetAdmin()
        {
            ResetChoice();
            admin = true;
            ColorCurrentState();
        }
    }
    
    public class TeamXPermissionPanel : TeamXPanel
    {     
        //Holds the rows of 'checkbox' buttons for each player.
        public List<PermissionWindowEntry> windowEntries = new List<PermissionWindowEntry>();
        //A prefab for constructing a checkbox button.
        public RectTransform buttonPrefabRect;

        //Zeepkist panel components.
        public Dictionary<TeamXPanelComponentName, TeamXPanelComponent> panelComponents;
        public TeamXPanelState currentState = TeamXPanelState.Closed;

        public void Initialize(LEV_LevelEditorCentral central)
        {
            GetPanelComponents();
            ConfigurePanel();
        }

        private void GetPanelComponents()
        {
            panelComponents = new Dictionary<TeamXPanelComponentName, TeamXPanelComponent>();            

            RectTransform innerPanel = transform.GetChild(1).GetComponent<RectTransform>();
            panelComponents.Add(TeamXPanelComponentName.Background, new TeamXPanelComponent(TeamXPanelComponentType.Image, TeamXPanelComponentName.Background, innerPanel));

            foreach (RectTransform rt in innerPanel)
            {
                switch (rt.gameObject.name)
                {
                    case "Save Panel Save Button":
                        panelComponents.Add(TeamXPanelComponentName.Save, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.Save, rt));
                        break;
                    case "Home Button":
                        panelComponents.Add(TeamXPanelComponentName.Home, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.Home, rt));
                        break;
                    case "Autosaves Button":
                        panelComponents.Add(TeamXPanelComponentName.Reload, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.Reload, rt));
                        break;
                    case "Backups Button":
                        panelComponents.Add(TeamXPanelComponentName.LoadPreview, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.LoadPreview, rt));
                        break;
                    case "Up One Level Button":
                        panelComponents.Add(TeamXPanelComponentName.UpOneLevel, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.UpOneLevel, rt));
                        break;
                    case "New Folder Button":
                        panelComponents.Add(TeamXPanelComponentName.NewFolder, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.NewFolder, rt));
                        break;
                    case "Sort Regular Levels Button":
                        panelComponents.Add(TeamXPanelComponentName.Upload, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.Upload, rt));
                        break;
                    case "Open Folder Button":
                        panelComponents.Add(TeamXPanelComponentName.OpenFolder, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.OpenFolder, rt));
                        break;
                    case "Exit Saving":
                        panelComponents.Add(TeamXPanelComponentName.Exit, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.Exit, rt));
                        break;
                    case "URL":
                        panelComponents.Add(TeamXPanelComponentName.URL, new TeamXPanelComponent(TeamXPanelComponentType.Text, TeamXPanelComponentName.URL, rt));
                        break;
                    case "Scroll View":
                        panelComponents.Add(TeamXPanelComponentName.ScrollView, new TeamXPanelComponent(TeamXPanelComponentType.ScrollView, TeamXPanelComponentName.ScrollView, rt));
                        break;
                    case "TextMeshPro - InputField":
                        panelComponents.Add(TeamXPanelComponentName.FileName, new TeamXPanelComponent(TeamXPanelComponentType.TextInput, TeamXPanelComponentName.FileName, rt));
                        break;
                    case "Medal Times Warning":
                        panelComponents.Add(TeamXPanelComponentName.TypeText, new TeamXPanelComponent(TeamXPanelComponentType.Text, TeamXPanelComponentName.TypeText, rt));
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
        
        private void ConfigurePanel()
        {
            //Set the background color of the panel
            panelComponents[TeamXPanelComponentName.Background].Image.color = InterfaceManager.darkGreen;

            //Create the permissions title
            panelComponents[TeamXPanelComponentName.URL].SetText("TeamX Permissions");

            //Create the reload button
            panelComponents[TeamXPanelComponentName.Reload].SetButtonText("Reload");
            panelComponents[TeamXPanelComponentName.Reload].SetRectAnchors(0.68f, 0.88f, 0.89f, 0.975f);
            panelComponents[TeamXPanelComponentName.Reload].BindButton(() => OnReloadButton());

            //Create the apply button
            panelComponents[TeamXPanelComponentName.Save].SetRectAnchors(0.825f, 0.025f, 0.975f, 0.125f);
            panelComponents[TeamXPanelComponentName.Save].BindButton(() => OnApplyButton());

            //Create the close button
            panelComponents[TeamXPanelComponentName.Exit].BindButton(() => OnCloseButton());

            //Resize the scroll view
            panelComponents[TeamXPanelComponentName.ScrollView].SetRectAnchors(0.025f, 0.15f, 0.975f, 0.85f);
            panelComponents[TeamXPanelComponentName.ScrollView].SetGridLayoutColumns(6, 0.25f);

            //Hide everything else
            panelComponents[TeamXPanelComponentName.Home].Disable();
            panelComponents[TeamXPanelComponentName.LoadPreview].Disable();
            panelComponents[TeamXPanelComponentName.UpOneLevel].Disable();
            panelComponents[TeamXPanelComponentName.Upload].Disable();
            panelComponents[TeamXPanelComponentName.NewFolder].Disable();
            panelComponents[TeamXPanelComponentName.OpenFolder].Disable();
            panelComponents[TeamXPanelComponentName.TypeText].Disable();
            panelComponents[TeamXPanelComponentName.FileName].Disable();

            buttonPrefabRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.Home].Rect.gameObject, panelComponents[TeamXPanelComponentName.Home].Rect.transform.parent).GetComponent<RectTransform>();
            buttonPrefabRect.gameObject.SetActive(false);            
        }

        private void OnCloseButton() 
        {
            Close();
        }

        private void Close()
        {
            gameObject.SetActive(false);
            currentState = TeamXPanelState.Closed;
        }

        private void OnReloadButton() 
        {
            EmptyWindowEntries();
            Plugin.Instance.client.SendPermissionTableRequest();
        }    

        private void EmptyWindowEntries()
        {
            foreach (PermissionWindowEntry pwe in windowEntries)
            {
                pwe.DestroyComponents();
            }

            windowEntries.Clear();
        }

        public void ImportEntries(List<(ulong,string,string)> entries)
        {
            EmptyWindowEntries();

            foreach((ulong,string,string) e in entries)
            {
                PermissionWindowEntry wEntry = new PermissionWindowEntry();
                wEntry.steamID = e.Item1;
                wEntry.user = e.Item2;
                switch(e.Item3)
                {
                    case "banned":
                        wEntry.banned = true;
                        break;
                    case "guest":
                        wEntry.guest = true;
                        break;
                    case "default":
                        wEntry.dfault = true;
                        break;
                    case "trusted":
                        wEntry.trusted = true;
                        break;
                    case "admin":
                        wEntry.admin = true;
                        break;
                }

                windowEntries.Add(wEntry);
            }

            FillWindowEntries();
        }
        
        private void FillWindowEntries()
        {
            foreach (PermissionWindowEntry pwe in windowEntries)
            {
                RectTransform userRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.Home].Rect.gameObject, panelComponents[TeamXPanelComponentName.Home].Rect.transform.parent).GetComponent<RectTransform>();
                pwe.entryComponents.Add(TeamXPanelComponentName.PermissionEntryUser, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.PermissionEntryUser, userRect));

                RectTransform bannedRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.Home].Rect.gameObject, panelComponents[TeamXPanelComponentName.Home].Rect.transform.parent).GetComponent<RectTransform>();
                pwe.entryComponents.Add(TeamXPanelComponentName.PermissionEntryBanned, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.PermissionEntryBanned, bannedRect));

                RectTransform guestRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.Home].Rect.gameObject, panelComponents[TeamXPanelComponentName.Home].Rect.transform.parent).GetComponent<RectTransform>();
                pwe.entryComponents.Add(TeamXPanelComponentName.PermissionEntryGuest, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.PermissionEntryGuest, guestRect));

                RectTransform defaultRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.Home].Rect.gameObject, panelComponents[TeamXPanelComponentName.Home].Rect.transform.parent).GetComponent<RectTransform>();
                pwe.entryComponents.Add(TeamXPanelComponentName.PermissionEntryDefault, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.PermissionEntryDefault, defaultRect));

                RectTransform trustedRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.Home].Rect.gameObject, panelComponents[TeamXPanelComponentName.Home].Rect.transform.parent).GetComponent<RectTransform>();
                pwe.entryComponents.Add(TeamXPanelComponentName.PermissionEntryTrusted, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.PermissionEntryTrusted, trustedRect));

                RectTransform adminRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.Home].Rect.gameObject, panelComponents[TeamXPanelComponentName.Home].Rect.transform.parent).GetComponent<RectTransform>();
                pwe.entryComponents.Add(TeamXPanelComponentName.PermissionEntryAdmin, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.PermissionEntryAdmin, adminRect));

                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryUser].Rect.SetParent(panelComponents[TeamXPanelComponentName.ScrollView].ScrollRect.content);
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryUser].SetButtonText(pwe.user);
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryUser].Enable();
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryUser].HideButtonImage();
                InterfaceManager.RecolorButton(pwe.entryComponents[TeamXPanelComponentName.PermissionEntryUser].Button, Color.black, Color.black, Color.black, true);

                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryBanned].Rect.SetParent(panelComponents[TeamXPanelComponentName.ScrollView].ScrollRect.content);
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryBanned].SetButtonText("Banned");
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryBanned].Enable();
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryBanned].HideButtonImage();
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryBanned].BindButton(() =>
                {
                    pwe.SetBanned();
                });

                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryGuest].Rect.SetParent(panelComponents[TeamXPanelComponentName.ScrollView].ScrollRect.content);
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryGuest].SetButtonText("Guest");
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryGuest].Enable();
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryGuest].HideButtonImage();
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryGuest].BindButton(() =>
                {
                    pwe.SetGuest();
                });

                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryDefault].Rect.SetParent(panelComponents[TeamXPanelComponentName.ScrollView].ScrollRect.content);
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryDefault].SetButtonText("Default");
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryDefault].Enable();
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryDefault].HideButtonImage();
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryDefault].BindButton(() =>
                {
                    pwe.SetDefault();
                });

                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryTrusted].Rect.SetParent(panelComponents[TeamXPanelComponentName.ScrollView].ScrollRect.content);
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryTrusted].SetButtonText("Trusted");
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryTrusted].Enable();
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryTrusted].HideButtonImage();
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryTrusted].BindButton(() =>
                {
                    pwe.SetTrusted();
                });

                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryAdmin].Rect.SetParent(panelComponents[TeamXPanelComponentName.ScrollView].ScrollRect.content);
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryAdmin].SetButtonText("Admin");
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryAdmin].Enable();
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryAdmin].HideButtonImage();
                pwe.entryComponents[TeamXPanelComponentName.PermissionEntryAdmin].BindButton(() =>
                {
                    pwe.SetAdmin();
                });

                pwe.ColorCurrentState();
            }
        }

        private void OnApplyButton()
        { 
            Plugin.Instance.client.SendPermissionTableSubmit(windowEntries);
            Close();
        }
    }
}
