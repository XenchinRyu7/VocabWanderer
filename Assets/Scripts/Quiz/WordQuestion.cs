using System.Collections.Generic;

[System.Serializable]
public class WordQuestion
{
    public string answer;
    public List<int> missingIndices;
    public string context;
    public int backgroundIndex;
}
