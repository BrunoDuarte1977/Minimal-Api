using Minimal_API;
using MySql.Data.MySqlClient;
using System;

        string connectionString = "Server=localhost;User Id=root;Password=admin;Database=minimal_api";
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                Console.WriteLine("Conexão bem-sucedida!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

IHostBuilder CreateHostBuilder(string[] args){
    return Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>{
        webBuilder.UseStartup<Startup>();
        //webBuilder.UseStartup<TesteConexao>();
    });
};

CreateHostBuilder(args).Build().Run();