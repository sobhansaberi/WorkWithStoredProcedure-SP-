using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Application.Services;

public class TransactionReportService : ITransactionReportService
{
    private readonly AppDbContext _dbContext;
    private readonly IDbConnection _dbConnection;

    public TransactionReportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbConnection = dbContext.Database.GetDbConnection();
    }

    public async Task<MultiResult<TotalTransactionReportResponseDto, TotalSumTransactionReportResponseDto>> TotalTransactionReport(TotalTransactionReportRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@terminalNumber", request.TerminalNumber);
        parameters.Add("@FromDate", request.FromDate);
        parameters.Add("@ToDate", request.ToDate);

        List<TotalTransactionReportResponseDto> data;
        MultiResult<TotalTransactionReportResponseDto, TotalSumTransactionReportResponseDto> multiData;
        if (!string.IsNullOrEmpty(request.TerminalNumber)
            && !string.IsNullOrWhiteSpace(request.TerminalNumber))
        {
            data = (await _dbConnection.QueryAsync<TotalTransactionReportResponseDto>(
                    "USP_RPT_TransactionStatus",
                    parameters,
        commandType: CommandType.StoredProcedure,
                    commandTimeout: _dbConnection.ConnectionTimeout)
                .ConfigureAwait(false)).ToList();
            multiData = new MultiResult<TotalTransactionReportResponseDto, TotalSumTransactionReportResponseDto>
            {
                Output1 = data,
                Output2 = null
            };
        }
        else
        {
            multiData = await MultipleResult
                .ReadMultipleAsync<TotalTransactionReportResponseDto, TotalSumTransactionReportResponseDto>(_dbConnection,
                "USP_RPT_TransactionStatus",
                parameters,
                CommandType.StoredProcedure);
        }

        return multiData;
    }
}
