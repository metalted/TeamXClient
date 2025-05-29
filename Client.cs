using System;
using Lidgren.Network;
using TeamXNetwork;
using TeamXClient.Extensions;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TeamXClient
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
        private void StartClient(string appID)
        {
            netPeerConfiguration = new NetPeerConfiguration(appID);
            netPeerConfiguration.ConnectionTimeout = 5000;
            client = new NetClient(netPeerConfiguration);
            client.Start();
        }

        /// <summary>
        /// Try to start the connection process with the server configured in the settings.
        /// </summary>
        public void AttemptToConnectToServer()
        {
            if (client == null)
            {
                throw new InvalidOperationException("Client is not initialized.");
            }

            Plugin.Instance.Log("Connecting...", LogType.Message);
            PlayerManager.Instance.messenger.Log("Attempting connection...", 2f);

            try
            {
                // Attempt to connect to the server
                client.Connect(Plugin.Instance.cfg_serverIP.Value, Plugin.Instance.cfg_serverPort.Value);
                Plugin.Instance.Log("Successfully started connecting to the server.", LogType.Message);
                ConnectionStatus = ConnectionStatus.Connecting;
            }
            catch (ArgumentException ex)
            {
                // Handle invalid IP address or port
                Plugin.Instance.Log($"Invalid input: {ex.Message}", LogType.Error);
            }
            catch (InvalidOperationException ex)
            {
                // Handle issues with client initialization or connection state
                Plugin.Instance.Log($"Operation failed: {ex.Message}", LogType.Error);
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions
                Plugin.Instance.Log($"Unexpected error: {ex.Message}", LogType.Error);
            }
        }

        /// <summary>
        /// Try to close the connection with the currently connected server.
        /// </summary>
        public void AttemptDisconnect()
        {
            try
            {
                // Attempt to disconnect the client
                Plugin.Instance.client.Disconnect();
            }
            catch (InvalidOperationException ex)
            {
                // Handle issues with client initialization or connection state
                Plugin.Instance.Log($"Operation failed: {ex.Message}", LogType.Error);
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions
                Plugin.Instance.Log($"Unexpected error: {ex.Message}", LogType.Error);
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
                        Plugin.Instance.Log($"Unhandled message type: {im.MessageType}", LogType.Warning);
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
                    Plugin.Instance.Log("ConnectionStatus: Connected!", LogType.Message);
                    PlayerManager.Instance.messenger.Log("Connected", 2f);
                    HandleHandshakeRequest();
                    break;

                case NetConnectionStatus.Disconnected:
                    ConnectionStatus = ConnectionStatus.Disconnected;
                    Plugin.Instance.Log("ConnectionStatus: Disconnected!", LogType.Message);
                    PlayerManager.Instance.messenger.Log("Disconnected", 2f);
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
                    Plugin.Instance.Log($"Received packet of type: {packetType.Name}", LogType.Debug);

                    try
                    {
                        HandlePacket(packet);
                    }
                    catch (ArgumentNullException ex)
                    {
                        Plugin.Instance.Log($"Error: {ex.Message}", LogType.Error);
                    }
                    catch (InvalidOperationException ex)
                    {
                        Plugin.Instance.Log($"Error: {ex.Message}", LogType.Error);
                    }
                    catch (Exception ex)
                    {
                        Plugin.Instance.Log($"Unexpected error in HandleDataMessage: {ex.Message}", LogType.Error);
                    }
                }
                else
                {
                    Plugin.Instance.Log($"Unknown packet ID: {packetId}", LogType.Warning);
                }
            }
            else
            {
                Plugin.Instance.Log("Failed to unpack the message.", LogType.Warning);
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
                case ServerRulesResponsePacket serverRulesResponse:
                    HandleServerRules(serverRulesResponse);
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
                case PermissionTableResponse permissionTableResponse:
                    HandlePermissionTableResponse(permissionTableResponse);
                    break;
                case SaveConfigurationResponsePacket saveConfigurationResponse:
                    HandleSaveConfigurationResponse(saveConfigurationResponse);
                    break;
                case LevelDirectoryResponsePacket levelDirectoryResponse:
                    HandleLevelDirectoryResponse(levelDirectoryResponse);
                    break;
                case ChatMessagePacket chatMessagePacket:
                    HandleChatMessage(chatMessagePacket);
                    break;
                case HornPacket hornPacket:
                    HandleHorn(hornPacket);
                    break;
                case CustomMessagePacket customMessagePacket:
                    HandleCustomMessage(customMessagePacket);
                    break;
                case ChatCommandResponsePacket chatCommandResponse:
                    HandleCommandResponse(chatCommandResponse);
                    break;
                case EditorDeselectAllOrderPacket deselectAllOrder:
                    HandleDeselectAllOrder(deselectAllOrder);
                    break;
                default:
                    // If no case matches, throw an exception
                    throw new InvalidOperationException($"Unhandled packet type: {packet.GetType().Name}");
            }
        }

        #region Connection and Players

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
            Plugin.Instance.multiplayer.cachedLocalPlayerData = Utils.GetLocalPlayerData();
            PlayerData lp = Plugin.Instance.multiplayer.cachedLocalPlayerData;

            Plugin.Instance.Log($"Access Granted, Local Player Data: {lp.ToDebugString()}", LogType.Debug);

            // Create and send player join packet
            PlayerJoinPacket playerJoin = new PlayerJoinPacket
            {
                Color = lp.color,
                Color_body = lp.color_body,
                Color_leftArm = lp.color_leftArm,
                Color_leftLeg = lp.color_leftLeg,
                Color_rightArm = lp.color_rightArm,
                Color_rightLeg = lp.color_rightLeg,
                FrontWheels = lp.frontWheels,
                Glasses = lp.glasses,
                Hat = lp.hat,
                Horn = lp.horn,
                Name = lp.name,
                Paraglider = lp.paraglider,
                RearWheels = lp.rearWheels,
                SteamID = lp.steamID,
                Zeepkist = lp.zeepkist
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(playerJoin, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);

            // Create and send editor state request packet
            EditorStateRequestPacket editorRequest = new EditorStateRequestPacket
            {
                SteamID = lp.steamID
            };

            var outgoingMessage2 = client.CreateMessage();
            PacketUtility.Pack(editorRequest, outgoingMessage2);
            client.SendMessage(outgoingMessage2, NetDeliveryMethod.ReliableOrdered, 0);

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
                color_rightLeg = playerJoinPacket.Color_rightLeg,
                frontWheels = playerJoinPacket.FrontWheels,
                glasses = playerJoinPacket.Glasses,
                hat = playerJoinPacket.Hat,
                horn = playerJoinPacket.Horn,
                name = playerJoinPacket.Name,
                paraglider = playerJoinPacket.Paraglider,
                rearWheels = playerJoinPacket.RearWheels,
                state = 0,
                steamID = playerJoinPacket.SteamID,
                zeepkist = playerJoinPacket.Zeepkist
            };

            Plugin.Instance.Log($"PlayerJoined, Player Data: {playerData.ToDebugString()}", LogType.Debug);

            Plugin.Instance.multiplayer.AddPlayer(playerData);

            PlayerManager.Instance.messenger.Log($"{playerData.name} has joined the game!", 2f);
        }

        /// <summary>
        /// Handles a player leaving the game by removing them from the multiplayer session.
        /// </summary>
        /// <param name="playerLeft">The <see cref="PlayerLeftPacket"/> containing the player's Steam ID.</param>
        public void HandlePlayerLeft(PlayerLeftPacket playerLeft)
        {
            string name = Plugin.Instance.multiplayer.GetPlayerName(playerLeft.SteamID);

            Plugin.Instance.multiplayer.RemovePlayer(playerLeft.SteamID);

            PlayerManager.Instance.messenger.Log($"{name} has left the game!", 2f);
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
        /// Handle a chat message send from the server.
        /// </summary>
        /// <param name="chatMessagePacket">The <see cref="ChatMessagePacket"/> containing the chat message.</param>
        public void HandleChatMessage(ChatMessagePacket chatMessagePacket)
        {
            InterfaceManager.ReceivedChat(chatMessagePacket);
        }

        /// <summary>
        /// Send a chat message to all other connected players.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void SendChatMessage(string message)
        {
            if(string.IsNullOrEmpty(message))
            {
                return;
            }

            ChatMessagePacket chatMessage = new ChatMessagePacket()
            {
                Message = message,
                SteamID = ClientSteamID,
                Username = Plugin.Instance.multiplayer.cachedLocalPlayerData.name,
                Color = Plugin.Instance.multiplayer.cachedLocalPlayerData.chatColor
            };

            InterfaceManager.ReceivedChat(chatMessage);

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(chatMessage, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        /// <summary>
        /// Send a command to the server if player is admin.
        /// </summary>
        /// <param name="command">The command to send</param>
        public void SendCommand(string command)
        {
            if(string.IsNullOrEmpty(command))
            {
                return;
            }

            if(!Plugin.Instance.perms.IsAdmin())
            {
                return;
            }

            ChatCommandPacket chatCommand = new ChatCommandPacket()
            {
                Command = command,
                SteamID = ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(chatCommand, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        /// <summary>
        /// Handle the response from a chat command.
        /// </summary>
        /// <param name="commandResponsePacket">The <see cref="ChatCommandResponsePacket"/> containing the response. </param>
        public void HandleCommandResponse(ChatCommandResponsePacket commandResponsePacket)
        {
            switch (commandResponsePacket.ResponseType)
            {
                case "PlayerList":
                    Debug.Log("### TeamX PlayerList ###");
                    string[] entries = commandResponsePacket.Message.Split('|');
                    foreach (string e in entries)
                    {
                        Debug.Log(e);
                    }
                    PlayerManager.Instance.messenger.Log("Playerlist logged", 2f);
                    break;
                case "Message":
                    PlayerManager.Instance.messenger.Log(commandResponsePacket.Message, 2f);
                    break;
                case "Command":
                    Plugin.Instance.OnCommandReceived?.Invoke(commandResponsePacket.Message);
                    break;
            }
        }

        /// <summary>
        /// Handle the message of somebody honking.
        /// </summary>
        /// <param name="hornPacket">The <see cref="HornPacket"/> containing the honk honk data. </param>
        public void HandleHorn(HornPacket hornPacket)
        {
            //Get the horn of this player
            int playerHornID = Plugin.Instance.multiplayer.GetPlayerHorn(hornPacket.SteamID);

            if(playerHornID != -1)
            {
                Transform playerTransform = Plugin.Instance.multiplayer.GetPlayerTransform(hornPacket.SteamID);

                if (playerTransform != null)
                {
                    Plugin.Instance.isRemoteHorn = true;
                    PlayerManager.Instance.hornsIndex.PlayHornPlayback((FMOD_HornsIndex.HornType)playerHornID, playerTransform, 2);
                }
            }
        }

        /// <summary>
        /// Send a honk honk to all other players connected.
        /// </summary>
        public void SendHorn()
        {
            HornPacket hornPacket = new HornPacket()
            {
                SteamID = ClientSteamID,
                HornID = Plugin.Instance.multiplayer.cachedLocalPlayerData.horn
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(hornPacket, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        /// <summary>
        /// Handle a custom message with whatever payload.
        /// </summary>
        /// <param name="customMessagePacket">The <see cref="CustomMessagePacket"/> containing the custom message data. </param>
        public void HandleCustomMessage(CustomMessagePacket customMessagePacket)
        {
            Plugin.Instance.OnCustomMessageReceived?.Invoke((customMessagePacket.SteamID, customMessagePacket.Payload));
        }

        /// <summary>
        /// Send a custom message to everybody connected to the server
        /// </summary>
        /// <param name="payload">The payload to send.</param>
        public void SendCustomMessage(string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return;
            }

            CustomMessagePacket customMessagePacket = new CustomMessagePacket()
            {
                SteamID = ClientSteamID,
                Payload = payload
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(customMessagePacket, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        #endregion

        #region Editor Messages

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
                //Request the ServerRulesPacket which we need to apply before we load the editor.
                ServerRulesRequestPacket serverRules = new ServerRulesRequestPacket()
                {
                    SteamID = ClientSteamID
                };

                var outgoingMessage = client.CreateMessage();
                PacketUtility.Pack(serverRules, outgoingMessage);
                client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);

                Plugin.Instance.game.gameState = GameManager.GameState.WaitingOnServerRulesInMainMenu;
            }
            else if (Plugin.Instance.game.gameState == GameManager.GameState.TeamXEditor)
            {
                //We received a new editor state package while in game, meaning the map has been reloaded from the server.
                
                //Deselect everything.
                Plugin.Instance.editor.Modifier.DeselectAllBlocks();

                //Destroy all blocks
                Plugin.Instance.editor.Modifier.ClearEditor();

                //Instantiate
                Plugin.Instance.editor.InstantiateFromState();

                //Reset ctrl z
                Plugin.Instance.editor.Central.undoRedo.ResetUndoList(false);
            }
        }

        /// <summary>
        /// Handles the creation of a block in the editor by adding it to the editor's data
        /// and visually creating it in the level editor if applicable.
        /// </summary>
        /// <param name="editorBlockCreate">The <see cref="EditorBlockCreatePacket"/> containing the block data.</param>
        public void HandleEditorBlockCreate(EditorBlockCreatePacket editorBlockCreate)
        {
            BlockPropertyJSONX packetBlock = BlockPropertyJSONX.FromJson(editorBlockCreate.BlockString);

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
            BlockPropertyJSONX packetBlock = BlockPropertyJSONX.FromJson(editorBlockUpdate.BlockString);

            Plugin.Instance.editor.Update(packetBlock);

            if (Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.UpdateBlock(packetBlock.blockPropertyJSON);
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

            if (Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.UpdateSkybox(Plugin.Instance.editor.Skybox);
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

            if (Plugin.Instance.editor.InLevelEditor())
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
            BlockPropertyJSONX packetBlock = BlockPropertyJSONX.FromJson(updateDenied.BlockString);
            Plugin.Instance.editor.Update(packetBlock);

            if (Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.UpdateBlock(packetBlock.blockPropertyJSON);
            }
        }

        /// <summary>
        /// Handles the denial of a block destruction request by re-adding the block
        /// to the editor's data and visually recreating it in the editor if applicable.
        /// </summary>
        /// <param name="destroyDenied">The <see cref="EditorBlockDestroyDeniedPacket"/> containing the block's data.</param>
        public void HandleEditorBlockDestroyDenied(EditorBlockDestroyDeniedPacket destroyDenied)
        {
            BlockPropertyJSONX packetBlock = BlockPropertyJSONX.FromJson(destroyDenied.BlockString);
            Plugin.Instance.editor.Add(packetBlock);

            if (Plugin.Instance.editor.InLevelEditor())
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

            if (Plugin.Instance.editor.InLevelEditor())
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

            if (Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.UpdateSkybox(Plugin.Instance.editor.Skybox);
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

        public void HandleDeselectAllOrder(EditorDeselectAllOrderPacket deselectOrder)
        {
            if(Plugin.Instance.editor.InLevelEditor())
            {
                Plugin.Instance.editor.Modifier.DeselectAllBlocks();
            }
        }

        /// <summary>
        /// Sends a block creation request to the server.
        /// </summary>
        /// <param name="block">The <see cref="Block"/> to be created.</param>
        public void SendBlockCreate(BlockPropertyJSONX block)
        {
            EditorBlockCreatePacket blockCreate = new EditorBlockCreatePacket()
            {
                BlockString = block.ToJson(),
                SteamID = ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(blockCreate, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);

            //Is this block part of the selection?
            if (Utils.IsBlockSelected(block.blockPropertyJSON.u))
            {
                SendSelection(block.blockPropertyJSON.u);
            }
        }

        /// <summary>
        /// Sends a block update request to the server.
        /// </summary>
        /// <param name="block">The <see cref="Block"/> to be updated.</param>
        public void SendBlockUpdate(BlockPropertyJSONX block)
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
        public void SendSkyboxUpdate(string skybox)
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

        #endregion

        #region Permission System 

        /// <summary>
        /// Handles the server rules response by assigning the received permissions to the local player or by blocking players from joining if they are banned.
        /// </summary>
        /// <param name="serverRules">The <see cref="ServerRulesResponsePacket "/> containing the local players permission data and server rule data.</param>
        public void HandleServerRules(ServerRulesResponsePacket serverRules)
        {
            PermissionProfile perms = new PermissionProfile(serverRules);
            Plugin.Instance.perms.SetLocalProfile(perms);
            
            //We are currently in the main menu, requesting permissions before joining the server.
            if (Plugin.Instance.game.gameState == GameManager.GameState.WaitingOnServerRulesInMainMenu)
            {
                //If the player is banned, go back to main menu state and attempt disconnecting.
                if (Plugin.Instance.perms.IsBanned())
                {
                    Plugin.Instance.game.gameState = GameManager.GameState.MainMenu;
                    Plugin.Instance.client.ConnectionStatus = ConnectionStatus.Disconnecting;

                    AttemptDisconnect();                    
                }
                //The player is allowed to join the server, go ahead an load the editor.
                else
                {
                    // Transition to the next state and load into the editor
                    Plugin.Instance.game.gameState = GameManager.GameState.EnteringTeamXFromMainMenu;
                    Plugin.Instance.game.LoadIntoEditorX();
                }
            }
            //We are in a server and received an updated permission package.
            else if(Plugin.Instance.game.gameState == GameManager.GameState.TeamXEditor || Plugin.Instance.game.gameState == GameManager.GameState.TeamXGame)
            {
                //Did we just get banned?
                if (Plugin.Instance.perms.IsBanned())
                {
                    //Main menu is the default state for non TeamX players.
                    Plugin.Instance.game.gameState = GameManager.GameState.MainMenu;
                    Plugin.Instance.client.ConnectionStatus = ConnectionStatus.Disconnecting;
                    AttemptDisconnect();
                }
            }
        }

        /// <summary>
        /// Handles the permission table received from the server (send to admins when requested from the permission panel).
        /// </summary>
        /// <param name="tableResponse">The <see cref="PermissionTableResponse"/> containing the permission entries.</param>
        public void HandlePermissionTableResponse(PermissionTableResponse tableResponse)
        {
            if (InterfaceManager.permissionPanel != null)
            {
                InterfaceManager.permissionPanel.ImportEntries(tableResponse.permissionTable);
            }
        }

        /// <summary>
        /// Sends a permission table request to the server, which returns a PermissionTableResponse if the caller is high enough permission.
        /// </summary>
        public void SendPermissionTableRequest()
        {
            PermissionTableRequest permissionTableRequest = new PermissionTableRequest()
            {
                SteamID = ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(permissionTableRequest, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        /// <summary>
        /// Send the updated permission table from the permission panel to the server.
        /// </summary>
        /// <param name="entries"></param>
        public void SendPermissionTableSubmit(List<TeamXPermissionPanelEntry> entries)
        {
            PermissionTableSubmit permissionTableSubmit = new PermissionTableSubmit()
            {
                SteamID = ClientSteamID
            };

            permissionTableSubmit.permissionTable = new List<(ulong, string, string)>();

            foreach (TeamXPermissionPanelEntry e in entries)
            {
                string perm = "default";

                if (e.banned)
                {
                    perm = "banned";
                }
                else if (e.guest)
                {
                    perm = "guest";
                }
                else if (e.trusted)
                {
                    perm = "trusted";
                }
                else if (e.admin)
                {
                    perm = "admin";
                }

                permissionTableSubmit.permissionTable.Add((e.steamID, e.user, perm));
            }

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(permissionTableSubmit, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        #endregion

        #region Save Configuration
        /// <summary>
        /// Handles the save configuration information received from the server (send to admins when requested from the save configuration panel).
        /// </summary>
        /// <param name="saveConfigurationResponse">The <see cref="SaveConfigurationResponsePacket"/> containing the save configuration data.</param>
        public void HandleSaveConfigurationResponse(SaveConfigurationResponsePacket saveConfigurationResponse)
        {
            if (InterfaceManager.saveConfigurationPanel != null)
            {
                InterfaceManager.saveConfigurationPanel.UpdateValues(saveConfigurationResponse.AutoSaveInterval, saveConfigurationResponse.BackupCount, saveConfigurationResponse.KeepBackupWithNoEditors, saveConfigurationResponse.LevelName, saveConfigurationResponse.LoadBackupOnStart);
            }
        }

        public void SendSaveConfigurationRequestPacket()
        {
            SaveConfigurationRequestPacket saveConfigurationRequest = new SaveConfigurationRequestPacket()
            {
                SteamID = ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(saveConfigurationRequest, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void SendSaveConfigurationSubmitPacket(int autosaveInterval, int backupCount, bool keepBackupWithNoEditors, string levelName, bool loadBackupOnStart)
        {
            SaveConfigurationSubmitPacket saveConfigurationSubmit = new SaveConfigurationSubmitPacket()
            {
                SteamID = ClientSteamID,
                AutoSaveInterval = autosaveInterval,
                BackupCount = backupCount,
                KeepBackupWithNoEditors = keepBackupWithNoEditors,
                LevelName = levelName,
                LoadBackupOnStart = loadBackupOnStart
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(saveConfigurationSubmit, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }
        #endregion

        #region Level Management
        /// <summary>
        /// Handles the level information received from the server (send to admins when requested from the level management panel).
        /// </summary>
        /// <param name="levelDirectoryResponse">The <see cref="LevelDirectoryResponsePacket"/> containing the local paths of all teamkist projects on the server.</param>
        public void HandleLevelDirectoryResponse(LevelDirectoryResponsePacket levelDirectoryResponse)
        {
            if (InterfaceManager.levelManagerPanel != null)
            {
                InterfaceManager.levelManagerPanel.ImportDirectories(levelDirectoryResponse.LocalPaths);
            }
        }

        public void SendLoadLevelRequestPacket(string localPath)
        {
            LoadLevelRequestPacket loadLevelRequest = new LoadLevelRequestPacket()
            {
                SteamID = ClientSteamID,
                LocalPath = localPath
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(loadLevelRequest, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void SendLevelDirectoryRequestPacket()
        {
            LevelDirectoryRequestPacket levelDirectoryRequest = new LevelDirectoryRequestPacket()
            {
                SteamID = ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(levelDirectoryRequest, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }

        public void SendSaveCurrentState()
        {
            SaveCurrentStatePacket saveCurrentState = new SaveCurrentStatePacket()
            {
                SteamID = ClientSteamID
            };

            var outgoingMessage = client.CreateMessage();
            PacketUtility.Pack(saveCurrentState, outgoingMessage);
            client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, 0);
        }
        #endregion
    }
}
