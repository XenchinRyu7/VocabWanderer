using System.Collections.Generic;

[System.Serializable]
public class CharacterInfo
{
    public string name;
    public string sprite;
    public string position;
}

[System.Serializable]
public class DialogLine
{
    public string speaker;
    public string text;
    public string expression;
    public string @event;
    public string quiz_schema;
    public int? quiz_index;
    public string next_schema; // Field untuk next_schema event
}

[System.Serializable]
public class DialogScene
{
    public string sceneId;
    public string schema; // Schema identifier (e.g., "schema_1")
    public string background;
    public List<CharacterInfo> characters;
    public List<DialogLine> dialog;
}
