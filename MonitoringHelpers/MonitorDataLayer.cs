﻿using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DataHarvester
{
    public class MonitorDataLayer : IMonitorDataLayer
    {
        ICredentials _credentials;
        NpgsqlConnectionStringBuilder builder;
        private string connString;
        private string context_connString;
        

        /// <summary>
        /// Constructor is used to build the connection string, 
        /// using a credentials object that has the relevant credentials 
        /// from the app settings, themselves derived from a json file.
        /// </summary>
        /// 
        public MonitorDataLayer(ICredentials credentials)
        {
            builder = new NpgsqlConnectionStringBuilder();

            builder.Host = credentials.Host;
            builder.Username = credentials.Username;
            builder.Password = credentials.Password;

            builder.Database = "mon";
            connString = builder.ConnectionString;

            builder.Database = "context";
            context_connString = builder.ConnectionString;

            _credentials = credentials;
        }

        public Credentials Credentials => (Credentials)_credentials;


        public bool SourceIdPresent(int source_id)
        {
            string sql_string = "Select id from sf.source_parameters where id = " + source_id.ToString();
            using (NpgsqlConnection Conn = new NpgsqlConnection(connString))
            {
                int res = Conn.QueryFirstOrDefault<int>(sql_string);
                return (res == 0) ? false : true;
            }
        }
         

        public Source FetchSourceParameters(int source_id)
        {
            using (NpgsqlConnection Conn = new NpgsqlConnection(connString))
            {
                return Conn.Get<Source>(source_id);
            }
        }


        public int GetNextHarvestEventId()
        {
            using (NpgsqlConnection Conn = new NpgsqlConnection(connString))
            {
                string sql_string = "select max(id) from sf.harvest_events ";
                int last_id = Conn.ExecuteScalar<int>(sql_string);
                return last_id + 1;
            }

        }

        public IEnumerable<StudyFileRecord> FetchStudyFileRecords(int source_id, int harvest_type_id = 1)
        {
            string sql_string = GetRecordSelectList();
            sql_string += " from sf.source_data_studies ";
            sql_string += GetWhereClause(source_id, harvest_type_id);
            sql_string += " order by local_path";

            using (NpgsqlConnection Conn = new NpgsqlConnection(connString))
            {
                return Conn.Query<StudyFileRecord>(sql_string);
            }
        }

        public IEnumerable<ObjectFileRecord> FetchObjectFileRecords(int source_id, int harvest_type_id = 1)
        {
            string sql_string = GetRecordSelectList();
            sql_string += " from sf.source_data_objects";
            sql_string += GetWhereClause(source_id, harvest_type_id);
            sql_string += " order by local_path";

            using (NpgsqlConnection Conn = new NpgsqlConnection(connString))
            {
                return Conn.Query<ObjectFileRecord>(sql_string);
            }
        }


        public int FetchFileRecordsCount(int source_id, string source_type,
                                       int harvest_type_id = 1, DateTime? cutoff_date = null)
        {
            string sql_string = "select count(*) ";
            sql_string += source_type.ToLower() == "study" ? "from sf.source_data_studies"
                                                 : "from sf.source_data_objects";
            sql_string += GetWhereClause(source_id, harvest_type_id);

            using (NpgsqlConnection Conn = new NpgsqlConnection(connString))
            {
                return Conn.ExecuteScalar<int>(sql_string);
            }
        }


        public int FetchFullFileCount(int source_id, string source_type, int harvest_type_id)
        {
            string sql_string = "select count(*) ";
            sql_string += source_type.ToLower() == "study" ? "from sf.source_data_studies"
                                                 : "from sf.source_data_objects";
            sql_string += " where source_id = " + source_id.ToString();
            sql_string += " and local_path is not null";
            if (harvest_type_id == 3)
            {
                sql_string += " and for_testing = true";
            }

            using (NpgsqlConnection Conn = new NpgsqlConnection(connString))
            {
                return Conn.ExecuteScalar<int>(sql_string);
            }
        }


        public IEnumerable<StudyFileRecord> FetchStudyFileRecordsByOffset(int source_id, int offset_num,
                                      int amount, int harvest_type_id = 1)
        {
            string sql_string = GetRecordSelectList();
            sql_string += " from sf.source_data_studies ";
            sql_string += GetWhereClause(source_id, harvest_type_id);
            sql_string += " order by local_path ";
            sql_string += " offset " + offset_num.ToString() + " limit " + amount.ToString();

            using (NpgsqlConnection Conn = new NpgsqlConnection(connString))
            {
                return Conn.Query<StudyFileRecord>(sql_string);
            }
        }

        public IEnumerable<ObjectFileRecord> FetchObjectFileRecordsByOffset(int source_id, int offset_num,
                                     int amount, int harvest_type_id = 1)
        {
            string sql_string = GetRecordSelectList();
            sql_string += " from sf.source_data_objects ";
            sql_string += GetWhereClause(source_id, harvest_type_id);
            sql_string += " order by local_path ";
            sql_string += " offset " + offset_num.ToString() + " limit " + amount.ToString();

            using (NpgsqlConnection Conn = new NpgsqlConnection(connString))
            {
                return Conn.Query<ObjectFileRecord>(sql_string);
            }
        }

        private string GetWhereClause(int source_id, int harvest_type_id)
        {
            string where_clause = "";
            if (harvest_type_id == 1)
            {
                // Count all files.
                where_clause = " where source_id = " + source_id.ToString();
            }
            else if (harvest_type_id == 2)
            {
                // Harvest files that have been downloaded since the last import, 
                // NOTE - not since the last harvest, as multiple harvests may have
                // been carried out. A file should be harvested for import if it 
                // has not yet been imported, or a new download (possible a new version) 
                // has taken place since the import.
                // So files needed where their download date > import date, or they are new
                // and therefore have a null import date

                where_clause = " where source_id = " + source_id.ToString() +
                               " and (last_downloaded >= last_imported or last_imported is null) ";
            }
            else if (harvest_type_id == 3)
            {
                // use records marked for testing only
                where_clause = " where source_id = " + source_id.ToString() +
                               " and for_testing = true ";
            }
            where_clause += " and local_path is not null";
            return where_clause;
        }


        private string GetRecordSelectList()
        {
            string sql_file_select_string = "select id, source_id, sd_id, remote_url, last_revised, ";
            sql_file_select_string += " assume_complete, download_status, local_path, last_saf_id, last_downloaded, ";
            sql_file_select_string += " last_harvest_id, last_harvested, last_import_id, last_imported ";
            return sql_file_select_string;
        }

        // get record of interest
        public StudyFileRecord FetchStudyFileRecord(string sd_id, int source_id, string source_type)
        {
            using (NpgsqlConnection Conn = new NpgsqlConnection(connString))
            {
                string sql_string = GetRecordSelectList();
                sql_string += " from sf.source_data_studies";
                sql_string += " where sd_id = '" + sd_id + "' and source_id = " + source_id.ToString();
                return Conn.Query<StudyFileRecord>(sql_string).FirstOrDefault();
            }
        }


        public ObjectFileRecord FetchObjectFileRecord(string sd_id, int source_id, string source_type)
        {
            using (NpgsqlConnection Conn = new NpgsqlConnection(connString))
            {
                string sql_string = GetRecordSelectList();
                sql_string += " from sf.source_data_objects";
                sql_string += " where sd_id = '" + sd_id + "' and source_id = " + source_id.ToString();
                return Conn.Query<ObjectFileRecord>(sql_string).FirstOrDefault();
            }
        }

        public void UpdateFileRecLastHarvested(int id, string source_type, int last_harvest_id)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                string sql_string = source_type.ToLower() == "study" ? "update sf.source_data_studies"
                                                           : "update sf.source_data_objects";
                sql_string += " set last_harvest_id = " + last_harvest_id.ToString() + ", ";
                sql_string += " last_harvested = current_timestamp";
                sql_string += " where id = " + id.ToString();
                conn.Execute(sql_string);
            }
        }

        public int StoreHarvestEvent(HarvestEvent harvest)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                return (int)conn.Insert<HarvestEvent>(harvest);
            }
        }
        

        // gets a 2 letter language code rather than thean the original 3
        public string lang_3_to_2(string lang_code_3)
        {
            using (NpgsqlConnection Conn = new NpgsqlConnection(context_connString))
            {
                string sql_string = "select code from lup.language_codes where ";
                sql_string += " marc_code = '" + lang_code_3 + "';";
                return Conn.Query<string>(sql_string).FirstOrDefault();
            }
        }


    }

}

