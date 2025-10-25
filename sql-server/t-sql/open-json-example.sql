declare @json varchar(max) = '[
  {
    "InvoiceNumber": 3333428,
    "PaidAmount": 10.0,
    "Bank": "Bank Name 1",
    "PaymentReference": "PaymentReference 1"
  },
  {
    "InvoiceNumber": 2457759,
    "PaidAmount": 15.0,
    "Bank": "Bank Name 2",
    "PaymentReference": "PaymentReference 2"
  }
]';

SELECT * FROM OPENJSON(@json) 
            WITH (InvoiceNumber BIGINT,
                PaidAmount DECIMAL(18,2),
                Bank VARCHAR(255),
                PaymentReference VARCHAR(512)
                ) as Invoices
