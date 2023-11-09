using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Cleanie
{
    [HarmonyPatch]
    public class WorkGiver_Filth_Prioritize_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return GetMethod(Assembly.GetAssembly(typeof(Pawn)), "RimWorld");

            if (Cleanie.DubsAssembly != null)
                yield return GetMethod(Cleanie.DubsAssembly, "DubsBadHygiene");
        }

        private static MethodBase GetMethod(Assembly assembly, string ns)
        {
            var workGiver = assembly.GetType($"{ns}.WorkGiver_CleanFilth");

            if (workGiver == null)
            {
                Log.Error("[Cleanie] Could not find WorkGiver_CleanFilth");
                return null;
            }

            var target = workGiver.GetProperties().Single(p => p.Name == "Prioritized").GetGetMethod();
            if (target == null)
            {
                Log.Error("[Cleanie] Could not find WorkGiver_CleanFilth.GetPriority");
                return null;
            }

            return target;
        }

        static bool Prefix(ref bool __result)
        {
            __result = true;

            return false;
        }
    }
}
