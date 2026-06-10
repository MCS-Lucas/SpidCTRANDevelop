BEGIN TRANSACTION;
ALTER TABLE [Usuarios] ADD [IsTeste] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260610172307_AddIsTesteUsuario', N'10.0.5');

COMMIT;
GO

