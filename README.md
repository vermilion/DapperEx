# DapperEx
My improved version of great Dapper library https://github.com/StackExchange/dapper-dot-net

Usage:

```csharp

public class CwwDataAccess : DapperExProvider
{
    public CwwDataAccess()
        : base("Data Source=localhost...")
    {
    }

    public IEnumerable<Data> Get()
    {
        return WithConnection(db =>
        {
            return db.Query<Data>("GetAll", null, commandType: CommandType.StoredProcedure);
        });
    }
}
        
```

+ Extended QueryMultiple mapper
```csharp
    public async Task<IEnumerable<Data>> GetAllMappedAsync()
    {
        return await WithConnection(async db =>
        {
            using (var gridReader = await db.QueryMultipleAsync("GetAll"))
            {
                var result = gridReader
                    .StartMap(new DataMapper<Data>(x => x.Id)
                        .OneToOne<Status>((vp, r) => vp.Status = r, x => x.StatusId)
                    )
                    .NextMultiple(...) //And so on
                    .EndMap();

                return result;
            }
        });
    }    
```
