using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaisonAeternum.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiConversationRelatedModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AIConversations_LearnerId",
                table: "AIConversations");

            migrationBuilder.AddColumn<int>(
                name: "RelatedModuleId",
                table: "AIConversations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_LearnerId_LastMessageAt",
                table: "AIConversations",
                columns: new[] { "LearnerId", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_RelatedFormationId",
                table: "AIConversations",
                column: "RelatedFormationId");

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_RelatedModuleId",
                table: "AIConversations",
                column: "RelatedModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_RelatedQuizAttemptId",
                table: "AIConversations",
                column: "RelatedQuizAttemptId");

            migrationBuilder.AddForeignKey(
                name: "FK_AIConversations_Formations_RelatedFormationId",
                table: "AIConversations",
                column: "RelatedFormationId",
                principalTable: "Formations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AIConversations_Modules_RelatedModuleId",
                table: "AIConversations",
                column: "RelatedModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AIConversations_QuizAttempts_RelatedQuizAttemptId",
                table: "AIConversations",
                column: "RelatedQuizAttemptId",
                principalTable: "QuizAttempts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIConversations_Formations_RelatedFormationId",
                table: "AIConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_AIConversations_Modules_RelatedModuleId",
                table: "AIConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_AIConversations_QuizAttempts_RelatedQuizAttemptId",
                table: "AIConversations");

            migrationBuilder.DropIndex(
                name: "IX_AIConversations_LearnerId_LastMessageAt",
                table: "AIConversations");

            migrationBuilder.DropIndex(
                name: "IX_AIConversations_RelatedFormationId",
                table: "AIConversations");

            migrationBuilder.DropIndex(
                name: "IX_AIConversations_RelatedModuleId",
                table: "AIConversations");

            migrationBuilder.DropIndex(
                name: "IX_AIConversations_RelatedQuizAttemptId",
                table: "AIConversations");

            migrationBuilder.DropColumn(
                name: "RelatedModuleId",
                table: "AIConversations");

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_LearnerId",
                table: "AIConversations",
                column: "LearnerId");
        }
    }
}
