using FluentMigrator;

namespace NewsHole.Data.Migrations
{
    [Migration(201401152156)]
    public class _2156_Create_User_And_ResetPassword_Tables : Migration
    {
        public override void Up()
        {
            Create.Table("User")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Email").AsString()
                .WithColumn("FirstName").AsString()
                .WithColumn("LastName").AsString()
                .WithColumn("PasswordHash").AsString();

            Create.Table("ResetPasswordEntry")
                .WithColumn("Token").AsString().PrimaryKey()
                .WithColumn("EntryDateTime").AsDateTime()
                .WithColumn("UserId").AsInt32().ForeignKey();
        }

        public override void Down()
        {
            Delete.Table("ResetPasswordEntry");
            Delete.Table("User");
        }
    }
}
