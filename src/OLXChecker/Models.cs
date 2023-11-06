using System.ComponentModel;

namespace OLXChecker
{
    public class Account : INotifyPropertyChanged
    {
        private int id;
        public int Id
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string secret;
        public string Secret
        {
            get { return secret; }
            set
            {
                if (secret != value)
                {
                    secret = value;
                    OnPropertyChanged(nameof(Secret));
                }
            }
        }

        private string access;
        public string Access
        {
            get { return access; }
            set
            {
                if (access != value)
                {
                    access = value;
                    OnPropertyChanged(nameof(Access));
                }
            }
        }

        private string refresh;
        public string Refresh
        {
            get { return refresh; }
            set
            {
                if (refresh != value)
                {
                    refresh = value;
                    OnPropertyChanged(nameof(Refresh));
                }
            }
        }

        private int numberOfMessages;
        public int NumberOfMessages
        {
            get { return numberOfMessages; }
            set
            {
                if (numberOfMessages != value)
                {
                    numberOfMessages = value;
                    OnPropertyChanged(nameof(NumberOfMessages));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
