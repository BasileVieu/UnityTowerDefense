using System.Collections.Generic;

[System.Serializable]
public class GameBehaviorCollection
{
    List<GameBehavior> behaviors = new List<GameBehavior>();

    public bool IsEmpty => behaviors.Count == 0;

    public void Add(GameBehavior _behavior)
    {
        behaviors.Add(_behavior);
    }

    public void Clear()
    {
        foreach (GameBehavior behavior in behaviors)
        {
            behavior.Recycle();
        }

        behaviors.Clear();
    }

    public void GameUpdate()
    {
        for (var i = 0; i < behaviors.Count; i++)
        {
            if (!behaviors[i].GameUpdate())
            {
                int lastIndex = behaviors.Count - 1;
                behaviors[i] = behaviors[lastIndex];
                behaviors.RemoveAt(lastIndex);
                i -= 1;
            }
        }
    }
}