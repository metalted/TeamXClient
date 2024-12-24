using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamXClient
{
    public class TeamXSaveConfigurationPanel : TeamXPanel
    {
        public int AutoSaveInterval;
        public int BackupCount;
        public bool KeepBackupWithNoEditors;
        public string LevelName;
        public bool LoadBackupOnStart;

        public Dictionary<TeamXPanelComponentName, TeamXPanelComponent> panelComponents;
        public TeamXPanelState currentState = TeamXPanelState.Closed;

        public void UpdateValues(int autosaveInterval, int backupCount, bool keepBackupWithNoEditors, string levelName, bool loadBackupOnStart)
        {
            AutoSaveInterval = autosaveInterval;
            BackupCount = backupCount;
            KeepBackupWithNoEditors = keepBackupWithNoEditors;
            LevelName = levelName;
            LoadBackupOnStart = loadBackupOnStart;
        }

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
            panelComponents[TeamXPanelComponentName.URL].SetText("TeamX Save Configuration");

            //Create the reload button
            panelComponents[TeamXPanelComponentName.Reload].SetButtonText("Reload");
            panelComponents[TeamXPanelComponentName.Reload].SetRectAnchors(0.68f, 0.88f, 0.89f, 0.975f);
            panelComponents[TeamXPanelComponentName.Reload].BindButton(() => OnReloadButton());

            //Create the apply button
            panelComponents[TeamXPanelComponentName.Save].SetRectAnchors(0.825f, 0.025f, 0.975f, 0.125f);
            panelComponents[TeamXPanelComponentName.Save].BindButton(() => OnApplyButton());

            //Create the close button
            panelComponents[TeamXPanelComponentName.Exit].BindButton(() => OnCloseButton());            

            //Create all the labels
            RectTransform autosaveLabelRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.URL].Rect.gameObject, panelComponents[TeamXPanelComponentName.URL].Rect.transform.parent).GetComponent<RectTransform>();
            panelComponents.Add(TeamXPanelComponentName.AutoSaveIntervalLabel, new TeamXPanelComponent(TeamXPanelComponentType.Text, TeamXPanelComponentName.AutoSaveIntervalLabel, autosaveLabelRect));
            
            RectTransform backupCountLabelRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.URL].Rect.gameObject, panelComponents[TeamXPanelComponentName.URL].Rect.transform.parent).GetComponent<RectTransform>();
            panelComponents.Add(TeamXPanelComponentName.BackupCountLabel, new TeamXPanelComponent(TeamXPanelComponentType.Text, TeamXPanelComponentName.BackupCountLabel, backupCountLabelRect));
            
            RectTransform keepBackupWithNoEditorsLabelRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.URL].Rect.gameObject, panelComponents[TeamXPanelComponentName.URL].Rect.transform.parent).GetComponent<RectTransform>();
            panelComponents.Add(TeamXPanelComponentName.KeepBackupWithNoEditorsLabel, new TeamXPanelComponent(TeamXPanelComponentType.Text, TeamXPanelComponentName.KeepBackupWithNoEditorsLabel, keepBackupWithNoEditorsLabelRect));
            
            RectTransform levelNameLabelRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.URL].Rect.gameObject, panelComponents[TeamXPanelComponentName.URL].Rect.transform.parent).GetComponent<RectTransform>();
            panelComponents.Add(TeamXPanelComponentName.LevelNameLabel, new TeamXPanelComponent(TeamXPanelComponentType.Text, TeamXPanelComponentName.LevelNameLabel, levelNameLabelRect));
            
            RectTransform loadBackupOnStartLabelRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.URL].Rect.gameObject, panelComponents[TeamXPanelComponentName.URL].Rect.transform.parent).GetComponent<RectTransform>();
            panelComponents.Add(TeamXPanelComponentName.LoadBackupOnStartLabel, new TeamXPanelComponent(TeamXPanelComponentType.Text, TeamXPanelComponentName.LoadBackupOnStartLabel, loadBackupOnStartLabelRect));

            //Create the inputs
            RectTransform autosaveInputRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.FileName].Rect.gameObject, panelComponents[TeamXPanelComponentName.FileName].Rect.transform.parent).GetComponent<RectTransform>();
            panelComponents.Add(TeamXPanelComponentName.AutoSaveIntervalInput, new TeamXPanelComponent(TeamXPanelComponentType.TextInput, TeamXPanelComponentName.AutoSaveIntervalInput, autosaveInputRect));

            RectTransform backupCountInputRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.FileName].Rect.gameObject, panelComponents[TeamXPanelComponentName.FileName].Rect.transform.parent).GetComponent<RectTransform>();
            panelComponents.Add(TeamXPanelComponentName.BackupCountInput, new TeamXPanelComponent(TeamXPanelComponentType.TextInput, TeamXPanelComponentName.BackupCountInput, backupCountInputRect));

            RectTransform keepBackupWithNoEditorsInputRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.Home].Rect.gameObject, panelComponents[TeamXPanelComponentName.Home].Rect.transform.parent).GetComponent<RectTransform>();
            panelComponents.Add(TeamXPanelComponentName.KeepBackupWithNoEditorsButton, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.KeepBackupWithNoEditorsButton, keepBackupWithNoEditorsInputRect));

            RectTransform levelNameInputRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.FileName].Rect.gameObject, panelComponents[TeamXPanelComponentName.FileName].Rect.transform.parent).GetComponent<RectTransform>();
            panelComponents.Add(TeamXPanelComponentName.LevelNameInput, new TeamXPanelComponent(TeamXPanelComponentType.TextInput, TeamXPanelComponentName.LevelNameInput, levelNameInputRect));

            RectTransform loadBackupOnStartInputRect = GameObject.Instantiate(panelComponents[TeamXPanelComponentName.Home].Rect.gameObject, panelComponents[TeamXPanelComponentName.Home].Rect.transform.parent).GetComponent<RectTransform>();
            panelComponents.Add(TeamXPanelComponentName.LoadBackupOnStartButton, new TeamXPanelComponent(TeamXPanelComponentType.Button, TeamXPanelComponentName.LoadBackupOnStartButton, loadBackupOnStartInputRect));

            //Position the elements
            panelComponents[TeamXPanelComponentName.AutoSaveIntervalLabel].SetRectAnchors(0.025f, 0.71f, 0.5f, 0.85f);
            panelComponents[TeamXPanelComponentName.AutoSaveIntervalLabel].SetText("Auto Save Interval:");

            panelComponents[TeamXPanelComponentName.BackupCountLabel].SetRectAnchors(0.025f, 0.57f, 0.5f, 0.71f);
            panelComponents[TeamXPanelComponentName.BackupCountLabel].SetText("Backup Count:");

            panelComponents[TeamXPanelComponentName.KeepBackupWithNoEditorsLabel].SetRectAnchors(0.025f, 0.43f, 0.5f, 0.57f);
            panelComponents[TeamXPanelComponentName.KeepBackupWithNoEditorsLabel].SetText("Keep Backup With No Editors:");

            panelComponents[TeamXPanelComponentName.LevelNameLabel].SetRectAnchors(0.025f, 0.29f, 0.5f, 0.43f);
            panelComponents[TeamXPanelComponentName.LevelNameLabel].SetText("Level Name:");

            panelComponents[TeamXPanelComponentName.LoadBackupOnStartLabel].SetRectAnchors(0.025f, 0.15f, 0.5f, 0.29f);
            panelComponents[TeamXPanelComponentName.LoadBackupOnStartLabel].SetText("Load Backup On Start:");

            panelComponents[TeamXPanelComponentName.AutoSaveIntervalInput].SetRectAnchors(0.5f, 0.72f, 0.975f, 0.84f);
            panelComponents[TeamXPanelComponentName.BackupCountInput].SetRectAnchors(0.5f, 0.58f, 0.975f, 0.70f);
            panelComponents[TeamXPanelComponentName.KeepBackupWithNoEditorsButton].SetRectAnchors(0.5f, 0.44f, 0.975f, 0.56f);
            panelComponents[TeamXPanelComponentName.LevelNameInput].SetRectAnchors(0.5f, 0.30f, 0.975f, 0.42f);
            panelComponents[TeamXPanelComponentName.LoadBackupOnStartButton].SetRectAnchors(0.5f, 0.16f, 0.975f, 0.28f);

            //Hide everything else
            panelComponents[TeamXPanelComponentName.Home].Disable();
            panelComponents[TeamXPanelComponentName.LoadPreview].Disable();
            panelComponents[TeamXPanelComponentName.UpOneLevel].Disable();
            panelComponents[TeamXPanelComponentName.Upload].Disable();
            panelComponents[TeamXPanelComponentName.NewFolder].Disable();
            panelComponents[TeamXPanelComponentName.OpenFolder].Disable();
            panelComponents[TeamXPanelComponentName.TypeText].Disable();
            panelComponents[TeamXPanelComponentName.FileName].Disable();
            panelComponents[TeamXPanelComponentName.ScrollView].Disable();
        }

        private void OnCloseButton()
        {
            Close();
        }

        private void Close()
        {
            Debug.Log("Close");
            gameObject.SetActive(false);
            currentState = TeamXPanelState.Closed;
        }

        private void OnReloadButton()
        {
            Debug.Log("Reload");

            Plugin.Instance.client.SendSaveConfigurationRequestPacket();
        }       

        private void OnApplyButton()
        {
            Debug.Log("Apply");


            Close();
        }
    }
}
