using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Open.Server.Migrations
{
    public partial class addedclienttable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CZ_Clients",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<string>(nullable: true),
                    ClientSecret = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CZ_Clients", x => x.Id);
                });


            migrationBuilder.Sql(@"
    INSERT INTO ""CZ_Clients"" (""ClientId"", ""ClientSecret"") VALUES
	('000000003', 'abcdef-ghij-klemn-opqrst'),
	('000000006', '1234567890-123456789'),
	('000000002', 'efaldj-alaldjfaj-aaldflaf'),
	('000000007', 'adfdfd334-adf-ff-adfkfj'),
	('000000023', 'adfafa-afafasdf-afasdf');
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CZ_Clients");
        }
    }
}
