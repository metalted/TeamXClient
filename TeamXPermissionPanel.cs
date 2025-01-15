using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamXClient
{   
    public class TeamXPermissionPanel : TeamXPanel
    {     
        //Holds the rows of 'checkbox' buttons for each player.
        public List<TeamXPermissionPanelEntry> windowEntries = new List<TeamXPermissionPanelEntry>();
        
        public override void OnInitialize(string title)
        {
            SetBackgroundColor(InterfaceManager.darkGreen);
            SetTitle(title);

            CreateTextButton("Reload", "Reload");
            elements["Reload"].SetRectAnchors(0.68f, 0.88f, 0.89f, 0.975f);
            elements["Reload"].BindButton(() => OnReloadButton());

            CreateTextButton("Apply", "Send to Server");
            elements["Apply"].SetRectAnchors(0.7f, 0.025f, 0.975f, 0.125f);
            elements["Apply"].BindButton(() => OnApplyButton());

            elements["ScrollView"].Enable();
            elements["ScrollView"].SetRectAnchors(0.025f, 0.15f, 0.975f, 0.85f);
            elements["ScrollView"].SetGridLayoutColumns(6, 0.25f);

            CreateTextButton("BackToMain", "<<");
            elements["BackToMain"].SetRectAnchors(0.025f, 0.88f, 0.05f, 0.975f);
            elements["BackToMain"].BindButton(() =>
            {
                InterfaceManager.mainPanel.Open(false);
                Close(false);
            });
        }

        private void OnReloadButton() 
        {
            EmptyWindowEntries();
            Plugin.Instance.client.SendPermissionTableRequest();
        }

        private void OnApplyButton()
        {
            Plugin.Instance.client.SendPermissionTableSubmit(windowEntries);
            Close();
        }

        private void EmptyWindowEntries()
        {
            foreach (TeamXPermissionPanelEntry pwe in windowEntries)
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
                TeamXPermissionPanelEntry wEntry = new TeamXPermissionPanelEntry();
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
            foreach (TeamXPermissionPanelEntry pwe in windowEntries)
            {
                TeamXPanelComponent user = CreateTextButton("User", pwe.user, false);
                user.Rect.SetParent(elements["ScrollView"].ScrollRect.content);
                InterfaceManager.RecolorButton(user.Button, Color.black, Color.black, Color.black, true);

                TeamXPanelComponent banned = CreateTextButton("Banned", "Banned", false);
                banned.Rect.SetParent(elements["ScrollView"].ScrollRect.content);
                banned.BindButton(pwe.SetBanned);

                TeamXPanelComponent guest = CreateTextButton("Guest", "Guest", false);
                guest.Rect.SetParent(elements["ScrollView"].ScrollRect.content);
                guest.BindButton(pwe.SetGuest);

                TeamXPanelComponent dfault = CreateTextButton("Default", "Default", false);
                dfault.Rect.SetParent(elements["ScrollView"].ScrollRect.content);
                dfault.BindButton(pwe.SetDefault);

                TeamXPanelComponent trusted = CreateTextButton("Trusted", "Trusted", false);
                trusted.Rect.SetParent(elements["ScrollView"].ScrollRect.content);
                trusted.BindButton(pwe.SetTrusted);

                TeamXPanelComponent admin = CreateTextButton("Admin", "Admin", false);
                admin.Rect.SetParent(elements["ScrollView"].ScrollRect.content);
                admin.BindButton(pwe.SetAdmin);

                pwe.entryComponents.Add("User", user);
                pwe.entryComponents.Add("Banned", banned);
                pwe.entryComponents.Add("Guest", guest);
                pwe.entryComponents.Add("Default", dfault);
                pwe.entryComponents.Add("Trusted", trusted);
                pwe.entryComponents.Add("Admin", admin);

                pwe.ColorCurrentState();               
            }
        }        
    }
}
