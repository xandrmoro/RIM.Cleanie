using System.Collections.Generic;
using Verse;

namespace Cleanie
{
    public class CleanieSettings : ModSettings
    {
        public class Pair : IExposable
        {
            public float Weight;
            public float Threshold;

            public void ExposeData()
            {
                Scribe_Values.Look(ref Weight, "weight");
                Scribe_Values.Look(ref Threshold, "threshold");
            }
        }

        private Dictionary<string, Pair> weights = new Dictionary<string, Pair> { { "None", new Pair { Weight = 50f, Threshold = 0f } } };
        public Dictionary<string, Pair> Weights => weights;

        public bool DoNotDisturb;

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref weights, "weights", LookMode.Value, LookMode.Deep);
            Scribe_Values.Look(ref DoNotDisturb, "doNotDisturb", false);

            base.ExposeData();
        }
    }
}
