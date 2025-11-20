using System;

namespace UniversityLab
{
    /// <summary>
    /// Інтерфейс для обчислення віку.
    /// </summary>
    public interface IAgeable
    {
        int GetAge(DateTime currentDate);
    }

    /// <summary>
    /// Інтерфейс для вводу даних (абстрагує джерело вводу).
    /// </summary>
    public interface IInputReader
    {
        string ReadString(string prompt);
        DateTime ReadDate(string prompt); // кидає FormatException, якщо користувач не дав правильну дату
        char ReadLetter(string prompt);
        int ReadInt(string prompt);
    }

    /// <summary>
    /// Інтерфейс для запуску логіки вводу в об'єкта (модель не читає з консолі напряму).
    /// </summary>
    public interface IInputable
    {
        void InputData(IInputReader reader);
    }

    /// <summary>
    /// Інтерфейс для підрахунку входжень символу в прізвищі.
    /// </summary>
    public interface ICountLetter
    {
        int CountLetter(char letter);
    }

    /// <summary>
    /// Абстрактний базовий клас Людина.
    /// </summary>
    public abstract class Person : IAgeable, IInputable, ICountLetter
    {
        /// <summary>Ім'я</summary>
        public string FirstName { get; private set; }

        /// <summary>Прізвище</summary>
        public string Surname { get; private set; }

        /// <summary>По-батькові</summary>
        public string Patronymic { get; private set; }

        /// <summary>Дата народження</summary>
        public DateTime BirthDate { get; private set; }

        protected Person() { }

        protected Person(string firstName, string surname, string patronymic, DateTime birthDate)
        {
            SetNames(firstName, surname, patronymic);
            SetBirthDate(birthDate);
        }

        /// <summary>
        /// Встановлює імена з валідацією.
        /// </summary>
        protected void SetNames(string firstName, string surname, string patronymic)
        {
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("FirstName is required");
            if (string.IsNullOrWhiteSpace(surname)) throw new ArgumentException("Surname is required");
            FirstName = firstName.Trim();
            Surname = surname.Trim();
            Patronymic = string.IsNullOrWhiteSpace(patronymic) ? string.Empty : patronymic.Trim();
        }

        /// <summary>
        /// Встановлює дату народження з валідацією (не пізніше сьогодні).
        /// </summary>
        protected void SetBirthDate(DateTime date)
        {
            if (date > DateTime.Today) throw new ArgumentException("BirthDate cannot be in the future");
            BirthDate = date.Date;
        }

        /// <summary>
        /// Абстрактний метод, щоб похідні класи могли додати специфічну інформацію.
        /// </summary>
        public abstract string GetRoleInfo();

        /// <summary>
        /// Обчислює вік на вказану дату.
        /// </summary>
        public virtual int GetAge(DateTime currentDate)
        {
            if (currentDate < BirthDate) throw new ArgumentException("Current date cannot be earlier than birth date");
            int age = currentDate.Year - BirthDate.Year;
            if (currentDate.Month < BirthDate.Month || (currentDate.Month == BirthDate.Month && currentDate.Day < BirthDate.Day))
                age--;
            return age;
        }

        /// <summary>
        /// Підраховує входження літери в прізвищі (без врахування регістру).
        /// </summary>
        public int CountLetter(char letter)
        {
            if (string.IsNullOrEmpty(Surname)) return 0;
            char lower = char.ToLowerInvariant(letter);
            int count = 0;
            foreach (char c in Surname.ToLowerInvariant())
            {
                if (c == lower) count++;
            }
            return count;
        }

        /// <summary>
        /// Заповнення даних через IInputReader.
        /// </summary>
        public virtual void InputData(IInputReader reader)
        {
            string first = reader.ReadString("Введіть ім'я: ");
            string surname = reader.ReadString("Введіть прізвище: ");
            string patronymic = reader.ReadString("Введіть по-батькові (можна пусто): ");
            DateTime birth = reader.ReadDate("Введіть дату народження (дд.мм.rrrr або rrrr-mm-dd): ");

            SetNames(first, surname, patronymic);
            SetBirthDate(birth);
        }
    }

    /// <summary>
    /// Клас Студент — похідний від Person.
    /// </summary>
    public class Student : Person
    {
        /// <summary>Рік вступу</summary>
        public int AdmissionYear { get; private set; }

        /// <summary>Спеціальність</summary>
        public string Specialty { get; private set; }

        public Student() : base() { }

        public Student(string firstName, string surname, string patronymic, DateTime birthDate, int admissionYear, string specialty)
            : base(firstName, surname, patronymic, birthDate)
        {
            SetAdmissionYear(admissionYear);
            SetSpecialty(specialty);
        }

        private void SetAdmissionYear(int year)
        {
            if (year < 1900 || year > DateTime.Today.Year) throw new ArgumentException("Invalid admission year");
            AdmissionYear = year;
        }

        private void SetSpecialty(string specialty)
        {
            if (string.IsNullOrWhiteSpace(specialty)) throw new ArgumentException("Specialty is required");
            Specialty = specialty.Trim();
        }

        public override string GetRoleInfo()
        {
            return $"Студент, спеціальність: {Specialty}, рік вступу: {AdmissionYear}";
        }

        public override void InputData(IInputReader reader)
        {
            // Викликаємо батьківський ввод
            base.InputData(reader);

            int admYear = reader.ReadInt("Введіть рік вступу до ВУЗу: ");
            string specialty = reader.ReadString("Введіть спеціальність: ");

            SetAdmissionYear(admYear);
            SetSpecialty(specialty);
        }
    }

    /// <summary>
    /// Реалізація IInputReader для консолі з коректною валідацією.
    /// </summary>
    public class ConsoleInputReader : IInputReader
    {
        public string ReadString(string prompt)
        {
            Console.Write(prompt);
            string? s = Console.ReadLine();
            return s ?? string.Empty;
        }

        public DateTime ReadDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s))
                {
                    Console.WriteLine("Порожній ввід — спробуйте ще.");
                    continue;
                }

                // Спробуємо кілька форматів
                if (DateTime.TryParse(s.Trim(), out DateTime dt))
                {
                    return dt.Date;
                }
                Console.WriteLine("Невірний формат дати. Спробуйте ще (наприклад 1999-12-31 або 31.12.1999).");
            }
        }

        public char ReadLetter(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (!string.IsNullOrEmpty(s))
                {
                    // беремо перший символ (коректніше вимагати один символ, але зручніше так)
                    return s.Trim()[0];
                }
                Console.WriteLine("Порожній ввід — введіть принаймні один символ.");
            }
        }

        public int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (int.TryParse(s, out int res)) return res;
                Console.WriteLine("Невірний ввід числа. Спробуйте ще.");
            }
        }
    }

    class Program
    {
        static void Main()
        {
            try
            {
                IInputReader reader = new ConsoleInputReader();

                Console.WriteLine("=== Введення даних для особи ===");
                // Можна створити конкретну реалізацію Person (наприклад, тимчасовий клас) — але Person абстрактний,
                // тому для демонстрації створимо Student і ще одного Student як 'Person' через поліморфізм.
                Person personAsStudent = new Student();
                personAsStudent.InputData(reader); // демонстрація IInputable через Person

                Console.WriteLine("\n=== Введення даних для студента ===");
                Student student = new Student();
                student.InputData(reader);

                // Поліморфізм: IAgeable
                IAgeable ageable = student;

                Console.WriteLine("\n=== Введення поточної дати для обчислення віку ===");
                DateTime current = reader.ReadDate("Введіть поточну дату (щоб коректно порахувати вік): ");

                int studentAge = ageable.GetAge(current);
                Console.WriteLine($"\nВік студента ({student.FirstName} {student.Surname}): {studentAge} років");

                // Підрахунок літери в прізвищі людини (використовуємо personAsStudent як Person)
                char letter = reader.ReadLetter("\nВведіть літеру для підрахунку в прізвищі людини: ");
                int occurrences = personAsStudent.CountLetter(letter);
                Console.WriteLine($"Літера '{letter}' зустрічається в прізвищі {personAsStudent.Surname} {occurrences} раз(и).");

                // Додатковий приклад: показати рольну інформацію (поліморфізм абстрактного методу)
                Console.WriteLine("\nІнформація про роль:");
                Console.WriteLine(personAsStudent.GetRoleInfo());
                Console.WriteLine(student.GetRoleInfo());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Сталася помилка: {ex.Message}");
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }
}
