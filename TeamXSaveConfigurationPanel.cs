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

        public override void OnInitialize(string title)
        {
            SetBackgroundColor(InterfaceManager.darkGreen);
            SetTitle(title);

            CreateTextButton("BackToMain", "<<");
            elements["BackToMain"].SetRectAnchors(0.025f, 0.88f, 0.05f, 0.975f);
            elements["BackToMain"].BindButton(() =>
            {
                InterfaceManager.mainPanel.Open(false);
                Close(false);
            });

            CreateTextButton("Reload", "Reload");
            elements["Reload"].SetRectAnchors(0.68f, 0.88f, 0.89f, 0.975f);
            elements["Reload"].BindButton(() => OnReloadButton());

            CreateTextButton("Apply", "Send to Server");
            elements["Apply"].SetRectAnchors(0.7f, 0.025f, 0.975f, 0.125f);
            elements["Apply"].BindButton(() => OnApplyButton());

            CreateLabel("AutosaveLabel", "Auto Save Interval:");
            elements["AutosaveLabel"].SetRectAnchors(0.025f, 0.71f, 0.5f, 0.85f);

            CreateLabel("BackupCountLabel", "Backup Count:");
            elements["BackupCountLabel"].SetRectAnchors(0.025f, 0.57f, 0.5f, 0.71f);

            CreateLabel("KeepBackupLabel", "Keep Backup With No Editors:");
            elements["KeepBackupLabel"].SetRectAnchors(0.025f, 0.43f, 0.5f, 0.57f);

            CreateLabel("LevelNameLabel", "Level Name:");
            elements["LevelNameLabel"].SetRectAnchors(0.025f, 0.29f, 0.5f, 0.43f);

            CreateLabel("LoadBackupStartLabel", "Load Backup On Start:");
            elements["LoadBackupStartLabel"].SetRectAnchors(0.025f, 0.15f, 0.5f, 0.29f);

            CreateTextInput("AutoSaveIntervalInput");
            elements["AutoSaveIntervalInput"].SetRectAnchors(0.5f, 0.72f, 0.975f, 0.84f);

            CreateTextInput("BackupCountInput");
            elements["BackupCountInput"].SetRectAnchors(0.5f, 0.58f, 0.975f, 0.70f);

            CreateTextButton("KeepBackupButton", "");
            elements["KeepBackupButton"].SetRectAnchors(0.5f, 0.44f, 0.975f, 0.56f);
            elements["KeepBackupButton"].BindButton(() =>
            {
                KeepBackupWithNoEditors = !KeepBackupWithNoEditors;

                elements["KeepBackupButton"].SetButtonText(KeepBackupWithNoEditors ? "Yes" : "No");
                InterfaceManager.RecolorButton(elements["KeepBackupButton"].Button, KeepBackupWithNoEditors ? Color.green : Color.red, Color.black, Color.black, true);
            });

            CreateTextInput("LevelNameInput");
            elements["LevelNameInput"].SetRectAnchors(0.5f, 0.30f, 0.975f, 0.42f);

            CreateTextButton("LoadBackupStartButton", "");
            elements["LoadBackupStartButton"].SetRectAnchors(0.5f, 0.16f, 0.975f, 0.28f);
            elements["LoadBackupStartButton"].BindButton(() =>
            {
                LoadBackupOnStart = !LoadBackupOnStart;

                elements["LoadBackupStartButton"].SetButtonText(LoadBackupOnStart ? "Yes" : "No");
                InterfaceManager.RecolorButton(elements["LoadBackupStartButton"].Button, LoadBackupOnStart ? Color.green : Color.red, Color.black, Color.black, true);
            });

            UpdateElements();
        }

        public void UpdateElements()
        {
            elements["AutoSaveIntervalInput"].SetText(AutoSaveInterval.ToString());
            elements["BackupCountInput"].SetText(BackupCount.ToString());

            elements["KeepBackupButton"].SetButtonText(KeepBackupWithNoEditors ? "Yes" : "No");
            InterfaceManager.RecolorButton(elements["KeepBackupButton"].Button, KeepBackupWithNoEditors ? Color.green : Color.red, Color.black, Color.black, true);

            elements["LevelNameInput"].SetText(LevelName);
            
            elements["LoadBackupStartButton"].SetButtonText(LoadBackupOnStart ? "Yes" : "No");
            InterfaceManager.RecolorButton(elements["LoadBackupStartButton"].Button, LoadBackupOnStart ? Color.green : Color.red, Color.black, Color.black, true);
        }

        private void OnReloadButton()
        {
            Debug.Log("Reload");

            Plugin.Instance.client.SendSaveConfigurationRequestPacket();
        }

        private void OnApplyButton()
        {
            Debug.Log("Apply");

            int autoSaveValue;
            if(!int.TryParse(elements["AutoSaveIntervalInput"].GetText(), out autoSaveValue))
            {
                PlayerManager.Instance.messenger.Log("Input invalid: autosave", 1f);
                return;
            }

            if(autoSaveValue < 60)
            {
                autoSaveValue = 60;
            }

            int backupCountValue;
            if(!int.TryParse(elements["BackupCountInput"].GetText(), out backupCountValue))
            {
                PlayerManager.Instance.messenger.Log("Input invalid: backup count", 1f);
                return;
            }

            if(backupCountValue < 0)
            {
                backupCountValue = 1;
            }

            string levelNameValue = elements["LevelNameInput"].GetText();
            if(string.IsNullOrEmpty(levelNameValue))
            {
                PlayerManager.Instance.messenger.Log("Input invalid: levelname", 1f);
                return;
            }

            AutoSaveInterval = autoSaveValue;
            BackupCount = backupCountValue;
            LevelName = levelNameValue;

            UpdateElements();

            Plugin.Instance.client.SendSaveConfigurationSubmitPacket(AutoSaveInterval, BackupCount, KeepBackupWithNoEditors, LevelName, LoadBackupOnStart);

            Close();
        }

        public void UpdateValues(int autosaveInterval, int backupCount, bool keepBackupWithNoEditors, string levelName, bool loadBackupOnStart)
        {
            AutoSaveInterval = autosaveInterval;
            BackupCount = backupCount;
            KeepBackupWithNoEditors = keepBackupWithNoEditors;
            LevelName = levelName;
            LoadBackupOnStart = loadBackupOnStart;

            UpdateElements();
        }    
    }
}
