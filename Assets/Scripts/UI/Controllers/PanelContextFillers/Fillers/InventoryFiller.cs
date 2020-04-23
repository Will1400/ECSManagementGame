using UnityEngine;
using System.Collections;
using Unity.Entities;
using System.Collections.Generic;
using System.Linq;

public class InventoryFiller : PanelFillerBase
{
    [SerializeField]
    private GameObject itemPrefab;
    [SerializeField]
    private Transform contentHolder;

    public override void Fill(Entity entity, bool keepUpdated = true)
    {
        base.Fill(entity, keepUpdated);

        foreach (Transform item in contentHolder)
        {
            Destroy(item.gameObject);
        }

        if (EntityManager.HasComponent<ResourceDataElement>(entity))
        {
            UpdateContent();
        }
    }

    protected override void UpdateContent()
    {
        if (!EntityManager.Exists(entity) || !EntityManager.HasComponent<ResourceDataElement>(entity))
            return;

        var buffer = EntityManager.GetBuffer<ResourceDataElement>(entity);

        Dictionary<string, int> items = new Dictionary<string, int>();

        for (int d = 0; d < buffer.Length; d++)
        {
            string key = buffer[d].Value.ResourceType.ToString();
            if (items.ContainsKey(key))
            {
                items[key]++;
            }
            else
            {
                items.Add(key, buffer[d].Value.Amount);
            }
        }

        for (int c = 0; c < items.Keys.Count - (contentHolder.childCount - 1); c++)
        {
            GameObject itemObject = Instantiate(itemPrefab);
            itemObject.transform.SetParent(contentHolder);
            itemObject.transform.localScale = Vector3.one;
        }

        int i = 0;
        foreach (Transform item in contentHolder)
        {
            if (i < items.Count)
            {
                var data = items.ElementAt(i);

                var itemController = item.GetComponent<InventoryItem>();
                itemController.Initialize(data.Key, data.Value);
            }
            else
            {
                Destroy(item.gameObject);
            }

            i++;
        }
    }
}
