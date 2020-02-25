using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    public CursorState CursorState;

    private void Awake()
    {
        if (Instance is null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}
