using System;
using System.Data;
using System.Data.SqlClient;
using System.Security;
using System.Transactions;

public class DatabaseAccess
{
    // Region class variables
    private SqlCommand _command;
    private SqlConnection _connection;
    private SqlDataAdapter _dataAdapter;
    private DataTable _dataTable;
    private DataSet _dataSet;
    private IDataReader _reader;

    // Constructor
    public DatabaseAccess()
    {
        _command = new SqlCommand();
        _connection = new SqlConnection();
        _dataAdapter = new SqlDataAdapter();
        _dataTable = new DataTable();
        _dataSet = new DataSet();
    }

    private void InitSQL(string connectionName = "", string userPassword = "")
    {
        if (!string.IsNullOrEmpty(connectionName))
        {
            connectionName = "_" + connectionName;
        }

        var connectionString = new SqlConnectionStringBuilder
        {
            DataSource = Properties.Settings.Default.data_source_bd,
            InitialCatalog = Properties.Settings.Default.initial_catalog_bd,
            IntegratedSecurity = Properties.Settings.Default.integrated_security_bd,
            PersistSecurityInfo = Properties.Settings.Default.persist_security_info_bd,
            TrustServerCertificate = Properties.Settings.Default.trust_server_certificate_bd
        };

        SecureString passwordSecure;
        switch (userPassword)
        {
            case "somepassword":
                connectionString.IntegratedSecurity = Properties.Settings.Default.IntegratedSecurity;
                passwordSecure = null;
                break;
            case "":
                passwordSecure = ConvertToSecureString(Properties.Settings.Default.password_bd);
                break;
            default:
                passwordSecure = ConvertToSecureString(userPassword);
                break;
        }

        if (userPassword == "somepassword")
        {
            _connection = new SqlConnection(connectionString.ConnectionString);
        }
        else
        {
            var credential = new SqlCredential(Properties.Settings.Default.user_bd, passwordSecure);
            _connection = new SqlConnection(connectionString.ConnectionString, credential);
        }

        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }
    }

    private void CloseSQL()
    {
        if (_connection.State == ConnectionState.Open)
        {
            _connection.Dispose();
            _connection.Close();
        }
    }

    /// <summary>
    /// Checks the database connection
    /// </summary>
    /// <param name="connectionName">Optional connection name for multiple connections</param>
    /// <param name="userPassword">Optional password override</param>
    /// <returns>True if connection successful, false otherwise</returns>
    public bool CheckConnection(string connectionName = "", string userPassword = "")
    {
        bool connected = false;
        try
        {
            InitSQL(connectionName, userPassword);
            if (_connection.State == ConnectionState.Open)
            {
                connected = true;
            }
        }
        catch (Exception)
        {
            connected = false;
        }
        finally
        {
            CloseSQL();
        }
        return connected;
    }

    /// <summary>
    /// Executes a non-query SQL command (INSERT, UPDATE, DELETE)
    /// </summary>
    /// <param name="sqlCommand">SQL command to execute</param>
    /// <param name="connectionName">Optional connection name</param>
    /// <param name="userPassword">Optional password override</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool ExecuteNonQuery(string sqlCommand, string connectionName = "", string userPassword = "")
    {
        bool success = false;
        try
        {
            InitSQL(connectionName, userPassword);
            _command = new SqlCommand(sqlCommand, _connection)
            {
                CommandTimeout = Properties.Settings.Default.timeout_bd,
                CommandType = CommandType.Text
            };
            _command.ExecuteNonQuery();
            success = true;
        }
        catch (SqlException ex)
        {
            throw new Exception($"Database error: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"General error: {ex.Message}");
        }
        finally
        {
            CloseSQL();
            _command?.Dispose();
        }
        return success;
    }

    /// <summary>
    /// Executes a query that returns a single value
    /// </summary>
    /// <param name="sqlCommand">SQL command to execute</param>
    /// <param name="connectionName">Optional connection name</param>
    /// <param name="userPassword">Optional password override</param>
    /// <returns>Integer value or -1000 if no result</returns>
    public int ExecuteScalar(string sqlCommand, string connectionName = "", string userPassword = "")
    {
        int returnValue = -1000;
        try
        {
            InitSQL(connectionName, userPassword);
            _command = new SqlCommand(sqlCommand, _connection)
            {
                CommandTimeout = Properties.Settings.Default.timeout_bd,
                CommandType = CommandType.Text
            };
            _reader = _command.ExecuteReader();

            if (_reader.Read() && !_reader.IsDBNull(0))
            {
                returnValue = Convert.ToInt32(_reader[0]);
            }
        }
        catch (SqlException ex)
        {
            throw new Exception($"Database error: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"General error: {ex.Message}");
        }
        finally
        {
            _reader?.Dispose();
            CloseSQL();
            _command?.Dispose();
        }
        return returnValue;
    }

    /// <summary>
    /// Executes a query that returns multiple rows
    /// </summary>
    /// <param name="sqlCommand">SQL command to execute</param>
    /// <param name="connectionName">Optional connection name</param>
    /// <param name="userPassword">Optional password override</param>
    /// <returns>DataTable containing the results</returns>
    public DataTable ExecuteQuery(string sqlCommand, string connectionName = "", string userPassword = "")
    {
        try
        {
            InitSQL(connectionName, userPassword);
            _command = new SqlCommand(sqlCommand, _connection)
            {
                CommandTimeout = Properties.Settings.Default.timeout_bd,
                CommandType = CommandType.Text
            };

            _dataAdapter = new SqlDataAdapter(_command);
            _dataTable = new DataTable();
            _dataAdapter.Fill(_dataTable);
            return _dataTable;
        }
        catch (SqlException ex)
        {
            throw new Exception($"Database error: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"General error: {ex.Message}");
        }
        finally
        {
            CloseSQL();
            _command?.Dispose();
            _dataAdapter?.Dispose();
        }
    }

    /// <summary>
    /// Executes a stored procedure
    /// </summary>
    /// <param name="procedureName">Name of the stored procedure</param>
    /// <param name="parameters">Array of SqlParameters</param>
    /// <param name="connectionName">Optional connection name</param>
    /// <param name="userPassword">Optional password override</param>
    /// <returns>DataTable containing the results</returns>
    public DataTable ExecuteStoredProcedure(string procedureName, SqlParameter[] parameters = null, string connectionName = "", string userPassword = "")
    {
        try
        {
            InitSQL(connectionName, userPassword);
            _command = new SqlCommand(procedureName, _connection)
            {
                CommandTimeout = Properties.Settings.Default.timeout_bd,
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null)
            {
                _command.Parameters.AddRange(parameters);
            }

            _dataAdapter = new SqlDataAdapter(_command);
            _dataTable = new DataTable();
            _dataAdapter.Fill(_dataTable);
            return _dataTable;
        }
        catch (SqlException ex)
        {
            throw new Exception($"Database error: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"General error: {ex.Message}");
        }
        finally
        {
            CloseSQL();
            _command?.Dispose();
            _dataAdapter?.Dispose();
        }
    }

    /// <summary>
    /// Bulk copies data to a specified table
    /// </summary>
    public bool BulkCopy(string tableName, string connectionName = "", string userPassword = "")
    {
        try
        {
            InitSQL(connectionName, userPassword);

            using (var copy = new SqlBulkCopy(_connection))
            {
                // Map columns
                for (int i = 0; i < _dataTable.Columns.Count; i++)
                {
                    copy.ColumnMappings.Add(i, i);
                }

                copy.DestinationTableName = tableName;
                copy.WriteToServer(_dataTable);
            }

            return true;
        }
        catch (SqlException ex)
        {
            throw new Exception($"Database error: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"General error: {ex.Message}");
        }
        finally
        {
            CloseSQL();
        }

        return false;
    }

    /// <summary>
    /// Executes multiple SQL commands within a transaction
    /// </summary>
    public bool ExecuteTransaction(string sql1, string sql2 = "", string sql3 = "", string sql4 = "", 
        string connectionName = "", string userPassword = "")
    {
        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.Serializable
        };

        using (var scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions))
        {
            try
            {
                InitSQL(connectionName, userPassword);

                _command = new SqlCommand
                {
                    Connection = _connection,
                    CommandTimeout = Properties.Settings.Default.timeout_bd,
                    CommandType = CommandType.Text
                };

                // Execute first query (mandatory)
                _command.CommandText = sql1;
                using (var reader = _command.ExecuteReader())
                {
                    reader.Close();
                }

                // Execute second query (optional)
                if (!string.IsNullOrEmpty(sql2))
                {
                    _command.CommandText = sql2;
                    using (var reader = _command.ExecuteReader())
                    {
                        reader.Close();
                    }

                    // Execute third query (optional)
                    if (!string.IsNullOrEmpty(sql3))
                    {
                        _command.CommandText = sql3;
                        using (var reader = _command.ExecuteReader())
                        {
                            reader.Close();
                        }

                        // Execute fourth query (optional)
                        if (!string.IsNullOrEmpty(sql4))
                        {
                            _command.CommandText = sql4;
                            using (var reader = _command.ExecuteReader())
                            {
                                reader.Close();
                            }
                        }
                    }
                }

                scope.Complete();
                return true;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"General error: {ex.Message}");
            }
            finally
            {
                CloseSQL();
                _command?.Dispose();
            }
        }

        return false;
    }

    private SecureString ConvertToSecureString(string password)
    {
        var secure = new SecureString();
        foreach (char c in password)
        {
            secure.AppendChar(c);
        }
        secure.MakeReadOnly();
        return secure;
    }

    // Implement IDisposable pattern
    public void Dispose()
    {
        _command?.Dispose();
        _connection?.Dispose();
        _dataAdapter?.Dispose();
        _dataTable?.Dispose();
        _dataSet?.Dispose();
        _reader?.Dispose();
    }
}
