using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SignSetManager
{
    public static List<SignCode> signCodes;
    
    private static SignSet sourceSignSet;
    private static SignSet targetSignSet;

    public static Sign GetSoureSign(SignCode signCode)
    { 
        return sourceSignSet.GetSign(signCode);
    }

    public static Sign GetTargetSign(SignCode signCode)
    {
        return targetSignSet.GetSign(signCode);
    }

    public static void SetSignSets(SignSet source, SignSet target)
    {
        sourceSignSet = source;
        sourceSignSet.Initialize();
        targetSignSet = target;
        targetSignSet.Initialize();
    }
    
    static SignSetManager()
    {
        //if any info is missing, set it up using the settings
        if (signCodes == null || sourceSignSet == null || targetSignSet == null)
        {
            SettingsManager.SetSignSets();
        }
    }
}
