using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

public class EndOfShiftReportService
{
    private readonly string _connectionString;

    public EndOfShiftReportService(string connectionString)
    {
        _connectionString = connectionString;
    }
}