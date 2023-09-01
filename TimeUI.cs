using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Extensions;
using Il2CppTMPro;
using UnityEngine;

namespace CompleteChaos;

public class TimeUI
{
    private static ModHelperPanel panel;
    private static ModHelperText text;

    public static void Create(GameObject screen)
    {
        panel=screen.AddModHelperPanel(new Info("TimeUI")
        {
            Anchor = new Vector2(.5f, .5f),
            Pivot = new Vector2(.5f, .5f),
            Position = new Vector2(-525, -155),
        });
        text = panel.AddText(new Info("TimeText", 0, 0, 1000, 200), "",42, TextAlignmentOptions.MidlineRight);  
    }
    public static void Update(float newTime)
    {
        if(text == null) return;
        text.SetText("Time until next shuffle: " + newTime.ToString("0.00"));
    }
}