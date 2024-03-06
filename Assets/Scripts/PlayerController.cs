using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[SelectionBase]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private bool debug; private string debugTag = "PlayerController: ";

    [Header("Horizontal Movement")]
    [SerializeField] [Range(0, 1)] [Tooltip("Ratio of smooth moving: Player will move [Distance to touch / This] every second")] 
    private float smoothMoveRatio;
    [SerializeField] private float xStart, xRange; //Starting X in game world, and range of horizontal movement in game world
    private float xTarget; //Target X position of the player

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Vector3 bulletSpawnOffset;
    [SerializeField] private float shootCooldown, bulletSpeed, bulletZLimit;

    //Shooting coroutine
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

    void Start()
    {
        //Starts the shooting coroutine
        StartCoroutine(Shoot());
    }

    void Update()
    {
        #region //Moves horizontally based on touch position on the screen

        if (Input.touchCount > 0)
        {
            //Gets the last touch
            Touch touch = Input.GetTouch(Input.touchCount-1);
            
            //Gets touch X position and converts it to a relative position (0 - left, 1 - right)
            float relative_touch_x = touch.position.x / Screen.width;
            relative_touch_x = relative_touch_x - 0.5f; //shifts the relative position to be centered at 0

            //Calculates desired X position of the object in world using xRange property
            xTarget = xStart + xRange * relative_touch_x;

            //Smoothly moves the player to the target X position
            float step = Mathf.Abs(transform.position.x - xTarget) / (smoothMoveRatio / Time.deltaTime);

            transform.position = Vector3.MoveTowards(transform.position, new Vector3(xTarget, transform.position.y, transform.position.z), step);
        }

        #endregion
    }
}
