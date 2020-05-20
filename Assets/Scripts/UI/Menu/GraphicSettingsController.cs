using UnityEngine;
using System.Collections;
using DG.Tweening;

public class GraphicSettingsController : MonoBehaviour
{
    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(1, .2f);
    }

    
}
