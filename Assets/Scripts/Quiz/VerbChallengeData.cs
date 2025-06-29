using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VerbChallengeWrapper
{
    public List<VerbChallenge> challenges;
}

[System.Serializable]
public class VerbChallenge
{
    public string verb_level;
    public string background_asset;
    public List<VerbQuestion> questions;
}

[System.Serializable]
public class VerbQuestion
{
    public string context;
    public string answer;
    public List<string> missing_letters;
    public string scrambled_word;
    public int time_limit_seconds;
}
