using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CutWheelRotation : MonoBehaviour
{
    private float speed = 150;
    int damageAmount = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, -1 * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject.FindWithTag("Player").GetComponent<PlayerProfile>().GetDamage(damageAmount);
        }
    }
}
