using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using System.Reflection;
using Verse;

namespace Cleanie
{
    [HarmonyPatch]
    public class WorkGiver_Filth_GetPriority_Patch
    {
        public static MethodBase TargetMethod()
        {
            var assembly = Assembly.GetAssembly(typeof(Pawn));
            
            var type = assembly.GetType("RimWorld.WorkGiver_CleanFilth");
            if (type != null)
            {
                return type.GetMethods().First(m => m.Name == "GetPriority" && m.GetParameters().Any(p => p.ParameterType == typeof(TargetInfo)));
            }

            return null;
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
                    var belowThreshold = room.GetStat(RoomStatDefOf.Cleanliness) <= Cleanie.Settings.Weights[room.Role.defName].Threshold;
                    var factor = 0.25f + Math.Min(Math.Max(0.75f - distance / 533f, 0f), 0.75f) * (belowThreshold ? 0.1f : 1f);
                    __result *= factor;
                }
            }

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
    }
}
