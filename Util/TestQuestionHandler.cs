using Dapper;
using MarusyaVezdekod.Models;
using Npgsql;

namespace MarusyaVezdekod.Util;

public class TestQuestionHandler
{
    public static async Task<ResponseResponse> HandleTestResponse(SessionTestProgress? progress,
        NpgsqlConnection connection, RequestModel requestModel)
    {
        var buttons = new[]
            {new Button {title = "да"}, new Button {title = "нет"}};
        var text = new List<string>();
        var tts = "";
        var firstTime = false;
        var cmd = requestModel.request.command;
        if (progress == null)
        {
            progress = new SessionTestProgress
            {
                question = 1,
                results = 0,
                session = requestModel.session.session_id
            };
            await connection.ExecuteAsync(
                "insert into test_progress(session, question, results) values (@session, @question, @results)",
                new {@session = progress.session, @question = progress.question, @results = progress.results});
            text.Add("Так точно! Начнём же тест на самую подходящую категорию Вездекода!");
            text.Add("Я задам 8 вопросов, которые раскроют ваше истинное предназначение");
            text.Add("Чтобы закончить заранее, просто скажите `закончить`");
            tts = "Так точно! Начнём же тест на самую подходящую категорию Вездекода!\n" +
                  "Я задам 8 вопросов, которые раскроют ваше истинное предназначение\n" +
                  "Чтобы закончить заранее, просто скажите ^закончить^.";
            firstTime = true;
        }


        var res = 0; //-1 - failure, 2 - end, 0,1 - false/true

        if (!firstTime)
        {
            // Проверка ответа
            res = progress.question switch
            {
                1 => Verify(cmd, "нет", "да"),
                2 => Verify(cmd, "не люблю", "люблю"),
                3 => Verify(cmd, "нет", "да"),
                4 => Verify(cmd, "нет", "да"),
                5 => Verify(cmd, "нет", "да"),
                6 => Verify(cmd, "не люблю", "люблю"),
                7 => Verify(cmd, "не нравится", "нравится"),
                8 => Verify(cmd, "не хочу", "хочу"),
                _ => res
            };
            if (res == -1)
            {
                text.Add("Извините, я вас не понимаю. Используйте одно из слов на кнопке, или скажите `Закончить`");
                tts = "Извините, я вас не понимаю. Используйте одно из слов на кнопке, или скажите `Закончить`";
            }
            else if (res != 2)
            {
                progress.results |= res << (progress.question - 1);
                progress.question++;
                await connection.ExecuteAsync(
                    "update test_progress set question = @question, results = @results where session=@session", new
                    {
                        @results = progress.results, @session = progress.session, @question = progress.question
                    });
            }
        }

        var cardId = 0;
        
        if (progress.question == 9 || res == 2)
        {
            return await CalculateResult(progress, connection, requestModel);
        }
        else
        {
            // Добавление вопроса
            switch (progress.question)
            {
                case 1:
                    text.Add("Первый вопрос: Любите Microsoft?");
                    tts += "Первый вопрос: Любите Microsoft?";
                    cardId = 457239017;
                    break;
                case 2:
                    text.Add("Второй вопрос! Любите яблоки?");
                    buttons = new[] {new Button {title = "Люблю"}, new Button {title = "Не люблю"}};
                    tts += "^Второй^ вопрос! `Любите яблоки?";
                    cardId = 457239019;
                    break;
                case 3:
                    text.Add("Третий вопрос! Вам больше по душе компактная разработка?");
                    tts += "Третий вопрос! Вам больше по душе компактная разработка?";
                    cardId = 457239020;
                    break;
                case 4:
                    text.Add("Четвёртый вопрос! Любите возиться с цифрами?");
                    tts += "Четвёртый вопрос! Любите возиться с цифрами?";
                    cardId = 457239021;
                    break;
                case 5:
                    text.Add("Пятый вопрос! У вас богатое воображение?");
                    tts = "Пятый вопрос! У вас богатое воображение?";
                    cardId = 457239022;
                    break;
                case 6:
                    text.Add("Шестой вопрос! Как относитесь к роботам?");
                    buttons = new[] {new Button {title = "Люблю"}, new Button {title = "Не люблю"}};
                    tts += "Шестой вопрос! Как относитесь к роботам?";
                    cardId = 457239024;
                    break;
                case 7:
                    text.Add("Седьмой вопрос! Нравится оптимизировать?");
                    buttons = new[] {new Button {title = "Нравится"}, new Button {title = "Не нравится"}};
                    tts += "Седьмой вопрос! Нравится оптимизировать?";
                    cardId = 457239023;
                    break;
                case 8:
                    text.Add("Финальный вопрос! Хотите ли вы наводить красоту?");
                    buttons = new[] {new Button {title = "Хочу"}, new Button {title = "Не хочу"}};
                    tts += "Финальный вопрос! Хотите ли вы наводить красоту?";
                    cardId = 457239025;
                    break;
            }
        }


        return new ResponseResponse
        {
            text = text.ToArray(),
            buttons = buttons,
            tts = tts,
            card = new CardCommon
            {
                type = "BigImage",
                image_id = cardId
            }
        };
    }

    private static int Verify(string target, string first, string second)
    {
        if (target.Contains(first))
        {
            return 0;
        }

        if (target.Contains(second))
        {
            return 1;
        }

        if (target == "on_interrupt")
        {
            return 2;
        }

        return -1;
    }

    private static async Task<ResponseResponse> CalculateResult(SessionTestProgress progress,
        NpgsqlConnection connection, RequestModel requestModel)
    {
        await connection.ExecuteAsync("delete from test_progress where session=@session",
            new {@session = progress.session});
        if (progress.question != 9)
        {
            return new ResponseResponse
            {
                end_session = true, text = new[] {"Хорошо, заканчиваем. Возвращайтесь ещё!"},
                tts = "Хорошо, заканчиваем. Возвращайтесь ещё!"
            };
        }
        else
        {
            var text = new List<string>
            {
                "Время вынести решение...",
                "Технология, подходящая вам больше всего, это... <speaker audio_vk_id=-2000512006_456239023>",
            };
            var tts = "Время вынести решение...\nТехнология, подходящая вам больше всего, это... <speaker audio_vk_id=-2000512006_456239023>\n";
            if ((progress.results & 1) == 1)
            {
                text.Add("WEB! Там необходимо писать на C#, созданном компанией Microsoft!");
                tts += "^WEB^! Там необходимо писать на C#, созданном компанией Microsoft!";
            }
            else if ((progress.results & 2) == 1)
            {
                text.Add("Mobile! Там можно писать под устройства, созданные компанией Apple!");
                tts += "^Mobile^! Там можно писать под устройства, созданные компанией Apple!";
            }
            else if ((progress.results & 4) == 1)
            {
                text.Add("Mobile! Там можно писать под компактные устройства!");
                tts += "^Mobile^! Там можно писать под компактные устройства!";
            }
            else if ((progress.results & 8) == 1)
            {
                text.Add("Data Analysis! Парсинг, много данных и цифр - вот ваша стихия!");
                tts = "^Data Analysis^! Парсинг, много данных и цифр - вот ваша стихия!";
            }
            else if ((progress.results & 16) == 1)
            {
                text.Add("Геймдев - вот где вы сможете раскрыть потенциал вашего воображения!");
                tts = "^Геймдев^ - вот где вы сможете раскрыть потенциал вашего воображения!";
            }
            else if ((progress.results & 32) == 1)
            {
                text.Add("Computer Vision - категория, где вы сможете программировать роботов!");
                tts = "^Computer Vision^ - категория, где вы сможете программировать роботов!";
            }
            else if ((progress.results & 64) == 1)
            {
                text.Add("Оптимизация и RL - оптимизируйте реальные процессы в своё удовольствие!");
                tts = "^Оптимизация и RL^ - оптимизируйте реальные процессы в своё удовольствие!";
            }
            else if ((progress.results & 128) == 1)
            {
                text.Add("Ваша тяга к красоте может быть удовлетворена категорией Дизайн Интерфейсов!");
                tts = "Ваша тяга к красоте может быть удовлетворена категорией ^Дизайн Интерфейсов^!";
            }

            return new ResponseResponse {end_session = true, text = text.ToArray(), tts = tts, card = new CardCommon
                {
                    type = "BigImage",
                    image_id = 457239018
                }
            };
        }
    }
}