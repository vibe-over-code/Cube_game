using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class proceduralgen : MonoBehaviour
{
    [SerializeField]
    GameObject floor;
    public Transform spawn;
    public Transform spawn1;
    public Color color1 = new Color(0.2F, 0.3F, 0.4F, 0.5F);
    public GameObject barrel;
    public GameObject deadlyObstacle; // Префаб ловушки

    GameObject Player;
    Commutator com;

    void Start()
    {
        com = GameObject.FindGameObjectWithTag("GameController").GetComponent<Commutator>();
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        float Dist_Player = Vector3.Distance(Player.transform.position, gameObject.transform.position);
        if (Dist_Player > 50)
        {
            com.GameOver.SetActive(true);
            Time.timeScale = 0f; // Остановить игру
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            com.levelUp();
            com.count++;

            int rnd = Random.Range(0, 10);
            if (rnd >= 7)
            {
                Instantiate(deadlyObstacle, spawn1.transform.position, Quaternion.identity);
            }
            else if (rnd >= 4)
            {
                Instantiate(barrel, spawn1.transform.position, Quaternion.identity);
            }

            color1 = new Color(Random.Range(0.1f, 1), Random.Range(0.1f, 1), Random.Range(0.1f, 1), Random.Range(0.1f, 1));
            floor.GetComponent<SpriteRenderer>().color = color1;
            Instantiate(floor, spawn.transform.position, Quaternion.identity);
            Destroy(floor);
        }
    }
}
