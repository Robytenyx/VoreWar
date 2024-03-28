using System.Collections.Generic;

public static class PreyLocStrings
{
    static readonly List<string> mouthSyn = new List<string>() { "mouth", "maw", "gullet", "jaw", "mug", "piehole", "muzzle"};
    static readonly List<string> wombSyn = new List<string>() { "womb", "lower belly", "pussy", "slit", "muff", "cunt" };
    static readonly List<string> breastSyn = new List<string>() { "breasts", "bosom", "bust", "mammaries", "boobs", "cleavage", "tits", "titties" };
    static readonly List<string> breastSynSing = new List<string>() { "breast", "bosom", "mammary", "boob", "cleavage", "tit", "titty", "bust", "chest" };
    static readonly List<string> breastSynPlural = new List<string>() { "breasts", "mammaries", "boobs", "tits", "titties" };
    static readonly List<string> ballsSyn = new List<string>() { "balls", "scrotum", "testicles", "nuts", "orbs", "nutsack" };
    static readonly List<string> ballsSynSing = new List<string>() { "scrotum", "nutsack", "sack", "ballsack" };
    static readonly List<string> ballsSynPlural = new List<string>() { "balls", "testicles", "nuts", "orbs", "testis" };
    static readonly List<string> stomachSyn = new List<string>() { "gut", "stomach", "belly", "tummy", "middle" };
    static readonly List<string> analSyn = new List<string>() { "butt", "ass", "bottom", "backside", "bum", "rear", "rump", "booty", "tush" };
    static readonly List<string> cockSyn = new List<string>() { "wang", "dick", "cock", "phallus", "member", "shaft", "pecker", "schlong" };

    static readonly List<string> wombFluid = new List<string>() { "cum", "ejaculate", "honey", "fem-fluids", "pussy juice" };
    static readonly List<string> breastFluid = new List<string>() { "milk", "delicious milk", "leaking milk", "lactation", "nourishing fluid" };
    static readonly List<string> ballsFluid = new List<string>() { "cum", "sperm", "semen", "jizz", "spunk", "seed" };
    static readonly List<string> stomachFluid = new List<string>() { "nutritious soup", "mush", "nutritious mush", "digestive juices", "chyme", "bubbling mush", "hot slurry", "meaty chunks", "stew", "melting flesh and bones" };


    static readonly List<string> wombVerb = new List<string>() { "release", "birth", "ejaculate" };
    static readonly List<string> breastVerb = new List<string>() { "release", "disgorge", "milk out" };
    static readonly List<string> ballsVerb = new List<string>() { "cum", "release", "ejaculate" };
    static readonly List<string> stomachVerb = new List<string>() { "puke", "spit", "spew", "disgorge" };

    static readonly List<string> wombVerbPastTense = new List<string>() { "released", "gave birth", "ejaculated" };
    static readonly List<string> breastVerbPastTense = new List<string>() { "released", "disgorged", "milked out" };
    static readonly List<string> ballsVerbPastTense = new List<string>() { "cummed", "released", "ejaculated" };
    static readonly List<string> stomachVerbPastTense = new List<string>() { "puked", "spat", "spewed", "disgorged" };

    static readonly List<string> oralVoreVerbPresentTense = new List<string>() { "eats", "devours", "swallows", "gulps", "wolfs", "horks" };
    static readonly List<string> oralVoreVerbPresentContinuousTense = new List<string>() { "eating", "devouring", "swallowing", "gobbling", "gulping", "wolfing", "horking", "downing" };
    static readonly List<string> oralVoreVerbPastTense = new List<string>() { "ate", "devoured", "swallowed", "gobbled", "gulped", "downed" };

    private static string genRandom(List<string> options)
    {
        int index = State.Rand.Next() % options.Count;
        return options[index];
    }

    /// <summary>
    /// Gets a random synonym for the body part(s) associatied with the provided <c>PreyLocation</c>.
    /// <br></br>
    /// NOTICE: Using this function for balls or breasts may return a singular or plural noun!
    /// <br></br>
    /// If specifically needing a singluar or plural noun, use <c>ToBreastSynPlural()</c>, <c>ToBallSynPlural()</c>, or <c>ToBallSynSing()</c> instead.
    /// </summary>
    public static string ToSyn(this PreyLocation preyLocation)
    {
        switch (preyLocation)
        {
            case PreyLocation.breasts:
                return genRandom(breastSyn);
            case PreyLocation.leftBreast:
                return genRandom(breastSyn);
            case PreyLocation.rightBreast:
                return genRandom(breastSyn);
            case PreyLocation.balls:
                return genRandom(ballsSyn);
            case PreyLocation.stomach:
                return genRandom(stomachSyn);
            case PreyLocation.stomach2:
                return genRandom(stomachSyn);
            case PreyLocation.womb:
                return genRandom(wombSyn);
            case PreyLocation.anal:
                return genRandom(analSyn);
            case PreyLocation.tail:
                return "tail";
            default:
                return "";
        }
    }

    /// <summary>
    /// Gets a random synonym for the body part(s) associatied with the provided <c>PreyLocation</c>., followed by the provided <c>string</c>(s), appended with "s"(s) if the random synonym is plural.
    /// <br></br><example>
    /// For example:
    /// <code>
    /// SynAndPluralForm(PreyLocation.balls, "work");
    /// </code>
    /// may return "balls work", "scrotum works", or "nuts work".
    /// 
    /// 
    /// <code>
    /// SynAndPluralForm(PreyLocation.balls, "quiver", "and shake");
    /// </code>
    /// may return "balls quiver and shake", "scrotum quivers and shakes", "testicles quiver and shake", "nuts quiver and shake".
    /// </example>
    /// </summary>
    public static string SynAndPluralForm(this PreyLocation preyLocation, params string[] rest)
    {
        string part;
        switch (preyLocation)
        {
            case PreyLocation.breasts:
                part = genRandom(breastSyn);
                break;
            case PreyLocation.leftBreast:
                part = genRandom(breastSyn);
                break;
            case PreyLocation.rightBreast:
                part = genRandom(breastSyn);
                break;
            case PreyLocation.balls:
                part = genRandom(ballsSyn);
                break;
            case PreyLocation.stomach:
                part = genRandom(stomachSyn);
                break;
            case PreyLocation.stomach2:
                part = genRandom(stomachSyn);
                break;
            case PreyLocation.womb:
                part = genRandom(wombSyn);
                break;
            case PreyLocation.anal:
                part = genRandom(analSyn);
                break;
            case PreyLocation.tail:
                part = "tail";
                break;
            default:
                part = "";
                break;
        }
        string result = part;
        for (int i = 0; i < rest.Length; i++)
        {
            rest[i] = rest[i].Trim();
            if (part.EndsWith("s"))
            {
                if (rest[i].EndsWith("s"))
                    result += " " + rest[i].LastIndexOf("s");
                else
                    result += " " + rest[i];
            }
            else
            {
                if (rest[i].EndsWith("s"))
                    result += " " + rest[i];
                else
                    result += " " + rest[i] + "s";
            }
        }
        return result;
    }

    /// <summary>
    /// Gets a random synonym for the body part(s) associatied with the provided <c>PreyLocation</c>., followed "is" or "are" as appropiate to the plurality of the synonym.
    /// </summary>
    public static string PartIsAre(this PreyLocation preyLocation)
    {
        string part;
        switch (preyLocation)
        {
            case PreyLocation.breasts:
                part = genRandom(breastSyn);
                break;
            case PreyLocation.leftBreast:
                part = genRandom(breastSyn);
                break;
            case PreyLocation.rightBreast:
                part = genRandom(breastSyn);
                break;
            case PreyLocation.balls:
                part = genRandom(ballsSyn);
                break;
            case PreyLocation.stomach:
                part = genRandom(stomachSyn);
                break;
            case PreyLocation.stomach2:
                part = genRandom(stomachSyn);
                break;
            case PreyLocation.womb:
                part = genRandom(wombSyn);
                break;
            case PreyLocation.anal:
                part = genRandom(analSyn);
                break;
            case PreyLocation.tail:
                part = "tail";
                break;
            default:
                part = "";
                break;
        }
        if (part.EndsWith("s"))
            return part + " are";
        else
            return part + " is";
    }

    /// <summary>
    /// Gets a random synonym for mouth.
    /// </summary>
    public static string ToMouthSyn()
    {
        return genRandom(mouthSyn);
    }

    /// <summary>
    /// Gets a random synonym for penis.
    /// </summary>
    public static string ToCockSyn()
    {
        return genRandom(cockSyn);
    }

    /// <summary>
    /// Gets a random plural synonym for breasts.
    /// </summary>
    public static string ToBreastSynPlural()
    {
        return genRandom(breastSynPlural);
    }

    /// <summary>
    /// Gets a random singular synonym for breasts.
    /// </summary>
    public static string ToBreastSynSing()
    {
        return genRandom(breastSynSing);
    }

    /// <summary>
    /// Gets a random plural synonym for scrotum.
    /// </summary>
    public static string ToBallSynPlural()
    {
        return genRandom(ballsSynPlural);
    }

    /// <summary>
    /// Gets a random singular synonym for scrotum.
    /// </summary>
    public static string ToBallSynSing()
    {
        return genRandom(ballsSynSing);
    }

    public static string ToFluid(this PreyLocation preyLocation)
    {
        switch (preyLocation)
        {
            case PreyLocation.breasts:
                return genRandom(breastFluid);
            case PreyLocation.leftBreast:
                return genRandom(breastFluid);
            case PreyLocation.rightBreast:
                return genRandom(breastFluid);
            case PreyLocation.balls:
                return genRandom(ballsFluid);
            case PreyLocation.stomach:
                return genRandom(stomachFluid);
            case PreyLocation.stomach2:
                return genRandom(stomachFluid);
            case PreyLocation.womb:
                return genRandom(wombFluid);
            default:
                return "";
        }

    }
    public static string ToVerb(this PreyLocation preyLocation)
    {
        switch (preyLocation)
        {
            case PreyLocation.breasts:
                return genRandom(breastVerb);
            case PreyLocation.leftBreast:
                return genRandom(breastVerb);
            case PreyLocation.rightBreast:
                return genRandom(breastVerb);
            case PreyLocation.balls:
                return genRandom(ballsVerb);
            case PreyLocation.stomach:
                return genRandom(stomachVerb);
            case PreyLocation.stomach2:
                return genRandom(stomachVerb);
            case PreyLocation.womb:
                return genRandom(wombVerb);
            default:
                return "";
        }
    }
    public static string ToVerbPastTense(this PreyLocation preyLocation)
    {
        switch (preyLocation)
        {
            case PreyLocation.breasts:
                return genRandom(breastVerbPastTense);
            case PreyLocation.leftBreast:
                return genRandom(breastVerbPastTense);
            case PreyLocation.rightBreast:
                return genRandom(breastVerbPastTense);
            case PreyLocation.balls:
                return genRandom(ballsVerbPastTense);
            case PreyLocation.stomach:
                return genRandom(stomachVerbPastTense);
            case PreyLocation.stomach2:
                return genRandom(stomachVerbPastTense);
            case PreyLocation.womb:
                return genRandom(wombVerbPastTense);
            default:
                return "";
        }
    }
    /// <summary>
    /// Gets a random Oral Vore Verb in Present Continuous Tense, such as "swallowing" or "eating".
    /// </summary>
    /// <returns></returns>
    public static string GetOralVoreVPCT()
    { return genRandom(oralVoreVerbPresentContinuousTense); }
    /// <summary>
    /// Gets a random Oral Vore Verb in Past Tense, such as "swallowed" or "ate".
    /// </summary>
    /// <returns></returns>
    public static string GetOralVoreVPastT()
    { return genRandom(oralVoreVerbPastTense); }
    /// <summary>
    /// Gets a random Oral Vore Verb in Present Tense, such as "eats" or "devours".
    /// </summary>
    /// <returns></returns>
    public static string GetOralVoreVPT()
    { return genRandom(oralVoreVerbPresentTense); }
}
