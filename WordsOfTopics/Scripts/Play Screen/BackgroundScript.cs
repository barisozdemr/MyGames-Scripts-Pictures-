using System;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundScript : MonoBehaviour
{
    public Image backgroundImage;
    
    public Sprite sporSprite;
    public Sprite biyolojiSprite;
    public Sprite enerjiSprite;
    public Sprite sanayiSprite;
    public Sprite tipSprite;
    public Sprite fizikSprite;
    public Sprite ulkeSprite;
    public Sprite baskentSprite;
    public Sprite yemekSprite;
    public Sprite kozmetikSprite;
    public Sprite egitimSprite;
    public Sprite ulasimSprite;
    public Sprite hayvanSprite;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setBackground(String topic)
    {
        if      (topic.Equals("SPOR")) backgroundImage.sprite = sporSprite;
        else if (topic.Equals("BİYOLOJİ")) backgroundImage.sprite = biyolojiSprite;
        else if (topic.Equals("ENERJİ")) backgroundImage.sprite = enerjiSprite;
        else if (topic.Equals("SANAYİ")) backgroundImage.sprite = sanayiSprite;
        else if (topic.Equals("TIP")) backgroundImage.sprite = tipSprite;
        else if (topic.Equals("FİZİK")) backgroundImage.sprite = fizikSprite;
        else if (topic.Equals("ÜLKE")) backgroundImage.sprite = ulkeSprite;
        else if (topic.Equals("BAŞKENT")) backgroundImage.sprite = baskentSprite;
        else if (topic.Equals("YEMEK")) backgroundImage.sprite = yemekSprite;
        else if (topic.Equals("KOZMETİK")) backgroundImage.sprite = kozmetikSprite;
        else if (topic.Equals("EĞİTİM")) backgroundImage.sprite = egitimSprite;
        else if (topic.Equals("ULAŞIM")) backgroundImage.sprite = ulasimSprite;
        else if (topic.Equals("HAYVAN")) backgroundImage.sprite = hayvanSprite;
    }
}
