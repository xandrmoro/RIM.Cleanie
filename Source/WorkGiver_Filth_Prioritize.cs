using HarmonyLib;
using RimWorld;

namespace Cleanie
{
    [HarmonyPatch(typeof(WorkGiver_Scanner), nameof(WorkGiver_Scanner.Prioritized), MethodType.Getter)]
    public class WorkGiver_Filth_Prioritize_Patch
    {
        static bool Prefix(ref bool __result, WorkGiver_Scanner __instance)
        {
            if (!(__instance is WorkGiver_CleanFilth))
            {
                return true;
            }

            __result = true;

            return false;
        }
    }
}
