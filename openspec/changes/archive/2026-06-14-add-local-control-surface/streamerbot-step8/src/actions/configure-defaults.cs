using System;

public class CPHInline
{
    private const string BaseUrlGlobal = "mouseShenanigans.localControl.baseUrl";
    private const string HorizontalProfileGlobal = "mouseShenanigans.localControl.horizontalProfile";
    private const string DoubleRightProfileGlobal = "mouseShenanigans.localControl.doubleRightProfile";

    public bool Execute()
    {
        EnsureGlobal(BaseUrlGlobal, "http://127.0.0.1:5178");
        EnsureGlobal(HorizontalProfileGlobal, "horizontal-inversion");
        EnsureGlobal(DoubleRightProfileGlobal, "double-right");
        CPH.LogInfo("[MSLC] Defaults ready. Start Mouse Shenanigans tray app before running endpoint actions.");
        return true;
    }

    private void EnsureGlobal(string name, string defaultValue)
    {
        string current = CPH.GetGlobalVar<string>(name, true);
        if (string.IsNullOrWhiteSpace(current))
        {
            CPH.SetGlobalVar(name, defaultValue, true);
        }
    }
}
