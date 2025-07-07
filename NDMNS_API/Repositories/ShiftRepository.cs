using Microsoft.Data.SqlClient;
using NDMNS_API.Models;
using NDMNS_API.Responses;
using System.Data;

namespace NDMNS_API.Repositories
{
    public class ShiftRepository
    {
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public ShiftRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        public DtoResponse<List<ShiftViewModel>> GetAllShifts()
        {
            List<ShiftViewModel> shifts = new List<ShiftViewModel>();

            try
            {
                string query =
                    "SELECT s.shf_id, s.usr_id, s.shf_startdate, s.shf_enddate, s.shf_starttime, s.shf_endtime, u.usr_name "
                    + "FROM tbl_t_shift s "
                    + "INNER JOIN tbl_m_user u ON s.usr_id = u.usr_id "
                    + "ORDER BY s.shf_startdate DESC, s.shf_starttime ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ShiftViewModel shift = new ShiftViewModel
                    {
                        Id = reader["shf_id"].ToString(),
                        UserId = reader["usr_id"].ToString(),
                        StartDate = Convert.ToDateTime(reader["shf_startdate"]),
                        EndDate = Convert.ToDateTime(reader["shf_enddate"]),
                        StartTime = (TimeSpan)reader["shf_starttime"],
                        EndTime = (TimeSpan)reader["shf_endtime"],
                        UserName = reader["usr_name"].ToString(),
                    };
                    shifts.Add(shift);
                }
                reader.Close();

                return new DtoResponse<List<ShiftViewModel>>
                {
                    status = 200,
                    message = "Shifts retrieved successfully",
                    data = shifts,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<ShiftViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<ShiftViewModel>>
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

        public DtoResponse<ShiftViewModel> GetShiftById(string id)
        {
            ShiftViewModel shift = null;

            try
            {
                string query =
                    "SELECT s.shf_id, s.usr_id, s.shf_startdate, s.shf_enddate, s.shf_starttime, s.shf_endtime, u.usr_name "
                    + "FROM tbl_t_shift s "
                    + "INNER JOIN tbl_m_user u ON s.usr_id = u.usr_id "
                    + "WHERE s.shf_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    shift = new ShiftViewModel
                    {
                        Id = reader["shf_id"].ToString(),
                        UserId = reader["usr_id"].ToString(),
                        StartDate = Convert.ToDateTime(reader["shf_startdate"]),
                        EndDate = Convert.ToDateTime(reader["shf_enddate"]),
                        StartTime = (TimeSpan)reader["shf_starttime"],
                        EndTime = (TimeSpan)reader["shf_endtime"],
                        UserName = reader["usr_name"].ToString(),
                    };
                }
                reader.Close();

                if (shift == null)
                {
                    return new DtoResponse<ShiftViewModel>
                    {
                        status = 404,
                        message = "Shift not found",
                        data = null,
                    };
                }

                return new DtoResponse<ShiftViewModel>
                {
                    status = 200,
                    message = "Shift retrieved successfully",
                    data = shift,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<ShiftViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<ShiftViewModel>
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

        public DtoResponse<List<ShiftViewModel>> GetShiftsByUserId(string userId)
        {
            List<ShiftViewModel> shifts = new List<ShiftViewModel>();

            try
            {
                string query =
                    "SELECT s.shf_id, s.usr_id, s.shf_startdate, s.shf_enddate, s.shf_starttime, s.shf_endtime, u.usr_name "
                    + "FROM tbl_t_shift s "
                    + "INNER JOIN tbl_m_user u ON s.usr_id = u.usr_id "
                    + "WHERE s.usr_id = @UserId "
                    + "ORDER BY s.shf_startdate DESC, s.shf_starttime ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@UserId", userId);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ShiftViewModel shift = new ShiftViewModel
                    {
                        Id = reader["shf_id"].ToString(),
                        UserId = reader["usr_id"].ToString(),
                        StartDate = Convert.ToDateTime(reader["shf_startdate"]),
                        EndDate = Convert.ToDateTime(reader["shf_enddate"]),
                        StartTime = (TimeSpan)reader["shf_starttime"],
                        EndTime = (TimeSpan)reader["shf_endtime"],
                        UserName = reader["usr_name"].ToString(),
                    };
                    shifts.Add(shift);
                }
                reader.Close();

                return new DtoResponse<List<ShiftViewModel>>
                {
                    status = 200,
                    message = "Shifts retrieved successfully",
                    data = shifts,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<ShiftViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<ShiftViewModel>>
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

        public DtoResponse<List<ShiftViewModel>> GetShiftsByDateRange(
            DateTime startDate,
            DateTime endDate
        )
        {
            List<ShiftViewModel> shifts = new List<ShiftViewModel>();

            try
            {
                string query =
                    "SELECT s.shf_id, s.usr_id, s.shf_startdate, s.shf_enddate, s.shf_starttime, s.shf_endtime, u.usr_name "
                    + "FROM tbl_t_shift s "
                    + "INNER JOIN tbl_m_user u ON s.usr_id = u.usr_id "
                    + "WHERE s.shf_startdate >= @StartDate AND s.shf_enddate <= @EndDate "
                    + "ORDER BY s.shf_startdate DESC, s.shf_starttime ASC";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@StartDate", startDate.Date);
                command.Parameters.AddWithValue("@EndDate", endDate.Date);
                _connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ShiftViewModel shift = new ShiftViewModel
                    {
                        Id = reader["shf_id"].ToString(),
                        UserId = reader["usr_id"].ToString(),
                        StartDate = Convert.ToDateTime(reader["shf_startdate"]),
                        EndDate = Convert.ToDateTime(reader["shf_enddate"]),
                        StartTime = (TimeSpan)reader["shf_starttime"],
                        EndTime = (TimeSpan)reader["shf_endtime"],
                        UserName = reader["usr_name"].ToString(),
                    };
                    shifts.Add(shift);
                }
                reader.Close();

                return new DtoResponse<List<ShiftViewModel>>
                {
                    status = 200,
                    message = "Shifts retrieved successfully",
                    data = shifts,
                };
            }
            catch (SqlException)
            {
                return new DtoResponse<List<ShiftViewModel>>
                {
                    status = 500,
                    message = "SQL Error: Failed to retrieve data",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<ShiftViewModel>>
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

        public DtoResponse<ShiftViewModel> AddShift(Shift shift, string createdBy)
        {
            try
            {
                var shiftDuration =
                    shift.EndTime >= shift.StartTime
                        ? shift.EndTime - shift.StartTime
                        : TimeSpan.FromDays(1) - shift.StartTime + shift.EndTime;
                if (shiftDuration.TotalHours != 8)
                {
                    return new DtoResponse<ShiftViewModel>
                    {
                        status = 400,
                        message = "Shift duration must be exactly 8 hours",
                        data = null,
                    };
                }

                var shiftPeriod = (shift.EndDate - shift.StartDate).Days + 1;
                if (shiftPeriod != 7)
                {
                    return new DtoResponse<ShiftViewModel>
                    {
                        status = 400,
                        message = "Shift period must be exactly 7 days (1 week)",
                        data = null,
                    };
                }

                if (!IsUserExists(shift.UserId))
                {
                    return new DtoResponse<ShiftViewModel>
                    {
                        status = 404,
                        message = "User not found",
                        data = null,
                    };
                }

                if (
                    IsShiftTimeConflict(
                        shift.UserId,
                        shift.StartDate,
                        shift.EndDate,
                        shift.StartTime,
                        shift.EndTime
                    )
                )
                {
                    return new DtoResponse<ShiftViewModel>
                    {
                        status = 409,
                        message =
                            "Shift time conflict detected. User already has a shift in this time period.",
                        data = null,
                    };
                }

                string newId = Guid.NewGuid().ToString();
                string query =
                    "INSERT INTO tbl_t_shift (shf_id, usr_id, shf_startdate, shf_enddate, shf_starttime, shf_endtime, shf_createdby, shf_createddate) "
                    + "VALUES (@Id, @UserId, @StartDate, @EndDate, @StartTime, @EndTime, @CreatedBy, @CreatedDate)";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", newId);
                command.Parameters.AddWithValue("@UserId", shift.UserId);
                command.Parameters.AddWithValue("@StartDate", shift.StartDate.Date);
                command.Parameters.AddWithValue("@EndDate", shift.EndDate.Date);
                // command.Parameters.AddWithValue("@StartTime", shift.StartTime);
                command.Parameters.Add("@StartTime", SqlDbType.Time).Value = shift.StartTime;
                // command.Parameters.AddWithValue("@EndTime", shift.EndTime);
                command.Parameters.Add("@EndTime", SqlDbType.Time).Value = shift.EndTime;
                command.Parameters.AddWithValue("@CreatedBy", createdBy);
                command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    shift.Id = newId;
                    var shiftViewModel = new ShiftViewModel
                    {
                        Id = newId,
                        UserId = shift.UserId,
                        StartDate = shift.StartDate,
                        EndDate = shift.EndDate,
                        StartTime = shift.StartTime,
                        EndTime = shift.EndTime,
                        UserName = "User not found",
                    };

                    return new DtoResponse<ShiftViewModel>
                    {
                        status = 201,
                        message = "Shift data saved successfully",
                        data = shiftViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<ShiftViewModel>
                    {
                        status = 500,
                        message = "Failed to save data Shift",
                        data = null,
                    };
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                return new DtoResponse<ShiftViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to save data Shift",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<ShiftViewModel>
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

        public DtoResponse<ShiftViewModel> UpdateShift(Shift shift, string updatedBy)
        {
            try
            {
                var shiftDuration = shift.EndTime - shift.StartTime;
                if (shiftDuration.TotalHours != 8)
                {
                    return new DtoResponse<ShiftViewModel>
                    {
                        status = 400,
                        message = "Shift duration must be exactly 8 hours",
                        data = null,
                    };
                }

                var shiftPeriod = (shift.EndDate - shift.StartDate).Days + 1;
                if (shiftPeriod != 7)
                {
                    return new DtoResponse<ShiftViewModel>
                    {
                        status = 400,
                        message = "Shift period must be exactly 7 days (1 week)",
                        data = null,
                    };
                }

                if (!IsUserExists(shift.UserId))
                {
                    return new DtoResponse<ShiftViewModel>
                    {
                        status = 404,
                        message = "User not found",
                        data = null,
                    };
                }

                if (
                    IsShiftTimeConflict(
                        shift.UserId,
                        shift.StartDate,
                        shift.EndDate,
                        shift.StartTime,
                        shift.EndTime,
                        shift.Id
                    )
                )
                {
                    return new DtoResponse<ShiftViewModel>
                    {
                        status = 409,
                        message =
                            "Shift time conflict detected. User already has a shift in this time period.",
                        data = null,
                    };
                }

                string query =
                    "UPDATE tbl_t_shift "
                    + "SET usr_id = @UserId, shf_startdate = @StartDate, shf_enddate = @EndDate, shf_starttime = @StartTime, shf_endtime = @EndTime, shf_updatedby = @UpdatedBy, shf_updateddate = @UpdatedDate "
                    + "WHERE shf_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", shift.Id);
                command.Parameters.AddWithValue("@UserId", shift.UserId);
                command.Parameters.AddWithValue("@StartDate", shift.StartDate.Date);
                command.Parameters.AddWithValue("@EndDate", shift.EndDate.Date);
                command.Parameters.AddWithValue("@StartTime", shift.StartTime);
                command.Parameters.AddWithValue("@EndTime", shift.EndTime);
                command.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    var shiftViewModel = new ShiftViewModel
                    {
                        Id = shift.Id,
                        UserId = shift.UserId,
                        StartDate = shift.StartDate,
                        EndDate = shift.EndDate,
                        StartTime = shift.StartTime,
                        EndTime = shift.EndTime,
                        UserName = "User not found",
                    };

                    return new DtoResponse<ShiftViewModel>
                    {
                        status = 200,
                        message = "Shift data saved successfully",
                        data = shiftViewModel,
                    };
                }
                else
                {
                    return new DtoResponse<ShiftViewModel>
                    {
                        status = 404,
                        message = "Shift not found",
                        data = null,
                    };
                }
            }
            catch (SqlException)
            {
                return new DtoResponse<ShiftViewModel>
                {
                    status = 500,
                    message = "SQL Error: Failed to update Shift",
                    data = null,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<ShiftViewModel>
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

        public DtoResponse<object> DeleteShift(string id)
        {
            try
            {
                string query = "DELETE FROM tbl_t_shift WHERE shf_id = @Id";

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@Id", id);

                _connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return new DtoResponse<object>
                    {
                        status = 200,
                        message = "Shift data deleted successfully",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 404,
                        message = "Shift not found or already deleted",
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
                            "Cannot delete Shift because it is referenced by existing records. Please remove all associated records first.",
                        data = null,
                    };
                }
                else
                {
                    return new DtoResponse<object>
                    {
                        status = 500,
                        message = "SQL Error: Failed to delete Shift",
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

        public bool IsUserExists(string userId)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM tbl_m_user WHERE usr_id = @UserId";
                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@UserId", userId);

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

        public bool IsShiftTimeConflict(
            string userId,
            DateTime startDate,
            DateTime endDate,
            TimeSpan startTime,
            TimeSpan endTime,
            string? excludeShiftId = null
        )
        {
            try
            {
                string query =
                    @"
                    SELECT COUNT(*) FROM tbl_t_shift 
                    WHERE usr_id = @UserId 
                    AND (
                        (shf_startdate <= @EndDate AND shf_enddate >= @StartDate)
                    )
                    AND (
                        (shf_starttime < @EndTime AND shf_endtime > @StartTime)
                        OR
                        (shf_starttime > shf_endtime AND (@StartTime > @EndTime OR shf_starttime <= @EndTime OR shf_endtime >= @StartTime))
                    )";

                if (!string.IsNullOrEmpty(excludeShiftId))
                {
                    query += " AND shf_id != @ExcludeShiftId";
                }

                SqlCommand command = new SqlCommand(query, _connection);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@StartDate", startDate.Date);
                command.Parameters.AddWithValue("@EndDate", endDate.Date);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@EndTime", endTime);

                if (!string.IsNullOrEmpty(excludeShiftId))
                {
                    command.Parameters.AddWithValue("@ExcludeShiftId", excludeShiftId);
                }

                _connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking shift conflict: {ex.Message}");
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
