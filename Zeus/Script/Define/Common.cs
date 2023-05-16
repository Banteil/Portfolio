namespace Zeus
{
    public interface IDataSerialize<T>
    {
        T Serialize();
    }
    public interface IDataDeserialize<T>
    {
        T Deserialize();
    }
}