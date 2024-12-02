using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamX
{
    public class PlayerObserver : MonoBehaviour
    {
        private float updateInterval = 0.15f;
        private float timer = 0f;

        private Vector3 position = Vector3.zero;
        private Vector3 euler = Vector3.zero;
        private Vector3 lastPosition = Vector3.zero;
        private Vector3 lastEuler = Vector3.zero;       

        public void Update()
        {
            position = transform.position;
            euler = transform.eulerAngles;

            timer += Time.deltaTime;

            if(timer >= updateInterval)
            {
                if(position != lastPosition || euler != lastEuler)
                {
                    Plugin.Instance.game.OnLocalTransformChange(new PlayerStateData() { SteamID = Plugin.Instance.client.ClientSteamID, Position = position, Rotation = euler, Mode = (byte) Plugin.Instance.multiplayer.LocalPlayerMode });
                }

                lastPosition = position;
                lastEuler = euler;

                timer = 0f;
            }
        }
    }
}
