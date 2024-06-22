using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    // Поле для хранения позиции спавна
    public Transform Bullet_Spawn;
    // Поле, хранящая префаб снаряда
    public GameObject Bullet;
    void Update()
    {
        // Создаем луч
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Рисуем красный луч от позиции спавна
        Debug.DrawRay(Bullet_Spawn.position, Bullet_Spawn.forward * 100, Color.red);
        // Нажимаем на ЛКМ
        if (Input.GetButtonDown("Fire1"))
        {
            // Создаем объект на основе префаба без вращений
            GameObject Shoot = Instantiate(Bullet, Bullet_Spawn.position, Quaternion.identity);
            Vector3 direction = Bullet_Spawn.forward;
            Debug.Log(direction);
            Shoot.GetComponent<Rigidbody>().velocity = direction*50;
        }
    }
}
