using System;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SerialPortReader : MonoBehaviour
{
    public string portName = "/dev/ttyUSB0";  
    public int baudRate = 9600;       

    private SerialPort serialPort;
    private Thread readThread;
    private bool isRunning;

    // Buffer to store received data
    private string receivedData = string.Empty;

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
        if (!string.IsNullOrEmpty(receivedData))
        {
            Debug.Log($"Received Data: {receivedData}");
            receivedData = string.Empty;  
        }
    }

    void ReadSerialData()
    {
        while (isRunning && serialPort.IsOpen)
        {
            try
            {
                string data = serialPort.ReadLine();
                if (!string.IsNullOrEmpty(data))
                {
                    receivedData = data;
                }
            }
            catch (TimeoutException)
            {
                
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
    }
}
