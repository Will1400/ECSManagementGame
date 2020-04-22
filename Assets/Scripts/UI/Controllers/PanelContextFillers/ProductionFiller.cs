using UnityEngine;
using System.Collections;
using Unity.Entities;
using TMPro;
using System;

public class ProductionFiller : PanelFiller
{
    [SerializeField]
    private TextMeshProUGUI resourceTypeText;
    [SerializeField]
    private TextMeshProUGUI amountPerProductionText;
    [SerializeField]
    private TextMeshProUGUI productionTimeText;
    [SerializeField]
    private TextMeshProUGUI productionTimeRemainingText;

    public override void Fill(Entity entity, bool keepUpdated = true)
    {
        base.Fill(entity, keepUpdated);

        if (EntityManager.HasComponent<ResourceProductionData>(entity))
        {
            UpdateContent();
            updateRate = .2f;
        }
    }

    protected override void UpdateContent()
    {
        if (!EntityManager.Exists(entity))
            return;

        if (EntityManager.HasComponent<ResourceProductionData>(entity))
        {
            var productionData = EntityManager.GetComponentData<ResourceProductionData>(entity);

            resourceTypeText.text = productionData.ResourceType.ToString();
            amountPerProductionText.text = productionData.AmountPerProduction.ToString();
            productionTimeText.text = productionData.ProductionTime.ToString();
            productionTimeRemainingText.text = Math.Round(productionData.ProductionTimeRemaining, 2).ToString();
        }
    }
}
