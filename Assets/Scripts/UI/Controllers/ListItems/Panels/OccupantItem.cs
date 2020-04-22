using UnityEngine;
using System.Collections;
using TMPro;
using System;

public class OccupantItem : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private TextMeshProUGUI genderText;
    [SerializeField]
    private TextMeshProUGUI ageText;
    [SerializeField]
    private TextMeshProUGUI StatusText;

    public void Initialize(Citizen citizen)
    {
        nameText.text = citizen.CitizenPersonalData.Name.ToString();
        genderText.text = citizen.CitizenPersonalData.Gender.ToString();
        ageText.text = citizen.CitizenPersonalData.Age.ToString();
    }
}
