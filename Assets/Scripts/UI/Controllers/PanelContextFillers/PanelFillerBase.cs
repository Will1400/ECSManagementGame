using UnityEngine;
using System.Collections;
using Unity.Entities;

public abstract class PanelFillerBase : MonoBehaviour
{
    protected EntityManager EntityManager;
    protected Entity entity;
    protected bool keepUpdated;

    protected float updateRate = 1;
    protected float nextUpdate;

    public virtual void Fill(Entity entity, bool keepUpdated = true)
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        this.entity = entity;
        this.keepUpdated = keepUpdated;
    }

    protected virtual void Update()
    {
        if (keepUpdated && Time.time >= nextUpdate)
        {
            UpdateContent();
            nextUpdate = Time.time + updateRate;
        }
    }

    protected abstract void UpdateContent();
}
