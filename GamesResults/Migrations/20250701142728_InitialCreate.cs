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
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "dbo",
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
                schema: "dbo",
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
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsersRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Actions",
                schema: "dbo",
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
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "Actions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ActionsRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bowlings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bowlings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                schema: "dbo",
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
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "Actions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "Components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Containers",
                schema: "dbo",
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
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "Containers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObjectTypes",
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "Containers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "System",
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "Containers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObjectProperties",
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "ObjectTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Objects",
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "ObjectTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Objects_Objects_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "dbo",
                        principalTable: "Objects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Objects_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Objects_Users_DeleterId",
                        column: x => x.DeleterId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Objects_Users_EditorId",
                        column: x => x.EditorId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ObjectPropertiesEditRoles",
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "ObjectProperties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ObjectPropertiesEditRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DictionaryItems",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    DictionaryTypeId = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    IsNotUsed = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionaryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DictionaryItems_DictionaryTypes_DictionaryTypeId",
                        column: x => x.DictionaryTypeId,
                        principalSchema: "dbo",
                        principalTable: "DictionaryTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DictionaryItems_Objects_Id",
                        column: x => x.Id,
                        principalSchema: "dbo",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "Actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Logs_Objects_ObjectId",
                        column: x => x.ObjectId,
                        principalSchema: "dbo",
                        principalTable: "Objects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Logs_Users_UserId1",
                        column: x => x.UserId1,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    SportType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Objects_Id",
                        column: x => x.Id,
                        principalSchema: "dbo",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Disciplines",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disciplines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disciplines_DictionaryItems_Id",
                        column: x => x.Id,
                        principalSchema: "dbo",
                        principalTable: "DictionaryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "DictionaryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Oils",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oils", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Oils_DictionaryItems_Id",
                        column: x => x.Id,
                        principalSchema: "dbo",
                        principalTable: "DictionaryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ranks",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ranks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ranks_DictionaryItems_Id",
                        column: x => x.Id,
                        principalSchema: "dbo",
                        principalTable: "DictionaryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogDetails",
                schema: "dbo",
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
                        principalSchema: "dbo",
                        principalTable: "Logs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    EventDate = table.Column<DateTime>(type: "date", nullable: false),
                    OilId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Objects_Id",
                        column: x => x.Id,
                        principalSchema: "dbo",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Events_Oils_OilId",
                        column: x => x.OilId,
                        principalSchema: "dbo",
                        principalTable: "Oils",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Players",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CityId = table.Column<long>(type: "bigint", nullable: true),
                    RankId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Cities_CityId",
                        column: x => x.CityId,
                        principalSchema: "dbo",
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Players_DictionaryItems_Id",
                        column: x => x.Id,
                        principalSchema: "dbo",
                        principalTable: "DictionaryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Players_Ranks_RankId",
                        column: x => x.RankId,
                        principalSchema: "dbo",
                        principalTable: "Ranks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EventTeamMembers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    EventId = table.Column<long>(type: "bigint", nullable: false),
                    TeamId = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventTeamMembers_Events_EventId",
                        column: x => x.EventId,
                        principalSchema: "dbo",
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTeamMembers_Objects_Id",
                        column: x => x.Id,
                        principalSchema: "dbo",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTeamMembers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "dbo",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTeamMembers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "dbo",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Participations",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    EventId = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: true),
                    TeamId = table.Column<long>(type: "bigint", nullable: true),
                    BowlingId = table.Column<long>(type: "bigint", nullable: false),
                    Result = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Game1 = table.Column<int>(type: "integer", nullable: true),
                    Game2 = table.Column<int>(type: "integer", nullable: true),
                    Game3 = table.Column<int>(type: "integer", nullable: true),
                    Game4 = table.Column<int>(type: "integer", nullable: true),
                    Game5 = table.Column<int>(type: "integer", nullable: true),
                    Game6 = table.Column<int>(type: "integer", nullable: true),
                    Summ = table.Column<int>(type: "integer", nullable: true),
                    Average = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participations_Bowlings_BowlingId",
                        column: x => x.BowlingId,
                        principalSchema: "dbo",
                        principalTable: "Bowlings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participations_Events_EventId",
                        column: x => x.EventId,
                        principalSchema: "dbo",
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participations_Objects_Id",
                        column: x => x.Id,
                        principalSchema: "dbo",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participations_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "dbo",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Participations_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "dbo",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    TeamId = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: true),
                    RankId = table.Column<long>(type: "bigint", nullable: true),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Cities_CityId",
                        column: x => x.CityId,
                        principalSchema: "dbo",
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamMembers_Objects_Id",
                        column: x => x.Id,
                        principalSchema: "dbo",
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "dbo",
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Ranks_RankId",
                        column: x => x.RankId,
                        principalSchema: "dbo",
                        principalTable: "Ranks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamMembers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "dbo",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actions_Name",
                schema: "dbo",
                table: "Actions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Actions_ObjectTypeId",
                schema: "dbo",
                table: "Actions",
                column: "ObjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Actions_ObjectTypeId_Name",
                schema: "dbo",
                table: "Actions",
                columns: new[] { "ObjectTypeId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Actions_Title",
                schema: "dbo",
                table: "Actions",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActionsRoles_RoleId",
                schema: "dbo",
                table: "ActionsRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_DistrictId",
                schema: "dbo",
                table: "Cities",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_LoadActionId",
                schema: "dbo",
                table: "Components",
                column: "LoadActionId");

            migrationBuilder.CreateIndex(
                name: "IX_Containers_Name",
                schema: "dbo",
                table: "Containers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DictionaryItems_DictionaryTypeId",
                schema: "dbo",
                table: "DictionaryItems",
                column: "DictionaryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_OilId",
                schema: "dbo",
                table: "Events",
                column: "OilId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTeamMembers_EventId",
                schema: "dbo",
                table: "EventTeamMembers",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTeamMembers_PlayerId",
                schema: "dbo",
                table: "EventTeamMembers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTeamMembers_TeamId",
                schema: "dbo",
                table: "EventTeamMembers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_LogDetails_LogId",
                schema: "dbo",
                table: "LogDetails",
                column: "LogId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_ActionId",
                schema: "dbo",
                table: "Logs",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_ObjectId",
                schema: "dbo",
                table: "Logs",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_UserId1",
                schema: "dbo",
                table: "Logs",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectProperties_ObjectTypeId_Name",
                schema: "dbo",
                table: "ObjectProperties",
                columns: new[] { "ObjectTypeId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ObjectProperties_ObjectTypeId_Title_GroupTitle_SubGroupTitle",
                schema: "dbo",
                table: "ObjectProperties",
                columns: new[] { "ObjectTypeId", "Title", "GroupTitle", "SubGroupTitle" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ObjectPropertiesEditRoles_RoleId",
                schema: "dbo",
                table: "ObjectPropertiesEditRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_AuthorId",
                schema: "dbo",
                table: "Objects",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_DeleterId",
                schema: "dbo",
                table: "Objects",
                column: "DeleterId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_EditorId",
                schema: "dbo",
                table: "Objects",
                column: "EditorId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_ParentId",
                schema: "dbo",
                table: "Objects",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Objects_TypeId",
                schema: "dbo",
                table: "Objects",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectTypes_Name",
                schema: "dbo",
                table: "ObjectTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ObjectTypes_RootContainerId",
                schema: "dbo",
                table: "ObjectTypes",
                column: "RootContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectTypes_Title",
                schema: "dbo",
                table: "ObjectTypes",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participations_BowlingId",
                schema: "dbo",
                table: "Participations",
                column: "BowlingId");

            migrationBuilder.CreateIndex(
                name: "IX_Participations_EventId",
                schema: "dbo",
                table: "Participations",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Participations_PlayerId",
                schema: "dbo",
                table: "Participations",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Participations_TeamId",
                schema: "dbo",
                table: "Participations",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_CityId",
                schema: "dbo",
                table: "Players",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_RankId",
                schema: "dbo",
                table: "Players",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                schema: "dbo",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Title",
                schema: "dbo",
                table: "Roles",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_CityId",
                schema: "dbo",
                table: "TeamMembers",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_PlayerId",
                schema: "dbo",
                table: "TeamMembers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_RankId",
                schema: "dbo",
                table: "TeamMembers",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_TeamId",
                schema: "dbo",
                table: "TeamMembers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_name",
                schema: "dbo",
                table: "Users",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_UsersRoles_UserId",
                schema: "dbo",
                table: "UsersRoles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_ObjectTypes_ObjectTypeId",
                schema: "dbo",
                table: "Actions",
                column: "ObjectTypeId",
                principalSchema: "dbo",
                principalTable: "ObjectTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bowlings_DictionaryItems_Id",
                schema: "dbo",
                table: "Bowlings",
                column: "Id",
                principalSchema: "dbo",
                principalTable: "DictionaryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_DictionaryItems_Id",
                schema: "dbo",
                table: "Cities",
                column: "Id",
                principalSchema: "dbo",
                principalTable: "DictionaryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Districts_DistrictId",
                schema: "dbo",
                table: "Cities",
                column: "DistrictId",
                principalSchema: "dbo",
                principalTable: "Districts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Components_Containers_Id",
                schema: "dbo",
                table: "Components",
                column: "Id",
                principalSchema: "dbo",
                principalTable: "Containers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Containers_Objects_Id",
                schema: "dbo",
                table: "Containers",
                column: "Id",
                principalSchema: "dbo",
                principalTable: "Objects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Objects_ObjectTypes_TypeId",
                schema: "dbo",
                table: "Objects");

            migrationBuilder.DropTable(
                name: "ActionsRoles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Disciplines",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "EventTeamMembers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LogDetails",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ObjectPropertiesEditRoles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Pages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Participations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "System",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TeamMembers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UsersRoles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Logs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ObjectProperties",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Components",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Bowlings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Events",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Players",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Teams",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Actions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Oils",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Cities",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Ranks",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Districts",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DictionaryItems",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DictionaryTypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ObjectTypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Containers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Objects",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "dbo");
        }
    }
}
