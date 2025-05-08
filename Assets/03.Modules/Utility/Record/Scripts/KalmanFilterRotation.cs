using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class KalmanFilterRotation
{
    private const float DefaultQ = 0.000001f;
    private const float DefaultR = 0.01f;

    private const float DefaultP = 1;

    private float q;
    private float r;
    private float p = DefaultP;
    private Vector4 x;
    private float k;
    public KalmanFilterRotation() : this(DefaultQ) { }

    public KalmanFilterRotation(float aQ = DefaultQ, float aR = DefaultR)
    {
        q = aQ;
        r = aR;
    }

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public Quaternion Update(Quaternion rotation, float? newQ = null, float? newR = null)
    {
        var measurement = new Vector4(rotation.x, rotation.y, rotation.z, rotation.w);
        
        // 제공된 경우 값을 업데이트합니다.
        if (newQ != null && q != newQ)
        {
            q = (float)newQ;
        }
        if (newR != null && r != newR)
        {
            r = (float)newR;
        }

        // 측정을 업데이트합니다.
        {
            k = (p + q) / (p + q + r);
            p = r * (p + q) / (r + p + q);
        }

        // 결과를 다시 계산으로 필터링합니다.
        var result = x + (measurement - x) * k;
        x = result;
        return new Quaternion(result.x, result.y, result.z, result.w);;
    }

    public void Reset()
    {
        p = DefaultP;
        x = Vector4.zero;
        k = 0;
    }
}
