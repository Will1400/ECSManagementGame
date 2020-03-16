using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MathHelper
{
    public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        float m = (toMax - toMin) / (fromMax - fromMin);
        float c = toMin - (m * fromMin);

        return m * value + c;
    }
}
