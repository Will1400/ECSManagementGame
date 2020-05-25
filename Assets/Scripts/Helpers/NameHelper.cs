using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

public class NameHelper : MonoBehaviour
{
    static string[] maleNames;
    static string[] femaleNames;


    public static string GetName(Gender gender)
    {
        if (maleNames == null)
        {
            var textAsset = Resources.Load<TextAsset>("Text/Names/MaleNames");
            maleNames = textAsset.text.Split(',').Select(x => x.Trim()).ToArray();
        }
        if (femaleNames == null)
        {
            var textAsset = Resources.Load<TextAsset>("Text/Names/FemaleNames");
            femaleNames = textAsset.text.Split(',').Select(x => x.Trim()).ToArray();
        }

        if (gender == Gender.Male)
        {
            return maleNames[Random.Range(0, maleNames.Length)];
        }
        else if (gender == Gender.Female)
        {
            return femaleNames[Random.Range(0, maleNames.Length)];
        }

        return string.Empty;
    }
}
