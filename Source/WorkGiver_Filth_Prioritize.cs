using HarmonyLib;
using System.Linq;
using System.Reflection;
using Verse;

namespace Cleanie
{
    [HarmonyPatch]
    public class WorkGiver_Filth_Prioritize_Patch
    {
        public static MethodBase TargetMethod()
        {
            var assembly = Assembly.GetAssembly(typeof(Pawn));

            var type = assembly.GetType("RimWorld.WorkGiver_CleanFilth");
            if (type != null)
            {
                return type.GetProperties().Single(p => p.Name == "Prioritized").GetGetMethod();
            }

            return null;
        }

        static bool Prefix(ref bool __result)
        {
            __result = true;

            return false;
        }
    }
}
