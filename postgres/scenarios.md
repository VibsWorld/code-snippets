* Insert only if NOT exists
```sql
INSERT INTO system_incident_types(id, opt_in, opt_out)
VALUES ('Cancellation', 'True', 'False')
ON CONFLICT (id) DO NOTHING;
```
