using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SingleFileProject
{
    // =============================
    //          INTERFACES
    // =============================

    public interface IAgeable
    {
        int GetAge(DateTime current);
    }

    public interface IInputReader
    {
        string ReadString(string message);
        int ReadInt(string message);
        DateTime ReadDate(string message);
        char ReadLetter(string message);
    }

    public interface IInputable
    {
        void InputData(IInputReader reader);
    }

    public interface ICountLetter
    {
        int CountLetter(char letter);
    }

    // =============================
    //            SERVICES
    // =============================

    public class ConsoleInputReader : IInputReader
    {
        public string ReadString(string message)
        {
            Console.Write(message);
            return Console.ReadLine();
        }

        public int ReadInt(string message)
        {
            int value;
            Console.Write(message);
            while (!int.TryParse(Console.ReadLine(), out value))
            {
                Console.WriteLine("Помилка! Введіть число.");
                Console.Write(message);
            }
            return value;
        }

        public DateTime ReadDate(string message)
        {
            DateTime date;
            Console.Write(message);
            while (!DateTime.TryParse(Console.ReadLine(), out date))
            {
                Console.WriteLine("Невірний формат дати.");
                Console.Write(message);
            }
            return date;
        }

        public char ReadLetter(string message)
        {
            Console.Write(message);
            string s = Console.ReadLine();
            return s.Length > 0 ? s[0] : ' ';
        }
    }

    // =============================
    //            MODELS
    // =============================

    public abstract class Person : IAgeable, IInputable, ICountLetter
    {
        public string FirstName { get; private set; }
        public string Surname { get; private set; }
        public string Patronymic { get; private set; }
        public DateTime BirthDate { get; private set; }

        protected void SetNames(string first, string last, string pat)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
                throw new ArgumentException("Ім'я та прізвище обов'язкові.");

            FirstName = first.Trim();
            Surname = last.Trim();
            Patronymic = string.IsNullOrWhiteSpace(pat) ? "" : pat.Trim();
        }

        protected void SetBirthDate(DateTime date)
        {
            if (date > DateTime.Today)
                throw new ArgumentException("Дата народження не може бути в майбутньому.");

            BirthDate = date.Date;
        }

        public virtual int GetAge(DateTime current)
        {
            int age = current.Year - BirthDate.Year;
            if (current.Month < BirthDate.Month ||
               (current.Month == BirthDate.Month && current.Day < BirthDate.Day))
                age--;

            return age;
        }

        public int CountLetter(char letter)
        {
            char target = char.ToLower(letter);
            int count = 0;
            foreach (char c in Surname.ToLower())
                if (c == target) count++;
            return count;
        }

        public abstract string GetRoleInfo();

        public virtual void InputData(IInputReader reader)
        {
            string first = reader.ReadString("Ім'я: ");
            string last = reader.ReadString("Прізвище: ");
            string pat = reader.ReadString("По-батькові: ");
            DateTime bd = reader.ReadDate("Дата народження: ");

            SetNames(first, last, pat);
            SetBirthDate(bd);
        }
    }

    public class Student : Person
    {
        public int AdmissionYear { get; private set; }
        public string Specialty { get; private set; }

        private void SetAdmissionYear(int year)
        {
            if (year < 1900 || year > DateTime.Now.Year)
                throw new ArgumentException("Некоректний рік вступу.");
            AdmissionYear = year;
        }

        private void SetSpecialty(string spec)
        {
            if (string.IsNullOrWhiteSpace(spec))
                throw new ArgumentException("Спеціальність обов'язкова.");
            Specialty = spec.Trim();
        }

        public override string GetRoleInfo() =>
            $"Студент ({Specialty}, {AdmissionYear} р.)";

        public override void InputData(IInputReader reader)
        {
            base.InputData(reader);
            SetAdmissionYear(reader.ReadInt("Рік вступу: "));
            SetSpecialty(reader.ReadString("Спеціальність: "));
        }
    }

    // =============================
    //      GRAPHICS LOGIC
    // =============================

    public static class GraphLogic
    {
        public static List<PointF> CalculatePoints()
        {
            List<PointF> pts = new();

            double xStart = 7.2;
            double xEnd = 12.0;
            double dx = 0.05;

            for (double x = xStart; x <= xEnd; x += dx)
            {
                double z = (2 * Math.Pow(Math.Sin(x + 2), 2)) / (x * x + 1);
                pts.Add(new PointF((float)x, (float)z));
            }

            return pts;
        }

        public static List<PointF> ScalePoints(List<PointF> world, Size sz)
        {
            float minX = 7.2f, maxX = 12f;

            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var p in world)
            {
                minY = Math.Min(minY, p.Y);
                maxY = Math.Max(maxY, p.Y);
            }

            List<PointF> scaled = new();

            foreach (var p in world)
            {
                float sx = (p.X - minX) / (maxX - minX) * (sz.Width - 60) + 40;
                float sy = sz.Height - ((p.Y - minY) / (maxY - minY) * (sz.Height - 60) + 40);
                scaled.Add(new PointF(sx, sy));
            }

            return scaled;
        }
    }

    // =============================
    //         CHART CONTROL
    // =============================

    public class ChartControl : Control
    {
        private List<PointF> worldPoints;

        public ChartControl()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            worldPoints = GraphLogic.CalculatePoints();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            List<PointF> scaled = GraphLogic.ScalePoints(worldPoints, ClientSize);

            using Pen axis = new Pen(Color.Black, 2);
            using Pen graph = new Pen(Color.Blue, 2);

            // Осі
            g.DrawLine(axis, 40, Height - 40, Width - 20, Height - 40);
            g.DrawLine(axis, 40, 20, 40, Height - 40);

            // Графік
            for (int i = 0; i < scaled.Count - 1; i++)
                g.DrawLine(graph, scaled[i], scaled[i + 1]);
        }
    }

    // =============================
    //            FORM
    // =============================

    public class GraphForm : Form
    {
        public GraphForm()
        {
            this.Text = "Графік функції z(x)";
            this.Width = 900;
            this.Height = 600;
            Controls.Add(new ChartControl() { Dock = DockStyle.Fill });
        }
    }

    // =============================
    //           PROGRAM
    // =============================

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            IInputReader reader = new ConsoleInputReader();

            Console.WriteLine("=== Введення студента ===");
            Student s = new Student();
            s.InputData(reader);

            DateTime now = reader.ReadDate("Поточна дата: ");
            Console.WriteLine($"Вік: {s.GetAge(now)}");

            char letter = reader.ReadLetter("Введіть літеру: ");
            Console.WriteLine($"У прізвищі '{s.Surname}' літера '{letter}' зустрічається {s.CountLetter(letter)} разів.");

            Console.WriteLine("\nПоказати графік? (y/n): ");
            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Application.EnableVisualStyles();
                Application.Run(new GraphForm());
            }
        }
    }
}
