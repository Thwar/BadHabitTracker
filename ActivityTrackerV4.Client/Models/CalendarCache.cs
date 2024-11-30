namespace ActivityTrackerV4.Models
{
    // Cache wrapper class to store data with a timestamp
    public class CacheWrapper<T>
    {
        public T Data { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
