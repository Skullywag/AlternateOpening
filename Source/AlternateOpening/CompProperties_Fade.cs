using System;
using Verse;
using RimWorld;

namespace AlternateOpening
{
    public class CompProperties_Fade : CompProperties
    {
        public int lifespanTicks;

        public CompProperties_Fade()
        {
            this.compClass = typeof(CompFade);
        }
    }
}
