using Dapper;
using System.Data;
using System.Data.Common;

namespace Application.Common.Utilities;

public static class MultipleResult
{
    public static async Task<MultiResult<TEntity, T>> ReadMultipleAsync<TEntity, T>(IDbConnection dbConnection, string query, DynamicParameters parameters, CommandType type)
        where T : class, new()
    {
        var result = await dbConnection.QueryMultipleAsync(query, parameters, commandType: type, commandTimeout: dbConnection.ConnectionTimeout).ConfigureAwait(false);
        var output1 = new List<TEntity>();
        var output2 = new T();
        while (!result.IsConsumed)
        {
            var entities1 = (await result.ReadAsync<TEntity>()).ToList();
            var entities2 = (await result.ReadAsync<T>()).FirstOrDefault();
            if (entities1 != null && entities1.Any())
            {
                output1.AddRange(entities1);
            }
            output2 = entities2;
        }
        var functionResult = new MultiResult<TEntity, T>
        {
            Output1 = output1,
            Output2 = output2
        };
        return functionResult;
    }
}
public class MultiResult<TEntity, T>
{
    public List<TEntity>? Output1 { get; set; }
    public T? Output2 { get; set; }
}