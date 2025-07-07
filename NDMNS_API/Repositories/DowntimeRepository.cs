using Microsoft.Data.SqlClient;
using NDMNS_API.Models;
using NDMNS_API.Responses;

namespace NDMNS_API.Repositories
{
    public class DowntimeRepository
    {
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public DowntimeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        public DtoResponse<List<DowntimeViewModel>> GetAllDowntimes()
        {
            List<DowntimeViewModel> downtimes = new List<DowntimeViewModel>();

            try
            {
                string query =
                    "SELECT d.dtm_id, d.net_id, d.dtm_description, d.dtm_ticketnumber, d.dtm_date, d.dtm_start, "
                    + "d.dtm_end, d.dtm_category, d.dtm_subcategory, d.dtm_action, d.dtm_status, "
                    + "n.net_name, n.sit_id, n.isp_id, s.sit_name, i.isp_name "
                    + "FROM tbl_t_downtime d "
                    + "LEFT JOIN tbl_m_network n ON d.net_id = n.net_id "
                    + "LEFT JOIN tbl_m_site s ON n.sit_id = s.sit_id "
                    + "LEFT JOIN tbl_m_isp i ON n.isp_id = i.isp_id "
                    + "ORDER BY d.dtm_start DESC";

                SqlCommand command = new SqlCommand(query, _connection);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DowntimeViewModel downtime = new DowntimeViewModel
                    {
                        Id = reader["dtm_id"].ToString(),
                        NetworkId = reader["net_id"].ToString(),
                        NetworkName = reader["net_name"]?.ToString() ?? "Network not found",
                        SiteId = reader["sit_id"]?.ToString(),
                        SiteName = reader["sit_name"]?.ToString() ?? "Site not found",
                        IspId = reader["isp_id"]?.ToString(),
                        IspName = reader["isp_name"]?.ToString() ?? "ISP not found",
                        Description = reader["dtm_description"].ToString(),
                        TicketNumber = reader["dtm_ticketnumber"].ToString(),
                        Date = Convert.ToDateTime(reader["dtm_date"]),
                        Start = Convert.ToDateTime(reader["dtm_start"]),
                        End =
                            reader["dtm_end"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["dtm_end"]),
                        Category = Convert.ToInt32(reader["dtm_category"]),
                        Subcategory =
                            reader["dtm_subcategory"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(reader["dtm_subcategory"]),
                        Action = reader["dtm_action"]?.ToString(),
                        Status = Convert.ToInt32(reader["dtm_status"]),
                    };
                    downtimes.Add(downtime);
                }
                reader.Close();

                return new DtoResponse<List<DowntimeViewModel>>
                {
                    status = 200,
                    message = "Downtimes retrieved successfully",
                    data = downtimes,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<DowntimeViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<DowntimeViewModel>>
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

        public DtoResponse<DowntimeViewModel> GetDowntimeById(string id)
        {
            DowntimeViewModel downtime = null;

            try
            {
                string query =
                    "SELECT d.dtm_id, d.net_id, d.dtm_description, d.dtm_ticketnumber, d.dtm_date, d.dtm_start, "
                    + "d.dtm_end, d.dtm_category, d.dtm_subcategory, d.dtm_action, d.dtm_status, "
                    + "n.net_name, n.sit_id, n.isp_id, s.sit_name, i.isp_name "
                    + "FROM tbl_t_downtime d "
                    + "LEFT JOIN tbl_m_network n ON d.net_id = n.net_id "
                    + "LEFT JOIN tbl_m_site s ON n.sit_id = s.sit_id "
                    + "LEFT JOIN tbl_m_isp i ON n.isp_id = i.isp_id "
                    + "WHERE d.dtm_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    downtime = new DowntimeViewModel
                    {
                        Id = reader["dtm_id"].ToString(),
                        NetworkId = reader["net_id"].ToString(),
                        NetworkName = reader["net_name"]?.ToString() ?? "Network not found",
                        SiteId = reader["sit_id"]?.ToString(),
                        SiteName = reader["sit_name"]?.ToString() ?? "Site not found",
                        IspId = reader["isp_id"]?.ToString(),
                        IspName = reader["isp_name"]?.ToString() ?? "ISP not found",
                        Description = reader["dtm_description"].ToString(),
                        TicketNumber = reader["dtm_ticketnumber"].ToString(),
                        Date = Convert.ToDateTime(reader["dtm_date"]),
                        Start = Convert.ToDateTime(reader["dtm_start"]),
                        End =
                            reader["dtm_end"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["dtm_end"]),
                        Category = Convert.ToInt32(reader["dtm_category"]),
                        Subcategory = Convert.ToInt32(reader["dtm_subcategory"]),
                        Action = reader["dtm_action"]?.ToString(),
                        Status = Convert.ToInt32(reader["dtm_status"]),
                    };
                }
                reader.Close();

                if (downtime == null)
                {
                    return new DtoResponse<DowntimeViewModel>
                    {
                        status = 404,
                        message = "Downtime not found",
                        data = null,
                    };
                }

                return new DtoResponse<DowntimeViewModel>
                {
                    status = 200,
                    message = "Downtime retrieved successfully",
                    data = downtime,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<DowntimeViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<DowntimeViewModel>
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

        public DtoResponse<Downtime> GetDowntimeByIdCrea(string id)
        {
            Downtime downtime = null;

            try
            {
                string query =
                    "SELECT d.dtm_id, d.net_id, d.dtm_description, d.dtm_ticketnumber, d.dtm_date, d.dtm_start, "
                    + "d.dtm_end, d.dtm_category, d.dtm_subcategory, d.dtm_action, d.dtm_status, d.dtm_createdby "
                    + "FROM tbl_t_downtime d "
                    + "WHERE d.dtm_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    downtime = new Downtime
                    {
                        Id = reader["dtm_id"].ToString(),
                        NetworkId = reader["net_id"].ToString(),
                        Description = reader["dtm_description"].ToString(),
                        TicketNumber = reader["dtm_ticketnumber"].ToString(),
                        Date = Convert.ToDateTime(reader["dtm_date"]),
                        Start = Convert.ToDateTime(reader["dtm_start"]),
                        End =
                            reader["dtm_end"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["dtm_end"]),
                        Category = Convert.ToInt32(reader["dtm_category"]),
                        Subcategory = Convert.ToInt32(reader["dtm_subcategory"]),
                        Action = reader["dtm_action"]?.ToString(),
                        Status = Convert.ToInt32(reader["dtm_status"]),
                        CreatedBy = reader["dtm_createdby"].ToString(),
                    };
                }
                reader.Close();

                if (downtime == null)
                {
                    return new DtoResponse<Downtime>
                    {
                        status = 404,
                        message = "Downtime not found",
                        data = null,
                    };
                }

                return new DtoResponse<Downtime>
                {
                    status = 200,
                    message = "Downtime retrieved successfully",
                    data = downtime,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<Downtime>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<Downtime>
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

        public DtoResponse<List<DowntimeViewModel>> GetDowntimesByNetworkId(string networkId)
        {
            List<DowntimeViewModel> downtimes = new List<DowntimeViewModel>();

            try
            {
                string query =
                    "SELECT d.dtm_id, d.net_id, d.dtm_description, d.dtm_ticketnumber, d.dtm_date, d.dtm_start, "
                    + "d.dtm_end, d.dtm_category, d.dtm_subcategory, d.dtm_action, d.dtm_status, "
                    + "n.net_name, n.sit_id, n.isp_id, s.sit_name, i.isp_name "
                    + "FROM tbl_t_downtime d "
                    + "LEFT JOIN tbl_m_network n ON d.net_id = n.net_id "
                    + "LEFT JOIN tbl_m_site s ON n.sit_id = s.sit_id "
                    + "LEFT JOIN tbl_m_isp i ON n.isp_id = i.isp_id "
                    + "WHERE d.net_id = @NetworkId "
                    + "ORDER BY d.dtm_start DESC";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@NetworkId", networkId);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DowntimeViewModel downtime = new DowntimeViewModel
                    {
                        Id = reader["dtm_id"].ToString(),
                        NetworkId = reader["net_id"].ToString(),
                        NetworkName = reader["net_name"]?.ToString() ?? "Network not found",
                        SiteId = reader["sit_id"]?.ToString(),
                        SiteName = reader["sit_name"]?.ToString() ?? "Site not found",
                        IspId = reader["isp_id"]?.ToString(),
                        IspName = reader["isp_name"]?.ToString() ?? "ISP not found",
                        Description = reader["dtm_description"].ToString(),
                        TicketNumber = reader["dtm_ticketnumber"].ToString(),
                        Date = Convert.ToDateTime(reader["dtm_date"]),
                        Start = Convert.ToDateTime(reader["dtm_start"]),
                        End =
                            reader["dtm_end"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["dtm_end"]),
                        Category = Convert.ToInt32(reader["dtm_category"]),
                        Subcategory = Convert.ToInt32(reader["dtm_subcategory"]),
                        Action = reader["dtm_action"]?.ToString(),
                        Status = Convert.ToInt32(reader["dtm_status"]),
                    };
                    downtimes.Add(downtime);
                }
                reader.Close();

                return new DtoResponse<List<DowntimeViewModel>>
                {
                    status = 200,
                    message = "Downtimes retrieved successfully",
                    data = downtimes,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<DowntimeViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<DowntimeViewModel>>
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

        public DtoResponse<DowntimeViewModel> GetLatestDowntimeByNetworkId(string networkId)
        {
            DowntimeViewModel latestDowntime = null;

            try
            {
                string query =
                    "SELECT TOP 1 d.dtm_id, d.net_id, d.dtm_description, d.dtm_ticketnumber, d.dtm_date, d.dtm_start, "
                    + "d.dtm_end, d.dtm_category, d.dtm_subcategory, d.dtm_action, d.dtm_status, "
                    + "n.net_name, n.sit_id, n.isp_id, s.sit_name, i.isp_name "
                    + "FROM tbl_t_downtime d "
                    + "LEFT JOIN tbl_m_network n ON d.net_id = n.net_id "
                    + "LEFT JOIN tbl_m_site s ON n.sit_id = s.sit_id "
                    + "LEFT JOIN tbl_m_isp i ON n.isp_id = i.isp_id "
                    + "WHERE d.net_id = @NetworkId "
                    + "ORDER BY d.dtm_date DESC, d.dtm_start DESC";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@NetworkId", networkId);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    latestDowntime = new DowntimeViewModel
                    {
                        Id = reader["dtm_id"].ToString(),
                        NetworkId = reader["net_id"].ToString(),
                        NetworkName = reader["net_name"]?.ToString() ?? "Network not found",
                        SiteId = reader["sit_id"]?.ToString(),
                        SiteName = reader["sit_name"]?.ToString() ?? "Site not found",
                        IspId = reader["isp_id"]?.ToString(),
                        IspName = reader["isp_name"]?.ToString() ?? "ISP not found",
                        Description = reader["dtm_description"].ToString(),
                        TicketNumber = reader["dtm_ticketnumber"].ToString(),
                        Date = Convert.ToDateTime(reader["dtm_date"]),
                        Start = Convert.ToDateTime(reader["dtm_start"]),
                        End =
                            reader["dtm_end"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["dtm_end"]),
                        Category = Convert.ToInt32(reader["dtm_category"]),
                        Subcategory = Convert.ToInt32(reader["dtm_subcategory"]),
                        Action = reader["dtm_action"]?.ToString(),
                        Status = Convert.ToInt32(reader["dtm_status"]),
                    };
                }
                reader.Close();

                if (latestDowntime == null)
                {
                    return new DtoResponse<DowntimeViewModel>
                    {
                        status = 404,
                        message = "No downtime found for this network",
                        data = null,
                    };
                }

                return new DtoResponse<DowntimeViewModel>
                {
                    status = 200,
                    message = "Latest downtime retrieved successfully",
                    data = latestDowntime,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<DowntimeViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<DowntimeViewModel>
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

        public DtoResponse<DowntimeViewModel> GetLatestDowntimeByTicketNumber(string ticketNumber)
        {
            DowntimeViewModel latestDowntime = null;

            try
            {
                string query =
                    "SELECT TOP 1 d.dtm_id, d.net_id, d.dtm_description, d.dtm_ticketnumber, d.dtm_date, d.dtm_start, "
                    + "d.dtm_end, d.dtm_category, d.dtm_subcategory, d.dtm_action, d.dtm_status, "
                    + "n.net_name, n.sit_id, n.isp_id, s.sit_name, i.isp_name "
                    + "FROM tbl_t_downtime d "
                    + "LEFT JOIN tbl_m_network n ON d.net_id = n.net_id "
                    + "LEFT JOIN tbl_m_site s ON n.sit_id = s.sit_id "
                    + "LEFT JOIN tbl_m_isp i ON n.isp_id = i.isp_id "
                    + "WHERE d.dtm_ticketnumber = @TicketNumber "
                    + "ORDER BY d.dtm_date DESC, d.dtm_start DESC";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@TicketNumber", ticketNumber);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    latestDowntime = new DowntimeViewModel
                    {
                        Id = reader["dtm_id"].ToString(),
                        NetworkId = reader["net_id"].ToString(),
                        NetworkName = reader["net_name"]?.ToString() ?? "Network not found",
                        SiteId = reader["sit_id"]?.ToString(),
                        SiteName = reader["sit_name"]?.ToString() ?? "Site not found",
                        IspId = reader["isp_id"]?.ToString(),
                        IspName = reader["isp_name"]?.ToString() ?? "ISP not found",
                        Description = reader["dtm_description"].ToString(),
                        TicketNumber = reader["dtm_ticketnumber"].ToString(),
                        Date = Convert.ToDateTime(reader["dtm_date"]),
                        Start = Convert.ToDateTime(reader["dtm_start"]),
                        End =
                            reader["dtm_end"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["dtm_end"]),
                        Category = Convert.ToInt32(reader["dtm_category"]),
                        Subcategory = Convert.ToInt32(reader["dtm_subcategory"]),
                        Action = reader["dtm_action"]?.ToString(),
                        Status = Convert.ToInt32(reader["dtm_status"]),
                    };
                }
                reader.Close();

                if (latestDowntime == null)
                {
                    return new DtoResponse<DowntimeViewModel>
                    {
                        status = 404,
                        message = "No downtime found for this ticket number",
                        data = null,
                    };
                }

                return new DtoResponse<DowntimeViewModel>
                {
                    status = 200,
                    message = "Latest downtime retrieved successfully",
                    data = latestDowntime,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<DowntimeViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<DowntimeViewModel>
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

        public DtoResponse<DowntimeViewModel> AddDowntime(Downtime downtime, string createdBy)
        {
            try
            {
                string newId = Guid.NewGuid().ToString();
                string query =
                    "INSERT INTO tbl_t_downtime (dtm_id, net_id, dtm_description, dtm_ticketnumber, dtm_date, dtm_start, "
                    + "dtm_category, dtm_status, dtm_createdby, dtm_createddate) "
                    + "VALUES (@Id, @NetworkId, @Description, @TicketNumber, @Date, @Start, @Category, @Status, @CreatedBy, @CreatedDate)";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", newId);
                command.Parameters.AddWithValue("@NetworkId", downtime.NetworkId);
                command.Parameters.AddWithValue("@Description", downtime.Description);
                command.Parameters.AddWithValue("@TicketNumber", downtime.TicketNumber);
                command.Parameters.AddWithValue("@Date", downtime.Date);
                command.Parameters.AddWithValue("@Start", downtime.Start);
                command.Parameters.AddWithValue("@Category", downtime.Category);
                // command.Parameters.AddWithValue("@Subcategory", downtime.Subcategory);
                command.Parameters.AddWithValue("@Status", downtime.Status);
                command.Parameters.AddWithValue("@CreatedBy", createdBy);
                command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    downtime.Id = newId;
                    var downtimeViewModel = new DowntimeViewModel
                    {
                        Id = newId,
                        NetworkId = downtime.NetworkId,
                        NetworkName = "Network not found",
                        Description = downtime.Description,
                        TicketNumber = downtime.TicketNumber,
                        Date = downtime.Date,
                        Start = downtime.Start,
                        End = downtime.End,
                        Category = downtime.Category,
                        Subcategory = downtime.Subcategory,
                        Action = downtime.Action,
                        Status = downtime.Status,
                    };

                    return new DtoResponse<DowntimeViewModel>
                    {
                        status = 201,
                        message = "Downtime data saved successfully",
                        data = downtimeViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<DowntimeViewModel>
                    {
                        status = 500,
                        message = "Failed to save data downtime",
                        data = null,
                    };
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                return new DtoResponse<DowntimeViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to save data downtime",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<DowntimeViewModel>
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

        public DtoResponse<DowntimeViewModel> UpdateDowntime(Downtime downtime, string updatedBy)
        {
            try
            {
                string query =
                    "UPDATE tbl_t_downtime "
                    + "SET net_id = @NetworkId, dtm_description = @Description, dtm_ticketnumber = @TicketNumber, dtm_date = @Date, "
                    + "dtm_start = @Start, dtm_end = @End, "
                    + "dtm_category = @Category, dtm_subcategory = @Subcategory, "
                    + "dtm_action = @Action, dtm_status = @Status, dtm_updatedby = @UpdatedBy, dtm_updateddate = @UpdatedDate "
                    + "WHERE dtm_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", downtime.Id);
                command.Parameters.AddWithValue("@NetworkId", downtime.NetworkId);
                command.Parameters.AddWithValue("@Description", downtime.Description);
                command.Parameters.AddWithValue("@TicketNumber", downtime.TicketNumber);
                command.Parameters.AddWithValue("@Date", downtime.Date);
                command.Parameters.AddWithValue("@Start", downtime.Start);
                command.Parameters.AddWithValue("@End", downtime.End ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Category", downtime.Category);
                command.Parameters.AddWithValue("@Subcategory", downtime.Subcategory);
                command.Parameters.AddWithValue("@Action", downtime.Action ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Status", downtime.Status);
                command.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    var downtimeViewModel = new DowntimeViewModel
                    {
                        Id = downtime.Id,
                        NetworkId = downtime.NetworkId,
                        NetworkName = "Network not found",
                        Description = downtime.Description,
                        TicketNumber = downtime.TicketNumber,
                        Date = downtime.Date,
                        Start = downtime.Start,
                        End = downtime.End,
                        Category = downtime.Category,
                        Subcategory = downtime.Subcategory,
                        Action = downtime.Action,
                        Status = downtime.Status,
                    };

                    return new DtoResponse<DowntimeViewModel>
                    {
                        status = 200,
                        message = "Downtime data saved successfully",
                        data = downtimeViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<DowntimeViewModel>
                    {
                        status = 404,
                        message = "Downtime not found",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<DowntimeViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update downtime",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<DowntimeViewModel>
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

        public DtoResponse<DowntimeViewModel> UpdateDowntimeCategory(
            Downtime downtime,
            string updatedBy
        )
        {
            try
            {
                string query =
                    "UPDATE tbl_t_downtime "
                    + "SET dtm_category = @Category, dtm_subcategory = @Subcategory, "
                    + "dtm_updatedby = @UpdatedBy, dtm_updateddate = @UpdatedDate "
                    + "WHERE dtm_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", downtime.Id);
                command.Parameters.AddWithValue("@Category", downtime.Category);
                command.Parameters.AddWithValue("@Subcategory", downtime.Subcategory);
                command.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    var downtimeViewModel = new DowntimeViewModel
                    {
                        Id = downtime.Id,
                        NetworkId = downtime.NetworkId,
                        NetworkName = "Network not found",
                        Description = downtime.Description,
                        TicketNumber = downtime.TicketNumber,
                        Date = downtime.Date,
                        Start = downtime.Start,
                        End = downtime.End,
                        Category = downtime.Category,
                        Subcategory = downtime.Subcategory,
                        Action = downtime.Action,
                        Status = downtime.Status,
                    };

                    return new DtoResponse<DowntimeViewModel>
                    {
                        status = 200,
                        message = "Downtime category data saved successfully",
                        data = downtimeViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<DowntimeViewModel>
                    {
                        status = 404,
                        message = "Downtime not found",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<DowntimeViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update downtime category",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<DowntimeViewModel>
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

        public DtoResponse<object> DeleteDowntime(string id)
        {
            try
            {
                string query = "DELETE FROM tbl_t_downtime WHERE dtm_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return new DtoResponse<object>
                    {
                        status = 200,
                        message = "Downtime data deleted successfully",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 404,
                        message = "Downtime not found or already deleted",
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
                            "Cannot delete downtime because it is referenced by other records. Please remove all related records first.",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 500,
                        message = "SQL Error: Failed to delete downtime",
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

        public bool IsNetworkExists(string networkId)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM tbl_m_network WHERE net_id = @NetworkId";
                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@NetworkId", networkId);

                _connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
            catch (Exception)
            {
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
