using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScreenBehavior : MonoBehaviour {

    public GameObject[] foods;
    public Transform foodPosition;

    public GameObject[] enemies;
    public Transform enemyPosition;

    public GameObject[] weapons;
    public Transform weaponPosition;

    void Start ()
    {
        spawnEnemy();
        spawnFood();
        spawnWeapon();
	}

    private void spawnEnemy()
    {
        if (GameObject.Find("Enemy") == null)
        {
            GameObject enemy = Instantiate(enemies[Random.Range(0, enemies.Length)], enemyPosition.position, Quaternion.identity);
            enemy.name = "Enemy";
        }

        Invoke("spawnEnemy", 2.5f);
    }

    private void spawnFood()
    {
        if (GameObject.Find("Food") == null)
        {
            GameObject food = Instantiate(foods[Random.Range(0, foods.Length)], foodPosition.position, Quaternion.identity);
            food.name = "Food";
        }

        Invoke("spawnFood", 2.5f);
    }

    private void spawnWeapon()
    {
        if (GameObject.Find("Weapon") == null)
        {
            GameObject weapon = Instantiate(weapons[Random.Range(0, weapons.Length)], weaponPosition.position, Quaternion.identity);
            weapon.name = "Weapon";
        }

        Invoke("spawnWeapon", 2.5f);
    }
}
