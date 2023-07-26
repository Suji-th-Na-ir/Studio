namespace Terra.Studio
{
    public interface IBaseComponent
    {
        public bool CanExecute { get; set; }
        public bool IsExecuted { get; set; }
    }
}
