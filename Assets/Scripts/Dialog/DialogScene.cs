using System.Collections.Generic;

[System.Serializable]
public class CharacterInfo
{
    public string name;
    public string sprite;
    public string position; // "left", "right"
}

[System.Serializable]
public class DialogLine
{
    public string speaker;
    public string text;
    public string expression;
}

[System.Serializable]
public class DialogScene
{
    public string sceneId;
    public string background;
    public List<CharacterInfo> characters;
    public List<DialogLine> dialog;
}
