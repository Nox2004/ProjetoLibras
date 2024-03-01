using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float xStart, xRange; //starting X in game world, and range of horizontal movement in game world
    private float xTarget; //target X position of the player

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Vector3 bulletSpawnOffset;
    [SerializeField] private float shootCooldown, bulletSpeed, bulletZLimit;


    //shooting coroutine
    IEnumerator Shoot()
    {
        while (true)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position + bulletSpawnOffset, Quaternion.identity);
            PlayerBulletController bullet_controller = bullet.GetComponent<PlayerBulletController>();

            bullet_controller.speed = bulletSpeed;
            bullet_controller.zLimit = bulletZLimit;

            yield return new WaitForSeconds(shootCooldown);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Shoot());
    }

    // Update is called once per frame
    void Update()
    {
        #region //moves horizontally based on mouse position on the screen

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(Input.touchCount-1);
            Debug.Log("Touch Position: " + touch.position);
            //Horizontally moves the player to the touch position

            //Gets touch X position and converts it to a relative position (0 - left, 1 - right)
            float relative_touch_x = touch.position.x / Screen.width;
            relative_touch_x = relative_touch_x - 0.5f; //shifts the relative position to be centered at 0

            //desired X position of the object in world
            xTarget = xStart + xRange * relative_touch_x;

            //Moves the player to the target X position
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(xTarget, transform.position.y, transform.position.z), step);
        }

        #endregion
    }
}
