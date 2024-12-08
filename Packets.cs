using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using TeamXClient.Extensions;

namespace TeamXClient
{
    public enum PermissionLevel
    {
        Banned = 0,
        Player = 1,
        Moderator = 2,
        Admin = 3,
        Owner = 4
    }

    public interface IPacket
    {
        void Deserialize(NetIncomingMessage im);

        void Serialize(NetOutgoingMessage om);
    }

    public static class PacketUtility
    {
        private static readonly Dictionary<ushort, Type> PacketTypeRegistry = new();

        /// <summary>
        /// Automatically registers all packets in the same namespace as <see cref="PacketUtility"/> 
        /// that implement <see cref="IPacket"/> and are structs.
        /// </summary>
        public static void AutoRegisterPacketsInSameNamespace()
        {
            string targetNamespace = typeof(PacketUtility).Namespace;

            var packetInterface = typeof(IPacket);

            var packetTypes = AppDomain.CurrentDomain
                                       .GetAssemblies()
                                       .SelectMany(a => a.GetTypes())
                                       .Where(t => t.Namespace != null &&
                                                   t.Namespace == targetNamespace &&
                                                   packetInterface.IsAssignableFrom(t) &&
                                                   t.IsValueType);

            foreach (var type in packetTypes)
            {
                RegisterPacketType(type);
            }
        }

        /// <summary>
        /// Registers a packet type and assigns it a unique ID based on its stable hash code.
        /// </summary>
        /// <param name="packetType">The type of the packet to register.</param>
        public static void RegisterPacketType(Type packetType)
        {
            ushort packetId = (ushort)(packetType.Name.GetStableHashCode() & ushort.MaxValue);
            PacketTypeRegistry[packetId] = packetType;
            Plugin.Instance.Log($"Registering: {packetType.Name}, Packet ID: {packetId}", LogType.Debug);
        }

        /// <summary>
        /// Retrieves the type of a packet using its ID.
        /// </summary>
        /// <param name="packetId">The ID of the packet.</param>
        /// <returns>The type of the packet, or null if not found.</returns>
        public static Type GetPacketType(ushort packetId)
        {
            return PacketTypeRegistry.TryGetValue(packetId, out var type) ? type : null;
        }

        /// <summary>
        /// Gets the packet ID for a given generic packet type.
        /// </summary>
        /// <typeparam name="T">The type of the packet.</typeparam>
        /// <returns>The ID of the packet.</returns>
        public static ushort GetPacketId<T>() where T : struct, IPacket
        {
            string typeName = typeof(T).Name;
            return (ushort)(typeName.GetStableHashCode() & ushort.MaxValue);
        }

        /// <summary>
        /// Generates a stable hash-based packet ID for a given type name.
        /// </summary>
        /// <param name="typeName">The name of the packet type.</param>
        /// <returns>The packet ID.</returns>
        private static ushort GetPacketId(string typeName)
        {
            return (ushort)(typeName.GetStableHashCode() & ushort.MaxValue);
        }

        /// <summary>
        /// Packs a packet into a <see cref="NetOutgoingMessage"/>.
        /// </summary>
        /// <typeparam name="T">The type of the packet.</typeparam>
        /// <param name="packet">The packet to pack.</param>
        /// <param name="outgoingMessage">The outgoing message to populate.</param>
        public static void Pack<T>(T packet, NetOutgoingMessage outgoingMessage) where T : struct, IPacket
        {
            ushort packetId = GetPacketId<T>();
            outgoingMessage.Write(packetId);
            packet.Serialize(outgoingMessage);
        }

        /// <summary>
        /// Unpacks a <see cref="NetIncomingMessage"/> to retrieve the message type.
        /// </summary>
        /// <param name="incomingMessage">The incoming message to unpack.</param>
        /// <param name="msgType">The message type.</param>
        /// <returns>True if unpacking was successful, otherwise false.</returns>
        public static bool Unpack(NetIncomingMessage incomingMessage, out ushort msgType)
        {
            try
            {
                msgType = incomingMessage.ReadUInt16();
                return true;
            }
            catch (Exception ex)
            {
                msgType = 0;
                return false;
            }
        }
    }

    /// <summary>
    /// Represents a handshake request packet sent during connection initialization.
    /// </summary>
    public struct HandshakeRequestPacket : IPacket
    {
        /// <summary>
        /// The handshake message content.
        /// </summary>
        public string Message;

        /// <summary>
        /// Deserializes the packet from a <see cref="NetIncomingMessage"/>.
        /// </summary>
        /// <param name="im">The incoming message.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            Message = im.ReadString();
        }

        /// <summary>
        /// Serializes the packet into a <see cref="NetOutgoingMessage"/>.
        /// </summary>
        /// <param name="om">The outgoing message.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(Message);
        }
    }

    /// <summary>
    /// Represents a handshake response packet sent from the client to the server during the connection process.
    /// </summary>
    public struct HandshakeResponsePacket : IPacket
    {
        /// <summary>
        /// The SteamID of the client being acknowledged.
        /// </summary>
        public ulong SteamID;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            SteamID = im.ReadUInt64();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(SteamID);
        }
    }

    /// <summary>
    /// Represents a packet sent from the server to the client to grant access and specify the user's permission level.
    /// </summary>
    public struct AccessGrantedPacket : IPacket
    {
        /// <summary>
        /// A message from the server, typically used to confirm the granted access.
        /// </summary>
        public string Message;

        /// <summary>
        /// The permission level granted to the client. 
        /// </summary>
        /// <remarks>
        /// Values are defined in the <see cref="PermissionLevel"/> enum.
        /// </remarks>
        public byte Level;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            Message = im.ReadString();
            Level = im.ReadByte();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(Message);
            om.Write(Level);
        }
    }

    /// <summary>
    /// Represents a packet sent from the server to the client to deny access, providing a reason for the denial.
    /// </summary>
    public struct AccessDeniedPacket : IPacket
    {
        /// <summary>
        /// The reason why access was denied.
        /// </summary>
        public string Reason;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            Reason = im.ReadString();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(Reason);
        }
    }

    /// <summary>
    /// Represents a packet sent when a player joins the session, containing the player's information and customization details.
    /// </summary>
    public struct PlayerJoinPacket : IPacket
    {
        /// <summary>The player's SteamID.</summary>
        public ulong SteamID;

        /// <summary>The player's display name.</summary>
        public string Name;

        /// <summary>The ID of the Zeepkist (cart) the player is using.</summary>
        public int Zeepkist;

        /// <summary>The ID of the front wheels.</summary>
        public int FrontWheels;

        /// <summary>The ID of the rear wheels.</summary>
        public int RearWheels;

        /// <summary>The ID of the paraglider.</summary>
        public int Paraglider;

        /// <summary>The ID of the horn.</summary>
        public int Horn;

        /// <summary>The ID of the hat.</summary>
        public int Hat;

        /// <summary>The ID of the glasses.</summary>
        public int Glasses;

        /// <summary>The player's body color.</summary>
        public int Color_body;

        /// <summary>The player's left arm color.</summary>
        public int Color_leftArm;

        /// <summary>The player's right arm color.</summary>
        public int Color_rightArm;

        /// <summary>The player's left leg color.</summary>
        public int Color_leftLeg;

        /// <summary>The player's right leg color.</summary>
        public int Color_rightLeg;

        /// <summary>The player's overall color.</summary>
        public int Color;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            SteamID = im.ReadUInt64();
            Name = im.ReadString();
            Zeepkist = im.ReadInt32();
            FrontWheels = im.ReadInt32();
            RearWheels = im.ReadInt32();
            Paraglider = im.ReadInt32();
            Horn = im.ReadInt32();
            Hat = im.ReadInt32();
            Glasses = im.ReadInt32();
            Color_body = im.ReadInt32();
            Color_leftArm = im.ReadInt32();
            Color_rightArm = im.ReadInt32();
            Color_leftLeg = im.ReadInt32();
            Color_rightLeg = im.ReadInt32();
            Color = im.ReadInt32();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(SteamID);
            om.Write(Name);
            om.Write(Zeepkist);
            om.Write(FrontWheels);
            om.Write(RearWheels);
            om.Write(Paraglider);
            om.Write(Horn);
            om.Write(Hat);
            om.Write(Glasses);
            om.Write(Color_body);
            om.Write(Color_leftArm);
            om.Write(Color_rightArm);
            om.Write(Color_leftLeg);
            om.Write(Color_rightLeg);
            om.Write(Color);
        }
    }

    /// <summary>
    /// Represents a request packet sent by the client to request the current state of the editor.
    /// </summary>
    public struct EditorStateRequestPacket : IPacket
    {
        /// <summary>The SteamID of the client making the request.</summary>
        public ulong SteamID;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            SteamID = im.ReadUInt64();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(SteamID);
        }
    }

    /// <summary>
    /// Represents a response from the server containing the current state of the editor,
    /// including floor, skybox, and serialized block data.
    /// </summary>
    public struct EditorStateResponsePacket : IPacket
    {
        /// <summary>
        /// The ID of the floor material.
        /// </summary>
        public int Floor;

        /// <summary>
        /// The ID of the skybox.
        /// </summary>
        public int Skybox;

        /// <summary>
        /// The number of blocks in the editor.
        /// </summary>
        public int BlockCount;

        /// <summary>
        /// A list of serialized block data strings.
        /// </summary>
        public List<string> BlockStrings;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="om">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage om)
        {
            BlockStrings = new List<string>();
            Floor = om.ReadInt32();
            Skybox = om.ReadInt32();
            BlockCount = om.ReadInt32();
            for (int i = 0; i < BlockCount; i++)
            {
                BlockStrings.Add(om.ReadString());
            }
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(Floor);
            om.Write(Skybox);
            om.Write(BlockCount);
            foreach (string s in BlockStrings)
            {
                om.Write(s);
            }
        }
    }

    /// <summary>
    /// Represents a packet sent to the server to notify about a new block, or sent from the server to the client to notify about another players block creation.
    /// </summary>
    public struct EditorBlockCreatePacket : IPacket
    {
        /// <summary>
        /// The SteamID of the client requesting the block creation.
        /// </summary>
        public ulong SteamID;

        /// <summary>
        /// The serialized string representation of the block to be created.
        /// </summary>
        public string BlockString;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            SteamID = im.ReadUInt64();
            BlockString = im.ReadString();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(SteamID);
            om.Write(BlockString);
        }
    }

    /// <summary>
    /// Represents a packet sent by the server to deny the creation of a block in the editor.
    /// </summary>
    public struct EditorBlockCreateDeniedPacket : IPacket
    {
        /// <summary>
        /// The unique identifier (UID) of the block that was denied creation.
        /// </summary>
        public string UID;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            UID = im.ReadString();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(UID);
        }
    }

    /// <summary>
    /// Represents a packet sent to the server to notify about a block removal, or sent from the server to the client to notify about another players block removal.
    /// </summary>
    public struct EditorBlockDestroyPacket : IPacket
    {
        /// <summary>
        /// The SteamID of the client requesting the block destruction.
        /// </summary>
        public ulong SteamID;

        /// <summary>
        /// The unique identifier (UID) of the block to be destroyed.
        /// </summary>
        public string UID;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            SteamID = im.ReadUInt64();
            UID = im.ReadString();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(SteamID);
            om.Write(UID);
        }
    }

    /// <summary>
    /// Represents a packet sent by the server to deny the destruction of a block in the editor.
    /// </summary>
    public struct EditorBlockDestroyDeniedPacket : IPacket
    {
        /// <summary>
        /// The serialized string representation of the block that was denied destruction.
        /// </summary>
        public string BlockString;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            BlockString = im.ReadString();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(BlockString);
        }
    }

    /// <summary>
    /// Represents a packet sent to the server to notify about a block update, or sent from the server to the client to notify about another players block update.
    /// </summary>
    public struct EditorBlockUpdatePacket : IPacket
    {
        /// <summary>
        /// The SteamID of the client requesting the block update.
        /// </summary>
        public ulong SteamID;

        /// <summary>
        /// The serialized string representation of the updated block data.
        /// </summary>
        public string BlockString;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            SteamID = im.ReadUInt64();
            BlockString = im.ReadString();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(SteamID);
            om.Write(BlockString);
        }
    }

    /// <summary>
    /// Represents a packet sent by the server to deny a block update in the editor.
    /// </summary>
    public struct EditorBlockUpdateDeniedPacket : IPacket
    {
        /// <summary>
        /// The serialized string representation of the block data that was denied for update.
        /// </summary>
        public string BlockString;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            BlockString = im.ReadString();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(BlockString);
        }
    }

    /// <summary>
    /// Represents a packet sent to the server to notify about a floor update, or sent from the server to the client to notify about another players floor update.
    /// </summary>
    public struct EditorFloorPacket : IPacket
    {
        /// <summary>
        /// The SteamID of the client making the request.
        /// </summary>
        public ulong SteamID;

        /// <summary>
        /// The ID of the floor material to apply.
        /// </summary>
        public int Floor;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            SteamID = im.ReadUInt64();
            Floor = im.ReadInt32();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(SteamID);
            om.Write(Floor);
        }
    }

    /// <summary>
    /// Represents a packet sent by the server to deny a floor update in the editor.
    /// </summary>
    public struct EditorFloorDeniedPacket : IPacket
    {
        /// <summary>
        /// The ID of the floor material that was denied for update.
        /// </summary>
        public int Floor;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            Floor = im.ReadInt32();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(Floor);
        }
    }

    /// <summary>
    /// Represents a packet sent to the server to notify about a skybox update, or sent from the server to the client to notify about another players skybox update.
    /// </summary>
    public struct EditorSkyboxPacket : IPacket
    {
        /// <summary>
        /// The SteamID of the client making the request.
        /// </summary>
        public ulong SteamID;

        /// <summary>
        /// The ID of the skybox to apply.
        /// </summary>
        public int Skybox;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            SteamID = im.ReadUInt64();
            Skybox = im.ReadInt32();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(SteamID);
            om.Write(Skybox);
        }
    }

    /// <summary>
    /// Represents a packet sent by the server to deny a skybox update in the editor.
    /// </summary>
    public struct EditorSkyboxDeniedPacket : IPacket
    {
        /// <summary>
        /// The ID of the skybox that was denied for update.
        /// </summary>
        public int Skybox;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            Skybox = im.ReadInt32();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(Skybox);
        }
    }

    /// <summary>
    /// Represents a packet sent to the server to request the selection of a block in the editor.
    /// </summary>
    public struct EditorSelectionPacket : IPacket
    {
        /// <summary>
        /// The SteamID of the client making the selection request.
        /// </summary>
        public ulong SteamID;

        /// <summary>
        /// The unique identifier (UID) of the block to select.
        /// </summary>
        public string UID;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            SteamID = im.ReadUInt64();
            UID = im.ReadString();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(SteamID);
            om.Write(UID);
        }
    }

    /// <summary>
    /// Represents a packet sent by the server to deny the selection of a block in the editor.
    /// </summary>
    public struct EditorSelectionDeniedPacket : IPacket
    {
        /// <summary>
        /// The unique identifier (UID) of the block that was denied for selection.
        /// </summary>
        public string UID;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            UID = im.ReadString();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(UID);
        }
    }

    /// <summary>
    /// Represents a packet sent to the server to request the deselection of a block in the editor.
    /// </summary>
    public struct EditorDeselectionPacket : IPacket
    {
        /// <summary>
        /// The SteamID of the client making the deselection request.
        /// </summary>
        public ulong SteamID;

        /// <summary>
        /// The unique identifier (UID) of the block to deselect.
        /// </summary>
        public string UID;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            SteamID = im.ReadUInt64();
            UID = im.ReadString();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(SteamID);
            om.Write(UID);
        }
    }

    /// <summary>
    /// Represents a packet sent from the server to notify that a player has left the session.
    /// </summary>
    public struct PlayerLeftPacket : IPacket
    {
        /// <summary>
        /// The SteamID of the player who left the session.
        /// </summary>
        public ulong SteamID;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            SteamID = im.ReadUInt64();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(SteamID);
        }
    }

    /// <summary>
    /// Represents a packet containing the current state of a player, including position, rotation, and mode.
    /// </summary>
    public struct PlayerStatePacket : IPacket
    {
        /// <summary>
        /// The SteamID of the player.
        /// </summary>
        public ulong SteamID;

        /// <summary>
        /// The X-coordinate of the player's position.
        /// </summary>
        public float PositionX;

        /// <summary>
        /// The Y-coordinate of the player's position.
        /// </summary>
        public float PositionY;

        /// <summary>
        /// The Z-coordinate of the player's position.
        /// </summary>
        public float PositionZ;

        /// <summary>
        /// The X-component of the player's rotation in Euler angles.
        /// </summary>
        public float EulerX;

        /// <summary>
        /// The Y-component of the player's rotation in Euler angles.
        /// </summary>
        public float EulerY;

        /// <summary>
        /// The Z-component of the player's rotation in Euler angles.
        /// </summary>
        public float EulerZ;

        /// <summary>
        /// The player's current mode (e.g., build, race, etc.).
        /// </summary>
        public byte Mode;

        /// <summary>
        /// Deserializes the packet data from the incoming message.
        /// </summary>
        /// <param name="im">The incoming message containing serialized data.</param>
        public void Deserialize(NetIncomingMessage im)
        {
            SteamID = im.ReadUInt64();
            PositionX = im.ReadFloat();
            PositionY = im.ReadFloat();
            PositionZ = im.ReadFloat();
            EulerX = im.ReadFloat();
            EulerY = im.ReadFloat();
            EulerZ = im.ReadFloat();
            Mode = im.ReadByte();
        }

        /// <summary>
        /// Serializes the packet data into the outgoing message.
        /// </summary>
        /// <param name="om">The outgoing message to populate with serialized data.</param>
        public void Serialize(NetOutgoingMessage om)
        {
            om.Write(SteamID);
            om.Write(PositionX);
            om.Write(PositionY);
            om.Write(PositionZ);
            om.Write(EulerX);
            om.Write(EulerY);
            om.Write(EulerZ);
            om.Write(Mode);
        }
    }
}
