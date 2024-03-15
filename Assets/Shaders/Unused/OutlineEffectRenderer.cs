using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class OutlineEffectRenderer : PostProcessEffectRenderer<OutlineEffectSettings>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("PostProcessing/OutlineEffect"));

        sheet.properties.SetMatrix("_ViewProjectInverse", (Camera.current.projectionMatrix * Camera.current.worldToCameraMatrix).inverse);
        sheet.properties.SetFloat("_OutlineThickness", settings.thickness);
        sheet.properties.SetFloat("_OulineDensity", settings.density);
        sheet.properties.SetColor("_OutlineColor", settings.color);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}