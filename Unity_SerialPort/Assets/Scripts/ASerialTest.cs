using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SerialPortReader : MonoBehaviour
{
    // Serial port settings
    public string portName = "/dev/tty.usbserial-1120";  
    public int baudRate = 115200;           

    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning;

    // Buffer to store received data
    private byte[] buffer = new byte[4]; // A float is 4 bytes
    private float receivedFloat = 0.0f;

    void Start()
    {
        // Initialize the serial port
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
        // Use the received data
        if (receivedFloat != 0.0f)
        {
            Debug.Log($"Received Float: {receivedFloat}");
            receivedFloat = 0.0f;  // Reset the received value after processing
        }
    }

    void ReadSerialData()
    {
        while (isRunning && serialPort.IsOpen)
        {
            try
            {
                int bytesRead = serialPort.Read(buffer, 0, 4);
                if (bytesRead == 4)
                {
                    receivedFloat = BitConverter.ToSingle(buffer, 0);
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
        // Clean up the serial port
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
    }
}
