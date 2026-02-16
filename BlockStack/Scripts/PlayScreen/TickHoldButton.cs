using UnityEngine;
using UnityEngine.EventSystems;

public class TickHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GameObject logicManager;
    private LogicScript logicScript;
    
    private float holdTime = 1f;

    private float timeStep = 0.2f;

    private bool isHolding = false;
    private float timer = 0f;
    
    private bool ticked = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        logicScript = logicManager.GetComponent<LogicScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isHolding) timer += Time.deltaTime;

        if (timer >= holdTime)
        {
            ticked = true;
            timer -= timeStep;
            logicScript.tickButtonClicked();
        }
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(ticked) ticked = false;
        else logicScript.tickButtonClicked();
        
        isHolding = false;
        timer = 0;
    }
}
