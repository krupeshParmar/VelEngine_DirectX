using System.ComponentModel;
using System.Runtime.Serialization;

namespace VelEditor
{
    [DataContract(IsReference = true)]
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // NOTE (to myself): if you're here to make this method internal, "don't"!
        // Find another way! Use the Force! I've faith in you!
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}