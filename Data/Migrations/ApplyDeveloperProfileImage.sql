BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021120000_AddDeveloperProfileImage'
)
BEGIN
    ALTER TABLE [Developers] ADD [ProfileImagePath] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021120000_AddDeveloperProfileImage'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261021120000_AddDeveloperProfileImage', N'10.0.10');
END;

COMMIT;
GO

