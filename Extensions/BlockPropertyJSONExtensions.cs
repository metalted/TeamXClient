using System;
using System.Linq;

namespace TeamXClient.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="BlockPropertyJSON"/> class.
    /// </summary>
    public static class BlockPropertyJSONExtensions
    {
        /// <summary>
        /// Copies properties from one <see cref="BlockPropertyJSON"/> instance to another.
        /// </summary>
        /// <param name="from">The source instance to copy properties from.</param>
        /// <param name="to">The target instance to copy properties to.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="from"/> or <paramref name="to"/> is null.</exception>
        public static void CopyTo(this BlockPropertyJSON from, BlockPropertyJSON to)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from), "Source BlockPropertyJSON cannot be null.");
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to), "Target BlockPropertyJSON cannot be null.");
            }

            to.blockID = from.blockID;
            to.position = from.position;
            to.eulerAngles = from.eulerAngles;
            to.localScale = from.localScale;
            to.properties = from.properties.ToList();
            to.UID = from.UID;
        }
    }
}
