using Microsoft.Data.SqlClient;
using NDMNS_API.Models;
using NDMNS_API.Responses;
using System.Data;

namespace NDMNS_API.Services
{
    public class HelperService
    {
        private readonly HttpClient _httpClient;
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public HelperService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            _httpClient.BaseAddress = new Uri(
                configuration["WhatsAppService:BaseUrl"] ?? "http://localhost:3000"
            );
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        public DtoResponse<List<GroupResponse>> GetGroups()
        {
            try
            {
                var response = _httpClient.GetAsync("/api/whatsapp/groups").Result;
                response.EnsureSuccessStatusCode();

                var jsonContent = response
                    .Content.ReadFromJsonAsync<DtoResponse<List<GroupResponse>>>()
                    .Result;
                return jsonContent
                    ?? new DtoResponse<List<GroupResponse>>
                    {
                        status = 400,
                        message = "Error retrieving Groups",
                    };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting WhatsApp groups: {ex.Message}");
                return new DtoResponse<List<GroupResponse>>
                {
                    status = 500,
                    message = $"Internal Server Error: {ex.Message}",
                    data = new List<GroupResponse>(),
                };
            }
        }

        /// <summary>
        /// Generate ticket number with format: [Site Name][DD][MM][YYYY][XXXXX][Shift UserCode]
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <returns>DtoResponse containing the generated ticket number</returns>
        public async Task<DtoResponse<string>> GenerateTicketNumberAsync(string siteId)
        {
            try
            {
                if (string.IsNullOrEmpty(siteId))
                {
                    return new DtoResponse<string>
                    {
                        status = 400,
                        message = "Site ID cannot be empty",
                        data = null,
                    };
                }

                var siteName = await GetSiteName(siteId);
                if (string.IsNullOrEmpty(siteName))
                {
                    return new DtoResponse<string>
                    {
                        status = 404,
                        message = "Site not found",
                        data = null,
                    };
                }

                var siteCode =
                    siteName.Length > 4
                        ? siteName.Substring(0, 4).ToUpper()
                        : siteName.PadRight(4, '0').ToUpper();

                var now = DateTime.Now;
                var day = now.Day.ToString("D2");
                var month = now.Month.ToString("D2");
                var year = now.Year.ToString();

                var currentShiftUserCode = await GetCurrentShiftUserCode(now);
                if (string.IsNullOrEmpty(currentShiftUserCode))
                {
                    return new DtoResponse<string>
                    {
                        status = 404,
                        message = "No active shift found for current time",
                        data = null,
                    };
                }

                var sequenceNumber = await GetNextTicketSequence(siteId, year);

                // Format ticket number: [Site Name][DD][MM][YYYY][XXXXX][Shift UserCode]
                var ticketNumber =
                    $"{siteCode}{day}{month}{year}{sequenceNumber:D5}{currentShiftUserCode}";

                return new DtoResponse<string>
                {
                    status = 200,
                    message = "Ticket number generated successfully",
                    data = ticketNumber,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<string>
                {
                    status = 500,
                    message = $"Internal Server Error: {ex.Message}",
                    data = null,
                };
            }
        }

        /// <summary>
        /// Decrypt ticket number to get detailed information
        /// </summary>
        /// <param name="ticketNumber">Ticket number to decrypt</param>
        /// <returns>DtoResponse containing detailed ticket information</returns>
        public DtoResponse<TicketInfo> DecryptTicketNumber(string ticketNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(ticketNumber))
                {
                    return new DtoResponse<TicketInfo>
                    {
                        status = 400,
                        message = "Ticket number cannot be empty",
                        data = null,
                    };
                }

                // Parse ticket number with format: [Site Name][DD][MM][YYYY][XXXXX][Shift UserCode]
                // Minimum length should be: 4 (site) + 2 (day) + 2 (month) + 4 (year) + 5 (sequence) + 3 (userCode) = 20
                if (ticketNumber.Length < 18)
                {
                    return new DtoResponse<TicketInfo>
                    {
                        status = 400,
                        message =
                            "Invalid ticket number format. Expected format: [Site Name][DD][MM][YYYY][XXXXX][Shift UserCode]",
                        data = null,
                    };
                }

                var siteCode = ticketNumber.Substring(0, 4);
                var dayStr = ticketNumber.Substring(4, 2);
                var monthStr = ticketNumber.Substring(6, 2);
                var yearStr = ticketNumber.Substring(8, 4);
                var sequenceStr = ticketNumber.Substring(12, 5);
                var shiftUserCode = ticketNumber.Substring(17, 3);

                if (!int.TryParse(dayStr, out int day) || day < 1 || day > 31)
                {
                    return new DtoResponse<TicketInfo>
                    {
                        status = 400,
                        message = "Invalid day format",
                        data = null,
                    };
                }

                // Validate month
                if (!int.TryParse(monthStr, out int month) || month < 1 || month > 12)
                {
                    return new DtoResponse<TicketInfo>
                    {
                        status = 400,
                        message = "Invalid month format",
                        data = null,
                    };
                }

                // Validate year
                if (!int.TryParse(yearStr, out int year) || year < 1900)
                {
                    return new DtoResponse<TicketInfo>
                    {
                        status = 400,
                        message = "Invalid year format",
                        data = null,
                    };
                }

                // Validate sequence number
                if (!int.TryParse(sequenceStr, out int sequenceNumber) || sequenceNumber < 1)
                {
                    return new DtoResponse<TicketInfo>
                    {
                        status = 400,
                        message = "Invalid sequence number format",
                        data = null,
                    };
                }

                // Validate date
                DateTime ticketDate;
                try
                {
                    ticketDate = new DateTime(year, month, day);
                }
                catch
                {
                    return new DtoResponse<TicketInfo>
                    {
                        status = 400,
                        message = "Invalid date",
                        data = null,
                    };
                }

                var ticketInfo = new TicketInfo
                {
                    TicketNumber = ticketNumber,
                    SiteCode = siteCode,
                    TicketDate = ticketDate,
                    Day = day,
                    Month = month,
                    Year = year,
                    SequenceNumber = sequenceNumber,
                    ShiftUserCode = shiftUserCode,
                    FormattedDate = ticketDate.ToString("dd/MM/yyyy"),
                };

                return new DtoResponse<TicketInfo>
                {
                    status = 200,
                    message = "Ticket decrypted successfully",
                    data = ticketInfo,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<TicketInfo>
                {
                    status = 500,
                    message = $"Internal Server Error: {ex.Message}",
                    data = null,
                };
            }
        }

        /// <summary>
        /// Get site name from database
        /// </summary>
        private async Task<string> GetSiteName(string siteId)
        {
            try
            {
                _connection.Open();

                const string query = "SELECT sit_name FROM tbl_m_site WHERE sit_id = @SiteId";

                using var command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@SiteId", siteId);

                var result = await command.ExecuteScalarAsync();
                return result?.ToString();
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        /// <summary>
        /// Get current shift user code based on current time using direct SQL query
        /// </summary>
        public async Task<string> GetCurrentShiftUserName(DateTime currentTime)
        {
            try
            {
                const string query =
                    "SELECT u.usr_name "
                    + "FROM tbl_t_shift s "
                    + "INNER JOIN tbl_m_user u ON s.usr_id = u.usr_id "
                    + "WHERE CAST(@Date AS DATE) >= s.shf_startdate "
                    + "AND CAST(@Date AS DATE) <= s.shf_enddate  "
                    + "AND CAST(@CurrentTime AS TIME) >= s.shf_starttime "
                    + "AND CAST(@CurrentTime AS TIME) <= s.shf_endtime";

                _connection.Open();

                using var command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Date", currentTime.Date);
                command.Parameters.AddWithValue("@CurrentTime", currentTime);

                var result = await command.ExecuteScalarAsync();
                return result?.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting current shift user: {ex.Message}");
                return null;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        /// <summary>
        /// Get current shift user code based on current time using direct SQL query
        /// </summary>
        private async Task<string> GetCurrentShiftUserCode(DateTime currentTime)
        {
            try
            {
                const string query =
                    "SELECT u.usr_code "
                    + "FROM tbl_t_shift s "
                    + "INNER JOIN tbl_m_user u ON s.usr_id = u.usr_id "
                    + "WHERE CAST(@Date AS DATE) >= s.shf_startdate "
                    + "AND CAST(@Date AS DATE) <= s.shf_enddate  "
                    + "AND ("
                    + "(s.shf_starttime <= s.shf_endtime "
                    + "AND CAST(@CurrentTime AS TIME) >= s.shf_starttime "
                    + "AND CAST(@CurrentTime AS TIME) <= s.shf_endtime)"
                    + "OR"
                    + "(s.shf_starttime > s.shf_endtime "
                    + "AND (CAST(@CurrentTime AS TIME) >= s.shf_starttime "
                    + "OR CAST(@CurrentTime AS TIME) <= s.shf_endtime))"
                    + ")";

                _connection.Open();

                using var command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Date", currentTime.Date);
                command.Parameters.AddWithValue("@CurrentTime", currentTime);

                var result = await command.ExecuteScalarAsync();
                return result?.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting current shift user: {ex.Message}");
                return null;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        /// <summary>
        /// Get next ticket sequence number for specific site in specific year
        /// </summary>
        private async Task<int> GetNextTicketSequence(string siteId, string year)
        {
            try
            {
                // Query to find the last sequence number for this site in this year
                // Updated to match new format: [Site Name][DD][MM][YYYY][XXXXX][Shift UserCode]
                const string query =
                    @"
                SELECT MAX(CAST(SUBSTRING(dtm_ticketnumber, 13, 5) AS INT)) as LastSequence
                FROM tbl_t_downtime dt
                INNER JOIN tbl_m_network nt ON dt.net_id = nt.net_id
                WHERE nt.sit_id = @SiteId 
                AND LEN(dt.dtm_ticketnumber) >= 18
                AND SUBSTRING(dtm_ticketnumber, 9, 4) = @Year
                AND ISNUMERIC(SUBSTRING(dtm_ticketnumber, 13, 5)) = 1";

                _connection.Open();
                using var command = new SqlCommand(query, _connection);
                command.Parameters.Add("@SiteId", SqlDbType.VarChar, 50).Value = siteId;
                command.Parameters.Add("@Year", SqlDbType.VarChar, 4).Value = year;

                var result = await command.ExecuteScalarAsync();

                return result == null || result == DBNull.Value ? 1 : Convert.ToInt32(result) + 1;
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        /// <summary>
        /// Calculate total ping time based on settings from tbl_r_setting table
        /// Formula: (PING_TIMEOUT * PING_ATTEMPTS) + (PING_DELAY * (PING_ATTEMPTS - 1))
        /// </summary>
        /// <returns>DtoResponse containing total ping time in milliseconds</returns>
        public async Task<DtoResponse<int>> GetTotalPingTime()
        {
            try
            {
                var settings = await GetPingSettings();

                if (settings == null)
                {
                    return new DtoResponse<int>
                    {
                        status = 404,
                        message = "Ping settings not found in database",
                        data = 0,
                    };
                }

                // Get required setting values
                var pingTimeout = GetSettingValue(settings, "PING_TIMEOUT", 2000); // Default 2 seconds
                var pingAttempts = GetSettingValue(settings, "PING_ATTEMPTS", 10); // Default 10 attempts
                var pingDelay = GetSettingValue(settings, "PING_DELAY", 25); // Default 25ms
                var pingInterval = GetSettingValue(settings, "PING_INTERVAL", 300000); // Default 5 minutes

                // Calculate total ping time
                // Formula: (timeout per ping * number of attempts) + (delay between pings * (attempts - 1))
                var totalPingTime =
                    (pingTimeout * pingAttempts) + (pingDelay * (pingAttempts - 1)) + pingInterval;

                return new DtoResponse<int>
                {
                    status = 200,
                    message =
                        $"Total ping time calculated successfully. Timeout: {pingTimeout}ms, Attempts: {pingAttempts}, Delay: {pingDelay}ms",
                    data = totalPingTime,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<int>
                {
                    status = 500,
                    message = $"Internal Server Error: {ex.Message}",
                    data = 0,
                };
            }
        }

        /// <summary>
        /// Get ping-related settings from tbl_r_setting table
        /// </summary>
        /// <returns>List of SettingViewModel containing ping settings</returns>
        private async Task<List<SettingViewModel>> GetPingSettings()
        {
            try
            {
                const string query =
                    @"
                        SELECT set_id as Id, set_name as Name, set_code as Code, set_value as Value 
                        FROM tbl_r_setting 
                        WHERE set_code IN ('PING_TIMEOUT', 'PING_ATTEMPTS', 'PING_DELAY', 'PING_INTERVAL')";

                _connection.Open();

                var settings = new List<SettingViewModel>();

                using var command = new SqlCommand(query, _connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    settings.Add(
                        new SettingViewModel
                        {
                            Id = reader["Id"].ToString(),
                            Name = reader["Name"].ToString(),
                            Code = reader["Code"].ToString(),
                            Value = reader["Value"].ToString(),
                        }
                    );
                }

                return settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting ping settings: {ex.Message}");
                return new List<SettingViewModel>();
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
            }
        }

        /// <summary>
        /// Get setting value as integer with default fallback
        /// </summary>
        /// <param name="settings">List of settings</param>
        /// <param name="code">Setting code to find</param>
        /// <param name="defaultValue">Default value if setting not found or invalid</param>
        /// <returns>Setting value as integer</returns>
        private int GetSettingValue(List<SettingViewModel> settings, string code, int defaultValue)
        {
            var setting = settings.FirstOrDefault(s => s.Code == code);

            if (setting == null || string.IsNullOrEmpty(setting.Value))
                return defaultValue;

            if (int.TryParse(setting.Value, out int value))
                return value;

            return defaultValue;
        }
    }

    /// <summary>
    /// Class to store ticket decryption information
    /// </summary>
    public class TicketInfo
    {
        public string TicketNumber { get; set; }
        public string SiteCode { get; set; }
        public DateTime TicketDate { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int SequenceNumber { get; set; }
        public string ShiftUserCode { get; set; }
        public string FormattedDate { get; set; }
    }
}
