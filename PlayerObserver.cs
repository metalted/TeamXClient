using System;
using UnityEngine;

namespace TeamXClient
{
    /// <summary>
    /// Observes the player's position and rotation in the scene and sends updates to the game if changes occur.
    /// </summary>
    public class PlayerObserver : MonoBehaviour
    {
        /// <summary>
        /// The interval, in seconds, at which player state updates are checked and sent.
        /// </summary>
        private const float UpdateInterval = 0.15f;

        /// <summary>
        /// Timer to track time elapsed since the last update.
        /// </summary>
        private float timer = 0f;

        /// <summary>
        /// The current position of the player.
        /// </summary>
        private Vector3 currentPosition = Vector3.zero;

        /// <summary>
        /// The current rotation of the player in Euler angles.
        /// </summary>
        private Vector3 currentEuler = Vector3.zero;

        /// <summary>
        /// The last recorded position of the player.
        /// </summary>
        private Vector3 lastPosition = Vector3.zero;

        /// <summary>
        /// The last recorded rotation of the player in Euler angles.
        /// </summary>
        private Vector3 lastEuler = Vector3.zero;

        /// <summary>
        /// Called every frame to check and update the player's position and rotation.
        /// </summary>
        private void Update()
        {
            // Get the current position and rotation of the player.
            currentPosition = transform.position;
            currentEuler = transform.eulerAngles;

            // Increment the timer.
            timer += Time.deltaTime;

            // Check if it's time to update the player's state.
            if (timer >= UpdateInterval)
            {
                // Send an update if the position or rotation has changed.
                if (HasTransformChanged())
                {
                    SendPlayerStateUpdate();
                    RecordLastTransform();
                }

                // Reset the timer.
                timer = 0f;
            }
        }

        /// <summary>
        /// Checks if the player's position or rotation has changed since the last update.
        /// </summary>
        /// <returns>True if the transform has changed; otherwise, false.</returns>
        private bool HasTransformChanged()
        {
            return currentPosition != lastPosition || currentEuler != lastEuler;
        }

        /// <summary>
        /// Sends the player's current state (position, rotation, and mode) to the game.
        /// </summary>
        private void SendPlayerStateUpdate()
        {
            var playerState = new PlayerStateData
            {
                SteamID = Plugin.Instance.client.ClientSteamID,
                Position = currentPosition,
                Rotation = currentEuler,
                Mode = (byte)Plugin.Instance.multiplayer.LocalPlayerMode
            };

            Plugin.Instance.game.OnLocalTransformChange(playerState);
        }

        /// <summary>
        /// Records the player's current position and rotation as the last known transform.
        /// </summary>
        private void RecordLastTransform()
        {
            lastPosition = currentPosition;
            lastEuler = currentEuler;
        }
    }
}