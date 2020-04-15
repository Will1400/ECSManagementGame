using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 10;
    [Range(0, 1), SerializeField]
    float zoomPercent = 1;
    [SerializeField]
    float zoomSpeed = 10;
    [SerializeField]
    float zoomSensitivity = 1;
    [SerializeField]
    float2 zoomMinMax = new float2(3, 10);

    void Update()
    {
        Move();

        Zoom();
    }

    void Move()
    {
        float3 input = new float3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        if (input.Equals(float3.zero))
            return;

        transform.Translate(math.normalize(input) * transform.position.y * Time.deltaTime, Space.World);
    }

    void Zoom()
    {
        float scroll = -Input.GetAxisRaw("Mouse ScrollWheel");
        zoomPercent += scroll * zoomSensitivity;
        zoomPercent = Mathf.Clamp01(zoomPercent);
        float height = MathHelper.Map(zoomPercent, 0, 1, zoomMinMax.x, zoomMinMax.y);
        transform.position = new float3(transform.position.x, math.lerp(transform.position.y, height, zoomSpeed * Time.deltaTime), transform.position.z);
    }
}
