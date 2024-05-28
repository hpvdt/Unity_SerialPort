using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SerialPortReader : MonoBehaviour
{
    public string portName = "/dev/tty.usbserial-1120";  // Set the port name (e.g., /dev/ttyUSB0 for Linux or /dev/tty.usbserial-* for Mac)
    public int baudRate = 115200;             // Set the baud rate

    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning;

    private byte[] buffer = new byte[4];  // Larger buffer to read multiple floats
    private int bufferIndex = 0;
    private object bufferLock = new object();

    private List<float> floatList = new List<float>();  // List to store the received floats

    void Start()
    {
        serialPort = new SerialPort(portName, baudRate);
        serialPort.ReadTimeout = 50;
        serialPort.WriteTimeout = 50;

        try
        {
            serialPort.Open();
            isRunning = true;
            readThread = new Thread(ReadSerialData);
            readThread.Start();
            Debug.Log("Serial port opened successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error opening serial port: {e.Message}");
        }
    }

    void Update()
    {
        lock (bufferLock)
        {
            while (bufferIndex >= 4)
            {
                float receivedFloat = BitConverter.ToSingle(buffer, 0);
                Debug.Log($"Received Float: {receivedFloat}");

                // Add the received float to the list
                floatList.Add(receivedFloat);

                // Shift the buffer
                Array.Copy(buffer, 4, buffer, 0, bufferIndex - 4);
                bufferIndex -= 4;
            }
        }
    }

    void ReadSerialData()
    {
        while (isRunning && serialPort.IsOpen)
        {
            try
            {
                int bytesRead = serialPort.Read(buffer, bufferIndex, buffer.Length - bufferIndex);
                if (bytesRead > 0)
                {
                    lock (bufferLock)
                    {
                        bufferIndex += bytesRead;
                    }
                }
            }
            catch (TimeoutException)
            {
                // Handle the timeout exception if needed
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading from serial port: {e.Message}");
            }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join();
        }

        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }

        Debug.Log("Serial port closed.");

        // Optionally, log all the collected floats
        foreach (var value in floatList)
        {
            Debug.Log(value);
        }
    }

    public float[] GetReceivedFloats()
    {
        lock (bufferLock)
        {
            return floatList.ToArray();
        }
    }
}
