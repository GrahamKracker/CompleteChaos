using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Extensions;
using Il2CppTMPro;
using UnityEngine;

namespace CompleteChaos;

public class SpeedUI
{   
    private static ModHelperPanel panel;
    private static ModHelperText text;

    public static void Create(GameObject screen)
    {
        panel=screen.AddModHelperPanel(new Info("SpeedUI")
        {
            Anchor = new Vector2(.5f, .5f),
            Pivot = new Vector2(.5f, .5f),
            Position = new Vector2(-525, -115),
        });
        text = panel.AddText(new Info("SpeedText", 0, 0, 1000, 200), "", 42, TextAlignmentOptions.MidlineRight);  
        
    }
    public static void Update(float newSpeed)
    {
        if(text == null) return;
        text.SetText("Current Speed: " + newSpeed.ToString("0.00")+"x");
    }
    
}
