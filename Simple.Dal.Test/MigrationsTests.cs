using Crpm.Infrastructure.Core;
using Crpm.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace Crpm.Dal.UnitTest
{
    [DbContext(typeof(CRPMContext))]
    [Migration(MigrationName)]
    public class MigrationsTests : Migration, IDisposable
    {
        private bool _disposed;
        private TestWebHost _testBaseWebHost;

        // for each new test unique migration name, that not exist int table "_EFMigrationHistory"
        public const string MigrationName = "2020081220207_AddMigrations_Test";

        [Fact]
        public void MigrationsTest()
        {
            _testBaseWebHost = new TestWebHost();

            using var tempServiceScope = GeneralContext.CreateServiceScope();
            var dbContext = tempServiceScope.ServiceProvider.GetService<CRPMContext>();
            var pendingMigrations = dbContext.Database.GetPendingMigrations();
            var appliedMigrations = dbContext.Database.GetAppliedMigrations();

            if (GeneralContext.LastErrors.Count == 0 && !pendingMigrations.Any() && appliedMigrations.Any(x => x == MigrationName))
                Assert.True(true);
            else
                Assert.True(false);
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            MigrationTablesUp(migrationBuilder);
            //MigrationColumnsUp(migrationBuilder);
            MigrationTablesDown(migrationBuilder);
            //MigrationColumnDown(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }

        protected void MigrationTablesUp(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterTable(name: "Test");

            migrationBuilder.AddPrimaryKey(name: "PK_Content", table: "Test", column: "TestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Test_User_UserGuid",
                table: "Test",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserGuid"); //, TODO: test onDelete: ReferentialAction.Cascade

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TestId",
                table: "Test",
                column: "TestId");
        }

        protected void MigrationColumnsUp(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "LastUpdated", table: "Test", nullable: true);
            //migrationBuilder.AlterColumn<DateTime>(name: "LastUpdated", table: "ProductCategories", nullable: false, defaultValueSql: "now()");
            //migrationBuilder.AddColumn<Guid>(name: "TestGuid", table: "Test", nullable: false, defaultValueSql: "newsequentialid()");
            migrationBuilder.RenameColumn(name: "FirstName", table: "Test", newName: "FirstName1");
            
        }

        protected void MigrationTablesDown(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(name: "PK_Content", table: "Test");
            migrationBuilder.DropForeignKey(name: "FK_Test_User_UserGuid", table: "Test");

            migrationBuilder.RenameTable(name: "Test", newName: "ActivityTemp");
            migrationBuilder.DropTable("ActivityTemp");
        }

        protected void MigrationColumnDown(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FirstName", table: "Test");
        }

        //examples:
        protected void MigrationSqlUp(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE dbo.Test ADD CONSTRAINT CK_Data_JsonData_MustBeJson CHECK (IsJson(JsonData) = 1);");
        }
        protected void MigrationSqlDown(MigrationBuilder migrationBuilder)
        {
            // TODO: not tested, only example
            migrationBuilder.Sql("ALTER TABLE dbo.Test DROP CONSTRAINT CK_Data_JsonData_MustBeJson;");
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _testBaseWebHost?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
