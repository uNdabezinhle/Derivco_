using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RouletteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouletteController : ControllerBase
    {
        private readonly IConfiguration _config;

        public RouletteController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("placebet")]
        public ActionResult PlaceBet([FromBody] Bet bet)
        {
            if (bet.Amount <= 0 || bet.Number < 0 || bet.Number > 36)
            {
                return BadRequest("Invalid bet data.");
            }

            try
            {
                using (var connection = new SqliteConnection(_config.GetConnectionString("RouletteDB")))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO Bets (Amount, Number) VALUES (@amount, @number)";
                    command.Parameters.AddWithValue("@amount", bet.Amount);
                    command.Parameters.AddWithValue("@number", bet.Number);
                    command.ExecuteNonQuery();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while placing the bet: {ex.Message}");
            }
        }

        [HttpGet("spin")]
        public ActionResult<int> Spin()
        {
            try
            {
                var result = new Random().Next(0, 37);

                using (var connection = new SqliteConnection(_config.GetConnectionString("RouletteDB")))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO Spins (Result) VALUES (@result)";
                    command.Parameters.AddWithValue("@result", result);
                    command.ExecuteNonQuery();
                }

                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while spinning the roulette: {ex.Message}");
            }
        }

        [HttpPost("payout")]
        public ActionResult<decimal> Payout([FromBody] int winningNumber)
        {
            if (winningNumber < 0 || winningNumber > 36)
            {
                return BadRequest("Invalid winning number.");
            }

            try
            {
                var totalAmountWon = 0m;

                using (var connection = new SqliteConnection(_config.GetConnectionString("RouletteDB")))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM Bets";
                    var reader = command.ExecuteReader();

                    var bets = new List<Bet>();

                    while (reader.Read())
                    {
                        bets.Add(new Bet
                        {
                            Id = reader.GetInt64(0),
                            Amount = reader.GetDecimal(1),
                            Number = reader.GetInt32(2)
                        });
                    }

                    reader.Close();

                    foreach (var bet in bets)
                    {
                        if (bet.Number == winningNumber)
                        {
                            totalAmountWon += bet.Amount * 35;
                        }
                        else
                        {
                            totalAmountWon -= bet.Amount;
                        }

                        command = connection.CreateCommand();
                        command.CommandText = "DELETE FROM Bets WHERE Id = @id";
                        command.Parameters.AddWithValue("@id", bet.Id);
                        command.ExecuteNonQuery();
                    }
                }

                return totalAmountWon;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while paying out the bets: {ex.Message}");
            }
        }

        [HttpGet("showpreviousspins")]
        public ActionResult<List<int>> ShowPreviousSpins()
        {
            try
            {
                using ( var connection = new SqliteConnection(_config.GetConnectionString("RouletteDB")))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Result FROM Spins ORDER BY Id DESC LIMIT 10";
                    var reader = command.ExecuteReader();

                    var spins = new List<int>();

                    while (reader.Read())
                    {
                        spins.Add(reader.GetInt32(0));
                    }

                    reader.Close();

                    return spins;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving previous spins: {ex.Message}");
            }
        }
    }
}