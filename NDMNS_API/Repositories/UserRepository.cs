using Microsoft.Data.SqlClient;
using NDMNS_API.Models;
using NDMNS_API.Responses;

namespace NDMNS_API.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        public DtoResponse<List<UserViewModel>> GetAllUsers()
        {
            List<UserViewModel> users = new List<UserViewModel>();

            try
            {
                string query =
                    "SELECT usr_id, usr_name, usr_code, usr_nrp, usr_role, usr_email, "
                    + "usr_whatsapp, usr_whatsappclient, usr_status "
                    + "FROM tbl_m_user "
                    + "WHERE usr_role != 'Admin' "
                    + "ORDER BY usr_name ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    UserViewModel user = new UserViewModel
                    {
                        Id = reader["usr_id"].ToString(),
                        Name = reader["usr_name"].ToString(),
                        Code = reader["usr_code"].ToString(),
                        Nrp = reader["usr_nrp"].ToString(),
                        Role = reader["usr_role"].ToString(),
                        Email = reader["usr_email"].ToString(),
                        WhatsApp = reader["usr_whatsapp"].ToString(),
                        WhatsAppClient = Convert.ToInt32(reader["usr_whatsappclient"]),
                        Status = Convert.ToInt32(reader["usr_status"]),
                    };
                    users.Add(user);
                }
                reader.Close();

                return new DtoResponse<List<UserViewModel>>
                {
                    status = 200,
                    message = "Users retrieved successfully",
                    data = users,
                };
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                return new DtoResponse<List<UserViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<UserViewModel>>
                {
                    status = 500,
                    message = "Internal Server Error: " + ex.Message,
                    data = null,
                };
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public DtoResponse<UserViewModel> GetUserById(string id)
        {
            UserViewModel user = null;

            try
            {
                string query =
                    "SELECT usr_id, usr_name, usr_code, usr_nrp, usr_password, usr_role, usr_email, "
                    + "usr_whatsapp, usr_whatsappclient, usr_status "
                    + "FROM tbl_m_user "
                    + "WHERE usr_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    user = new UserViewModel
                    {
                        Id = reader["usr_id"].ToString(),
                        Name = reader["usr_name"].ToString(),
                        Code = reader["usr_code"].ToString(),
                        Nrp = reader["usr_nrp"].ToString(),
                        Password = reader["usr_password"].ToString(),
                        Role = reader["usr_role"].ToString(),
                        Email = reader["usr_email"].ToString(),
                        WhatsApp = reader["usr_whatsapp"].ToString(),
                        WhatsAppClient = Convert.ToInt32(reader["usr_whatsappclient"]),
                        Status = Convert.ToInt32(reader["usr_status"]),
                    };
                }
                reader.Close();

                if (user == null)
                {
                    return new DtoResponse<UserViewModel>
                    {
                        status = 404,
                        message = "User not found",
                        data = null,
                    };
                }

                return new DtoResponse<UserViewModel>
                {
                    status = 200,
                    message = "User retrieved successfully",
                    data = user,
                };
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                return new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = "Internal Server Error: " + ex.Message,
                    data = null,
                };
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public DtoResponse<UserViewModel> GetUserByNumber(string whatsApp)
        {
            UserViewModel user = null;

            try
            {
                string query =
                    "SELECT usr_id, usr_name, usr_code, usr_nrp, usr_role, usr_email, "
                    + "usr_whatsapp, usr_whatsappclient, usr_status "
                    + "FROM tbl_m_user "
                    + "WHERE usr_whatsapp = @WhatsApp";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@WhatsApp", whatsApp);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    user = new UserViewModel
                    {
                        Id = reader["usr_id"].ToString(),
                        Name = reader["usr_name"].ToString(),
                        Code = reader["usr_code"].ToString(),
                        Nrp = reader["usr_nrp"].ToString(),
                        Role = reader["usr_role"].ToString(),
                        Email = reader["usr_email"].ToString(),
                        WhatsApp = reader["usr_whatsapp"].ToString(),
                        WhatsAppClient = Convert.ToInt32(reader["usr_whatsappclient"]),
                        Status = Convert.ToInt32(reader["usr_status"]),
                    };
                }
                reader.Close();

                if (user == null)
                {
                    return new DtoResponse<UserViewModel>
                    {
                        status = 404,
                        message = "User not found",
                        data = null,
                    };
                }

                return new DtoResponse<UserViewModel>
                {
                    status = 200,
                    message = "User retrieved successfully",
                    data = user,
                };
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                return new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = "Internal Server Error: " + ex.Message,
                    data = null,
                };
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public DtoResponse<UserViewModel> AddUser(User user, string createdBy)
        {
            try
            {
                var existingUser = CheckNrpExists(user.Nrp);
                if (existingUser)
                {
                    return new DtoResponse<UserViewModel>
                    {
                        status = 409,
                        message = "NRP already exists",
                        data = null,
                    };
                }

                string newId = Guid.NewGuid().ToString();
                string query =
                    "INSERT INTO tbl_m_user (usr_id, usr_name, usr_code, usr_nrp, usr_password, usr_role, "
                    + "usr_email, usr_whatsapp, usr_whatsappclient, usr_status, usr_createdby, usr_createddate) "
                    + "VALUES (@Id, @Name, @Code, @Nrp, @Password, @Role, @Email, @WhatsApp, @WhatsAppClient, "
                    + "@Status, @CreatedBy, @CreatedDate)";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", newId);
                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Code", user.Code);
                command.Parameters.AddWithValue("@Nrp", user.Nrp);
                command.Parameters.AddWithValue(
                    "@Password",
                    BCrypt.Net.BCrypt.HashPassword(user.Password)
                );
                command.Parameters.AddWithValue("@Role", user.Role);
                command.Parameters.AddWithValue("@Email", (object)user.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@WhatsApp", (object)user.WhatsApp ?? DBNull.Value);
                command.Parameters.AddWithValue("@WhatsAppClient", user.WhatsAppClient);
                command.Parameters.AddWithValue("@Status", user.Status);
                command.Parameters.AddWithValue("@CreatedBy", createdBy);
                command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    var userViewModel = new UserViewModel
                    {
                        Id = newId,
                        Name = user.Name,
                        Code = user.Code,
                        Nrp = user.Nrp,
                        Role = user.Role,
                        Email = user.Email,
                        WhatsApp = user.WhatsApp,
                        WhatsAppClient = user.WhatsAppClient,
                        Status = user.Status,
                    };

                    return new DtoResponse<UserViewModel>
                    {
                        status = 201,
                        message = "User data saved successfully",
                        data = userViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<UserViewModel>
                    {
                        status = 500,
                        message = "Failed to save data user",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to save data user",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = "Internal Server Error: " + ex.Message,
                    data = null,
                };
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public DtoResponse<UserViewModel> UpdateUser(string id, User user, string updatedBy)
        {
            try
            {
                var existingUser = CheckNrpExistsExcludeId(user.Nrp, id);
                if (existingUser)
                {
                    return new DtoResponse<UserViewModel>
                    {
                        status = 409,
                        message = "Nrp already exists",
                        data = null,
                    };
                }

                string query;
                SqlCommand command;

                if (!string.IsNullOrWhiteSpace(user.Password))
                {
                    query =
                        "UPDATE tbl_m_user "
                        + "SET usr_name = @Name, usr_code = @Code, usr_nrp = @Nrp, usr_password = @Password, "
                        + "usr_role = @Role, usr_email = @Email, usr_whatsapp = @WhatsApp, usr_whatsappclient = @WhatsAppClient, "
                        + "usr_status = @Status, usr_updatedby = @UpdatedBy, usr_updateddate = @UpdatedDate "
                        + "WHERE usr_id = @Id";

                    command = new SqlCommand(query, _connection);
                    command.Parameters.AddWithValue(
                        "@Password",
                        BCrypt.Net.BCrypt.HashPassword(user.Password)
                    );
                }
                else
                {
                    query =
                        "UPDATE tbl_m_user "
                        + "SET usr_name = @Name, usr_code = @Code, usr_nrp = @Nrp, "
                        + "usr_role = @Role, usr_email = @Email, usr_whatsapp = @WhatsApp, usr_whatsappclient = @WhatsAppClient, "
                        + "usr_status = @Status, usr_updatedby = @UpdatedBy, usr_updateddate = @UpdatedDate "
                        + "WHERE usr_id = @Id";

                    command = new SqlCommand(query, _connection);
                }

                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Code", user.Code);
                command.Parameters.AddWithValue("@Nrp", user.Nrp);
                command.Parameters.AddWithValue("@Role", user.Role);
                command.Parameters.AddWithValue("@Email", (object)user.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@WhatsApp", (object)user.WhatsApp ?? DBNull.Value);
                command.Parameters.AddWithValue("@WhatsAppClient", user.WhatsAppClient);
                command.Parameters.AddWithValue("@Status", user.Status);
                command.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    var userViewModel = new UserViewModel
                    {
                        Id = id,
                        Name = user.Name,
                        Code = user.Code,
                        Nrp = user.Nrp,
                        Role = user.Role,
                        Email = user.Email,
                        WhatsApp = user.WhatsApp,
                        WhatsAppClient = user.WhatsAppClient,
                        Status = user.Status,
                    };

                    return new DtoResponse<UserViewModel>
                    {
                        status = 200,
                        message = "User data saved successfully",
                        data = userViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<UserViewModel>
                    {
                        status = 404,
                        message = "User not found",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update user",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = "Internal Server Error: " + ex.Message,
                    data = null,
                };
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public DtoResponse<UserViewModel> UpdateStatusUser(string id, UserViewModel user)
        {
            try
            {
                string query =
                    "UPDATE tbl_m_user "
                    + "SET usr_whatsappclient = @WhatsAppClient "
                    + "WHERE usr_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);

                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@WhatsAppClient", user.WhatsAppClient);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    var userViewModel = new UserViewModel
                    {
                        Id = id,
                        Name = user.Name,
                        Code = user.Code,
                        Nrp = user.Nrp,
                        Role = user.Role,
                        Email = user.Email,
                        WhatsApp = user.WhatsApp,
                        WhatsAppClient = user.WhatsAppClient,
                        Status = user.Status,
                    };

                    return new DtoResponse<UserViewModel>
                    {
                        status = 200,
                        message = "User data saved successfully",
                        data = userViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<UserViewModel>
                    {
                        status = 404,
                        message = "User not found",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update user",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = "Internal Server Error: " + ex.Message,
                    data = null,
                };
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public DtoResponse<UserViewModel> UpdateUserPassword(string id, User user, string updatedBy)
        {
            try
            {
                string query =
                    "UPDATE tbl_m_user " + "SET usr_password = @Password " + "WHERE usr_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);

                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue(
                    "@Password",
                    BCrypt.Net.BCrypt.HashPassword(user.Password)
                );

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    var userViewModel = new UserViewModel
                    {
                        Id = id,
                        Name = user.Name,
                        Code = user.Code,
                        Nrp = user.Nrp,
                        Role = user.Role,
                        Email = user.Email,
                        WhatsApp = user.WhatsApp,
                        WhatsAppClient = user.WhatsAppClient,
                        Status = user.Status,
                    };

                    return new DtoResponse<UserViewModel>
                    {
                        status = 200,
                        message = "User data saved successfully",
                        data = userViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<UserViewModel>
                    {
                        status = 404,
                        message = "User not found",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update user",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = "Internal Server Error: " + ex.Message,
                    data = null,
                };
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public DtoResponse<object> DeleteUser(string id)
        {
            try
            {
                string query = "DELETE FROM tbl_m_user WHERE usr_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return new DtoResponse<object>
                    {
                        status = 200,
                        message = "User data deleted successfully",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 404,
                        message = "User not found or already deleted",
                        data = null,
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 547) // Foreign key constraint
                {
                    return new DtoResponse<object>
                    {
                        status = 409,
                        message =
                            "Cannot delete User because it is referenced by other records. Please remove all references first.",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 500,
                        message = "SQL Error: Failed to delete user",
                        data = null,
                    };
                }
            }
            catch (Exception ex)
            {
                return new DtoResponse<object>
                {
                    status = 500,
                    message = "Internal Server Error: " + ex.Message,
                    data = null,
                };
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public DtoResponse<UserViewModel> Login(LoginRequest request)
        {
            try
            {
                string query =
                    "SELECT usr_id, usr_name, usr_nrp, usr_password, usr_role, usr_email, usr_status, "
                    + "usr_whatsapp, usr_whatsappclient "
                    + "FROM tbl_m_user "
                    + "WHERE usr_nrp = @Nrp";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Nrp", request.Nrp);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string storedPassword = reader["usr_password"].ToString();
                    int userStatus = Convert.ToInt32(reader["usr_status"]);

                    if (userStatus != 2)
                    {
                        reader.Close();
                        return new DtoResponse<UserViewModel>
                        {
                            status = 403,
                            message = "User account is inactive",
                            data = null,
                        };
                    }

                    if (BCrypt.Net.BCrypt.Verify(request.Password, storedPassword))
                    {
                        var loginResponse = new UserViewModel
                        {
                            Id = reader["usr_id"].ToString(),
                            Name = reader["usr_name"].ToString(),
                            Nrp = reader["usr_nrp"].ToString(),
                            Role = reader["usr_role"].ToString(),
                            Email = reader["usr_email"].ToString(),
                            WhatsApp = reader["usr_whatsapp"].ToString(),
                            WhatsAppClient = Convert.ToInt32(
                                reader["usr_whatsappclient"].ToString()
                            ),
                            Status = userStatus,
                        };

                        reader.Close();

                        return new DtoResponse<UserViewModel>
                        {
                            status = 200,
                            message = "Login successful",
                            data = loginResponse,
                        };
                    }
                    else
                    {
                        reader.Close();
                        return new DtoResponse<UserViewModel>
                        {
                            status = 401,
                            message = "Failed! Invalid NRP or Password",
                            data = null,
                        };
                    }
                }
                else
                {
                    reader.Close();
                    return new DtoResponse<UserViewModel>
                    {
                        status = 401,
                        message = "Invalid Nrp or password",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to authenticate user",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = "Internal Server Error: " + ex.Message,
                    data = null,
                };
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        private bool CheckNrpExists(string Nrp)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT COUNT(*) FROM tbl_m_user WHERE usr_nrp = @Nrp";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Nrp", Nrp);

                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CheckNrpExistsExcludeId(string Nrp, string excludeId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query =
                        "SELECT COUNT(*) FROM tbl_m_user WHERE usr_nrp = @Nrp AND usr_id != @Id";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Nrp", Nrp);
                    command.Parameters.AddWithValue("@Id", excludeId);

                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
