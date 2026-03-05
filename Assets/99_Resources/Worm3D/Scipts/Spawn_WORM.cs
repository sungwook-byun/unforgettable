using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_WORM : MonoBehaviour
{
    public float delay;
    float timer;
    public int max;
    public int worm_count = 0;
    public GameObject worms;
    public float max_pos;
    public float min_pos;
    public float delaycount;
    // Start is called before the first frame update
    void Start()
    {
        if(max >= 1000)
        {
            max = 1000;
        }
        if(delay <= 0.001f)
        {
            delay = 0.001f;
        }
        
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
        timer += 1f * Time.deltaTime;
        delay -= delaycount * Time.deltaTime;
        if(timer >= delay)
        {
            if (worm_count <= max)
            {
                Spawn();
            }
        }
    }
    void Spawn()
    {
        Instantiate(worms, new Vector3(Random.Range(max_pos, min_pos), 10, Random.Range(max_pos, min_pos)), Quaternion.identity);
        worm_count += 1;
        Debug.Log("Worm");
        timer = 0;
    }
}
