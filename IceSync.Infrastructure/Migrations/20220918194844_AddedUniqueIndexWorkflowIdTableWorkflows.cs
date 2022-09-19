using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IceSync.Infrastructure.Migrations
{
    public partial class AddedUniqueIndexWorkflowIdTableWorkflows : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Workflows_WorkflowId",
                table: "Workflows",
                column: "WorkflowId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workflows_WorkflowId",
                table: "Workflows");
        }
    }
}
