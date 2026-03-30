using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HighlighterScript : MonoBehaviour
{
    private float timeStep = 0.07f;
    private float timer = 0f;

    private int stage = 1;
    
    private Image image;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image = this.GetComponent<Image>();
        
        Color c = image.color;
        c.a = 0;
        image.color = c;
        
        transform.localScale = Vector3.zero;

        StartCoroutine(flash());
    }

    IEnumerator flash()
    {
        while (true)
        {
            timer += Time.deltaTime;

            if (stage == 1)
            {
                float value = timer / timeStep;

                if (value >= 1)
                {
                    stage = 2;
                    timer = 0;
                    yield return null;
                }
            
                Color c = image.color;
                c.a = value;
                image.color = c;
                
                transform.localScale = new Vector3(value, value, value);

                yield return null;
            }
            else if (stage == 2)
            {
                float value = 1 - (timer / timeStep);

                if (value <= 0)
                {
                    Destroy(transform.gameObject);
                }
            
                Color c = image.color;
                c.a = value;
                image.color = c;
                
                transform.localScale = new Vector3(value, value, value);

                yield return null;
            }
        }
    }
}
