using Microsoft.Data.SqlClient;
using NDMNS_API.Models;
using NDMNS_API.Responses;

namespace NDMNS_API.Repositories
{
    public class PicRepository
    {
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public PicRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        public DtoResponse<List<PicViewModel>> GetAllPics()
        {
            List<PicViewModel> pics = new List<PicViewModel>();

            try
            {
                string query =
                    "SELECT p.pic_id, p.sit_id, p.pic_nrp, p.pic_name, p.pic_role, p.pic_whatsappnumber, p.pic_emailaddress, s.sit_name "
                    + "FROM tbl_m_pic p "
                    + "INNER JOIN tbl_m_site s ON p.sit_id = s.sit_id "
                    + "ORDER BY p.pic_name ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    PicViewModel pic = new PicViewModel
                    {
                        Id = reader["pic_id"].ToString(),
                        SiteId = reader["sit_id"].ToString(),
                        Nrp = reader["pic_nrp"].ToString(),
                        Name = reader["pic_name"].ToString(),
                        Role = Convert.ToInt32(reader["pic_role"]),
                        WhatsappNumber = reader["pic_whatsappnumber"].ToString(),
                        EmailAddress = reader["pic_emailaddress"].ToString(),
                        SiteName = reader["sit_name"].ToString(),
                    };
                    pics.Add(pic);
                }
                reader.Close();

                return new DtoResponse<List<PicViewModel>>
                {
                    status = 200,
                    message = "PICs retrieved successfully",
                    data = pics,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<PicViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<PicViewModel>>
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

        public DtoResponse<PicViewModel> GetPicById(string id)
        {
            PicViewModel pic = null;

            try
            {
                string query =
                    "SELECT p.pic_id, p.sit_id, p.pic_nrp, p.pic_name, p.pic_role, p.pic_whatsappnumber, p.pic_emailaddress, s.sit_name "
                    + "FROM tbl_m_pic p "
                    + "INNER JOIN tbl_m_site s ON p.sit_id = s.sit_id "
                    + "WHERE p.pic_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    pic = new PicViewModel
                    {
                        Id = reader["pic_id"].ToString(),
                        SiteId = reader["sit_id"].ToString(),
                        Nrp = reader["pic_nrp"].ToString(),
                        Name = reader["pic_name"].ToString(),
                        Role = Convert.ToInt32(reader["pic_role"]),
                        WhatsappNumber = reader["pic_whatsappnumber"].ToString(),
                        EmailAddress = reader["pic_emailaddress"].ToString(),
                        SiteName = reader["sit_name"].ToString(),
                    };
                }
                reader.Close();

                if (pic == null)
                {
                    return new DtoResponse<PicViewModel>
                    {
                        status = 404,
                        message = "PIC not found",
                        data = null,
                    };
                }

                return new DtoResponse<PicViewModel>
                {
                    status = 200,
                    message = "PIC retrieved successfully",
                    data = pic,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<PicViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<PicViewModel>
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

        public DtoResponse<List<PicViewModel>> GetPicsBySiteId(string siteId)
        {
            List<PicViewModel> pics = new List<PicViewModel>();

            try
            {
                string query =
                    "SELECT p.pic_id, p.sit_id, p.pic_nrp, p.pic_name, p.pic_role, p.pic_whatsappnumber, p.pic_emailaddress, s.sit_name "
                    + "FROM tbl_m_pic p "
                    + "INNER JOIN tbl_m_site s ON p.sit_id = s.sit_id "
                    + "WHERE p.sit_id = @SiteId "
                    + "ORDER BY p.pic_name ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@SiteId", siteId);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    PicViewModel pic = new PicViewModel
                    {
                        Id = reader["pic_id"].ToString(),
                        SiteId = reader["sit_id"].ToString(),
                        Nrp = reader["pic_nrp"].ToString(),
                        Name = reader["pic_name"].ToString(),
                        Role = Convert.ToInt32(reader["pic_role"]),
                        WhatsappNumber = reader["pic_whatsappnumber"].ToString(),
                        EmailAddress = reader["pic_emailaddress"].ToString(),
                        SiteName = reader["sit_name"].ToString(),
                    };
                    pics.Add(pic);
                }
                reader.Close();

                return new DtoResponse<List<PicViewModel>>
                {
                    status = 200,
                    message = "PICs retrieved successfully",
                    data = pics,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<PicViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<PicViewModel>>
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

        public DtoResponse<List<PicViewModel>> GetPicsByRole(int role)
        {
            List<PicViewModel> pics = new List<PicViewModel>();

            try
            {
                string query =
                    "SELECT p.pic_id, p.sit_id, p.pic_nrp, p.pic_name, p.pic_role, p.pic_whatsappnumber, p.pic_emailaddress, s.sit_name "
                    + "FROM tbl_m_pic p "
                    + "INNER JOIN tbl_m_site s ON p.sit_id = s.sit_id "
                    + "WHERE p.pic_role = @Role "
                    + "ORDER BY p.pic_name ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Role", role);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    PicViewModel pic = new PicViewModel
                    {
                        Id = reader["pic_id"].ToString(),
                        SiteId = reader["sit_id"].ToString(),
                        Nrp = reader["pic_nrp"].ToString(),
                        Name = reader["pic_name"].ToString(),
                        Role = Convert.ToInt32(reader["pic_role"]),
                        WhatsappNumber = reader["pic_whatsappnumber"].ToString(),
                        EmailAddress = reader["pic_emailaddress"].ToString(),
                        SiteName = reader["sit_name"].ToString(),
                    };
                    pics.Add(pic);
                }
                reader.Close();

                return new DtoResponse<List<PicViewModel>>
                {
                    status = 200,
                    message = "PICs retrieved successfully",
                    data = pics,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<PicViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<PicViewModel>>
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

        public DtoResponse<PicViewModel> AddPic(Pic pic, string createdBy)
        {
            try
            {
                string newId = Guid.NewGuid().ToString();
                string query =
                    "INSERT INTO tbl_m_pic (pic_id, sit_id, pic_nrp, pic_name, pic_role, pic_whatsappnumber, pic_emailaddress, pic_createdby, pic_createddate) "
                    + "VALUES (@Id, @SiteId, @Nrp, @Name, @Role, @WhatsappNumber, @EmailAddress, @CreatedBy, @CreatedDate)";

                Console.WriteLine(pic.Nrp);

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", newId);
                command.Parameters.AddWithValue("@SiteId", pic.SiteId);
                command.Parameters.AddWithValue("@Nrp", pic.Nrp);
                command.Parameters.AddWithValue("@Name", pic.Name);
                command.Parameters.AddWithValue("@Role", pic.Role);
                command.Parameters.AddWithValue("@WhatsappNumber", pic.WhatsappNumber);
                command.Parameters.AddWithValue(
                    "@EmailAddress",
                    pic.EmailAddress ?? (object)DBNull.Value
                );
                command.Parameters.AddWithValue("@CreatedBy", createdBy);
                command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    pic.Id = newId;
                    var picViewModel = new PicViewModel
                    {
                        Id = newId,
                        SiteId = pic.SiteId,
                        Nrp = pic.Nrp,
                        Name = pic.Name,
                        Role = pic.Role,
                        WhatsappNumber = pic.WhatsappNumber,
                        EmailAddress = pic.EmailAddress,
                        SiteName = "Site not found",
                    };

                    return new DtoResponse<PicViewModel>
                    {
                        status = 201,
                        message = "PIC data saved successfully",
                        data = picViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<PicViewModel>
                    {
                        status = 500,
                        message = "Failed to save data PIC",
                        data = null,
                    };
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                return new DtoResponse<PicViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to save data PIC",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<PicViewModel>
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

        public DtoResponse<PicViewModel> UpdatePic(Pic pic, string updatedBy)
        {
            try
            {
                string query =
                    "UPDATE tbl_m_pic "
                    + "SET sit_id = @SiteId, pic_nrp = @Nrp, pic_name = @Name, pic_role = @Role, pic_whatsappnumber = @WhatsappNumber, pic_emailaddress = @EmailAddress, pic_updatedby = @UpdatedBy, pic_updateddate = @UpdatedDate "
                    + "WHERE pic_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", pic.Id);
                command.Parameters.AddWithValue("@SiteId", pic.SiteId);
                command.Parameters.AddWithValue("@Nrp", pic.Nrp);
                command.Parameters.AddWithValue("@Name", pic.Name);
                command.Parameters.AddWithValue("@Role", pic.Role);
                command.Parameters.AddWithValue("@WhatsappNumber", pic.WhatsappNumber);
                command.Parameters.AddWithValue(
                    "@EmailAddress",
                    pic.EmailAddress ?? (object)DBNull.Value
                );
                command.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    var picViewModel = new PicViewModel
                    {
                        Id = pic.Id,
                        SiteId = pic.SiteId,
                        Nrp = pic.Nrp,
                        Name = pic.Name,
                        Role = pic.Role,
                        WhatsappNumber = pic.WhatsappNumber,
                        EmailAddress = pic.EmailAddress,
                        SiteName = "Site not found",
                    };

                    return new DtoResponse<PicViewModel>
                    {
                        status = 200,
                        message = "PIC data saved successfully",
                        data = picViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<PicViewModel>
                    {
                        status = 404,
                        message = "PIC not found",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<PicViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update PIC",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<PicViewModel>
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

        public DtoResponse<object> DeletePic(string id)
        {
            try
            {
                string query = "DELETE FROM tbl_m_pic WHERE pic_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return new DtoResponse<object>
                    {
                        status = 200,
                        message = "PIC data deleted successfully",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 404,
                        message = "PIC not found or already deleted",
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
                            "Cannot delete PIC because it is referenced by existing Message(s) or Email(s). Please remove all Messages and Emails associated with this PIC first.",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 500,
                        message = "SQL Error: Failed to delete PIC",
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
    }
}
