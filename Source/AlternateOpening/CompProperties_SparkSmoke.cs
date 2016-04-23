using System;
using Verse;
using RimWorld;

namespace AlternateOpening
{
    public class CompProperties_SparkSmoke : CompProperties
    {
        public bool smoke;
        public float smokeChance;
        public bool spark;
        public float sparkChance;
        public int extraDamageCellsInt;
        public float extraDamageCellChance;

        public CompProperties_SparkSmoke()
        {
            this.compClass = typeof(CompSparkSmoke);
        }
    }
}
