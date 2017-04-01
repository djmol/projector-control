using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectorControl
{
    public class RemoteCommander
    {
        // remoteData byte arrays are based on the NEC documentation on remote commands
        // The first two bytes identify the command, and the last byte is a checksum (which is not calculated here, but simply appended)
        private static readonly Dictionary<string, byte[]> remoteData = new Dictionary<string, byte[]>(17)
        {
            { "off", new byte[] { 0xdb, 0x00, 0xee }},
            { "on", new byte[] { 0x02, 0x00, 0x15 }},
            { "avMute", new byte[] { 0x13, 0x00, 0x26 }},
            { "comp1Input", new byte[] { 0x4b, 0x00, 0x5e }},
            { "comp2Input", new byte[] { 0x4c, 0x00, 0x5f }},
            { "freeze", new byte[] { 0x8a, 0x00, 0x9d }},
            { "picture", new byte[] { 0x29, 0x00, 0x3c }},
            { "volDown", new byte[] { 0x85, 0x00, 0x98 }},
            { "volUp", new byte[] { 0x84, 0x00, 0x97 }},
            { "menu", new byte[] { 0x06, 0x00, 0x19 }},
            { "enter", new byte[] { 0x0b, 0x00, 0x1e }},
            { "exit", new byte[] { 0x0c, 0x00, 0x1f }},
            { "up", new byte[] { 0x07, 0x00, 0x1a }},
            { "down", new byte[] { 0x08, 0x00, 0x1b }},
            { "left", new byte[] { 0x0a, 0x00, 0x1d }},
            { "right", new byte[] { 0x09, 0x00, 0x1c }},
            { "source", new byte[] { 0xd7, 0x00, 0xea }}
        };

        // remoteHeader enables NEC projectors to identify a packet as a remote command
        private static readonly byte[] remoteHeader = { 0x02, 0x0f, 0x00, 0x00, 0x02 };

        private byte[] command;

        public RemoteCommander()
        {
            command = new byte[8];
        }

        // Accepts ID of remote command and packages it as a byte array (header + data)
        public void SetCommand(string commandId)
        {
            remoteHeader.CopyTo(command, 0);
            remoteData[commandId].CopyTo(command, remoteHeader.Length);
        }

        public byte[] GetCommand()
        {
            return command;
        }

    }
}