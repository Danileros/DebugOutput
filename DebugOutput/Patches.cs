using HarmonyLib;
using Il2Cpp;

namespace DebugOutput;

internal class Patches
{
    [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.UpdateDebugLines))]
    public class HUDManager_UpdateDebugLines
    {
        public static void Postfix(HUDManager __instance, Panel_HUD hud)
        {
            DebugManager.DebugOutput(hud.m_Label_DebugLines);
        }
    }
}