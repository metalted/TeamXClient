using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using UnityEngine;

namespace TeamX
{
    public class Client
    {
        private NetPeerConfiguration netPeerConfiguration;
        private NetClient client;

        public ConnectionStatus connectionStatus { get; private set; }
        public PermissionLevel permissionLevel;
        public ulong ClientSteamID;
        
        public Client(ulong steamID)
        {
            ClientSteamID = steamID;
            netPeerConfiguration = new NetPeerConfiguration("TeamX");
            netPeerConfiguration.ConnectionTimeout = 5000;
            client = new NetClient(netPeerConfiguration);
            client.Start();
        }

        public bool Connect(string ip, int port)
        {
            if(client == null)
            {
                return false;
            }

            if (connectionStatus == ConnectionStatus.Connecting || connectionStatus == ConnectionStatus.Connected)
            {
                return false;
            }

            try
            {
                connectionStatus = ConnectionStatus.Connecting;
                client.Connect(ip, port);
                return true;
            }
            catch
            {
                return false;
            }
        }       

        public bool Disconnect()
        {
            if (client == null)
            {
                return false;
            }

            client.Disconnect("");
            connectionStatus = ConnectionStatus.Disconnecting;
            return true;
        }

        private bool ShouldReadMessages()
        {
            return connectionStatus == ConnectionStatus.Connecting || connectionStatus == ConnectionStatus.Connected;
        }

        public void Run()
        {
            if(ShouldReadMessages())
            {
                ReadMessages();
            }
        }

        private void ReadMessages()
        {
            if(client == null)
            {
                return;
            }

            NetIncomingMessage im;

            while((im = client.ReadMessage()) != null)
            {
                switch(im.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        switch (im.SenderConnection.Status)
                        {
                            case NetConnectionStatus.Connected:
                                connectionStatus = ConnectionStatus.Connected;
                                Plugin.Instance.Log("ConnectionStatus: Connected!");
                                HandleHandshakeRequest();
                                break;
                            case NetConnectionStatus.Disconnected:
                                connectionStatus = ConnectionStatus.NotConnected;
                                Plugin.Instance.Log("ConnectionStatus: Not Connected!");
                                break;
                        }
                        break;
                    case NetIncomingMessageType.Data:
                        if (PacketUtility.Unpack(im, out ushort packetId))
                        {
                            Type packetType = PacketUtility.GetPacketType(packetId);

                            if (packetType != null)
                            {
                                var packet = (IPacket)Activator.CreateInstance(packetType);
                                packet.Deserialize(im);
                                Plugin.Instance.Log($"Received packet of type: {packetType.Name}");
                                HandlePacket(packet);
                            }
                            else
                            {
                                Plugin.Instance.Log($"Unknown packet ID: {packetId}");
                            }
                        }
                        else
                        {
                            Plugin.Instance.Log("Failed to unpack the message.");
                        }
                        break;
                }
            }
        }

        public void HandlePacket(IPacket packet)
        {
            switch(packet)
            {
                case HandshakeRequestPacket handshakeRequestPacket:
                    //HandleHandshakeRequest(handshakeRequestPacket);
                    break;
                case AccessDeniedPacket accessDeniedPacket:
                    HandleAccessDenied(accessDeniedPacket);
                    break;
                case AccessGrantedPacket accessGrantedPacket:
                    HandleAccessGranted(accessGrantedPacket);
                    break;
                case PlayerJoinPacket playerJoinPacket:
                    HandlePlayerJoin(playerJoinPacket);
                    break;
                case PlayerLeftPacket playerLeftPacket:
                    HandlePlayerLeft(playerLeftPacket);
                    break;
                case PlayerStatePacket playerStatePacket:
                    HandlePlayerState(playerStatePacket);
                    break;
                case EditorStateResponsePacket editorStateResponse:
                    HandleEditorState(editorStateResponse);
                    break;
                case EditorBlockCreatePacket editorBlockCreatePacket:
                    HandleEditorBlockCreate(editorBlockCreatePacket);
                    break;
                case EditorBlockUpdatePacket editorBlockUpdatePacket:
                    HandleEditorBlockUpdate(editorBlockUpdatePacket);
                    break;
                case EditorBlockDestroyPacket editorBlockDestroyPacket:
                    HandleEditorBlockDestroy(editorBlockDestroyPacket);
                    break;
                case EditorFloorPacket editorFloorPacket:
                    HandleEditorFloor(editorFloorPacket);
                    break;
                case EditorSkyboxPacket editorSkyboxPacket:
                    HandleEditorSkybox(editorSkyboxPacket);
                    break;
                case EditorBlockCreateDeniedPacket editorBlockCreateDeniedPacket:
                    HandlerEditorBlockCreateDenied(editorBlockCreateDeniedPacket);
                    break;
                case EditorBlockUpdateDeniedPacket editorBlockUpdateDeniedPacket:
                    HandleEditorBlockUpdateDenied(editorBlockUpdateDeniedPacket);
                    break;
                case EditorBlockDestroyDeniedPacket editorBlockDestroyDeniedPacket:
                    HandleEditorBlockDestroyDenied(editorBlockDestroyDeniedPacket);
                    break;
                case EditorFloorDeniedPacket editorFloorDeniedPacket:
                    HandleEditorFloorDenied(editorFloorDeniedPacket);
                    break;
                case EditorSkyboxDeniedPacket editorSkyboxDeniedPacket:
                    HandleEditorSkyboxDenied(editorSkyboxDeniedPacket);
                    break;
                case EditorSelectionDeniedPacket editorSelectionDeniedPacket:
                    HandleSelectionDenied(editorSelectionDeniedPacket);
                    break;
            }
        }

        public void HandleHandshakeRequest()
        {
            HandshakeResponsePacket handshakeResponse = new HandshakeResponsePacket()
            {
                SteamID = ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(handshakeResponse, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);

            Plugin.Instance.game.gameState = GameManager.GameState.WaitingForAccess;
        }

        public void HandleAccessDenied(AccessDeniedPacket accessDenied)
        {
            Plugin.Instance.game.gameState = GameManager.GameState.MainMenu;
            Plugin.Instance.client.connectionStatus = ConnectionStatus.Disconnecting;
            Plugin.Instance.client.Disconnect();
        }

        public void HandleAccessGranted(AccessGrantedPacket accessGranted)
        {
            PlayerData localPlayer = Utils.GetLocalPlayerData();
            PlayerJoinPacket playerJoin = new PlayerJoinPacket()
            {
                Color = localPlayer.color,
                Color_body = localPlayer.color_body,
                Color_leftArm = localPlayer.color_leftArm,
                Color_leftLeg = localPlayer.color_leftLeg,
                Color_rightArm = localPlayer.color_rightArm,
                Color_rightLeg = localPlayer.color_rightLeg,
                FrontWheels = localPlayer.frontWheels,
                Glasses = localPlayer.glasses,
                Hat = localPlayer.hat,
                Horn = localPlayer.horn,
                Name = localPlayer.name,
                Paraglider = localPlayer.paraglider,
                RearWheels = localPlayer.rearWheels,
                SteamID = localPlayer.steamID,
                Zeepkist = localPlayer.zeepkist
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(playerJoin, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);

            EditorStateRequestPacket editorRequest = new EditorStateRequestPacket()
            {
                SteamID = localPlayer.steamID
            };

            var outgoingMessage2 = client.CreateMessage();
            PacketUtility.Pack(editorRequest, outgoingMessage2);
            client.SendMessage(outgoingMessage2, NetDeliveryMethod.ReliableOrdered, 0);

            permissionLevel = (PermissionLevel) accessGranted.Level;

            Plugin.Instance.game.gameState = GameManager.GameState.WaitingOnEditorDataInMainMenu;
        }
    
        public void HandlePlayerJoin(PlayerJoinPacket playerJoinPacket)
        {
            //Turn the player joined packet into player data
            PlayerData playerData = new PlayerData()
            {
                color = playerJoinPacket.Color,
                color_body = playerJoinPacket.Color_body,
                color_leftArm = playerJoinPacket.Color_leftArm,
                color_leftLeg = playerJoinPacket.Color_leftLeg,
                color_rightArm = playerJoinPacket.Color_rightArm,
                color_rightLeg = playerJoinPacket.Color_leftLeg,
                frontWheels = playerJoinPacket.FrontWheels,
                glasses = playerJoinPacket.Glasses,
                hat = playerJoinPacket.Hat,
                horn = playerJoinPacket.Horn,
                name = playerJoinPacket.Name,
                paraglider = playerJoinPacket.Paraglider,
                rearWheels = playerJoinPacket.RearWheels,
                state = 0,
                steamID = playerJoinPacket.SteamID
            };

            Plugin.Instance.multiplayer.AddPlayer(playerData);
        }

        public void HandlePlayerLeft(PlayerLeftPacket playerLeft)
        {
            Plugin.Instance.multiplayer.RemovePlayer(playerLeft.SteamID);
        }
    
        public void HandlePlayerState(PlayerStatePacket playerState)
        {
            PlayerStateData stateData = new PlayerStateData()
            {
                SteamID = playerState.SteamID,
                Position = new Vector3(playerState.PositionX, playerState.PositionY, playerState.PositionZ),
                Rotation = new Vector3(playerState.EulerX, playerState.EulerY, playerState.EulerZ),
                Mode = playerState.Mode
            };

            Plugin.Instance.multiplayer.UpdatePlayerState(stateData);
        }
    
        public void HandleEditorState(EditorStateResponsePacket editorState)
        {
            EditorStateData state = new EditorStateData()
            {
                floor = editorState.Floor,
                skybox = editorState.Skybox,
                blocks = editorState.BlockStrings               
            };

            Plugin.Instance.editor.SetState(state);

            if(Plugin.Instance.game.gameState == GameManager.GameState.WaitingOnEditorDataInMainMenu)
            {
                //Data received, load into the editor.
                Plugin.Instance.game.gameState = GameManager.GameState.EnteringTeamXFromMainMenu;
                Plugin.Instance.game.LoadIntoEditorX();
            }
        }
    
        public void HandleEditorBlockCreate(EditorBlockCreatePacket editorBlockCreate)
        {
            Block packetBlock = Plugin.Instance.editor.JSONToBlock(editorBlockCreate.BlockString);

            //Update the editor
            Plugin.Instance.editor.Add(packetBlock);

            if(Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.CreateBlock(packetBlock);
            }
        }    
    
        public void HandleEditorBlockUpdate(EditorBlockUpdatePacket editorBlockUpdate)
        {
            Block packetBlock = Plugin.Instance.editor.JSONToBlock(editorBlockUpdate.BlockString);

            Plugin.Instance.editor.Update(packetBlock);

            if (Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.UpdateBlock(Utils.BlockToBlockPropertyJSON(packetBlock));
            }
        }

        public void HandleEditorBlockDestroy(EditorBlockDestroyPacket editorBlockDestroy)
        {
            Plugin.Instance.editor.Remove(editorBlockDestroy.UID);

            if(Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.DestroyBlock(editorBlockDestroy.UID);
            }
        }

        public void HandleEditorSkybox(EditorSkyboxPacket editorSkybox)
        {
            Plugin.Instance.editor.SetSkybox(editorSkybox.Skybox);

            if(Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.UpdateSkybox(editorSkybox.Skybox);
            }
        }

        public void HandleEditorFloor(EditorFloorPacket editorFloor)
        {
            Plugin.Instance.editor.SetFloor(editorFloor.Floor);

            if(Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.UpdateFloor(editorFloor.Floor);
            }
        }    
    
        public void HandlerEditorBlockCreateDenied(EditorBlockCreateDeniedPacket createDenied)
        {
            Plugin.Instance.editor.Remove(createDenied.UID);
            Plugin.Instance.editor.Modifier.DestroyBlock(createDenied.UID);
        }

        public void HandleEditorBlockUpdateDenied(EditorBlockUpdateDeniedPacket updateDenied)
        {
            Block packetBlock = Plugin.Instance.editor.JSONToBlock(updateDenied.BlockString);
            Plugin.Instance.editor.Update(packetBlock);

            BlockPropertyJSON blockJSON = Utils.BlockToBlockPropertyJSON(packetBlock);
            Plugin.Instance.editor.Modifier.UpdateBlock(blockJSON);
        }

        public void HandleEditorBlockDestroyDenied(EditorBlockDestroyDeniedPacket destroyDenied)
        {
            Block packetBlock = Plugin.Instance.editor.JSONToBlock(destroyDenied.BlockString);
            Plugin.Instance.editor.Add(packetBlock);
            Plugin.Instance.editor.Modifier.CreateBlock(packetBlock);
        }

        public void HandleEditorFloorDenied(EditorFloorDeniedPacket floorDenied)
        {
            Plugin.Instance.editor.SetFloor(floorDenied.Floor);
        }

        public void HandleEditorSkyboxDenied(EditorSkyboxDeniedPacket skyboxDenied)
        {
            Plugin.Instance.editor.SetSkybox(skyboxDenied.Skybox);
        }

        public void HandleSelectionDenied(EditorSelectionDeniedPacket selectionDenied)
        {
            Plugin.Instance.editor.selectionObserver.DeselectBlock(selectionDenied.UID);
        }

        public void SendBlockCreate(Block block)
        {
            EditorBlockCreatePacket blockCreate = new EditorBlockCreatePacket()
            {
                BlockString = Plugin.Instance.editor.BlockToJSON(block),
                SteamID = Plugin.Instance.client.ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(blockCreate, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void SendBlockUpdate(Block block)
        {
            EditorBlockUpdatePacket blockUpdate = new EditorBlockUpdatePacket()
            {
                BlockString = Plugin.Instance.editor.BlockToJSON(block),
                SteamID = Plugin.Instance.client.ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(blockUpdate, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void SendBlockDestroy(string uid)
        {
            EditorBlockDestroyPacket blockDestroy = new EditorBlockDestroyPacket()
            {
                UID = uid,
                SteamID = Plugin.Instance.client.ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(blockDestroy, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void SendFloorUpdate(int floor)
        {
            EditorFloorPacket floorPacket = new EditorFloorPacket()
            {
                Floor = floor,
                SteamID = Plugin.Instance.client.ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(floorPacket, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void SendSkyboxUpdate(int skybox)
        {
            EditorSkyboxPacket skyboxPacket = new EditorSkyboxPacket()
            {
                Skybox = skybox,
                SteamID = Plugin.Instance.client.ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(skyboxPacket, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }
    
        public void SendPlayerState(PlayerStateData stateData)
        {
            PlayerStatePacket playerState = new PlayerStatePacket()
            {
                SteamID = ClientSteamID,
                PositionX = stateData.Position.x,
                PositionY = stateData.Position.y,
                PositionZ = stateData.Position.z,
                EulerX = stateData.Rotation.x,
                EulerY = stateData.Rotation.y,
                EulerZ = stateData.Rotation.z,
                Mode = stateData.Mode
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(playerState, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void SendSelection(string uid)
        {
            EditorSelectionPacket editorSelection = new EditorSelectionPacket()
            {
                SteamID = ClientSteamID,
                UID = uid
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(editorSelection, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }
        
        public void SendDeselection(string uid)
        {
            EditorDeselectionPacket editorDeselection = new EditorDeselectionPacket()
            {
                SteamID = ClientSteamID,
                UID = uid
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(editorDeselection, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }
    }
}
