# Spec 020: Foundation Hardening — Backend Technical Design

## EF Core Entity Constraint Refactoring

We will clean up duplicate validations by keeping constraints solely inside Fluent API configuration classes (implementing `IEntityTypeConfiguration<T>`) and removing Data Annotations (`[Required]`, `[MaxLength]`) from the domain model properties.

### 1. Refactor Candidate Entity and Configuration
* **Modify** [Candidate.cs](file:///C:/Users/felip/source/Micro/src/Micro.API/Data/Models/Candidate.cs):
  Remove `[Required]` and `[MaxLength]` attributes. Keep only clean properties.
* **Keep** [CandidateConfiguration.cs](file:///C:/Users/felip/source/Micro/src/Micro.API/Data/Configuration/CandidateConfiguration.cs) as the single source of truth for database limits.

### 2. Create Missing Configurations
To align with the project standard, we will create explicit configuration classes for `CostCenter`, `Department`, and `SalaryBand` entities.
* **New File** [CostCenterConfiguration.cs](file:///C:/Users/felip/source/Micro/src/Micro.API/Data/Configuration/CostCenterConfiguration.cs):
  ```csharp
  public class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
  {
      public void Configure(EntityTypeBuilder<CostCenter> builder)
      {
          builder.HasKey(x => x.Id);
          builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
          builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
      }
  }
  ```
* **New File** [DepartmentConfiguration.cs](file:///C:/Users/felip/source/Micro/src/Micro.API/Data/Configuration/DepartmentConfiguration.cs):
  ```csharp
  public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
  {
      public void Configure(EntityTypeBuilder<Department> builder)
      {
          builder.HasKey(x => x.Id);
          builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
          builder.Property(x => x.IsActive).HasDefaultValue(true);
      }
  }
  ```
* **New File** [SalaryBandConfiguration.cs](file:///C:/Users/felip/source/Micro/src/Micro.API/Data/Configuration/SalaryBandConfiguration.cs):
  ```csharp
  public class SalaryBandConfiguration : IEntityTypeConfiguration<SalaryBand>
  {
      public void Configure(EntityTypeBuilder<SalaryBand> builder)
      {
          builder.HasKey(x => x.Id);
          builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
          builder.Property(x => x.MinAmount).HasColumnType("decimal(18,2)");
          builder.Property(x => x.MaxAmount).HasColumnType("decimal(18,2)");
          builder.Property(x => x.Currency).IsRequired().HasMaxLength(10).HasDefaultValue("USD");
      }
  }
  ```

---

## Test Coverage Enhancements

### 1. Preset Validator Unit Tests
We will add a new test suite file [PresetValidatorsTests.cs](file:///C:/Users/felip/source/Micro/tests/Micro.Tests/Infrastructure/CustomFields/PresetValidatorsTests.cs) targeting all custom validation presets:
* **CPF / CNPJ / PIS**: Verify checksum computation, validation of incorrect checksums, length boundaries, and rejection of repeated digits (e.g. `111.111.111-11`).
* **CEP / CNH**: Verify format matching (regex verification).
* **Phone (Landline/Mobile) / Email / URL**: Verify general formats.

### 2. CustomFieldValidator Unit Tests
We will add [CustomFieldValidatorTests.cs](file:///C:/Users/felip/source/Micro/tests/Micro.Tests/Infrastructure/CustomFields/CustomFieldValidatorTests.cs):
* Test mandatory constraints (`IsRequired = true`).
* Test text length boundaries, number ranges, date ranges, and choice constraints.
* Verify preset format checks and mask validation.

### 3. API Endpoint Integration Tests
We will add integration tests targeting all API endpoint classes that currently have zero integration tests:
* [CandidateEndpointsTests.cs](file:///C:/Users/felip/source/Micro/tests/Micro.Tests/Endpoints/CandidateEndpointsTests.cs) (CRUD, route validation, database cascades)
* [RoleEndpointsTests.cs](file:///C:/Users/felip/source/Micro/tests/Micro.Tests/Endpoints/RoleEndpointsTests.cs) (Role listing, creation, permission bindings, deduplication checks)
* [UserEndpointsTests.cs](file:///C:/Users/felip/source/Micro/tests/Micro.Tests/Endpoints/UserEndpointsTests.cs) (User listing, profile retrieval, user-role associations, invitation tracking)
* [AdminEndpointsTests.cs](file:///C:/Users/felip/source/Micro/tests/Micro.Tests/Endpoints/AdminEndpointsTests.cs) (Data seeding and database reset flows)

---

## Query and Transaction Optimizations

### 1. Batch Custom Field Retrieval (N+1 Query Resolution)
To address performance bottlenecks where detail or list endpoints loop over entities and execute database queries for custom fields per-item:
* **Helper Methods** in [CustomFieldPersistence.cs](file:///C:/Users/felip/source/Micro/src/Micro.API/Infrastructure/CustomFields/CustomFieldPersistence.cs):
  - `GetBatchApplicationValuesAsync(db, applicationIds, Statuses)`: Bulk retrieves application custom field values and definition mappings (global, job-posting specific, and requisition specific) in exactly 4 database queries.
  - `GetBatchRequisitionValuesAsync(db, requisitionIds)`: Bulk retrieves requisition custom field values and definitions in exactly 2 database queries.
* **Endpoints Integration**:
  - Update `CandidateEndpoints.GetCandidateDetail`, `ApplicationEndpoints.GetAdminApplications`, and `RequisitionEndpoints.GetRequisitions` to fetch the custom fields in bulk before rendering the list or detail payload.

### 2. Consolidated SaveChanges in ApplyToJob (Atomicity & Round-trips)
* **Modify** `ApplicationEndpoints.ApplyToJob` in [ApplicationEndpoints.cs](file:///C:/Users/felip/source/Micro/src/Micro.API/Endpoints/Application/ApplicationEndpoints.cs):
  - Consolidate the two separate `await db.SaveChangesAsync()` calls into a single atomic `SaveChangesAsync()` invocation at the very end of the handler.
  - This ensures that if custom field mapping/saving fails, the entire application creation fails and rolls back, preventing partial data corruption.

### 3. Read-Only Query No-Tracking
* **Modify** read-only lists queries in `GetAdminApplications` and `GetRequisitions` to append `.AsNoTracking()`, preventing the overhead of tracking read-only instances.
