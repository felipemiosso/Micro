using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Micro.API.Data;
using Micro.API.Data.Models;
using Micro.API.Infrastructure.CustomFields;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Micro.Tests.Infrastructure.CustomFields;

[Collection("TestDatabase")]
public class CustomFieldPersistenceTests : IntegrationTestBase
{
    public CustomFieldPersistenceTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetBatchRequisitionValues_ReturnsCorrectValues()
    {
        // Arrange
        Guid req1Id = Guid.NewGuid();
        Guid req2Id = Guid.NewGuid();

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var dept = await db.Departments.FirstAsync();
            var band = await db.SalaryBands.FirstAsync();
            var cc = await db.CostCenters.FirstAsync();

            // Create definitions
            var def1 = new CustomFieldDefinition
            {
                Id = Guid.NewGuid(),
                Label = "Test Req Field 1",
                TargetEntity = CustomFieldTargetEntity.Requisition,
                FieldType = CustomFieldType.ShortText,
                IsGlobal = true
            };
            var def2 = new CustomFieldDefinition
            {
                Id = Guid.NewGuid(),
                Label = "Test Req Field 2",
                TargetEntity = CustomFieldTargetEntity.Requisition,
                FieldType = CustomFieldType.Number,
                IsGlobal = true
            };
            db.CustomFieldDefinitions.AddRange(def1, def2);

            // Create values for req 1
            db.CustomFieldValues.Add(new CustomFieldValue
            {
                Id = Guid.NewGuid(),
                CustomFieldDefinitionId = def1.Id,
                EntityId = req1Id,
                TargetEntity = CustomFieldTargetEntity.Requisition,
                Value = "Value 1"
            });

            // Create values for req 2
            db.CustomFieldValues.Add(new CustomFieldValue
            {
                Id = Guid.NewGuid(),
                CustomFieldDefinitionId = def2.Id,
                EntityId = req2Id,
                TargetEntity = CustomFieldTargetEntity.Requisition,
                Value = "42"
            });

            await db.SaveChangesAsync();
        }

        // Act
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var results = await CustomFieldPersistence.GetBatchRequisitionValuesAsync(db, new[] { req1Id, req2Id });

            // Assert
            Assert.NotNull(results);
            Assert.True(results.ContainsKey(req1Id));
            Assert.True(results.ContainsKey(req2Id));

            var req1Vals = results[req1Id];
            Assert.Single(req1Vals);
            Assert.Equal("Value 1", req1Vals[0].Value);

            var req2Vals = results[req2Id];
            Assert.Single(req2Vals);
            Assert.Equal("42", req2Vals[0].Value);
        }
    }

    [Fact]
    public async Task GetBatchApplicationValues_FiltersByActiveScopeAndLinkage()
    {
        // Arrange
        Guid appId = Guid.NewGuid();
        Guid jobPostingId = Guid.NewGuid();
        Guid requisitionId = Guid.NewGuid();

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var dept = await db.Departments.FirstAsync();
            var band = await db.SalaryBands.FirstAsync();
            var cc = await db.CostCenters.FirstAsync();

            var requisition = new Requisition
            {
                Id = requisitionId,
                Title = "Req for JP",
                DepartmentId = dept.Id,
                SalaryBandId = band.Id,
                CostCenterId = cc.Id,
                OpeningsCount = 1,
                EmploymentType = EmploymentType.FullTime,
                WorkplaceType = WorkplaceType.OnSite,
                Location = "Location",
                JobDescription = "Description",
                CreatedBy = "Admin",
                HiringManagerId = Guid.NewGuid()
            };
            db.Requisitions.Add(requisition);

            var jobPosting = new JobPosting
            {
                Id = jobPostingId,
                RequisitionId = requisition.Id,
                Title = "Job for App",
                Description = "Description",
                Status = JobPostingStatus.Published
            };
            db.JobPostings.Add(jobPosting);

            var candidate = new Candidate
            {
                Id = Guid.NewGuid(),
                FullName = "Bob Tester",
                Email = $"bob-{Guid.NewGuid()}@example.com",
                Phone = "12345"
            };
            db.Candidates.Add(candidate);

            var application = new Application
            {
                Id = appId,
                CandidateId = candidate.Id,
                JobPostingId = jobPostingId,
                Status = ApplicationStatus.Interview
            };
            db.Applications.Add(application);

            // Definition 1: global applied field (active)
            var defGlobal = new CustomFieldDefinition
            {
                Id = Guid.NewGuid(),
                Label = "Global Applied",
                TargetEntity = CustomFieldTargetEntity.Application_Applied,
                FieldType = CustomFieldType.ShortText,
                IsGlobal = true
            };
            // Definition 2: selectable requisition custom field linked to the requisition (active)
            var defSelectable = new CustomFieldDefinition
            {
                Id = Guid.NewGuid(),
                Label = "Selectable Req Field",
                TargetEntity = CustomFieldTargetEntity.Application_Interview,
                FieldType = CustomFieldType.ShortText,
                IsGlobal = false
            };
            db.CustomFieldDefinitions.AddRange(defGlobal, defSelectable);

            db.RequisitionCustomFields.Add(new RequisitionCustomField
            {
                RequisitionId = requisitionId,
                CustomFieldDefinitionId = defSelectable.Id
            });

            // Set values
            db.CustomFieldValues.AddRange(
                new CustomFieldValue
                {
                    Id = Guid.NewGuid(),
                    CustomFieldDefinitionId = defGlobal.Id,
                    EntityId = appId,
                    TargetEntity = CustomFieldTargetEntity.Application_Applied,
                    Value = "Global Val"
                },
                new CustomFieldValue
                {
                    Id = Guid.NewGuid(),
                    CustomFieldDefinitionId = defSelectable.Id,
                    EntityId = appId,
                    TargetEntity = CustomFieldTargetEntity.Application_Interview,
                    Value = "Selectable Val"
                }
            );

            await db.SaveChangesAsync();
        }

        // Act
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MicroDbContext>();
            var results = await CustomFieldPersistence.GetBatchApplicationValuesAsync(db, new[] { appId });

            // Assert
            Assert.NotNull(results);
            Assert.True(results.ContainsKey(appId));
            var appVals = results[appId];

            Assert.Equal(2, appVals.Count);
            Assert.Contains(appVals, x => x.Value == "Global Val");
            Assert.Contains(appVals, x => x.Value == "Selectable Val");
        }
    }
}
