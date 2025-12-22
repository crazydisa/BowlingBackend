using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GamesResults.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Bowling");

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    IsPosition = table.Column<bool>(type: "boolean", nullable: false),
                    IsDepartment = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SapsanId = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    dt_change = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    login = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    patronymic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    inn = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    phone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    e_mail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    remark = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    can_be_gip = table.Column<int>(type: "integer", nullable: true),
                    id_organization = table.Column<long>(type: "bigint", nullable: true),
                    emp_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    id_room = table.Column<long>(type: "bigint", nullable: true),
                    birthday = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    pm_ObjectId = table.Column<long>(type: "bigint", nullable: true),
                    placement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sid = table.Column<byte[]>(type: "bytea", nullable: true),
                    id_int = table.Column<int>(type: "integer", nullable: true),
                    id_position = table.Column<long>(type: "bigint", nullable: true),
                    id_subdivision = table.Column<long>(type: "bigint", nullable: true),
                    id_otdel = table.Column<long>(type: "bigint", nullable: true),
                    dt_start = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    dt_finish = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    greid = table.Column<int>(type: "integer", nullable: true),
                    host = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IpAdress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    data_source = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true, defaultValueSql: "'B'::character varying"),
                    is_chief = table.Column<bool>(type: "boolean", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: true, defaultValueSql: "10"),
                    id_func_subdivision = table.Column<long>(type: "bigint", nullable: true),
                    id_func_otdel = table.Column<long>(type: "bigint", nullable: true),
                    short_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    id_external = table.Column<long>(type: "bigint", nullable: true),
                    id_type = table.Column<int>(type: "integer", nullable: false),
                    external_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsersRoles",
                schema: "Bowling",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersRoles", x => new { x.RoleId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UsersRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Bowling",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsersRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Bowling",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Actions",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ObjectTypeId = table.Column<int>(type: "integer", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsChange = table.Column<bool>(type: "boolean", nullable: false),
                    IsLogDetails = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActionsRoles",
                schema: "Bowling",
                columns: table => new
                {
                    ActionId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionsRoles", x => new { x.ActionId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_ActionsRoles_Actions_ActionId",
                        column: x => x.ActionId,
                        principalSchema: "Bowling",
                        principalTable: "Actions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ActionsRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Bowling",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bowlings",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Website = table.Column<string>(type: "text", nullable: true),
                    CityId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bowlings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    DistrictId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Components",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    LoadActionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Components_Actions_LoadActionId",
                        column: x => x.LoadActionId,
                        principalSchema: "Bowling",
                        principalTable: "Actions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pages_Components_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "Components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Containers",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Containers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DictionaryTypes",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionaryTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DictionaryTypes_Containers_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "Containers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObjectTypes",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    NameSpace = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DisplayExpr = table.Column<string>(type: "text", nullable: true),
                    RootContainerId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObjectTypes_Containers_RootContainerId",
                        column: x => x.RootContainerId,
                        principalSchema: "Bowling",
                        principalTable: "Containers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "System",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_System", x => x.Id);
                    table.ForeignKey(
                        name: "FK_System_Containers_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "Containers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObjectProperties",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Title = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    GroupTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SubGroupTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SortIndex = table.Column<int>(type: "integer", nullable: true),
                    ObjectTypeId = table.Column<int>(type: "integer", nullable: true),
                    TypeName = table.Column<string>(type: "text", nullable: true),
                    NameSpace = table.Column<string>(type: "text", nullable: true),
                    DictionaryTypeName = table.Column<string>(type: "text", nullable: true),
                    DataFormat = table.Column<string>(type: "text", nullable: true),
                    DisplayExpr = table.Column<string>(type: "text", nullable: true),
                    RelatedField = table.Column<string>(type: "text", nullable: true),
                    RelatedType = table.Column<string>(type: "text", nullable: true),
                    IsIdentifier = table.Column<bool>(type: "boolean", nullable: false),
                    IsNumeric = table.Column<bool>(type: "boolean", nullable: false),
                    IsBoolean = table.Column<bool>(type: "boolean", nullable: false),
                    IsString = table.Column<bool>(type: "boolean", nullable: false),
                    IsMultiline = table.Column<bool>(type: "boolean", nullable: false),
                    IsGuid = table.Column<bool>(type: "boolean", nullable: false),
                    IsDate = table.Column<bool>(type: "boolean", nullable: false),
                    IsObject = table.Column<bool>(type: "boolean", nullable: false),
                    IsInclude = table.Column<bool>(type: "boolean", nullable: false),
                    IsArray = table.Column<bool>(type: "boolean", nullable: false),
                    IsNullable = table.Column<bool>(type: "boolean", nullable: false),
                    IsHiddenByDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsHiddenInLogDetail = table.Column<bool>(type: "boolean", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "boolean", nullable: false),
                    IsNotAvailable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObjectProperties_ObjectTypes_ObjectTypeId",
                        column: x => x.ObjectTypeId,
                        principalSchema: "Bowling",
                        principalTable: "ObjectTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Objects",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TypeId = table.Column<int>(type: "integer", nullable: true),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "varchar(5000)", maxLength: 5000, nullable: true),
                    SortIndex = table.Column<int>(type: "integer", nullable: true),
                    IconName = table.Column<string>(type: "text", nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AuthorId = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EditorId = table.Column<long>(type: "bigint", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeleterId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Objects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Objects_ObjectTypes_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "Bowling",
                        principalTable: "ObjectTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Objects_Objects_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "Bowling",
                        principalTable: "Objects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Objects_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalSchema: "Bowling",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Objects_Users_DeleterId",
                        column: x => x.DeleterId,
                        principalSchema: "Bowling",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Objects_Users_EditorId",
                        column: x => x.EditorId,
                        principalSchema: "Bowling",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ObjectPropertiesEditRoles",
                schema: "Bowling",
                columns: table => new
                {
                    PropertyId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectPropertiesEditRoles", x => new { x.PropertyId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_ObjectPropertiesEditRoles_ObjectProperties_PropertyId",
                        column: x => x.PropertyId,
                        principalSchema: "Bowling",
                        principalTable: "ObjectProperties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ObjectPropertiesEditRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Bowling",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DictionaryItems",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    DictionaryTypeId = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    IsNotUsed = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionaryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DictionaryItems_DictionaryTypes_DictionaryTypeId",
                        column: x => x.DictionaryTypeId,
                        principalSchema: "Bowling",
                        principalTable: "DictionaryTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DictionaryItems_Objects_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UserId1 = table.Column<long>(type: "bigint", nullable: true),
                    ObjectId = table.Column<long>(type: "bigint", nullable: true),
                    LogTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LogLevel = table.Column<int>(type: "integer", nullable: false),
                    LogText = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Logs_Actions_ActionId",
                        column: x => x.ActionId,
                        principalSchema: "Bowling",
                        principalTable: "Actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Logs_Objects_ObjectId",
                        column: x => x.ObjectId,
                        principalSchema: "Bowling",
                        principalTable: "Objects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Logs_Users_UserId1",
                        column: x => x.UserId1,
                        principalSchema: "Bowling",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_DictionaryItems_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "DictionaryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Oils",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Pattern = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oils", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Oils_DictionaryItems_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "DictionaryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogDetails",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogDetails_Logs_LogId",
                        column: x => x.LogId,
                        principalSchema: "Bowling",
                        principalTable: "Logs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "Unknown"),
                    CityId = table.Column<long>(type: "bigint", nullable: true),
                    DistrictId = table.Column<long>(type: "bigint", nullable: true),
                    PlayerRatingId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_City",
                        column: x => x.CityId,
                        principalSchema: "Bowling",
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Players_DictionaryItems_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "DictionaryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Players_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalSchema: "Bowling",
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Tournaments",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TournamentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Unknown"),
                    Format = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Unknown"),
                    ScoringSystem = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Scratch"),
                    StartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EndDate = table.Column<DateTime>(type: "date", nullable: true),
                    MaxTeamSize = table.Column<int>(type: "integer", nullable: true),
                    MinTeamSize = table.Column<int>(type: "integer", nullable: true),
                    RatingsUpdated = table.Column<bool>(type: "boolean", nullable: false),
                    RatingsUpdatedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    BowlingId = table.Column<long>(type: "bigint", nullable: true),
                    OilId = table.Column<long>(type: "bigint", nullable: true),
                    CityId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tournaments_Bowlings_BowlingId",
                        column: x => x.BowlingId,
                        principalSchema: "Bowling",
                        principalTable: "Bowlings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tournaments_Cities_CityId",
                        column: x => x.CityId,
                        principalSchema: "Bowling",
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tournaments_Objects_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tournaments_Oils_OilId",
                        column: x => x.OilId,
                        principalSchema: "Bowling",
                        principalTable: "Oils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PlayerRatings",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false, defaultValue: 1500),
                    PeakRating = table.Column<int>(type: "integer", nullable: false, defaultValue: 1500),
                    TournamentCount = table.Column<int>(type: "integer", nullable: false),
                    AveragePlace = table.Column<double>(type: "numeric(5,2)", nullable: false),
                    Top3Percentage = table.Column<double>(type: "double precision", nullable: false),
                    Top10Percentage = table.Column<double>(type: "double precision", nullable: false),
                    TotalGames = table.Column<int>(type: "integer", nullable: false),
                    TotalPins = table.Column<int>(type: "integer", nullable: false),
                    AverageScore = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerRatings_Objects_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerRatings_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "Bowling",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TournamentId = table.Column<long>(type: "bigint", nullable: false),
                    GenderTeam = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Objects_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Teams_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalSchema: "Bowling",
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentDocuments",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileData = table.Column<byte[]>(type: "bytea", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FileHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Md5Hash = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    TournamentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentDocuments_Objects_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentDocuments_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalSchema: "Bowling",
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentResults",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    TournamentId = table.Column<long>(type: "bigint", nullable: false),
                    Place = table.Column<int>(type: "integer", nullable: false),
                    TotalScore = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    AverageScore = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false),
                    GameScoresJson = table.Column<string>(type: "text", nullable: false),
                    ResultDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Game1 = table.Column<int>(type: "integer", nullable: false),
                    Game2 = table.Column<int>(type: "integer", nullable: false),
                    Game3 = table.Column<int>(type: "integer", nullable: false),
                    Game4 = table.Column<int>(type: "integer", nullable: false),
                    Game5 = table.Column<int>(type: "integer", nullable: false),
                    Game6 = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentResults_Objects_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentResults_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalSchema: "Bowling",
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RatingHistories",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PlayerRatingId = table.Column<long>(type: "bigint", nullable: false),
                    TournamentId = table.Column<long>(type: "bigint", nullable: false),
                    OldRating = table.Column<int>(type: "integer", nullable: false),
                    NewRating = table.Column<int>(type: "integer", nullable: false),
                    RatingChange = table.Column<int>(type: "integer", nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ChangeReason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RatingHistories_Objects_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RatingHistories_PlayerRatings_PlayerRatingId",
                        column: x => x.PlayerRatingId,
                        principalSchema: "Bowling",
                        principalTable: "PlayerRatings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RatingHistories_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalSchema: "Bowling",
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    TeamId = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Member"),
                    IsCaptain = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    OrderNumber = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AverageInTeam = table.Column<decimal>(type: "numeric(6,2)", nullable: false, defaultValue: 0m),
                    GamesPlayedInTeam = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    JoinedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Objects_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "Bowling",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "Bowling",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndividualResults",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    HighGame = table.Column<int>(type: "integer", nullable: false),
                    LowGame = table.Column<int>(type: "integer", nullable: false),
                    StrikeCount = table.Column<int>(type: "integer", nullable: false),
                    SpareCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndividualResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndividualResults_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "Bowling",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndividualResults_TournamentResults_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "TournamentResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamResults",
                schema: "Bowling",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    TeamId = table.Column<long>(type: "bigint", nullable: false),
                    MemberScoresJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamResults_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "Bowling",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamResults_TournamentResults_Id",
                        column: x => x.Id,
                        principalSchema: "Bowling",
                        principalTable: "TournamentResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actions_Name",
                schema: "Bowling",
                table: "Actions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Actions_ObjectTypeId",
                schema: "Bowling",
                table: "Actions",
                column: "ObjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Actions_ObjectTypeId_Name",
                schema: "Bowling",
                table: "Actions",
                columns: new[] { "ObjectTypeId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Actions_Title",
                schema: "Bowling",
                table: "Actions",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActionsRoles_RoleId",
                schema: "Bowling",
                table: "ActionsRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Bowlings_CityId",
                schema: "Bowling",
                table: "Bowlings",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_DistrictId",
                schema: "Bowling",
                table: "Cities",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_LoadActionId",
                schema: "Bowling",
                table: "Components",
                column: "LoadActionId");

            migrationBuilder.CreateIndex(
                name: "IX_Containers_Name",
                schema: "Bowling",
                table: "Containers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryItems_DictionaryTypeId",
                schema: "Bowling",
                table: "DictionaryItems",
                column: "DictionaryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualResults_PlayerId",
                schema: "Bowling",
                table: "IndividualResults",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_LogDetails_LogId",
                schema: "Bowling",
                table: "LogDetails",
                column: "LogId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_ActionId",
                schema: "Bowling",
                table: "Logs",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_ObjectId",
                schema: "Bowling",
                table: "Logs",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_UserId1",
                schema: "Bowling",
                table: "Logs",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectProperties_ObjectTypeId_Name",
                schema: "Bowling",
                table: "ObjectProperties",
                columns: new[] { "ObjectTypeId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ObjectProperties_ObjectTypeId_Title_GroupTitle_SubGroupTitle",
                schema: "Bowling",
                table: "ObjectProperties",
                columns: new[] { "ObjectTypeId", "Title", "GroupTitle", "SubGroupTitle" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ObjectPropertiesEditRoles_RoleId",
                schema: "Bowling",
                table: "ObjectPropertiesEditRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_AuthorId",
                schema: "Bowling",
                table: "Objects",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_DeleterId",
                schema: "Bowling",
                table: "Objects",
                column: "DeleterId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_EditorId",
                schema: "Bowling",
                table: "Objects",
                column: "EditorId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_ParentId",
                schema: "Bowling",
                table: "Objects",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_TypeId",
                schema: "Bowling",
                table: "Objects",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectTypes_Name",
                schema: "Bowling",
                table: "ObjectTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ObjectTypes_RootContainerId",
                schema: "Bowling",
                table: "ObjectTypes",
                column: "RootContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectTypes_Title",
                schema: "Bowling",
                table: "ObjectTypes",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRatings_PlayerId",
                schema: "Bowling",
                table: "PlayerRatings",
                column: "PlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRatings_Rating",
                schema: "Bowling",
                table: "PlayerRatings",
                column: "Rating",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Players_CityId",
                schema: "Bowling",
                table: "Players",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_DistrictId",
                schema: "Bowling",
                table: "Players",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Email",
                schema: "Bowling",
                table: "Players",
                column: "Email",
                unique: true,
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Players_FullName",
                schema: "Bowling",
                table: "Players",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Gender_BirthDate",
                schema: "Bowling",
                table: "Players",
                columns: new[] { "Gender", "BirthDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RatingHistories_Player_Date",
                schema: "Bowling",
                table: "RatingHistories",
                columns: new[] { "PlayerRatingId", "ChangeDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RatingHistories_TournamentId",
                schema: "Bowling",
                table: "RatingHistories",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                schema: "Bowling",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Title",
                schema: "Bowling",
                table: "Roles",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_PlayerId",
                schema: "Bowling",
                table: "TeamMembers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_Team_Captain",
                schema: "Bowling",
                table: "TeamMembers",
                columns: new[] { "TeamId", "IsCaptain" },
                filter: "\"IsCaptain\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_Team_Order",
                schema: "Bowling",
                table: "TeamMembers",
                columns: new[] { "TeamId", "OrderNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_Team_Player",
                schema: "Bowling",
                table: "TeamMembers",
                columns: new[] { "TeamId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamResults_TeamId",
                schema: "Bowling",
                table: "TeamResults",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Name",
                schema: "Bowling",
                table: "Teams",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Tournament_Name",
                schema: "Bowling",
                table: "Teams",
                columns: new[] { "TournamentId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_TournamentId",
                schema: "Bowling",
                table: "Teams",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentDocuments_TournamentId",
                schema: "Bowling",
                table: "TournamentDocuments",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentResults_TournamentId",
                schema: "Bowling",
                table: "TournamentResults",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_BowlingId",
                schema: "Bowling",
                table: "Tournaments",
                column: "BowlingId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_CityId",
                schema: "Bowling",
                table: "Tournaments",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_OilId",
                schema: "Bowling",
                table: "Tournaments",
                column: "OilId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_StartDate",
                schema: "Bowling",
                table: "Tournaments",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_Type_Date",
                schema: "Bowling",
                table: "Tournaments",
                columns: new[] { "TournamentType", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_name",
                schema: "Bowling",
                table: "Users",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_UsersRoles_UserId",
                schema: "Bowling",
                table: "UsersRoles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_ObjectTypes_ObjectTypeId",
                schema: "Bowling",
                table: "Actions",
                column: "ObjectTypeId",
                principalSchema: "Bowling",
                principalTable: "ObjectTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bowlings_Cities_CityId",
                schema: "Bowling",
                table: "Bowlings",
                column: "CityId",
                principalSchema: "Bowling",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bowlings_Objects_Id",
                schema: "Bowling",
                table: "Bowlings",
                column: "Id",
                principalSchema: "Bowling",
                principalTable: "Objects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_DictionaryItems_Id",
                schema: "Bowling",
                table: "Cities",
                column: "Id",
                principalSchema: "Bowling",
                principalTable: "DictionaryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Districts_DistrictId",
                schema: "Bowling",
                table: "Cities",
                column: "DistrictId",
                principalSchema: "Bowling",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Components_Containers_Id",
                schema: "Bowling",
                table: "Components",
                column: "Id",
                principalSchema: "Bowling",
                principalTable: "Containers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Containers_Objects_Id",
                schema: "Bowling",
                table: "Containers",
                column: "Id",
                principalSchema: "Bowling",
                principalTable: "Objects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Objects_ObjectTypes_TypeId",
                schema: "Bowling",
                table: "Objects");

            migrationBuilder.DropTable(
                name: "ActionsRoles",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "IndividualResults",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "LogDetails",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "ObjectPropertiesEditRoles",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Pages",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "RatingHistories",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "System",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "TeamMembers",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "TeamResults",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "TournamentDocuments",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "UsersRoles",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Logs",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "ObjectProperties",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Components",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "PlayerRatings",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Teams",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "TournamentResults",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Actions",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Players",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Tournaments",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Bowlings",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Oils",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Cities",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Districts",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "DictionaryItems",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "DictionaryTypes",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "ObjectTypes",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Containers",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Objects",
                schema: "Bowling");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "Bowling");
        }
    }
}
