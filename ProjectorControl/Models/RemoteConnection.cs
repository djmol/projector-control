using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text;

namespace ProjectorControl
{
    public class RemoteConnection
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private BinaryWriter binaryWriter;
        private BinaryReader binaryReader;
        private static string ip;

        public RemoteConnection()
        {
            tcpClient = new TcpClient();
            tcpClient.NoDelay = true;
            tcpClient.LingerState = new LingerOption(true, 10);
        }

        public void Connect()
        {
            // Attempts to set up TCP connection with selected projector
            try
            {
                // 7142 is standard NEC port for accepting commands
                tcpClient.Connect(ip, 7142);
                networkStream = tcpClient.GetStream();
                binaryWriter = new BinaryWriter(networkStream, Encoding.UTF8);
                binaryReader = new BinaryReader(networkStream, Encoding.UTF8);
            }
            catch (SocketException e)
            {
                throw new Exception("Could not connect. Make sure the projector is plugged in, the power switch is turned on, and the projector is connected to the local network.", e);
            }
        }

        public void Write(byte[] command)
        {
            // Single-byte array so we can send it as a parameter to Convert.ToByte()
            byte[] incomingBytes = new byte[1];
            int attemptCount = 0;
            // If a remote command is successful, an NEC projector should return 22 as the first byte (A2 is error)
            while (incomingBytes[0] != 0x22 && attemptCount < 5)
            {
                // Write command to stream
                binaryWriter.Write(command);
                binaryWriter.Flush();
                // Read incoming (projector-sent) bytes from stream
                incomingBytes[0] = Convert.ToByte(binaryReader.Read());
                attemptCount++;
                if (attemptCount >= 5)
                {
                    // After five failed attempts to send the command, error message displayed in pop-up and command is aborted
                    throw new Exception("Command error. Projector unresponsive.");
                }
            }
        }

        public void Flush()
        {
            binaryWriter.Flush();
        }

        public void Close()
        {
            //tcpClient.ReceiveTimeout = 3000;
            binaryWriter.Close();
            networkStream.Close();
            tcpClient.Close();
        }

        public void SetIp(string deviceIp)
        {
            ip = deviceIp;
        }

        public string GetIp()
        {
            return ip;
        }
    }
}