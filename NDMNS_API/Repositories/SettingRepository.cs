using Microsoft.Data.SqlClient;
using NDMNS_API.Models;

namespace NDMNS_API.Repositories
{
    public class SettingRepository
    {
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public SettingRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        public List<SettingViewModel> GetAllSettings()
        {
            List<SettingViewModel> settings = new List<SettingViewModel>();

            try
            {
                string query =
                    "SELECT set_id, set_name, set_code, set_value "
                    + "FROM tbl_r_setting "
                    + "ORDER BY set_name ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    SettingViewModel setting = new SettingViewModel
                    {
                        Id = reader["set_id"].ToString(),
                        Name = reader["set_name"].ToString(),
                        Code = reader["set_code"].ToString(),
                        Value = reader["set_value"].ToString(),
                    };
                    settings.Add(setting);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return settings;
        }

        public SettingViewModel GetSettingById(string id)
        {
            SettingViewModel setting = null;

            try
            {
                string query =
                    "SELECT set_id, set_name, set_code, set_value "
                    + "FROM tbl_r_setting "
                    + "WHERE set_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    setting = new SettingViewModel
                    {
                        Id = reader["set_id"].ToString(),
                        Name = reader["set_name"].ToString(),
                        Code = reader["set_code"].ToString(),
                        Value = reader["set_value"].ToString(),
                    };
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return setting;
        }

        public SettingViewModel GetSettingByCode(string code)
        {
            SettingViewModel setting = null;

            try
            {
                string query =
                    "SELECT set_id, set_name, set_code, set_value "
                    + "FROM tbl_r_setting "
                    + "WHERE set_code = @Code";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Code", code);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    setting = new SettingViewModel
                    {
                        Id = reader["set_id"].ToString(),
                        Name = reader["set_name"].ToString(),
                        Code = reader["set_code"].ToString(),
                        Value = reader["set_value"].ToString(),
                    };
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return setting;
        }

        public void AddSetting(Setting setting, string createdBy)
        {
            try
            {
                string newId = Guid.NewGuid().ToString();
                string query =
                    "INSERT INTO tbl_r_setting (set_id, set_name, set_code, set_value, set_createdby, set_createddate) "
                    + "VALUES (@Id, @Name, @Code, @Value, @CreatedBy, @CreatedDate)";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", newId);
                command.Parameters.AddWithValue("@Name", setting.Name);
                command.Parameters.AddWithValue("@Code", setting.Code);
                command.Parameters.AddWithValue("@Value", setting.Value);
                command.Parameters.AddWithValue("@CreatedBy", createdBy);
                command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                _connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public void UpdateSetting(Setting setting, string updatedBy)
        {
            try
            {
                string query =
                    "UPDATE tbl_r_setting "
                    + "SET set_name = @Name, set_code = @Code, set_value = @Value, set_updatedby = @UpdatedBy, set_updateddate = @UpdatedDate "
                    + "WHERE set_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", setting.Id);
                command.Parameters.AddWithValue("@Name", setting.Name);
                command.Parameters.AddWithValue("@Code", setting.Code);
                command.Parameters.AddWithValue("@Value", setting.Value);
                command.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                _connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public string DeleteSetting(string id)
        {
            var result = "";

            try
            {
                string query = "DELETE FROM tbl_r_setting WHERE set_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    result = "success";
                }
                else
                {
                    result = "Setting not found or already deleted";
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 547)
                {
                    result = "Foreign Key Conflict";
                }
                else
                {
                    result = $"Database error: {sqlEx.Message}";
                }
            }
            catch (Exception ex)
            {
                result = $"Unexpected error: {ex.Message}";
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return result;
        }

        public bool IsCodeExists(string code, string? excludeId = null)
        {
            bool exists = false;

            try
            {
                string query = "SELECT COUNT(*) FROM tbl_r_setting WHERE set_code = @Code";

                if (!string.IsNullOrEmpty(excludeId))
                {
                    query += " AND set_id != @ExcludeId";
                }

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Code", code);

                if (!string.IsNullOrEmpty(excludeId))
                {
                    command.Parameters.AddWithValue("@ExcludeId", excludeId);
                }

                _connection.Open();
                int count = (int)command.ExecuteScalar();
                exists = count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking code existence: {ex.Message}");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }

            return exists;
        }
    }
}
