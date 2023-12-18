using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Sockets;
using UnityEngine;

public class Thought
{
    public Entity entity;
    public string thought;

    public Thought(Entity entity, string thought)
    {
        this.entity = entity;
        this.thought = thought;
    }
}
public class OpenAIManager : MonoBehaviour
{
    public string HandlerIP = "127.0.0.1";
    public int HandlerPort = 9438;
    private Socket socket;
    public static Queue<Thought> thoughtQueue = new Queue<Thought>();

    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(HandlerIP, HandlerPort);
        Debug.Log("Connected to OpenAI Handler");
    }

    void Update()
    {
        if (thoughtQueue.Count > 0)
        {
            Thought thought = thoughtQueue.Dequeue();
            //TODO: send thought to OpenAI and get response
        }
    }

    void OnDestory()
    {
        socket.Close();
    }
    
    public static void SubmitThought(Entity entity, string thought)
    {
        thoughtQueue.Enqueue(new Thought(entity, thought));
    }
}