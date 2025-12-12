**Insert only if NOT exists**
```sql
INSERT INTO system_incident_types(id, opt_in, opt_out)
VALUES ('Cancellation', 'True', 'False')
ON CONFLICT (id) DO NOTHING;
```

**Duplicate a row in postgres**
```sql 
-- Source - https://stackoverflow.com/a/76312801
-- Posted by Petri Ryhänen
-- Retrieved 2025-12-10, License - CC BY-SA 4.0

CREATE TEMP TABLE tmp (like web_book);
INSERT INTO tmp SELECT * FROM web_book WHERE id = 3;
UPDATE tmp SET id = nextval('web_book_id_seq');
INSERT INTO web_book SELECT * from tmp;
```
**Add or Substract Days**
```sql
--ref: https://www.datacamp.com/doc/postgresql/date-arithmetic-(+)
SELECT '2023-10-15'::date + interval '5 days';
SELECT CURRENT_DATE + interval '5 days';
```
