using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityIconManager : MonoBehaviour
{
    private float endValue = 1f;
    private float endAlpha = 1f;
    private PlayerStats akiraStats;
    private bool unlocked;
    private Image img;
    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<Image>();
        unlocked = false;

        akiraStats = GameObject.Find("Akira").GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.CompareTag("FireBreath")) {
            if (!akiraStats.CheckBreath(0f)) {
                return;
            }
        } else if (gameObject.CompareTag("FireWall")) {
            if (!akiraStats.CheckWall(0f)) {
                return;
            }
        } else if (gameObject.CompareTag("Leap")) {
            if (!akiraStats.CheckLeap(0f)) {
                return;
            }
        }

        if(unlocked) {
            return;
        }
        
        float h, s, v;
        Color.RGBToHSV(img.color, out h, out s, out v);
        v = endValue;
        Color newColor = Color.HSVToRGB(h, s, v);
        newColor.a = endAlpha;
        img.color = newColor;
        unlocked = true;
    }
}
