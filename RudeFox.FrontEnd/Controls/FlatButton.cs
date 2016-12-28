using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RudeFox.Controls
{
    public class FlatButton : Button
    {
        static FlatButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlatButton), new FrameworkPropertyMetadata(typeof(FlatButton)));
        }

        public bool IsDestructive
        {
            get { return (bool)GetValue(IsDestructiveProperty); }
            set { SetValue(IsDestructiveProperty, value); }
        }

        public static readonly DependencyProperty IsDestructiveProperty =
            DependencyProperty.Register(nameof(IsDestructive), typeof(bool), typeof(FlatButton), new PropertyMetadata(false));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(FlatButton), new PropertyMetadata(new CornerRadius(3)));



        public bool IsRecommended
        {
            get { return (bool)GetValue(IsRecommendedProperty); }
            set { SetValue(IsRecommendedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsGood.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRecommendedProperty =
            DependencyProperty.Register(nameof(IsRecommended), typeof(bool), typeof(FlatButton), new PropertyMetadata(false));

    }

  
}
