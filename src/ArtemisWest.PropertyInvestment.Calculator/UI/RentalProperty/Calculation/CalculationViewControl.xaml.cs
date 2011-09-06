namespace ArtemisWest.PropertyInvestment.Calculator.UI.RentalProperty.Calculation
{
    /// <summary>
    /// Interaction logic for CalculationViewControl.xaml
    /// </summary>
    public partial class CalculationViewControl
    {
        public CalculationViewControl()
        {
            InitializeComponent();
        }

        public CalculationViewModel ViewModel
        {
            get { return DataContext as CalculationViewModel; }
            set { DataContext = value; }
        }

    }
}
