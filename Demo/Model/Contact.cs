using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ListViewSample.Model
{
    public class Contact : INotifyPropertyChanged
    {
        private static readonly Random Random = new Random();

        #region Properties
        private string _initials;
        public string Initials
        {
            get
            {
                if (_initials == string.Empty && FirstName != string.Empty && LastName != string.Empty)
                {
                    _initials = FirstName[0].ToString() + LastName[0];
                }
                return _initials;
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                if (_name == string.Empty && FirstName != string.Empty && LastName != string.Empty)
                {
                    _name = FirstName + " " + LastName;
                }
                return _name;
            }
        }

        private string _lastName;
        public string LastName
        {
            get
            {
                return _lastName;
            }
            set
            {
                _lastName = value;
                _initials = string.Empty; // force to recalculate the value 
                _name = string.Empty; // force to recalculate the value 
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _firstName;
        public string FirstName
        {
            get
            {
                return _firstName;
            }
            set
            {
                _firstName = value;
                _initials = string.Empty; // force to recalculate the value 
                _name = string.Empty; // force to recalculate the value 
            }
        }
        public string Position { get; set; }
        public string PhoneNumber { get; set; }
        public string Biography { get; set; }

        private bool _isSwipedRight;
        public bool IsSwipedRight
        {
            get { return _isSwipedRight; }
            set
            {
                _isSwipedRight = value;
                Notify(nameof(IsSwipedRight));
            }
        }

        private bool _isSwipedLeft;
        public bool IsSwipedLeft
        {
            get { return _isSwipedLeft; }
            set
            {
                _isSwipedLeft = value;
                Notify(nameof(IsSwipedLeft));
            }
        }

        private bool _isStar;
        public bool IsStar
        {
            get { return _isStar; }
            set
            {
                if (value != _isStar)
                {
                    _isStar = value;
                    Notify(nameof(IsStar));
                }
            }
        }

        private bool _isDelete;
        public bool IsDelete
        {
            get { return _isDelete; }
            set
            {
                if (value != _isDelete)
                {
                    _isDelete = value;
                    Notify(nameof(IsDelete));
                }
            }
        }

        protected void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public Contact()
        {
            // default values for each property.
            _initials = string.Empty;
            _name = string.Empty;
            LastName = string.Empty;
            FirstName = string.Empty;
            Position = string.Empty;
            PhoneNumber = string.Empty;
            Biography = string.Empty;
        }

        #region Public Methods
        public static Contact GetNewContact()
        {
            return new Contact()
            {
                FirstName = GenerateFirstName(),
                LastName = GenerateLastName(),
                Biography = GetBiography(),
                PhoneNumber = GeneratePhoneNumber(),
                Position = GeneratePosition(),
                IsSwipedRight = false,
                IsSwipedLeft = false
            };
        }
        public static ObservableCollection<Contact> GetContacts(int numberOfContacts)
        {
            ObservableCollection<Contact> contacts = new ObservableCollection<Contact>();

            for (int i = 0; i < numberOfContacts; i++)
            {
                contacts.Add(GetNewContact());
            }
            return contacts;
        }
        public static ObservableCollection<GroupInfoList> GetContactsGrouped(int numberOfContacts)
        {
            ObservableCollection<GroupInfoList> groups = new ObservableCollection<GroupInfoList>();

            var query = from item in GetContacts(numberOfContacts)
                        group item by item.LastName[0] into g
                        orderby g.Key
                        select new { GroupName = g.Key, Items = g };

            foreach (var g in query)
            {
                GroupInfoList info = new GroupInfoList();
                info.Key = g.GroupName;
                foreach (var item in g.Items)
                {
                    info.Add(item);
                }
                groups.Add(info);
            }

            return groups;
        }
        #endregion

        #region Helpers
        private static string GeneratePosition()
        {
            List<string> positions = new List<string>() { "Program Manager", "Developer", "Product Manager", "Evangelist" };
            return positions[Random.Next(0, positions.Count)];
        }
        private static string GetBiography()
        {
            List<string> biographies = new List<string>()
            {
                @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer id facilisis lectus. Cras nec convallis ante, quis pulvinar tellus. Integer dictum accumsan pulvinar. Pellentesque eget enim sodales sapien vestibulum consequat.",
                @"Maecenas eu sapien ac urna aliquam dictum.",
                @"Nullam eget mattis metus. Donec pharetra, tellus in mattis tincidunt, magna ipsum gravida nibh, vitae lobortis ante odio vel quam.",
                @"Quisque accumsan pretium ligula in faucibus. Mauris sollicitudin augue vitae lorem cursus condimentum quis ac mauris. Pellentesque quis turpis non nunc pretium sagittis. Nulla facilisi. Maecenas eu lectus ante. Proin eleifend vel lectus non tincidunt. Fusce condimentum luctus nisi, in elementum ante tincidunt nec.",
                @"Aenean in nisl at elit venenatis blandit ut vitae lectus. Praesent in sollicitudin nunc. Pellentesque justo augue, pretium at sem lacinia, scelerisque semper erat. Ut cursus tortor at metus lacinia dapibus.",
                @"Ut consequat magna luctus justo egestas vehicula. Integer pharetra risus libero, et posuere justo mattis et.",
                @"Proin malesuada, libero vitae aliquam venenatis, diam est faucibus felis, vitae efficitur erat nunc non mauris. Suspendisse at sodales erat.",
                @"Aenean vulputate, turpis non tincidunt ornare, metus est sagittis erat, id lobortis orci odio eget quam. Suspendisse ex purus, lobortis quis suscipit a, volutpat vitae turpis.",
                @"Duis facilisis, quam ut laoreet commodo, elit ex aliquet massa, non varius tellus lectus et nunc. Donec vitae risus ut ante pretium semper. Phasellus consectetur volutpat orci, eu dapibus turpis. Fusce varius sapien eu mattis pharetra.",
                @"Nam vulputate eu erat ornare blandit. Proin eget lacinia erat. Praesent nisl lectus, pretium eget leo et, dapibus dapibus velit. Integer at bibendum mi, et fringilla sem."
            };
            return biographies[Random.Next(0, biographies.Count)];
        }

        private static string GeneratePhoneNumber()
        {
            return string.Format("{0:(###)} {1:###}-{2:####}", Random.Next(100, 999), Random.Next(100, 999), Random.Next(1000, 9999));
        }
        private static string GenerateFirstName()
        {
            List<string> names = new List<string>() { "Lilly", "Mukhtar", "Sophie", "Femke", "Abdul-Rafi'", "Chirag-ud-D...", "Mariana", "Aarif", "Sara", "Ibadah", "Fakhr", "Ilene", "Sardar", "Hanna", "Julie", "Iain", "Natalia", "Henrik", "Rasa", "Quentin", "Gadi", "Pernille", "Ishtar", "Jimme", "Justina", "Lale", "Elize", "Rand", "Roshanara", "Rajab", "Bijou", "Marcus", "Marcus", "Alima", "Francisco", "Thaqib", "Andreas", "Mariana", "Amalie", "Rodney", "Dena", "Fadl", "Ammar", "Anna", "Nasreen", "Reem", "Tomas", "Filipa", "Frank", "Bari'ah", "Parvaiz", "Jibran", "Tomas", "Elli", "Carlos", "Diego", "Henrik", "Aruna", "Vahid", "Eliana", "Roxane", "Amanda", "Ingrid", "Wander", "Malika", "Basim", "Eisa", "Alina", "Andreas", "Deeba", "Diya", "Parveen", "Bakr", "Celine", "Bakr", "Marcus", "Daniel", "Mathea", "Edmee", "Hedda", "Maria", "Maja", "Alhasan", "Alina", "Hedda", "Victor", "Aaftab", "Guilherme", "Maria", "Kai", "Sabien", "Abdel", "Fadl", "Bahaar", "Vasco", "Jibran", "Parsa", "Catalina", "Fouad", "Colette" };
            return names[Random.Next(0, names.Count)];
        }
        private static string GenerateLastName()
        {
            List<string> lastnames = new List<string>() { "Carlson", "Attia", "Quint", "Hollenberg", "Khoury", "Araujo", "Hakimi", "Seegers", "Abadi", "al", "Krommenhoek", "Siavashi", "Kvistad", "Sjo", "Vanderslik", "Fernandes", "Dehli", "Sheibani", "Laamers", "Batlouni", "Lyngvær", "Oveisi", "Veenhuizen", "Gardenier", "Siavashi", "Mutlu", "Karzai", "Mousavi", "Natsheh", "Seegers", "Nevland", "Lægreid", "Bishara", "Cunha", "Hotaki", "Kyvik", "Cardoso", "Pilskog", "Pennekamp", "Nuijten", "Bettar", "Borsboom", "Skistad", "Asef", "Sayegh", "Sousa", "Medeiros", "Kregel", "Shamoun", "Behzadi", "Kuzbari", "Ferreira", "Van", "Barros", "Fernandes", "Formo", "Nolet", "Shahrestaani", "Correla", "Amiri", "Sousa", "Fretheim", "Van", "Hamade", "Baba", "Mustafa", "Bishara", "Formo", "Hemmati", "Nader", "Hatami", "Natsheh", "Langen", "Maloof", "Berger", "Ostrem", "Bardsen", "Kramer", "Bekken", "Salcedo", "Holter", "Nader", "Bettar", "Georgsen", "Cunha", "Zardooz", "Araujo", "Batalha", "Antunes", "Vanderhoorn", "Nader", "Abadi", "Siavashi", "Montes", "Sherzai", "Vanderschans", "Neves", "Sarraf", "Kuiters" };
            return lastnames[Random.Next(0, lastnames.Count)];
        }
        #endregion
    }
}
