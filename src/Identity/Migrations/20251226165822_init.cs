using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "repobasecsf");

            migrationBuilder.CreateTable(
                name: "repobasecsf_AspNetRoles",
                schema: "repobasecsf",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repobasecsf_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "repobasecsf_AspNetUsers",
                schema: "repobasecsf",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CPF_CNPJ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Street = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Complement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Neighborhood = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalCustomerGatewayID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalCustomerCardGatewayID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferenceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferenceProfession = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferencePosition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferenceBehaviors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferenceExtraInformation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repobasecsf_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "repobasecsf_AspNetRoleClaims",
                schema: "repobasecsf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repobasecsf_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_repobasecsf_AspNetRoleClaims_repobasecsf_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "repobasecsf",
                        principalTable: "repobasecsf_AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "repobasecsf_AspNetUserClaims",
                schema: "repobasecsf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repobasecsf_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_repobasecsf_AspNetUserClaims_repobasecsf_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "repobasecsf",
                        principalTable: "repobasecsf_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "repobasecsf_AspNetUserLogins",
                schema: "repobasecsf",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repobasecsf_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_repobasecsf_AspNetUserLogins_repobasecsf_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "repobasecsf",
                        principalTable: "repobasecsf_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "repobasecsf_AspNetUserRoles",
                schema: "repobasecsf",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repobasecsf_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_repobasecsf_AspNetUserRoles_repobasecsf_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "repobasecsf",
                        principalTable: "repobasecsf_AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_repobasecsf_AspNetUserRoles_repobasecsf_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "repobasecsf",
                        principalTable: "repobasecsf_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "repobasecsf_AspNetUserTokens",
                schema: "repobasecsf",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repobasecsf_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_repobasecsf_AspNetUserTokens_repobasecsf_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "repobasecsf",
                        principalTable: "repobasecsf_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "repobasecsf_UserRefreshToken",
                schema: "repobasecsf",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repobasecsf_UserRefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_repobasecsf_UserRefreshToken_repobasecsf_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "repobasecsf",
                        principalTable: "repobasecsf_AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_repobasecsf_AspNetRoleClaims_RoleId",
                schema: "repobasecsf",
                table: "repobasecsf_AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "repobasecsf",
                table: "repobasecsf_AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_repobasecsf_AspNetUserClaims_UserId",
                schema: "repobasecsf",
                table: "repobasecsf_AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_repobasecsf_AspNetUserLogins_UserId",
                schema: "repobasecsf",
                table: "repobasecsf_AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_repobasecsf_AspNetUserRoles_RoleId",
                schema: "repobasecsf",
                table: "repobasecsf_AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "repobasecsf",
                table: "repobasecsf_AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "repobasecsf",
                table: "repobasecsf_AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_repobasecsf_UserRefreshToken_UserId",
                schema: "repobasecsf",
                table: "repobasecsf_UserRefreshToken",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "repobasecsf_AspNetRoleClaims",
                schema: "repobasecsf");

            migrationBuilder.DropTable(
                name: "repobasecsf_AspNetUserClaims",
                schema: "repobasecsf");

            migrationBuilder.DropTable(
                name: "repobasecsf_AspNetUserLogins",
                schema: "repobasecsf");

            migrationBuilder.DropTable(
                name: "repobasecsf_AspNetUserRoles",
                schema: "repobasecsf");

            migrationBuilder.DropTable(
                name: "repobasecsf_AspNetUserTokens",
                schema: "repobasecsf");

            migrationBuilder.DropTable(
                name: "repobasecsf_UserRefreshToken",
                schema: "repobasecsf");

            migrationBuilder.DropTable(
                name: "repobasecsf_AspNetRoles",
                schema: "repobasecsf");

            migrationBuilder.DropTable(
                name: "repobasecsf_AspNetUsers",
                schema: "repobasecsf");
        }
    }
}
