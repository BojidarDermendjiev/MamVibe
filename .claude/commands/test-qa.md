Run the full MomVibe QA test suite across backend and frontend. Execute these steps in order and report a clear pass/fail summary:

## Steps

1. **Backend — Unit Tests**
   ```
   dotnet test backend/tests/MomVibe.UnitTests --verbosity normal
   ```

2. **Backend — Integration Tests** (requires PostgreSQL running)
   ```
   dotnet test backend/tests/MomVibe.IntegrationTests --verbosity normal
   ```

3. **Frontend — Lint**
   ```
   cd frontend && npm run lint
   ```

4. **Frontend — Build check**
   ```
   cd frontend && npm run build
   ```

## Rules

- Run all 4 steps regardless of earlier failures so the user gets a full picture.
- At the end, print a summary table:
  | Step | Status |
  |------|--------|
  | Unit Tests | PASS / FAIL |
  | Integration Tests | PASS / FAIL |
  | Frontend Lint | PASS / FAIL |
  | Frontend Build | PASS / FAIL |
- If **any** step fails, end with: "QA FAILED — do not start the project until all issues are resolved."
- If **all** steps pass, end with: "QA PASSED — safe to start the project."
