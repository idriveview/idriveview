using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IDriveView.Models
{
    public class QueueUploadViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _items;
        private string _newItemText;
        private int _itemsCount;

        public QueueUploadViewModel()
        {
            Items = new ObservableCollection<string>();
            Items.CollectionChanged += (s, e) => ItemsCount = Items.Count; // Обновляем при изменении коллекции
        }

        public ObservableCollection<string> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
                ItemsCount = _items.Count; // Обновляем при замене коллекции
            }
        }

        public int ItemsCount
        {
            get => _itemsCount;
            set
            {
                _itemsCount = value;
                OnPropertyChanged();
            }
        }

        public string NewItemText
        {
            get => _newItemText;
            set
            {
                _newItemText = value;
                OnPropertyChanged();
            }
        }

        public int Count => Items.Count;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
