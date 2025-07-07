using Microsoft.Data.SqlClient;
using NDMNS_API.Models;
using NDMNS_API.Responses;

namespace NDMNS_API.Repositories
{
    public class NetworkRepository
    {
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public NetworkRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        public DtoResponse<List<NetworkViewModel>> GetAllNetworks()
        {
            List<NetworkViewModel> networks = new List<NetworkViewModel>();

            try
            {
                string query =
                    @"
                    SELECT 
                        n.net_id, n.net_name, n.net_ip, n.net_latency, 
                        n.net_status, n.net_last_update, n.sit_id, n.isp_id, n.net_cid,
                        s.sit_name,
                        s.sit_location,
                        i.isp_name
                    FROM tbl_m_network n
                    LEFT JOIN tbl_m_site s ON n.sit_id = s.sit_id
                    LEFT JOIN tbl_m_isp i ON n.isp_id = i.isp_id
                    ORDER BY n.net_name ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    NetworkViewModel network = new NetworkViewModel
                    {
                        Id = reader["net_id"].ToString(),
                        Name = reader["net_name"].ToString(),
                        Ip = reader["net_ip"].ToString(),
                        Latency = Convert.ToDecimal(reader["net_latency"]),
                        Status = Convert.ToInt32(reader["net_status"]),
                        LastUpdate = Convert.ToDateTime(reader["net_last_update"]),
                        SiteId = reader["sit_id"].ToString(),
                        IspId = reader["isp_id"].ToString(),
                        Cid = reader["net_cid"].ToString(),
                        SiteName = reader["sit_name"]?.ToString() ?? "",
                        SiteLocation = reader["sit_location"]?.ToString() ?? "",
                        IspName = reader["isp_name"]?.ToString() ?? "",
                    };
                    networks.Add(network);
                }
                reader.Close();

                return new DtoResponse<List<NetworkViewModel>>
                {
                    status = 200,
                    message = "Networks retrieved successfully",
                    data = networks,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<NetworkViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<NetworkViewModel>>
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

        public DtoResponse<NetworkViewModel> GetNetworkById(string id)
        {
            NetworkViewModel network = null;

            try
            {
                string query =
                    @"
                    SELECT 
                        n.net_id, n.net_name, n.net_ip, n.net_status, n.net_latency, 
                        n.net_last_update, n.sit_id, n.isp_id, n.net_cid,
                        s.sit_name,
                        s.sit_location,
                        i.isp_name
                    FROM tbl_m_network n
                    LEFT JOIN tbl_m_site s ON n.sit_id = s.sit_id
                    LEFT JOIN tbl_m_isp i ON n.isp_id = i.isp_id
                    WHERE n.net_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    network = new NetworkViewModel
                    {
                        Id = reader["net_id"].ToString(),
                        Name = reader["net_name"].ToString(),
                        Ip = reader["net_ip"].ToString(),
                        Latency = Convert.ToDecimal(reader["net_latency"]),
                        Status = Convert.ToInt32(reader["net_status"]),
                        LastUpdate = Convert.ToDateTime(reader["net_last_update"]),
                        SiteId = reader["sit_id"].ToString(),
                        IspId = reader["isp_id"].ToString(),
                        Cid = reader["net_cid"].ToString(),
                        SiteName = reader["sit_name"]?.ToString() ?? "",
                        SiteLocation = reader["sit_location"]?.ToString() ?? "",
                        IspName = reader["isp_name"]?.ToString() ?? "",
                    };
                }
                reader.Close();

                if (network == null)
                {
                    return new DtoResponse<NetworkViewModel>
                    {
                        status = 404,
                        message = "Network not found",
                        data = null,
                    };
                }

                return new DtoResponse<NetworkViewModel>
                {
                    status = 200,
                    message = "Network retrieved successfully",
                    data = network,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<NetworkViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<NetworkViewModel>
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

        public DtoResponse<List<NetworkViewModel>> GetNetworksBySite(string siteId)
        {
            List<NetworkViewModel> networks = new List<NetworkViewModel>();

            try
            {
                string query =
                    @"
                    SELECT 
                        n.net_id, n.net_name, n.net_ip, n.net_latency, 
                        n.net_status, n.net_last_update, n.sit_id, n.isp_id, n.net_cid,
                        s.sit_name,
                        s.sit_location,
                        i.isp_name
                    FROM tbl_m_network n
                    LEFT JOIN tbl_m_site s ON n.sit_id = s.sit_id
                    LEFT JOIN tbl_m_isp i ON n.isp_id = i.isp_id
                    WHERE n.sit_id = @SiteId
                    ORDER BY n.net_name ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@SiteId", siteId);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    NetworkViewModel network = new NetworkViewModel
                    {
                        Id = reader["net_id"].ToString(),
                        Name = reader["net_name"].ToString(),
                        Ip = reader["net_ip"].ToString(),
                        Latency = Convert.ToDecimal(reader["net_latency"]),
                        Status = Convert.ToInt32(reader["net_status"]),
                        LastUpdate = Convert.ToDateTime(reader["net_last_update"]),
                        SiteId = reader["sit_id"].ToString(),
                        IspId = reader["isp_id"].ToString(),
                        Cid = reader["net_cid"].ToString(),
                        SiteName = reader["sit_name"]?.ToString() ?? "",
                        SiteLocation = reader["sit_location"]?.ToString() ?? "",
                        IspName = reader["isp_name"]?.ToString() ?? "",
                    };
                    networks.Add(network);
                }
                reader.Close();

                return new DtoResponse<List<NetworkViewModel>>
                {
                    status = 200,
                    message = "Networks retrieved successfully",
                    data = networks,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<NetworkViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<NetworkViewModel>>
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

        public DtoResponse<List<NetworkViewModel>> GetNetworksByIsp(string ispId)
        {
            List<NetworkViewModel> networks = new List<NetworkViewModel>();

            try
            {
                string query =
                    @"
                    SELECT 
                        n.net_id, n.net_name, n.net_ip, n.net_latency, 
                        n.net_status, n.net_last_update, n.sit_id, n.isp_id, n.net_cid,
                        s.sit_name,
                        s.sit_location,
                        i.isp_name
                    FROM tbl_m_network n
                    LEFT JOIN tbl_m_site s ON n.sit_id = s.sit_id
                    LEFT JOIN tbl_m_isp i ON n.isp_id = i.isp_id
                    WHERE n.isp_id = @IspId
                    ORDER BY n.net_name ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@IspId", ispId);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    NetworkViewModel network = new NetworkViewModel
                    {
                        Id = reader["net_id"].ToString(),
                        Name = reader["net_name"].ToString(),
                        Ip = reader["net_ip"].ToString(),
                        Latency = Convert.ToDecimal(reader["net_latency"]),
                        Status = Convert.ToInt32(reader["net_status"]),
                        LastUpdate = Convert.ToDateTime(reader["net_last_update"]),
                        SiteId = reader["sit_id"].ToString(),
                        IspId = reader["isp_id"].ToString(),
                        Cid = reader["net_cid"].ToString(),
                        SiteName = reader["sit_name"]?.ToString() ?? "",
                        SiteLocation = reader["sit_location"]?.ToString() ?? "",
                        IspName = reader["isp_name"]?.ToString() ?? "",
                    };
                    networks.Add(network);
                }
                reader.Close();

                return new DtoResponse<List<NetworkViewModel>>
                {
                    status = 200,
                    message = "Networks retrieved successfully",
                    data = networks,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<NetworkViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<NetworkViewModel>>
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

        public DtoResponse<NetworkViewModel> AddNetwork(Network network, string createdBy)
        {
            try
            {
                string newId = Guid.NewGuid().ToString();
                string query =
                    "INSERT INTO tbl_m_network (net_id, net_name, net_ip, net_status, sit_id, isp_id, net_cid, net_latency, net_last_update, net_createdby, net_createddate) "
                    + "VALUES (@Id, @Name, @Ip, @Status, @SiteId, @IspId, @Cid, @Latency, @LastUpdate, @CreatedBy, @CreatedDate)";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", newId);
                command.Parameters.AddWithValue("@Name", network.Name);
                command.Parameters.AddWithValue("@Ip", network.Ip);
                command.Parameters.AddWithValue("@Status", network.Status);
                command.Parameters.AddWithValue("@SiteId", network.SiteId);
                command.Parameters.AddWithValue("@IspId", network.IspId);
                command.Parameters.AddWithValue("@Cid", network.Cid);
                command.Parameters.AddWithValue("@Latency", network.Latency);
                command.Parameters.AddWithValue("@LastUpdate", network.LastUpdate);
                command.Parameters.AddWithValue("@CreatedBy", createdBy);
                command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    network.Id = newId;
                    var networkViewModel = new NetworkViewModel
                    {
                        Id = newId,
                        Name = network.Name,
                        Ip = network.Ip,
                        Status = network.Status,
                        SiteId = network.SiteId,
                        IspId = network.IspId,
                        Cid = network.Cid,
                        Latency = network.Latency,
                        LastUpdate = network.LastUpdate,
                        SiteName = "Site not found", // Will be updated if needed
                        IspName = "ISP not found", // Will be updated if needed
                    };

                    return new DtoResponse<NetworkViewModel>
                    {
                        status = 201,
                        message = "Network data saved successfully",
                        data = networkViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<NetworkViewModel>
                    {
                        status = 500,
                        message = "Failed to save data Network",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<NetworkViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to save data Network",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<NetworkViewModel>
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

        public DtoResponse<NetworkViewModel> UpdateNetwork(Network network, string updatedBy)
        {
            try
            {
                string query =
                    "UPDATE tbl_m_network "
                    + "SET net_name = @Name, net_ip = @Ip, net_status = @Status, sit_id = @SiteId, isp_id = @IspId, net_cid = @Cid, net_updatedby = @UpdatedBy, net_updateddate = @UpdatedDate "
                    + "WHERE net_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", network.Id);
                command.Parameters.AddWithValue("@Name", network.Name);
                command.Parameters.AddWithValue("@Ip", network.Ip);
                command.Parameters.AddWithValue("@Status", network.Status);
                command.Parameters.AddWithValue("@SiteId", network.SiteId);
                command.Parameters.AddWithValue("@IspId", network.IspId);
                command.Parameters.AddWithValue("@Cid", network.Cid);
                command.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    var networkViewModel = new NetworkViewModel
                    {
                        Id = network.Id,
                        Name = network.Name,
                        Ip = network.Ip,
                        Status = network.Status,
                        SiteId = network.SiteId,
                        IspId = network.IspId,
                        Cid = network.Cid,
                        SiteName = "Site not found",
                        IspName = "ISP not found",
                    };

                    return new DtoResponse<NetworkViewModel>
                    {
                        status = 200,
                        message = "Network data saved successfully",
                        data = networkViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<NetworkViewModel>
                    {
                        status = 404,
                        message = "Network not found",
                        data = null,
                    };
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                return new DtoResponse<NetworkViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update Network",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<NetworkViewModel>
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

        public DtoResponse<NetworkViewModel> UpdateNetworkPing(Network network)
        {
            try
            {
                string query =
                    "UPDATE tbl_m_network "
                    + "SET net_status = @Status, net_latency = @Latency, net_last_update = @LastUpdate "
                    + "WHERE net_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", network.Id);
                command.Parameters.AddWithValue("@Status", network.Status);
                command.Parameters.AddWithValue("@Latency", network.Latency);
                command.Parameters.AddWithValue("@LastUpdate", network.LastUpdate);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    var networkViewModel = new NetworkViewModel
                    {
                        Id = network.Id,
                        Status = network.Status,
                        Latency = network.Latency,
                        LastUpdate = network.LastUpdate,
                    };

                    return new DtoResponse<NetworkViewModel>
                    {
                        status = 200,
                        message = "Network ping data saved successfully",
                        data = networkViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<NetworkViewModel>
                    {
                        status = 404,
                        message = "Network not found",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<NetworkViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update Network ping",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<NetworkViewModel>
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

        public DtoResponse<object> DeleteNetwork(string id)
        {
            try
            {
                string query = "DELETE FROM tbl_m_network WHERE net_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return new DtoResponse<object>
                    {
                        status = 200,
                        message = "Network data deleted successfully",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 404,
                        message = "Network not found or already deleted",
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
                            "Cannot delete Network because it is referenced by existing Downtime(s), Message(s) or Email(s). Please remove all data associated with this Network first.",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 500,
                        message = "SQL Error: Failed to delete Network",
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

        public bool IsSiteExists(string siteId)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM tbl_m_site WHERE sit_id = @SiteId";
                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@SiteId", siteId);

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
