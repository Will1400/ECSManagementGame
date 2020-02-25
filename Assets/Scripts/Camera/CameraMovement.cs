using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    float speed = 10;

    void Update()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        transform.Translate(input * speed * Time.deltaTime, Space.World);
    }
}
