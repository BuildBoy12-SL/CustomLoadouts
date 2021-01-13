namespace CustomLoadouts
{
    using Exiled.API.Interfaces;
    
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Global { get; set; } = false;
    }
}