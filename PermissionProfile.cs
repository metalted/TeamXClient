using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamXNetwork;

namespace TeamXClient
{
    /// <summary>
    /// A class that contains the local permissions.
    /// </summary>
    public class PermissionProfile
    {
        public bool IsAdministrator = false;
        public bool CanJoin = false;
        public bool CanCreate = false;
        public bool CanEdit = false;
        public bool CanEditAll = false;
        public bool CanEditFloor = false;
        public bool CanEditSkybox = false;
        public bool CanDestroy = false;
        public int BlockLimit = 0;
        public List<int> BannedBlocks = new List<int>();

        public PermissionProfile() { }

        public PermissionProfile(ServerRulesResponsePacket serverRules)
        {
            IsAdministrator = serverRules.IsAdministrator;
            CanJoin = serverRules.CanJoin;
            CanCreate = serverRules.CanCreate;
            CanEdit = serverRules.CanEdit;
            CanEditAll = serverRules.CanEditAll;
            CanEditFloor = serverRules.CanEditFloor;
            CanEditSkybox = serverRules.CanEditSkybox;
            CanDestroy = serverRules.CanDestroy;
            BlockLimit = serverRules.BlockLimit;
            BannedBlocks = serverRules.BannedBlocks;
        }
    }
}
