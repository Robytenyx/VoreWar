using System.Collections.Generic;

public class ScarabTacticalAI : TacticalAI
{
    public ScarabTacticalAI(List<Actor_Unit> actors, TacticalTileType[,] tiles, int AISide, bool defendingVillage = false) : base(actors, tiles, AISide, defendingVillage)
    {
    }

    protected override void GetNewOrder(Actor_Unit actor)
    {
        foundPath = false;
        didAction = false; // Very important fix: surrounded retreaters sometimes just skipped doing attacks because this was never set to false in or before "fightwithoutmoving"

        path = null;

        if (Config.KuroTenkoEnabled && actor.PredatorComponent != null)
        {
            if (actor.PredatorComponent.CanFeed())
            {
                RunFeed(actor, "breast", true, "Scarab");
                if (foundPath || didAction) return;
            }
            if (actor.PredatorComponent.CanFeedCum())
            {
                RunFeed(actor, "cock", true, "Scarab");
                if (foundPath || didAction) return;
            }
        }
        int spareMp = CheckActionEconomyOfActorFromPositionWithAP(actor, actor.Position, actor.Movement);

        // Get value of a third of actor's movement
        int thirdMovement = actor.MaxMovement() / 3;

        // If unit would have enough AP left over...
        if (spareMp >= thirdMovement)
        {
            // Unit will belly rub themselves
            RunBellyRub(actor, spareMp);
            if (path != null)
                return;
            if (didAction) return;
        }

        // If unit is under temptation...
        if (actor.Unit.GetStatusEffect(StatusEffectType.Temptation) != null && (State.Rand.Next(2) == 0 || actor.Unit.GetStatusEffect(StatusEffectType.Temptation).Duration <= 2))
        {
            // Unit will forcefeed themselves
            RunForceFeed(actor);
        }

        // If unit can pounce...
        if (actor.Unit.HasTrait(Traits.Pounce) && actor.Movement >= 2)
        {
            // Unit will check for suitable target and vore pounce
            RunVorePounce(actor);
            if (path != null)
                return;
            if (didAction) return;
        }

        // Unit will check for suitable target and vore
        RunPred(actor);
        if (didAction || foundPath)
            return;

        // Unit will check for suitable target and resurrect
        TryResurrect(actor);
        // Unit will check for suitable target and reanimate
        TryReanimate(actor);
        // Unit will check for suitable target and bind
        RunBind(actor);
        // If unit lacks a physical weapon or a random chance of 50% is met...
        if (State.Rand.Next(2) == 0 || actor.Unit.HasWeapon == false)
            // Unit will attempt a spell
            RunSpells(actor);
        if (path != null)
            return;
        // If unit can pounce...
        if (actor.Unit.HasTrait(Traits.Pounce) && actor.Movement >= 2)
        {
            if (IsRanged(actor) == false)
            {
                // Unit will check for suitable target and pounce
                RunMeleePounce(actor);
                if (didAction) return;
            }
        }
        if (foundPath || didAction) return;
        // Unit attempts to attack
        if (IsRanged(actor))
            RunRanged(actor);
        else
            RunMelee(actor);
        if (foundPath || didAction) return;

        if (Config.KuroTenkoEnabled && actor.PredatorComponent != null)
        {
            if ((Config.FeedingType == FeedingType.Both || Config.FeedingType == FeedingType.BreastfeedOnly) && actor.PredatorComponent.CanFeed())
            {
                RunFeed(actor, "breast");
                if (foundPath || didAction) return;
            }
            if ((Config.FeedingType == FeedingType.Both || Config.FeedingType == FeedingType.CumFeedOnly) && actor.PredatorComponent.CanFeedCum())
            {
                RunFeed(actor, "cock");
                if (foundPath || didAction) return;
            }
            RunSuckle(actor);
            if (foundPath || didAction) return;
        }

        RunBellyRub(actor, actor.Movement);
        if (foundPath || didAction) return;
        //Search for surrendered targets outside of vore range
        //If no path to any targets, will sit out its turn
        RunPred(actor, true);
        if (foundPath || didAction) return;
        actor.ClearMovement();
    }
}
