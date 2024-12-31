using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamXClient
{
    public class TeamXMainPanel : TeamXPanel
    {
        public override void OnInitialize(string title)
        {
            SetBackgroundColor(InterfaceManager.darkGreen);
            SetTitle(title);

            CreateTextButton("Permissions", "Permissions");
            CreateTextButton("LevelManagement", "Level Management");
            CreateTextButton("SaveConfig", "Save Configuration");

            elements["Permissions"].SetRectAnchors(0.3f, 0.71f, 0.7f, 0.8f);
            elements["LevelManagement"].SetRectAnchors(0.3f, 0.61f, 0.7f, 0.7f);
            elements["SaveConfig"].SetRectAnchors(0.3f, 0.51f, 0.7f, 0.6f);

            elements["Permissions"].BindButton(() =>
            {
                InterfaceManager.permissionPanel.Open(false);
                Close(false);
            });

            elements["LevelManagement"].BindButton(() =>
            {
                InterfaceManager.levelManagerPanel.Open(false);
                Close(false);
            });

            elements["SaveConfig"].BindButton(() =>
            {
                InterfaceManager.saveConfigurationPanel.Open(false);
                Close(false);
            });
        }
    }
}
