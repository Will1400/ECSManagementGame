using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class CitizenDetailsFiller : PanelFillerBase
{
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private TextMeshProUGUI genderText;
    [SerializeField]
    private TextMeshProUGUI ageText;

    public override void Fill(Entity entity, bool keepUpdated = true)
    {
        base.Fill(entity, keepUpdated);

        if (EntityManager.HasComponent<Citizen>(entity))
        {
            UpdateContent();
        }
    }

    protected override void UpdateContent()
    {
        if (!EntityManager.Exists(entity))
            return;

        if (EntityManager.HasComponent<Citizen>(entity))
        {
            var data = EntityManager.GetComponentData<Citizen>(entity);

            nameText.text = data.CitizenPersonalData.Name.ToString();
            genderText.text = data.CitizenPersonalData.Gender.ToString();
            ageText.text = data.CitizenPersonalData.Age.ToString();
        }
    }
}
