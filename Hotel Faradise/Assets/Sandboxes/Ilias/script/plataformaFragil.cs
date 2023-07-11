using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class plataformaFragil : MonoBehaviour
{
    Rigidbody2D rb;
    BoxCollider2D bc;
    public float timeToDrop = 0.5f;
    private float timeToDestroy = 4f;
    private bool alreadyFallen = false;
    public float spawnTime = 2f;
    public GameObject plataformaPrefab;
    private Vector2 positionSpawn;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        positionSpawn = new Vector2(transform.position.x, transform.position.y);
        rb.isKinematic = true;
        bc.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter2D(Collision2D col) {
        if(!alreadyFallen && col.gameObject.tag.Equals("Player") ){
            alreadyFallen = true;
            Invoke("SpawnPlatform", spawnTime);
            Invoke("DropPlatform", timeToDrop);
            Invoke("DestroyObject", timeToDestroy);

        }
    }

    void DropPlatform(){
        rb.isKinematic = false;
        bc.enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);

    }
    
    void SpawnPlatform()
    {
        var newPlat = GameObject.Instantiate(plataformaPrefab, positionSpawn, Quaternion.identity);
        newPlat.name = "NewPlatform";
    }

    void DestroyObject(){
        Destroy(gameObject);
    }
}
