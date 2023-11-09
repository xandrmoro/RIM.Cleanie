using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Cleanie
{
    [HarmonyPatch]
    public class WorkGiver_Filth_GetPriority_Patch
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

            var target = workGiver.GetMethods().First(m => m.Name == "GetPriority" && m.GetParameters().Any(p => p.ParameterType == typeof(TargetInfo)));
            if (target == null)
            {
                Log.Error("[Cleanie] Could not find WorkGiver_CleanFilth.GetPriority");
                return null;
            }

            return target;
        }

        static bool Prefix(ref float __result, Pawn pawn, TargetInfo t)
        {
            var room = t.Cell.GetRoom(t.Map);

            if (room == null)
            {
                __result = 0;
            }
            else
            {
                var dnd = false;

                if (Cleanie.Settings.DoNotDisturb 
                    && !(room.Role == RoomRoleDefOf.Hospital || room.Role == RoomRoleDefOf.None) 
                    && room.ContainedBeds.Any(b => !b.ForHumanBabies && b.CurOccupants.Any()))
                {
                    dnd = true;
                }

                if (dnd)
                {
                    __result = 0;
                }
                else
                {
                    __result = GetRoomDefWeights(room.Role);

                    var distance = pawn.Position.DistanceToSquared(t.CenterCell);
                    var belowThreshold = room.GetStat(RoomStatDefOf.Cleanliness) <= GetRoomDefThreshold(room.Role);
                    var factor = 0.25f + Math.Min(Math.Max(0.75f - distance / 533f, 0f), 0.75f) * (belowThreshold ? 0.1f : 1f);
                    __result *= factor;
                }
            }
#if DEBUG
            Log.Message($"[Cleanie] {pawn.Name} {pawn.Position} {t.Cell} {room.Role.label} {__result}");
#endif

            return false;
        }

        static float GetRoomDefWeights(RoomRoleDef def)
        {
            if (Cleanie.Settings.Weights.TryGetValue(def.defName, out var pair))
            {
                return pair.Weight;
            }
            else
            {
                return 50f;
            }            
        }

        static float GetRoomDefThreshold(RoomRoleDef def)
        {
            if (Cleanie.Settings.Weights.TryGetValue(def.defName, out var pair))
            {
                return pair.Threshold;
            }
            else
            {
                return 0f;
            }
        }
    }
}
