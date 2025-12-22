using GamesResults.Models.Bowling;
using GamesResults.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
//using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System.Collections;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Reflection;

//using PirAppBpLib.Models;

namespace GamesResults
{
    public class AppDbContext : DbContext
    {
        const string dbSchema = "Bowling";

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
        
        public DbSet<Models.User> Users { get; set; } = null!;
        public DbSet<Models.Role> Roles { get; set; } = null!;
        public DbSet<Models.Action> Actions { get; set; } = null!;
        public DbSet<Models.Object> Objects { get; set; } = null!;
        public DbSet<Models.ObjectType> ObjectTypes { get; set; } = null!;
        public DbSet<Models.ObjectProperty> ObjectProperties { get; set; } = null!;
        public DbSet<Models.Container> Containers { get; set; } = null!;
        public DbSet<Models.System> System { get; set; } = null!;
        public DbSet<Models.Component> Components { get; set; } = null!;
        public DbSet<Models.Page> Pages { get; set; } = null!;
        public DbSet<Models.DictionaryType> DictionaryTypes { get; set; } = null!;
        public DbSet<Models.DictionaryItem> DictionaryItems { get; set; } = null!;
        public DbSet<Models.Log> Logs { get; set; } = null!;
        public DbSet<Models.LogDetail> LogDetails { get; set; } = null!;

        //bowling class
        // DbSet'ы для каждой сущности
        public DbSet<Bowling> BowlingCenters { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<TournamentDocument> TournamentDocuments { get; set; }
        public DbSet<Oil> Oils { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerRating> PlayerRatings { get; set; }
        public DbSet<RatingHistory> RatingHistories { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }

        // DbSet'ы для наследования (базовый и производные)
        public DbSet<BaseTournamentResult> TournamentResults { get; set; }
        public DbSet<IndividualResult> IndividualResults { get; set; }
        public DbSet<TeamResult> TeamResults { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Object>().UseTptMappingStrategy();
            //modelBuilder.HasSequence<long>("TestSequence", schema: "dbo").StartsAt(1).IncrementsBy(1);

            modelBuilder.Entity<Models.User>(entity =>
            {

                entity.ToTable("Users", dbSchema);

                entity.HasIndex(c => c.Name);

                entity.Property(p => p.Name)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(p => p.Title)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(p => p.EMail)
                    .HasMaxLength(255);

                entity.HasMany(o => o.Roles)
                    .WithMany(p => p.Users)
                    .UsingEntity<Dictionary<string, object>>(
                        "UsersRoles",
                        j => j
                            .HasOne<Models.Role>()
                            .WithMany()
                            .HasForeignKey("RoleId")
                            .OnDelete(DeleteBehavior.Cascade),
                        j => j
                            .HasOne<Models.User>()
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.ClientCascade));
                entity.Property(e => e.Birthday)
               .HasColumnType("timestamp without time zone")
               .HasColumnName("birthday");
                entity.Property(e => e.CanBeGip).HasColumnName("can_be_gip");
                entity.Property(e => e.DataSource)
                    .HasMaxLength(1)
                    .HasDefaultValueSql("'B'::character varying")
                    .HasColumnName("data_source");
                entity.Property(e => e.DtChange)
                    .HasDefaultValueSql("now()")
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("dt_change");
                entity.Property(e => e.DtFinish)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("dt_finish");
                entity.Property(e => e.DtStart)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("dt_start");
                entity.Property(e => e.EMail)
                    .HasMaxLength(100)
                    .HasColumnName("e_mail");
                entity.Property(e => e.EmpNumber)
                    .HasMaxLength(50)
                    .HasColumnName("emp_number");
                entity.Property(e => e.ExternalKey)
                    .HasMaxLength(128)
                    .HasColumnName("external_key");
                entity.Property(e => e.Greid).HasColumnName("greid");
                entity.Property(e => e.Host)
                    .HasMaxLength(50)
                    .HasColumnName("host");
                entity.Property(e => e.IdExternal).HasColumnName("id_external");
                entity.Property(e => e.IdFuncOtdel).HasColumnName("id_func_otdel");
                entity.Property(e => e.IdFuncSubdivision).HasColumnName("id_func_subdivision");
                entity.Property(e => e.IdInt).HasColumnName("id_int");
                entity.Property(e => e.IdOrganization).HasColumnName("id_organization");
                entity.Property(e => e.IdOtdel).HasColumnName("id_otdel");
                entity.Property(e => e.IdPosition).HasColumnName("id_position");
                entity.Property(e => e.IdRoom).HasColumnName("id_room");
                entity.Property(e => e.IdSubdivision).HasColumnName("id_subdivision");
                entity.Property(e => e.IdType).HasColumnName("id_type");
                entity.Property(e => e.Inn)
                    .HasMaxLength(12)
                    .HasColumnName("inn");
                entity.Property(e => e.IpAdress).HasMaxLength(50);
                entity.Property(e => e.IsChief).HasColumnName("is_chief");
                entity.Property(e => e.Login)
                    .HasMaxLength(100)
                    .HasColumnName("login");
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");
                entity.Property(e => e.Patronymic)
                    .HasMaxLength(100)
                    .HasColumnName("patronymic");
                entity.Property(e => e.Phone)
                    .HasMaxLength(100)
                    .HasColumnName("phone");
                entity.Property(e => e.Placement)
                    .HasMaxLength(100)
                    .HasColumnName("placement");
                entity.Property(e => e.PmObjectId).HasColumnName("pm_ObjectId");
                entity.Property(e => e.Remark)
                    .HasMaxLength(255)
                    .HasColumnName("remark");
                entity.Property(e => e.ShortName)
                    .HasMaxLength(100)
                    .HasColumnName("short_name");
                entity.Property(e => e.Sid).HasColumnName("sid");
                entity.Property(e => e.Status)
                    .HasDefaultValueSql("10")
                    .HasColumnName("status");
                entity.Property(e => e.Surname)
                    .HasMaxLength(100)
                    .HasColumnName("surname");

                
            });

            modelBuilder.Entity<Models.Role>(o =>
            {
                o.ToTable("Roles", dbSchema);

                o.HasIndex(c => c.Name)
                    .IsUnique();

                o.HasIndex(c => c.Title)
                    .IsUnique();

                o.Property(p => p.Name)
                    .HasMaxLength(255)
                    .IsRequired();

                o.Property(p => p.Title)
                    .HasMaxLength(255)
                    .IsRequired();

                o.HasMany(o => o.Actions)
                    .WithMany(p => p.Roles);

                o.HasMany(o => o.Users)
                    .WithMany(p => p.Roles);
            });

            modelBuilder.Entity<Models.Action>(o =>
            {
                o.ToTable("Actions", dbSchema);

                o.HasIndex(c => c.Name)
                    .IsUnique();

                o.HasIndex(c => c.Title)
                    .IsUnique();

                o.HasIndex(c => c.ObjectTypeId);

                o.HasIndex(c => new { c.ObjectTypeId, c.Name });

                o.Property(p => p.Name)
                    .HasMaxLength(255)
                    .IsRequired();

                o.Property(p => p.Title)
                    .HasMaxLength(255)
                    .IsRequired();

                o.HasOne(o => o.ObjectType)
                    .WithMany()
                    .HasForeignKey(o => o.ObjectTypeId)
                    .OnDelete(DeleteBehavior.NoAction);

                o.HasMany(o => o.Roles)
                    .WithMany(p => p.Actions)
                    .UsingEntity<Dictionary<string, object>>(
                        "ActionsRoles",
                        j => j
                            .HasOne<Models.Role>()
                            .WithMany()
                            .HasForeignKey("RoleId")
                            .OnDelete(DeleteBehavior.Cascade),
                        j => j
                            .HasOne<Models.Action>()
                            .WithMany()
                            .HasForeignKey("ActionId")
                            .OnDelete(DeleteBehavior.ClientCascade));
            });

            modelBuilder.Entity<Models.Object>(o =>
            {
                o.ToTable("Objects", dbSchema);

                o.HasKey(c => c.Id);
               // o.Property(e => e.Id).UseIdentityColumn().HasColumnName("id");
                //o.Property(e => e.Id).ValueGeneratedOnAdd();
                //.HasDefaultValueSql("(newid())")
                //.HasColumnName("id");
                //o.Property(c=> c.Id).ValueGeneratedNever();
                //        o.Property(c => c.Id).ValueGeneratedOnAddOrUpdate()
                //.Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Save);
                //o.Property(e => e.Id).ValueGeneratedOnAdd().HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn).HasDefaultValue(0);

                //o.HasIndex(c => c.TypeId, "IX_Objects_TypeId");

                o.HasIndex(c => c.ParentId);

                //o.HasIndex(c => new { c.TypeId, c.DeletedAt });

                //o.HasIndex(c => new { c.ParentId, c.DeletedAt });

                //o.HasIndex(c => new { c.Title, c.TypeId, c.DeletedAt })
                //    .IsUnique();

                o.Property(p => p.Title)
                    .HasMaxLength(1000)
                    .IsRequired();

                o.Property(p => p.Description)
                    .HasMaxLength(5000).HasColumnType("varchar(5000)");

                o.HasOne(o => o.Type)
                    .WithMany()
                    .HasForeignKey(o => o.TypeId)
                    .OnDelete(DeleteBehavior.NoAction);

                o.HasOne(o => o.Parent)
                    .WithMany()
                    .HasForeignKey(o => o.ParentId)
                    .OnDelete(DeleteBehavior.NoAction);

                o.HasOne(o => o.Author)
                    .WithMany()
                    .HasForeignKey(o => o.AuthorId)
                    .OnDelete(DeleteBehavior.NoAction);

                o.HasOne(o => o.Editor)
                    .WithMany()
                    .HasForeignKey(o => o.EditorId)
                    .OnDelete(DeleteBehavior.NoAction);

                o.HasOne(o => o.Deleter)
                    .WithMany()
                    .HasForeignKey(o => o.DeleterId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Models.ObjectType>(o =>
            {
                o.ToTable("ObjectTypes", dbSchema);

                o.HasIndex(c => c.Name)
                    .IsUnique();

                o.HasIndex(c => c.Title)
                    .IsUnique();

                o.Property(p => p.Name)
                    .HasMaxLength(1000)
                    .IsRequired();

                o.Property(p => p.Title)
                    .HasMaxLength(1000)
                    .IsRequired();

                o.HasOne(o => o.RootContainer)
                    .WithMany()
                    .HasForeignKey(o => o.RootContainerId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Models.ObjectProperty>(o =>
            {
                o.ToTable("ObjectProperties", dbSchema);

                o.HasIndex(c => new { c.ObjectTypeId, c.Name })
                    .IsUnique();

                o.HasIndex(c => new { c.ObjectTypeId, c.Title, c.GroupTitle, c.SubGroupTitle })
                    .IsUnique();

                o.Property(p => p.Name)
                    .HasMaxLength(1000)
                    .IsRequired();

                o.Property(p => p.Title)
                    .HasMaxLength(1000)
                    .IsRequired();

                o.Property(p => p.GroupTitle)
                    .HasMaxLength(100);

                o.Property(p => p.SubGroupTitle)
                    .HasMaxLength(100);

                o.HasMany(p => p.EditRoles)
                    .WithMany(p => p.EditProperties)
                    .UsingEntity<Dictionary<string, object>>(
                        "ObjectPropertiesEditRoles",
                        j => j
                            .HasOne<Models.Role>()
                            .WithMany()
                            .HasForeignKey("RoleId")
                            .OnDelete(DeleteBehavior.Cascade),
                        j => j
                            .HasOne<Models.ObjectProperty>()
                            .WithMany()
                            .HasForeignKey("PropertyId")
                            .OnDelete(DeleteBehavior.ClientCascade));
            });

            modelBuilder.Entity<Models.Container>(o =>
            {
                o.HasIndex(c => c.Name)
                    .IsUnique();
                o.Property(p => p.Name).HasMaxLength(1000);
               

                o.ToTable("Containers", dbSchema);
            });

            modelBuilder.Entity<Models.System>(o =>
            {
                o.ToTable("System", dbSchema);
            });

            modelBuilder.Entity<Models.Component>(o =>
            {
                o.ToTable("Components", dbSchema);
            });

            modelBuilder.Entity<Models.Page>(o =>
            {
                //o.ToTable("Pages", dbSchema);
            });

            modelBuilder.Entity<Models.DictionaryType>(o =>
            {
                o.ToTable("DictionaryTypes", dbSchema);
            });
            modelBuilder.Entity<Models.DictionaryItem>().HasBaseType<Models.Object>();

            modelBuilder.Entity<Models.DictionaryItem>(o =>
            {
                o.ToTable("DictionaryItems", dbSchema);

                //o.HasIndex(c => new { c.DictionaryTypeId, c.Name });
                o.Property(c => c.Name).HasColumnName("name")
                   .HasMaxLength(1000).HasColumnType("varchar(1000)") 
                   ;
                
            });

            modelBuilder.Entity<Models.Log>(o =>
            {
                o.ToTable("Logs", dbSchema);
            });

            modelBuilder.Entity<Models.LogDetail>(o =>
            {
                o.ToTable("LogDetails", dbSchema);
            });

            //bowling class
            modelBuilder.Entity<Player>(builder =>
            {
                builder.ToTable("Players", dbSchema);

                // Свойства
                builder.Property(p => p.FullName)
                    .IsRequired()
                    .HasMaxLength(150);
                builder.Property(a => a.BirthDate).HasColumnType("date");
                builder.Property(a => a.Country).HasMaxLength(100);
                builder.Property(p => p.Email)
                    .HasMaxLength(100);
                builder.Property(p => p.Phone)
                    .HasMaxLength(20);

                // Добавляем конфигурацию для свойства Gender
                builder.Property(a => a.Gender)
                    .HasDefaultValue(Gender.Unknown) // Значение по умолчанию
                    .HasConversion(
                        v => v.ToString(),           // Конвертация в строку для хранения в БД
                        v => (Gender)Enum.Parse(typeof(Gender), v))
                    .HasMaxLength(10);
                // Индексы
                builder.HasIndex(p => p.FullName)
                    .HasDatabaseName("IX_Players_FullName");

                builder.HasIndex(p => p.Email)
                    .HasDatabaseName("IX_Players_Email")
                    .IsUnique()
                    .HasFilter("\"Email\" IS NOT NULL");
                builder.HasIndex(p => p.DistrictId)
                    .HasDatabaseName("IX_Players_DistrictId");
                builder.HasIndex(p => new { p.Gender, p.BirthDate })
                    .HasDatabaseName("IX_Players_Gender_BirthDate");

                // 1. Связи много игроков -> один район
                builder.HasOne(p => p.District)
                    .WithMany(d => d.Players)          //район имеет коллекцию игроков
                    .HasForeignKey(p => p.DistrictId)  // Внешний ключ в Player
                    .OnDelete(DeleteBehavior.SetNull); // При удалении района не удаляем игрока
                
                // 2. Связь один игрок -> один рейтинг
                builder.HasOne(p => p.PlayerRating)
                    .WithOne(pr => pr.Player)
                    .HasForeignKey<PlayerRating>(pr => pr.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade); // При удалении игрока удаляем рейтинг

                // Ограничения
                //builder.HasCheckConstraint("CK_Player_Age",
                //    "DateOfBirth IS NULL OR DateOfBirth <= GETDATE()");

                //builder.HasCheckConstraint("CK_Player_Email",
                //    "Email IS NULL OR Email LIKE '%@%.%'");

                // Значения по умолчанию
                builder.Property(p => p.BirthDate)
                    .HasDefaultValue(null);

                // Seed данные (опционально)
                //builder.HasData(
                //    new Player
                //    {
                //        Id = 1,
                //        FullName = "Тестовый Игрок",
                //        Gender = Gender.Male,
                //        DistrictId = 1
                //    }
                //);


            });
            modelBuilder.Entity<Team>(builder =>
            {
                builder.ToTable("Teams", dbSchema);

                builder.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                builder.HasIndex(t => t.Name)
                    .HasDatabaseName("IX_Teams_Name")
                    .IsUnique();
                builder.Property(t => t.Abbreviation)
                    .HasMaxLength(10);
                builder.Property(t => t.LogoUrl)
                    .HasMaxLength(500);

                // Индексы
                builder.HasIndex(t => new { t.TournamentId, t.Name })
                    .HasDatabaseName("IX_Teams_Tournament_Name")
                    .IsUnique();
                builder.HasIndex(t => t.TournamentId)
                    .HasDatabaseName("IX_Teams_TournamentId");

                //1. Связь однин турнир -> много команд
                builder.HasOne(t => t.Tournament)
                    .WithMany(tournament => tournament.Teams)
                    .HasForeignKey(t => t.TournamentId) //Внешний ключ в в Team
                    .OnDelete(DeleteBehavior.Cascade);  // При удалении турнира удаляются команды
                
                //2. Связь одна команда -> много TeamMembers
                builder.HasMany(t => t.Members)
                    .WithOne(tm => tm.Team)
                    .HasForeignKey(tm => tm.TeamId)    //Внешний ключ в в TeamMembers
                    .OnDelete(DeleteBehavior.Cascade); // При удалении команды удаляются TeamMembers

                //3. Связь одна команда -> много TemResults
                builder.HasMany(t => t.Results)
                    .WithOne(tr => tr.Team)
                    .HasForeignKey(tr => tr.TeamId)     //Внешний ключ в в TemResults
                    .OnDelete(DeleteBehavior.Restrict); // Результаты не удаляем

                // Ограничения
                //builder.HasCheckConstraint("CK_Team_Name",
                //    "LEN(Name) >= 2");
            });
            //TeamMember
            modelBuilder.Entity<TeamMember>(builder =>
            {
                builder.ToTable("TeamMembers", dbSchema);

                // Свойства
                builder.Property(tm => tm.Role)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(TeamMemberRole.Member);

                builder.Property(tm => tm.IsCaptain)
                    .HasDefaultValue(false);

                builder.Property(tm => tm.OrderNumber)
                    .HasDefaultValue(0);

                builder.Property(tm => tm.AverageInTeam)
                    .HasColumnType("decimal(6,2)")
                    .HasDefaultValue(0);

                builder.Property(tm => tm.GamesPlayedInTeam)
                    .HasDefaultValue(0);

                builder.Property(tm => tm.JoinedDate)
                    .HasColumnType("timestamp with time zone") // или "timestamptz"
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();

                // Индексы
                builder.HasIndex(tm => new { tm.TeamId, tm.PlayerId })
                    .HasDatabaseName("IX_TeamMembers_Team_Player")
                    .IsUnique(); // Игрок не может быть в одной команде дважды

                builder.HasIndex(tm => tm.PlayerId)
                    .HasDatabaseName("IX_TeamMembers_PlayerId");

                builder.HasIndex(tm => new { tm.TeamId, tm.OrderNumber })
                    .HasDatabaseName("IX_TeamMembers_Team_Order");

                builder.HasIndex(tm => new { tm.TeamId, tm.IsCaptain })
                    .HasDatabaseName("IX_TeamMembers_Team_Captain")
                    .HasFilter("\"IsCaptain\" = true");

                // 1. Связи одна команда -> много TeamMembers
                builder.HasOne(tm => tm.Team)
                    .WithMany(t => t.Members)
                    .HasForeignKey(tm => tm.TeamId)
                    .OnDelete(DeleteBehavior.Cascade); //При удалении команды удаляются и TeamMembers

                // 2. Связь один игрок -> в многих командах
                builder.HasOne(tm => tm.Player)
                    .WithMany(p => p.TeamMembers)
                    .HasForeignKey(tm => tm.PlayerId)
                    .OnDelete(DeleteBehavior.Restrict); // Не удаляем игрока при удалении из команды

                // Ограничения
                //builder.HasCheckConstraint("CK_TeamMember_Order",
                //    "OrderNumber >= 0");

                //builder.HasCheckConstraint("CK_TeamMember_Average",
                //    "AverageInTeam >= 0 AND AverageInTeam <= 300");

                //builder.HasCheckConstraint("CK_TeamMember_Games",
                //    "GamesPlayedInTeam >= 0");

                //// Триггеры через HasCheckConstraint (упрощенно)
                //builder.HasCheckConstraint("CK_TeamMember_SingleCaptain",
                //    @"
                //NOT EXISTS (
                //    SELECT 1 FROM Bowling.TeamMembers tm2 
                //    WHERE tm2.TeamId = TeamId 
                //    AND tm2.IsCaptain = 1 
                //    AND tm2.Id != Id
                //) OR IsCaptain = 0
                //");

                // Ограничение на максимальное количество участников в команде
                // (проверяется на уровне приложения или триггером БД)
            });
            // Ограничение на размер файла (если нужно)
            modelBuilder.Entity<TournamentDocument>(builder =>
            {
                builder
                .Property(e => e.FileData)
                .HasColumnType("bytea");

                // Индексы для оптимизации
                builder
                    .HasIndex(d => d.TournamentId)
                    .HasDatabaseName("IX_TournamentDocuments_TournamentId");

                // 1. Связь один турнир -> много документов
                builder
                .HasOne(d => d.Tournament)
                .WithMany(td => td.Documents)
                .HasForeignKey(td => td.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);  //При удалении турнира удаляем и все документы
            });

            // Базовая конфигурация для BaseTournamentResult
            modelBuilder.Entity<BaseTournamentResult>(builder =>
            {
                builder.ToTable("TournamentResults", dbSchema);

                builder.Property(r => r.TotalScore)
                    .HasColumnType("decimal(8,2)");

                builder.Property(r => r.AverageScore)
                    .HasColumnType("decimal(6,2)");

                //// Индексы для оптимизации
                //builder.HasIndex(r => new { r.TournamentId, r.Place })
                //    .HasDatabaseName("IX_TournamentResults_Tournament_Place");

                // Связь с турниром. Один турнир -> много результатов
                builder.HasOne(r => r.Tournament)
                    .WithMany(t => t.Results)
                    .HasForeignKey(r => r.TournamentId)
                    .OnDelete(DeleteBehavior.Cascade); //При удалении турнира удаляем и результаты
            });

            // Конфигурация для IndividualResult
            modelBuilder.Entity<IndividualResult>(builder =>
            {
                builder.ToTable("IndividualResults", dbSchema);

                // Наследует все конфигурации от BaseTournamentResult
                // Индекс должен быть только на свойства в ЭТОЙ таблице
                //builder.HasIndex(r => new { r.PlayerId, r.TournamentId, r.Place })
                //    .HasDatabaseName("IX_IndividualResults_Player_Tournament_Place")
                //    .IsUnique();
                // Дополнительные индексы
                builder.HasIndex(r => r.PlayerId)
                    .HasDatabaseName("IX_IndividualResults_PlayerId");

                //builder.HasIndex(r => new { r.PlayerId, r.TournamentId })
                //    .HasDatabaseName("IX_IndividualResults_Player_Tournament")
                //    .IsUnique();

                // Связь с игроком
                builder.HasOne(r => r.Player)
                    .WithMany(p => p.IndividualResults)
                    .HasForeignKey(r => r.PlayerId)
                    .OnDelete(DeleteBehavior.Restrict); //ЗАПРЕТИТЬ удаление игрока с результатами
            });

            // Конфигурация для TeamResult
            modelBuilder.Entity<TeamResult>(builder =>
            {
                builder.ToTable("TeamResults", dbSchema);

                //builder.HasIndex(r => new { r.TeamId, r.TournamentId, r.Place })
                //   .HasDatabaseName("IX_TeamResults_Team_Tournament_Place")
                //   .IsUnique();
                // Дополнительные индексы
                builder.HasIndex(r => r.TeamId)
                    .HasDatabaseName("IX_TeamResults_TeamId");

                //builder.HasIndex(r => new { r.TeamId, r.TournamentId })
                //    .HasDatabaseName("IX_TeamResults_Team_Tournament")
                //    .IsUnique();

                // Связь с командой
                builder.HasOne(r => r.Team)
                    .WithMany(t => t.Results)
                    .HasForeignKey(r => r.TeamId)
                    .OnDelete(DeleteBehavior.Restrict); //Не удаляем команду с результатами
            });

            modelBuilder.Entity<City>(builder =>
            {
                // 1. Связь City → Players (Один город → много игроков)
                builder.HasMany(c => c.Players)
                    .WithOne(p => p.City)           // Player имеет навигационное свойство City
                    .HasForeignKey(p => p.CityId)   // Внешний ключ в Player
                    .OnDelete(DeleteBehavior.SetNull) // При удалении города, CityId = NULL у игроков
                    .HasConstraintName("FK_Players_City"); // Имя ограничения в БД

                // 2. Связь City → Tournaments (Один город → много турниров)
                builder.HasMany(c => c.Tournaments)
                    .WithOne(t => t.City)           // Tournament имеет навигационное свойство City
                    .HasForeignKey(t => t.CityId)   // Внешний ключ в Tournament
                    .OnDelete(DeleteBehavior.SetNull); // При удалении города, CityId = NULL в турнире
                
                // 3. Связь City → Tournaments (Один город → много турниров)
                builder.HasMany(c => c.BowlingCenters)
                    .WithOne(t => t.City)           // Bowling имеет навигационное свойство City
                    .HasForeignKey(t => t.CityId)   // Внешний ключ в Bowling
                    .OnDelete(DeleteBehavior.SetNull); // При удалении города, CityId = NULL в Bowling

                //4. Связь много городов -> один район
                builder.HasOne(c => c.District)
                    .WithMany(d => d.Cities)
                    .HasForeignKey(c => c.DistrictId)
                    .OnDelete(DeleteBehavior.SetNull); //при удалении района DistrictId = null в City

            });
            modelBuilder.Entity<District>(builder =>
            {
                builder.ToTable("Districts", dbSchema);
                // 1. Связи District → Cities (Один округ → много городов)
                builder.HasMany(a => a.Cities)
                    .WithOne(tm => tm.District)         // City имеет навигационное свойство District
                    .HasForeignKey(tm => tm.DistrictId) // Внешний ключ в City
                    .OnDelete(DeleteBehavior.SetNull); // При удалении района, DistrictId = NULL в городе
                // 2. Связи District → Players (Один округ → много игроков)
                builder.HasMany(a => a.Players)
                    .WithOne(tm => tm.District)         // Player имеет навигационное свойство District
                    .HasForeignKey(tm => tm.DistrictId) // Внешний ключ в Player
                    .OnDelete(DeleteBehavior.SetNull); // При удалении района, DistrictId = NULL у игрока
            });
            
            modelBuilder.Entity<Oil>(builder =>
            {
                builder.ToTable("Oils", dbSchema);

                // Связи
                builder.HasMany(a => a.Tournaments)
                    .WithOne(tm => tm.Oil)
                    .HasForeignKey(tm => tm.OilId)
                    .OnDelete(DeleteBehavior.SetNull); // При удалении программы масла, OilId = NULL в турнире
            });
            modelBuilder.Entity<Bowling>(builder =>
            {
                builder.ToTable("Bowlings", dbSchema);
                // Связи
                builder.HasMany(b => b.Tournaments)
                    .WithOne(t => t.Bowling)
                    .HasForeignKey(t => t.BowlingId)
                    .OnDelete(DeleteBehavior.SetNull); // При удалении боулинга, BowlingId = NULL в таблице турнира
                builder.HasOne(b => b.City)
                    .WithMany(c => c.BowlingCenters)
                    .HasForeignKey(b => b.CityId)
                    .OnDelete(DeleteBehavior.Cascade); //При удалении города удаляем и боулинги
            });

            // SQL View для рейтинга
            modelBuilder.Entity<PlayerRankingView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_PlayerRankings", "Bowling");
            });

            modelBuilder.Entity<Tournament>(builder =>
            {
                builder.ToTable("Tournaments", dbSchema);
                builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200);
                builder.Property(t => t.TournamentType)
                    .HasDefaultValue(TournamentType.Unknown) // Значение по умолчанию
                    .HasConversion(
                        v => v.ToString(),           // Конвертация в строку для хранения в БД
                        v => (TournamentType)Enum.Parse(typeof(TournamentType), v))
                    .HasMaxLength(20)
                    .HasDefaultValue(TournamentType.Unknown);

                builder.Property(t => t.Format)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(TournamentFormat.Unknown);

                builder.Property(t => t.ScoringSystem)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(ScoringSystem.Scratch);


                builder.Property(t => t.StartDate)
                    .HasColumnType("date");
                //.IsRequired();

                builder.Property(t => t.EndDate)
                    .HasColumnType("date");
                    //.IsRequired();

                // Индексы
                builder.HasIndex(t => t.StartDate)
                    .HasDatabaseName("IX_Tournaments_StartDate");

                builder.HasIndex(t => new { t.TournamentType, t.StartDate })
                    .HasDatabaseName("IX_Tournaments_Type_Date");

                // Связи
                // Настройка связи Tournament - TournamentDocument
                builder
                    .HasMany(e => e.Documents)
                    .WithOne(d => d.Tournament)
                    .HasForeignKey(d => d.TournamentId)
                    .OnDelete(DeleteBehavior.Cascade); // При удалении турнира удаляются и документы
                builder.HasMany(e => e.Results)
                    .WithOne(d => d.Tournament)
                    .HasForeignKey(d => d.TournamentId)
                    .OnDelete(DeleteBehavior.Cascade); // При удалении турнира удаляются и результаты

            });
            modelBuilder.Entity<PlayerRating>(builder =>
            {
                builder.ToTable("PlayerRatings", dbSchema);


                builder.Property(pr => pr.Rating)
                    .HasDefaultValue(1500);

                builder.Property(pr => pr.PeakRating)
                    .HasDefaultValue(1500);

                builder.Property(pr => pr.AveragePlace)
                    .HasColumnType("decimal(5,2)");

                builder.Property(pr => pr.AverageScore)
                    .HasColumnType("decimal(6,2)");

                // Индексы
                builder.HasIndex(pr => pr.Rating)
                    .HasDatabaseName("IX_PlayerRatings_Rating")
                    .IsDescending();

                builder.HasIndex(pr => pr.PlayerId)
                    .HasDatabaseName("IX_PlayerRatings_PlayerId")
                    .IsUnique();

                // Связи
                builder.HasOne(pr => pr.Player)
                    .WithOne(p => p.PlayerRating) // Указываем навигационное свойство в Player
                    .HasForeignKey<PlayerRating>(pr => pr.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade); //При удалении игрока удаляем и его рейтинг
            });

            modelBuilder.Entity<RatingHistory>(builder =>
            {
                builder.ToTable("RatingHistories", "Bowling");


                builder.Property(rh => rh.ChangeReason)
                    .HasMaxLength(50);

                // Индексы
                builder.HasIndex(rh => new { rh.PlayerRatingId, rh.ChangeDate })
                    .HasDatabaseName("IX_RatingHistories_Player_Date");

                builder.HasIndex(rh => rh.TournamentId)
                    .HasDatabaseName("IX_RatingHistories_TournamentId");

                // Связи
                builder.HasOne(rh => rh.PlayerRating)
                    .WithMany(pr => pr.History) // Указать навигационное свойство
                    .HasForeignKey(rh => rh.PlayerRatingId)
                    .OnDelete(DeleteBehavior.Cascade); //При удалении рейтинга удаляем и всю историю

                builder.HasOne(rh => rh.Tournament)
                    .WithMany(t => t.History)
                    .HasForeignKey(rh => rh.TournamentId)
                    .OnDelete(DeleteBehavior.Restrict); //Запретить удаление турнира, если есть история
            });
        }

       private Type? FindTypeInAssembly(Assembly assembly, string name)
        {
            var appAssemblyTypes = assembly.GetTypes();
            return appAssemblyTypes.FirstOrDefault(o => o.Name == name); 
        }
        private List<Type> FetchDbSetTypes(Type dbContext)
        {

            var properties = dbContext.GetType().GetProperties();
            var dbSets = new List<Type>();
            foreach (var property in properties)
            {
                var propertyType = property.PropertyType;
                if (propertyType.IsGenericType && propertyType.Name.ToLower().Contains("dbset"))
                {
                    Type dbSetType = propertyType.GenericTypeArguments[0]; //point of interest here
                    dbSets.Add(dbSetType);
                }
            }
            return dbSets;
        }
        private IEnumerable<IEntityType> FetchDbSetTypes2()
        {
            var dbSets = this.Model.GetEntityTypes();

            return dbSets;
        }
        private List<T> FetchFromTableWhere<T>(DbSet<T> _, string condition) where T : class
        {
            return this.Set<T>().Where(condition).ToList();
        }
        private long FetchFromTableSelectFirstOrDefault<T>(DbSet<T> _, string condition) where T : class
        {
            return this.Set<T>().Select(condition).FirstOrDefault();
        }
        public virtual int GetKey<T>(T entity)
        {
            var keyName = this.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties
                .Select(x => x.Name).Single();
            return (int)entity.GetType().GetProperty(keyName).GetValue(entity, null);
        }
        public Models.ObjectType GetObjectType<T>()
        {
            return GetObjectType(typeof(T));
        }
        public Models.ObjectType? GetObjectType(string typeName, bool createProperties = true)
        {
            var assemblyType = typeof(Models.ObjectType).Assembly.GetTypes().FirstOrDefault(type => type.Name == typeName);
            if (assemblyType != null)
            {
                return GetObjectType(assemblyType, null, createProperties);
            }
            return null;
        }
        public Models.ObjectType GetObjectType(Type type, Models.ObjectType[]? objectTypes = null, bool createProperties = true, bool overwrit = true)
        {
            Models.ObjectType? objectType;

            if (objectTypes == null)
            {
                objectType = ObjectTypes.Include(o => o.Properties).SingleOrDefault(o => o.Name == type.Name);
            }
            else
            {
                objectType = objectTypes?.SingleOrDefault(o => o.Name == type.Name);
                
            }
            if (!overwrit && objectType != null && !createProperties) return objectType;
            if (objectType == null)
            {
                objectType = new Models.ObjectType()
                {
                    Name = type.Name,
                    Title = type.Name
                };
                ObjectTypes.Add(objectType);

                if (!createProperties)
                {
                    SaveChanges();
                }
            }
            


            if (objectType != null && overwrit)
            {
                objectType.NameSpace = type.Namespace;

                var displayExprType = FindTypeInAssembly(type.Assembly, "displayExpr");
                
                if (displayExprType == null) return objectType;
                var members = type.GetMembers();
                IEnumerable<MemberInfo> memWithAttr = from member in members
                                                  where member.GetCustomAttribute(displayExprType, false) != null
                                                  select member;
                if (memWithAttr.Any())
                {
                    objectType.DisplayExpr = memWithAttr.FirstOrDefault().Name;
                }
            }
            
            if (createProperties)
            {
                var typeProperties = type.GetProperties()
                                        .Select(propertyInfo => GetObjectProperty(propertyInfo))
                                        .ToList();

                foreach (var idProp in (from o in typeProperties
                                        where o.IsIdentifier
                                        select o).ToArray())
                {
                    foreach (MemberInfo i in (type.GetMember(idProp.Name)))
                    {
                        foreach (object at in i.GetCustomAttributes(true))
                        {
                            Models.ObjectPropertyAttribute map = at as Models.ObjectPropertyAttribute;
                            if (map != null)
                            {
                                
                                if (idProp.TypeName == null || idProp.TypeName == "")
                                    idProp.TypeName = map.TypeName;
                                if (idProp.DisplayExpr == null)
                                    idProp.DisplayExpr = map.DisplayExpr;
                                if (idProp.NameSpace == null)
                                    idProp.NameSpace = map.NameSpace;
                            }
                        }
                    }
                    if (idProp.TypeName == null) {
                        if (idProp.Name.EndsWith("Id"))
                        {
                            string objPropName = idProp.Name.Substring(0, idProp.Name.Length - 2);
                            var objProp = typeProperties.SingleOrDefault(prop => prop.Name == objPropName);
                            if (objProp != null)
                            {
                                idProp.TypeName = objProp.TypeName;
                                idProp.DictionaryTypeName = objProp.DictionaryTypeName;
                            }
                            else
                            {
                                var dbContextTypes = type.Assembly
                                 .DefinedTypes.Where(t => typeof(DbContext).IsAssignableFrom(t))
                                 .ToList();

                                var tapeName = objPropName;
                                List<Type> dbsetTypes = FetchDbSetTypes(dbContextTypes.FirstOrDefault());
                                foreach (var dbsetType in dbsetTypes)
                                {
                                    if (dbsetType.Name == tapeName)
                                    {
                                        idProp.TypeName = tapeName;
                                    }
                                }
                            }
                        }
                        else if (idProp.Name.StartsWith("Id") && !idProp.Name.EndsWith("Navigation"))
                        {
                            string objPropName = idProp.Name + "Navigation";
                            var objProp = typeProperties.SingleOrDefault(prop => prop.Name == objPropName);
                            if (objProp != null)
                            {
                                idProp.TypeName = objProp.TypeName;
                                idProp.DictionaryTypeName = objProp.DictionaryTypeName;
                            }
                            else
                            {
                                if (idProp.Name.Length > 2)
                                {
                                    var dbContextTypes = type.Assembly
                                        .DefinedTypes.Where(t => typeof(DbContext).IsAssignableFrom(t))
                                        .ToList();
                                    var tapeName = idProp.Name.Substring(2, idProp.Name.Length - 2);
                                    List<Type> dbsetTypes = FetchDbSetTypes(dbContextTypes.FirstOrDefault());
                                    foreach (var dbsetType in dbsetTypes)
                                    {
                                        if (dbsetType.Name == tapeName)
                                        {
                                            idProp.TypeName = tapeName;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (idProp.TypeName != null)
                    {
                        Type? myType = null;
                        try
                        {
                            myType = type.Assembly.GetType(type.Namespace + "." + idProp.TypeName);
                        }
                        catch { }
                        if (myType != null) {
                            var displayExprType = FindTypeInAssembly(type.Assembly, "displayExpr");
                            var fields = myType.GetMembers();
                            IEnumerable<MemberInfo> miInfos = from member in fields
                                                              where member.GetCustomAttribute(displayExprType, false) != null// && member.GetCustomAttribute<PirAppBp.Models.Sapsan.displayExpr>().Match(new PirAppBp.Models.Sapsan.displayExpr())
                                                              select member;
                            if (miInfos.Any())
                            {
                                if (idProp.DisplayExpr == null)
                                {
                                    idProp.DisplayExpr = miInfos.FirstOrDefault().Name;
                                }
                            }
                        }
                    }
                    //if (idProp.TypeName == null)
                    //{

                    //    IEntityType? fake = this.Model.FindEntityType(type);
                    //    var references = Entry(fake);//.GetDeclaredReferencingForeignKeys(); //.GetNavigations().FirstOrDefault(o => o.Name.Contains(idProp);
                    //    if(references!=null)
                    //        foreach (var refer in ((INavigation)references.Metadata).ForeignKey.Properties)
                    //        {
                    //            var name = refer.Name;
                    //        }
                    //}
                    //if (idProp.TypeName == null && idProp.Name.ToLower()!="id")
                    //{
                    //    List<Type> dbsetTypes = FetchDbSetTypes();
                    //    var rr = FetchDbSetTypes2();
                    //    long valueId = 0;
                    //    string condition;
                    //    Type? propType = null;
                    //    foreach (var dbsetType in dbsetTypes)
                    //    {
                    //        if (type.Name == dbsetType.Name)
                    //        {
                    //            try
                    //            {
                    //                condition = idProp.Name;
                    //                Type myType = typeof(Microsoft.EntityFrameworkCore.Internal.InternalDbSet<>).MakeGenericType(dbsetType);
                    //                dynamic instance = Activator.CreateInstance(myType, this, dbsetType.Name);
                    //                valueId = FetchFromTableSelectFirstOrDefault(instance, condition);
                    //                IEntityType? fake = this.Model.FindEntityType(dbsetType);
                    //                if (fake != null)
                    //                    propType = Type.GetType(fake?.Name)?.GetProperty(idProp.Name).PropertyType;
                    //            }
                    //            catch (Exception)
                    //            {
                    //                //might fail due to other models
                    //            }
                    //        }
                    //    }
                    //    foreach (var dbsetType in dbsetTypes)
                    //    {
                    //        if (propType != null && valueId != 0 && dbsetType.Name != "User" && dbsetType.Name != "Role" && dbsetType.Name != "Action" && dbsetType.Name != "Page" && dbsetType.Name != "LogDetail" && dbsetType.Name != "DictionaryType" && dbsetType.Name != "Log" && dbsetType.Name != "Component" && dbsetType.Name != "Object" && dbsetType.Name != "System" && dbsetType.Name != "Container" && dbsetType.Name != "DictionaryItem" && dbsetType.Name != "ObjectType" && dbsetType.Name != "ObjectProperty" && idProp.Name != "Id")
                    //        {
                    //            IEntityType fake = this.Model.FindEntityType(dbsetType);
                    //            var key = fake?.FindPrimaryKey()?.Properties.Select(x => x.Name).Single();
                    //            var keyType = Type.GetType(fake?.Name)?.GetProperty(key).PropertyType;
                    //            condition = string.Format("{0} == {1}", key, valueId);
                    //            string propTypeName = propType.Name;
                    //            if (propType.IsGenericType)
                    //                propTypeName = propType.GenericTypeArguments[0].Name;
                    //            if (keyType.Name == propTypeName)
                    //                try
                    //                {
                    //                    Type myType = typeof(Microsoft.EntityFrameworkCore.Internal.InternalDbSet<>).MakeGenericType(dbsetType);
                    //                    dynamic instance = Activator.CreateInstance(myType, this, dbsetType.Name);

                    //                    var typeNamesFromDb = FetchFromTableWhere(instance, condition);
                    //                    if (typeNamesFromDb.Count > 0)
                    //                    {

                    //                        idProp.TypeName = dbsetType.Name;
                    //                        break;

                    //                    }
                    //                    else
                    //                    {

                    //                    }
                    //                }
                    //                catch (Exception ex)
                    //                {
                    //                    //might fail due to other models
                    //                }
                    //        }
                    //    }
                    //}

                }

                int sortIndex = 11;
                foreach (var prop in (from o in typeProperties
                                      where o.SortIndex == null
                                      select o).ToArray())
                {
                    prop.SortIndex = sortIndex;
                    prop.IsNullable = true;
                    sortIndex++;
                }

                bool isChanged = false;

                if (objectType.Properties == null || objectType.Properties.Count == 0)
                {
                    objectType.Properties = typeProperties;
                    isChanged = true;
                }
                else
                {

                    foreach (var objProp in objectType.Properties)
                    {
                        var typeProp = typeProperties.SingleOrDefault(p => p.Name == objProp.Name);
                        if (typeProp != null)
                        {
                            if (objProp.TypeName != typeProp.TypeName ||
                                objProp.DictionaryTypeName != typeProp.DictionaryTypeName ||
                                objProp.IsIdentifier != typeProp.IsIdentifier ||
                                objProp.IsNumeric != typeProp.IsNumeric ||
                                objProp.IsBoolean != typeProp.IsBoolean ||
                                objProp.IsString != typeProp.IsString ||
                                objProp.IsGuid != typeProp.IsGuid ||
                                objProp.IsDate != typeProp.IsDate ||
                                objProp.IsObject != typeProp.IsObject ||
                                objProp.IsArray != typeProp.IsArray ||
                                objProp.DisplayExpr != typeProp.DisplayExpr ||
                                objProp.IsNotAvailable != typeProp.IsNotAvailable ||
                                objProp.NameSpace != typeProp.NameSpace)
                            {
                                objProp.TypeName = typeProp.TypeName;
                                objProp.DictionaryTypeName = typeProp.DictionaryTypeName;
                                objProp.IsIdentifier = typeProp.IsIdentifier;
                                objProp.IsNumeric = typeProp.IsNumeric;
                                objProp.IsBoolean = typeProp.IsBoolean;
                                objProp.IsString = typeProp.IsString;
                                objProp.IsGuid = typeProp.IsGuid;
                                objProp.IsDate = typeProp.IsDate;
                                objProp.IsObject = typeProp.IsObject;
                                objProp.IsArray = typeProp.IsArray;
                                objProp.DisplayExpr = typeProp.DisplayExpr;
                                objProp.NameSpace = typeProp.NameSpace;
                                objProp.IsNotAvailable = typeProp.IsNotAvailable;
                                isChanged = true;
                            }
                        }
                        else
                        {
                            if (!objProp.IsNotAvailable)
                            {
                                objProp.IsNotAvailable = true;
                                isChanged = true;
                            }
                        }
                    }
                    foreach (var typeProp in typeProperties)
                    {
                        var objProp = objectType.Properties.SingleOrDefault(p => p.Name == typeProp.Name);
                        if (objProp == null)
                        {
                            objectType.Properties.Add(typeProp);
                            isChanged = true;
                        }
                    }

                }

                if (isChanged)
                {
                    SaveChanges();
                }
            }
 

            return objectType;
        }
        private Models.ObjectProperty GetObjectProperty(PropertyInfo propertyInfo)
        {
            var propType = propertyInfo.PropertyType;

            var property = new Models.ObjectProperty()
            {
                Name = propertyInfo.Name,
                IsNullable = (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>)),
            };

            if (propType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propType))
            {
                property.IsArray = true;
            }
            else if(propType.IsClass && propType.FullName != "System.String")
            {
                property.IsObject = true;
            }

            if (property.IsObject || property.IsArray)
            {
                property.TypeName = propType.Name;

                if (property.Name != "Type")
                {
                    property.DictionaryTypeName = DictionaryTypes.SingleOrDefault(o => o.Name.StartsWith(property.Name))?.Name;
                }
            }

            switch (propertyInfo.Name)
            {
                case "Id":
                    property.Title = "Id";
                    property.IsNotAvailable = true;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 1;
                    break;

                case "SortIndex":
                    property.Title = "Порядок сортировки";
                    property.SortIndex = 2;
                    break;

                case "SapsanId":
                    property.Title = "Id Сапсан";
                    property.IsNotAvailable = true;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 3;
                    break;

                case "Name":
                    property.Title = "Имя";
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 4;
                    break;

                case "Title":
                    property.Title = "Название";
                    property.SortIndex = 5;
                    break;

                case "Description":
                    property.Title = "Описание";
                    property.SortIndex = 6;
                    property.IsNullable = true;
                    break;

                case "IconName":
                    property.Title = "Имя иконки";
                    property.IsNullable = true;
                    property.IsNotAvailable = true;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 7;
                    break;

                case "TypeId":
                    property.Title = "Id типа объекта";
                    property.IsNotAvailable = true;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 8;
                    break;

                case "Type":
                    property.Title = "Тип объекта";
                    property.IsNotAvailable = true;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 9;
                    break;

                case "ParentId":
                    property.Title = "Id родителя";
                    property.IsNotAvailable = true;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 10;
                    break;

                case "Parent":
                    property.Title = "Родитель";
                    property.IsNotAvailable = true;
                    property.SortIndex = 11;
                    break;

                case "DictionaryTypeId":
                    property.Title = "Id типа справочника";
                    property.IsNotAvailable = true;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 12;
                    break;

                case "DictionaryType":
                    property.Title = "Тип справочника";
                    property.IsHiddenByDefault = true;
                    property.IsReadOnly = true;
                    property.SortIndex = 13;
                    break;

                case "IsNotUsed":
                    property.Title = "Не используется";
                    property.SortIndex = 14;
                    break;

                case "IsSystem":
                    property.Title = "Системный";
                    property.IsNotAvailable = true;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 15;
                    break;

                case "CreatedAt":
                    property.Title = "Время создания";
                    property.IsHiddenByDefault = true;
                    property.SortIndex = 101;
                    property.IsReadOnly = true;
                    property.IsHiddenInLogDetail = true;
                    break;

                case "AuthorId":
                    property.Title = "Id создателя";
                    property.IsNotAvailable = false;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 102;
                    break;

                case "Author":
                    property.Title = "Создал";
                    property.IsHiddenByDefault = true;
                    property.IsHiddenInLogDetail = true;
                    property.IsInclude = true;
                    property.IsReadOnly = true;
                    property.SortIndex = 103;
                    break;

                case "ModifiedAt":
                    property.Title = "Время изменения";
                    property.IsHiddenByDefault = true;
                    property.IsHiddenInLogDetail = true;
                    property.IsReadOnly = true;
                    property.SortIndex = 104;
                    break;

                case "EditorId":
                    property.Title = "Id изменившего";
                    property.IsNotAvailable = true;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 105;
                    break;

                case "Editor":
                    property.Title = "Изменил";
                    property.IsHiddenByDefault = true;
                    property.IsHiddenInLogDetail = true;
                    property.IsInclude = true;
                    property.IsReadOnly = true;
                    property.SortIndex = 106;
                    break;

                case "Version":
                    property.Title = "Версия";
                    property.IsNotAvailable = true;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 107;
                    break;

                case "DeletedAt":
                    property.Title = "Время удаления";
                    property.IsNotAvailable = true;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 108;
                    break;

                case "DeleterId":
                    property.Title = "Id удалившего";
                    property.IsNotAvailable = true;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 109;
                    break;

                case "Deleter":
                    property.Title = "Удалил";
                    property.IsNotAvailable = true;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 110;
                    break;

                case "IsDeleted":
                    property.Title = "Удален";
                    property.IsNotAvailable = true;
                    property.IsHiddenInLogDetail = true;
                    property.SortIndex = 111;
                    break;

                default:
                    var type = ObjectTypes.FirstOrDefault(type => type.Name == propertyInfo.Name);
                    if (type != null)
                    {
                        property.Title = type.Title;
                    }
                    else
                    {
                        property.Title = propertyInfo.Name;
                    }
                    break;
            }

            if ((propertyInfo.Name.EndsWith("Id") || (propertyInfo.Name.StartsWith("Id") && !propertyInfo.Name.EndsWith("Navigation"))) &&
                (propType == typeof(int) || propType == typeof(int?) ||
                 propType == typeof(long) || propType == typeof(long?) ||
                 propType == typeof(long) || propType == typeof(long?) ||
                 propType == typeof(Guid) || propType == typeof(Guid?)))
            {
                property.IsIdentifier = true;
            }

            if (propType == typeof(int) || propType == typeof(int?) ||
                     propType == typeof(long) || propType == typeof(long?) ||
                     propType == typeof(short) || propType == typeof(short?))
            {
                property.IsNumeric = true;
            }
            else if (propType == typeof(float) || propType == typeof(float?) ||
                     propType == typeof(decimal) || propType == typeof(decimal?) ||
                     propType == typeof(double) || propType == typeof(double?))
            {
                property.IsNumeric = true;
                property.DataFormat = "#,##0.00";
            }
            else if (propType == typeof(bool) || propType == typeof(bool?))
            {
                property.IsBoolean = true;
            }
            else if (propType == typeof(string))
            {
                property.IsString = true;
            }
            else if (propType == typeof(long) || propType == typeof(long?))
            {
                property.IsGuid = true;
            }
            else if (propType == typeof(DateTime) || propType == typeof(DateTime?))
            {
                property.IsDate = true;
                property.DataFormat = "dd.MM.yyyy HH:mm";
            }
            else if (propType == typeof(DateOnly) || propType == typeof(DateOnly?))
            {
                property.IsDate = true;
                property.DataFormat = "dd.MM.yyyy";
            }
            else
            {
            }

            return property;
        }
    }
}
