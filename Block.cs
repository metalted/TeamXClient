using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamX
{
    /// <summary>
    /// Represents a block in the editor or game world, containing properties for position,
    /// rotation, scale, and additional metadata.
    /// </summary>
    public class Block
    {
        /// <summary>
        /// Gets or sets the X-coordinate of the block's position in world space.
        /// </summary>
        public float PositionX { get; set; }

        /// <summary>
        /// Gets or sets the Y-coordinate of the block's position in world space.
        /// </summary>
        public float PositionY { get; set; }

        /// <summary>
        /// Gets or sets the Z-coordinate of the block's position in world space.
        /// </summary>
        public float PositionZ { get; set; }

        /// <summary>
        /// Gets or sets the X-axis rotation of the block in Euler angles.
        /// </summary>
        public float EulerAnglesX { get; set; }

        /// <summary>
        /// Gets or sets the Y-axis rotation of the block in Euler angles.
        /// </summary>
        public float EulerAnglesY { get; set; }

        /// <summary>
        /// Gets or sets the Z-axis rotation of the block in Euler angles.
        /// </summary>
        public float EulerAnglesZ { get; set; }

        /// <summary>
        /// Gets or sets the local scale of the block along the X-axis.
        /// </summary>
        public float LocalScaleX { get; set; }

        /// <summary>
        /// Gets or sets the local scale of the block along the Y-axis.
        /// </summary>
        public float LocalScaleY { get; set; }

        /// <summary>
        /// Gets or sets the local scale of the block along the Z-axis.
        /// </summary>
        public float LocalScaleZ { get; set; }

        /// <summary>
        /// Gets or sets a list of additional properties for the block, stored as float values.
        /// </summary>
        /// <remarks>
        /// This list can include custom properties specific to the block, such as material,
        /// behavior settings, or other configurable attributes.
        /// </remarks>
        public List<float> Properties { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the block's type.
        /// </summary>
        /// <remarks>
        /// This ID corresponds to the block's predefined type in the game or editor.
        /// </remarks>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier (UID) for the block instance.
        /// </summary>
        /// <remarks>
        /// The UID is unique to each block instance and is used for distinguishing between blocks
        /// of the same type in the editor or game world.
        /// </remarks>
        public string UID { get; set; }

        /// <summary>
        /// Gets or sets the Steam ID of the player associated with the block.
        /// </summary>
        /// <remarks>
        /// This is typically used in multiplayer scenarios to identify which player
        /// owns or created the block.
        /// </remarks>
        public ulong SteamID { get; set; }
    }
}
