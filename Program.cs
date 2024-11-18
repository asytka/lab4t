using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Text.Json;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Lab4TelBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var botToken = builder.Configuration["Telegram:BotToken"];
            var webhookUrl = builder.Configuration["Telegram:WebhookUrl"];

            // Register TelegramBotClient as a singleton
            builder.Services.AddSingleton<ITelegramBotClient>(sp =>
                new TelegramBotClient(botToken));

            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the webhook before starting the server
            ConfigureWebhook(app.Services, webhookUrl);

            app.UseRouting();
            app.UseAuthorization();

            // Map the webhook endpoint
            app.MapPost("/api/webhook", async (HttpContext context, ITelegramBotClient botClient, CancellationToken cancellationToken, ILogger<Program> logger) =>
            {
                try
                {
                    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();

                    logger.LogInformation($"Raw Update Received: {requestBody}");

                    var options1 = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var update = System.Text.Json.JsonSerializer.Deserialize<Update>(requestBody, Telegram.Bot.JsonBotAPI.Options);
                    //var result = JsonConvert.DeserializeObject<Update>(requestBody);
                    if (update == null)
                    {
                        logger.LogWarning("Failed to deserialize the update");
                        return Results.BadRequest();
                    }

                    logger.LogInformation($"Deserialized Update: {update}");
                    logger.LogInformation($"Message: {update.Message?.Text}");
                    logger.LogInformation($"Message Type: {update.Message?.Type}");
                    logger.LogInformation($"Update Type: {update.Type}");

                    if (update.Type == UpdateType.Message && update.Message?.Text != null)
                    {
                        var message = update.Message;
                        logger.LogInformation($"Message from {message.From.Username}: {message.Text}");

                        if (message.Text == "/start")
                        {
                            // Send inline buttons for starting the bot
                            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                    new[] { InlineKeyboardButton.WithCallbackData("Керівництво кафедри", "governance"), InlineKeyboardButton.WithCallbackData("Історія кафедри", "history") },
                    new[] {InlineKeyboardButton.WithCallbackData("Наукова діяльність", "science"), InlineKeyboardButton.WithCallbackData("Партнери кафедри", "partners") },
            });

                            await botClient.SendMessage(
                                chatId: message.Chat.Id,
                                text: "Вітаю! Що саме Ви хочете дізнатись про кафедру прикладної математики?",
                                replyMarkup: inlineKeyboard,
                                cancellationToken: cancellationToken
                            );
                        }
                        else
                        {
                            await botClient.SendMessage(
                                chatId: message.Chat.Id,
                                text: "🤖Вибачте, я не знаю такою команди👀\n\n" +
                                "🤓Для початку роботи введіть команду /start",
                                cancellationToken: cancellationToken
                            );
                        }
                    }

                    // Handle callback queries (button clicks)
                    else 
                    {
                        var callbackQuery = update.CallbackQuery;
                        logger.LogInformation($"Received callback: {callbackQuery.Data}");

                        if (callbackQuery.Data == "governance")
                        {
                            await botClient.SendMessage(
                                chatId: callbackQuery.Message.Chat.Id,
                                text: "🔬Завідувач кафедри: д.ф-м.н, проф. Маркович Богдан Михайлович\n\n" +
                                "🔬Заступник завідувача кафедри: к. ф-м. н, доц. Пізюр Ярополк Володимирович\n\n" +
                                "🔬Заступник завідувача кафедри з наукової роботи та міжнародної співпраці: д.т.н., проф. Бунь Ростислав Адамович\n\n" +
                                "🔬Заступник завідувача кафедри з навчально-методичної роботи: к.т.н. доц. Рижа Ірина Андріївна",
                                cancellationToken: cancellationToken
                            );
                        }
                        else if (callbackQuery.Data == "history")
                        {
                            await botClient.SendMessage(
                                chatId: callbackQuery.Message.Chat.Id,
                                text: "🌱Кафедру прикладної математики створено рішенням Вченої ради Львівського політехнічного інституту у вересні 1971 року.\n\n" +
                                "💡З 2001 року кафедрою завідував заслужений працівник освіти України, доктор фізико — математичних наук, професор Костробій Петро Петрович.\n\n" +
                                "📚З 2022 року завідувачем кафедри став доктор фізико-математичних наук, професор Маркович Богдан Михайлович.\n\n" +
                                "📚Освітньо — професійні програми підготовки фахівців в області прикладної математики та інформатики забезпечують вміння формалізувати задачу та розробити її математичну модель," +
                                "вибрати або розробити числовий метод розв’язання, оцінити його ефективність(збіжність, точність, стійкість)," +
                                "побудувати алгоритми, реалізувати їх у вигляді прикладного програмного забезпечення на ПЕОМ або в комп’ютерній мережі," +
                                "налагодити програмний комплекс для розв’язування поставленої реальної задачі. ",
                                cancellationToken: cancellationToken
                            );
                        }

                        else if (callbackQuery.Data == "science")
                        {
                            await botClient.SendMessage(
                                chatId: callbackQuery.Message.Chat.Id,
                                text: "👩🏻‍🔬Наукова діяльність кафедри має декілька напрямів в рамках проекту з розробки математичних моделей і методів їх чисельної реалізації для опису природничих і суспільних явищ:\n\n" +
                                "⚛️Розробка ефективних чисельних методів розв’язування задач Коші, крайових задач та задач на власні числа для звичайних диференціальних рівнянь (державний реєстраційний номер 0107U009513);\n\n" +
                                "🥼Дослідження сучасних проблем аналізу, диференціальних рівнянь та теорії ймовірності (державний реєстраційний номер 0107U009514);" +
                                "(науковий керівник теми — Чабанюк Я.М. д. фіз.-мат. наук, професор; виконавці: Пташник Б. Й., чл.-кор. НАНУ," +
                                "доктор фіз.-мат. наук, професор; Гладун В. Р., канд. фіз.-мат. наук, доцент; Мединський І. П.," +
                                "канд. фіз.-мат. наук, доцент; Ружевич Н. А., канд. фіз.-мат. наук, доцент; Репетило С. М., аспірант).\n\n" +
                                "🧠Дослідження математичних моделей конкретних типів систем (державний реєстраційний номер 0107U009516); (науковий керівник — Костробій П. П., докт. фіз.-мат. наук, професор",
                                cancellationToken: cancellationToken
                            );
                        }
                        else if (callbackQuery.Data == "partners")
                        {
                            InlineKeyboardMarkup inlineKeyboardPartners = new InlineKeyboardMarkup(new[]
                            {
                new[] {InlineKeyboardButton.WithCallbackData("Міжнародний інститут прикладного системного аналізу", "psa") },
                new[] {InlineKeyboardButton.WithCallbackData("Інститут системного аналізу Польської академії наук", "pan") },
                new[] {InlineKeyboardButton.WithCallbackData("Інститут прикладних проблем механіки і математики ім. Я. С. Підстригача", "ysp") },
                new[] {InlineKeyboardButton.WithCallbackData("SoftServe", "softserve"), InlineKeyboardButton.WithCallbackData("Кредобанк", "credobank")}
            });

                            await botClient.SendMessage(
                                chatId: callbackQuery.Message.Chat.Id,
                                text: "📝Виберіть про кого з партнерів ви б хотіли дізнатись:",
                                replyMarkup: inlineKeyboardPartners,
                                cancellationToken: cancellationToken
                            );
                        }

                        else if (callbackQuery.Data == "psa")
                        {
                            await botClient.SendMessage(
                                chatId: callbackQuery.Message.Chat.Id,
                                text: "🔍Ось програми у Міжнародному інституті прикладного системного аналізу:\n\n" +
                                "📝«Методи просторової інвентаризації емісій парникових газів Кіотського протоколу з врахуванням їх невизначеностей»" +
                                " «Регіональний просторовий кадастр емісій парникових газів з врахуванням невизначеностей вхідних даних»",
                                cancellationToken: cancellationToken
                            );
                        }
                        else if (callbackQuery.Data == "pan")
                        {
                            await botClient.SendMessage(
                                chatId: callbackQuery.Message.Chat.Id,
                                text: "🔍Ось програми у Інституті системного аналізу Польської академії наук:\n\n" +
                                "📝«Геоінформаційні технології, просторово-часові підходи та оцінювання повного вуглецевого балансу для підвищення точності інвентаризацій парникових газів»",
                                cancellationToken: cancellationToken
                            );
                        }
                        else if (callbackQuery.Data == "ysp")
                        {
                            await botClient.SendMessage(
                                chatId: callbackQuery.Message.Chat.Id,
                                text: "🔍Ось програми у Інститут прикладних проблем механіки і математики ім. Я. С. Підстригача:\n\n" +
                                "📝«Моделювання та розроблення методів розрахунку напружено-деформованого стану структурно-неоднорідних тіл за дії теплових та силових чинників»",
                                cancellationToken: cancellationToken
                            );
                        }
                        else if (callbackQuery.Data == "softserve")
                        {
                            await botClient.SendMessage(
                                chatId: callbackQuery.Message.Chat.Id,
                                text: "🔍Ось коротко про співпрацю з компанією SoftServe:\n\n" +
                                "🎓Студенти, що навчаються на освітній програмі «Прикладна математика та інформатика», поєднують фундаментальну освіту з реальним досвідом компанії." +
                                "Також студенти мають змогу працювати з реальними проектами, робота над якими буде зарахована їм як кредити навчання в Університеті.\r\n\r\n" +
                                "🔎Програма долучає студентів до ІТ-спільноти з перших днів в університеті. Вони навчаються ефективно працювати в команді під наставництвом менторів від SoftServe," +
                                "отримують тільки актуальні знання, а також зможуть додати до майбутнього портфоліо демо версію проєкту, як результат здобутої практики.",
                                cancellationToken: cancellationToken
                            );
                        }
                        else if (callbackQuery.Data == "credobank")
                        {
                            await botClient.SendMessage(
                                chatId: callbackQuery.Message.Chat.Id,
                                text: "🔍Ось коротко про співпрацю з компанією CredoBank:\n\n" +
                                "💡Кафедра прикладної математики розширює коло партнерів: у квітні 2024р. відбулася стратегічна зустріч між представниками правління Кредобанку," +
                                "зокрема, з заступником голови правління Адамом Свірським, та директором Інституту прикладної математики та фундаментальних наук Петром Пукачем," +
                                "завідувачем кафедри прикладної математики Богданом Марковичем та гарантом освітньої програми Оленою Гайдучок. \r\n\r\n" +
                                "🧠Також відбулася зустріч представників правління банку, директорів відділень із студентами 1-3 курсів освітньої програми" +
                                "\"Фінансовий інжиніринг\". Студентів ознайомлено із основами  банківської системи, специфікою роботи  відділень фінансового" +
                                "моніторингу, моделювання ризиків, IT відділу, підтримки, кредитування фізичних та юридичних осіб, та інших.\r\n\r\n" +
                                "🌐На зустрічі обговорено перспективи та подальші кроки майбутньої співпраці. Презентовано попередні домовленості" +
                                "щодо подальших дій з обох сторін для практичної реалізації спільних проєктів.",
                                cancellationToken: cancellationToken
                            );
                        }

                    }

                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing webhook: {ex.Message}");
                    return Results.StatusCode(500);
                }
            });

            app.Run();
        }

        // Configure Webhook
        private static async void ConfigureWebhook(IServiceProvider services, string webhookUrl)
        {
            var botClient = services.GetRequiredService<ITelegramBotClient>();
            await botClient.SetWebhook(webhookUrl);
            Console.WriteLine($"Webhook is set to: {webhookUrl}");
        }
    }
}
