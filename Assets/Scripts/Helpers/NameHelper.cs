using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class NameHelper : MonoBehaviour
{

    public static string GetName(Gender gender)
    {
        string path = "";
        if (gender == Gender.Male)
        {
            path = @"Assets\Resources\Text\Names\MaleNames.csv";
        }
        else if (gender == Gender.Female)
        {
            path = @"Assets\Resources\Text\Names\FemaleNames.csv";

        }
        Stopwatch watch = new Stopwatch();
        watch.Start();

        var name = File.ReadLines(path).Skip(Random.Range(0, 1000)).First().Split(',').First();
        
        watch.Stop();
        Debug.Log(watch.Elapsed);

        return name;
    }
}
