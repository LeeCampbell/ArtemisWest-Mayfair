using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ArtemisWest.PropertyInvestment.Calculator.Controls
{
    [TemplatePart(Name = ValueTextName, Type = typeof(TextBox))]
    public sealed class RangeInput : Control
    {
        public const string ValueTextName = "ValueText";

        static RangeInput()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeInput), new FrameworkPropertyMetadata(typeof(RangeInput)));
        }

        #region Title DependencyProperty

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(RangeInput), new UIPropertyMetadata());

        #endregion

        #region Value DependencyProperty

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value", typeof(double), typeof(RangeInput),
                new FrameworkPropertyMetadata(
                    1d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region MinimumValue DependencyProperty

        public double MinimumValue
        {
            get { return (double)GetValue(MinimumValueProperty); }
            set { SetValue(MinimumValueProperty, value); }
        }
        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.Register("MinimumValue", typeof(double), typeof(RangeInput), new UIPropertyMetadata(0d));

        #endregion

        #region MaximumValue DependencyProperty

        public double MaximumValue
        {
            get { return (double)GetValue(MaximumValueProperty); }
            set { SetValue(MaximumValueProperty, value); }
        }
        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.Register("MaximumValue", typeof(double), typeof(RangeInput), new UIPropertyMetadata(10d));

        #endregion

        #region MinimumIncrement DependencyProperty

        public double MinimumIncrement
        {
            get { return (double)GetValue(MinimumIncrementProperty); }
            set { SetValue(MinimumIncrementProperty, value); }
        }
        public static readonly DependencyProperty MinimumIncrementProperty =
            DependencyProperty.Register("MinimumIncrement", typeof(double), typeof(RangeInput), new UIPropertyMetadata(1d));

        #endregion

        #region AutoIncrementAmount DependencyProperty

        public double AutoIncrementAmount
        {
            get { return (double)GetValue(AutoIncrementAmountProperty); }
            set { SetValue(AutoIncrementAmountProperty, value); }
        }
        public static readonly DependencyProperty AutoIncrementAmountProperty =
            DependencyProperty.Register("AutoIncrementAmount", typeof(double), typeof(RangeInput), new UIPropertyMetadata(1d));

        #endregion

        #region ValueToTextConverter DependencyProperty

        public IValueConverter ValueToTextConverter
        {
            get { return (IValueConverter)GetValue(ValueToTextConverterProperty); }
            set { SetValue(ValueToTextConverterProperty, value); }
        }
        public static readonly DependencyProperty ValueToTextConverterProperty =
            DependencyProperty.Register("ValueToTextConverter", typeof(IValueConverter), typeof(RangeInput), new UIPropertyMetadata(OnValueToTextConverterChanged));

        private static void OnValueToTextConverterChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var self = (RangeInput)dependencyObject;
            self.OnValueToTextConverterChanged(args);
        }

        private void OnValueToTextConverterChanged(DependencyPropertyChangedEventArgs args)
        {
            SetValueTextConverter(args.NewValue as IValueConverter);
        }

        private void SetValueTextConverter(IValueConverter valueTextConverter)
        {
            var template = this.Template;
            if (template == null) return;

            var valueTextBox = this.Template.FindName(ValueTextName, this) as TextBox;
            if (valueTextBox == null) return;
            
            BindingOperations.SetBinding(
                valueTextBox,
                TextBox.TextProperty,
                new Binding("Value")
                    {

                        RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
                        Mode = BindingMode.TwoWay,
                        Converter = valueTextConverter
                    });
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SetValueTextConverter(ValueToTextConverter);
        }
    }
}
