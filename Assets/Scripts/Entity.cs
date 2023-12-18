using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Entity : MonoBehaviour
{
    public string title;
    public string history;
    public int memoryCapacity;
    public float health;
    private float age;
    public float exactLifeExpectancy, approximateLifeExpectancy;
    public bool conscious, movable;
    public float thoughtFrequency;

    private Dictionary<string, Entity> interactionQueue = new Dictionary<string, Entity>();

    void Start()
    {
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
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Entity otherEntity = other.GetComponent<Entity>();
        string otherTitle = otherEntity.title;
        if (otherEntity != null && !interactionQueue.ContainsKey(otherTitle))
        {
            interactionQueue.Add(otherTitle, otherEntity);
            Debug.Log("Entity " + title + " has encountered " + otherTitle);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Entity otherEntity = other.GetComponent<Entity>();
        string otherTitle = otherEntity.title;
        if (otherEntity != null && interactionQueue.ContainsKey(otherTitle))
        {
            interactionQueue.Remove(otherTitle);
            Debug.Log("Entity " + title + " has left " + otherTitle);
        }
    }

    void OnDestory()
    {
        SaveState();
        StopCoroutine(Life());
    }

    private bool LoadState()
    {
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
        string json = JsonUtility.ToJson(this);
        FileManager.WriteToFile(title + ".json", json);
    }

    IEnumerator Life()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f / thoughtFrequency);
            if (!conscious)
            {
                if (history.Length > memoryCapacity)
                {
                    ShortenHistory();
                }
            }
            else
            {
                Think();
                if (history.Length > memoryCapacity)
                {
                    ShortenHistory();
                }
            }
        }
    }

    private void ShortenHistory()
    {
        //TODO: faint some memories to keep the length of history below memoryCapacity
    }

    private void Think()
    {
        string thought = history;
        thought += "Currently, I am " + Utils.HumanTime(age) + " old and my species has an approximate life expectancy of " + Utils.HumanTime(approximateLifeExpectancy) + ". ";
        thought += "However, my health, that is, " + health + " (range of health is from 0.0 to 1.0), is also a factor in my life expectancy.";
        thought += "I just have encountered ";
        int i=0;
        foreach (KeyValuePair<string, Entity> entry in interactionQueue)
        {
            if (i != interactionQueue.Count - 1)
            {
                thought += entry.Key + " at cartesian coordinates (" + entry.Value.transform.position.x + ", " + entry.Value.transform.position.y + ", " + entry.Value.transform.position.z + "), ";
            }
            else
            {
                thought += "and " + entry.Key + " at cartesian coordinates (" + entry.Value.transform.position.x + ", " + entry.Value.transform.position.y + ", " + entry.Value.transform.position.z + "). ";
            }
        }
        history = thought;
        thought += "\n" + "Given who I am, tell me the next task(s) I should do. Please be specific, don't repeat anything I have said already, and only use first-person perspective (e.g. \"I should eat an apple\"). ";
        OpenAIManager.SubmitThought(this, thought);
    }
}
