* Sample Repo https://github.com/codersgyan/postgres-queue-y

### If your goal is to ensure two concurrent asynchronous sessions in PostgreSQL select two different rows (for instance, for processing/updating jobs, so "no two sessions pick the same row")—you need a strategy for mutual exclusion between transactions.

* Code Snippet
```sql

 WITH next_job AS ( --CTE used
      SELECT id 
      FROM jobs
      WHERE status = 'pending'
      ORDER BY created_at
      FOR UPDATE SKIP LOCKED --Skip locked rows and find the latest unlocked row
      LIMIT 1 --Remember to use LIMIT 1 for best results
    )
    UPDATE jobs
    SET status = 'processing',
        attempts = attempts + 1
    WHERE id = (SELECT id FROM next_job) --Process the locked row
    RETURNING *;
```
```sql
BEGIN;

SELECT *
FROM jobs
WHERE status = 'pending'
ORDER BY id
FOR UPDATE SKIP LOCKED
LIMIT 1;
```
How it works:

`FOR UPDATE` locks the selected row for the current transaction.
`SKIP LOCKED` skips rows that have already been locked by other transactions.
Multiple concurrent sessions can run this: each gets a different (still-unlocked) row.

Typical Workflow
Each worker/session does:

1. `BEGIN;`
2. SELECT ... FOR UPDATE SKIP LOCKED LIMIT 1; (Gets a row—if available)
3. Process the row.
4. `UPDATE` or `DELETE` to mark as done.
5. `COMMIT;` (Releases the lock.)

```sql
-- Session 1
BEGIN;
SELECT id FROM jobs WHERE status = 'pending'
FOR UPDATE SKIP LOCKED
LIMIT 1;

-- Session 2 (at the same time)
BEGIN;
SELECT id FROM jobs WHERE status = 'pending'
FOR UPDATE SKIP LOCKED
LIMIT 1;

-- Each session will get a different job row.
```
