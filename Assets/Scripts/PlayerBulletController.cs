using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletController : MonoBehaviour
{
    public float speed, zLimit; //speed of the bullet, and the limit of the bullet in the z axis
    [SerializeField] protected float hp;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);    
        
        //if the bullet is out of the screen, destroy it
        if (transform.position.z > zLimit)
        {
            Destroy(gameObject);
        }

        
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, speed * Time.deltaTime))
        {
            if (hit.collider.gameObject.CompareTag("Enemy")) // Replace "Enemy" with your enemy tag
            {
                GameObject enemy = hit.collider.gameObject;
                enemy.GetComponent<EnemyController>().hp -= 1;
                Destroy(gameObject);
            }
        }
        
    }
}
