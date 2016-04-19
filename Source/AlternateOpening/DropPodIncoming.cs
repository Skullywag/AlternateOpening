using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace AlternateOpening
{
    [StaticConstructorOnStartup]
    public class DropPodIncoming : Thing
    {
        protected const int MinTicksToImpact = 120;

        protected const int MaxTicksToImpact = 200;

        private const int SoundAnticipationTicks = 100;

        public DropPodInfo contents;

        protected int ticksToImpact = 120;

        private bool soundPlayed;

        private static readonly Material ShadowMat = MaterialPool.MatFrom("Things/Special/DropPodShadow", ShaderDatabase.Transparent);

        public Faction factionDirect = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Colony);

        public override Vector3 DrawPos
        {
            get
            {
                Vector3 result = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.FlyingItem);
                float num = ticksToImpact * ticksToImpact * 0.01f;
                result.x -= num * 0.4f;
                result.z += num * 0.6f;
                return result;
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.ticksToImpact = Rand.RangeInclusive(120, 200);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref ticksToImpact, "ticksToImpact", 0, false);
            Scribe_Deep.LookDeep<DropPodInfo>(ref contents, "contents", new object[0]);
        }

        public override void Tick()
        {
            // If the pod is inflight, slight ending delay here to stop smoke motes covering the crash
            if (ticksToImpact > 8)
            {
                // Throw some smoke and fire glow trails
                MoteThrower.ThrowSmoke(DrawPos, 3f);
                MoteThrower.ThrowFireGlow(DrawPos.ToIntVec3(), 1.5f);
            }
            this.ticksToImpact--;
            if (ticksToImpact <= 0)
            {
                PodImpact();
            }
            if (!soundPlayed && ticksToImpact < 100)
            {
                soundPlayed = true;
                SoundDefOf.DropPodFall.PlayOneShot(Position);
            }
        }

        public override void DrawAt(Vector3 drawLoc)
        {
            base.DrawAt(drawLoc);
            Vector3 pos = this.TrueCenter();
            pos.y = Altitudes.AltitudeFor(AltitudeLayer.Shadows);
            float num = 2f + ticksToImpact / 100f;
            Vector3 s = new Vector3(num, 1f, num);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, Rotation.AsQuat, s);
            Graphics.DrawMesh(MeshPool.plane10Back, matrix, DropPodIncoming.ShadowMat, 0);
        }

        private void PodImpact()
        {
            // max side length of drawSize or actual size determine result crater radius
            var impactRadius = Mathf.Max(Mathf.Max(def.Size.x, def.Size.z), Mathf.Max(Graphic.drawSize.x, Graphic.drawSize.y)) * 1.5f;
            for (int i = 0; i < 6; i++)
            {
                Vector3 loc = Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f);
                MoteThrower.ThrowDustPuff(loc, 1.2f);
            }
            // Create a crashed drop pod
            DropPodCrashed dropPod = (DropPodCrashed)ThingMaker.MakeThing(ThingDef.Named("DropPodCrashed"), null);
            dropPod.info = contents;
            dropPod.SetFactionDirect(factionDirect);
            // Spawn the crater
            var crater = (Crater)ThingMaker.MakeThing(ThingDef.Named("Crater"));
            // adjust result crater size to the impact zone radius
            crater.impactRadius = impactRadius;
            // spawn the crater, rotated to the random angle, to provide visible variety
            GenSpawn.Spawn(crater, Position, Rot4.North);

            // Spawn the crashed drop pod
            GenSpawn.Spawn(dropPod, Position, Rotation);
            // For all cells around the crater centre point based on half its diameter (radius)
            foreach (IntVec3 current in GenRadial.RadialCellsAround(crater.Position, impactRadius, true))
            {
                // List all things found in these cells
                List<Thing> list = Find.ThingGrid.ThingsListAt(current);
                // Reverse iterate through the things so we can destroy without breaking the pointer
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    // If its a plant, filth, or an item
                    if (list[i].def.category == ThingCategory.Plant || list[i].def.category == ThingCategory.Filth || list[i].def.category == ThingCategory.Item)
                    {
                        // Destroy it
                        list[i].Destroy();
                    }
                }
            }
            RoofDef roof = Position.GetRoof();
            if (roof != null)
            {
                if (!roof.soundPunchThrough.NullOrUndefined())
                {
                    roof.soundPunchThrough.PlayOneShot(Position);
                }
                if (roof.filthLeaving != null)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        FilthMaker.MakeFilth(Position, roof.filthLeaving, 1);
                    }
                }
            }
            // Do a bit of camera shake for added effect
            CameraShaker.DoShake(0.020f);
            // Fire an explosion with motes
            GenExplosion.DoExplosion(Position, 0.5f, DamageDefOf.Bomb, this, null, null, null, null, 0f, false, null, 0f);
            CellRect cellRect = CellRect.CenteredOn(Position, 2);
            cellRect.ClipInsideMap();
            for (int i = 0; i < 5; i++)
            {
                IntVec3 randomCell = cellRect.RandomCell;
                MoteThrower.ThrowFireGlow(DrawPos.ToIntVec3(), 1.5f);
            }
            this.Destroy(DestroyMode.Vanish);
        }
    }
}
