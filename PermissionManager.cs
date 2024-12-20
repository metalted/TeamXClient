using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamXNetwork;

namespace TeamXClient
{
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

    public class PermissionManager
    {
        private PermissionProfile LocalProfile;

        public PermissionManager()
        {
            LocalProfile = new PermissionProfile();
        }

        public void SetLocalProfile(PermissionProfile profile)
        {
            LocalProfile = profile;
        }

        public bool CanJoin()
        {
            return LocalProfile.CanJoin;
        }

        public bool IsBanned()
        {
            return !LocalProfile.CanJoin;
        }

        public bool CanCreate()
        {
            return LocalProfile.CanCreate;
        }

        public bool CanEdit()
        {
            return LocalProfile.CanEdit;
        }

        public bool CanEditAll()
        {
            return LocalProfile.CanEditAll;
        }

        public bool CanEditFloor()
        {
            return LocalProfile.CanEditFloor;
        }

        public bool CanEditSkybox()
        {
            return LocalProfile.CanEditSkybox;
        }

        public bool CanDestroy()
        {
            return LocalProfile.CanDestroy;
        }

        public int GetBlockLimit()
        {
            return LocalProfile.BlockLimit;
        }

        public bool IsBlockBanned(int blockID)
        {
            if(LocalProfile.BannedBlocks.Contains(blockID))
            {
                return true;
            }

            return false;
        }

        public List<int> GetBannedBlocks()
        {
            return LocalProfile.BannedBlocks;
        }
    }
}
