using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace VelEditor.Utilities.Controls
{
    class ScalarBox : NumberBox
    {
        static ScalarBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScalarBox), new FrameworkPropertyMetadata(typeof(ScalarBox)));
        }
    }
}
