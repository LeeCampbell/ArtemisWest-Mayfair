namespace ArtemisWest.Mayfair.Infrastructure
{
    public sealed class ViewModelState
    {
        public static readonly ViewModelState Idle = new ViewModelState(false);
        public static readonly ViewModelState Busy = new ViewModelState(true);

        public static ViewModelState Error(string errorMessage)
        {
            return new ViewModelState(errorMessage);
        }

        private ViewModelState(bool isBusy)
        {
            IsBusy = isBusy;
        }
        private ViewModelState(string errorMessage)
        {
            IsBusy = false;
            HasError = true;
            ErrorMessage = errorMessage;
        }

        public bool IsBusy { get; }
        public bool HasError { get; }
        public string ErrorMessage { get; }
    }
}