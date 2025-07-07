using Microsoft.Data.SqlClient;
using NDMNS_API.Models;
using NDMNS_API.Responses;

namespace NDMNS_API.Repositories
{
    public class HelpdeskRepository
    {
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public HelpdeskRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        public DtoResponse<List<HelpdeskViewModel>> GetAllHelpdesks()
        {
            List<HelpdeskViewModel> helpdesks = new List<HelpdeskViewModel>();

            try
            {
                string query =
                    "SELECT h.hlp_id, h.isp_id, h.hlp_name, h.hlp_role, h.hlp_whatsappnumber, h.hlp_emailaddress, i.isp_name "
                    + "FROM tbl_m_helpdesk h "
                    + "INNER JOIN tbl_m_isp i ON h.isp_id = i.isp_id "
                    + "ORDER BY h.hlp_name ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    HelpdeskViewModel helpdesk = new HelpdeskViewModel
                    {
                        Id = reader["hlp_id"].ToString(),
                        IspId = reader["isp_id"].ToString(),
                        Name = reader["hlp_name"].ToString(),
                        Role = Convert.ToInt32(reader["hlp_role"]),
                        WhatsappNumber = reader["hlp_whatsappnumber"].ToString(),
                        EmailAddress = reader["hlp_emailaddress"].ToString(),
                        IspName = reader["isp_name"].ToString(),
                    };
                    helpdesks.Add(helpdesk);
                }
                reader.Close();

                return new DtoResponse<List<HelpdeskViewModel>>
                {
                    status = 200,
                    message = "Helpdesks retrieved successfully",
                    data = helpdesks,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<HelpdeskViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<HelpdeskViewModel>>
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

        public DtoResponse<HelpdeskViewModel> GetHelpdeskById(string id)
        {
            HelpdeskViewModel helpdesk = null;

            try
            {
                string query =
                    "SELECT h.hlp_id, h.isp_id, h.hlp_name, h.hlp_role, h.hlp_whatsappnumber, h.hlp_emailaddress, i.isp_name "
                    + "FROM tbl_m_helpdesk h "
                    + "INNER JOIN tbl_m_isp i ON h.isp_id = i.isp_id "
                    + "WHERE h.hlp_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    helpdesk = new HelpdeskViewModel
                    {
                        Id = reader["hlp_id"].ToString(),
                        IspId = reader["isp_id"].ToString(),
                        Name = reader["hlp_name"].ToString(),
                        Role = Convert.ToInt32(reader["hlp_role"]),
                        WhatsappNumber = reader["hlp_whatsappnumber"].ToString(),
                        EmailAddress = reader["hlp_emailaddress"].ToString(),
                        IspName = reader["isp_name"].ToString(),
                    };
                }
                reader.Close();

                if (helpdesk == null)
                {
                    return new DtoResponse<HelpdeskViewModel>
                    {
                        status = 404,
                        message = "Helpdesk not found",
                        data = null,
                    };
                }

                return new DtoResponse<HelpdeskViewModel>
                {
                    status = 200,
                    message = "Helpdesk retrieved successfully",
                    data = helpdesk,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<HelpdeskViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<HelpdeskViewModel>
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

        public DtoResponse<List<HelpdeskViewModel>> GetHelpdesksByIspId(string ispId)
        {
            List<HelpdeskViewModel> helpdesks = new List<HelpdeskViewModel>();

            try
            {
                string query =
                    "SELECT h.hlp_id, h.isp_id, h.hlp_name, h.hlp_role, h.hlp_whatsappnumber, h.hlp_emailaddress, i.isp_name "
                    + "FROM tbl_m_helpdesk h "
                    + "INNER JOIN tbl_m_isp i ON h.isp_id = i.isp_id "
                    + "WHERE h.isp_id = @IspId "
                    + "ORDER BY h.hlp_name ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@IspId", ispId);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    HelpdeskViewModel helpdesk = new HelpdeskViewModel
                    {
                        Id = reader["hlp_id"].ToString(),
                        IspId = reader["isp_id"].ToString(),
                        Name = reader["hlp_name"].ToString(),
                        Role = Convert.ToInt32(reader["hlp_role"]),
                        WhatsappNumber = reader["hlp_whatsappnumber"].ToString(),
                        EmailAddress = reader["hlp_emailaddress"].ToString(),
                        IspName = reader["isp_name"].ToString(),
                    };
                    helpdesks.Add(helpdesk);
                }
                reader.Close();

                return new DtoResponse<List<HelpdeskViewModel>>
                {
                    status = 200,
                    message = "Helpdesks retrieved successfully",
                    data = helpdesks,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<HelpdeskViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<HelpdeskViewModel>>
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

        public DtoResponse<HelpdeskViewModel> AddHelpdesk(Helpdesk helpdesk, string createdBy)
        {
            try
            {
                string newId = Guid.NewGuid().ToString();
                string query =
                    "INSERT INTO tbl_m_helpdesk (hlp_id, isp_id, hlp_name, hlp_role, hlp_whatsappnumber, hlp_emailaddress, hlp_createdby, hlp_createddate) "
                    + "VALUES (@Id, @IspId, @Name, @Role, @WhatsappNumber, @EmailAddress, @CreatedBy, @CreatedDate)";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", newId);
                command.Parameters.AddWithValue("@IspId", helpdesk.IspId);
                command.Parameters.AddWithValue("@Name", helpdesk.Name);
                command.Parameters.AddWithValue("@Role", helpdesk.Role);
                command.Parameters.AddWithValue("@WhatsappNumber", helpdesk.WhatsappNumber);
                command.Parameters.AddWithValue("@EmailAddress", helpdesk.EmailAddress);
                command.Parameters.AddWithValue("@CreatedBy", createdBy);
                command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    helpdesk.Id = newId;
                    var helpdeskViewModel = new HelpdeskViewModel
                    {
                        Id = newId,
                        IspId = helpdesk.IspId,
                        Name = helpdesk.Name,
                        Role = helpdesk.Role,
                        WhatsappNumber = helpdesk.WhatsappNumber,
                        EmailAddress = helpdesk.EmailAddress,
                        IspName = "ISP not found", // Will be updated if needed
                    };

                    return new DtoResponse<HelpdeskViewModel>
                    {
                        status = 201,
                        message = "Helpdesk data saved successfully",
                        data = helpdeskViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<HelpdeskViewModel>
                    {
                        status = 500,
                        message = "Failed to save data Helpdesk",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<HelpdeskViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to save data Helpdesk",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<HelpdeskViewModel>
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

        public DtoResponse<HelpdeskViewModel> UpdateHelpdesk(Helpdesk helpdesk, string updatedBy)
        {
            try
            {
                string query =
                    "UPDATE tbl_m_helpdesk "
                    + "SET isp_id = @IspId, hlp_name = @Name, hlp_role = @Role, hlp_whatsappnumber = @WhatsappNumber, hlp_emailaddress = @EmailAddress, hlp_updatedby = @UpdatedBy, hlp_updateddate = @UpdatedDate "
                    + "WHERE hlp_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", helpdesk.Id);
                command.Parameters.AddWithValue("@IspId", helpdesk.IspId);
                command.Parameters.AddWithValue("@Name", helpdesk.Name);
                command.Parameters.AddWithValue("@Role", helpdesk.Role);
                command.Parameters.AddWithValue("@WhatsappNumber", helpdesk.WhatsappNumber);
                command.Parameters.AddWithValue("@EmailAddress", helpdesk.EmailAddress);
                command.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    var helpdeskViewModel = new HelpdeskViewModel
                    {
                        Id = helpdesk.Id,
                        IspId = helpdesk.IspId,
                        Name = helpdesk.Name,
                        Role = helpdesk.Role,
                        WhatsappNumber = helpdesk.WhatsappNumber,
                        EmailAddress = helpdesk.EmailAddress,
                        IspName = "ISP not found",
                    };

                    return new DtoResponse<HelpdeskViewModel>
                    {
                        status = 200,
                        message = "Helpdesk data saved successfully",
                        data = helpdeskViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<HelpdeskViewModel>
                    {
                        status = 404,
                        message = "Helpdesk not found",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<HelpdeskViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update Helpdesk",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<HelpdeskViewModel>
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

        public DtoResponse<object> DeleteHelpdesk(string id)
        {
            try
            {
                string query = "DELETE FROM tbl_m_helpdesk WHERE hlp_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return new DtoResponse<object>
                    {
                        status = 200,
                        message = "Helpdesk data deleted successfully",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 404,
                        message = "Helpdesk not found or already deleted",
                        data = null,
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 547)
                {
                    return new DtoResponse<object>
                    {
                        status = 409,
                        message =
                            "Cannot delete Helpdesk because it is referenced by existing data. Please remove all data associated with this Helpdesk first.",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 500,
                        message = "SQL Error: Failed to delete Helpdesk",
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

        public bool IsIspExists(string ispId)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM tbl_m_isp WHERE isp_id = @IspId";
                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@IspId", ispId);

                _connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }
    }
}
