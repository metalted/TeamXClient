using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamXClient
{
    public class TeamXPermissionPanelEntry
    {
        public Dictionary<string, TeamXPanelComponent> entryComponents;

        public string user;
        public ulong steamID;
        public bool banned;
        public bool guest;
        public bool dfault;
        public bool trusted;
        public bool admin;

        public TeamXPermissionPanelEntry()
        {
            entryComponents = new Dictionary<string, TeamXPanelComponent>();
        }

        public void DestroyComponents()
        {
            foreach (KeyValuePair<string, TeamXPanelComponent> kvp in entryComponents)
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
            InterfaceManager.RecolorButton(entryComponents["Banned"].Button, banned ? Color.green : InterfaceManager.darkgrey, Color.black, Color.black, true);
            InterfaceManager.RecolorButton(entryComponents["Guest"].Button, guest ? Color.green : InterfaceManager.darkgrey, Color.black, Color.black, true);
            InterfaceManager.RecolorButton(entryComponents["Default"].Button, dfault ? Color.green : InterfaceManager.darkgrey, Color.black, Color.black, true);
            InterfaceManager.RecolorButton(entryComponents["Trusted"].Button, trusted ? Color.green : InterfaceManager.darkgrey, Color.black, Color.black, true);
            InterfaceManager.RecolorButton(entryComponents["Admin"].Button, admin ? Color.green : InterfaceManager.darkgrey, Color.black, Color.black, true);
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
}
