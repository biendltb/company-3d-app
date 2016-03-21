using System;
using System.Net.Sockets;

namespace TIS_3dAntiCollision.Core
{
    class SensorConnection
    {
        private TcpClient tcpClient;
        private byte[] receivedData;

        public byte[] ReceivedData
        {
            get { return receivedData; }
        }

        public SensorConnection()
        {
            receivedData = new byte[8192];
        }

        /// <summary>
        /// Connect to the sensor
        /// </summary>
        /// <param name="ip">Sensor IP address</param>
        /// <param name="port">Sensor TCP port</param>
        /// <returns>Returns the result of connection</returns>
        public bool Connect(string ip, int port)
        {
            tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(ip, port);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
            }

            return tcpClient.Connected;
        }

        /// <summary>
        /// Close the connection
        /// </summary>
        /// <returns>Returns whether the connection is closed or not</returns>
        public bool Disconnect()
        {
            if (tcpClient.Connected)
            {
                tcpClient.GetStream().Close();
                tcpClient.Close();
            }

            return !tcpClient.Connected;
        }

        /// <summary>
        /// Write the command to sensor
        /// </summary>
        /// <param name="command">Sensor command</param>
        /// <returns>Returns whether the command is writed</returns>
        public bool WriteSensor(string command)
        {
            char stx = (char)2;
            char etx = (char)3;
            string cmd_string = stx + command + etx;
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(cmd_string);

            try
            {
                tcpClient.GetStream().Write(outStream, 0, outStream.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Receive data from sensor
        /// </summary>
        /// <returns>Returns whether data is received</returns>
        public bool ReadSensor()
        {
            try
            {
                tcpClient.GetStream().Read(receivedData, 0, (int)tcpClient.ReceiveBufferSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                return false;
            }

            return true;
        }


        public bool isConnect()
        {
            return tcpClient.Connected;
        }
    }
}
