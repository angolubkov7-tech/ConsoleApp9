using System;
using System.Collections.Generic;


namespace Medvedev
{
    class Song
    {
        private string name;
        private string author;
        private Song prev;

        public Song() : this("Без названия", "Неизвестен")
        {

        }
        //Конструктор с параметрами
        public Song(string name, string author) : this(name, author, null)
        {

        }

        public Song(string name, string author, Song prev)
        {
            this.name = name;
            this.author = author;
            this.prev = prev;
        }
        public void SetName(string name)
        {
            this.name = name;
        }

        public void SetAuthor(string author)
        {
            this.author = author;
        }

        public void SetPrev(Song prev)
        {
            this.prev = prev;
        }

        public void Print()
        {
            Console.WriteLine($"{name} - {author}");
        }

        public string Title()
        {
            return $"{name} — {author}";
        }

        public override bool Equals(object d)
        {
            if (d == null) return false;

            Song other = d as Song;
            if (other == null) return false;

            if (ReferenceEquals(this, other)) return true;

            //StringComparison.OrdinalIgnoreCase - для быстрого сравнения строк без учета регистра заставляет сравнивать числовые коды символов без учета верхнего или нижнего регистра
            return string.Equals(this.name, other.name, StringComparison.OrdinalIgnoreCase) && string.Equals(this.author, other.author, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return Title();
        }
    }

    class Program
    {
        public static void Main(string[] args)
        {
            List<Song> songs = new List<Song>
            {
                new Song("Кукла колдуна", "Король и Шут"),
                new Song("Лесник", "Король и Шут"),
                new Song("Группа крови", "Кино"),
                new Song("Звезда по имени Солнце", "Кино")
            };

            for (int i = 1; i < songs.Count; i++)
            {
                songs[i].SetPrev(songs[i - 1]);
            }

            Console.WriteLine("Список песен");
            for (int i = 0; i < songs.Count; i++)
            {
                Console.Write($"{i + 1}. ");
                songs[i].Print();
            }

            Console.WriteLine("Сравнение первой и второй песни");
            bool equals = songs[0].Equals(songs[1]);
            Console.WriteLine($"{songs[0].Title()} и {songs[1].Title()}" + (equals ? "-Одинаковые" : "-Разные"));

            Song mySong = new Song();
            Console.WriteLine($"mysong - {mySong.Title()}");
        }
    }
}