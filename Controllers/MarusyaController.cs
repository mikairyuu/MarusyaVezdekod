using Dapper;
using MarusyaVezdekod.Models;
using MarusyaVezdekod.Util;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace MarusyaVezdekod.Controllers;

[ApiController]
[Route("webhook")]
public class MarusyaController : ControllerBase
{
    private readonly ILogger<MarusyaController> _logger;
    private readonly string _connectionString;

    public MarusyaController(ILogger<MarusyaController> logger)
    {
        _connectionString = "Server=localhost;Database=postgres;User Id=postgres;";
        _logger = logger;
    }

    [HttpPost]
    public async Task<ResponseModel> Get(RequestModel requestModel)
    {
        var resultSession = new SessionResponse
        {
            message_id = requestModel.session.message_id,
            session_id = requestModel.session.session_id,
            user_id = requestModel.session.application.application_id
        };
        NpgsqlConnection connection = null;
        try
        {
            await using (connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var testProgress = (await connection.QueryAsync<SessionTestProgress>(
                    "select * from test_progress where session=@session",
                    new
                    {
                        @session = requestModel.session.session_id
                    })).FirstOrDefault();
                ResponseResponse resultResponse;
                var cmd = requestModel.request.command;
                if ((cmd.Contains("команда х") || cmd.Contains("команда x")) && cmd.Contains("вездекод"))
                {
                    resultResponse = new ResponseResponse
                    {
                        text = new[] {"Привет вездекодерам!"},
                        tts = "Привет вездек`одерам!"
                    };
                }
                else if (cmd.Contains("тест") || testProgress != null)
                {
                    resultResponse =
                        await TestQuestionHandler.HandleTestResponse(testProgress, connection, requestModel);
                }
                else
                {
                    resultResponse = new ResponseResponse
                    {
                        text = new[] {"Простите, но ваш запрос мне не ясен."},
                        tts = "Простите, но ваш запрос мне не ясен."
                    };
                }

                await connection?.CloseAsync();
                return new ResponseModel {response = resultResponse, session = resultSession};
            }
        }
        catch (Exception e)
        {
            // ignored
        }
        finally
        {
            connection?.CloseAsync();
        }

        return new ResponseModel
        {
            response = new ResponseResponse
            {
                text = new[] {"К сожалению, произошла какая-то ошибка, сообщите о ней разработчикам."},
                tts = "К сожалению, произошла какая-то ошибка, сообщите о ней разработчикам."
            },
            session = resultSession
        };
    }
}