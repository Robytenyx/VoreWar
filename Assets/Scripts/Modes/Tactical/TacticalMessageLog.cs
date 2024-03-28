using OdinSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static LogUtilities;
using Random = UnityEngine.Random;

public class TacticalMessageLog
{
    [OdinSerialize]
    List<EventLog> events;

    public bool ShowOdds = false;
    public bool ShowHealing = true;
    public bool ShowSpells = true;
    public bool ShowMisses = true;
    public bool ShowWeaponCombat = true;
    public bool ShowInformational = true;
    public bool ShowPureFluff = true;

    public bool SimpleText = false;

    Unit defaultPrey;

    public TacticalMessageLog()
    {
        defaultPrey = new Unit(Race.Humans);
        defaultPrey.DefaultBreastSize = -1;
        defaultPrey.DickSize = -1;
        defaultPrey.Name = "[Redacted]";
        events = new List<EventLog>();
    }

    internal class EventLog
    {
        [OdinSerialize]
        internal MessageLogEvent Type;
        [OdinSerialize]
        internal float Odds;
        [OdinSerialize]
        internal Unit Target;
        [OdinSerialize]
        internal Unit Unit;
        [OdinSerialize]
        internal Unit Prey;
        [OdinSerialize]
        internal Unit AdditionalUnit;
        [OdinSerialize]
        internal PreyLocation preyLocation;
        [OdinSerialize]
        internal PreyLocation oldLocation;
        [OdinSerialize]
        internal Race oldRace;
        [OdinSerialize]
        internal Weapon Weapon;
        [OdinSerialize]
        internal int Damage;
        [OdinSerialize]
        internal int Bonus;
        [OdinSerialize]
        internal string Message;
        [OdinSerialize]
        internal string Extra;
    }




    class SpellLog : EventLog
    {
        [OdinSerialize]
        internal SpellTypes SpellType;
    }

    internal enum MessageLogEvent
    {
        Hit,
        Miss,
        Devour,
        Unbirth,
        CockVore,
        BreastVore,
        BellyRub,
        BreastRub,
        BallMassage,
        Feed,
        Birth,
        TransferFail,
        TransferSuccess,
        Resist,
        Kill,
        Digest,
        Absorb,
        Escape,
        Freed,
        Regurgitated,
        Heal,
        NewTurn,
        LowHealth,
        FeedCum,
        Forcefeeding,
        RandomDigestion,
        Miscellaneous,
        PartialEscape,
        TailVore,
        AnalVore,
        Dazzle,
        SpellHit,
        SpellMiss,
        SpellKill,
        CurseExpires,
        DiminishmentExpires,
        TailRub,
        Suckle,
        SuckleFail,
        VoreStealFail,
        VoreStealSuccess,
        GreatEscapeKeep,
        GreatEscapeFlee,
        ManualRegurgitation,
        SpellCast,
        TraitConvert,
        TraitRebirth,
        CumGestation,
        ForcefeedFail
    }

    public void RefreshListing()
    {
        StringBuilder sb = new StringBuilder();
        var validEvents = events.Where(s => EventValid(s));
        List<EventLog> last200;

        if (validEvents.Count() > 200)
        {
            last200 = validEvents.ToList().GetRange(validEvents.Count() - 200, 200);
        }
        else
            last200 = validEvents.ToList();

        foreach (EventLog action in last200)
        {
            sb.AppendLine(EventDescription(action));
        }
        State.GameManager.TacticalMode.LogUI.Text.text = sb.ToString();
        State.GameManager.TacticalMode.LogUI.Text.transform.Translate(new Vector3(0, 30000, 0));
    }

    internal string DebugDump()
    {
        StringBuilder sb = new StringBuilder();

        foreach (EventLog action in events)
        {
            sb.AppendLine(EventDescription(action));
        }
        return sb.ToString();
    }

    bool EventValid(EventLog test)
    {
        switch (test.Type)
        {
            case MessageLogEvent.Heal:
                if (ShowHealing == false) return false; break;
            case MessageLogEvent.Miss:
                if (ShowMisses == false || ShowWeaponCombat == false) return false; break;
            case MessageLogEvent.Resist:
            case MessageLogEvent.Dazzle:
            case MessageLogEvent.SpellMiss:
                if (ShowMisses == false || ShowSpells == false) return false; break;
            case MessageLogEvent.SpellHit:
            case MessageLogEvent.SpellCast:
                if (ShowSpells == false) return false; break;
            case MessageLogEvent.LowHealth:
            case MessageLogEvent.Absorb:
            case MessageLogEvent.NewTurn:
                if (ShowInformational == false) return false; break;
            case MessageLogEvent.RandomDigestion:
            case MessageLogEvent.GreatEscapeKeep:
                if (ShowPureFluff == false) return false; break;
            case MessageLogEvent.Hit:
                if (ShowWeaponCombat == false) return false; break;

        }
        return true;
    }

    public void Clear()
    {
        events.Clear();
        State.GameManager.TacticalMode.LogUI.Text.text = "";
    }



    void UpdateListing()
    {
        if (events.Count > 4000)
            events.RemoveRange(0, 400);
        if (State.GameManager.TacticalMode.turboMode)
            return;
        if (EventValid(events.Last()) == false)
            return;
        State.GameManager.TacticalMode.LogUI.Text.text += EventDescription(events.Last()) + "\n";
        if (State.GameManager.TacticalMode.LogUI.Text.text.Length > 10000)
        {
            State.GameManager.TacticalMode.LogUI.Text.text = State.GameManager.TacticalMode.LogUI.Text.text.Substring(1000);
        }
        State.GameManager.TacticalMode.TacticalLogUpdated = true;
    }

    private string EventDescription(EventLog action)
    {
        string odds = "";
        if (ShowOdds && action.Odds > 0)
            odds = $" ({Math.Round(action.Odds * 100f, 2)}% success)";
        string msg;
        switch (action.Type)
        {
            case MessageLogEvent.Hit:
                return $"<b>{action.Unit.Name}</b> hit <b>{action.Target.Name}</b> with a {GetWeaponTrueName(action.Weapon, action.Unit)} for <color=red>{action.Damage}</color> points of damage.{odds}";
            case MessageLogEvent.Miss:
                msg = GenerateMissMessage(action);
                msg = msg += odds;
                return msg;
            case MessageLogEvent.Devour:
                msg = GenerateSwallowMessage(action);
                msg = msg += odds;
                return msg;
            case MessageLogEvent.Unbirth:
                msg = GenerateUBSwallowMessage(action);
                msg = msg += odds;
                return msg;
            case MessageLogEvent.CockVore:
                msg = GenerateCVSwallowMessage(action);
                msg = msg += odds;
                return msg;
            case MessageLogEvent.BreastVore:
                msg = GenerateBVSwallowMessage(action);
                msg = msg += odds;
                return msg;
            case MessageLogEvent.TailVore:
                msg = GenerateTVSwallowMessage(action);
                msg = msg += odds;
                return msg;
            case MessageLogEvent.AnalVore:
                msg = GenerateAVSwallowMessage(action);
                msg = msg += odds;
                return msg;
            case MessageLogEvent.BellyRub:
                return GenerateBellyRubMessage(action);
            case MessageLogEvent.BreastRub:
                return GenerateBreastRubMessage(action);
            case MessageLogEvent.TailRub:
                return GenerateTailRubMessage(action);
            case MessageLogEvent.BallMassage:
                return GenerateBallMassageMessage(action);
            case MessageLogEvent.TransferSuccess:
                return GetStoredMessage(StoredLogTexts.MessageTypes.TransferMessages, action);
            case MessageLogEvent.VoreStealSuccess:
                return GetStoredMessage(StoredLogTexts.MessageTypes.VoreStealMessages, action);
            //return $"<b>{action.Target.Name}</b> gently pushes down <b>{action.Unit.Name}</b> as {GPPHe(action.Target)} straddles {GPPHim(action.Unit)}. As {GPPHe(action.Target)} rides {GPPHim(action.Unit)}, {GPPHe(action.Unit)} cums, shooting {GPPHis(action.Unit)} prey straight into {GPPHis(action.Target)} {action.preyLocation.ToSyn()}.{odds}";
            case MessageLogEvent.TransferFail:
                return $"<b>{action.Unit.Name}</b> is a bit too quick, and {GPPHis(action.Unit)} prey gets partially released.";
            case MessageLogEvent.VoreStealFail:
                if (action.oldLocation == PreyLocation.breasts || action.oldLocation == PreyLocation.leftBreast || action.oldLocation == PreyLocation.rightBreast)
                    return $"<b>{action.Target.Name}</b> shoves <b>{action.Unit.Name}</b> off of {GPPHim(action.Target)} before {GPPHe(action.Unit)} can suck <b>{action.Prey.Name}</b> out of {GPPHis(action.Target)} breasts.";
                else
                    return $"<b>{action.Target.Name}</b> shoves <b>{action.Unit.Name}</b> off of {GPPHim(action.Target)} before {GPPHe(action.Unit)} can suck <b>{action.Prey.Name}</b> out of {GPPHis(action.Target)} balls.";
            case MessageLogEvent.Feed:
                return GetStoredMessage(StoredLogTexts.MessageTypes.BreastFeedMessages, action);
            case MessageLogEvent.FeedCum:
                return GetStoredMessage(StoredLogTexts.MessageTypes.CumFeedMessages, action);
            case MessageLogEvent.Suckle:
                if (action.preyLocation == PreyLocation.breasts || action.preyLocation == PreyLocation.leftBreast || action.preyLocation == PreyLocation.rightBreast)
                    return $"<b>{action.Unit.Name}</b> hugs <b>{action.Target.Name}</b>, pinning {GPPHis(action.Target)} arms to {GPPHis(action.Target)} sides as {GPPHe(action.Unit)} starts sucking on {GPPHis(action.Target)} breasts!{odds}";
                else
                    return $"<b>{action.Unit.Name}</b> knocks down <b>{action.Target.Name}</b> and begins sucking {GPPHis(action.Target)} rod.{odds}";
            case MessageLogEvent.SuckleFail:
                if (action.preyLocation == PreyLocation.breasts || action.preyLocation == PreyLocation.leftBreast || action.preyLocation == PreyLocation.rightBreast)
                    return $"<b>{action.Unit.Name}</b> hugs <b>{action.Target.Name}</b>, but {GPPHe(action.Target)} breaks free from {GPPHis(action.Unit)} hold before {action.Unit.Name} can do anything!{odds}";
                else
                    return $"<b>{action.Unit.Name}</b> tries to knock down <b>{action.Target.Name}</b>, but {action.Target.Name} stands {GPPHis(action.Target)} ground!{odds}";
            case MessageLogEvent.Birth:
                return GenerateBirthMessage(action);
            case MessageLogEvent.CumGestation:
                return GenerateCumGestationMessage(action);
            case MessageLogEvent.Resist:
                return $"<b>{action.Unit.Name}</b> tried to vore <b>{action.Target.Name}</b>, but was fought off.{odds}";
            case MessageLogEvent.Kill:
                return GenerateKillMessage(action);
            case MessageLogEvent.Digest:
                return GenerateDigestionDeathMessage(action);
            case MessageLogEvent.Absorb:
                return GenerateAbsorptionMessage(action);
            case MessageLogEvent.Escape:
                return GenerateEscapeMessage(action, odds);
            case MessageLogEvent.PartialEscape:
                return $"<b>{action.Target.Name}</b> escaped from <b>{action.Unit.Name}</b>'s second stomach, only to find {GPPHimself(action.Target)} back in the first stomach.{odds}";
            case MessageLogEvent.Freed:
                return $"<b>{action.Target.Name}</b> was freed because <b>{action.Unit.Name}</b> died.";
            //$"<b>{action.Target.Name}</b> sees insides of {action.preyLocation.ToSyn()} around him melting, only to find {GPPHimself(action.Target)} <b>{action.Unit.Name}</b>'s {action.preyLocation.ToSyn()}{odds}"
            case MessageLogEvent.Regurgitated:
                return $"<b>{action.Unit.Name}</b> hears {GPPHis(action.Unit)} comrade's plea for help and regurgitates <b>{action.Target.Name}</b>.";
            case MessageLogEvent.Heal:
                if (new[] { "breastfeeding", "cumfeeding" }.Contains(action.Message))
                {
                    string message = "";
                    if (action.Damage != 0 && action.Bonus == 0)
                        message = $"<b>{action.Unit.Name}</b> <color=blue>healed {action.Damage}</color> from the milk.";
                    else if (action.Damage != 0 && action.Bonus != 0)
                        message = $"<b>{action.Unit.Name}</b> <color=blue>healed {action.Damage}</color> and <color=blue>gained {action.Bonus} experience</color> from the milk.";
                    else
                        message = $"<b>{action.Unit.Name}</b> <color=blue>gained {action.Bonus} experience</color> from the milk.";
                    if (action.Extra == "honey")
                        message = message.Replace("milk", "honey");
                    else if (action.Message == "cumfeeding")
                        message = message.Replace("milk", "cum");
                    return message;
                }
                return $"<b>{action.Unit.Name}</b> <color=blue>healed {action.Damage}</color> from absorbing {GPPHis(action.Unit)} prey.";
            case MessageLogEvent.NewTurn:
                return action.Message;
            case MessageLogEvent.LowHealth:
                return GenerateDigestionLowHealthMessage(action);
            case MessageLogEvent.Miscellaneous:
                return action.Message;
            case MessageLogEvent.RandomDigestion:
                return GenerateRandomDigestionMessage(action);
            case MessageLogEvent.Dazzle:
                return $"<b>{action.Unit.Name}</b> was dazzled by <b>{action.Target.Name}</b>, the distraction wasting {GPPHis(action.Unit)} turn.{odds}";
            case MessageLogEvent.SpellCast:
                msg = GenerateSpellCastMessage((SpellLog)action);
                return msg;
            case MessageLogEvent.SpellHit:
                msg = GenerateSpellHitMessage((SpellLog)action);
                msg = msg += odds;
                return msg;
            case MessageLogEvent.SpellMiss:
                msg = GenerateSpellMissMessage((SpellLog)action);
                msg = msg += odds;
                return msg;
            case MessageLogEvent.SpellKill:
                string spellName = SpellList.SpellDict[((SpellLog)action).SpellType].Name;
                return $"<b>{action.Unit.Name}</b> killed <b>{action.Target.Name}</b> with the {spellName} spell.";
            case MessageLogEvent.CurseExpires:
                return GenerateCurseExpiringMessage(action);
            case MessageLogEvent.DiminishmentExpires:
                return GenerateDiminshmentExpiringMessage(action);
            case MessageLogEvent.GreatEscapeKeep:
                return GenerateGreatEscapeKeepMessage(action);
            case MessageLogEvent.GreatEscapeFlee:
                return GenerateGreatEscapeFleeMessage(action);
            case MessageLogEvent.ManualRegurgitation:
                return GenerateRegurgitationMessage(action);
            // return $"<b>{action.Unit.Name}</b> triggers my test message by regurgitating <b>{action.Target.Name}</b>.";
            case MessageLogEvent.Forcefeeding:
                return GenerateForcefeedingMessage(action);
            case MessageLogEvent.ForcefeedFail:
                return GenerateForcefeedFailMessage(action);
            case MessageLogEvent.TraitConvert:
                return GenerateTraitConvertMessage(action);
            case MessageLogEvent.TraitRebirth:
                return GenerateTraitRebirthMessage(action);
            default:
                return string.Empty;
        }
    }
    private string GenerateBreastRubMessage(EventLog action)
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b> massages {(action.Unit == action.Target ? GPPHis(action.Target) : "<b>" + action.Target.Name + "</b>'s")} full breasts.";
        return GetStoredMessage(StoredLogTexts.MessageTypes.BreastRubMessages, action);
    }

    private string GenerateTailRubMessage(EventLog action)
    {
        if (SimpleText)
        {
            if (action.Unit.Race == Race.Terrorbird)
                return $"<b>{action.Unit.Name}</b> massages {(action.Unit == action.Target ? GPPHis(action.Target) : "<b>" + action.Target.Name + "</b>'s")} filled crop.";
            else
                return $"<b>{action.Unit.Name}</b> massages {(action.Unit == action.Target ? GPPHis(action.Target) : "<b>" + action.Target.Name + "</b>'s")} stuffed tail.";
        }
        return GetStoredMessage(StoredLogTexts.MessageTypes.TailRubMessages, action);
    }


    private string GenerateBallMassageMessage(EventLog action)
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b> massages {(action.Unit == action.Target ? GPPHis(action.Target) : "<b>" + action.Target.Name + "</b>'s")} full scrotum.";
        return GetStoredMessage(StoredLogTexts.MessageTypes.BallMassageMessages, action);
    }

    string GenerateSpellCastMessage(SpellLog action)
    {
        var spell = SpellList.SpellDict[action.SpellType];
        switch (action.SpellType)
        {
            case SpellTypes.IceBlast:
                if (action.Unit.Race == Race.Dragon)
                    return $"<b>{action.Unit.Name}</b> lifts up into the air, opening {GPPHis(action.Unit)} maw and spewing out a blast of frost!";
                goto default;
            case SpellTypes.Pyre:
                if (action.Unit.Race == Race.Dragon)
                    return $"<b>{action.Unit.Name}</b> lifts up into the air, opening {GPPHis(action.Unit)} maw and spewing out a cone of fire!";
                goto default;
            case SpellTypes.AlraunePuff:
                return $"<b>{action.Unit.Name}</b> expels a cloud of pollen!";
            case SpellTypes.Web:
                if (action.Unit.Race == Race.Driders)
                    return $"<b>{action.Unit.Name}</b> looses a stream of silk from {GPPHis(action.Unit)} abdomen!";
                goto default;
            case SpellTypes.ViperPoison:
                return $"<b>{action.Unit.Name}</b> spits out a {GetRandomStringFrom("gob", "glob", "blob")} of venom!";
            case SpellTypes.Petrify:
                return $"<b>{action.Unit.Name}</b> steadies {GPPHis(action.Unit)} eyes into a death stare!";
            case SpellTypes.GlueBomb:
                return $"<b>{action.Unit.Name}</b>'s glue bomb affected <b>{action.Target.Name}</b>.";
            case SpellTypes.HypnoGas:
                if (Config.FartOnAbsorb)
                    return $"<b>{action.Unit.Name}</b> looses an oddly alluring fart {GPPHis(action.Unit)} rump...";
                else if (Config.BurpFraction > 0)
                    return $"<b>{action.Unit.Name}</b> lets out a deep belch with a peculiarly enticing scent...";
                return $"<b>{action.Unit.Name}</b> emits a bewitching gas!";
            case SpellTypes.Bind:
                return $"<b>{action.Unit.Name}</b> casts a binding spell!";
            default:
                return $"<b>{action.Unit.Name}</b> casts <b>{spell.Name}</b>!";
        }
    }

    string GenerateSpellHitMessage(SpellLog action)
    {
        var spell = SpellList.SpellDict[action.SpellType];
        switch (action.SpellType)
        {
            case SpellTypes.Shield:
                return $"<b>{action.Target.Name}</b>'s body toughens!";
            case SpellTypes.Mending:
                return $"A regenerative force begins slowly healing <b>{action.Target.Name}</b>'s wounds!";
            case SpellTypes.Speed:
                return $"<b>{action.Target.Name}</b>'s movement quickens!";
            case SpellTypes.Valor:
                return $"<b>{action.Target.Name}</b>'s attacks are empowered!";
            case SpellTypes.Predation:
                return $"<b>{action.Target.Name}</b>'s voracious instincts are roused!";
            case SpellTypes.Poison:
                return $"<b>{action.Target.Name}</b> was poisoned!";
            case SpellTypes.PreysCurse:
                return $"<b>{action.Target.Name}</b> now suffers the Prey's Curse!";
            case SpellTypes.Maw:
            case SpellTypes.GateMaw:
                return $"<b>{action.Target.Name}</b> was teleported into <b>{action.Unit.Name}</b>'s {action.preyLocation.ToSyn()}!";
            case SpellTypes.Charm:
                return $"<b>{action.Target.Name}</b> was charmed!";
            case SpellTypes.Enlarge:
                return $"<b>{action.Target.Name}</b>'s body grows larger!";
            case SpellTypes.Diminishment:
                return $"<b>{action.Target.Name}</b>'s body shrinks down!";
            case SpellTypes.ViralInfection:
                return $"<b>{action.Target.Name}</b> was infected with a consumptive virus!";
            case SpellTypes.DivinitysEmbrace:
                return $"<b>{action.Target.Name}</b> feels refreshed as <b>{action.Unit.Name}</b>'s protective spell envelops {GPPHim(action.Target)}!";
            case SpellTypes.Resurrection:
                return $"<b>{action.Target.Name}</b> rises again!";
            case SpellTypes.AmplifyMagic:
                return $"<b>{action.Target.Name}</b>'s will becomes more resolute!";
            case SpellTypes.Evocation:
                return $"<b>{action.Target.Name}</b> gained {action.Damage} Spell Force stacks and {action.Damage / 2} AP!";
            case SpellTypes.AlraunePuff:
                return $"<b>{action.Target.Name}</b> was weakened by the pollen!";
            case SpellTypes.Web:
                return $"<b>{action.Target.Name}</b> was webbed!";
            case SpellTypes.GlueBomb:
                return $"<b>{action.Target.Name}</b> was greatly slowed by the sticky fluid!";
            case SpellTypes.ViperPoison:
                return $"<b>{action.Target.Name}</b> was badly poisoned!";
            case SpellTypes.Petrify:
                return $"<b>{action.Target.Name}</b> was turned to stone!";
            case SpellTypes.HypnoGas:
                return $"<b>{action.Target.Name}</b> was enamored by the scent, {GPPHis(action.Target)} mind escaping {GPPHim(action.Target)}!";
            case SpellTypes.Whispers:
                return $"<b>{action.Target.Name}</b> can no longer think of anything but being <b>{action.Unit.Name}</b>'s prey!";
            default:
                return $"<b>{action.Target.Name}</b> took <color=red>{action.Damage}</color> damage!";
        }
    }

    string GenerateSpellMissMessage(SpellLog action)
    {
        if (action.SpellType == SpellTypes.None)
            return "";
        // var spell = SpellList.SpellDict[action.SpellType];
        return $"<b>{action.Target.Name}</b> was unaffected!";
    }

    private string GenerateMissMessage(EventLog action)
    {
        int rand = Random.Range(0, 3);
        switch (rand)
        {
            case 0:
                return $"<b>{action.Unit.Name}</b> missed <b>{action.Target.Name}</b> with {GPPHis(action.Unit)} {GetWeaponTrueName(action.Weapon, action.Unit)}.";
            case 1:
                {
                    if (action.Weapon.Range > 1) return $"<b>{action.Unit.Name}</b> took a shot at <b>{action.Target.Name}</b> with {GPPHis(action.Unit)} {GetWeaponTrueName(action.Weapon, action.Unit)}, but missed.";
                    else return $"<b>{action.Unit.Name}</b> struck at <b>{action.Target.Name}</b> with {GPPHis(action.Unit)} {GetWeaponTrueName(action.Weapon, action.Unit)}, but went wide.";
                }
            default:
                return $"<b>{action.Target.Name}</b> dodged <b>{action.Unit.Name}</b>'s attempted attack with {GPPHis(action.Unit)} {GetWeaponTrueName(action.Weapon, action.Unit)}.";
        }
    }

    /// <summary>
    /// Generates a message for the tactical log when a unit dies through damage from a weapon.
    /// </summary>
    private string GenerateKillMessage(EventLog action)
    {
        int rand = Random.Range(0, 6);
        switch (rand)
        {
            case 0: return $"<b>{action.Unit.Name}</b> killed <b>{action.Target.Name}</b> with {GPPHis(action.Unit)} {GetWeaponTrueName(action.Weapon, action.Unit)}.";
            case 1: return $"<b>{action.Target.Name}</b> was brought down by <b>{action.Unit.Name}</b>'s {GetWeaponTrueName(action.Weapon, action.Unit)}.";
            case 2: return $"<b>{action.Target.Name}</b>'s fight was brought to an end by <b>{action.Unit.Name}</b>'s {GetWeaponTrueName(action.Weapon, action.Unit)}.";
            case 3:
                if (action.Weapon.Range > 1) return $"<b>{action.Target.Name}</b> was struck down by an accurate hit of <b>{action.Unit.Name}'s</b> {GetWeaponTrueName(action.Weapon, action.Unit)}.";
                else return $"<b>{action.Unit.Name}</b> struck <b>{action.Target.Name}</b> down with a skilled strike of {GPPHis(action.Unit)} {GetWeaponTrueName(action.Weapon, action.Unit)}.";
            case 4: return $"<b>{action.Target.Name}</b> was slain by <b>{action.Unit.Name}</b> wielding {GPPHis(action.Unit)} {GetWeaponTrueName(action.Weapon, action.Unit)}.";
            default: return $"<b>{action.Unit.Name}</b> put an end to <b>{action.Target.Name}</b> with {GPPHis(action.Unit)} {GetWeaponTrueName(action.Weapon, action.Unit)}.";
        }
    }

    private string GenerateSwallowMessage(EventLog action)  // Oral vore devouring messages.
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b> ate <b>{action.Target.Name}</b>.";

        return GetStoredMessage(StoredLogTexts.MessageTypes.SwallowMessages, action);


    }

    private string GenerateBVSwallowMessage(EventLog action)
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b> breast vores <b>{action.Target.Name}</b>.";
        return GetStoredMessage(StoredLogTexts.MessageTypes.BreastVoreMessages, action);
    }

    string GenerateCurseExpiringMessage(EventLog action)
    {
        if (action.preyLocation == PreyLocation.stomach || action.preyLocation == PreyLocation.stomach2)
        {
            return GetRandomStringFrom(
                $"As the curse comes to an end, <b>{action.Target.Name}</b> tries and figures out where {GPPHeIs(action.Target)} and begins to scream in horror as {GPPHe(action.Target)} realize{SIfSingular(action.Target)} what’s happened.",
                $"<b>{action.Unit.Name}</b> begins to worry as the curse on {GPPHis(action.Unit)} meal wears off. Surprisingly though, <b>{action.Target.Name}</b> continues to massage {GPPHis(action.Unit)} belly walls with enthusiasm.",
                $"<b>{action.Unit.Name}</b>’s tummy goes from a smooth, gentle surface to a sudden mass of angry rippling as {GPPHis(action.Unit)} previously willing prey realizes {GPPHe(action.Target)} {HasHave(action.Target)} been tricked.",
                $"<b>{action.Target.Name}</b> confusedly asks where {GPPHeIs(action.Target)} as the spell breaks. <b>{action.Unit.Name}</b> tells {GPPHim(action.Target)} {GPPHeIsAbbr(action.Target)} exactly where {GPPHeIsAbbr(action.Target)} meant to be as {GPPHe(action.Unit)} lovingly embraces {GPPHis(action.Unit)} swollen stomach."
                );
        }
        else
        {
            return GetRandomStringFrom(
                $"As the curse comes to an end, <b>{action.Target.Name}</b> tries and figures out where {GPPHeIs(action.Target)} and begins to scream in horror as {GPPHe(action.Target)} realize{SIfSingular(action.Target)} what’s happened.",
                $"<b>{action.Target.Name}</b> confusedly asks where {GPPHeIs(action.Target)} as the spell breaks. <b>{action.Unit.Name}</b> tells {GPPHim(action.Target)} {GPPHeIsAbbr(action.Target)} exactly where {GPPHeIsAbbr(action.Target)} meant to be as {GPPHe(action.Unit)} lovingly embraces {GPPHis(action.Unit)} swollen stomach."
            );
        }
    }

    string GenerateDiminshmentExpiringMessage(EventLog action)
    {
        if (action.preyLocation == PreyLocation.stomach || action.preyLocation == PreyLocation.stomach2)
        {
            return GetRandomStringFrom(
                $"<b>{action.Unit.Name}</b>’s stomach expands violently as {GPPHis(action.Unit)} previously diminutive prey reverts to {GPPHis(action.Target)} regular size.",
                $"<b>{action.Unit.Name}</b> falls onto the ground as {GPPHis(action.Unit)} belly is suddenly filled with a full-sized {action.Target.Race}. {GPPHe(action.Unit)} rubs {GPPHis(action.Unit)} engorged gut before standing once more.",
                $"<b>{action.Unit.Name}</b> had been eagerly waiting for {GPPHis(action.Unit)} tiny meal to revert to its regular size. When {GPPHis(action.Unit)} gut finally expands, the air is filled with {GPPHis(action.Unit)} cries of pleasure and a great sloshing.",
                $"<b>{action.Unit.Name}</b>’s tummy nearly bursts as <b>{action.Target.Name}</b> reverts to {GPPHis(action.Target)} usual size."
                );
        }
        else
        {
            return GetRandomStringFrom(
                $"<b>{action.Unit.Name}</b>’s {PreyLocStrings.ToSyn(action.preyLocation)} expands violently as {GPPHis(action.Unit)} previously diminutive prey reverts to {GPPHis(action.Target)} regular size.",
                $"<b>{action.Unit.Name}</b> had been eagerly waiting for {GPPHis(action.Unit)} tiny meal to revert to its regular size. When {GPPHis(action.Unit)} {PreyLocStrings.ToSyn(action.preyLocation)} finally expands, the air is filled with {GPPHis(action.Unit)} cries of pleasure and a great sloshing."
            );
        }

    }

    private string GenerateEscapeMessage(EventLog action, string odds)
    {
        if (SimpleText)
        {
            return $"<b>{action.Target.Name}</b> escaped from <b>{action.Unit.Name}</b>'s {action.preyLocation.ToSyn()}.{odds}";
        }
        if (action.preyLocation == PreyLocation.stomach)
        {
            if (action.Target.Race < Race.Vagrants || action.Target.Race >= Race.Selicia) // Prey Humanoid
            {
                if (action.Unit.Race < Race.Vagrants || action.Unit.Race >= Race.Selicia) // Pred Humanoid
                    return GetRandomStringFrom(
                    $"From within <b>{action.Unit.Name}</b>’s gurgling gut, <b>{action.Target.Name}</b> remembers all the loved ones that would miss {GPPHim(action.Target)} and with this incentive forces {GPPHis(action.Target)} way out.{odds}",
                    $"<b>{action.Unit.Name}</b>’s stomach finds something particularly disagreeable with how <b>{action.Target.Name}</b> tastes. With a wretched gag, <b>{action.Target.Name}</b> is expelled from <b>{action.Unit.Name}</b>’s tummy.{odds}",
                    $"The rampant indigestion caused by <b>{action.Target.Name}</b>’s incessant struggles causes <b>{action.Unit.Name}</b> to reluctantly release {GPPHis(action.Unit)} stubborn prey.{odds}",
                    $"<b>{action.Target.Name}</b>’s determination proves greater than the strength of <b>{action.Unit.Name}</b>’s constitution as {GPPHe(action.Target)} free{SIfSingular(action.Target)} {GPPHimself(action.Unit)} from {GPPHis(action.Unit)} fleshy prison.{odds}",
                    $"<b>{action.Target.Name}</b> claws {GPPHis(action.Target)} way up <b>{action.Unit.Name}</b>’s throat and is able to pull {GPPHimself(action.Target)} free.{odds}",
                    $"<b>{action.Unit.Name}</b> can feel the tip of a weapon stabbing at {GPPHis(action.Unit)} insides. Panicking, the worried predator spits <b>{action.Target.Name}</b> up quickly.{odds}",
                    $"<b>{action.Target.Name}</b> tricks {GPPHis(action.Target)} would-be predator with a heartfelt sob story. <b>{action.Unit.Name}</b> believes it and naïvely lets the clever prey climb out of {GPPHis(action.Unit)} gullet.{odds}",
                    $"<b>{action.Target.Name}</b> becomes terrified as the acids begin to tear into {GPPHis(action.Target)} flesh and in a sudden bout of panic forces <b>{action.Unit.Name}</b> to throw {GPPHim(action.Target)} up.{odds}",
                    $"<b>{action.Unit.Name}</b> relaxes and arrogantly pats {GPPHis(action.Unit)} swollen belly while taunting {GPPHis(action.Unit)} prey; {GPPHeIsAbbr(action.Unit)} taken by surprise as <b>{action.Target.Name}</b> uses the moment of relaxation to fight {GPPHis(action.Target)} way out.{odds}",
                    $"<b>{action.Unit.Name}</b> watches with concern as {GPPHis(action.Unit)} belly suddenly lets out an angry roar. <b>{action.Target.Name}</b> had kept a number of inedible herbs for just this occasion and as they break down they force the belly to expel its contents.{odds}"
                    );
                else  // Pred Feral
                    return GetRandomStringFrom(
                        $"From within <b>{action.Unit.Name}</b>’s gurgling gut, <b>{action.Target.Name}</b> remembers all the loved ones that would miss {GPPHim(action.Target)} and with this incentive forces {GPPHis(action.Target)} way out.{odds}",
                        $"<b>{action.Unit.Name}</b>’s stomach finds something particularly disagreeable with how <b>{action.Target.Name}</b> tastes. With a wretched gag, <b>{action.Target.Name}</b> is expelled from <b>{action.Unit.Name}</b>’s tummy.{odds}",
                        $"The rampant indigestion caused by <b>{action.Target.Name}</b>’s incessant struggles causes <b>{action.Unit.Name}</b> to reluctantly release {GPPHis(action.Unit)} stubborn prey.{odds}",
                        $"<b>{action.Target.Name}</b>’s determination proves greater than the strength of <b>{action.Unit.Name}</b>’s constitution as {GPPHe(action.Target)} free{SIfSingular(action.Target)} {GPPHimself(action.Unit)} from {GPPHis(action.Unit)} fleshy prison.{odds}",
                        $"<b>{action.Target.Name}</b> claws {GPPHis(action.Target)} way up <b>{action.Unit.Name}</b>’s throat and is able to pull {GPPHimself(action.Target)} free.{odds}",
                        $"<b>{action.Unit.Name}</b> can feel the tip of a weapon stabbing at {GPPHis(action.Unit)} insides. Panicking, the worried predator spits <b>{action.Target.Name}</b> up quickly.{odds}",
                        $"<b>{action.Target.Name}</b> becomes terrified as the acids begin to tear into {GPPHis(action.Target)} flesh and in a sudden bout of panic forces <b>{action.Unit.Name}</b> to throw {GPPHim(action.Target)} up.{odds}",
                        $"<b>{action.Unit.Name}</b> watches with concern as {GPPHis(action.Unit)} belly suddenly lets out an angry roar. <b>{action.Target.Name}</b> had kept a number of inedible herbs for just this occasion and as they break down they force the belly to expel its contents.{odds}"
                    );
            }
            else // Prey Feral
            {
                if (action.Unit.Race < Race.Vagrants || action.Unit.Race >= Race.Selicia) // Pred Humanoid
                    return GetRandomStringFrom(
                    $"<b>{action.Unit.Name}</b>’s stomach finds something particularly disagreeable with how <b>{action.Target.Name}</b> tastes. With a wretched gag, <b>{action.Target.Name}</b> is expelled from <b>{action.Unit.Name}</b>’s tummy.{odds}",
                    $"The rampant indigestion caused by <b>{action.Target.Name}</b>’s incessant struggles causes <b>{action.Unit.Name}</b> to reluctantly release {GPPHis(action.Unit)} stubborn prey.{odds}",
                    $"<b>{action.Target.Name}</b>’s determination proves greater than the strength of <b>{action.Unit.Name}</b>’s constitution as {GPPHe(action.Target)} free{SIfSingular(action.Target)} {GPPHimself(action.Unit)} from {GPPHis(action.Unit)} fleshy prison.{odds}",
                    $"<b>{action.Target.Name}</b> claws {GPPHis(action.Target)} way up <b>{action.Unit.Name}</b>’s throat and is able to pull {GPPHimself(action.Target)} free.{odds}",
                    $"<b>{action.Target.Name}</b> becomes terrified as the acids begin to tear into {GPPHis(action.Target)} flesh and in a sudden bout of panic forces <b>{action.Unit.Name}</b> to throw {GPPHim(action.Target)} up.{odds}",
                    $"<b>{action.Unit.Name}</b> relaxes and arrogantly pats {GPPHis(action.Unit)} swollen belly while taunting {GPPHis(action.Unit)} prey; {GPPHeIsAbbr(action.Unit)} taken by surprise as <b>{action.Target.Name}</b> uses the moment of relaxation to fight {GPPHis(action.Target)} way out.{odds}",
                    $"<b>{action.Target.Name}</b>'s survival instincts take over, letting {GPPHim(action.Target)} channel a burst of near supernatural strength and setting {GPPHim(action.Target)} free.{odds}",
                    $"<b>{action.Target.Name}</b>'s natural built-in weapons proove too much to leave {GPPHim(action.Target)} contained. The irritated gut soon sets {GPPHim(action.Target)} free.{odds}"
                    );
                else  // Pred Feral
                    return GetRandomStringFrom(
                        $"<b>{action.Unit.Name}</b>’s stomach finds something particularly disagreeable with how <b>{action.Target.Name}</b> tastes. With a wretched gag, <b>{action.Target.Name}</b> is expelled from <b>{action.Unit.Name}</b>’s tummy.{odds}",
                        $"The rampant indigestion caused by <b>{action.Target.Name}</b>’s incessant struggles causes <b>{action.Unit.Name}</b> to reluctantly release {GPPHis(action.Unit)} stubborn prey.{odds}",
                        $"<b>{action.Target.Name}</b>’s determination proves greater than the strength of <b>{action.Unit.Name}</b>’s constitution as {GPPHe(action.Target)} free{SIfSingular(action.Target)} {GPPHimself(action.Unit)} from {GPPHis(action.Unit)} fleshy prison.{odds}",
                        $"<b>{action.Target.Name}</b> claws {GPPHis(action.Target)} way up <b>{action.Unit.Name}</b>’s throat and is able to pull {GPPHimself(action.Target)} free.{odds}",
                        $"<b>{action.Target.Name}</b> becomes terrified as the acids begin to tear into {GPPHis(action.Target)} flesh and in a sudden bout of panic forces <b>{action.Unit.Name}</b> to throw {GPPHim(action.Target)} up.{odds}",
                        $"<b>{action.Target.Name}</b>'s survival instincts take over, letting {GPPHim(action.Target)} channel a burst of near supernatural strength and setting {GPPHim(action.Target)} free.{odds}",
                        $"<b>{action.Target.Name}</b>'s natural built-in weapons proove too much to leave {GPPHim(action.Target)} contained. The irritated gut soon sets {GPPHim(action.Target)} free.{odds}"
                    );
            }
        }
        else if (action.preyLocation == PreyLocation.breasts)
        {
            if (action.Unit.Race == Race.Kangaroos)
            {
                return GetRandomStringFrom(
                    $"Just when all hope seemed lost, <b>{action.Target.Name}</b> manages to pry <b>{action.Unit.Name}</b>'s pouch entrance open, and clambers out, taking large breaths of fresh air. {odds}",
                    $"In the chaos of battle, <b>{action.Unit.Name}</b> leans over, causing a crease to appear in {GPPHis(action.Unit)} pouch, forcing the pouch's entrance to unseal ever-so-slightly. <b>{action.Target.Name}</b> seizes the opportunity, clawing {GPPHis(action.Target)} way out, and taking several deep, victorious gulps of real air.",
                    $"Rather suddenly, a blade pokes out of <b>{action.Unit.Name}</b>'s pouch's entrance, a knife or dagger of some kind. \"Let me out right now, or I'll carve your whole stupid pouch off,\" <b>{action.Target.Name}</b> angrily demands. <b>{action.Unit.Name}</b>, who would rather not be mutilated, caves and quickly pushes <b>{action.Target.Name}</b> out.",
                    $"Rather suddenly, a blade pokes out of <b>{action.Unit.Name}</b>'s pouch's entrance, a knife or dagger of some kind. \"Let me out right now, or I'll carve your whole stupid pouch off,\" <b>{action.Target.Name}</b> angrily demands. Seeing no other option, <b>{action.Unit.Name}</b> opens {GPPHis(action.Unit)} pouch, and <b>{action.Target.Name}</b> quickly jumps out.",
                    $"In the chaos of battle, <b>{action.Unit.Name}</b> leans over, allowing <b>{action.Target.Name}</b> an opportunity to escape, which {GPPHe(action.Target)} take{SIfSingular(action.Target)} quite happily.",
                    $"As a blade pokes out of <b>{action.Unit.Name}</b>'s pouch's entrance, <b>{action.Target.Name}</b> demands to be let out. Within moments, <b>{action.Unit.Name}</b> complies."
                );
            }
            else
                return GetRandomStringFrom(
                    $"As the jiggling of <b>{ApostrophizeWithOrWithoutS(action.Unit.Name)}</b> {GetRandomStringFrom("breasts", "boobs", "tits")} hit a peak, a hand suddenly stretches out from between them. This hand is soon followed by the rest of <b>{action.Target.Name}</b>, pulling {GPPHimself(action.Target)} out. {odds}",
                    $"<b>{ApostrophizeWithOrWithoutS(action.Unit.Name)}</b> {GetRandomStringFrom("breasts", "boobs", "tits")} begin to bounce up and down, up and down, faster and faster until <b>{action.Target.Name}</b> is launched out from between them. {odds}",
                    $"As <b>{ApostrophizeWithOrWithoutS(action.Unit.Name)}</b> {GetRandomStringFrom("breasts", "boobs", "tits")} begin bouncing up in attempts to hit {GPPHis(action.Unit)} face, <b>{action.Unit.Name}</b> releases <b>{action.Target.Name}</b> rather than suffer \"death by sentient boob fat.\" {odds}"
                );
        }
        else
        {
            return GetRandomStringFrom(
                $"<b>{action.Target.Name}</b> escaped from <b>{action.Unit.Name}</b>'s {action.preyLocation.ToSyn()}.{odds}",
                $"From within <b>{action.Unit.Name}</b>’s {action.preyLocation.ToSyn()}, <b>{action.Target.Name}</b> remembers all the loved ones that would miss {GPPHim(action.Target)}, and with this incentive forces {GPPHis(action.Target)} way out.{odds}",
                $"<b>{action.Target.Name}</b>’s determination proves greater than the strength of <b>{action.Unit.Name}</b>’s constitution as {GPPHe(action.Target)} free{SIfSingular(action.Target)} {GPPHimself(action.Target)} from {GPPHis(action.Target)} fleshy prison.{odds}",
                $"<b>{action.Unit.Name}</b> can feel the tip of a weapon stabbing at {GPPHis(action.Unit)} insides. Panicking, the worried predator spits <b>{action.Target.Name}</b> up quickly.{odds}",
                $"<b>{action.Target.Name}</b> tricks {GPPHis(action.Target)} would-be predator with a heartfelt sob story. <b>{action.Unit.Name}</b> believes it and naïvely lets the clever prey climb back out.{odds}"
            );
        }

    }

    private string GenerateRegurgitationMessage(EventLog action)
    {
        if (SimpleText)
        {
            return $"<b>{action.Unit.Name}</b> regurgitates <b>{action.Target.Name}</b>.";
        }
        List<string> possibleLines = new List<string>();
        if (action.Unit.Race == Race.Slimes)
        {
            possibleLines.Add($"As <b>{action.Unit.Name}</b> moves forward, {GPPHis(action.Unit)} slimey body contorts, leaving behind <b>{action.Target.Name}</b>, covered in goo, but otherwise alive.");
            if (action.Target.Race != Race.Slimes)
                possibleLines.Add($"For a moment, <b>{action.Unit.Name}</b> appears to be undergoing mitosis, splitting in half. Then, one half pulls itself off a slightly freaked out <b>{action.Target.Name}</b>, the other becoming <b>{action.Unit.Name}</b> once again.");
            else
                possibleLines.Add($"For a moment, <b>{action.Unit.Name}</b> appears to be undergoing mitosis, splitting in half. Then, one half begins to shift slightly as <b>{action.Target.Name}</b> becomes a seperate slime once more.");
            return GetRandomStringFrom(possibleLines.ToArray());
        }
        possibleLines.Add($"<b>{action.Unit.Name}</b> {GetRandomStringFrom("regurgitated", "released", "freed", "pushed out")} <b>{action.Target.Name}</b>{GetRandomStringFrom("", $"from {GPPHis(action.Unit)} {PreyLocStrings.ToSyn(action.preyLocation)}")}.");
        possibleLines.Add($"<b>{action.Unit.Name}</b> decides to eject <b>{action.Target.Name}</b> from {GPPHis(action.Unit)} {PreyLocStrings.ToSyn(action.preyLocation)}.");
        possibleLines.Add($"As <b>{action.Unit.Name}</b> hears a gurgle{GetRandomStringFrom("", $" eminate from {GPPHis(action.Unit)} {PreyLocStrings.ToSyn(action.preyLocation)}")}, {GPPHe(action.Unit)} force{SIfSingular(action.Unit)} <b>{action.Target.Name}</b> out, not wishing to digest {GPPHim(action.Target)}.");
        if (action.preyLocation == PreyLocation.stomach || action.preyLocation == PreyLocation.anal)
        {
            possibleLines.Add($"<b>{action.Target.Name}</b> was released from <b>{action.Unit.Name}</b>'s stomach.");
            possibleLines.Add($"With a great heave, <b>{action.Unit.Name}</b> {GetRandomStringFrom("vomits out", "spits out", "pukes up", "coughs up")} a still living <b>{action.Target.Name}</b>.");
            if (Config.Scat)
                possibleLines.Add($"As <b>{action.Unit.Name}</b>'s guts grumble, and the bulge <b>{action.Target.Name}</b> makes seems to move downwards, <b>{action.Unit.Name}</b> is briefly worried that {GPPHe(action.Unit)} killed <b>{action.Target.Name}</b>. <b>{action.Unit.Name}</b> hurridly does a series of clenches to force <b>{action.Target.Name}</b> out, and is relieved when, instead of shit, a perfectly healthy <b>{action.Target.Name}</b> slides out {GPPHis(action.Unit)} anus.");
            switch (action.preyLocation)
            {
                case PreyLocation.stomach:
                    possibleLines.Add($"<b>{action.Unit.Name}</b>, not wishing to {(action.Unit.HasTrait(Traits.Endosoma) && action.Unit.Side == action.Target.Side ? $"carry around <b>{action.Target.Name}</b> any longer" : $"digest <b>{action.Target.Name}</b>")}, sticks a finger in {GPPHis(action.Unit)} throat and {GetRandomStringFrom("vomits out", "throws up", "coughs up")} {GetRandomStringFrom($"<b>{action.Target.Name}</b>", $"the {GetRaceDescSingl(action.Target)}")}.");
                    possibleLines.Add($"<b>{action.Target.Name}</b> was released back out <b>{action.Unit.Name}</b>'s mouth.");
                    possibleLines.Add($"<b>{action.Unit.Name}</b> pushes up on the bulge <b>{action.Target.Name}</b> makes in {GPPHis(action.Unit)} belly. It isn't long before <b>{action.Target.Name}</b> is pushed back out {GetRandomStringFrom($"the way {GPPHe(action.Target)} came in", $"<b>{action.Unit.Name}</b>'s mouth")}.");
                    possibleLines.Add($"<b>{action.Unit.Name}</b> pushes down on the bulge <b>{action.Target.Name}</b> makes in {GPPHis(action.Unit)} belly. For a moment, this appears to do nothing aside from cause <b>{action.Target.Name}</b> some discomfort. Then, <b>{action.Target.Name}</b> emerges intact from {GetRandomStringFrom($"<b>{action.Unit.Name}</b>", $"the {GetRaceDescSingl(action.Unit)}")}'s ass!");
                    possibleLines.Add($"<b>{action.Unit.Name}</b> pushes down on the bulge <b>{action.Target.Name}</b> makes in {GPPHis(action.Unit)} belly. For a moment, this appears to do nothing aside from cause <b>{action.Target.Name}</b> some discomfort. Then, <b>{action.Target.Name}</b> emerges intact from {GetRandomStringFrom($"<b>{action.Unit.Name}</b>", $"the {GetRaceDescSingl(action.Unit)}")}'s ass! Having completed a full tour through <b>{action.Unit.Name}</b>'s body, <b>{action.Target.Name}</b> simply stands there, confused.");
                    break;
                case PreyLocation.anal:
                    possibleLines.Add($"<b>{action.Target.Name}</b> was released back out <b>{action.Unit.Name}</b>'s asshole.");
                    possibleLines.Add($"<b>{action.Unit.Name}</b> pushes down on the bulge <b>{action.Target.Name}</b> makes in {GPPHis(action.Unit)} gut. It isn't long before <b>{action.Target.Name}</b> is pushed back out {GetRandomStringFrom($"the way {GPPHe(action.Target)} came in", $"<b>{action.Unit.Name}</b>'s anus")}.");
                    possibleLines.Add($"<b>{action.Unit.Name}</b> pushes up on the bulge <b>{action.Target.Name}</b> makes in {GPPHis(action.Unit)} gut. It isn't long before <b>{action.Target.Name}</b>'s face appears in the back of <b>{action.Unit.Name}</b>'s throat, before being promptly spat all the way out.");
                    possibleLines.Add($"<b>{action.Unit.Name}</b> pushes up on the bulge <b>{action.Target.Name}</b> makes in {GPPHis(action.Unit)} gut. It isn't long before <b>{action.Target.Name}</b>'s face appears in the back of <b>{action.Unit.Name}</b>'s throat, before being promptly spat all the way out. Having made it all the way through <b>{action.Unit.Name}</b> going the wrong way, {GetRandomStringFrom($"<b>{action.Target.Name}</b>", $"the {GetRaceDescSingl(action.Target)}")} shudders, usure what to do next.");
                    possibleLines.Add($"As <b>{action.Unit.Name}</b> clenches, <b>{action.Target.Name}</b> can feel {GPPHimself(action.Target)} being pulled back down into {GetRandomStringFrom($"<b>{action.Target.Name}</b>", $"the {GetRaceDescSingl(action.Target)}")}'s intestines. It isn't long before {GPPHeIs(action.Target)} pushed back out <b>{action.Unit.Name}</b>'s {GetRandomStringFrom("butt", "ass", "asshole", "anus", "rectum")}, smelly but alive.");
                    break;
                default:
                    return $"What the hell happened? The prey was in the stomach somewhere and now they're not. Message Scarabyte on Discord, please.";
            }
        }
        else if (action.preyLocation == PreyLocation.balls)
        {
            possibleLines.Add($"<b>{action.Unit.Name}</b> reaches down and strokes {GPPHis(action.Unit)} throbbing cock. Once <b>{action.Unit.Name}</b> climaxes, alongside the expected cum emerges <b>{action.Target.Name}</b>{GetRandomStringFrom($"", $", sticky and wet but otherwise unharmed")}.");
            possibleLines.Add($"<b>{action.Unit.Name}</b> faps <b>{action.Target.Name}</b> out of {GPPHis(action.Unit)} {PreyLocStrings.ToSyn(action.preyLocation)}.");
            possibleLines.Add($"<b>{action.Unit.Name}</b> presses upwards on the underside of {GPPHis(action.Unit)} {PreyLocStrings.ToSyn(action.preyLocation)}, forcing <b>{action.Target.Name}</b> {GetRandomStringFrom($"out", $"to re-emerge from {GPPHis(action.Unit)} {PreyLocStrings.ToCockSyn()}")}.");
            possibleLines.Add($"<b>{action.Target.Name}</b> was released from <b>{action.Unit.Name}</b>'s balls.");
            possibleLines.Add($"As <b>{action.Unit.Name}</b> clenches, {GPPHis(action.Unit)} balls shrink inwards, showing the whole of <b>{action.Target.Name}</b>'s trapped form. Slowly, that form moves upwards, sliding up <b>{action.Unit.Name}</b>'s cock, before <b>{action.Target.Name}</b> is extruded from the tip.");
            possibleLines.Add($"After nearly tripping on {GPPHis(action.Unit)} own engorged {PreyLocStrings.ToSyn(action.preyLocation)}, <b>{action.Unit.Name}</b> decides enough is enough, and quickly {GetRandomStringFrom("faps", "forces", "pushes", "cums")} <b>{action.Target.Name}</b> out{GetRandomStringFrom("", $", not even checking if <b>{action.Target.Name}</b> survived or if {GPPHe(action.Target)} became a puddle of {GetRandomStringFrom($"{GetRaceDescSingl(action.Unit)} {GetRandomStringFrom("jizz", "cum", "spunk")}", $"{GetRandomStringFrom("jizz", "cum", "spunk")}", $"{GetRaceDescSingl(action.Target)} batter")}")}.");
        }
        else if (action.preyLocation == PreyLocation.womb)
        {
            possibleLines.Add($"<b>{action.Unit.Name}</b> reaches down and rubs {GPPHis(action.Unit)} soaking vagina. Once <b>{action.Unit.Name}</b> climaxes, alongside the expected {PreyLocStrings.ToFluid(action.preyLocation)} emerges <b>{action.Target.Name}</b>{GetRandomStringFrom($"", $", sticky and wet but otherwise unharmed")}.");
            possibleLines.Add($"<b>{action.Unit.Name}</b> decides to \"rebirth\" <b>{action.Target.Name}</b> into this world, sliding {GPPHim(action.Target)} out of {GPPHis(action.Unit)} {PreyLocStrings.ToSyn(action.preyLocation)}.");
            possibleLines.Add($"<b>{action.Unit.Name}</b> decides to push <b>{action.Target.Name}</b> back out of {GPPHis(action.Unit)} {PreyLocStrings.ToSyn(action.preyLocation)}, silently {GetRandomStringFrom($"hop", $"pray")}ing that {GPPHe(action.Unit)}'ll get to stick {GetRandomStringFrom($"<b>{action.Target.Name}</b>", $"{GPPHim(action.Target)}")} right back in.");
            possibleLines.Add($"<b>{action.Target.Name}</b> was released from <b>{action.Unit.Name}</b>'s womb.");
        }
        else if (action.preyLocation == PreyLocation.breasts || action.preyLocation == PreyLocation.leftBreast || action.preyLocation == PreyLocation.rightBreast)
        {
            if (action.Unit.Race == Race.Kangaroos)
            {
                possibleLines.Add($"\"Okay, ride's over.\" As <b>{action.Unit.Name}</b> speaks these words to <b>{action.Target.Name}</b>, {GPPHe(action.Unit)} push{EsIfSingular(action.Unit)} upwards on the bottom of {GPPHis(action.Unit)} pouch, forcing <b>{action.Target.Name}</b> out and onto the ground.");
                possibleLines.Add($"As <b>{action.Unit.Name}</b> nearly falls over, {GPPHe(action.Unit)} say{SIfSingular(action.Unit)}, \"Ugh, Mom always made this look so easy. You know what? Out.\" With that, <b>{action.Unit.Name}</b> unceremoniously shoves <b>{action.Target.Name}</b> out of {GPPHis(action.Unit)} pouch.");
                possibleLines.Add($"As <b>{action.Target.Name}</b> moves around inside <b>{action.Unit.Name}</b>, <b>{action.Unit.Name}</b> feels a slight twitching in the muscles of {GPPHis(action.Unit)} pouch's entrance. Not wanting to kill <b>{action.Target.Name}</b>, <b>{action.Unit.Name}</b> quickly reaches into {GPPHis(action.Unit)} pouch and pulls out <b>{action.Target.Name}</b>. \"You got lucky,\" <b>{action.Unit.Name}</b> says to <b>{action.Target.Name}</b>. \"A moment later and my pouch would have sealed, trapping you inside.\"{GetRandomStringFrom("", " Strangely, that doesn't sound that bad to <b>{action.Target.Name}</b>.")}");
                possibleLines.Add($"<b>{action.Unit.Name}</b> pushes upwards on the bottom of {GPPHis(action.Unit)} pouch, forcing <b>{action.Target.Name}</b> out and onto the ground.");
                possibleLines.Add($"After nearly falling over, <b>{action.Unit.Name}</b> unceremoniously dumps <b>{action.Target.Name}</b> out of {GPPHis(action.Unit)} pouch.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> feels a slight twitching in the muscles of {GPPHis(action.Unit)} pouch's entrance, and quickly pulls <b>{action.Target.Name}</b> out before {GPPHis(action.Unit)} pouch sealed for good.");
            }
            else
            {
                if (action.preyLocation == PreyLocation.breasts)
                {

                    possibleLines.Add($"<b>{action.Unit.Name}</b> squeezes {GPPHis(action.Unit)} {GetRandomStringFrom("squirming", "wriggling")} boobs, pushing out large amounts of milk, and one very wet <b>{action.Target.Name}</b>.");
                    possibleLines.Add($"After {GPPHis(action.Unit)} full breasts nearly tips {GPPHim(action.Unit)} over, <b>{action.Unit.Name}</b> decides to release <b>{action.Target.Name}</b>, in the process regaining {GPPHis(action.Unit)} balance.");
                    possibleLines.Add($"<b>{action.Target.Name}</b> was released from <b>{action.Unit.Name}</b>'s breasts.");
                }
                else
                {
                    possibleLines.Add($"<b>{action.Unit.Name}</b> squeezes {GPPHis(action.Unit)} {GetRandomStringFrom("squirming", "wriggling")} {(action.preyLocation == PreyLocation.leftBreast ? "left" : "right")} boob, pushing out large amounts of milk, and one very wet <b>{action.Target.Name}</b>.");
                    possibleLines.Add($"After giving {GPPHimself(action.Unit)} a hearty slap on {GPPHis(action.Unit)} {(action.preyLocation == PreyLocation.leftBreast ? "left" : "right")} {GetRandomStringFrom("boob", "breast", "titty")}, <b>{action.Unit.Name}</b> sees <b>{action.Target.Name}</b>'s head poke out of {GPPHis(action.Unit)} nipple! After a moment, <b>{action.Unit.Name}</b> sighs and pulls <b>{action.Target.Name}</b> all the way out.");
                    possibleLines.Add($"After {GPPHis(action.Unit)} full breast nearly tips {GPPHim(action.Unit)} over, <b>{action.Unit.Name}</b> decides to release <b>{action.Target.Name}</b>, in the process regaining {GPPHis(action.Unit)} balance.");
                    possibleLines.Add($"<b>{action.Target.Name}</b> was released from <b>{action.Unit.Name}</b>'s {(action.preyLocation == PreyLocation.leftBreast ? "left" : "right")} breast.");
                }
            }
        }
        return GetRandomStringFrom(possibleLines.ToArray());
    }

    private string GenerateBellyRubMessage(EventLog action)
    {
        return GetStoredMessage(StoredLogTexts.MessageTypes.BellyRubMessages, action);
    }

    private string GenerateForcefeedingMessage(EventLog action)
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b> forced {GPPHis(action.Unit)} way into <b>{action.Target.Name}</b>'s {PreyLocStrings.ToSyn(action.preyLocation)}.";
        List<string> possibleLines = new List<string>();
        if (action.Unit.Race == Race.Kangaroos && action.preyLocation == PreyLocation.breasts)
        {
            possibleLines.Add($"<b>{action.Unit.Name}</b> approaches <b>{action.Target.Name}</b> and abruptly yanks open {GPPHis(action.Unit)} [WFKP]. Before the {GetRaceDescSingl(action.Target)} can comment on how frankly rude this is, <b>{action.Unit.Name}</b> has already forced {GPPHis(action.Unit)} way inside.");
            possibleLines.Add($"While <b>{action.Target.Name}</b> isn't paying attention, <b>{action.Unit.Name}</b> jumps into the {GetRaceDescSingl(action.Target)}'s pouch. Somehow, <b>{action.Target.Name}</b> doesn't notice this, and carries on as {GPPHeWas(action.Target)}.");
            possibleLines.Add($"<b>{action.Unit.Name}</b> has already decided that {GPPHeIsAbbr(action.Unit)} {GetRaceDescSingl(action.Target)} food. The only questions left are who and how? For the who, <b>{action.Unit.Name}</b> selects <b>{action.Target.Name}</b>. After some thought, the {GetRaceDescSingl(action.Unit)} decides to see what the inside of a [WFKP] is like. Before <b>{action.Target.Name}</b> can attempt to stop {GPPHim(action.Unit)}, the {GetRaceDescSingl(action.Unit)} is already fully within {GPPHis(action.Target)} [WFKP].");
            possibleLines.Add($"After forcing {GPPHimself(action.Unit)} into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} [WFKP], <b>{action.Unit.Name}</b> is a touch surprised by the way the [WFKP]'s entrance seals above {GPPHim(action.Unit)}. Unlike most force-feeders, <b>{action.Unit.Name}</b> doesn't necessarily want to be digested. So now, both <b>{action.Target.Name}</b> and <b>{action.Unit.Name}</b> get to be unhappy!");
            possibleLines.Add($"<b>{action.Unit.Name}</b> smacks <b>{action.Target.Name}</b> across the face. While {GPPHeIs(action.Target)} dazed, <b>{action.Unit.Name}</b> climbs into {GPPHis(action.Target)} [WFKP].{GetRandomStringFrom("", $" When <b>{action.Target.Name}</b> regains {GPPHis(action.Target)} senses, {GPPHe(action.Target)} say{SIfSingular(action.Target)}, \"You could have just asked.\"")}");
            return GetRandomStringFrom(possibleLines.ToArray());
        }
        possibleLines.Add($"<b>{action.Unit.Name}</b> crams {GPPHimself(action.Unit)} into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}{GetRandomStringFrom(".", "!")}");
        possibleLines.Add($"It all happened so fast. One moment, <b>{action.Target.Name}</b> was bracing {GPPHimself(action.Target)} against a charging <b>{action.Unit.Name}</b>. The next, {GPPHis(action.Target)} {PreyLocStrings.ToSyn(action.preyLocation)} was bulging out with <b>{action.Target.Name}</b> stored inside.");
        possibleLines.Add($"After <b>{action.Unit.Name}</b> finished forcing {GPPHimself(action.Unit)} into <b>{action.Target.Name}</b>'s {PreyLocStrings.ToSyn(action.preyLocation)}, <b>{action.Target.Name}</b> says simply; \"Rude.If you wanted in, you could've let me choose were to put you, or at least let me get some pleasure out of your journey in.\"");
        possibleLines.Add($"One moment, <b>{action.Unit.Name}</b> was outside of <b>{action.Target.Name}</b>'s body. The next, {GPPHeWas(action.Unit)} inside. Notably, <b>{action.Target.Name}</b> was given no choice in this decision.");
        possibleLines.Add($"<b>{action.Unit.Name}</b> walks up to <b>{action.Target.Name}</b> and unceremoniously forces {GPPHimself(action.Unit)} into the {GetRaceDescSingl(action.Target)}'s {PreyLocStrings.ToSyn(action.preyLocation)}. <b>{action.Target.Name}</b>, for {GPPHis(action.Target)} part stares at {GPPHis(action.Target)} {GetRandomStringFrom("bloated", "engorged", "enlarged", "bulging")} {PreyLocStrings.ToSyn(action.preyLocation)} with utter confusion.");
        possibleLines.Add($"<b>{action.Unit.Name}</b> tackles <b>{action.Target.Name}</b>, and uses the moment of confusion to force {GPPHimself(action.Unit)} into the {GetRaceDescSingl(action.Target)}'s {PreyLocStrings.ToSyn(action.preyLocation)}.");
        switch (action.preyLocation)
        {
            case PreyLocation.stomach:
                possibleLines.Add($"At {GPPHis(action.Unit)} first glimpse of the {(ActorHumanoid(action.Target) ? "warrior's" : "beast's")} maw, <b>{action.Unit.Name}</b> dives right down {GPPHis(action.Target)} gullet. One swallow reflex later, <b>{ApostrophizeWithOrWithoutS(action.Target.Name)}</b> belly has been filled.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> walks up to <b>{action.Target.Name}</b> and pries {GPPHis(action.Target)} {PreyLocStrings.ToMouthSyn()} open before manually crawling down into {GetRandomStringFrom(ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>"), $"the {GetRaceDescSingl(action.Target)}'s")} {PreyLocStrings.ToSyn(action.preyLocation)}, much to {GetRandomStringFrom(GPPHis(action.Target), ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>"), $"the {GetRaceDescSingl(action.Target)}'s")} confusion.");
                possibleLines.Add($"Noticing that {GPPHis(action.Target)} {PreyLocStrings.ToMouthSyn()} is slightly ajar, <b>{action.Unit.Name}</b> makes a running leap into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToMouthSyn()}, sliding quickly down into the {GetRaceDescSingl(action.Target)}'s {PreyLocStrings.ToSyn(action.preyLocation)}{GetRandomStringFrom(".", "!")}");
                possibleLines.Add($"<b>{action.Unit.Name}</b> spots <b>{action.Target.Name}</b> and decided that {GPPHeIsAbbr(action.Unit)} got to go inside. Without so much as an \"excuse me\", the {GetRaceDescSingl(action.Unit)} unapologetically forces {GPPHimself(action.Unit)} down the {GetRaceDescSingl(action.Target)}\'s {PreyLocStrings.ToMouthSyn()}.");
                possibleLines.Add($"As <b>{action.Unit.Name}</b> walks over to <b>{action.Target.Name}</b>, {GPPHe(action.Unit)} pick{SIfSingular(action.Unit)} up a stick. Once at the {GetRaceDescSingl(action.Target)}, {GPPHe(action.Unit)} use{SIfSingular(action.Unit)} the stick to force open {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToMouthSyn()}. By the time that {GPPHe(action.Target)} can spit the stick out, the <b>{action.Target.Name}</b> is already in {GPPHis(action.Target)} {PreyLocStrings.ToSyn(action.preyLocation)}.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> abruptly sticks {GPPHis(action.Unit)} head in {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} mouth, looking around the inside for a few moments, before forcing {GPPHis(action.Unit)} way down into {GetRandomStringFrom(ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>"), $"the {GetRaceDescSingl(action.Target)}'s")} {PreyLocStrings.ToSyn(action.preyLocation)}.");
                break;
            case PreyLocation.anal:
                possibleLines.Add($"<b>{action.Unit.Name}</b> starts by shoving one {(ActorHumanoid(action.Unit) ? "arm" : "forelimb")} up <b>{ApostrophizeWithOrWithoutS(action.Target.Name)}</b> ass, then another. Inch by inch {GPPHe(action.Unit)} vigorously squeez{EsIfSingular(action.Unit)} {GPPHimself(action.Unit)} into the anal depths.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> jumps face-first into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}, yanking {GPPHimself(action.Unit)} up and inside in a matter of moments.");
                possibleLines.Add($"When {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} back is turned, <b>{action.Unit.Name}</b> takes {GPPHis(action.Unit)} chance, jamming {GPPHis(action.Unit)} whole body up {GetRandomStringFrom(ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>"), $"the {GetRaceDescSingl(action.Target)}'s")} {PreyLocStrings.ToSyn(action.preyLocation)}.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> spots <b>{action.Target.Name}</b> and decided that {GPPHeIsAbbr(action.Unit)} got to go inside. Without so much as an \"excuse me,\" the {GetRaceDescSingl(action.Unit)} unapologetically forces {GPPHimself(action.Unit)} up the {GetRaceDescSingl(action.Target)}'s {PreyLocStrings.ToSyn(action.preyLocation)}.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> slips behind <b>{action.Target.Name}</b> and punches {GPPHim(action.Target)} right in the {PreyLocStrings.ToSyn(action.preyLocation)}! Where most would assume this is an attack on the {GetRaceDescSingl(action.Target)}, {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} next action, forcing {GPPHimself(action.Unit)} all the way inside {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)} proves the action had other motivations.");
                possibleLines.Add($"Without giving the {GetRaceDescSingl(action.Target)} any time to argue or protest, <b>{action.Unit.Name}</b> crawls up {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}.");
                break;
            case PreyLocation.womb:
                possibleLines.Add($"<b>{action.Unit.Name}</b> pries apart <b>{ApostrophizeWithOrWithoutS(action.Target.Name)}</b> vulva using {GPPHis(action.Unit)} face, grabbing onto any body part {GPPHe(action.Unit)} can find to slip {GPPHimself(action.Unit)} all the way in, aided by the {ApostrophizeWithOrWithoutS(GetRaceDescSingl(action.Target))} contractions of sudden arousal.");
                possibleLines.Add($"After being knocked to the ground by the {GetRaceDescSingl(action.Unit)}, <b>{action.Target.Name}</b> finds {GPPHis(action.Unit)} {PreyLocStrings.ToSyn(action.preyLocation)} being aggressively licked by <b>{action.Target.Name}</b>. This carries on for a few moments, before <b>{action.Target.Name}</b> suddenly and rapidly shoves {GPPHimself(action.Unit)} into {GetRandomStringFrom(ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>"), $"the {GetRaceDescSingl(action.Target)}'s")} womb.");
                //possibleLines.Add($"<b>{action.Unit.Name}</b> runs over to <b>{action.Target.Name}</b> before getting on the ground and {kneeling {between the {GetRaceDescSingl(action.Target)}'s legs<used for humanoids>/under {his/her/their} {PreyLocStrings.ToSyn(action.preyLocation)}<used for legless humanoids(Lamia, Vipers, Slimes, etc.)>}/ crawling below {his/her/their} {PreyLocStrings.ToSyn(action.preyLocation)}<used for quadrapeds and similar>}. Before <b>{action.Target.Name}</b> can question this, <b>{action.Unit.Name}</b> bolts upright, forcing {GPPHimself(action.Unit)} up into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {{PreyLocStrings.ToSyn(action.preyLocation)}/{PreyLocStrings.ToSyn(action.preyLocation)}}{!/.}");
                possibleLines.Add($"<b>{action.Unit.Name}</b> spots <b>{action.Target.Name}</b> and decided that {GPPHeIsAbbr(action.Unit)} got to go inside. Running over, <b>{action.Unit.Name}</b> rapidly pushes {GPPHis(action.Unit)} way into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> slips behind <b>{action.Target.Name}</b> and kicks {GPPHim(action.Target)} right in the {PreyLocStrings.ToSyn(action.preyLocation)}! Where most would assume this is an attack on the {GetRaceDescSingl(action.Target)}, {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} next action, knocking <b>{action.Target.Name}</b> over and forcing {GPPHimself(action.Unit)} further into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)} proves the action had other motivations.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> sticks a finger up {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}. Then a hand. Then a whole arm. Then two hands. Then <b>{action.Unit.Name}</b> pushes off the ground to force {GPPHimself(action.Unit)} all the way up into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}.");
                break;
            case PreyLocation.balls:
                possibleLines.Add($"<b>{action.Unit.Name}</b> sucks <b>{ApostrophizeWithOrWithoutS(action.Target.Name)}</b> tip. As soon as {GPPHe(action.Unit)} start{SIfSingular(action.Unit)} sticking {GPPHis(action.Unit)} tongue inside, however, it's more like the {ApostrophizeWithOrWithoutS(InfoPanel.RaceSingular(action.Target))} throbbing member is doing the sucking, allowing <b>{action.Unit.Name}</b> to wiggle all the way into {GPPHis(action.Target)} sack.");
                possibleLines.Add($"One moment, <b>{action.Unit.Name}</b> was just walking up to <b>{action.Target.Name}</b>, then, not even two seconds later, <b>{action.Unit.Name}</b> had already shoved half of {GPPHis(action.Unit)} body down the shocked {GetRaceDescSingl(action.Target)}'s {PreyLocStrings.ToSyn(action.preyLocation)}! Only three or so seconds after that, <b>{action.Unit.Name}</b> was fully in {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> strokes {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}, getting it nice and {GetRandomStringFrom("hard", "erect")}, before shoving {GPPHimself(action.Unit)} down into the unprepared {GetRaceDescSingl(action.Target)}'s {PreyLocStrings.ToSyn(action.preyLocation)}{GetRandomStringFrom(".", "!")}");
                possibleLines.Add($"<b>{action.Unit.Name}</b> has spotted {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}. In that instant, <b>{action.Unit.Name}</b> knows exactly what {GPPHe(action.Unit)} must do. Without any warning, <b>{action.Unit.Name}</b> is shoving {GPPHimself(action.Unit)} down {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}, the lenth of the shaft bulging with the {GetRaceDescSingl(action.Unit)}'s every movement.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> gives <b>{action.Target.Name}</b> a blowjob. As <b>{action.Target.Name}</b> nears ejaculation, <b>{action.Unit.Name}</b> pulls back and then dives head first down {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> sticks {GPPHis(action.Unit)} hand down {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}. Then the {GetRaceDescSingl(action.Unit)} grabs some of the loose skin around the {PreyLocStrings.ToSyn(action.preyLocation)} from the inside, and uses it as a handhold to rapidly, and not particularaly pleasently, pull {GPPHimself(action.Unit)} into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}."); //Ideally, there would be some check to insure that the two uses of {PreyLocStrings.ToSyn(action.preyLocation)} in this line always pull different words, but if that isn't possible, it should still be fine.
                break;
            case PreyLocation.breasts:
                possibleLines.Add($"In just a few deft movements, <b>{action.Unit.Name}</b> crams {GPPHimself(action.Unit)} into <b>{ApostrophizeWithOrWithoutS(action.Target.Name)}</b> tits.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> charges into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}, briefly motorboating {GPPHim(action.Target)} before pushing extra hard and vanishing into the space between the {GetRaceDescSingl(action.Target)}'s {PreyLocStrings.ToSyn(action.preyLocation)}.");
                possibleLines.Add($"Using {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)} as handholds, <b>{action.Unit.Name}</b> pulls {GPPHimself(action.Unit)} into the {GetRaceDescSingl(action.Target)}'s cleavage.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> approaches <b>{action.Target.Name}</b> and abruptly pulls apart {GPPHis(action.Unit)} {PreyLocStrings.ToSyn(action.preyLocation)}, before stuffing {GPPHimself(action.Unit)} into the gap between.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> grabs {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)} and shoves {GPPHimself(action.Unit)} between {GPPHis(action.Unit)} {PreyLocStrings.ToSyn(action.preyLocation)}! Much to {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} surprise, {GPPHe(action.Unit)} sink{SIfSingular(action.Unit)} into the soft flesh at the bottom, becoming living fat on {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> knocks <b>{action.Target.Name}</b> down onto {GPPHis(action.Unit)} back, before {GetRandomStringFrom("jumping feet", "diving head")}-first into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}, disappearing with one quick *shlump* noise.");
                break;
            case PreyLocation.leftBreast:
            case PreyLocation.rightBreast:
                possibleLines.Add($"In just a few deft movements, <b>{action.Unit.Name}</b> crams {GPPHimself(action.Unit)} into <b>{ApostrophizeWithOrWithoutS(action.Target.Name)}</b> {(action.preyLocation == PreyLocation.leftBreast ? "left" : "right")} {GetRandomStringFrom("tit", "boob", "breast")}.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> grabs {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {(action.preyLocation == PreyLocation.leftBreast ? "left" : "right")} {PreyLocStrings.ToSyn(action.preyLocation)} and shoves it into {GPPHis(action.Unit)} face. Then, with a wet *shlorp* sound, {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} head disappears into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}, followed shortly by the rest of {GPPHis(action.Unit)} body.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> latches onto {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {(action.preyLocation == PreyLocation.leftBreast ? "left" : "right")} {PreyLocStrings.ToSyn(action.preyLocation)}, suckling for a moment or two, before forcing {GPPHimself(action.Unit)} through the nipple, and vanishing into the boob beyond.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> spots <b>{action.Target.Name}</b> and decided that {GPPHeIsAbbr(action.Unit)} got to go inside. With a motion resembling a headbutt, <b>{action.Unit.Name}</b> slips {GPPHis(action.Unit)} head into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {(action.preyLocation == PreyLocation.leftBreast ? "left" : "right")} {PreyLocStrings.ToSyn(action.preyLocation)}, followed shortly by the rest of {GPPHis(action.Unit)} body.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> sucks on {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {(action.preyLocation == PreyLocation.leftBreast ? "left" : "right")} {PreyLocStrings.ToSyn(action.preyLocation)}. As <b>{action.Target.Name}</b> relaxes to let it happen, <b>{action.Unit.Name}</b> pulls back and then dives head first into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {(action.preyLocation == PreyLocation.leftBreast ? "left" : "right")} {PreyLocStrings.ToSyn(action.preyLocation)}.");
                //possibleLines.Add($"<b>{action.Unit.Name}</b> knocks <b>{action.Target.Name}</b> down onto {GPPHis(action.Unit)} back, before kicking the center of {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {(action.preyLocation == PreyLocation.leftBreast ? "left" : "right")} {PreyLocStrings.ToSyn(action.preyLocation)}, getting {GPPHis(action.Unit)} foot inside. Then {GPPHe(action.Unit)} slowly feed{SIfSingular(action.Unit)} the rest of {GPPHimself(action.Unit)} into the {GetRaceDescSingl(action.Target)}'s {PreyLocStrings.ToSyn(action.preyLocation)}.<Line should be off-limits to feetless units(Lamia, Vipers, etc.) and non-bipedal units (Feral Lizard, Schiwardez, etc.).>");
                break;
            default:
                return $"Where is";
        }
        return GetRandomStringFrom(possibleLines.ToArray());
    }

    private string GenerateForcefeedFailMessage(EventLog action)
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b> forced {GPPHis(action.Unit)} way into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}.";
        List<string> possibleLines = new List<string>();
        possibleLines.Add($"As <b>{action.Unit.Name}</b> attempts to pry open {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} mouth, {GPPHeIs(action.Unit)} rather surprised to find that {GetRandomStringFrom(ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>"), GetRaceDescSingl(action.Target))} mouth seems incapable of stretching far enough.");
        possibleLines.Add($"<b>{action.Unit.Name}</b> attempts to stick {GPPHis(action.Unit)} head up {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} [WFA], only to find that it refuses to open anywhere near large enough for that.{GetRandomStringFrom("", $"<b>{action.Target.Name}</b> kindly asks the {GetRaceDescSingl(action.Unit)} to stop.")}");
        possibleLines.Add($"{ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} attempt to force-feed {GPPHimself(action.Unit)} into <b>{action.Target.Name}</b> has been thwarted by the refusal of any holes on the {GetRaceDescSingl(action.Target)}'s body to open anywhere near wide enough.");
        if (action.Target.HasDick)
            possibleLines.Add($"<b>{action.Unit.Name}</b> strokes {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToCockSyn()}, getting it nice and {GetRandomStringFrom("hard", "stiff", "erect")}, before attempting to pry open the tip to get inside, only to find that the tip barely opens wide enough to fit a blade of grass, let alone a whole {GetRaceDescSingl(action.Unit)}.");
        if (action.Target.HasVagina)
            possibleLines.Add($"<b>{action.Unit.Name}</b> sticks {GPPHis(action.Unit)} hand up {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(PreyLocation.womb)}, getting it nice and wet, before attempting to push further in, only to find that {GPPHeIsAbbr(action.Unit)} already about as far in as {GPPHe(action.Unit)} can go{GetRandomStringFrom(".", $", and that {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {PreyLocStrings.ToSyn(PreyLocation.womb)} isn't budging any further.")}");
        return GetRandomStringFrom(possibleLines.ToArray());
    }
    private string GenerateTraitConvertMessage(EventLog action)
    {
        //State.World?.MainEmpires != null && (State.World.GetEmpireOfSide(action.Unit.Side))
        if (SimpleText)
            return $"{action.Prey.Name} converted from one side to another thanks to {action.Unit.Name}'s digestion conversion trait.";
        List<string> possibleLines = new List<string>();
        possibleLines.Add($"With {ApostrophizeWithOrWithoutS($"<b>{action.Prey.Name}</b>")} body partially dissolved, {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} body takes this opportunity to rewrite {ApostrophizeWithOrWithoutS($"<b>{action.Prey.Name}</b>")} worldview before putting {GPPHim(action.Prey)} back together and letting them out, now to fight for {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} side.");
        possibleLines.Add($"Right before death within <b>{action.Unit.Name}</b>, <b>{action.Prey.Name}</b> is released on the condition of joining {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} side of the battle.");
        possibleLines.Add($"<b>{action.Prey.Name}</b> emerges from <b>{action.Unit.Name}</b> a changed {GetRaceDescSingl(action.Prey)}, ready to fight for {(State.GameManager.PureTactical ? $"the {GetRaceDescSingl(action.Unit)}" : $"the {State.World.GetEmpireOfSide(action.Unit.Side)}")}.");
        if (!State.GameManager.PureTactical)
        {
            possibleLines.Add($"<b>{action.Prey.Name}</b> was converted to the side of the {State.World.GetEmpireOfSide(action.Unit.Side)}, thanks to a large amount of \"persuasion\" by <b>{action.Unit.Name}</b>.");
            possibleLines.Add($"Within <b>{action.Unit.Name}</b>, <b>{action.Prey.Name}</b> has been brainwashed, and {(State.World.GetEmpireOfSide(action.Unit.Side).Race >= Race.Selicia ? $"is now ready to do the {State.World.GetEmpireOfSide(action.Unit.Side).Race}' bidding" : $"now fully believes in the {InfoPanel.RaceSingular(State.World.GetEmpireOfSide(action.Unit.Side))} cause.")}");
        }
            
        return GetRandomStringFrom(possibleLines.ToArray());
    }
    private string GenerateTraitRebirthMessage(EventLog action)
    {
        if (SimpleText)
            return $"{action.Prey.Name} converted from one side to another and changed race thanks to {action.Unit.Name}'s converting digestion rebirth trait.";
        if (action.Unit.Race == Race.Slimes)
            return $"With a squelch, <b>{action.Unit.Name}</b> performs mitosis{(action.oldRace == action.Prey.Race ? "." : $", <b>{action.Prey.Name}</b> becoming a seperate slime.")}";
        List<string> possibleLines = new List<string>();

        possibleLines.Add($"<b>{action.Prey.Name}</b> is expelled from {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}, {(action.oldRace == action.Prey.Race ? "with a changed outlook on life" : "changed in mind and body")}.");
        possibleLines.Add($"Finished, <b>{action.Prey.Name}</b> is released from {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}, {(action.oldRace == action.Prey.Race ? $"now ready to fight alongside <b>{action.Unit.Name}</b> rather than against {GPPHim(action.Unit)}." : "a changed person. Quite literally.")}");
        if (action.oldRace == action.Prey.Race)
            possibleLines.Add($"Within <b>{action.Unit.Name}</b>, <b>{action.Prey.Name}</b> has been transformed into {RaceArticleSingular(action.Prey.Race)}. With the process complete, <b>{action.Prey.Name}</b> is freed into the world once more.");

        possibleLines.Add($"With {ApostrophizeWithOrWithoutS($"<b>{action.Prey.Name}</b>")} body partially dissolved, {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} body takes this opportunity to rewrite {ApostrophizeWithOrWithoutS($"<b>{action.Prey.Name}</b>")} worldview and genetics, before allowing the brand new {GetRaceDescSingl(action.Prey)} back out.");
        possibleLines.Add($"<b>{action.Prey.Name}</b> is released from inside {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} body, though now as {RaceArticleSingular(action.Prey.Race)}.");
        possibleLines.Add($"As <b>{action.Prey.Name}</b> is released from <b>{action.Unit.Name}</b>, {GPPHe(action.Prey)} look{SIfSingular(action.Prey)} at {GPPHimself(action.Prey)}, and note{SIfSingular(action.Prey)} that {GPPHeIs(action.Prey)} now {RaceArticleSingular(action.Prey.Race)}.");
        possibleLines.Add($"{Capitalize(RaceArticleSingular(action.Prey.Race))} is expelled from {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} body, the brand new form of <b>{action.Prey.Name}</b>.");
        possibleLines.Add($"<b>{action.Prey.Name}</b> emerges from <b>{action.Unit.Name}</b> changed into {RaceArticleSingular(action.Prey.Race)}.");

        switch (action.preyLocation)
        {
            case PreyLocation.womb:
                possibleLines.Add($"Feeling that <b>{action.Prey.Name}</b> has completed {GPPHis(action.Prey)} stay in {GPPHis(action.Unit)} {PreyLocStrings.ToSyn(action.preyLocation)}, <b>{action.Unit.Name}</b> gives birth to <b>{action.Prey.Name}</b>, who looks to {GPPHis(action.Prey)} new mother for directions.");
                possibleLines.Add($"<b>{action.Unit.Name}</b> gets down and gives birth to <b>{action.Prey.Name}</b>{GetRandomStringFrom(".", ", who tries to go back in after being nearly blinded by the light.")}"); // this version is meant to be used if A;<b>{action.Prey.Name}</b> has eyes and B;it isn't night.
                if(action.oldRace != action.Prey.Race)
                {
                    possibleLines.Add($"As <b>{action.Prey.Name}</b> wakes up within {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)}, {GPPHe(action.Prey)} start{SIfSingular(action.Prey)} to struggle. Taking this as a sign, <b>{action.Unit.Name}</b> gets down and rebirths <b>{action.Prey.Name}</b> into this world.");
                    possibleLines.Add($"{ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} womb, having completed the task of taking <b>{action.Prey.Name}</b> apart and putting {GPPHim(action.Prey)} back together as something new, signals {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} brain that it's time to see what <b>{action.Prey.Name}</b> has become. With only a few skilled pushes, <b>{action.Prey.Name}</b> is reborn, now as {RaceArticleSingular(action.Prey.Race)}.");
                    possibleLines.Add($"It is done. {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} womb has done its work, and <b>{action.Prey.Name}</b> is now {RaceArticleSingular(action.Prey.Race)}. Getting down, <b>{action.Unit.Name}</b> births <b>{action.Prey.Name}</b> back into this world.");
                }
                break;
            case PreyLocation.stomach:
            case PreyLocation.stomach2:
                possibleLines.Add($"\"Okay, I think you're done,\" <b>{action.Unit.Name}</b> says before coughing up <b>{action.Prey.Name}</b>{(action.oldRace == action.Prey.Race ? "." : $", who appears to have become {RaceArticleSingular(action.Prey.Race)}.")}");
                possibleLines.Add($"<b>{action.Unit.Name}</b> barfs up <b>{action.Prey.Name}</b>, {(action.oldRace == action.Prey.Race ? "who seems fully intact, if a little tired." : $"who emerges as a brand new {GetRaceDescSingl(action.Prey)}.")}"); 
                possibleLines.Add($"With a grumble from {GPPHis(action.Unit)} guts, <b>{action.Unit.Name}</b> squats and pushes out a living {GetRaceDescSingl(action.Prey)}. {(action.oldRace == action.Prey.Race ? $"<b>{action.Unit.Name}</b> looks behind {GPPHim(action.Unit)} and exclaims, \"Woah, you're alive?\"" : $"<b>{action.Unit.Name}</b> looks to <b>{action.Prey.Name}</b> and tells {GPPHim(action.Prey)}, \"Be glad my guts turned you into {RaceArticleSingular(action.Prey.Race)}, and not into {GetRaceDescSingl(action.Unit)} [WFF].\"")}");
                possibleLines.Add($"After pushing <b>{action.Prey.Name}</b> out {GPPHis(action.Unit)} {PreyLocStrings.ToSyn(PreyLocation.anal)}, <b>{action.Unit.Name}</b> turns and tells <b>{action.Prey.Name}</b>, {(action.oldRace == action.Prey.Race ? "\"You smell bad. You... should probably fix that.\"" : $"\"Congrats on becoming {RaceArticleSingular(action.Prey.Race)}! Sorry your first moments as one had to smell this bad, though...\"")}"); 
                break;
            case PreyLocation.balls:
                possibleLines.Add($"With one last grunt, <b>{action.Prey.Name}</b> erupts, alive, from {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} {PreyLocStrings.ToCockSyn()}.{(action.oldRace == action.Prey.Race ? "" : $" <b>{action.Prey.Name}</b> hasn't noticed yet, but {GPPHis(action.Prey)} stay in {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} {PreyLocStrings.ToBallSynSing()} appears to have overwrote {GPPHis(action.Prey)} {GetRandomStringFrom("DNA", "genetic code")}, making {GPPHim(action.Prey)} into {RaceArticleSingular(action.Prey.Race)}.")}");
                if (action.oldRace == action.Prey.Race)
                {
                    possibleLines.Add($"During the time spent in {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} {PreyLocStrings.ToSyn(PreyLocation.balls)}, the {GetRaceDescSingl(action.Unit)}'s {PreyLocStrings.ToFluid(PreyLocation.balls)} had been attacking {ApostrophizeWithOrWithoutS($"<b>{action.Prey.Name}</b>")} very {GetRandomStringFrom("DNA", "genetics", "genes")}, rewriting them into a brand new {GetRaceDescSingl(action.Prey)}.");
                    possibleLines.Add($"It is done. {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} sperm has done its work, and <b>{action.Prey.Name}</b> is now {RaceArticleSingular(action.Prey.Race)}. With a tight grip on {GPPHis(action.Unit)} {PreyLocStrings.ToCockSyn()}, <b>{action.Unit.Name}</b> {GetRandomStringFrom("faps", "masturbates")}, going faster and faster until <b>{action.Prey.Name}</b> is launched back into the world, alongside a healthy dose of {PreyLocStrings.ToFluid(PreyLocation.balls)}.");
                }
                break;
            case PreyLocation.leftBreast:
            case PreyLocation.rightBreast:
                possibleLines.Add($"<b>{action.Unit.Name}</b> grips {GPPHis(action.Unit)} {(action.preyLocation == PreyLocation.leftBreast ? "left" : "right")} {PreyLocStrings.ToBreastSynSing()} and squeezes, forcing out a milk soaked {(action.oldRace == action.Prey.Race ? $"<b>{action.Prey.Name}</b>" : GetRaceDescSingl(action.Prey))}.");
                if (action.oldRace == action.Prey.Race)
                {
                    possibleLines.Add($"Sensing that {GPPHis(action.Unit)} {PreyLocStrings.ToBreastSynSing()} has done its job, <b>{action.Unit.Name}</b> forces a milk-soaked {GetRaceDescSingl(action.Prey)} from {GPPHis(action.Unit)} {(action.preyLocation == PreyLocation.leftBreast ? "left" : "right")} nipple, the new form of <b>{action.Prey.Name}</b>.");
                    possibleLines.Add($"It is done. {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} {(action.preyLocation == PreyLocation.leftBreast ? "left" : "right")} breast has done its work, and <b>{action.Prey.Name}</b> is now {RaceArticleSingular(action.Prey.Race)}. <b>{action.Unit.Name}</b> sucks {GPPHis(action.Unit)} {PreyLocStrings.ToBreastSynSing()} until enough of <b>{action.Prey.Name}</b> has popped out for <b>{action.Unit.Name}</b> to grab {GPPHim(action.Prey)} and pull the {GetRaceDescSingl(action.Prey)} out.");
                }
                
                break;
            case PreyLocation.breasts:
                possibleLines.Add($"{ApostrophizeWithOrWithoutS($"<b>{action.Prey.Name}</b>")} essence is abruptly shot from {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} {PreyLocStrings.ToSyn(action.preyLocation)} as pure milk, before coagulating and reforming into {RaceArticleSingular(action.Prey.Race)}.");
                possibleLines.Add($"Feeling like showing mercy, <b>{action.Unit.Name}</b> decides to release <b>{action.Prey.Name}</b> from {GPPHis(action.Unit)} {GetRandomStringFrom($"{PreyLocStrings.ToBreastSynSing()} fat", PreyLocStrings.ToSyn(action.preyLocation))}. However, it would seem that {ApostrophizeWithOrWithoutS($"<b>{action.Prey.Name}</b>")} stay in the {GetRaceDescSingl(action.Unit)}'s {PreyLocStrings.ToBreastSynSing()} altered {GPPHis(action.Prey)} very essence, turning {GPPHim(action.Prey)} into {RaceArticleSingular(action.Prey.Race)}.");
                if (action.oldRace == action.Prey.Race)
                    possibleLines.Add($"It is done. {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} boobs have done their work, and <b>{action.Prey.Name}</b> is now {RaceArticleSingular(action.Prey.Race)}. For a moment, the flesh between {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} {PreyLocStrings.ToBreastSynSing()} appears to stretch and contort, until, all at once, <b>{action.Prey.Name}</b> and {GPPHis(action.Prey)} new form slip out into the world once more.");
                break;
            default:
                break;
        }
        return GetRandomStringFrom(possibleLines.ToArray());
    }
    private string GenerateBirthMessage(EventLog action)
    {
        if (SimpleText)
            return $"With a loud grunt, <b>{action.Unit.Name}</b> pushes <b>{action.Target.Name}</b> from {GPPHis(action.Unit)} womb, and breathes a sigh of relief.{action.Odds}";
        List<string> possibleLines = new List<string>();
        possibleLines.Add($"With {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} body partially dissolved, {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} body takes this opportunity to rewrite {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} worldview{(action.Extra == "rebirth" ? "and genetics": "")} before allowing the brand new {GetRaceDescSingl(action.Target)} back out.");
        possibleLines.Add($"<b>{action.Target.Name}</b> is released from inside {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} body{(action.Extra == "rebirth" ? $", though now as {RaceArticleSingular(action.Target.Race)}" : "")}.");
        if (action.Extra == "rebirth")
        {
            possibleLines.Add($"As <b>{action.Target.Name}</b> is released from <b>{action.Unit.Name}</b>, {GPPHe(action.Target)} look{SIfSingular(action.Target)} at {GPPHimself(action.Target)}, and note that {GPPHeIs(action.Target)} now {RaceArticleSingular(action.Target.Race)}.");
            possibleLines.Add($"{Capitalize(RaceArticleSingular(action.Target.Race))} is expelled from {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} body, the brand new form of <b>{action.Target.Name}</b>.");
            possibleLines.Add($"<b>{action.Target.Name}</b> emerges from <b>{action.Unit.Name}</b> changed into {RaceArticleSingular(action.Target.Race)}.");
        }
        return GetRandomStringFrom(possibleLines.ToArray());
    }
    private string GenerateCumGestationMessage(EventLog action)
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b> pumps what remains of <b>{action.Prey.Name}</b> into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} womb, providing nutrients to strengthen <b>{action.AdditionalUnit.Name}</b>.";
        List<string> possibleLines = new List<string>();
        possibleLines.Add($"<b>{action.Unit.Name}</b> pumps what remains of <b>{action.Prey.Name}</b> into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} womb, providing nutrients to strengthen <b>{action.AdditionalUnit.Name}</b>.");
        possibleLines.Add($"As <b>{action.Unit.Name}</b> and <b>{action.Target.Name}</b> {GetRandomStringFrom("have sex", "fuck")}, the liquid remains of <b>{action.Prey.Name}</b> shoot into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {GetRandomStringFrom("womb", "uterus")}, where what little remains of <b>{action.Prey.Name}</b> is simply absorbed into <b>{action.AdditionalUnit.Name}</b>.");
        possibleLines.Add($"The {PreyLocStrings.ToFluid(PreyLocation.balls)} that was once <b>{action.Prey.Name}</b> travels up {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} {PreyLocStrings.ToCockSyn()}, the former {GetRaceDescSingl(action.Prey)} gushing up into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {GetRandomStringFrom("womb", "uterus")} where {GPPHeIs(action.Prey)} promptly absorbed into <b>{action.AdditionalUnit.Name}</b>.");
        possibleLines.Add($"<b>{action.Target.Name}</b> rides {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} {PreyLocStrings.ToCockSyn()}, milking out a liqufied <b>{action.Prey.Name}</b>, who flows into the already occupied {GetRandomStringFrom("womb", "uterus")}, where {GPPHis(action.Prey)} remains fuse with <b>{action.AdditionalUnit.Name}</b>.");
        possibleLines.Add($"<b>{action.Target.Name}</b> pushes <b>{action.Unit.Name}</b> down to the ground and fucks {GPPHim(action.Unit)} until the liquid remains of <b>{action.Prey.Name}</b> flow up into {GPPHis(action.Target)} {GetRandomStringFrom("womb", "uterus")}, where <b>{action.Prey.Name}</b> is absorbed into <b>{action.AdditionalUnit.Name}</b>.");
        possibleLines.Add($"<b>{action.Unit.Name}</b> pumps <b>{action.Prey.Name}</b> through {GPPHis(action.Unit)} {PreyLocStrings.ToCockSyn()} and into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {GetRandomStringFrom("womb", "uterus")}, where {GPPHis(action.Prey)} remains are used by <b>{action.AdditionalUnit.Name}</b> to fuel {GPPHis(action.AdditionalUnit)} growth.");
        if (Config.CondomsForCV)
        {
            possibleLines.Add($"As <b>{action.Unit.Name}</b> and <b>{action.Target.Name}</b> have sex, an untimely squirm from <b>{action.AdditionalUnit.Name}</b> coincides with <b>{action.Unit.Name}</b> cumming hard, allowing the former {GetRaceDescSingl(action.Prey)} <b>{action.Prey.Name}</b> to rip through the condom, flowing into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {GetRandomStringFrom("womb", "uterus")} before being absorbed into the regrowing <b>{action.AdditionalUnit.Name}</b>.");
            possibleLines.Add($"As <b>{action.Unit.Name}</b> goes to have sex with <b>{action.Target.Name}</b>, {GPPHe(action.Unit)} pull{SIfSingular(action.Unit)} out a condom, before being told to put it away. Without that barrier of protection, the {PreyLocStrings.ToFluid(PreyLocation.balls)} that was once <b>{action.Prey.Name}</b> spills into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {GetRandomStringFrom("womb", "uterus")} unimpeded, where it is easily absorbed into <b>{action.AdditionalUnit.Name}</b>.");
            possibleLines.Add($"As {ApostrophizeWithOrWithoutS($"<b>{action.Unit.Name}</b>")} {PreyLocStrings.ToBallSynSing()} shrinks, {ApostrophizeWithOrWithoutS($"<b>{action.Prey.Name}</b>")} liquefied form fills the rubber barrier between {GetRandomStringFrom(ApostrophizeWithOrWithoutS(action.Unit.Name), $"the {ApostrophizeWithOrWithoutS(GetRaceDescSingl(action.Unit))}")} {PreyLocStrings.ToCockSyn()} and {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {GetRandomStringFrom("womb", "uterus")}. As <b>{action.Unit.Name}</b> pulls {GPPHis(action.Unit)} {PreyLocStrings.ToCockSyn()} back out, the condom is left behind, where it is pulled into {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {GetRandomStringFrom("womb", "uterus")}, and it is there that it pops, with <b>{action.Prey.Name}</b> becoming a shower of nutrients for <b>{action.AdditionalUnit.Name}</b>.");
            Unit randomAlly = GetRandomAlly(action.Unit, action.Target, action.Prey, action.AdditionalUnit);
            if (randomAlly != null)
            {
                possibleLines.Add($"<b>{action.Unit.Name}</b> slips on a condom and goes to have sex with <b>{action.Target.Name}</b>. After they're done, <b>{action.Unit.Name}</b> observes that former <b>{action.Prey.Name}</b> is leaking from 7 small holes in the condom. Knowing what happened, <b>{action.Unit.Name}</b> shouts \"{randomAlly.Name}! Not funny!\" Within {ApostrophizeWithOrWithoutS($"<b>{action.Target.Name}</b>")} {GetRandomStringFrom("womb", "uterus")}, the trickling streams of \"<b>{action.Prey.Name}</b>\" are absorbed by <b>{action.AdditionalUnit.Name}</b>.");
                possibleLines.Add($"Before having sex, <b>{action.Unit.Name}</b> looks for {GPPHis(action.Unit)} condom, but can't find it. \"I think I left it with {randomAlly.Name}. Want me to go get it?\" <b>{action.Target.Name}</b> responds \"Nah, I got a {GetRaceDescSingl(action.AdditionalUnit)} in my {GetRandomStringFrom("womb", "belly", "tummy")}, they'll absorb everything.\" True to {GPPHis(action.Target)} word, the remains of <b>{action.Prey.Name}</b> are absorbed by <b>{action.AdditionalUnit.Name}</b>.");
            }
        }
        return GetRandomStringFrom(possibleLines.ToArray());
    }

    private string GenerateUBSwallowMessage(EventLog action)
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b> unbirths <b>{action.Target.Name}</b>.";
        return GetStoredMessage(StoredLogTexts.MessageTypes.UnbirthMessages, action);
    }

    private string GenerateTVSwallowMessage(EventLog action)
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b> tail vores <b>{action.Target.Name}</b>.";
        return GetStoredMessage(StoredLogTexts.MessageTypes.TailVoreMessages, action);
    }

    private string GenerateAVSwallowMessage(EventLog action)
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b> anal vores <b>{action.Target.Name}</b>.";
        return GetStoredMessage(StoredLogTexts.MessageTypes.AnalVoreMessages, action);
    }

    private string GenerateCVSwallowMessage(EventLog action)
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b> cock vores <b>{action.Target.Name}</b>.";
        return GetStoredMessage(StoredLogTexts.MessageTypes.CockVoreMessages, action);
    }

    private string GenerateRandomDigestionMessage(EventLog action)
    {
        return GetStoredMessage(StoredLogTexts.MessageTypes.RandomDigestionMessages, action);

    }

    private string GenerateDigestionLowHealthMessage(EventLog action)
    {
        if (Config.Scat && (action.preyLocation == PreyLocation.stomach || action.preyLocation == PreyLocation.stomach2) && State.Rand.Next(5) == 0)
        {
            return GetRandomStringFrom(
                $"<b>{action.Target.Name}</b> is well on {GPPHis(action.Target)} way to becoming <b>{action.Unit.Name}</b>'s poop.",
                $"<b>{action.Target.Name}</b> is increasingly falling apart into a foul mess, waiting to be flushed into <b>{action.Unit.Name}</b>'s intestines.",
                $"<b>{action.Target.Name}</b> doesn’t have the fortitude left to resist {GPPHis(action.Target)} destiny as a {GetRaceDescSingl(action.Unit)}'s next bowel movement anymore.",
                $"<b>{action.Unit.Name}</b> can feel <b>{action.Target.Name}</b>'s struggles getting weaker, kindly reminding {GPPHim(action.Target)} that if {GPPHe(action.Target)} fail{SIfSingular(action.Target)} to escape {GPPHeIs(action.Target)} getting melted into turds.",
                $"<b>{action.Unit.Name}</b>’s {action.preyLocation.ToSyn()} rumbles ominously while telling <b>{action.Target.Name}</b> that {GPPHe(action.Unit)} will enjoy shitting {GPPHim(action.Target)} out later.");
        }
        if (Config.HardVoreDialog && Random.Range(0, 5) == 0)
        {
            string loc = action.preyLocation.ToSyn();
            string locs = (loc.EndsWith("s") ? "" : "s");
            GetRandomStringFrom($"<b>{action.Unit.Name}</b> hears {GPPHis(action.Unit)} {loc} gurgle{locs} intensely. {Capitalize(GPPHe(action.Unit))} feels <b>{action.Target.Name}</b> begin to slip under {GPPHis(action.Unit)} turbulent acids.",
                                $"<b>{action.Unit.Name}</b>'s {loc} glurt{locs} and blort{locs}, {GPPHis(action.Unit)} {GetPredDesc(action.Target)} prey starting to break down. <b>{action.Target.Name}</b> seems doomed.");
        }
        if (action.preyLocation == PreyLocation.breasts)
        {
            if (action.Unit.Race == Race.Kangaroos)
                return GetRandomStringFrom(
                    $"With so little air in <b>{action.Unit.Name}</b>'s pouch, <b>{action.Target.Name}</b>'s mind has gone a little fuzzy. {Capitalize(GPPHeIs(action.Target))} now talking to {GPPHimself(action.Target)}.",
                    $"With no breathable air left in <b>{action.Unit.Name}</b>'s pouch, <b>{action.Target.Name}</b>'s vision begins to grow dark and fuzzy around the edges. The end of <b>{action.Target.Name}</b> is nigh.",
                    $"The lack of air within <b>{action.Unit.Name}</b>'s pouch has taken its toll on <b>{action.Target.Name}</b>, whose struggles have begun to slow.",
                    $"As the O2 levels in <b>{action.Unit.Name}</b>'s pouch drop to critically low levels, <b>{action.Target.Name}</b> begins to hallucinate. Rather than continue to struggle, <b>{action.Target.Name}</b> decides that a better use of their little remaining oxygen is in having a conversation with these hallucinations.",
                    $"<b>{action.Target.Name}</b>'s breathing has now replaced most of the O2 in <b>{action.Unit.Name}</b>'s pouch with CO2. With the air mixture so inhospitable, <b>{action.Target.Name}</b> falls into a coughing fit. As <b>{action.Unit.Name}</b>'s fellow soldiers look at {GPPHim(action.Unit)}, <b>{action.Unit.Name}</b> blushes, and smacks {GPPHis(action.Unit)} pouch a few times, hoping to {GetRandomStringFrom("rob", "drain")} <b>{action.Target.Name}</b> of the last of {GPPHis(action.Target)} strength."
                );
            else
                return GetRandomStringFrom(
                    $"As <b>{action.Target.Name}</b> continues to struggle, {GPPHe(action.Target)} find{SIfSingular(action.Target)} {GPPHimself(action.Target)} less and less able to remember anything about the world beyond <b>{ApostrophizeWithOrWithoutS(action.Unit.Name)}</b> {GetRandomStringFrom("breasts", "boobs", "tits")}.",
                    $"With each exertion of strength, <b>{action.Target.Name}</b> forgets a little more. Right now, {GPPHeIsAbbr(action.Target)} wondering \"what's my name? It's {GetRandomStringFrom($"<b>{ApostrophizeWithOrWithoutS(action.Unit.Name)}</b> breasts", "boob fat", "titties")}, right?\"",
                    $"As <b>{action.Target.Name}</b> sways on <b>{ApostrophizeWithOrWithoutS(action.Unit.Name)}</b> chest, {GPPHe(action.Target)} begin{SIfSingular(action.Target)} to feel somewhat... faded. As though a little less \"<b>{action.Target.Name}</b>\" exists with every sway."
                );
        }
        int ran = Random.Range(0, 9);
        switch (ran)
        {
            case 0:
                string loc = action.preyLocation.ToSyn();
                return $"<b>{action.Target.Name}</b> feels weak; <b>{action.Unit.Name}</b>'s {loc + (loc.EndsWith("s") ? " are" : " is")} overwhelming.";
            case 1:
                return $"<b>{action.Target.Name}</b> is about to give up fighting <b>{action.Unit.Name}</b>'s {action.preyLocation.ToSyn()}.";
            case 2:
                return $"<b>{action.Target.Name}</b> is fading in the {GetPredDesc(action.Unit)} {GetRaceDescSingl(action.Unit)}'s {action.preyLocation.ToSyn()}. <b>{action.Unit.Name}</b> licks {GPPHis(action.Unit)} lips smugly, feeling it happen.";
            case 3:
                return $"The struggles of <b>{action.Target.Name}</b> become weaker, {GPPHis(action.Target)} death imminent.";
            case 4:
                return $"<b>{action.Target.Name}</b> feels {GPPHis(action.Target)} body becoming soft and pliable.";
            case 5:
                return $"<b>{action.Target.Name}</b> clearly doesn’t have the strength to avoid {GPPHis(action.Target)} messy fate anymore.";
            case 6:
                return $"<b>{action.Target.Name}</b> whimpers, realizing {GPPHis(action.Target)} gurgly doom has arrived as <b>{action.Unit.Name}</b>'s {action.preyLocation.ToSyn()} readies to contract one last time.";
            case 7:
                return $"<b>{action.Unit.Name}</b> can feel <b>{action.Target.Name}</b> submitting to {GPPHis(action.Unit)} {action.preyLocation.ToSyn()}, licking {GPPHis(action.Unit)} lips in satisfaction.";
            default:
                return $"<b>{action.Target.Name}</b> has no strength left. Fears death in the {action.preyLocation.ToSyn()} of <b>{action.Unit.Name}</b>.";
        }
    }

    private string GenerateDigestionDeathMessage(EventLog action)
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b> digested <b>{action.Target.Name}</b>.";

        return GetStoredMessage(StoredLogTexts.MessageTypes.DigestionDeathMessages, action);
    }

    private string GenerateGreatEscapeKeepMessage(EventLog action)
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b> held <b>{action.Target.Name}</b> without digesting {GPPHim(action.Target)}.";

        return GetStoredMessage(StoredLogTexts.MessageTypes.GreatEscapeKeep, action);
    }

    private string GenerateGreatEscapeFleeMessage(EventLog action)
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b>'s prey <b>{action.Target.Name}</b> managed to escape and flee the map.";

        return GetStoredMessage(StoredLogTexts.MessageTypes.GreatEscapeFlee, action);
    }

    private string GenerateAbsorptionMessage(EventLog action)
    {
        if (SimpleText)
            return $"<b>{action.Unit.Name}</b> finished absorbing the leftover nutrients from <b>{action.Target.Name}</b>.";
        return GetStoredMessage(StoredLogTexts.MessageTypes.AbsorptionMessages, action);

    }

    string GetStoredMessage(StoredLogTexts.MessageTypes msgType, EventLog action)
    {
        List<StoredLogTexts.EventString> list = StoredLogTexts.Redirect(msgType);
        IEnumerable<StoredLogTexts.EventString> messages = list.Where(s => (s.ActorRace == action.Unit.Race || s.ActorRace == (Race)4000) && (s.TargetRace == action.Target.Race || s.TargetRace == (Race)4000) &&
        s.Conditional(action));

        if (messages.Any() == false)
        {
            return $"Couldn't find matching message {action.Unit.Name} {action.Type} {action.Target?.Name ?? ""}";
        }

        int priority = messages.Max(s => s.Priority);
        if (priority == 9 && State.Rand.Next(2) == 0)
            priority = 8;

        StoredLogTexts.EventString[] array = messages.Where(s => s.Priority == priority).ToArray();
        return array[State.Rand.Next(array.Length)].GetString(action);
    }

    public void RegisterHit(Unit Attacker, Unit Defender, Weapon weapon, int damage, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Hit,
            Unit = Attacker,
            Damage = damage,
            Target = Defender,
            Weapon = weapon,
            Odds = odds
        });
        UpdateListing();
    }

    public void RegisterMiss(Unit Attacker, Unit Defender, Weapon weapon, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Miss,
            Unit = Attacker,
            Target = Defender,
            Weapon = weapon,
            Odds = odds
        });
        UpdateListing();
    }

    public void RegisterVore(Unit predator, Unit prey, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Devour,
            Unit = predator,
            Target = prey,
            Odds = odds,
            preyLocation = PreyLocation.stomach,
        });
        UpdateListing();
    }

    public void RegisterUnbirth(Unit predator, Unit prey, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Unbirth,
            Unit = predator,
            Target = prey,
            Odds = odds,
            preyLocation = PreyLocation.womb,
        });
        UpdateListing();
    }

    public void RegisterCockVore(Unit predator, Unit prey, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.CockVore,
            Unit = predator,
            Target = prey,
            Odds = odds,
            preyLocation = PreyLocation.balls,
        });
        UpdateListing();
    }

    public void RegisterBreastVore(Unit predator, Unit prey, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.BreastVore,
            Unit = predator,
            Target = prey,
            Odds = odds,
            preyLocation = PreyLocation.breasts,
        });
        UpdateListing();
    }

    public void RegisterTailVore(Unit predator, Unit prey, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.TailVore,
            Unit = predator,
            Target = prey,
            Odds = odds,
            preyLocation = PreyLocation.tail,
        });
        UpdateListing();
    }

    public void RegisterAnalVore(Unit predator, Unit prey, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.AnalVore,
            Unit = predator,
            Target = prey,
            Odds = odds,
            preyLocation = PreyLocation.anal,
        });
        UpdateListing();
    }

    public void RegisterBellyRub(Unit rubber, Unit target, Unit prey, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.BellyRub,
            Unit = rubber,
            Target = target,
            Prey = prey ?? defaultPrey,
            Odds = odds,
            preyLocation = PreyLocation.stomach,
        });
        UpdateListing();
    }

    public void RegisterBreastRub(Unit rubber, Unit target, Unit prey, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.BreastRub,
            Unit = rubber,
            Target = target,
            Prey = prey ?? defaultPrey,
            Odds = odds,
            preyLocation = PreyLocation.breasts,
        });
        UpdateListing();
    }

    public void RegisterTailRub(Unit predator, Unit target, Unit prey, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.TailRub,
            Unit = predator,
            Target = prey,
            Prey = prey ?? defaultPrey,
            Odds = odds,
            preyLocation = PreyLocation.tail,
        });
        UpdateListing();
    }

    public void RegisterBallMassage(Unit rubber, Unit target, Unit prey, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.BallMassage,
            Unit = rubber,
            Target = target,
            Prey = prey ?? defaultPrey,
            Odds = odds
        });
        UpdateListing();
    }

    public void RegisterFeed(Unit predator, Unit target, Unit prey, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Feed,
            Unit = predator,
            Target = target,
            Prey = prey ?? defaultPrey,
            Odds = odds
        });
        UpdateListing();
    }

    public void RegisterCumFeed(Unit predator, Unit target, Unit prey, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.FeedCum,
            Unit = predator,
            Target = target,
            Prey = prey ?? defaultPrey,
            Odds = odds
        });
        UpdateListing();
    }

    public void RegisterSuckle(Unit user, Unit target, PreyLocation location, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Suckle,
            Unit = user,
            Target = target,
            preyLocation = location,
            Odds = odds
        });
        UpdateListing();
    }

    public void RegisterSuckleFail(Unit user, Unit target, PreyLocation location, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.SuckleFail,
            Unit = user,
            Target = target,
            preyLocation = location,
            Odds = odds
        });
        UpdateListing();
    }

    public void RegisterBirth(Unit predator, Unit prey, float odds, string extra = "none")
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Birth,
            Unit = predator,
            Target = prey,
            Odds = odds,
            Extra = extra
        });
        UpdateListing();
    }

    public void RegisterTransferSuccess(Unit donor, Unit recipient, Unit donation, float odds, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.TransferSuccess,
            Unit = donor,
            Target = recipient,
            Prey = donation,
            Odds = odds,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void RegisterTransferFail(Unit predator, Unit prey, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.TransferFail,
            Unit = predator,
            Target = prey,
            Odds = odds,
        });
        UpdateListing();
    }

    public void RegisterVoreStealSuccess(Unit donor, Unit recipient, Unit donation, float odds, PreyLocation loc, PreyLocation oldLoc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.VoreStealSuccess,
            Unit = recipient,
            Target = donor,
            Prey = donation,
            Odds = odds,
            preyLocation = loc,
            oldLocation = oldLoc,
        });
        UpdateListing();
    }

    public void RegisterVoreStealFail(Unit donor, Unit recipient, Unit donation, PreyLocation oldLoc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.VoreStealFail,
            Unit = recipient,
            Target = donor,
            Prey = donation,
            oldLocation = oldLoc,
        });
        UpdateListing();
    }

    public void RegisterResist(Unit predator, Unit prey, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Resist,
            Unit = predator,
            Target = prey,
            Odds = odds
        });
        UpdateListing();
    }

    public void RegisterDazzle(Unit attacker, Unit target, float odds)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Dazzle,
            Unit = attacker,
            Target = target,
            Odds = odds
        });
        UpdateListing();
    }

    public void RegisterSpellCast(Unit attacker, Unit target, SpellTypes type)
    {
        events.Add(new SpellLog
        {
            Type = MessageLogEvent.SpellCast,
            Unit = attacker,
            Target = target,
            SpellType = type
        });
        UpdateListing();
    }

    public void RegisterSpellHit(Unit attacker, Unit target, SpellTypes type, int damage, float odds, PreyLocation loc = PreyLocation.stomach)
    {
        events.Add(new SpellLog
        {
            Type = MessageLogEvent.SpellHit,
            Unit = attacker,
            Target = target,
            Damage = damage,
            SpellType = type,
            Odds = odds,
            preyLocation = loc
        });
        UpdateListing();
    }

    public void RegisterSpellMiss(Unit attacker, Unit target, SpellTypes type, float odds)
    {
        events.Add(new SpellLog
        {
            Type = MessageLogEvent.SpellMiss,
            Unit = attacker,
            Target = target,
            SpellType = type,
            Odds = odds
        });
        UpdateListing();
    }

    public void RegisterSpellKill(Unit attacker, Unit target, SpellTypes type)
    {
        events.Add(new SpellLog
        {
            Type = MessageLogEvent.SpellKill,
            Unit = attacker,
            Target = target,
            SpellType = type
        });
        UpdateListing();
    }


    public void RegisterKill(Unit Attacker, Unit Defender, Weapon weapon)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Kill,
            Unit = Attacker,
            Target = Defender,
            Weapon = weapon
        });
        UpdateListing();
    }

    public void RegisterDigest(Unit predator, Unit prey, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Digest,
            Unit = predator,
            Target = prey,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void RegisterAbsorb(Unit predator, Unit prey, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Absorb,
            Unit = predator,
            Target = prey,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void RegisterPartialEscape(Unit predator, Unit prey, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.PartialEscape,
            Unit = predator,
            Target = prey,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void RegisterEscape(Unit predator, Unit prey, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Escape,
            Unit = predator,
            Target = prey,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void RegisterFreed(Unit predator, Unit prey, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Freed,
            Unit = predator,
            Target = prey,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void RegisterRegurgitated(Unit predator, Unit prey, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Regurgitated,
            Unit = predator,
            Target = prey,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void RegisterHeal(Unit unit, int[] amount, string type = "absorb", string extra = "none")
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Heal,
            Unit = unit,
            Damage = amount[0],
            Bonus = amount[1],
            Message = type,
            Extra = extra
        });
        UpdateListing();
    }

    public void RegisterNearDigestion(Unit predator, Unit prey, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.LowHealth,
            Unit = predator,
            Target = prey,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void RegisterCurseExpiration(Unit predator, Unit prey, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.CurseExpires,
            Unit = predator,
            Target = prey,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void RegisterDiminishmentExpiration(Unit predator, Unit prey, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.DiminishmentExpires,
            Unit = predator,
            Target = prey,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void LogDigestionRandom(Unit predator, Unit prey, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.RandomDigestion,
            Unit = predator,
            Target = prey,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void LogGreatEscapeKeep(Unit predator, Unit prey, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.GreatEscapeKeep,
            Unit = predator,
            Target = prey,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void LogGreatEscapeFlee(Unit predator, Unit prey, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.GreatEscapeFlee,
            Unit = predator,
            Target = prey,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void RegisterNewTurn(string name, int amount)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.NewTurn,
            Message = $"Turn {amount} - {name}"
        });
        UpdateListing();
    }

    public void RegisterMiscellaneous(string str)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Miscellaneous,
            Message = str,
        });
        UpdateListing();
    }

    public void RegisterRegurgitate(Unit predator, Unit prey, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.ManualRegurgitation,
            Unit = predator,
            Target = prey,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void RegisterForcefeed(Unit prey, Unit forcedPred, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.Forcefeeding,
            Unit = prey,
            Target = forcedPred,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void RegisterForcefeedFail(Unit prey, Unit target)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.ForcefeedFail,
            Unit = prey,
            Target = target
        });
        UpdateListing();
    }

    public void RegisterTraitConvert(Unit pred, Unit prey, PreyLocation loc)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.TraitConvert,
            Unit = pred,
            Target = prey,
            Prey = prey,
            preyLocation = loc,
        });
        UpdateListing();
    }

    public void RegisterTraitRebirth(Unit pred, Unit prey, PreyLocation loc, Race origRace)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.TraitRebirth,
            Unit = pred,
            Target = prey,
            Prey = prey,
            preyLocation = loc,
            oldRace = origRace
        });
        UpdateListing();
    }

    public void RegisterCumGestation(Unit pred, Unit mother, Unit seed, Unit offspring)
    {
        events.Add(new EventLog
        {
            Type = MessageLogEvent.CumGestation,
            Unit = pred,
            Target = mother,
            Prey = seed,
            AdditionalUnit = offspring
        });
        UpdateListing();
    }
}
