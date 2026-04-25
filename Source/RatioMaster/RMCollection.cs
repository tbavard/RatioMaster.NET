namespace RatioMaster_source
{
    using System.Collections;

    internal class RMCollection<TItem> : CollectionBase
    {
        internal TItem this[int index]
        {
            get
            {
                return (TItem)this.List[index];
            }

            set
            {
                this.List[index] = value;
            }
        }

        internal int Add(TItem value)
        {
            return this.List.Add(value);
        }
    }
}