using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamXNetwork;

namespace TeamXClient
{
    /// <summary>
    /// A simple class that handles permissions for the local player.
    /// </summary>
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

        public bool IsAdmin()
        {
            return LocalProfile.IsAdministrator;
        }
    }
}
