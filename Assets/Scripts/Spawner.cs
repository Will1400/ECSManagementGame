using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    GameObject prefabToSpawn;
    [SerializeField]
    int amountToSpawn;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            for (int i = 0; i < amountToSpawn; i++)
            {
                Instantiate(prefabToSpawn);
            }
        }
    }
}
