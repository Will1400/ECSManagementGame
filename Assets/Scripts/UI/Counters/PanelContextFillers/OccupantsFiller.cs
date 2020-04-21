using UnityEngine;
using System.Collections;
using Unity.Entities;
using System.Collections.Generic;

public class OccupantsFiller : PanelFiller
{
    [SerializeField]
    private GameObject occupantPrefab;
    [SerializeField]
    private Transform contentHolder;

    public override void Fill(Entity entity, bool keepUpdated = true)
    {
        base.Fill(entity, keepUpdated);

        foreach (Transform item in contentHolder)
        {
            Destroy(item.gameObject);
        }

        if (EntityManager.HasComponent<CitizenElement>(entity))
        {
            UpdateContent();
        }
    }

    protected override void UpdateContent()
    {
        if (!EntityManager.Exists(entity))
            return;

        if (EntityManager.HasComponent<CitizenElement>(entity))
        {
            var buffer = EntityManager.GetBuffer<CitizenElement>(entity);

            int numOfItemsToCreate = buffer.Length - (contentHolder.childCount - 1);
            for (int r = 0; r <= numOfItemsToCreate; r++)
            {
                GameObject itemObject = Instantiate(occupantPrefab);
                itemObject.transform.SetParent(contentHolder);
                itemObject.transform.localScale = Vector3.one;
            }

            //_ = contentHolder.childCount;
            int i = 0;
            foreach (Transform item in contentHolder)
            {
                if (i < buffer.Length)
                {
                    Citizen citizen = buffer[i].Value;

                    var itemController = item.GetComponent<OccupantItem>();
                    itemController.Initialize(citizen);
                }
                else
                {
                    Destroy(item.gameObject);
                }

                i++;
            }
            //_ = contentHolder.childCount;

        }
    }
}
