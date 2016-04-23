using Verse;
using UnityEngine;
using System.Collections.Generic;
using Random = System.Random;

namespace AlternateOpening
{
    public class CompSparkSmoke : ThingComp
    {
        public Effecter sparks = null; // Global effects variable
        public int cellsDamaged = 0;
        public IntVec3 smoker; // A random cell in the occupied list
        public IntVec3 sparker; // A random cell in the occupied list
        public IEnumerable<IntVec3> occupiedCells = null; // List of cells the drop pod occupies
        public List<IntVec3> damagedCells = new List<IntVec3>(); // list of smoke and spark generators

        public CompProperties_SparkSmoke Props
        {
            get
            {
                return (CompProperties_SparkSmoke)this.props;
            }
        }
        public override void PostSpawnSetup()
        {
            // Do base setup
            base.PostSpawnSetup();
            // Setup list of cells the drop ship occupies
            occupiedCells = GenAdj.CellsOccupiedBy(parent);
            // Pick a random cell for mote usage
            smoker = occupiedCells.RandomElement<IntVec3>();
            // And another
            sparker = occupiedCells.RandomElement<IntVec3>();
            foreach (var cell in GenAdj.CellsOccupiedBy(parent))
            {
                if (cellsDamaged == 0)
                {
                    damagedCells.Add(cell);
                }
                if (Props.extraDamageCellsInt > 0)
                {
                    for(int i = 0; i < Props.extraDamageCellsInt; i++)
                    if (Props.extraDamageCellChance > new Random().NextDouble())
                    {
                        damagedCells.Add(cell);
                    }
                }
            }
        }
        public override void PostExposeData()
        {
            // Base date to save
            base.PostExposeData();
        }
        public override void CompTick()
        {
            base.CompTick();
            // Throw smoke mote
            foreach (IntVec3 currentCell in damagedCells)
            {
                if (Props.smoke)
                {
                    if (Rand.Value < Props.smokeChance)
                    {
                        ThrowSmokeBlack(parent.Position.ToVector3Shifted(), 0.5f);
                    }
                }
                if (Props.spark)
                {
                    // Setup a new spark effect
                    sparks = new Effecter(DefDatabase<EffecterDef>.GetNamed("ElectricShort"));
                    // If we have a spark effecter
                    if (sparks != null)
                    {
                        if (Rand.Value < Props.sparkChance)
                        {
                            // Continue effect
                            sparks.EffectTick(parent.Position, parent);
                            sparks.Cleanup();
                            sparks = null;
                        }
                    }
                }
            }          
        }
        public static void ThrowSmokeBlack(Vector3 loc, float size)
        {
            // Only throw smoke every 10 ticks
            if (Find.TickManager.TicksGame % 10 != 0)
            {
                return;
            }
            // Make the mote
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDef.Named("Mote_SmokeBlack"), null);
            // Set a size
            moteThrown.ScaleUniform = Rand.Range(1.5f, 2.5f) * size;
            // Set a rotation
            moteThrown.exactRotationRate = Rand.Range(-0.5f, 0.5f);
            // Set a position
            moteThrown.exactPosition = loc;
            // Set angle and speed
            moteThrown.SetVelocityAngleSpeed((float)Rand.Range(30, 40), Rand.Range(0.008f, 0.012f));
            // Spawn mote
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3());
        }
    }
}
