using Microsoft.Data.SqlClient;
using NDMNS_API.Models;
using NDMNS_API.Responses;
using NDMNS_API.Services;

namespace NDMNS_API.Repositories
{
    public class IspRepository
    {
        private readonly HelperService _helperService;
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public IspRepository(IConfiguration configuration, HelperService helperService)
        {
            _helperService = helperService;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        public DtoResponse<List<IspViewModel>> GetAllIsps()
        {
            List<IspViewModel> isps = new List<IspViewModel>();

            try
            {
                var groupsResponse = _helperService.GetGroups();
                var groups = groupsResponse?.data ?? new List<GroupResponse>();

                string query =
                    "SELECT isp_id, isp_name, isp_whatsappgroup, isp_emailaddress "
                    + "FROM tbl_m_isp "
                    + "ORDER BY isp_name ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string whatsappId = reader["isp_whatsappgroup"].ToString();
                    var matchingGroup = groups.FirstOrDefault(g => g.Id == whatsappId);

                    IspViewModel isp = new IspViewModel
                    {
                        Id = reader["isp_id"].ToString(),
                        Name = reader["isp_name"].ToString(),
                        WhatsappGroup = whatsappId,
                        EmailAddress = reader["isp_emailaddress"].ToString(),
                        WhatsappGroupName = matchingGroup?.Name ?? "Group not found",
                    };
                    isps.Add(isp);
                }
                reader.Close();

                return new DtoResponse<List<IspViewModel>>
                {
                    status = 200,
                    message = "ISPs retrieved successfully",
                    data = isps,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<IspViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<IspViewModel>>
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

        public DtoResponse<IspViewModel> GetIspById(string id)
        {
            IspViewModel isp = null;

            try
            {
                var groupsResponse = _helperService.GetGroups();
                var groups = groupsResponse?.data ?? new List<GroupResponse>();

                string query =
                    "SELECT isp_id, isp_name, isp_whatsappgroup, isp_emailaddress "
                    + "FROM tbl_m_isp "
                    + "WHERE isp_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string whatsappId = reader["isp_whatsappgroup"].ToString();
                    var matchingGroup = groups.FirstOrDefault(g => g.Id == whatsappId);

                    isp = new IspViewModel
                    {
                        Id = reader["isp_id"].ToString(),
                        Name = reader["isp_name"].ToString(),
                        WhatsappGroup = whatsappId,
                        EmailAddress = reader["isp_emailaddress"].ToString(),
                        WhatsappGroupName = matchingGroup?.Name ?? "Group not found",
                    };
                }
                reader.Close();

                if (isp == null)
                {
                    return new DtoResponse<IspViewModel>
                    {
                        status = 404,
                        message = "ISP not found",
                        data = null,
                    };
                }

                return new DtoResponse<IspViewModel>
                {
                    status = 200,
                    message = "ISP retrieved successfully",
                    data = isp,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<IspViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<IspViewModel>
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

        public DtoResponse<IspViewModel> GetIspByNetworkId(string networkId)
        {
            IspViewModel isp = null;

            try
            {
                var groupsResponse = _helperService.GetGroups();
                var groups = groupsResponse?.data ?? new List<GroupResponse>();

                string query =
                    "SELECT i.isp_id, i.isp_name, i.isp_whatsappgroup, i.isp_emailaddress "
                    + "FROM tbl_m_isp i "
                    + "JOIN tbl_m_network n ON i.isp_id = n.isp_id "
                    + "WHERE n.net_id = @NetworkId";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@NetworkId", networkId);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string whatsappId = reader["isp_whatsappgroup"].ToString();
                    var matchingGroup = groups.FirstOrDefault(g => g.Id == whatsappId);

                    isp = new IspViewModel
                    {
                        Id = reader["isp_id"].ToString(),
                        Name = reader["isp_name"].ToString(),
                        WhatsappGroup = whatsappId,
                        EmailAddress = reader["isp_emailaddress"].ToString(),
                        WhatsappGroupName = matchingGroup?.Name ?? "Group not found",
                    };
                }
                reader.Close();

                if (isp == null)
                {
                    return new DtoResponse<IspViewModel>
                    {
                        status = 404,
                        message = "ISP not found for this network",
                        data = null,
                    };
                }

                return new DtoResponse<IspViewModel>
                {
                    status = 200,
                    message = "ISP retrieved successfully",
                    data = isp,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<IspViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<IspViewModel>
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

        public DtoResponse<IspViewModel> AddIsp(Isp isp, string createdBy)
        {
            try
            {
                string newId = Guid.NewGuid().ToString();
                string query =
                    "INSERT INTO tbl_m_isp (isp_id, isp_name, isp_whatsappgroup, isp_emailaddress, isp_createdby, isp_createddate) "
                    + "VALUES (@Id, @Name, @WhatsappGroup, @EmailAddress, @CreatedBy, @CreatedDate)";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", newId);
                command.Parameters.AddWithValue("@Name", isp.Name);
                command.Parameters.AddWithValue("@WhatsappGroup", isp.WhatsappGroup);
                command.Parameters.AddWithValue("@EmailAddress", isp.EmailAddress);
                command.Parameters.AddWithValue("@CreatedBy", createdBy);
                command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    var ispViewModel = new IspViewModel
                    {
                        Id = newId,
                        Name = isp.Name,
                        WhatsappGroup = isp.WhatsappGroup,
                        EmailAddress = isp.EmailAddress,
                        WhatsappGroupName = "Group not found", // Will be updated by GetGroups if needed
                    };

                    return new DtoResponse<IspViewModel>
                    {
                        status = 201,
                        message = "ISP data saved successfully",
                        data = ispViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<IspViewModel>
                    {
                        status = 500,
                        message = "Failed to save data ISP",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<IspViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to save data ISP",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<IspViewModel>
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

        public DtoResponse<IspViewModel> UpdateIsp(Isp isp, string updatedBy)
        {
            try
            {
                string query =
                    "UPDATE tbl_m_isp "
                    + "SET isp_name = @Name, isp_whatsappgroup = @WhatsappGroup, isp_emailaddress = @EmailAddress, isp_updatedby = @UpdatedBy, isp_updateddate = @UpdatedDate "
                    + "WHERE isp_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", isp.Id);
                command.Parameters.AddWithValue("@Name", isp.Name);
                command.Parameters.AddWithValue("@WhatsappGroup", isp.WhatsappGroup);
                command.Parameters.AddWithValue("@EmailAddress", isp.EmailAddress);
                command.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    var ispViewModel = new IspViewModel
                    {
                        Id = isp.Id,
                        Name = isp.Name,
                        WhatsappGroup = isp.WhatsappGroup,
                        EmailAddress = isp.EmailAddress,
                        WhatsappGroupName = "Group not found",
                    };

                    return new DtoResponse<IspViewModel>
                    {
                        status = 200,
                        message = "ISP data saved successfully",
                        data = ispViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<IspViewModel>
                    {
                        status = 404,
                        message = "ISP not found",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<IspViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update ISP",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<IspViewModel>
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

        public DtoResponse<object> DeleteIsp(string id)
        {
            try
            {
                string query = "DELETE FROM tbl_m_isp WHERE isp_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return new DtoResponse<object>
                    {
                        status = 200,
                        message = "ISP data deleted successfully",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 404,
                        message = "ISP not found or already deleted",
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
                            "Cannot delete ISP because it is referenced by existing Network(s) or Email Address. Please remove all associated data with this ISP first.",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 500,
                        message = "SQL Error: Failed to delete ISP",
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
    }
}
