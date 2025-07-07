using Microsoft.Data.SqlClient;
using NDMNS_API.Models;
using NDMNS_API.Responses;
using NDMNS_API.Services;

namespace NDMNS_API.Repositories
{
    public class MessageRepository
    {
        private readonly HelperService _helperService;
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public MessageRepository(IConfiguration configuration, HelperService helperService)
        {
            _helperService = helperService;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        public DtoResponse<List<MessageViewModel>> GetAllMessages()
        {
            List<MessageViewModel> messages = new List<MessageViewModel>();

            try
            {
                var groupsResponse = _helperService.GetGroups();
                var groups = groupsResponse?.data ?? new List<GroupResponse>();

                string query =
                    "SELECT m.msg_id, m.msg_date, m.msg_recipient, m.msg_recipienttype, m.msg_messageid, m.msg_text, "
                    + "m.msg_image, m.msg_type, m.msg_category, m.msg_level, m.msg_status, m.dtm_id, "
                    + "d.dtm_description, d.dtm_ticketnumber "
                    + "FROM tbl_t_message m "
                    + "LEFT JOIN tbl_t_downtime d ON m.dtm_id = d.dtm_id "
                    + "ORDER BY m.msg_date DESC";

                SqlCommand command = new SqlCommand(query, _connection);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string receiverId = reader["msg_recipient"].ToString();
                    var matchingGroup = groups.FirstOrDefault(g => g.Id == receiverId);

                    MessageViewModel message = new MessageViewModel
                    {
                        Id = reader["msg_id"].ToString(),
                        Date = Convert.ToDateTime(reader["msg_date"]),
                        Recipient = receiverId,
                        RecipientType = Convert.ToInt32(reader["msg_recipienttype"]),
                        MessageId = reader["msg_messageid"].ToString(),
                        Text = reader["msg_text"].ToString(),
                        Image = reader["msg_image"].ToString(),
                        Type = Convert.ToInt32(reader["msg_type"]),
                        Category = Convert.ToInt32(reader["msg_category"]),
                        Level = Convert.ToInt32(reader["msg_level"]),
                        Status = Convert.ToInt32(reader["msg_status"]),
                        DowntimeId = reader["dtm_id"].ToString(),
                        DowntimeDescription =
                            reader["dtm_description"]?.ToString() ?? "No description",
                        RecipientName = matchingGroup?.Name ?? "Group not found",
                        DowntimeTicketNumber = reader["dtm_ticketnumber"].ToString() ?? "",
                    };
                    messages.Add(message);
                }
                reader.Close();

                return new DtoResponse<List<MessageViewModel>>
                {
                    status = 200,
                    message = "Messages retrieved successfully",
                    data = messages,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<MessageViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve messages",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<MessageViewModel>>
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

        public DtoResponse<MessageViewModel> GetMessageById(string id)
        {
            MessageViewModel message = null;

            try
            {
                var groupsResponse = _helperService.GetGroups();
                var groups = groupsResponse?.data ?? new List<GroupResponse>();

                string query =
                    "SELECT m.msg_id, m.msg_date, m.msg_recipient, m.msg_recipienttype, m.msg_messageid, m.msg_text, "
                    + "m.msg_image, m.msg_type, m.msg_category, m.msg_level, m.msg_status, m.dtm_id, "
                    + "d.dtm_description "
                    + "FROM tbl_t_message m "
                    + "LEFT JOIN tbl_t_downtime d ON m.dtm_id = d.dtm_id "
                    + "WHERE m.msg_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string receiverId = reader["msg_recipient"].ToString();
                    var matchingGroup = groups.FirstOrDefault(g => g.Id == receiverId);

                    message = new MessageViewModel
                    {
                        Id = reader["msg_id"].ToString(),
                        Date = Convert.ToDateTime(reader["msg_date"]),
                        Recipient = receiverId,
                        RecipientType = Convert.ToInt32(reader["msg_recipienttype"]),
                        MessageId = reader["msg_messageid"].ToString(),
                        Text = reader["msg_text"].ToString(),
                        Image = reader["msg_image"].ToString(),
                        Type = Convert.ToInt32(reader["msg_type"]),
                        Category = Convert.ToInt32(reader["msg_category"]),
                        Level = Convert.ToInt32(reader["msg_level"]),
                        Status = Convert.ToInt32(reader["msg_status"]),
                        DowntimeId = reader["dtm_id"].ToString(),
                        DowntimeDescription =
                            reader["dtm_description"]?.ToString() ?? "No description",
                        RecipientName = matchingGroup?.Name ?? "Group not found",
                    };
                }
                reader.Close();

                if (message == null)
                {
                    return new DtoResponse<MessageViewModel>
                    {
                        status = 404,
                        message = "Message not found",
                        data = null,
                    };
                }

                return new DtoResponse<MessageViewModel>
                {
                    status = 200,
                    message = "Message retrieved successfully",
                    data = message,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<MessageViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve message",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<MessageViewModel>
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

        public DtoResponse<MessageViewModel> GetMessageByMessageId(string msgId)
        {
            MessageViewModel message = null;

            try
            {
                var groupsResponse = _helperService.GetGroups();
                var groups = groupsResponse?.data ?? new List<GroupResponse>();

                string query =
                    "SELECT m.msg_id, m.msg_date, m.msg_recipient, m.msg_recipienttype, m.msg_messageid, m.msg_text, "
                    + "m.msg_image, m.msg_type, m.msg_category, m.msg_level, m.msg_status, m.dtm_id, "
                    + "d.dtm_description "
                    + "FROM tbl_t_message m "
                    + "LEFT JOIN tbl_t_downtime d ON m.dtm_id = d.dtm_id "
                    + "WHERE m.msg_messageid = @MessageId";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@MessageId", msgId);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string receiverId = reader["msg_recipient"].ToString();
                    var matchingGroup = groups.FirstOrDefault(g => g.Id == receiverId);

                    message = new MessageViewModel
                    {
                        Id = reader["msg_id"].ToString(),
                        Date = Convert.ToDateTime(reader["msg_date"]),
                        Recipient = receiverId,
                        RecipientType = Convert.ToInt32(reader["msg_recipienttype"]),
                        MessageId = reader["msg_messageid"].ToString(),
                        Text = reader["msg_text"].ToString(),
                        Image = reader["msg_image"].ToString(),
                        Type = Convert.ToInt32(reader["msg_type"]),
                        Category = Convert.ToInt32(reader["msg_category"]),
                        Level = Convert.ToInt32(reader["msg_level"]),
                        Status = Convert.ToInt32(reader["msg_status"]),
                        DowntimeId = reader["dtm_id"].ToString(),
                        DowntimeDescription =
                            reader["dtm_description"]?.ToString() ?? "No description",
                        RecipientName = matchingGroup?.Name ?? "Group not found",
                    };
                }
                reader.Close();

                if (message == null)
                {
                    return new DtoResponse<MessageViewModel>
                    {
                        status = 404,
                        message = "Message not found",
                        data = null,
                    };
                }

                return new DtoResponse<MessageViewModel>
                {
                    status = 200,
                    message = "Message retrieved successfully",
                    data = message,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<MessageViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve message",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<MessageViewModel>
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

        public DtoResponse<List<MessageViewModel>> GetMessagesByDowntimeId(string downtimeId)
        {
            List<MessageViewModel> messages = new List<MessageViewModel>();

            try
            {
                var groupsResponse = _helperService.GetGroups();
                var groups = groupsResponse?.data ?? new List<GroupResponse>();

                string query =
                    "SELECT m.msg_id, m.msg_date, m.msg_recipient, m.msg_recipienttype, m.msg_messageid, m.msg_text, "
                    + "m.msg_image, m.msg_type, m.msg_category, m.msg_level, m.msg_status, m.dtm_id, "
                    + "d.dtm_description "
                    + "FROM tbl_t_message m "
                    + "LEFT JOIN tbl_t_downtime d ON m.dtm_id = d.dtm_id "
                    + "WHERE m.dtm_id = @DowntimeId";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@DowntimeId", downtimeId);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string receiverId = reader["msg_recipient"].ToString();
                    var matchingGroup = groups.FirstOrDefault(g => g.Id == receiverId);

                    MessageViewModel message = new MessageViewModel
                    {
                        Id = reader["msg_id"].ToString(),
                        Date = Convert.ToDateTime(reader["msg_date"]),
                        Recipient = receiverId,
                        RecipientType = Convert.ToInt32(reader["msg_recipienttype"]),
                        MessageId = reader["msg_messageid"].ToString(),
                        Text = reader["msg_text"].ToString(),
                        Image = reader["msg_image"].ToString(),
                        Type = Convert.ToInt32(reader["msg_type"]),
                        Category = Convert.ToInt32(reader["msg_category"]),
                        Level = Convert.ToInt32(reader["msg_level"]),
                        Status = Convert.ToInt32(reader["msg_status"]),
                        DowntimeId = reader["dtm_id"].ToString(),
                        DowntimeDescription =
                            reader["dtm_description"]?.ToString() ?? "No description",
                        RecipientName = matchingGroup?.Name ?? "Group not found",
                    };
                    messages.Add(message);
                }
                reader.Close();

                return new DtoResponse<List<MessageViewModel>>
                {
                    status = 200,
                    message = "Messages retrieved successfully",
                    data = messages,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<MessageViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve messages",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<MessageViewModel>>
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

        public DtoResponse<List<MessageViewModel>> GetMessagesByNetworkDown(string networkId)
        {
            List<MessageViewModel> messages = new List<MessageViewModel>();

            try
            {
                var groupsResponse = _helperService.GetGroups();
                var groups = groupsResponse?.data ?? new List<GroupResponse>();

                string query =
                    "SELECT m.msg_id, m.dtm_id, m.msg_date, m.msg_recipient, m.msg_recipienttype, m.msg_messageid, m.msg_text, m.msg_image, "
                    + "m.msg_type, m.msg_category, m.msg_level, m.msg_status, d.dtm_description "
                    + "FROM tbl_t_message m "
                    + "INNER JOIN tbl_t_downtime d ON m.dtm_id = d.dtm_id "
                    + "INNER JOIN tbl_m_network n ON d.net_id = n.net_id "
                    + "WHERE n.net_id = @NetworkId "
                    + "AND n.net_status = 1 "
                    + "AND d.dtm_status = 1 "
                    + "AND d.dtm_start = n.net_last_update "
                    + "ORDER BY m.msg_date DESC;";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@NetworkId", networkId);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string receiverId = reader["msg_recipient"].ToString();
                    var matchingGroup = groups.FirstOrDefault(g => g.Id == receiverId);

                    MessageViewModel message = new MessageViewModel
                    {
                        Id = reader["msg_id"].ToString(),
                        Date = Convert.ToDateTime(reader["msg_date"]),
                        Recipient = receiverId,
                        RecipientType = Convert.ToInt32(reader["msg_recipienttype"]),
                        MessageId = reader["msg_messageid"].ToString(),
                        Text = reader["msg_text"].ToString(),
                        Image = reader["msg_image"].ToString(),
                        Type = Convert.ToInt32(reader["msg_type"]),
                        Category = Convert.ToInt32(reader["msg_category"]),
                        Level = Convert.ToInt32(reader["msg_level"]),
                        Status = Convert.ToInt32(reader["msg_status"]),
                        DowntimeId = reader["dtm_id"].ToString(),
                        DowntimeDescription =
                            reader["dtm_description"]?.ToString() ?? "No description",
                        RecipientName = matchingGroup?.Name ?? "Group not found",
                    };
                    messages.Add(message);
                }
                reader.Close();

                return new DtoResponse<List<MessageViewModel>>
                {
                    status = 200,
                    message = "Messages retrieved successfully",
                    data = messages,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<MessageViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve messages",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<MessageViewModel>>
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

        public DtoResponse<MessageViewModel> GetLatestMessageByDowntimeId(string downtimeId)
        {
            MessageViewModel latestMessage = null;

            try
            {
                var groupsResponse = _helperService.GetGroups();
                var groups = groupsResponse?.data ?? new List<GroupResponse>();

                string query =
                    "SELECT TOP 1 m.msg_id, m.msg_date, m.msg_recipient, m.msg_recipienttype, m.msg_messageid, m.msg_text, "
                    + "m.msg_image, m.msg_type, m.msg_category, m.msg_level, m.msg_status, m.dtm_id, "
                    + "d.dtm_description "
                    + "FROM tbl_t_message m "
                    + "LEFT JOIN tbl_t_downtime d ON m.dtm_id = d.dtm_id "
                    + "WHERE m.dtm_id = @DowntimeId "
                    + "AND (m.msg_type = 1 OR m.msg_type = 3) "
                    + "AND (m.msg_category = 1 OR m.msg_category = 3) "
                    + "AND m.msg_level >= 1 "
                    + "ORDER BY m.msg_date DESC";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@DowntimeId", downtimeId);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string receiverId = reader["msg_recipient"].ToString();
                    var matchingGroup = groups.FirstOrDefault(g => g.Id == receiverId);

                    latestMessage = new MessageViewModel
                    {
                        Id = reader["msg_id"].ToString(),
                        Date = Convert.ToDateTime(reader["msg_date"]),
                        Recipient = receiverId,
                        RecipientType = Convert.ToInt32(reader["msg_recipienttype"]),
                        MessageId = reader["msg_messageid"].ToString(),
                        Text = reader["msg_text"].ToString(),
                        Image = reader["msg_image"].ToString(),
                        Type = Convert.ToInt32(reader["msg_type"]),
                        Category = Convert.ToInt32(reader["msg_category"]),
                        Level = Convert.ToInt32(reader["msg_level"]),
                        Status = Convert.ToInt32(reader["msg_status"]),
                        DowntimeId = reader["dtm_id"].ToString(),
                        DowntimeDescription =
                            reader["dtm_description"]?.ToString() ?? "No description",
                        RecipientName = matchingGroup?.Name ?? "Group not found",
                    };
                }
                reader.Close();

                if (latestMessage == null)
                {
                    return new DtoResponse<MessageViewModel>
                    {
                        status = 404,
                        message = "Latest message not found",
                        data = null,
                    };
                }

                return new DtoResponse<MessageViewModel>
                {
                    status = 200,
                    message = "Latest message retrieved successfully",
                    data = latestMessage,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<MessageViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve latest message",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<MessageViewModel>
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

        public DtoResponse<MessageViewModel> GetFirstMessageByDowntimeId(
            string downtimeId,
            string groupId
        )
        {
            MessageViewModel firstMessage = null;

            try
            {
                var groupsResponse = _helperService.GetGroups();
                var groups = groupsResponse?.data ?? new List<GroupResponse>();

                string query =
                    "SELECT TOP 1 m.msg_id, m.msg_date, m.msg_recipient, m.msg_recipienttype, m.msg_messageid, m.msg_text, "
                    + "m.msg_image, m.msg_type, m.msg_category, m.msg_level, m.msg_status, m.dtm_id, "
                    + "d.dtm_description "
                    + "FROM tbl_t_message m "
                    + "LEFT JOIN tbl_t_downtime d ON m.dtm_id = d.dtm_id "
                    + "WHERE m.dtm_id = @DowntimeId "
                    + "AND m.msg_recipient = @GroupId "
                    + "AND (m.msg_type = 1 OR m.msg_type = 3) "
                    + "AND (m.msg_category = 1 OR m.msg_category = 3) "
                    + "AND m.msg_level = 1 "
                    + "ORDER BY m.msg_date DESC";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@DowntimeId", downtimeId);
                command.Parameters.AddWithValue("@GroupId", groupId);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string receiverId = reader["msg_recipient"].ToString();
                    var matchingGroup = groups.FirstOrDefault(g => g.Id == receiverId);

                    firstMessage = new MessageViewModel
                    {
                        Id = reader["msg_id"].ToString(),
                        Date = Convert.ToDateTime(reader["msg_date"]),
                        Recipient = receiverId,
                        RecipientType = Convert.ToInt32(reader["msg_recipienttype"]),
                        MessageId = reader["msg_messageid"].ToString(),
                        Text = reader["msg_text"].ToString(),
                        Image = reader["msg_image"].ToString(),
                        Type = Convert.ToInt32(reader["msg_type"]),
                        Category = Convert.ToInt32(reader["msg_category"]),
                        Level = Convert.ToInt32(reader["msg_level"]),
                        Status = Convert.ToInt32(reader["msg_status"]),
                        DowntimeId = reader["dtm_id"].ToString(),
                        DowntimeDescription =
                            reader["dtm_description"]?.ToString() ?? "No description",
                        RecipientName = matchingGroup?.Name ?? "Group not found",
                    };
                }
                reader.Close();

                if (firstMessage == null)
                {
                    return new DtoResponse<MessageViewModel>
                    {
                        status = 404,
                        message = "First message not found",
                        data = null,
                    };
                }

                return new DtoResponse<MessageViewModel>
                {
                    status = 200,
                    message = "First message retrieved successfully",
                    data = firstMessage,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<MessageViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve first message",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<MessageViewModel>
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

        public DtoResponse<int> GetSentMessageCount()
        {
            try
            {
                string query = "SELECT COUNT(*) FROM tbl_t_message";

                SqlCommand command = new SqlCommand(query, _connection);
                _connection.Open();

                int count = (int)command.ExecuteScalar();

                return new DtoResponse<int>
                {
                    status = 200,
                    message = "Message count retrieved successfully",
                    data = count,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<int>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve message count",
                    data = 0,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<int>
                {
                    status = 500,
                    message = "Internal Server Error: " + ex.Message,
                    data = 0,
                };
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public DtoResponse<MessageViewModel> AddMessage(Message message, string userId)
        {
            try
            {
                string newId = Guid.NewGuid().ToString();
                string query =
                    "INSERT INTO tbl_t_message (msg_id, msg_date, msg_recipient, msg_recipienttype, msg_messageid, msg_text, "
                    + "msg_image, msg_type, msg_category, msg_level, msg_status, dtm_id, msg_createdby, msg_createddate) "
                    + "VALUES (@Id, @Date, @Recipient, @RecipientType, @MessageId, @Text, @Image, @Type, @Category, @Level, @Status, @DowntimeId, @CreatedBy, @CreatedDate)";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", newId);
                command.Parameters.AddWithValue("@Date", message.Date);
                command.Parameters.AddWithValue("@Recipient", message.Recipient);
                command.Parameters.AddWithValue("@RecipientType", message.RecipientType);
                command.Parameters.AddWithValue("@MessageId", message.MessageId);
                command.Parameters.AddWithValue("@Text", message.Text);
                command.Parameters.AddWithValue("@Image", message.Image ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Type", message.Type);
                command.Parameters.AddWithValue("@Category", message.Category);
                command.Parameters.AddWithValue("@Level", message.Level);
                command.Parameters.AddWithValue("@Status", message.Status);
                command.Parameters.AddWithValue("@DowntimeId", message.DowntimeId);
                command.Parameters.AddWithValue("@CreatedBy", userId);
                command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    message.Id = newId;
                    var messageViewModel = new MessageViewModel
                    {
                        Id = newId,
                        Date = message.Date,
                        Recipient = message.Recipient,
                        RecipientType = message.RecipientType,
                        MessageId = message.MessageId,
                        Text = message.Text,
                        Image = message.Image,
                        Type = message.Type,
                        Category = message.Category,
                        Level = message.Level,
                        Status = message.Status,
                        DowntimeId = message.DowntimeId,
                        DowntimeDescription = "No description",
                        RecipientName = "Group not found",
                    };
                    return new DtoResponse<MessageViewModel>
                    {
                        status = 201,
                        message = "Message data saved successfully",
                        data = messageViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<MessageViewModel>
                    {
                        status = 500,
                        message = "Failed to save data message",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<MessageViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to save data message",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<MessageViewModel>
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

        public DtoResponse<MessageViewModel> AddMessageFollowUp(Message message, string userId)
        {
            try
            {
                string newId = Guid.NewGuid().ToString();
                string query =
                    "INSERT INTO tbl_t_message (msg_id, msg_date, msg_recipient, msg_recipienttype, msg_messageid, msg_text, "
                    + "msg_type, msg_category, msg_level, msg_status, dtm_id, msg_createdby, msg_createddate) "
                    + "VALUES (@Id, @Date, @Recipient, @RecipientType, @MessageId, @Text, "
                    + "@Type, @Category, @Level, @Status, @DowntimeId, @CreatedBy, @CreatedDate)";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", newId);
                command.Parameters.AddWithValue("@Date", message.Date);
                command.Parameters.AddWithValue("@Recipient", message.Recipient);
                command.Parameters.AddWithValue("@RecipientType", message.RecipientType);
                command.Parameters.AddWithValue("@MessageId", message.MessageId);
                command.Parameters.AddWithValue("@Text", message.Text);
                command.Parameters.AddWithValue("@Type", message.Type);
                command.Parameters.AddWithValue("@Category", message.Category);
                command.Parameters.AddWithValue("@Level", message.Level);
                command.Parameters.AddWithValue("@Status", message.Status);
                command.Parameters.AddWithValue("@DowntimeId", message.DowntimeId);
                command.Parameters.AddWithValue("@CreatedBy", userId);
                command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    message.Id = newId;
                    var messageViewModel = new MessageViewModel
                    {
                        Id = newId,
                        Date = message.Date,
                        Recipient = message.Recipient,
                        RecipientType = message.RecipientType,
                        MessageId = message.MessageId,
                        Text = message.Text,
                        Image = message.Image,
                        Type = message.Type,
                        Category = message.Category,
                        Level = message.Level,
                        Status = message.Status,
                        DowntimeId = message.DowntimeId,
                        DowntimeDescription = "No description",
                        RecipientName = "Group not found",
                    };

                    return new DtoResponse<MessageViewModel>
                    {
                        status = 201,
                        message = "Follow-up message data saved successfully",
                        data = messageViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<MessageViewModel>
                    {
                        status = 500,
                        message = "Failed to save data follow-up message",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<MessageViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to save data follow-up message",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<MessageViewModel>
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

        public DtoResponse<MessageViewModel> UpdateMessage(
            string id,
            Message message,
            string userId
        )
        {
            try
            {
                string query =
                    @"
                    UPDATE tbl_t_message 
                    SET msg_text = @Text, 
                        msg_image = @Image, 
                        msg_type = @Type, 
                        msg_category = @Category, 
                        msg_level = @Level, 
                        msg_status = @Status,
                        msg_updatedby = @UpdatedBy,
                        msg_updateddate = @UpdatedDate
                    WHERE msg_id = @Id";

                using (SqlCommand command = new SqlCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Text", message.Text);
                    command.Parameters.AddWithValue(
                        "@Image",
                        message.Image ?? (object)DBNull.Value
                    );
                    command.Parameters.AddWithValue("@Type", message.Type);
                    command.Parameters.AddWithValue("@Category", message.Category);
                    command.Parameters.AddWithValue("@Level", message.Level);
                    command.Parameters.AddWithValue("@Status", message.Status);
                    command.Parameters.AddWithValue("@UpdatedBy", userId);
                    command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                    _connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        var messageViewModel = new MessageViewModel
                        {
                            Id = message.Id,
                            Date = message.Date,
                            Recipient = message.Recipient,
                            RecipientType = message.RecipientType,
                            MessageId = message.MessageId,
                            Text = message.Text,
                            Image = message.Image,
                            Type = message.Type,
                            Category = message.Category,
                            Level = message.Level,
                            Status = message.Status,
                            DowntimeId = message.DowntimeId,
                            DowntimeDescription = "No description",
                            RecipientName = "Group not found",
                        };

                        return new DtoResponse<MessageViewModel>
                        {
                            status = 200,
                            message = "Message data saved successfully",
                            data = messageViewModel,
                        };
                    }
                    else
                    {
                        return new DtoResponse<MessageViewModel>
                        {
                            status = 404,
                            message = "Message not found",
                            data = null,
                        };
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                return new DtoResponse<MessageViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update message",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return new DtoResponse<MessageViewModel>
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

        public DtoResponse<MessageViewModel> UpdateMessageStatus(
            string id,
            int status,
            string userId
        )
        {
            try
            {
                string query =
                    @"
                    UPDATE tbl_t_message 
                    SET msg_status = @Status,
                        msg_updatedby = @UpdatedBy,
                        msg_updateddate = @UpdatedDate
                    WHERE msg_id = @Id";

                using (SqlCommand command = new SqlCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Status", status);
                    command.Parameters.AddWithValue("@UpdatedBy", userId);
                    command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                    _connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return new DtoResponse<MessageViewModel>
                        {
                            status = 200,
                            message = "Message data saved successfully",
                            data = null,
                        };
                    }
                    else
                    {
                        return new DtoResponse<MessageViewModel>
                        {
                            status = 404,
                            message = "Message not found",
                            data = null,
                        };
                    }
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<MessageViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update message",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<MessageViewModel>
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
