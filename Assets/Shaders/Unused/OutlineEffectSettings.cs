using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[PostProcess(typeof(OutlineEffectRenderer), PostProcessEvent.AfterStack, "Custom/OutlineEffect")]
public class OutlineEffectSettings : PostProcessEffectSettings
{
    [Range(0.0f, 1.0f)]
    public FloatParameter thickness = new FloatParameter { value = 0.2f };

    [Range(0.0f, 1.0f)]
    public FloatParameter density = new FloatParameter { value = 0.75f };

    public ColorParameter color = new ColorParameter { value = Color.black };
}
