using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using TeamX.Extensions;
using UnityEngine;

namespace TeamX
{
    /// <summary>
    /// Represents a client in the network.
    /// </summary>
    /// <remarks>
    /// This class handles all network interactions.
    /// </remarks>
    public class Client
    {
        /// <summary>
        /// The configuration for the clients network connection.
        /// </summary>
        private NetPeerConfiguration netPeerConfiguration;
        /// <summary>
        /// The client interacting with the network.
        /// </summary>
        private NetClient client;
        /// <summary>
        /// The id of this app used in the client.
        /// </summary>
        private string appIdentifier = "TeamX";

        /// <summary>
        /// Gets the current connection status of the client.
        /// </summary>
        /// <value>The connection status.</value>
        public ConnectionStatus ConnectionStatus { get; private set; }
        /// <summary>
        /// Gets the current permission level for this client.
        /// </summary>
        /// <value>The permission level.</value>
        public PermissionLevel PermissionLevel { get; private set; }
        /// <summary>
        /// Gets the steam ID belonging to this client.
        /// </summary>
        /// <value>Steam ID of the client.</value>
        public ulong ClientSteamID { get; private set; }
        
        public Client(ulong steamID)
        {
            ClientSteamID = steamID;
            StartClient(appIdentifier);
        }

        /// <summary>
        /// Starts a new client for connecting to the network.
        /// </summary>
        /// <param name="appID">The applications identifier.</param>
        /// <remarks>The application identifier needs to match the application identifier on the server.</remarks>
        /// 
        private void StartClient(string appID)
        {
            netPeerConfiguration = new NetPeerConfiguration(appID);
            netPeerConfiguration.ConnectionTimeout = 5000;
            client = new NetClient(netPeerConfiguration);
            client.Start();
        }

        /// <summary>
        /// Connects the client to the server with the specified IP address and port.
        /// </summary>
        /// <param name="ip">The IP address of the server (e.g., "127.0.0.1").</param>
        /// <param name="port">The port of the server (e.g., 8080).</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the client is not initialized or if the client is already connecting or connected.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the provided IP address or port is invalid.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if an unexpected error occurs while connecting.
        /// </exception>
        public void Connect(string ip, int port)
        {
            // Validate the input parameters
            if (string.IsNullOrWhiteSpace(ip))
            {
                throw new ArgumentException("IP address cannot be null or empty.", nameof(ip));
            }

            if (port <= 0 || port > 65535)
            {
                throw new ArgumentException("Port must be between 1 and 65535.", nameof(port));
            }

            // Ensure the client is initialized
            if (client == null)
            {
                throw new InvalidOperationException("Client is not initialized.");
            }

            // Ensure the client is not already connecting or connected
            if (ConnectionStatus == ConnectionStatus.Connecting || ConnectionStatus == ConnectionStatus.Connected)
            {
                throw new InvalidOperationException("Client is already connecting or connected.");
            }

            try
            {
                ConnectionStatus = ConnectionStatus.Connecting;
                client.Connect(ip, port);
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                ConnectionStatus = ConnectionStatus.Disconnected;
                throw new InvalidOperationException("An unexpected error occurred while connecting.", ex);
            }
        }

        /// <summary>
        /// Disconnects the client from the server.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the client is not initialized.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the client is already disconnected.
        /// </exception>
        public void Disconnect()
        {
            if (client == null)
            {
                throw new InvalidOperationException("Client is not initialized.");
            }

            if (ConnectionStatus == ConnectionStatus.Disconnected)
            {
                throw new InvalidOperationException("Client is already disconnected.");
            }

            try
            {
                client.Disconnect("");
                ConnectionStatus = ConnectionStatus.Disconnecting;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while disconnecting the client.", ex);
            }
        }

        /// <summary>
        /// Processes incoming messages from the server if the client is connecting, connected or disconnecting.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the client is not initialized.
        /// </exception>
        public void ProcessIncomingMessages()
        {
            if (client == null)
            {
                throw new InvalidOperationException("Client is not initialized.");
            }

            if (ConnectionStatus != ConnectionStatus.Connecting && ConnectionStatus != ConnectionStatus.Connected && ConnectionStatus != ConnectionStatus.Disconnecting)
            {
                Plugin.Instance.Log("Cannot read messages: Client is not connected or (dis)connecting.");
                return;
            }

            NetIncomingMessage im;

            while ((im = client.ReadMessage()) != null)
            {
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        HandleStatusChanged(im);
                        break;

                    case NetIncomingMessageType.Data:
                        HandleDataMessage(im);
                        break;

                    default:
                        Plugin.Instance.Log($"Unhandled message type: {im.MessageType}");
                        break;
                }
            }
        }

        /// <summary>
        /// Handles status change messages from the server.
        /// </summary>
        /// <param name="message">The incoming message containing the status change.</param>
        private void HandleStatusChanged(NetIncomingMessage message)
        {
            switch (message.SenderConnection.Status)
            {
                case NetConnectionStatus.Connected:
                    ConnectionStatus = ConnectionStatus.Connected;
                    Plugin.Instance.Log("ConnectionStatus: Connected!");
                    HandleHandshakeRequest();
                    break;

                case NetConnectionStatus.Disconnected:
                    ConnectionStatus = ConnectionStatus.Disconnected;
                    Plugin.Instance.Log("ConnectionStatus: Disconnected!");
                    break;
            }
        }

        /// <summary>
        /// Handles data messages received from the server.
        /// </summary>
        /// <param name="message">The incoming data message.</param>
        private void HandleDataMessage(NetIncomingMessage message)
        {
            if (PacketUtility.Unpack(message, out ushort packetId))
            {
                Type packetType = PacketUtility.GetPacketType(packetId);

                if (packetType != null)
                {
                    var packet = (IPacket)Activator.CreateInstance(packetType);
                    packet.Deserialize(message);
                    Plugin.Instance.Log($"Received packet of type: {packetType.Name}");

                    try
                    {
                        HandlePacket(packet);
                    }
                    catch (ArgumentNullException ex)
                    {
                        Plugin.Instance.Log($"Error: {ex.Message}");
                    }
                    catch (InvalidOperationException ex)
                    {
                        Plugin.Instance.Log($"Error: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Plugin.Instance.Log($"Unexpected error: {ex.Message}");
                    }
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
        }

        /// <summary>
        /// Handles incoming packets by processing them based on their type.
        /// </summary>
        /// <param name="packet">The packet to handle. Must implement the <see cref="IPacket"/> interface.</param>
        /// <remarks>
        /// This method uses pattern matching to determine the specific type of the packet and delegates
        /// the processing to the corresponding handler method. Each packet type has its own handler for
        /// specific actions or responses.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="packet"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the packet type is unhandled or unsupported.
        /// </exception>
        private void HandlePacket(IPacket packet)
        {
            // Check if the packet is null
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet), "The packet cannot be null.");
            }

            // Use pattern matching to handle specific packet types
            switch (packet)
            {
                case HandshakeRequestPacket handshakeRequestPacket:
                    // HandleHandshakeRequest(handshakeRequestPacket);
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

                default:
                    // If no case matches, throw an exception
                    throw new InvalidOperationException($"Unhandled packet type: {packet.GetType().Name}");
            }
        }

        /// <summary>
        /// Handles the handshake request by responding with the client's Steam ID and
        /// updating the game state to <see cref="GameManager.GameState.WaitingForAccess"/>.
        /// </summary>
        public void HandleHandshakeRequest()
        {
            HandshakeResponsePacket handshakeResponse = new HandshakeResponsePacket
            {
                SteamID = ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(handshakeResponse, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);

            Plugin.Instance.game.gameState = GameManager.GameState.WaitingForAccess;
        }

        /// <summary>
        /// Handles the access denial by transitioning the game state to the main menu,
        /// updating the connection status, and disconnecting the client.
        /// </summary>
        /// <param name="accessDenied">The <see cref="AccessDeniedPacket"/> received from the server.</param>
        public void HandleAccessDenied(AccessDeniedPacket accessDenied)
        {
            Plugin.Instance.game.gameState = GameManager.GameState.MainMenu;
            Plugin.Instance.client.ConnectionStatus = ConnectionStatus.Disconnecting;

            try
            {
                // Attempt to disconnect the client
                Plugin.Instance.client.Disconnect();
            }
            catch (InvalidOperationException ex)
            {
                // Handle issues with client initialization or connection state
                Console.WriteLine($"Operation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles access granted by sending player join and editor state request packets
        /// to the server and updating the permission level and game state.
        /// </summary>
        /// <param name="accessGranted">The <see cref="AccessGrantedPacket"/> received from the server.</param>
        public void HandleAccessGranted(AccessGrantedPacket accessGranted)
        {
            PlayerData localPlayer = Utils.GetLocalPlayerData();

            // Create and send player join packet
            PlayerJoinPacket playerJoin = new PlayerJoinPacket
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

            // Create and send editor state request packet
            EditorStateRequestPacket editorRequest = new EditorStateRequestPacket
            {
                SteamID = localPlayer.steamID
            };

            var outgoingMessage2 = client.CreateMessage();
            PacketUtility.Pack(editorRequest, outgoingMessage2);
            client.SendMessage(outgoingMessage2, NetDeliveryMethod.ReliableOrdered, 0);

            PermissionLevel = (PermissionLevel)accessGranted.Level;
            Plugin.Instance.game.gameState = GameManager.GameState.WaitingOnEditorDataInMainMenu;
        }

        /// <summary>
        /// Handles a player joining by converting the join packet into player data
        /// and adding the player to the multiplayer session.
        /// </summary>
        /// <param name="playerJoinPacket">The <see cref="PlayerJoinPacket"/> received from the server.</param>
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

        /// <summary>
        /// Handles a player leaving the game by removing them from the multiplayer session.
        /// </summary>
        /// <param name="playerLeft">The <see cref="PlayerLeftPacket"/> containing the player's Steam ID.</param>
        public void HandlePlayerLeft(PlayerLeftPacket playerLeft)
        {
            Plugin.Instance.multiplayer.RemovePlayer(playerLeft.SteamID);
        }

        /// <summary>
        /// Handles the update of a player's state by converting the packet data into
        /// a player state object and updating the multiplayer session.
        /// </summary>
        /// <param name="playerState">The <see cref="PlayerStatePacket"/> containing the player's state data.</param>
        public void HandlePlayerState(PlayerStatePacket playerState)
        {
            PlayerStateData stateData = new PlayerStateData
            {
                SteamID = playerState.SteamID,
                Position = new Vector3(playerState.PositionX, playerState.PositionY, playerState.PositionZ),
                Rotation = new Vector3(playerState.EulerX, playerState.EulerY, playerState.EulerZ),
                Mode = playerState.Mode
            };

            Plugin.Instance.multiplayer.UpdatePlayerState(stateData);
        }

        /// <summary>
        /// Handles the editor state response by updating the editor state
        /// and transitioning the game state if necessary.
        /// </summary>
        /// <param name="editorState">The <see cref="EditorStateResponsePacket"/> containing the editor state data.</param>
        public void HandleEditorState(EditorStateResponsePacket editorState)
        {
            EditorStateData state = new EditorStateData
            {
                floor = editorState.Floor,
                skybox = editorState.Skybox,
                blocks = editorState.BlockStrings
            };

            Plugin.Instance.editor.SetState(state);

            if (Plugin.Instance.game.gameState == GameManager.GameState.WaitingOnEditorDataInMainMenu)
            {
                // Transition to the next state and load into the editor
                Plugin.Instance.game.gameState = GameManager.GameState.EnteringTeamXFromMainMenu;
                Plugin.Instance.game.LoadIntoEditorX();
            }
        }

        /// <summary>
        /// Handles the creation of a block in the editor by adding it to the editor's data
        /// and visually creating it in the level editor if applicable.
        /// </summary>
        /// <param name="editorBlockCreate">The <see cref="EditorBlockCreatePacket"/> containing the block data.</param>
        public void HandleEditorBlockCreate(EditorBlockCreatePacket editorBlockCreate)
        {
            Block packetBlock = editorBlockCreate.BlockString.FromJson();

            // Update the editor state
            Plugin.Instance.editor.Add(packetBlock);

            if (Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.CreateBlock(packetBlock);
            }
        }

        /// <summary>
        /// Handles the update of a block in the editor by updating the editor's data
        /// and visually updating the block in the level editor if applicable.
        /// </summary>
        /// <param name="editorBlockUpdate">The <see cref="EditorBlockUpdatePacket"/> containing the updated block data.</param>
        public void HandleEditorBlockUpdate(EditorBlockUpdatePacket editorBlockUpdate)
        {
            Block packetBlock = editorBlockUpdate.BlockString.FromJson();

            Plugin.Instance.editor.Update(packetBlock);

            if (Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.UpdateBlock(packetBlock.ToBlockPropertyJSON());
            }
        }

        /// <summary>
        /// Handles the destruction of a block in the editor by removing it from the editor's data
        /// and visually destroying the block in the level editor if applicable.
        /// </summary>
        /// <param name="editorBlockDestroy">The <see cref="EditorBlockDestroyPacket"/> containing the block's unique ID.</param>
        public void HandleEditorBlockDestroy(EditorBlockDestroyPacket editorBlockDestroy)
        {
            Plugin.Instance.editor.Remove(editorBlockDestroy.UID);

            if (Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.DestroyBlock(editorBlockDestroy.UID);
            }
        }

        /// <summary>
        /// Handles the update of the editor's skybox by applying the new skybox
        /// and visually updating it in the level editor if applicable.
        /// </summary>
        /// <param name="editorSkybox">The <see cref="EditorSkyboxPacket"/> containing the skybox data.</param>
        public void HandleEditorSkybox(EditorSkyboxPacket editorSkybox)
        {
            Plugin.Instance.editor.Skybox = editorSkybox.Skybox;

            if(Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.UpdateSkybox(editorSkybox.Skybox);
            }
        }

        /// <summary>
        /// Handles the update of the editor's floor by applying the new floor data
        /// and visually updating it in the level editor if applicable.
        /// </summary>
        /// <param name="editorFloor">The <see cref="EditorFloorPacket"/> containing the floor data.</param>
        public void HandleEditorFloor(EditorFloorPacket editorFloor)
        {
            Plugin.Instance.editor.Floor = editorFloor.Floor;

            if(Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.UpdateFloor(editorFloor.Floor);
            }
        }

        /// <summary>
        /// Handles the denial of a block creation request by removing the block
        /// from the editor's data and visually destroying it if applicable.
        /// </summary>
        /// <param name="createDenied">The <see cref="EditorBlockCreateDeniedPacket"/> containing the block's unique ID.</param>
        public void HandlerEditorBlockCreateDenied(EditorBlockCreateDeniedPacket createDenied)
        {
            Plugin.Instance.editor.Remove(createDenied.UID);

            if (Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.DestroyBlock(createDenied.UID);
            }
        }

        /// <summary>
        /// Handles the denial of a block update request by restoring the block's previous state
        /// in the editor and visually updating it if applicable.
        /// </summary>
        /// <param name="updateDenied">The <see cref="EditorBlockUpdateDeniedPacket"/> containing the block's previous state.</param>
        public void HandleEditorBlockUpdateDenied(EditorBlockUpdateDeniedPacket updateDenied)
        {
            Block packetBlock = updateDenied.BlockString.FromJson();
            Plugin.Instance.editor.Update(packetBlock);

            if(Plugin.Instance.editor.InLevelEditor())
            {
                BlockPropertyJSON blockPropertyJSON = packetBlock.ToBlockPropertyJSON();
                Plugin.Instance.editor.Modifier.UpdateBlock(blockPropertyJSON);
            }            
        }

        /// <summary>
        /// Handles the denial of a block destruction request by re-adding the block
        /// to the editor's data and visually recreating it in the editor if applicable.
        /// </summary>
        /// <param name="destroyDenied">The <see cref="EditorBlockDestroyDeniedPacket"/> containing the block's data.</param>
        public void HandleEditorBlockDestroyDenied(EditorBlockDestroyDeniedPacket destroyDenied)
        {
            Block packetBlock = destroyDenied.BlockString.FromJson();
            Plugin.Instance.editor.Add(packetBlock);

            if(Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.CreateBlock(packetBlock);
            }            
        }

        /// <summary>
        /// Handles the denial of a floor update request by restoring the floor
        /// to the specified value in the editor and visually updating it in the editor if applicable.
        /// </summary>
        /// <param name="floorDenied">The <see cref="EditorFloorDeniedPacket"/> containing the floor data.</param>
        public void HandleEditorFloorDenied(EditorFloorDeniedPacket floorDenied)
        {
            Plugin.Instance.editor.Floor = floorDenied.Floor;

            if(Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.UpdateFloor(floorDenied.Floor);
            }
        }

        /// <summary>
        /// Handles the denial of a skybox update request by restoring the skybox
        /// to the specified value in the editor and visually updating it in the editor if applicable.
        /// </summary>
        /// <param name="skyboxDenied">The <see cref="EditorSkyboxDeniedPacket"/> containing the skybox data.</param>
        public void HandleEditorSkyboxDenied(EditorSkyboxDeniedPacket skyboxDenied)
        {
            Plugin.Instance.editor.Skybox = skyboxDenied.Skybox;

            if(Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.UpdateSkybox(skyboxDenied.Skybox);
            }
        }

        /// <summary>
        /// Handles the denial of a selection request by deselecting the block
        /// with the specified UID in the editor.
        /// </summary>
        /// <param name="selectionDenied">The <see cref="EditorSelectionDeniedPacket"/> containing the block's UID.</param>
        public void HandleSelectionDenied(EditorSelectionDeniedPacket selectionDenied)
        {
            if (Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.DeselectBlock(selectionDenied.UID);
            }
        }

        /// <summary>
        /// Sends a block creation request to the server.
        /// </summary>
        /// <param name="block">The <see cref="Block"/> to be created.</param>
        public void SendBlockCreate(Block block)
        {
            EditorBlockCreatePacket blockCreate = new EditorBlockCreatePacket()
            {
                BlockString = block.ToJson(),
                SteamID = ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(blockCreate, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        /// <summary>
        /// Sends a block update request to the server.
        /// </summary>
        /// <param name="block">The <see cref="Block"/> to be updated.</param>
        public void SendBlockUpdate(Block block)
        {
            EditorBlockUpdatePacket blockUpdate = new EditorBlockUpdatePacket()
            {
                BlockString = block.ToJson(),
                SteamID = ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(blockUpdate, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        /// <summary>
        /// Sends a block destruction request to the server.
        /// </summary>
        /// <param name="uid">The unique identifier (UID) of the block to be destroyed.</param>
        public void SendBlockDestroy(string uid)
        {
            EditorBlockDestroyPacket blockDestroy = new EditorBlockDestroyPacket()
            {
                UID = uid,
                SteamID = ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(blockDestroy, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        /// <summary>
        /// Sends a floor update request to the server.
        /// </summary>
        /// <param name="floor">The new floor value to be set.</param>
        public void SendFloorUpdate(int floor)
        {
            EditorFloorPacket floorPacket = new EditorFloorPacket()
            {
                Floor = floor,
                SteamID = ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(floorPacket, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        /// <summary>
        /// Sends a skybox update request to the server.
        /// </summary>
        /// <param name="skybox">The new skybox value to be set.</param>
        public void SendSkyboxUpdate(int skybox)
        {
            EditorSkyboxPacket skyboxPacket = new EditorSkyboxPacket()
            {
                Skybox = skybox,
                SteamID = ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(skyboxPacket, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        /// <summary>
        /// Sends the player's current state to the server.
        /// </summary>
        /// <param name="stateData">The <see cref="PlayerStateData"/> representing the player's state.</param>
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

        /// <summary>
        /// Sends a selection request to the server for a block with the specified UID.
        /// </summary>
        /// <param name="uid">The unique identifier (UID) of the block to select.</param>
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

        /// <summary>
        /// Sends a deselection request to the server for a block with the specified UID.
        /// </summary>
        /// <param name="uid">The unique identifier (UID) of the block to deselect.</param>
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
