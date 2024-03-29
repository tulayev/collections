﻿namespace Collections.Utils
{
    public static class Heroku
    {
        public static string GetHerokuConnectionString(IConfiguration config)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                return config.GetConnectionString("Postgres");

            
            string connectionUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            var databaseUri = new Uri(connectionUrl);
            string db = databaseUri.LocalPath.TrimStart('/');
            string[] userInfo = databaseUri.UserInfo.Split(':', StringSplitOptions.RemoveEmptyEntries);

            return $"User ID={userInfo[0]};Password={userInfo[1]};Host={databaseUri.Host};Port={databaseUri.Port};Database={db};Pooling=true;SSL Mode=Require;Trust Server Certificate=True;";
        }
    }
}
