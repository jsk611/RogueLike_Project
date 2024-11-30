using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScriptForParabola : MonoBehaviour
{
    // Start is called before the first frame update
    
    public float v;
  
    public Quaternion S;

    float g;
    float t;
    float x;
    float y;
    float sin;
    float cos;

    LineRenderer line;
    void Start()
    {
        g = -Physics.gravity.y;
        
        line = GetComponent<LineRenderer>();

        y = v * sin * t - 0.5f * g * t * t + 1;
        x = v*cos*t;

        sin = Mathf.Sin(S.eulerAngles.y);
        cos = Mathf.Cos(S.eulerAngles.x);

     //   t = (v * sin + Mathf.Pow((Mathf.Pow(v*sin,2) + 2 * g), 0.5f)) / g;
        Debug.Log(t);
        line.SetPosition(0, gameObject.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        t = (v * sin + Mathf.Pow((Mathf.Pow(v * sin, 2) + 2 * g), 0.5f)) / g;
        sin = Mathf.Sin(S.eulerAngles.y*Mathf.Deg2Rad);
        cos = Mathf.Cos(S.eulerAngles.x*Mathf.Deg2Rad);
        y = v * sin * t - 0.5f * g * t * t + 1;
        x = v * cos * t;
       
        gameObject.transform.position = new Vector3(x, y, 0);
        float temp = 0;

        
        for (int i = 1; i < line.positionCount; i++)
        {
            temp += t / line.positionCount;
            line.SetPosition(i, new Vector3(calx(temp),caly(temp),0));
        }
    }
    float calx(float t)
    {
        float result;
        result = v * cos * t;
        return result;
    }
    float caly(float t)
    {

        float result;
        result = v * sin * t - 0.5f * g * t * t + 1;
        return result;
    }
}
