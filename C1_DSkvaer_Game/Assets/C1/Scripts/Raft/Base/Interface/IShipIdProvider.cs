namespace Ship {
    public interface IShipIdProvider {
        string CreateId();
        string TagHint { get; }
    }
}
