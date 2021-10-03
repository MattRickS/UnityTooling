namespace GameServices
{
    public interface IGameService
    {
        public bool LoadOnStart { get; set; }
        public bool SaveOnQuit { get; set; }
        public bool Load();
        public bool Save();
    }

}