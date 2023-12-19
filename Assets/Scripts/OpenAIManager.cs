using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Sockets;
using UnityEngine;

using System.Threading;

public class OpenAIManager : MonoBehaviour
{
    public string HandlerIP = "127.0.0.1";
    public int HandlerPort = 9438;
    private Socket socket, gatherThoughtsSocket;
    public bool connected = false;

    private Thread responseThread;
    public Queue<string> responseQueue = new Queue<string>();

    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        gatherThoughtsSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(HandlerIP, HandlerPort);
        gatherThoughtsSocket.Connect(HandlerIP, HandlerPort + 1);
        connected = true;
        Debug.Log("Connected to OpenAI Handler");
        responseThread = new Thread(QueueResponses);
        responseThread.Start();
    }

    void OnDestory()
    {
        if (responseThread != null && responseThread.IsAlive) responseThread.Abort();
        socket.Close();
        gatherThoughtsSocket.Close();
    }

    private void QueueResponses()
    {
        while(true)
        {
            byte[] response_length = new byte[4];
            socket.Receive(response_length);
            int response_length_int = System.BitConverter.ToInt32(response_length, 0);
            byte[] response = new byte[response_length_int];
            socket.Receive(response);
            while (response.Length < response_length_int) socket.Receive(response, response.Length, response_length_int - response.Length, SocketFlags.None);
            string response_str = System.Text.Encoding.ASCII.GetString(response);
            responseQueue.Enqueue(response_str);
        }
    }

    public void SendString(string str)
    {
        byte[] buffer = System.Text.Encoding.ASCII.GetBytes(str);
        int buffer_length = buffer.Length;
        byte[] length = System.BitConverter.GetBytes(buffer_length);
        socket.Send(length);
        socket.Send(buffer);
    }

    public string GatherThoughts(string str)
    {
        byte[] buffer = System.Text.Encoding.ASCII.GetBytes(str);
        int buffer_length = buffer.Length;
        byte[] length = System.BitConverter.GetBytes(buffer_length);
        gatherThoughtsSocket.Send(length);
        gatherThoughtsSocket.Send(buffer);
        byte[] response_length = new byte[4];
        gatherThoughtsSocket.Receive(response_length);
        int response_length_int = System.BitConverter.ToInt32(response_length, 0);
        byte[] response = new byte[response_length_int];
        gatherThoughtsSocket.Receive(response);
        while (response.Length < response_length_int) gatherThoughtsSocket.Receive(response, response.Length, response_length_int - response.Length, SocketFlags.None);
        string response_str = System.Text.Encoding.ASCII.GetString(response);
        return response_str;
    }
}