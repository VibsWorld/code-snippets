* Sample Repo https://github.com/codersgyan/postgres-queue-y

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
