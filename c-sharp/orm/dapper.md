### Inner joins in Dapper with Nested Objects
```csharp
 public async Task<PagedResults<Order>> GetLastNOrdersAsync(int? Page = 1, int? PageSize = 20)
 {
     var result = new PagedResults<Order>();
     if (Page is null)
     {
         Page = 1;
         _logger.LogInformation("Received Page as null. So Initializing Page={Page}", Page);
     }
     if (PageSize is null)
     {
         PageSize = 20;
         _logger.LogInformation("Received PageSize as null. So Initializing PageSize={PageSize}", PageSize);
     }

     string sql = "select " +
         "TS.Shipment_Code, TS.Consignor_Contact, TS.Consignor_CoName, TS.Consignor_Addr1, TS.Consignor_Addr2, TS.Consignor_Addr3, TS.Consignor_City, TS.Consignor_State, TS.Consignor_Country, TS.Consignee_Contact, TS.Consignee_CoName, TS.Consignee_Addr1, TS.Consignee_Addr2, TS.Consignee_Addr3, TS.Consignee_City, TS.Consignee_State, TS.Consignee_Country, TS.CreatedDate, TS.CarrierAWB, TS.Carrier_Code" +
         ",M.Customer_Code, M.Cust_CoName,M.ContactEmail" +
         ",MWU.First_Name, MWU.Last_Name, MWU.Customer_ID from T_Shipment TS INNER JOIN M_Customer M on M.Customer_Code = TS.Account_Number INNER JOIN M_WebUsers MWU on MWU.Customer_ID = M.Customer_Code WHERE TS.Shipment_Status > 1 and ISNUMERIC(TS.Invoice_Number)=1 and MWU.Account_Type='Cust' order by TS.CreatedDate DESC" +
         " OFFSET @Offset ROWS" +
         " FETCH NEXT @PageSize ROWS ONLY;" +
         "SELECT 100";


     var multi = await _db.QueryMultipleAsync(sql, new
     {
         Offset = (Page - 1) * PageSize,
         PageSize = PageSize
     });

     result.Items = multi.Read<T_Shipment, M_Customer, M_WebUsers, Order>(
         (shipment, customer, mwu) =>
         {
             return new Order
             {
                 Id = shipment.Shipment_Code,
                 Customer = new Customer
                 {
                     Id = customer.Customer_Code,
                     Name = customer.Cust_CoName == "" ? mwu.First_Name + "" + mwu.Last_Name : customer.Cust_CoName,
                     Email = customer.ContactEmail,
                     CompanyName = customer.Cust_CoName
                 },
                 SenderAddress = new Address
                 {
                     Name = shipment.Consignor_Contact,
                     CompanyName = shipment.Consignor_CoName,
                     AddressLine1 = shipment.Consignor_Addr1,
                     AddressLine2 = shipment.Consignor_Addr2,
                     AddressLine3 = shipment.Consignor_Addr3,
                     City = shipment.Consignor_City,
                     State = shipment.Consignor_State,
                     Country = shipment.Consignor_Country
                 },
                 ReceiverAddress = new Address
                 {
                     Name = shipment.Consignee_Contact,
                     CompanyName = shipment.Consignee_CoName,
                     AddressLine1 = shipment.Consignee_Addr1,
                     AddressLine2 = shipment.Consignee_Addr2,
                     AddressLine3 = shipment.Consignee_Addr3,
                     City = shipment.Consignee_City,
                     State = shipment.Consignee_State,
                     Country = shipment.Consignee_Country
                 },
                 CarrierDetails = GetCarrierById(shipment.Carrier_Code),
                 Name = shipment.CarrierAWB ?? "",
                 OrderAmount = _rnd.Next(30, 200),
                 OrderTime = shipment.CreatedDate
             };
         },
          splitOn: "Customer_Code,Customer_ID", buffered: true
         );

     result.TotalCount = multi.ReadFirst<long>();
     return result;
 }

```
