using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamXClient
{
    public class TeamXLevelManagerPanel : TeamXPanel
    {
        private string selectedLevelPath = "";
        private Dictionary<string, List<string>> projectFolders;
        private List<TeamXPanelComponent> currentButtons = new List<TeamXPanelComponent>();

        public void ImportDirectories(List<string> localPaths)
        {
            projectFolders = Utils.GroupTeamKistFilesByProject(localPaths);

            CreateProjectList();
        }

        private void EmptyList()
        {
            //Destroy the current buttons
            foreach (TeamXPanelComponent c in currentButtons)
            {
                if (c.Rect != null)
                {
                    GameObject.Destroy(c.Rect.gameObject);
                }
            }

            currentButtons.Clear();
        }

        private void CreateProjectList()
        {
            EmptyList();

            elements["BackButton"].Disable();
            elements["Selected"].SetText("");

            int counter = 0;
            foreach(string project in projectFolders.Keys)
            {
                TeamXPanelComponent c = CreateTextButton("ProjectListItem" + counter, project, false);
                currentButtons.Add(c);
                c.BindButton(() => OnClickedProject(project));
                c.Rect.SetParent(elements["ScrollView"].ScrollRect.content);
                counter++;
            }
        }

        private void OnClickedProject(string project)
        {
            EmptyList();

            elements["BackButton"].Enable();

            int counter = 0;
            foreach (string f in projectFolders[project])
            {
                TeamXPanelComponent c = CreateTextButton("ProjectListItem" + counter, f, false);
                currentButtons.Add(c);
                c.BindButton(() => OnClickedProjectFile(project, f));
                c.Rect.SetParent(elements["ScrollView"].ScrollRect.content);
                counter++;
            }
        }

        private void OnClickedProjectFile(string project, string fileName)
        {
            selectedLevelPath = project + "/ServerSaves/" + fileName;
            Plugin.Instance.Log(selectedLevelPath, LogType.Debug);
            elements["Selected"].SetText(fileName);
        }

        public override void OnInitialize(string title)
        {
            SetBackgroundColor(InterfaceManager.darkGreen);
            SetTitle(title);

            CreateTextButton("Reload", "Reload");
            elements["Reload"].SetRectAnchors(0.68f, 0.88f, 0.89f, 0.975f);
            elements["Reload"].BindButton(() => OnReloadButton());

            elements["ScrollView"].Enable();
            elements["ScrollView"].SetRectAnchors(0.025f, 0.15f, 0.975f, 0.85f);
            elements["ScrollView"].SetGridLayoutColumns(2, 0.1f);

            CreateTextButton("Apply", "Send to Server");
            elements["Apply"].SetRectAnchors(0.7f, 0.025f, 0.975f, 0.125f);
            elements["Apply"].BindButton(() => OnApplyButton());

            CreateTextButton("SaveNow", "Save Current Map");
            elements["SaveNow"].SetRectAnchors(0.025f, 0.025f, 0.3f, 0.125f);
            elements["SaveNow"].BindButton(() => OnSaveNowButton());

            CreateLabel("Selected", "");
            elements["Selected"].SetRectAnchors(0.325f, 0.025f, 0.675f, 0.125f);

            CreateTextButton("BackButton", "< Levels");
            elements["BackButton"].SetRectAnchors(0.5f, 0.88f, 0.6f, 0.975f);
            elements["BackButton"].BindButton(() => OnBackButton());
            elements["BackButton"].Disable();

            CreateTextButton("BackToMain", "<<");
            elements["BackToMain"].SetRectAnchors(0.025f, 0.88f, 0.05f, 0.975f);
            elements["BackToMain"].BindButton(() =>
            {
                InterfaceManager.mainPanel.Open(false);
                Close(false);
            });
        }

        private void OnBackButton()
        {
            CreateProjectList();
        }

        private void OnReloadButton()
        {
            EmptyList();
            selectedLevelPath = "";
            elements["Selected"].SetText("");
            elements["BackButton"].Disable();
            Plugin.Instance.client.SendLevelDirectoryRequestPacket();
        }

        private void OnApplyButton()
        {
            if(string.IsNullOrEmpty(selectedLevelPath))
            {
                PlayerManager.Instance.messenger.Log("No save selected", 1f);
                return;
            }

            Plugin.Instance.client.SendLoadLevelRequestPacket(selectedLevelPath);
            Close();
        }

        private void OnSaveNowButton()
        {
            PlayerManager.Instance.messenger.Log("Send save request...", 2f);
            Plugin.Instance.client.SendSaveCurrentState();
        }
    }
}     