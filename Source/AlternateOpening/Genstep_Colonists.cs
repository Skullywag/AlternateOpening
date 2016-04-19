using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace AlternateOpening
{
    public class Genstep_Colonists : Genstep
    {
        private const int NumStartingMealsPerColonist = 10;

        private const int NumStartingMedPacksPerColonist = 8;

        public override void Generate()
        {
            foreach (Pawn current in MapInitData.colonists)
            {
                current.SetFactionDirect(Faction.OfColony);
                PawnComponentsUtility.AddAndRemoveDynamicComponents(current, false);
                current.needs.mood.thoughts.TryGainThought(ThoughtDefOf.NewColonyOptimism);
                foreach (Pawn current2 in MapInitData.colonists)
                {
                    if (current2 != current)
                    {
                        DamageDef def;
                        def = DamageDefOf.Blunt;
                        Thought_SocialMemory thought_SocialMemory = (Thought_SocialMemory)ThoughtMaker.MakeThought(ThoughtDefOf.CrashedTogether);
                        thought_SocialMemory.SetOtherPawn(current2);
                        current.needs.mood.thoughts.TryGainThought(thought_SocialMemory);
                        current2.TakeDamage(new DamageInfo(def, 1));
                    }
                }
            }
            Genstep_Colonists.CreateInitialWorkSettings();
            bool startedDirectInEditor = MapInitData.StartedDirectInEditor;
            List<List<Thing>> list = new List<List<Thing>>();
            foreach (Pawn current3 in MapInitData.colonists)
            {
                if (MapInitData.startedFromEntry && Rand.Value < 0.5f)
                {
                    current3.health.AddHediff(HediffDefOf.CryptosleepSickness, null, null);
                }
                List<Thing> list2 = new List<Thing>();
                list2.Add(current3);
                Thing thing = ThingMaker.MakeThing(ThingDefOf.MealSurvivalPack, null);
                thing.stackCount = 10;
                list2.Add(thing);
                Thing thing2 = ThingMaker.MakeThing(ThingDefOf.Medicine, null);
                thing2.stackCount = 8;
                list2.Add(thing2);
                list.Add(list2);
            }
            List<Thing> list3 = new List<Thing>
            {
                ThingMaker.MakeThing(ThingDefOf.Gun_SurvivalRifle, null),
                ThingMaker.MakeThing(ThingDefOf.Gun_Pistol, null),
                ThingMaker.MakeThing(ThingDefOf.MeleeWeapon_Knife, ThingDefOf.Plasteel),
                Genstep_Colonists.GenerateRandomPet()
            };
            int num = 0;
            foreach (Thing current4 in list3)
            {
                current4.SetFactionDirect(Faction.OfColony);
                list[num].Add(current4);
                num++;
                if (num >= list.Count)
                {
                    num = 0;
                }
            }
            bool canInstaDropDuringInit = startedDirectInEditor;
            DropPodUtility.DropThingGroupsNear(MapGenerator.PlayerStartSpot, list, 110, canInstaDropDuringInit, true, true);
        }

        private static Thing GenerateRandomPet()
        {
            PawnKindDef kindDef = (from td in DefDatabase<PawnKindDef>.AllDefs
                                   where td.race.category == ThingCategory.Pawn && td.RaceProps.petness > 0f
                                   select td).RandomElementByWeight((PawnKindDef td) => td.RaceProps.petness);
            Pawn pawn = PawnGenerator.GeneratePawn(kindDef, Faction.OfColony);
            if (pawn.Name == null || pawn.Name.Numerical)
            {
                pawn.Name = NameGenerator.GeneratePawnName(pawn, NameStyle.Full, null);
            }
            Pawn pawn2 = MapInitData.colonists.RandomElement<Pawn>();
            pawn2.relations.AddDirectRelation(PawnRelationDefOf.Bond, pawn);
            return pawn;
        }

        private static void CreateInitialWorkSettings()
        {
            foreach (Pawn current in MapInitData.colonists)
            {
                current.workSettings.DisableAll();
            }
            foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
            {
                if (w.alwaysStartActive)
                {
                    foreach (Pawn current2 in from col in MapInitData.colonists
                                              where !col.story.WorkTypeIsDisabled(w)
                                              select col)
                    {
                        current2.workSettings.SetPriority(w, 3);
                    }
                }
                else
                {
                    bool flag = false;
                    foreach (Pawn current3 in MapInitData.colonists)
                    {
                        if (!current3.story.WorkTypeIsDisabled(w) && current3.skills.AverageOfRelevantSkillsFor(w) >= 6f)
                        {
                            current3.workSettings.SetPriority(w, 3);
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        IEnumerable<Pawn> source = from col in MapInitData.colonists
                                                   where !col.story.WorkTypeIsDisabled(w)
                                                   select col;
                        if (source.Any<Pawn>())
                        {
                            Pawn pawn = source.InRandomOrder(null).MaxBy((Pawn c) => c.skills.AverageOfRelevantSkillsFor(w));
                            pawn.workSettings.SetPriority(w, 3);
                        }
                        else if (w.requireCapableColonist)
                        {
                            Log.Error("No colonist could do requireCapableColonist work type " + w);
                        }
                    }
                }
            }
        }
    }
}
