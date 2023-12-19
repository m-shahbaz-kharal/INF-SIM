using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using TMPro;
using UnityEngine;

[Serializable, RequireComponent(typeof(OpenAIManager))]
public class Entity : MonoBehaviour
{
    private static int nextId = 0;
    private int id = nextId++;
    public string title;
    public string history;
    public int memoryCapacity;
    public float health;
    public float age;
    public float approximateLifeExpectancy, exactLifeExpectancy;
    public bool conscious, movable;
    public float thoughtFrequency;

    private Dictionary<int, Entity> interactionQueue = new Dictionary<int, Entity>();

    private OpenAIManager mOpenAIManager;

    public TextMeshProUGUI textUI;

    private IEnumerator UpdateUI(string text)
    {
        try {StopCoroutine("UpdateUI");} catch (Exception) {}
        textUI.text = title + ":" + text;
        yield return new WaitForSeconds(5.0f);
        textUI.text = "";
    }

    void Start()
    {
        mOpenAIManager = GetComponent<OpenAIManager>();
        if(!LoadState())
        {
            string past = "I am " + title + ". ";
            past += "I am " + (conscious ? "a conscious" : "an unconscious") + " being that is " + (movable ? "able" : "unable") + " to move. ";
            past += "My past can be described as:\n" + history + "\n";
            past += "Each event in my past has shaped me into who I am today.\n";
            history = past;
        }
        StartCoroutine(Life());
    }

    void FixedUpdate()
    {
        age += Time.fixedDeltaTime;
        if (age > exactLifeExpectancy || health <= 0.0f)
        {
            StartCoroutine(UpdateUI("I am dead."));
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Entity otherEntity = other.GetComponent<Entity>();
        if (otherEntity != null && !interactionQueue.ContainsKey(otherEntity.id))
        {
            string otherTitle = otherEntity.title;
            interactionQueue.Add(otherEntity.id, otherEntity);
            Debug.Log("Entity " + title + " has encountered " + otherTitle);
            StartCoroutine(UpdateUI("I have encountered " + otherTitle));
        }
    }

    void OnTriggerExit(Collider other)
    {
        Entity otherEntity = other.GetComponent<Entity>();
        if (otherEntity != null && interactionQueue.ContainsKey(otherEntity.id))
        {
            string otherTitle = otherEntity.title;
            interactionQueue.Remove(otherEntity.id);
            Debug.Log("Entity " + title + " has left " + otherTitle);
            StartCoroutine(UpdateUI("I have left " + otherTitle));
        }
    }

    void OnDestory()
    {
        SaveState();
        StopCoroutine(Life());
    }

    private bool LoadState()
    {
        StartCoroutine(UpdateUI("Loading State ..."));
        string json;
        if (FileManager.LoadFromFile(title + ".json", out json))
        {
            JsonUtility.FromJsonOverwrite(json, this);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void SaveState()
    {
        StartCoroutine(UpdateUI("Saving State ..."));
        string json = JsonUtility.ToJson(this);
        FileManager.WriteToFile(title + ".json", json);
    }

    IEnumerator Life()
    {
        while (mOpenAIManager.connected == false) yield return new WaitForSeconds(0.1f);
        while (true)
        {
            yield return new WaitForSeconds(1.0f / thoughtFrequency);
            StartCoroutine(UpdateUI("Thinking ..."));
            string current_thought = conscious ? ConsciousThink() : UnconsciousThink();
            mOpenAIManager.SendString(current_thought);
            Debug.Log("Entity " + title + " is thinking: " + current_thought);
            while (mOpenAIManager.responseQueue.Count == 0) yield return new WaitForSeconds(1.0f);
            string response = mOpenAIManager.responseQueue.Dequeue();
            Debug.Log("Entity " + title + " has received a response: " + response);
            StartCoroutine(UpdateUI("Acting ..."));
            Act(response);
            if (history.Length > memoryCapacity) ShortenHistory();
        }
    }

    private void ShortenHistory()
    {
        //TODO: faint some memories to keep the length of history below memoryCapacity
    }

    private string UnconsciousThink()
    {
        string thought = history;
        thought += "\n" + "Given my history, how will I get affected. Please be specific, don't repeat anything I have said already, and only use first-person perspective (e.g. \"because I am an apple and someone ate a part of me, my health is lowered\"). ";
        return thought;
    }

    private string ConsciousThink()
    {
        List<int> keysToRemove = new List<int>();
        foreach (KeyValuePair<int, Entity> entry in interactionQueue)
        {
            if (entry.Value == null) keysToRemove.Add(entry.Key);
        }
        foreach (int key in keysToRemove) interactionQueue.Remove(key);
        string thought = history;
        thought += "I am perceiving " + interactionQueue.Count + " other entities around me. ";
        if(interactionQueue.Count == 1)
        {
            thought += "The other entity is ";
        }
        else
        {
            thought += "The other entities are ";
        }
        int i=0;
        foreach (KeyValuePair<int, Entity> entry in interactionQueue)
        {
            if (i == interactionQueue.Count - 1 && i != 0)
            {
                thought += "and ";
            }
            thought += entry.Value.title;
            if (i != interactionQueue.Count - 1)
            {
                thought += ", ";
            }
            i++;
        }
        thought += "\n" + "Given who I am and what I am perceiving now, tell me the next task(s) I should do. Please be specific, don't repeat anything I have said already, and only use first-person perspective (e.g. \"I should eat an apple\"). ";
        return thought;
    }

    public void Act(string response)
    {
        if (response.Equals("OK"))
        {
            Debug.Log("Entity " + title + " has received an OK response");
            return;
        }
        string[] parts = response.Split("<->");
        switch(parts[0])
        {
            case "move":
                Dictionary<string, string> move_dict = new Dictionary<string, string>
                {
                    { parts[1].Split(":")[0], parts[1].Split(":")[1] },
                    { parts[2].Split(":")[0], parts[2].Split(":")[1] },
                    { parts[3].Split(":")[0], parts[3].Split(":")[1] }
                };
                float x = float.Parse(move_dict["position_x"]);
                float y = float.Parse(move_dict["position_y"]);
                float z = float.Parse(move_dict["position_z"]);
                if (movable)
                {
                    transform.position = new Vector3(x, y, z);
                    StartCoroutine(UpdateUI("[Moving] " + x + ", " + y + ", " + z));
                    mOpenAIManager.SendString("success");
                }
                else
                {
                    StartCoroutine(UpdateUI("[Moving] I am unable to move"));
                    mOpenAIManager.SendString("failed because the entity is unable to move.");
                }
                break;
            case "affect_self":
                Dictionary<string, string> affect_self_dict = new Dictionary<string, string>
                {
                    { parts[1].Split(":")[0], parts[1].Split(":")[1] },
                    { parts[2].Split(":")[0], parts[2].Split(":")[1] },
                    { parts[3].Split(":")[0], parts[3].Split(":")[1] }
                };
                string added_history = affect_self_dict["add_to_history"];
                float added_health = float.Parse(affect_self_dict["add_to_health"]);
                bool affected_movable = bool.Parse(affect_self_dict["movability"]);
                history += added_history;
                health += added_health;
                movable = conscious ? affected_movable : movable;
                StartCoroutine(UpdateUI("[Affecting][Self] " + added_history + " [Health] " + added_health + " [Movable] " + movable));
                mOpenAIManager.SendString("success");
                break;
            case "affect_other":
                Dictionary<string, string> affect_other_dict = new Dictionary<string, string>
                {
                    { parts[1].Split(":")[0], parts[1].Split(":")[1] },
                    { parts[2].Split(":")[0], parts[2].Split(":")[1] }
                };
                int other_id = int.Parse(affect_other_dict["entity_id"]);
                string other_history = affect_other_dict["add_to_history"];
                if (interactionQueue.ContainsKey(other_id))
                {
                    interactionQueue[other_id].history += other_history;
                    StartCoroutine(UpdateUI("[Affecting][" + interactionQueue[other_id].title + "] " + other_history));
                    mOpenAIManager.SendString("success");
                }
                else
                {
                    mOpenAIManager.SendString("failed because the entity is not there anymore.");
                }
                break;
            case "response":
                history += parts[1];
                StartCoroutine(UpdateUI("[History Update] " + parts[1]));
                break;
            case "query":
                Dictionary<string, string> query_dict = new Dictionary<string, string>
                {
                    { parts[1].Split(":")[0], parts[1].Split(":")[1] }
                };
                string query = query_dict["query"];
                if(query.Equals("history"))
                {
                    mOpenAIManager.SendString(history);
                }
                else if(query.Equals("health"))
                {
                    mOpenAIManager.SendString("in range from 0.0 to 1.0, " + health + " is my current health.");
                }
                else if(query.Equals("age"))
                {
                    mOpenAIManager.SendString(Utils.HumanTime(age));
                }
                else if(query.Equals("location"))
                {
                    mOpenAIManager.SendString("in cartesian coordinates (" + transform.position.x + ", " + transform.position.y + ", " + transform.position.z + ")");
                }
                else if(query.Equals("movability"))
                {
                    mOpenAIManager.SendString(movable.ToString());
                }
                else if(query.Equals("consciousness"))
                {
                    mOpenAIManager.SendString(conscious.ToString());
                }
                else if(query.Equals("life_expectancy"))
                {
                    mOpenAIManager.SendString(Utils.HumanTime(approximateLifeExpectancy));
                }
                else
                {
                    mOpenAIManager.SendString("failed because the query is not recognized.");
                }
                break;
            default:
                break;
        }
    }
}
