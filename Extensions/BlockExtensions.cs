using Newtonsoft.Json;
using System;
using UnityEngine;

namespace TeamXClient.Extensions
{
    /// <summary>
    /// Provides JSON serialization and deserialization methods for the <see cref="Block"/> class.
    /// </summary>
    public static class BlockExtensions
    {
        /// <summary>
        /// Serializes the <see cref="Block"/> instance to a JSON string.
        /// </summary>
        /// <param name="block">The block to serialize.</param>
        /// <returns>A JSON string representation of the block.</returns>
        public static string ToJson(this Block block)
        {
            return JsonConvert.SerializeObject(block);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="Block"/> instance.
        /// </summary>
        /// <param name="json">The JSON string representing the block.</param>
        /// <returns>The deserialized block.</returns>
        public static Block FromJson(this string json)
        {
            return JsonConvert.DeserializeObject<Block>(json);
        }

        /// <summary>
        /// Converts a <see cref="Block"/> object to a <see cref="BlockPropertyJSON"/>.
        /// </summary>
        /// <param name="block">The block to convert.</param>
        /// <returns>A JSON representation of the block.</returns>
        public static BlockPropertyJSON ToBlockPropertyJSON(this Block block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block), "Block cannot be null.");
            }

            return new BlockPropertyJSON
            {
                blockID = block.ID,
                eulerAngles = new Vector3(block.EulerAnglesX, block.EulerAnglesY, block.EulerAnglesZ),
                localScale = new Vector3(block.LocalScaleX, block.LocalScaleY, block.LocalScaleZ),
                position = new Vector3(block.PositionX, block.PositionY, block.PositionZ),
                properties = block.Properties,
                UID = block.UID
            };
        }
    }
}