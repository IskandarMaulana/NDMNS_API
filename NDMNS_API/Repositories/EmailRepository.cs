using Microsoft.Data.SqlClient;
using NDMNS_API.Models;
using NDMNS_API.Responses;

namespace NDMNS_API.Repositories
{
    public class EmailRepository
    {
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public EmailRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        public DtoResponse<List<EmailViewModel>> GetAllEmails()
        {
            List<EmailViewModel> emails = new List<EmailViewModel>();

            try
            {
                string query =
                    "SELECT e.eml_id, e.dtm_id, e.eml_type, e.eml_subject, e.eml_body, e.eml_image, e.eml_date, d.dtm_description, d.dtm_ticketnumber, eml_status "
                    + "FROM tbl_t_email e "
                    + "INNER JOIN tbl_t_downtime d ON e.dtm_id = d.dtm_id "
                    + "ORDER BY e.eml_date DESC";

                SqlCommand command = new SqlCommand(query, _connection);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    EmailViewModel email = new EmailViewModel
                    {
                        Id = reader["eml_id"].ToString(),
                        DowntimeId = reader["dtm_id"].ToString(),
                        Type = Convert.ToInt32(reader["eml_type"].ToString()),
                        Status = Convert.ToInt32(reader["eml_status"].ToString()),
                        Subject = reader["eml_subject"].ToString(),
                        Body = reader["eml_body"].ToString(),
                        Image = reader["eml_image"].ToString(),
                        Date = Convert.ToDateTime(reader["eml_date"]),
                        DowntimeDescription = reader["dtm_description"].ToString(),
                        DowntimeTicketNumber = reader["dtm_ticketnumber"].ToString(),
                    };
                    emails.Add(email);
                }
                reader.Close();

                foreach (var email in emails)
                {
                    LoadEmailDetails(email);
                }

                return new DtoResponse<List<EmailViewModel>>
                {
                    status = 200,
                    message = "Emails retrieved successfully",
                    data = emails,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<EmailViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<EmailViewModel>>
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

        public DtoResponse<List<EmailViewModel>> GetEmailsByNetworkDown(string networkId)
        {
            List<EmailViewModel> emails = new List<EmailViewModel>();

            try
            {
                string query =
                    "SELECT e.eml_id, e.dtm_id, e.eml_type, e.eml_subject, e.eml_body, e.eml_image, e.eml_date, d.dtm_description "
                    + "FROM tbl_t_email e "
                    + "INNER JOIN tbl_t_downtime d ON e.dtm_id = d.dtm_id "
                    + "INNER JOIN tbl_m_network n ON d.net_id = n.net_id "
                    + "WHERE n.net_id = @NetworkId "
                    + "AND n.net_status = 1 "
                    + "AND d.dtm_status = 1 "
                    + "AND d.dtm_start = n.net_last_update "
                    + "ORDER BY e.eml_date DESC";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@NetworkId", networkId);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    EmailViewModel email = new EmailViewModel
                    {
                        Id = reader["eml_id"].ToString(),
                        DowntimeId = reader["dtm_id"].ToString(),
                        Type = Convert.ToInt32(reader["eml_type"].ToString()),
                        Subject = reader["eml_subject"].ToString(),
                        Body = reader["eml_body"].ToString(),
                        Image = reader["eml_image"].ToString(),
                        Date = Convert.ToDateTime(reader["eml_date"]),
                        DowntimeDescription = reader["dtm_description"].ToString(),
                    };
                    emails.Add(email);
                }
                reader.Close();

                // Load detail emails for each email
                foreach (var email in emails)
                {
                    LoadEmailDetails(email);
                }

                return new DtoResponse<List<EmailViewModel>>
                {
                    status = 200,
                    message = "Emails retrieved successfully",
                    data = emails,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<EmailViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<EmailViewModel>>
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

        public DtoResponse<EmailViewModel> GetEmailById(string id)
        {
            EmailViewModel email = null;

            try
            {
                string query =
                    "SELECT e.eml_id, e.dtm_id, e.eml_type, e.eml_subject, e.eml_body, e.eml_image, e.eml_date, d.dtm_description "
                    + "FROM tbl_t_email e "
                    + "INNER JOIN tbl_t_downtime d ON e.dtm_id = d.dtm_id "
                    + "WHERE e.eml_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    email = new EmailViewModel
                    {
                        Id = reader["eml_id"].ToString(),
                        DowntimeId = reader["dtm_id"].ToString(),
                        Type = Convert.ToInt32(reader["eml_type"].ToString()),
                        Subject = reader["eml_subject"].ToString(),
                        Body = reader["eml_body"].ToString(),
                        Image = reader["eml_image"].ToString(),
                        Date = Convert.ToDateTime(reader["eml_date"]),
                        DowntimeDescription = reader["dtm_description"].ToString(),
                    };
                }
                reader.Close();

                if (email == null)
                {
                    return new DtoResponse<EmailViewModel>
                    {
                        status = 404,
                        message = "Email not found",
                        data = null,
                    };
                }

                // Load detail emails
                LoadEmailDetails(email);

                return new DtoResponse<EmailViewModel>
                {
                    status = 200,
                    message = "Email retrieved successfully",
                    data = email,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<EmailViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<EmailViewModel>
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

        public DtoResponse<EmailViewModel> AddEmail(
            Email email,
            List<DetailEmailPic> detailEmailPics,
            List<DetailEmailHelpdesk> detailEmailHelpdesks,
            string createdBy
        )
        {
            SqlTransaction transaction = null;

            try
            {
                string newId = Guid.NewGuid().ToString();
                _connection.Open();
                transaction = _connection.BeginTransaction();

                string emailQuery =
                    "INSERT INTO tbl_t_email (eml_id, dtm_id, eml_type, eml_subject, eml_body, eml_image, eml_date, eml_status, eml_createdby, eml_createddate) "
                    + "VALUES (@Id, @DowntimeId, @Type, @Subject, @Body, @Image, @Date, @Status, @CreatedBy, @CreatedDate)";

                SqlCommand emailCommand = new SqlCommand(emailQuery, _connection, transaction);
                emailCommand.Parameters.AddWithValue("@Id", newId);
                emailCommand.Parameters.AddWithValue("@DowntimeId", email.DowntimeId);
                emailCommand.Parameters.AddWithValue("@Type", email.Type);
                emailCommand.Parameters.AddWithValue("@Subject", email.Subject);
                emailCommand.Parameters.AddWithValue("@Body", email.Body);
                emailCommand.Parameters.AddWithValue("@Image", email.Image);
                emailCommand.Parameters.AddWithValue("@Date", email.Date);
                emailCommand.Parameters.AddWithValue("@Status", email.Status);
                emailCommand.Parameters.AddWithValue("@CreatedBy", createdBy);
                emailCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                int emailRowsAffected = emailCommand.ExecuteNonQuery();

                if (emailRowsAffected == 0)
                {
                    transaction.Rollback();
                    return new DtoResponse<EmailViewModel>
                    {
                        status = 500,
                        message = "Failed to save data email",
                        data = null,
                    };
                }

                if (detailEmailPics != null && detailEmailPics.Count > 0)
                {
                    foreach (var detailPic in detailEmailPics)
                    {
                        string detailPicId = Guid.NewGuid().ToString();
                        string detailPicQuery =
                            "INSERT INTO tbl_t_detailemailpic (dtl_eml_id, eml_id, pic_id, dtl_eml_type, dtl_eml_emailaddress, dtl_eml_createdby, dtl_eml_hlp_createddate) "
                            + "VALUES (@DetailId, @EmailId, @PicId, @Type, @EmailAddress, @CreatedBy, @CreatedDate)";

                        SqlCommand detailPicCommand = new SqlCommand(
                            detailPicQuery,
                            _connection,
                            transaction
                        );
                        detailPicCommand.Parameters.AddWithValue("@DetailId", detailPicId);
                        detailPicCommand.Parameters.AddWithValue("@EmailId", newId);
                        detailPicCommand.Parameters.AddWithValue("@PicId", detailPic.PicId);
                        detailPicCommand.Parameters.AddWithValue("@Type", detailPic.Type);
                        detailPicCommand.Parameters.AddWithValue(
                            "@EmailAddress",
                            detailPic.EmailAddress
                        );
                        detailPicCommand.Parameters.AddWithValue("@CreatedBy", createdBy);
                        detailPicCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        detailPicCommand.ExecuteNonQuery();
                    }
                }

                if (detailEmailHelpdesks != null && detailEmailHelpdesks.Count > 0)
                {
                    foreach (var detailHelpdesk in detailEmailHelpdesks)
                    {
                        string detailHelpdeskId = Guid.NewGuid().ToString();
                        string detailHelpdeskQuery =
                            "INSERT INTO tbl_t_detailemailhelpdesk (dtl_eml_id, eml_id, hlp_id, dtl_eml_type, dtl_eml_emailaddress, dtl_eml_createdby, dtl_eml_hlp_createddate) "
                            + "VALUES (@DetailId, @EmailId, @HelpdeskId, @Type, @EmailAddress, @CreatedBy, @CreatedDate)";

                        SqlCommand detailHelpdeskCommand = new SqlCommand(
                            detailHelpdeskQuery,
                            _connection,
                            transaction
                        );
                        detailHelpdeskCommand.Parameters.AddWithValue(
                            "@DetailId",
                            detailHelpdeskId
                        );
                        detailHelpdeskCommand.Parameters.AddWithValue("@EmailId", newId);
                        detailHelpdeskCommand.Parameters.AddWithValue(
                            "@HelpdeskId",
                            detailHelpdesk.HelpdeskId
                        );
                        detailHelpdeskCommand.Parameters.AddWithValue("@Type", detailHelpdesk.Type);
                        detailHelpdeskCommand.Parameters.AddWithValue(
                            "@EmailAddress",
                            detailHelpdesk.EmailAddress
                        );
                        detailHelpdeskCommand.Parameters.AddWithValue("@CreatedBy", createdBy);
                        detailHelpdeskCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        detailHelpdeskCommand.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                _connection.Close();

                var result = GetEmailById(newId);
                if (result.status == 200)
                {
                    result.message = "Email data saved successfully";
                    result.status = 201;
                }

                return result;
            }
            catch (SqlException ex)
            {
                transaction?.Rollback();
                Console.WriteLine(ex.Message);
                return new DtoResponse<EmailViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to save data email",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                return new DtoResponse<EmailViewModel>
                {
                    status = 500,
                    message = "Internal Server Error: " + ex.Message,
                    data = null,
                };
            }
            finally
            {
                transaction?.Dispose();
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public DtoResponse<EmailViewModel> UpdateEmail(
            Email email,
            string updatedBy,
            List<DetailEmailPic> detailEmailPics = null,
            List<DetailEmailHelpdesk> detailEmailHelpdesks = null
        )
        {
            SqlTransaction transaction = null;

            try
            {
                _connection.Open();
                transaction = _connection.BeginTransaction();

                // Update main email
                string emailQuery =
                    "UPDATE tbl_t_email "
                    + "SET dtm_id = @DowntimeId, eml_type = @Type, eml_subject = @Subject, eml_body = @Body, eml_image = @Image, eml_date = @Date, eml_updatedby = @UpdatedBy, eml_updateddate = @UpdatedDate "
                    + "WHERE eml_id = @Id";

                SqlCommand emailCommand = new SqlCommand(emailQuery, _connection, transaction);
                emailCommand.Parameters.AddWithValue("@Id", email.Id);
                emailCommand.Parameters.AddWithValue("@DowntimeId", email.DowntimeId);
                emailCommand.Parameters.AddWithValue("@Type", email.Type);
                emailCommand.Parameters.AddWithValue("@Subject", email.Subject);
                emailCommand.Parameters.AddWithValue("@Body", email.Body);
                emailCommand.Parameters.AddWithValue("@Image", email.Image);
                emailCommand.Parameters.AddWithValue("@Date", email.Date);
                emailCommand.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                emailCommand.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                int emailRowsAffected = emailCommand.ExecuteNonQuery();

                if (emailRowsAffected == 0)
                {
                    transaction.Rollback();
                    return new DtoResponse<EmailViewModel>
                    {
                        status = 404,
                        message = "Email not found",
                        data = null,
                    };
                }

                // Delete existing detail email pics
                string deletePicsQuery = "DELETE FROM tbl_t_detailemailpic WHERE eml_id = @EmailId";
                SqlCommand deletePicsCommand = new SqlCommand(
                    deletePicsQuery,
                    _connection,
                    transaction
                );
                deletePicsCommand.Parameters.AddWithValue("@EmailId", email.Id);
                deletePicsCommand.ExecuteNonQuery();

                // Delete existing detail email helpdesks
                string deleteHelpdesksQuery =
                    "DELETE FROM tbl_t_detailemailhelpdesk WHERE eml_id = @EmailId";
                SqlCommand deleteHelpdesksCommand = new SqlCommand(
                    deleteHelpdesksQuery,
                    _connection,
                    transaction
                );
                deleteHelpdesksCommand.Parameters.AddWithValue("@EmailId", email.Id);
                deleteHelpdesksCommand.ExecuteNonQuery();

                // Insert new detail email pics
                if (detailEmailPics != null && detailEmailPics.Count > 0)
                {
                    foreach (var detailPic in detailEmailPics)
                    {
                        string detailPicId = Guid.NewGuid().ToString();
                        string detailPicQuery =
                            "INSERT INTO tbl_t_detailemailpic (dtl_eml_id, eml_id, pic_id, dtl_eml_type, dtl_eml_emailaddress, dtl_eml_createdby, dtl_eml_hlp_createddate) "
                            + "VALUES (@DetailId, @EmailId, @PicId, @Type, @EmailAddress, @CreatedBy, @CreatedDate)";

                        SqlCommand detailPicCommand = new SqlCommand(
                            detailPicQuery,
                            _connection,
                            transaction
                        );
                        detailPicCommand.Parameters.AddWithValue("@DetailId", detailPicId);
                        detailPicCommand.Parameters.AddWithValue("@EmailId", email.Id);
                        detailPicCommand.Parameters.AddWithValue("@PicId", detailPic.PicId);
                        detailPicCommand.Parameters.AddWithValue("@Type", detailPic.Type);
                        detailPicCommand.Parameters.AddWithValue(
                            "@EmailAddress",
                            detailPic.EmailAddress
                        );
                        detailPicCommand.Parameters.AddWithValue("@CreatedBy", updatedBy);
                        detailPicCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        detailPicCommand.ExecuteNonQuery();
                    }
                }

                // Insert new detail email helpdesks
                if (detailEmailHelpdesks != null && detailEmailHelpdesks.Count > 0)
                {
                    foreach (var detailHelpdesk in detailEmailHelpdesks)
                    {
                        string detailHelpdeskId = Guid.NewGuid().ToString();
                        string detailHelpdeskQuery =
                            "INSERT INTO tbl_t_detailemailhelpdesk (dtl_eml_id, eml_id, hlp_id, dtl_eml_type, dtl_eml_emailaddress, dtl_eml_createdby, dtl_eml_hlp_createddate) "
                            + "VALUES (@DetailId, @EmailId, @HelpdeskId, @Type, @EmailAddress, @CreatedBy, @CreatedDate)";

                        SqlCommand detailHelpdeskCommand = new SqlCommand(
                            detailHelpdeskQuery,
                            _connection,
                            transaction
                        );
                        detailHelpdeskCommand.Parameters.AddWithValue(
                            "@DetailId",
                            detailHelpdeskId
                        );
                        detailHelpdeskCommand.Parameters.AddWithValue("@EmailId", email.Id);
                        detailHelpdeskCommand.Parameters.AddWithValue(
                            "@HelpdeskId",
                            detailHelpdesk.HelpdeskId
                        );
                        detailHelpdeskCommand.Parameters.AddWithValue("@Type", detailHelpdesk.Type);
                        detailHelpdeskCommand.Parameters.AddWithValue(
                            "@EmailAddress",
                            detailHelpdesk.EmailAddress
                        );
                        detailHelpdeskCommand.Parameters.AddWithValue("@CreatedBy", updatedBy);
                        detailHelpdeskCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        detailHelpdeskCommand.ExecuteNonQuery();
                    }
                }

                transaction.Commit();

                // Return the updated email with details
                var result = GetEmailById(email.Id);
                if (result.status == 200)
                {
                    result.message = "Email data saved successfully";
                }

                return result;
            }
            catch (SqlException)
            {
                transaction?.Rollback();
                return new DtoResponse<EmailViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update email",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                return new DtoResponse<EmailViewModel>
                {
                    status = 500,
                    message = "Internal Server Error: " + ex.Message,
                    data = null,
                };
            }
            finally
            {
                transaction?.Dispose();
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public DtoResponse<object> DeleteEmail(string id)
        {
            SqlTransaction transaction = null;

            try
            {
                _connection.Open();
                transaction = _connection.BeginTransaction();

                // Delete detail email pics first (foreign key constraint)
                string deletePicsQuery = "DELETE FROM tbl_t_detailemailpic WHERE eml_id = @EmailId";
                SqlCommand deletePicsCommand = new SqlCommand(
                    deletePicsQuery,
                    _connection,
                    transaction
                );
                deletePicsCommand.Parameters.AddWithValue("@EmailId", id);
                deletePicsCommand.ExecuteNonQuery();

                // Delete detail email helpdesks
                string deleteHelpdesksQuery =
                    "DELETE FROM tbl_t_detailemailhelpdesk WHERE eml_id = @EmailId";
                SqlCommand deleteHelpdesksCommand = new SqlCommand(
                    deleteHelpdesksQuery,
                    _connection,
                    transaction
                );
                deleteHelpdesksCommand.Parameters.AddWithValue("@EmailId", id);
                deleteHelpdesksCommand.ExecuteNonQuery();

                // Delete main email
                string emailQuery = "DELETE FROM tbl_t_email WHERE eml_id = @Id";
                SqlCommand emailCommand = new SqlCommand(emailQuery, _connection, transaction);
                emailCommand.Parameters.AddWithValue("@Id", id);

                int rowsAffected = emailCommand.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    transaction.Commit();
                    return new DtoResponse<object>
                    {
                        status = 200,
                        message = "Email data deleted successfully",
                        data = null,
                    };
                }
                else
                {
                    transaction.Rollback();
                    return new DtoResponse<object>
                    {
                        status = 404,
                        message = "Email not found or already deleted",
                        data = null,
                    };
                }
            }
            catch (SqlException sqlEx)
            {
                transaction?.Rollback();
                if (sqlEx.Number == 547)
                {
                    return new DtoResponse<object>
                    {
                        status = 409,
                        message = "Cannot delete email because it is referenced by other records",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 500,
                        message = "SQL Error: Failed to delete email",
                        data = null,
                    };
                }
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                return new DtoResponse<object>
                {
                    status = 500,
                    message = "Internal Server Error: " + ex.Message,
                    data = null,
                };
            }
            finally
            {
                transaction?.Dispose();
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        private void LoadEmailDetails(EmailViewModel email)
        {
            try
            {
                string picQuery =
                    "SELECT dep.dtl_eml_id, dep.eml_id, dep.pic_id, dep.dtl_eml_type, dep.dtl_eml_emailaddress, p.pic_name "
                    + "FROM tbl_t_detailemailpic dep "
                    + "INNER JOIN tbl_m_pic p ON dep.pic_id = p.pic_id "
                    + "WHERE dep.eml_id = @EmailId";

                using (SqlConnection tempConnection = new SqlConnection(_connectionString))
                {
                    tempConnection.Open();
                    SqlCommand picCommand = new SqlCommand(picQuery, tempConnection);
                    picCommand.Parameters.AddWithValue("@EmailId", email.Id);

                    SqlDataReader picReader = picCommand.ExecuteReader();
                    while (picReader.Read())
                    {
                        email.DetailEmailPics.Add(
                            new DetailEmailPicViewModel
                            {
                                Id = picReader["dtl_eml_id"].ToString(),
                                EmailId = picReader["eml_id"].ToString(),
                                PicId = picReader["pic_id"].ToString(),
                                Type = Convert.ToInt32(picReader["dtl_eml_type"]),
                                EmailAddress = picReader["dtl_eml_emailaddress"].ToString(),
                                PicName = picReader["pic_name"].ToString(),
                            }
                        );
                    }
                    picReader.Close();

                    string helpdeskQuery =
                        "SELECT deh.dtl_eml_id, deh.eml_id, deh.hlp_id, deh.dtl_eml_type, deh.dtl_eml_emailaddress, h.hlp_name "
                        + "FROM tbl_t_detailemailhelpdesk deh "
                        + "INNER JOIN tbl_m_helpdesk h ON deh.hlp_id = h.hlp_id "
                        + "WHERE deh.eml_id = @EmailId";

                    SqlCommand helpdeskCommand = new SqlCommand(helpdeskQuery, tempConnection);
                    helpdeskCommand.Parameters.AddWithValue("@EmailId", email.Id);

                    SqlDataReader helpdeskReader = helpdeskCommand.ExecuteReader();
                    while (helpdeskReader.Read())
                    {
                        email.DetailEmailHelpdesks.Add(
                            new DetailEmailHelpdeskViewModel
                            {
                                Id = helpdeskReader["dtl_eml_id"].ToString(),
                                EmailId = helpdeskReader["eml_id"].ToString(),
                                HelpdeskId = helpdeskReader["hlp_id"].ToString(),
                                Type = Convert.ToInt32(helpdeskReader["dtl_eml_type"]),
                                EmailAddress = helpdeskReader["dtl_eml_emailaddress"].ToString(),
                                HelpdeskName = helpdeskReader["hlp_name"].ToString(),
                            }
                        );
                    }
                    helpdeskReader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading email details: {ex.Message}");
            }
        }
    }
}
