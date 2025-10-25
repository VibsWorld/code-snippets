//Poco Generator
//Generate CRUD Operations from Tablename

 public class SQLTablesSchema
 {
     /*
      Query Used
     SELECT c.column_id,c.NAME,c.[object_id],c.system_type_id,tt.[name] AS TypeName,c.max_length,c.PRECISION,c.is_nullable,c.is_identity, c.is_rowguidcol FROM sys.columns C               INNER JOIN sys.tables t
                    ON t.object_id = c.object_id
            INNER JOIN sys.types tt
                    ON tt.system_type_id = c.system_type_id
     WHERE  t.NAME = @TableName
      */

     public int column_id { get; set; }
     public string name { get; set; } = "";
     public long object_id { get; set; }
     public short system_type_id { get; set; }
     public string TypeName { get; set; } = "";
     public int max_length { get; set; }
     public short precision { get; set; }
     public short scale { get; set; }
     public bool is_nullable { get; set; }
     public bool is_identity { get; set; }
     public bool is_rowguidcol { get; set; }
     
 }

 public async Task CreateDapperInsertMethodsFromTableSqlColumnsGeneratedFromSchema()
 {
     string tableName = "MerchantInfo";
     string dbInstance = "_db";
     string tableClassName = tableName;
     var resultObjectMerchanInfo = await _db.QueryAsync<SQLTablesSchema>(@"SELECT c.column_id,c.NAME,c.[object_id],c.system_type_id,tt.[name] AS TypeName,c.max_length,c.PRECISION,c.is_nullable,c.is_identity, c.is_rowguidcol FROM sys.columns C INNER JOIN sys.tables t                       ON t.object_id = c.object_id INNER JOIN sys.types tt ON tt.system_type_id = c.system_type_id WHERE  t.NAME = @TableName", new
         {
             TableName = tableName
         });
     AssertionOptions.FormattingOptions.MaxDepth = 100;

     resultObjectMerchanInfo.Should().NotBeNull();
     StringBuilder sb = new StringBuilder();
     StringBuilder sbInsertQuery = new StringBuilder();
     StringBuilder sbInsertQueryValues = new StringBuilder();
     string inputObjectName = $"inputObj{tableClassName}";
     sb.AppendLine($@"public async Task<{tableClassName}> Insert{tableClassName}({tableClassName} {inputObjectName})" + "\n{");
     sb.AppendLine("DynamicParameters _params = new DynamicParameters();");
     string identityColumn = string.Empty;
     int itemIndex = 0, intLastItemIndex = resultObjectMerchanInfo.Count();
     sbInsertQuery.AppendLine($"INSERT INTO {tableName}");
    
     foreach (var item in resultObjectMerchanInfo) 
     {
         itemIndex = itemIndex + 1;

         if (item is null) throw new NullReferenceException();
         if (item.TypeName is null) throw new NullReferenceException();

         if (item.is_identity && string.IsNullOrWhiteSpace(identityColumn))
         {
             identityColumn = item.name;
             sbInsertQueryValues.Insert(0,Environment.NewLine + "OUTPUT INSERTED.*" + Environment.NewLine);
             continue;
         }

         if (itemIndex == 1)
         {
             sbInsertQuery.Append($"({item.name}");
             sbInsertQueryValues.Append($" VALUES (@{item.name}");
         }
         else if (itemIndex < intLastItemIndex)
         {
             sbInsertQuery.Append($",{item.name}");
             sbInsertQueryValues.Append($",@{item.name}");
         }
         else
         {
             sbInsertQuery.Append($"{item.name})");
             sbInsertQueryValues.Append($",@{item.name})");
         }

         if (item.TypeName.Equals("uniqueidentifier"))
         {
             sb.AppendLine($@"_params.Add(""@{item.name}"", {inputObjectName}.{item.name} ?? Guid.NewGuid(), DbType.Guid, ParameterDirection.Input);");
         }
         else if (item.TypeName.Equals("tinyint"))
         {
             sb.AppendLine($@"_params.Add(""@{item.name}"", {inputObjectName}.{item.name}, DbType.Int16, ParameterDirection.Input);");
         }
         else if (item.TypeName.Equals("int"))
         {
             sb.AppendLine($@"_params.Add(""@{item.name}"", {inputObjectName}.{item.name}, DbType.Int32, ParameterDirection.Input);");
         }
         else if (item.TypeName.Equals("bigint"))
         {
             sb.AppendLine($@"_params.Add(""@{item.name}"", {inputObjectName}.{item.name}, DbType.Int64, ParameterDirection.Input);");
         }
         else if (item.TypeName.Equals("bit"))
         {
             sb.AppendLine($@"_params.Add(""@{item.name}"", {inputObjectName}.{item.name}, DbType.Boolean, ParameterDirection.Input);");
         }
         else if (item.TypeName.Equals("decimal"))
         {
             sb.AppendLine($@"_params.Add(""@{item.name}"", {inputObjectName}.{item.name}, DbType.Decimal, ParameterDirection.Input, {item.precision},{item.scale});");
         }
         else if (item.TypeName.Equals("char"))
         {
             sb.AppendLine($@"_params.Add(""@{item.name}"", {inputObjectName}.{item.name}, DbType.AnsiStringFixedLength, ParameterDirection.Input, {(item.max_length < 0 ? "Int32.MaxValue" : item.max_length.ToString())});");
         }
         else if (item.TypeName.Equals("varchar"))
         {
             sb.AppendLine($@"_params.Add(""@{item.name}"", {inputObjectName}.{item.name}, DbType.AnsiString, ParameterDirection.Input, {(item.max_length < 0 ? "Int32.MaxValue" : item.max_length.ToString())});");
         }
         else if (item.TypeName.Equals("nvarchar"))
         {
             sb.AppendLine($@"_params.Add(""@{item.name}"", {inputObjectName}.{item.name}, DbType.String, ParameterDirection.Input, {(item.max_length < 0 ? "Int32.MaxValue" : item.max_length.ToString())});");
         }
         else if (item.TypeName.Equals("sysname"))
         {
             sb.AppendLine($@"_params.Add(""@{item.name}"", {inputObjectName}.{item.name}, DbType.String, ParameterDirection.Input, {(item.max_length < 0 ? "Int32.MaxValue" : item.max_length.ToString())});");
         }
         else if (item.TypeName.Equals("nchar"))
         {
             sb.AppendLine($@"_params.Add(""@{item.name}"", {inputObjectName}.{item.name}, DbType.StringFixedLength, ParameterDirection.Input, {(item.max_length < 0 ? "Int32.MaxValue" : item.max_length.ToString())});");
         }
         else if (item.TypeName.Contains("date"))
         {
             sb.AppendLine($@"_params.Add(""@{item.name}"", {inputObjectName}.{item.name} ?? DateTime.Now, DbType.DateTime, ParameterDirection.Input);");
         }
         else
         {
             sb.AppendLine($@"_params.Add(""@{item.name})""");
         }
     }

     sb.AppendLine($@"{tableClassName} obj = await {dbInstance}.QuerySingleAsync<{tableClassName}>(@""{sbInsertQuery.ToString() + sbInsertQueryValues.ToString()}"",_params);");

     sb.AppendLine($"return obj;");


     sb.AppendLine("}");

     _logger.LogInformation(sb.ToString());
     //_logger.LogInformation(sbInsertQuery.ToString() + sbInsertQueryValues.ToString());
     //_logger.LogObject(resultObjectMerchanInfo);
 }
