using System;
using System.Collections.Generic;

namespace Medvedev
{
    enum ProjectStatus { Draft, InProgress, Closed }
    enum TaskStatus { Assigned, InProgress, OnReview, Done }

    class Person
    {
        private string name;
        public Person(string name) { this.name = name; }
        public string Name { get { return name; } }
        public override string ToString() { return name; }
    }

    class Report
    {
        private readonly string text;
        private readonly DateTime date;
        private readonly Person author;

        public Report(string text, DateTime date, Person author)
        {
            this.text = text;
            this.date = date;
            this.author = author;
        }

        public Report(string text, Person author) : this(text, DateTime.Now, author) { }

        public string Text { get { return text; } }
        public DateTime Date { get { return date; } }
        public Person Author { get { return author; } }

        public override string ToString()
        {
            return $"{date:dd.MM.yyyy HH:mm} | {author.Name}: {text}";
        }
    }

    class Task
    {
        private static int nextNumber = 1;

        private int number;
        private string description;
        private DateTime deadline;
        private Person initiator;
        private Person assignee;
        private TaskStatus status;
        private Report report;
        private Project project;

        public Task(string description, DateTime deadline, Person initiator, Person assignee, Project project)
        {
            this.number = nextNumber++;
            this.description = description;
            this.deadline = deadline;
            this.initiator = initiator;
            this.assignee = assignee;
            this.project = project;
            this.status = TaskStatus.Assigned;
            this.report = null;
        }

        public int Number { get { return number; } }
        public string Description { get { return description; } }
        public DateTime Deadline { get { return deadline; } }
        public Person Initiator { get { return initiator; } }
        public Person Assignee { get { return assignee; } }
        public TaskStatus Status { get { return status; } }
        public Report Report { get { return report; } }
        public Project Project { get { return project; } }

        public void TakeToWork()
        {
            if (status != TaskStatus.Assigned)
                throw new InvalidOperationException("Взять в работу можно только назначенную задачу");
            if (assignee == null)
                throw new InvalidOperationException("У задачи нет исполнителя");
            status = TaskStatus.InProgress;
        }

        public void Delegate(Person newAssignee)
        {
            if (status != TaskStatus.Assigned)
                throw new InvalidOperationException("Делегировать можно только назначенную задачу");
            if (newAssignee == null)
                throw new ArgumentNullException(nameof(newAssignee));
            if (newAssignee == assignee)
                throw new InvalidOperationException("Нельзя делегировать самому себе");
            assignee = newAssignee;
        }

        public void Reject()
        {
            if (status != TaskStatus.Assigned)
                throw new InvalidOperationException("Отклонить можно только назначенную задачу");
            assignee = null;
        }

        public void Reassign(Person newAssignee)
        {
            if (status != TaskStatus.Assigned)
                throw new InvalidOperationException("Переназначить можно только назначенную задачу");
            if (assignee != null)
                throw new InvalidOperationException("У задачи уже есть исполнитель");
            if (newAssignee == null)
                throw new ArgumentNullException(nameof(newAssignee));
            assignee = newAssignee;
        }

        public void SubmitReport(string text)
        {
            if (status != TaskStatus.InProgress)
                throw new InvalidOperationException("Сдать отчёт можно только по задаче в работе");
            if (assignee == null)
                throw new InvalidOperationException("У задачи нет исполнителя");
            report = new Report(text, assignee);
            status = TaskStatus.OnReview;
        }

        public void ApproveReport()
        {
            if (status != TaskStatus.OnReview)
                throw new InvalidOperationException("Утвердить отчёт можно только на проверке");
            status = TaskStatus.Done;
        }

        public void ReturnReport()
        {
            if (status != TaskStatus.OnReview)
                throw new InvalidOperationException("Вернуть отчёт можно только с проверки");
            report = null;
            status = TaskStatus.InProgress;
        }

        public override string ToString()
        {
            string assigneeStr = assignee == null ? "нет" : assignee.Name;
            return $"Задача №{number}: \"{description}\" | срок: {deadline:dd.MM.yyyy} | исполнитель: {assigneeStr} | статус: {StatusToRussian(status)}";
        }

        public static string StatusToRussian(TaskStatus s)
        {
            switch (s)
            {
                case TaskStatus.Assigned: return "Назначена";
                case TaskStatus.InProgress: return "В работе";
                case TaskStatus.OnReview: return "На проверке";
                case TaskStatus.Done: return "Выполнена";
                default: return "?";
            }
        }
    }

    class Project
    {
        private string description;
        private DateTime startDate;
        private DateTime endDate;
        private Person initiator;
        private Person teamLead;
        private List<Task> tasks;
        private ProjectStatus status;

        public Project(string description, DateTime startDate, DateTime endDate, Person initiator, Person teamLead)
        {
            this.description = description;
            this.startDate = startDate;
            this.endDate = endDate;
            this.initiator = initiator;
            this.teamLead = teamLead;
            this.tasks = new List<Task>();
            this.status = ProjectStatus.Draft;
        }

        public string Description { get { return description; } }
        public DateTime StartDate { get { return startDate; } }
        public DateTime EndDate { get { return endDate; } }
        public Person Initiator { get { return initiator; } }
        public Person TeamLead { get { return teamLead; } }
        public ProjectStatus Status { get { return status; } }
        public IReadOnlyList<Task> Tasks { get { return tasks; } }

        public Task AddTask(string description, DateTime deadline, Person assignee)
        {
            if (status != ProjectStatus.Draft)
                throw new InvalidOperationException("Задачи можно создавать только в статусе «Проект»");
            Task t = new Task(description, deadline, teamLead, assignee, this);
            tasks.Add(t);
            return t;
        }

        public void RemoveTask(Task task)
        {
            if (status != ProjectStatus.Draft)
                throw new InvalidOperationException("Удалять задачи можно только в статусе «Проект»");
            if (task.Assignee != null)
                throw new InvalidOperationException("Удалять можно только задачи без исполнителя");
            tasks.Remove(task);
        }

        public void Start()
        {
            if (status != ProjectStatus.Draft)
                throw new InvalidOperationException("Запустить можно только проект в статусе «Проект»");
            if (tasks.Count == 0)
                throw new InvalidOperationException("В проекте нет задач");
            foreach (var t in tasks)
                if (t.Assignee == null)
                    throw new InvalidOperationException($"У задачи №{t.Number} нет исполнителя");
            status = ProjectStatus.InProgress;
        }

        public bool CanClose()
        {
            if (status != ProjectStatus.InProgress) return false;
            if (tasks.Count == 0) return false;
            foreach (var t in tasks)
                if (t.Status != TaskStatus.Done) return false;
            return true;
        }

        public void Close()
        {
            if (!CanClose())
                throw new InvalidOperationException("Нельзя закрыть проект: не все задачи выполнены");
            status = ProjectStatus.Closed;
        }

        public Task FindTaskByNumber(int number)
        {
            foreach (var t in tasks)
                if (t.Number == number) return t;
            return null;
        }

        public override string ToString()
        {
            int done = 0;
            foreach (var t in tasks) if (t.Status == TaskStatus.Done) done++;
            return $"Проект \"{description}\" | статус: {StatusToRussian(status)} | задач: {tasks.Count} (выполнено {done})";
        }

        public static string StatusToRussian(ProjectStatus s)
        {
            switch (s)
            {
                case ProjectStatus.Draft: return "Проект";
                case ProjectStatus.InProgress: return "Исполнение";
                case ProjectStatus.Closed: return "Закрыт";
                default: return "?";
            }
        }
    }

    class Program
    {
        static Person ChoosePerson(List<Person> people, Person exclude)
        {
            Console.WriteLine("Выберите сотрудника:");
            for (int i = 0; i < people.Count; i++)
            {
                if (people[i] == exclude) continue;
                Console.WriteLine($"{i + 1} — {people[i].Name}");
            }
            while (true)
            {
                Console.Write("Ваш выбор: ");
                int idx;
                if (int.TryParse(Console.ReadLine(), out idx) && idx >= 1 && idx <= people.Count)
                {
                    Person p = people[idx - 1];
                    if (p != exclude) return p;
                }
                Console.WriteLine("Некорректно, повторите.");
            }
        }

        static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                int v;
                if (int.TryParse(Console.ReadLine(), out v) && v >= min && v <= max) return v;
                Console.WriteLine($"Введите число от {min} до {max}.");
            }
        }

        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            List<Person> people = new List<Person>
            {
                new Person("Тимур"),
                new Person("Рашид"),
                new Person("О Ильхам"),
                new Person("Оркадий"),
                new Person("Володя"),
                new Person("Сергей"),
                new Person("Ляйсан"),
                new Person("Марат"),
                new Person("Дина"),
                new Person("Ильдар")
            };

            Person initiator = people[0];
            Person teamLead = people[3];

            Project project = new Project(
                "Автоматизация бухгалтерии",
                DateTime.Today,
                DateTime.Today.AddDays(30),
                initiator,
                teamLead);

            string[] descriptions = new string[]
            {
                "Настроить сетевое оборудование",
                "Разработать модуль отчётности",
                "Обновить документацию",
                "Провести аудит безопасности",
                "Настроить резервное копирование",
                "Разработать API для интеграции",
                "Обновить серверное ПО",
                "Провести обучение сотрудников",
                "Оптимизировать базу данных",
                "Подготовить план автоматизации"
            };

            for (int i = 0; i < people.Count; i++)
            {
                project.AddTask(descriptions[i], DateTime.Today.AddDays(10 + i), people[i]);
            }

            Console.WriteLine("=== Проект создан ===");
            Console.WriteLine(project);
            Console.WriteLine("\nЗадачи:");
            foreach (var t in project.Tasks)
                Console.WriteLine("  " + t);

            Console.WriteLine("\nПеревод проекта в статус «Исполнение»...");
            project.Start();
            Console.WriteLine(project);

            foreach (var task in project.Tasks)
            {
                Console.WriteLine("\n==========================================");
                while (task.Status != TaskStatus.Done)
                {
                    Console.WriteLine(task);
                    if (task.Report != null)
                        Console.WriteLine("Отчёт: " + task.Report);

                    if (task.Status == TaskStatus.Assigned)
                    {
                        Console.WriteLine("Действия:");
                        Console.WriteLine("1 — Взять в работу");
                        Console.WriteLine("2 — Делегировать");
                        Console.WriteLine("3 — Отклонить");
                        int action = ReadInt("Ваш выбор: ", 1, 3);
                        if (action == 1)
                        {
                            task.TakeToWork();
                            Console.WriteLine($"{task.Assignee.Name} взял задачу в работу.");
                        }
                        else if (action == 2)
                        {
                            Person newAssignee = ChoosePerson(people, task.Assignee);
                            task.Delegate(newAssignee);
                            Console.WriteLine($"Задача делегирована {newAssignee.Name}.");
                        }
                        else
                        {
                            task.Reject();
                            Console.WriteLine("Задача отклонена. Назначьте нового исполнителя.");
                            Person newAssignee = ChoosePerson(people, null);
                            task.Reassign(newAssignee);
                            Console.WriteLine($"Задача переназначена {newAssignee.Name}.");
                        }
                    }
                    else if (task.Status == TaskStatus.InProgress)
                    {
                        Console.Write("Введите текст отчёта: ");
                        string text = Console.ReadLine();
                        task.SubmitReport(text);
                        Console.WriteLine("Отчёт сдан, задача на проверке.");
                    }
                    else if (task.Status == TaskStatus.OnReview)
                    {
                        Console.WriteLine("1 — Утвердить отчёт");
                        Console.WriteLine("2 — Вернуть на доработку");
                        int action = ReadInt("Ваш выбор: ", 1, 2);
                        if (action == 1)
                        {
                            task.ApproveReport();
                            Console.WriteLine("Отчёт утверждён. Задача выполнена.");
                        }
                        else
                        {
                            task.ReturnReport();
                            Console.WriteLine("Отчёт возвращён на доработку.");
                        }
                    }
                }
            }

            Console.WriteLine("\n==========================================");
            Console.WriteLine("Все задачи выполнены.");
            if (project.CanClose())
            {
                project.Close();
                Console.WriteLine("Проект закрыт.");
            }
            Console.WriteLine(project);
        }
    }
}
