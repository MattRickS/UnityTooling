using System;


namespace Inventory
{
    /*
    Represents an item with modifications. Modifications are to be applied on
    top of the ItemData, eg,
        data = ItemData(itemID, statistics={Attack: 10})
        delta = ItemDelta(itemID, statistics={Attack: -1})
        value = data.GetStat(Attack) + delta.GetStat(Attack)
    */
    [Serializable]
    public class ItemDelta
    {
        public string itemID;
        public string deltaID = System.Guid.NewGuid().ToString();
        public SerializableDictionary<Statistic, int> statistics;

        public string Id() { return deltaID; }

        public int GetStat(Statistic stat)
        {
            int value;
            if (statistics.TryGetValue(stat, out value))
            {
                return value;
            }
            return 0;
        }
    }
}
