using UnityEngine;
using System.Collections;

public abstract class GraphicsControllerBase : MonoBehaviour
{
    protected int currentLevel;

    public abstract void PresetChanged(int level);
}
