using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Program
{
    private static ITelegramBotClient _botClient; //Клиент бота
    
    private static ReceiverOptions _receiverOptions; //Настройки сервера, по которому будет работать бот
    
    static async Task Main()
    {
        _botClient = new TelegramBotClient("7696700747:AAG3M6jk89tavjVa131-CRzPZ-U-Vx-028g");
        _receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message,
            },
        };
        
        using var cts = new CancellationTokenSource();
        
        _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, _receiverOptions, cts.Token); //Запуск бота
        Console.WriteLine("Bot Started!");
        
        await Task.Delay(-1); //Бот работает пока не закроется консоль
    }
    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Обработка входящих сообщений
    }
    
    private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // Обработка ошибок
    }
}