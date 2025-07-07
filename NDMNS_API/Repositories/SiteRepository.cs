using Microsoft.Data.SqlClient;
using NDMNS_API.Models;
using NDMNS_API.Responses;
using NDMNS_API.Services;

namespace NDMNS_API.Repositories
{
    public class SiteRepository
    {
        private readonly string _connectionString;
        private readonly SqlConnection _connection;
        private readonly HelperService _helperService;

        public SiteRepository(IConfiguration configuration, HelperService helperService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
            _helperService = helperService;
        }

        public DtoResponse<List<SiteViewModel>> GetAllSites()
        {
            List<SiteViewModel> sites = new List<SiteViewModel>();

            try
            {
                var groupsResponse = _helperService.GetGroups();
                var groups = groupsResponse?.data ?? new List<GroupResponse>();

                string query =
                    "SELECT sit_id, sit_name, sit_whatsappgroup, sit_location "
                    + "FROM tbl_m_site "
                    + "ORDER BY sit_name ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string whatsappId = reader["sit_whatsappgroup"].ToString();
                    var matchingGroup = groups.FirstOrDefault(g => g.Id == whatsappId);

                    SiteViewModel site = new SiteViewModel
                    {
                        Id = reader["sit_id"].ToString(),
                        Name = reader["sit_name"].ToString(),
                        WhatsappGroup = whatsappId,
                        WhatsappGroupName = matchingGroup?.Name ?? "Group not found",
                        Location = reader["sit_location"].ToString(),
                    };
                    sites.Add(site);
                }
                reader.Close();

                return new DtoResponse<List<SiteViewModel>>
                {
                    status = 200,
                    message = "Sites retrieved successfully",
                    data = sites,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<SiteViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<SiteViewModel>>
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

        public DtoResponse<SiteViewModel> GetSiteById(string id)
        {
            SiteViewModel site = null;

            try
            {
                var groupsResponse = _helperService.GetGroups();
                var groups = groupsResponse?.data ?? new List<GroupResponse>();

                string query =
                    "SELECT sit_id, sit_name, sit_whatsappgroup, sit_location "
                    + "FROM tbl_m_site "
                    + "WHERE sit_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string whatsappId = reader["sit_whatsappgroup"].ToString();
                    var matchingGroup = groups.FirstOrDefault(g => g.Id == whatsappId);

                    site = new SiteViewModel
                    {
                        Id = reader["sit_id"].ToString(),
                        Name = reader["sit_name"].ToString(),
                        WhatsappGroup = whatsappId,
                        WhatsappGroupName = matchingGroup?.Name ?? "Group not found",
                        Location = reader["sit_location"].ToString(),
                    };
                }
                reader.Close();

                if (site == null)
                {
                    return new DtoResponse<SiteViewModel>
                    {
                        status = 404,
                        message = "Site not found",
                        data = null,
                    };
                }

                return new DtoResponse<SiteViewModel>
                {
                    status = 200,
                    message = "Site retrieved successfully",
                    data = site,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<SiteViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<SiteViewModel>
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

        public DtoResponse<SiteViewModel> AddSite(Site site, string createdBy)
        {
            try
            {
                string newId = Guid.NewGuid().ToString();
                string query =
                    "INSERT INTO tbl_m_site (sit_id, sit_name, sit_whatsappgroup, sit_location, sit_createdby, sit_createddate) "
                    + "VALUES (@Id, @Name, @WhatsappGroup, @Location, @CreatedBy, @CreatedDate)";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", newId);
                command.Parameters.AddWithValue("@Name", site.Name);
                command.Parameters.AddWithValue("@WhatsappGroup", site.WhatsappGroup);
                command.Parameters.AddWithValue("@Location", site.Location);
                command.Parameters.AddWithValue("@CreatedBy", createdBy);
                command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    site.Id = newId;
                    var siteViewModel = new SiteViewModel
                    {
                        Id = newId,
                        Name = site.Name,
                        WhatsappGroup = site.WhatsappGroup,
                        Location = site.Location,
                        WhatsappGroupName = "Group not found", // Will be updated by GetGroups if needed
                    };

                    return new DtoResponse<SiteViewModel>
                    {
                        status = 201,
                        message = "Site data saved successfully",
                        data = siteViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<SiteViewModel>
                    {
                        status = 500,
                        message = "Failed to save data site",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<SiteViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to save data site",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<SiteViewModel>
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

        public DtoResponse<SiteViewModel> UpdateSite(Site site, string updatedBy)
        {
            try
            {
                string query =
                    "UPDATE tbl_m_site "
                    + "SET sit_name = @Name, sit_whatsappgroup = @WhatsappGroup, sit_location = @Location, sit_updatedby = @UpdatedBy, sit_updateddate = @UpdatedDate "
                    + "WHERE sit_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", site.Id);
                command.Parameters.AddWithValue("@Name", site.Name);
                command.Parameters.AddWithValue("@WhatsappGroup", site.WhatsappGroup);
                command.Parameters.AddWithValue("@Location", site.Location);
                command.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    var siteViewModel = new SiteViewModel
                    {
                        Id = site.Id,
                        Name = site.Name,
                        WhatsappGroup = site.WhatsappGroup,
                        Location = site.Location,
                        WhatsappGroupName = "Group not found",
                    };

                    return new DtoResponse<SiteViewModel>
                    {
                        status = 200,
                        message = "Site data saved successfully",
                        data = siteViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<SiteViewModel>
                    {
                        status = 404,
                        message = "Site not found",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<SiteViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update site",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<SiteViewModel>
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

        public DtoResponse<object> DeleteSite(string id)
        {
            try
            {
                string query = "DELETE FROM tbl_m_site WHERE sit_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return new DtoResponse<object>
                    {
                        status = 200,
                        message = "Site data deleted successfully",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 404,
                        message = "Site not found or already deleted",
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
                            "Cannot delete Site because it is referenced by existing Network(s) or PIC(s). Please remove all Networks and PICs associated with this Site first.",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 500,
                        message = "SQL Error: Failed to delete site",
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
