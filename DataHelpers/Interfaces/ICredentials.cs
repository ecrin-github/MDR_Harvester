﻿namespace DataHarvester
{
    public interface ICredentials
    {
        string Host { get; set; }
        string Password { get; set; }
        string Username { get; set; }

        string GetConnectionString(string database_name, int harvest_type_id);
    }
}