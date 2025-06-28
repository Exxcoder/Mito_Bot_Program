using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;

    // Хранилище данных
    private static Dictionary<long, UserInfo> _userInfo = new Dictionary<long, UserInfo>();
    private static Dictionary<long, UserState> _userStates = new Dictionary<long, UserState>();
    private static Dictionary<string, Project> _projects = new Dictionary<string, Project>();
    private static Dictionary<string, string> _taskAssignments = new Dictionary<string, string>();
    private static Dictionary<string, List<long>> _projectMembers = new Dictionary<string, List<long>>();
    private static Dictionary<string, List<string>> _projectErrors = new Dictionary<string, List<string>>();
    private static Dictionary<string, List<string>> _fixedErrors = new Dictionary<string, List<string>>();
    private static Dictionary<string, List<string>> _fixedTasks = new Dictionary<string, List<string>>();

    private static readonly string DataFilePath = "botdata.json";

    enum UserStateType
    {
        None,
        AwaitingProjectName,
        AwaitingProjectPassword,
        AwaitingTask,
        SelectingProject,
        AwaitingTaskAssignment,
        SelectingAssignee,
        AwaitingNewPassword,
        ConfirmDeleteProject,
        AwaitingErrorDescription,
        AwaitingErrorToDelete,
        ViewingErrors,
        ViewingFixedErrors,
        AwaitingTaskToDelete,
        ViewingProjectMembers,
        ViewingFixedTasks
    }

    [Serializable]
    class UserInfo
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    [Serializable]
    class UserState
    {
        public UserStateType State { get; set; }
        public string TempData { get; set; }
        public string CurrentProject { get; set; }
        public string TaskToAssign { get; set; }
    }

    [Serializable]
    class Project
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public List<string> Tasks { get; set; } = new List<string>();
    }

    [Serializable]
    class BotData
    {
        public Dictionary<long, UserInfo> UserInfo { get; set; }
        public Dictionary<long, UserState> UserStates { get; set; }
        public Dictionary<string, Project> Projects { get; set; }
        public Dictionary<string, string> TaskAssignments { get; set; }
        public Dictionary<string, List<long>> ProjectMembers { get; set; }
        public Dictionary<string, List<string>> ProjectErrors { get; set; }
        public Dictionary<string, List<string>> FixedErrors { get; set; }
        public Dictionary<string, List<string>> FixedTasks { get; set; }
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine($"Data will be saved to: {Path.GetFullPath(DataFilePath)}");

        LoadData();
        _botClient = new TelegramBotClient("7696700747:AAG3M6jk89tavjVa131-CRzPZ-U-Vx-028g");
        _receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery },
            ThrowPendingUpdates = true,
        };

        using var cts = new CancellationTokenSource();
        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

        Console.WriteLine("Bot Started!");
        await Task.Delay(-1);
        cts.Cancel();
    }

    private static void SaveData()
    {
        try
        {
            var data = new BotData
            {
                UserInfo = _userInfo,
                UserStates = _userStates,
                Projects = _projects,
                TaskAssignments = _taskAssignments,
                ProjectMembers = _projectMembers,
                ProjectErrors = _projectErrors,
                FixedErrors = _fixedErrors,
                FixedTasks = _fixedTasks
            };

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            System.IO.File.WriteAllText(DataFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
        }
    }

    private static void LoadData()
    {
        try
        {
            if (!System.IO.File.Exists(DataFilePath)) return;

            string json = System.IO.File.ReadAllText(DataFilePath);
            var data = JsonConvert.DeserializeObject<BotData>(json);

            _userInfo = data.UserInfo ?? new Dictionary<long, UserInfo>();
            _userStates = data.UserStates ?? new Dictionary<long, UserState>();
            _projects = data.Projects ?? new Dictionary<string, Project>();
            _taskAssignments = data.TaskAssignments ?? new Dictionary<string, string>();
            _projectMembers = data.ProjectMembers ?? new Dictionary<string, List<long>>();
            _projectErrors = data.ProjectErrors ?? new Dictionary<string, List<string>>();
            _fixedErrors = data.FixedErrors ?? new Dictionary<string, List<string>>();
            _fixedTasks = data.FixedTasks ?? new Dictionary<string, List<string>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
        }
    }

    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    await HandleMessage(botClient, update.Message);
                    break;
                case UpdateType.CallbackQuery:
                    await HandleCallbackQuery(botClient, update.CallbackQuery);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static async Task HandleMessage(ITelegramBotClient botClient, Message message)
    {
        if (message.From == null) return;
        var chatId = message.Chat.Id;
        var userId = message.From.Id;

        // Сохраняем информацию о пользователе
        if (!_userInfo.ContainsKey(userId))
        {
            _userInfo[userId] = new UserInfo
            {
                Username = message.From.Username,
                FirstName = message.From.FirstName,
                LastName = message.From.LastName
            };
            SaveData();
        }

        if (!_userStates.TryGetValue(chatId, out var userState))
        {
            userState = new UserState();
            _userStates[chatId] = userState;
        }

        // Обработка команд /start и /cancel
        if (message.Text == "/start" || message.Text == "/cancel")
        {
            userState.State = UserStateType.None;
            userState.CurrentProject = null;
            await ShowMainMenu(botClient, chatId);
            return;
        }

        // Обработка кнопки "Назад"
        if (message.Text == "Назад")
        {
            await HandleBackCommand(botClient, chatId, userState);
            return;
        }

        // Обработка состояний
        switch (userState.State)
        {
            case UserStateType.AwaitingProjectName:
                userState.TempData = message.Text;
                userState.State = UserStateType.AwaitingProjectPassword;
                await botClient.SendTextMessageAsync(chatId, "Введите пароль для проекта:");
                return;

            case UserStateType.AwaitingProjectPassword:
                if (string.IsNullOrEmpty(userState.CurrentProject))
                {
                    if (_projects.ContainsKey(userState.TempData))
                    {
                        await botClient.SendTextMessageAsync(chatId,
                            $"Проект '{userState.TempData}' уже существует. Введите другое название.");
                        userState.State = UserStateType.AwaitingProjectName;
                        return;
                    }

                    var project = new Project
                    {
                        Name = userState.TempData,
                        Password = message.Text
                    };
                    _projects[project.Name] = project;
                    userState.CurrentProject = project.Name;
                    userState.State = UserStateType.None;

                    if (!_projectMembers.ContainsKey(project.Name))
                        _projectMembers[project.Name] = new List<long>();
                    _projectMembers[project.Name].Add(userId);

                    SaveData();
                    await botClient.SendTextMessageAsync(chatId, $"Проект '{project.Name}' создан!");
                    await ShowTasks(botClient, chatId, project.Name);
                    await ShowProjectMenu(botClient, chatId, project.Name);
                }
                else
                {
                    if (_projects.TryGetValue(userState.CurrentProject, out var existingProject))
                    {
                        if (existingProject.Password == message.Text)
                        {
                            userState.State = UserStateType.None;

                            if (!_projectMembers.ContainsKey(existingProject.Name))
                                _projectMembers[existingProject.Name] = new List<long>();
                            if (!_projectMembers[existingProject.Name].Contains(userId))
                                _projectMembers[existingProject.Name].Add(userId);

                            SaveData();
                            await botClient.SendTextMessageAsync(chatId, $"Добро пожаловать в проект '{existingProject.Name}'");
                            await ShowTasks(botClient, chatId, existingProject.Name);
                            await ShowProjectMenu(botClient, chatId, existingProject.Name);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(chatId,
                                "Неверный пароль. Попробуйте снова или отмените (/cancel)");
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Проект не найден");
                        userState.State = UserStateType.None;
                        await ShowMainMenu(botClient, chatId);
                    }
                }
                return;

            case UserStateType.SelectingProject:
                if (_projects.TryGetValue(message.Text, out var selectedProject))
                {
                    userState.CurrentProject = selectedProject.Name;
                    userState.State = UserStateType.AwaitingProjectPassword;
                    await botClient.SendTextMessageAsync(chatId, $"Введите пароль для проекта '{selectedProject.Name}':");
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Проект не найден. Попробуйте еще раз:");
                }
                return;

            case UserStateType.AwaitingTask:
                _projects[userState.CurrentProject].Tasks.Add(message.Text);
                SaveData();
                await botClient.SendTextMessageAsync(chatId, $"Задача '{message.Text}' добавлена в проект '{userState.CurrentProject}'");
                userState.State = UserStateType.None;
                await ShowTasks(botClient, chatId, userState.CurrentProject);
                await ShowProjectMenu(botClient, chatId, userState.CurrentProject);
                return;

            case UserStateType.AwaitingTaskAssignment:
                if (message.Text == "Назад")
                {
                    userState.State = UserStateType.None;
                    await ShowProjectMenu(botClient, chatId, userState.CurrentProject);
                    return;
                }

                userState.TaskToAssign = message.Text;
                userState.State = UserStateType.SelectingAssignee;
                await ShowAssigneeSelection(botClient, chatId, userState.CurrentProject);
                return;

            case UserStateType.SelectingAssignee:
                if (_projectMembers.TryGetValue(userState.CurrentProject, out var members))
                {
                    var selectedMember = members.FirstOrDefault(m =>
                        _userInfo.TryGetValue(m, out var user) && (
                            message.Text == $"@{user.Username}" ||
                            message.Text == $"{user.FirstName} {user.LastName}".Trim()
                        ));

                    if (selectedMember != 0)
                    {
                        _taskAssignments[userState.TaskToAssign] = GetUserDisplayName(selectedMember);
                        SaveData();
                        await botClient.SendTextMessageAsync(chatId,
                            $"Задача '{userState.TaskToAssign}' назначена на {GetUserDisplayName(selectedMember)}");
                        userState.State = UserStateType.None;
                        await ShowProjectMenu(botClient, chatId, userState.CurrentProject);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Пользователь не найден. Попробуйте еще раз:");
                    }
                }
                return;

            case UserStateType.AwaitingNewPassword:
                if (_projects.TryGetValue(userState.CurrentProject, out var projectToUpdate))
                {
                    projectToUpdate.Password = message.Text;
                    SaveData();
                    await botClient.SendTextMessageAsync(chatId, "Пароль успешно изменен!");
                    userState.State = UserStateType.None;
                    await ShowProjectMenu(botClient, chatId, userState.CurrentProject);
                }
                return;

            case UserStateType.ConfirmDeleteProject:
                if (message.Text.Equals("Да", StringComparison.OrdinalIgnoreCase))
                {
                    if (_projects.Remove(userState.CurrentProject))
                    {
                        _projectMembers.Remove(userState.CurrentProject);
                        _projectErrors.Remove(userState.CurrentProject);
                        _fixedErrors.Remove(userState.CurrentProject);
                        _fixedTasks.Remove(userState.CurrentProject);
                        var tasksToRemove = _taskAssignments.Keys
                            .Where(k => _projects.Values.Any(p => p.Tasks.Contains(k)))
                            .ToList();
                        foreach (var task in tasksToRemove)
                        {
                            _taskAssignments.Remove(task);
                        }
                        SaveData();
                        await botClient.SendTextMessageAsync(chatId, $"Проект '{userState.CurrentProject}' удален.");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Ошибка при удалении проекта.");
                    }
                    userState.State = UserStateType.None;
                    userState.CurrentProject = null;
                    await ShowMainMenu(botClient, chatId);
                }
                else if (message.Text.Equals("Нет", StringComparison.OrdinalIgnoreCase))
                {
                    await botClient.SendTextMessageAsync(chatId, "Удаление отменено.");
                    userState.State = UserStateType.None;
                    await ShowProjectMenu(botClient, chatId, userState.CurrentProject);
                }
                return;

            case UserStateType.AwaitingErrorDescription:
                if (!string.IsNullOrEmpty(message.Text))
                {
                    if (!_projectErrors.ContainsKey(userState.CurrentProject))
                        _projectErrors[userState.CurrentProject] = new List<string>();

                    _projectErrors[userState.CurrentProject].Add(message.Text);
                    SaveData();
                    await botClient.SendTextMessageAsync(chatId, $"Ошибка добавлена в проект '{userState.CurrentProject}'");
                    userState.State = UserStateType.None;
                    await ShowErrorsMenu(botClient, chatId, userState.CurrentProject);
                }
                return;

            case UserStateType.AwaitingErrorToDelete:
                if (_projectErrors.TryGetValue(userState.CurrentProject, out var errors))
                {
                    if (int.TryParse(message.Text, out int errorIndex) && errorIndex > 0 && errorIndex <= errors.Count)
                    {
                        var errorToRemove = errors[errorIndex - 1];
                        errors.RemoveAt(errorIndex - 1);

                        if (!_fixedErrors.ContainsKey(userState.CurrentProject))
                            _fixedErrors[userState.CurrentProject] = new List<string>();

                        _fixedErrors[userState.CurrentProject].Add(errorToRemove);
                        SaveData();

                        await botClient.SendTextMessageAsync(chatId,
                            $"Ошибка '{errorToRemove}' помечена как исправленная");
                        userState.State = UserStateType.None;
                        await ShowErrorsMenu(botClient, chatId, userState.CurrentProject);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Неверный номер ошибки. Попробуйте еще раз:");
                    }
                }
                return;

            case UserStateType.ViewingErrors:
                await ShowErrorsList(botClient, chatId, userState.CurrentProject);
                return;

            case UserStateType.ViewingFixedErrors:
                await ShowFixedErrorsList(botClient, chatId, userState.CurrentProject);
                return;

            case UserStateType.AwaitingTaskToDelete:
                if (_projects.TryGetValue(userState.CurrentProject, out var currentProject))
                {
                    if (int.TryParse(message.Text, out int taskIndex) && taskIndex > 0 && taskIndex <= currentProject.Tasks.Count)
                    {
                        var taskToRemove = currentProject.Tasks[taskIndex - 1];
                        currentProject.Tasks.RemoveAt(taskIndex - 1);
                        _taskAssignments.Remove(taskToRemove);

                        // Добавляем задачу в список исправленных
                        if (!_fixedTasks.ContainsKey(userState.CurrentProject))
                            _fixedTasks[userState.CurrentProject] = new List<string>();
                        _fixedTasks[userState.CurrentProject].Add(taskToRemove);

                        SaveData();
                        await botClient.SendTextMessageAsync(chatId, $"Задача '{taskToRemove}' помечена как исправленная");
                        userState.State = UserStateType.None;
                        await ShowProjectMenu(botClient, chatId, userState.CurrentProject);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Неверный номер задачи. Попробуйте еще раз:");
                    }
                }
                return;

            case UserStateType.ViewingProjectMembers:
                await ShowProjectMembers(botClient, chatId, userState.CurrentProject);
                return;

            case UserStateType.ViewingFixedTasks:
                await ShowFixedTasksList(botClient, chatId, userState.CurrentProject);
                return;
        }

        // Обработка команд главного меню
        switch (message.Text)
        {
            case "Создать проект":
                userState.State = UserStateType.AwaitingProjectName;
                await botClient.SendTextMessageAsync(chatId, "Введите название проекта:",
                    replyMarkup: new ReplyKeyboardRemove());
                break;

            case "Выбрать проект":
                if (_projects.Count == 0)
                {
                    await botClient.SendTextMessageAsync(chatId, "Нет доступных проектов.");
                    return;
                }

                var keyboard = new ReplyKeyboardMarkup(
                    _projects.Keys.Select(p => new KeyboardButton(p)).ToArray())
                {
                    ResizeKeyboard = true
                };

                userState.State = UserStateType.SelectingProject;
                await botClient.SendTextMessageAsync(chatId, "Выберите проект:", replyMarkup: keyboard);
                break;
        }

        // Обработка команд меню проекта
        if (userState.CurrentProject != null)
        {
            switch (message.Text)
            {
                case "Добавить задачу":
                    userState.State = UserStateType.AwaitingTask;
                    await botClient.SendTextMessageAsync(chatId, "Введите задачу:",
                        replyMarkup: new ReplyKeyboardRemove());
                    break;

                case "Просмотр задач":
                    await ShowTasks(botClient, chatId, userState.CurrentProject);
                    break;

                case "Удалить задачу":
                    await ShowTasksForDeletion(botClient, chatId, userState.CurrentProject);
                    break;

                case "Распределить задачи":
                    await ShowTaskAssignmentMenu(botClient, chatId, userState.CurrentProject);
                    break;

                case "Исправленные задачи":
                    userState.State = UserStateType.ViewingFixedTasks;
                    await ShowFixedTasksList(botClient, chatId, userState.CurrentProject);
                    break;

                case "Ошибки":
                    await ShowErrorsMenu(botClient, chatId, userState.CurrentProject);
                    break;

                case "Участники проекта":
                    userState.State = UserStateType.ViewingProjectMembers;
                    await ShowProjectMembers(botClient, chatId, userState.CurrentProject);
                    break;

                case "Изменить пароль":
                    userState.State = UserStateType.AwaitingNewPassword;
                    await botClient.SendTextMessageAsync(chatId, "Введите новый пароль для проекта:",
                        replyMarkup: new ReplyKeyboardRemove());
                    break;

                case "Удалить проект":
                    userState.State = UserStateType.ConfirmDeleteProject;
                    var confirmKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { "Да", "Нет" }
                    })
                    {
                        ResizeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(chatId,
                        $"Вы уверены, что хотите удалить проект '{userState.CurrentProject}'? (Все данные будут потеряны)",
                        replyMarkup: confirmKeyboard);
                    break;

                case "В главное меню":
                    userState.State = UserStateType.None;
                    userState.CurrentProject = null;
                    await ShowMainMenu(botClient, chatId);
                    break;
            }
        }

        // Обработка команд меню ошибок
        if (userState.CurrentProject != null &&
            (_userStates[chatId].State == UserStateType.None ||
             _userStates[chatId].State == UserStateType.ViewingErrors ||
             _userStates[chatId].State == UserStateType.ViewingFixedErrors))
        {
            switch (message.Text)
            {
                case "Добавить ошибку":
                    userState.State = UserStateType.AwaitingErrorDescription;
                    await botClient.SendTextMessageAsync(chatId, "Опишите ошибку:",
                        replyMarkup: new ReplyKeyboardRemove());
                    break;

                case "Список ошибок":
                    userState.State = UserStateType.ViewingErrors;
                    await ShowErrorsList(botClient, chatId, userState.CurrentProject);
                    break;

                case "Исправленные ошибки":
                    userState.State = UserStateType.ViewingFixedErrors;
                    await ShowFixedErrorsList(botClient, chatId, userState.CurrentProject);
                    break;

                case "Удалить ошибку":
                    await ShowErrorsForDeletion(botClient, chatId, userState.CurrentProject);
                    break;
            }
        }
    }

    private static async Task HandleBackCommand(ITelegramBotClient botClient, long chatId, UserState userState)
    {
        switch (userState.State)
        {
            case UserStateType.AwaitingErrorDescription:
            case UserStateType.AwaitingErrorToDelete:
            case UserStateType.ViewingErrors:
            case UserStateType.ViewingFixedErrors:
                userState.State = UserStateType.None;
                await ShowErrorsMenu(botClient, chatId, userState.CurrentProject);
                break;

            case UserStateType.SelectingAssignee:
                userState.State = UserStateType.AwaitingTaskAssignment;
                await ShowTaskAssignmentMenu(botClient, chatId, userState.CurrentProject);
                break;

            case UserStateType.AwaitingTaskAssignment:
                userState.State = UserStateType.None;
                await ShowProjectMenu(botClient, chatId, userState.CurrentProject);
                break;

            case UserStateType.AwaitingTaskToDelete:
            case UserStateType.ViewingProjectMembers:
            case UserStateType.ViewingFixedTasks:
                userState.State = UserStateType.None;
                await ShowProjectMenu(botClient, chatId, userState.CurrentProject);
                break;

            default:
                userState.State = UserStateType.None;
                await ShowMainMenu(botClient, chatId);
                break;
        }
    }

    private static string GetUserDisplayName(long userId)
    {
        if (_userInfo.TryGetValue(userId, out var user))
        {
            if (!string.IsNullOrEmpty(user.Username))
                return $"@{user.Username}";
            return $"{user.FirstName} {user.LastName}".Trim();
        }
        return userId.ToString();
    }

    private static async Task ShowMainMenu(ITelegramBotClient botClient, long chatId)
    {
        var replyMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Создать проект", "Выбрать проект" }
        })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Главное меню:",
            replyMarkup: replyMarkup);
    }

    private static async Task ShowProjectMenu(ITelegramBotClient botClient, long chatId, string projectName)
    {
        var replyMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Добавить задачу", "Просмотр задач" },
            new KeyboardButton[] { "Удалить задачу", "Распределить задачи" },
            new KeyboardButton[] { "Исправленные задачи", "Ошибки" },
            new KeyboardButton[] { "Участники проекта", "Изменить пароль" },
            new KeyboardButton[] { "Удалить проект", "В главное меню" }
        })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Меню проекта '{projectName}':",
            replyMarkup: replyMarkup);
    }

    private static async Task ShowTasks(ITelegramBotClient botClient, long chatId, string projectName)
    {
        var project = _projects[projectName];
        if (project.Tasks.Count == 0)
        {
            await botClient.SendTextMessageAsync(chatId, "В проекте пока нет задач.");
            return;
        }

        var tasksList = $"Задачи проекта '{projectName}':\n\n" +
                        string.Join("\n", project.Tasks.Select((t, i) =>
                        {
                            var assignment = _taskAssignments.ContainsKey(t) ? $" (исполнитель: {_taskAssignments[t]})" : "";
                            return $"{i + 1}. {t}{assignment}";
                        }));

        await botClient.SendTextMessageAsync(chatId, tasksList);
    }

    private static async Task ShowTasksForDeletion(ITelegramBotClient botClient, long chatId, string projectName)
    {
        var project = _projects[projectName];
        if (project.Tasks.Count == 0)
        {
            await botClient.SendTextMessageAsync(chatId, "Нет задач для удаления.");
            return;
        }

        var tasksList = $"Выберите номер задачи для пометки как исправленной:\n\n" +
                        string.Join("\n", project.Tasks.Select((t, i) => $"{i + 1}. {t}"));

        var keyboard = new ReplyKeyboardMarkup(
            Enumerable.Range(1, project.Tasks.Count)
                .Select(i => new KeyboardButton(i.ToString()))
                .Concat(new[] { new KeyboardButton("Назад") })
                .ToArray())
        {
            ResizeKeyboard = true
        };

        _userStates[chatId].State = UserStateType.AwaitingTaskToDelete;
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: tasksList,
            replyMarkup: keyboard);
    }

    private static async Task ShowTaskAssignmentMenu(ITelegramBotClient botClient, long chatId, string projectName)
    {
        var project = _projects[projectName];
        if (project.Tasks.Count == 0)
        {
            await botClient.SendTextMessageAsync(chatId, "Нет задач для распределения.");
            return;
        }

        var keyboard = new ReplyKeyboardMarkup(
            project.Tasks.Select(t => new KeyboardButton(t)).Concat(new[] { new KeyboardButton("Назад") }).ToArray())
        {
            ResizeKeyboard = true
        };

        _userStates[chatId].State = UserStateType.AwaitingTaskAssignment;
        await botClient.SendTextMessageAsync(chatId, "Выберите задачу для назначения исполнителя:", replyMarkup: keyboard);
    }

    private static async Task ShowAssigneeSelection(ITelegramBotClient botClient, long chatId, string projectName)
    {
        if (!_projectMembers.TryGetValue(projectName, out var members) || members.Count == 0)
        {
            await botClient.SendTextMessageAsync(chatId, "В проекте нет участников для назначения задачи.");
            _userStates[chatId].State = UserStateType.None;
            await ShowProjectMenu(botClient, chatId, projectName);
            return;
        }

        var memberButtons = members.Select(m =>
        {
            var name = GetUserDisplayName(m);
            return new KeyboardButton(name);
        }).ToList();

        memberButtons.Add(new KeyboardButton("Назад"));

        var keyboard = new ReplyKeyboardMarkup(memberButtons)
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(chatId, "Выберите исполнителя:", replyMarkup: keyboard);
    }

    private static async Task ShowProjectMembers(ITelegramBotClient botClient, long chatId, string projectName)
    {
        if (!_projectMembers.TryGetValue(projectName, out var members) || members.Count == 0)
        {
            await botClient.SendTextMessageAsync(chatId, "В проекте пока нет участников.");
            return;
        }

        var membersList = $"Участники проекта '{projectName}':\n\n" +
                         string.Join("\n", members.Select(m => GetUserDisplayName(m)));

        var keyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Назад") })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: membersList,
            replyMarkup: keyboard);
    }

    private static async Task ShowFixedTasksList(ITelegramBotClient botClient, long chatId, string projectName)
    {
        if (!_fixedTasks.TryGetValue(projectName, out var fixedTasks) || fixedTasks.Count == 0)
        {
            await botClient.SendTextMessageAsync(chatId, "Нет исправленных задач.");
            return;
        }

        var tasksList = $"Исправленные задачи проекта '{projectName}':\n\n" +
                       string.Join("\n", fixedTasks.Select((t, i) => $"{i + 1}. {t}"));

        var keyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Назад") })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: tasksList,
            replyMarkup: keyboard);
    }

    private static async Task ShowErrorsMenu(ITelegramBotClient botClient, long chatId, string projectName)
    {
        var replyMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Добавить ошибку", "Список ошибок" },
            new KeyboardButton[] { "Удалить ошибку", "Исправленные ошибки" },
            new KeyboardButton[] { "Назад" }
        })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"Меню ошибок проекта '{projectName}':",
            replyMarkup: replyMarkup);
    }

    private static async Task ShowErrorsList(ITelegramBotClient botClient, long chatId, string projectName)
    {
        if (!_projectErrors.TryGetValue(projectName, out var errors) || errors.Count == 0)
        {
            await botClient.SendTextMessageAsync(chatId, "В проекте пока нет ошибок.");
            return;
        }

        var errorsList = $"Ошибки проекта '{projectName}':\n\n" +
                        string.Join("\n", errors.Select((e, i) => $"{i + 1}. {e}"));

        var keyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Назад") })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: errorsList,
            replyMarkup: keyboard);
    }

    private static async Task ShowFixedErrorsList(ITelegramBotClient botClient, long chatId, string projectName)
    {
        if (!_fixedErrors.TryGetValue(projectName, out var fixedErrors) || fixedErrors.Count == 0)
        {
            await botClient.SendTextMessageAsync(chatId, "Нет исправленных ошибок.");
            return;
        }

        var errorsList = $"Исправленные ошибки проекта '{projectName}':\n\n" +
                        string.Join("\n", fixedErrors.Select((e, i) => $"{i + 1}. {e}"));

        var keyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Назад") })
        {
            ResizeKeyboard = true
        };

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: errorsList,
            replyMarkup: keyboard);
    }

    private static async Task ShowErrorsForDeletion(ITelegramBotClient botClient, long chatId, string projectName)
    {
        if (!_projectErrors.TryGetValue(projectName, out var errors) || errors.Count == 0)
        {
            await botClient.SendTextMessageAsync(chatId, "Нет ошибок для удаления.");
            return;
        }

        var errorsList = $"Выберите номер ошибки для пометки как исправленной:\n\n" +
                        string.Join("\n", errors.Select((e, i) => $"{i + 1}. {e}"));

        var keyboard = new ReplyKeyboardMarkup(
            Enumerable.Range(1, errors.Count)
                .Select(i => new KeyboardButton(i.ToString()))
                .Concat(new[] { new KeyboardButton("Назад") })
                .ToArray())
        {
            ResizeKeyboard = true
        };

        _userStates[chatId].State = UserStateType.AwaitingErrorToDelete;
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: errorsList,
            replyMarkup: keyboard);
    }

    private static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Received {callbackQuery.Data}");
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        Console.WriteLine(error is ApiRequestException apiRequestException
            ? $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}"
            : error.ToString());

        return Task.CompletedTask;
    }
}