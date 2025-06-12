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
    public string @event; // gunakan @event karena event adalah keyword C#
    public string quiz_schema;
    public int? quiz_index; // nullable, karena tidak semua node punya
}

[System.Serializable]
public class DialogScene
{
    public string sceneId;
    public string background;
    public List<CharacterInfo> characters;
    public List<DialogLine> dialog;
}
