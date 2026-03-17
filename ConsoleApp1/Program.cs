using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Game
{
    static Dictionary<string, string> messages; // словарь для сообщений

    /// <summary>Вывод текста в консоль</summary>
    static void Print(string message = "")
    {
        Console.WriteLine(message);
    }

    /// <summary>Ввод текста в консоль</summary>
    static string ReadUserInput()
    {
        string? input = Console.ReadLine();
        if (input == null)
            return "";                       
        return input.Trim();
    }

    static void Main()
    {
        messages = ChooseLanguage();           // выбор языка и получение сообщений
        int timer = GetTimerFromUser();        // получение времени на ход
        Print(messages["welcome"]);

        string firstWord = GetFirstWord();     // ввод начального слова
        List<string> usedWords = new List<string>();

        RunGame(firstWord, timer, usedWords);  // запуск игрового цикла

        Print(messages["gameOver"]);
    }

    /// <summary>Основной игровой цикл.</summary>
    /// <param name="firstWord">Базовое слово.</param>
    /// <param name="timer">Время на ход в секундах.</param>
    /// <param name="usedWords">Список использованных слов.</param>
    static void RunGame(string firstWord, int timer, List<string> usedWords)
    {
        int player = 1;

        while (true)
        {
            DisplayPlayerTurn(player);
            string input = GetWordWithTimer(timer);

            if (input == null)
            {
                // Время вышло – текущий игрок проиграл
                DisplayTimeUp(player);
                DisplayWinner(player == 1 ? 2 : 1);
                break;
            }

            if (!IsValidWord(input, firstWord, usedWords))
            {
                DisplayInvalidWord();
                DisplayWinner(player == 1 ? 2 : 1);
                break;
            }

            usedWords.Add(input);
            DisplayUsedWords(usedWords);
            player = player == 1 ? 2 : 1;
        }
    }

    /// <summary>Выбор языка и инициализация словаря сообщений.</summary>
    /// <returns>Словарь с локализованными сообщениями.</returns>
    static Dictionary<string, string> ChooseLanguage()
    {
        while (true)
        {
            Print("Choose language / Выберите язык:");
            Print("1 - English");
            Print("2 - Русский");
            string choice = ReadUserInput();

            if (choice == "1")
                return CreateEnglishMessages();
            if (choice == "2")
                return CreateRussianMessages();

            Print("Invalid choice. Please enter 1 or 2. / Неверный выбор. Пожалуйста, введите 1 или 2.");
        }
    }

    /// <summary>Создаёт словарь сообщений на английском.</summary>
    static Dictionary<string, string> CreateEnglishMessages()
    {
        return new Dictionary<string, string>
        {
            {"welcome", "Words Game"},
            {"enterBase", "Enter first word (8-30 letters):"},
            {"errorBase", "Error! Enter a word 8-30 letters long, letters only."},
            {"playerTurn", "Player {0}'s turn"},
            {"enterWord", "Enter a word:"},
            {"timeUp", "Player {0} didn't enter a word!"},
            {"playerWins", "Player {0} wins!"},
            {"invalidWord", "Invalid word!"},
            {"usedWords", "Used words:"},
            {"secondsChoice", "Seconds per move:"},
            {"gameOver", "Game over."},
            {"invalidSeconds", "Invalid input. Please enter a positive integer."}
        };
    }

    /// <summary>Создаёт словарь сообщений на русском.</summary>
    static Dictionary<string, string> CreateRussianMessages()
    {
        return new Dictionary<string, string>
        {
            {"welcome", "Игра \"Слова\""},
            {"enterBase", "Введите начальное слово (8-30 букв):"},
            {"errorBase", "Ошибка! Введите слово от 8 до 30 букв, только буквы."},
            {"playerTurn", "Ход игрока {0}"},
            {"enterWord", "Введите слово:"},
            {"timeUp", "Игрок {0} не успел!"},
            {"playerWins", "Игрок {0} победил!"},
            {"invalidWord", "Неверное слово!"},
            {"usedWords", "Использованные слова:"},
            {"secondsChoice", "Секунд на ход"},
            {"gameOver", "Игра окончена."},
            {"invalidSeconds", "Неверный ввод. Пожалуйста, введите положительное целое число."}
        };
    }

    /// <summary>Запрашивает у пользователя время на ход (положительное целое число).</summary>
    /// <returns>Количество секунд.</returns>
    static int GetTimerFromUser()
    {
        while (true)
        {
            Print(messages["secondsChoice"]);
            string seconds = ReadUserInput();
            if (int.TryParse(seconds, out int timer) && timer > 0)
                return timer;
            Print(messages["invalidSeconds"]);
        }
    }

    /// <summary>Запрашивает начальное слово, проверяя длину и состав.</summary>
    /// <returns>Корректное начальное слово в нижнем регистре.</returns>
    static string GetFirstWord()
    {
        while (true)
        {
            Print(messages["enterBase"]);
            string word = ReadUserInput();
            word = word.Trim().ToLower();

            if (word.Length >= 8 && word.Length <= 30 && word.All(char.IsLetter))
                return word;

            Print(messages["errorBase"]);
        }
    }

    /// <summary>Отображает сообщение о текущем ходе игрока.</summary>
    /// <param name="player">Номер игрока (1 или 2).</param>
    static void DisplayPlayerTurn(int player)
    {
        Print();
        Print(string.Format(messages["playerTurn"], player));
        Print(messages["enterWord"]);
    }

    /// <summary>Читает строку из консоли с ограничением по времени.</summary>
    /// <param name="seconds">Доступное время в секундах.</param>
    /// <returns>Введённое слово в нижнем регистре или null, если время истекло.</returns>
    static string GetWordWithTimer(int seconds)
    {
        var tcs = new TaskCompletionSource<bool>();
        using (var timer = new System.Threading.Timer(_ => tcs.TrySetResult(true), null, seconds * 1000, Timeout.Infinite))
        {
            Task<string> readTask = Task.Run(() => ReadUserInput());
            int completedIndex = Task.WaitAny(readTask, tcs.Task);
            return completedIndex == 0 ? readTask.Result?.Trim().ToLower() : null;
        }
    }

    /// <summary>Отображает сообщение об истечении времени у игрока.</summary>
    /// <param name="player">Номер игрока, не успевшего ввести слово.</param>
    static void DisplayTimeUp(int player)
    {
        Print(string.Format(messages["timeUp"], player));
    }

    /// <summary>Отображает сообщение о победе игрока.</summary>
    /// <param name="player">Номер победившего игрока.</param>
    static void DisplayWinner(int player)
    {
        Print(string.Format(messages["playerWins"], player));
    }

    /// <summary>Отображает сообщение о неверном слове.</summary>
    static void DisplayInvalidWord()
    {
        Print(messages["invalidWord"]);
    }

    /// <summary>Отображает список использованных слов.</summary>
    /// <param name="usedWords">Список использованных слов.</param>
    static void DisplayUsedWords(List<string> usedWords)
    {
        Print(messages["usedWords"]);
        foreach (string word in usedWords)
            Print(word);
    }

    /// <summary>Проверяет корректность введённого слова.</summary>
    /// <param name="word">Проверяемое слово.</param>
    /// <param name="baseWord">Базовое слово, из букв которого составляется новое.</param>
    /// <param name="usedWords">Список уже использованных слов.</param>
    /// <returns>true, если слово допустимо; иначе false.</returns>
    static bool IsValidWord(string word, string baseWord, List<string> usedWords)
    {
        word = word?.Trim().ToLower() ?? "";
        if (string.IsNullOrWhiteSpace(word)) return false;
        if (usedWords.Contains(word)) return false;

        var letters = baseWord.ToLower().ToList();
        foreach (char c in word)
        {
            if (!letters.Contains(c)) return false;
            letters.Remove(c);
        }
        return true;
    }
}