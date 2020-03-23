using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleTargetObject : MonoBehaviour
{
    public void ToggleObject(GameObject gameObject)
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
