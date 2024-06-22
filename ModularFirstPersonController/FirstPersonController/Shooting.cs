using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    // ���� ��� �������� ������� ������
    public Transform Bullet_Spawn;
    // ����, �������� ������ �������
    public GameObject Bullet;
    void Update()
    {
        // ������� ���
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // ������ ������� ��� �� ������� ������
        Debug.DrawRay(Bullet_Spawn.position, Bullet_Spawn.forward * 100, Color.red);
        // �������� �� ���
        if (Input.GetButtonDown("Fire1"))
        {
            // ������� ������ �� ������ ������� ��� ��������
            GameObject Shoot = Instantiate(Bullet, Bullet_Spawn.position, Quaternion.identity);
            Vector3 direction = Bullet_Spawn.forward;
            Debug.Log(direction);
            Shoot.GetComponent<Rigidbody>().velocity = direction*50;
        }
    }
}
