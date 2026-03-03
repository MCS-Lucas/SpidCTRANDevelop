-- Alteração no armazenamento dos horários das viagens no Banco
-- Apenas rode essa query no SSMS para correção

-- Migration: CorrigirPrecisaoHorarios
-- Altera HoraInicio e HoraFim de time(7) para time(0)

BEGIN TRANSACTION;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260303140603_CorrigirPrecisaoHorarios'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c]
        ON [d].[parent_column_id] = [c].[column_id]
        AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Viagens]')
        AND [c].[name] = N'HoraInicio');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Viagens] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Viagens] ALTER COLUMN [HoraInicio] time(0) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260303140603_CorrigirPrecisaoHorarios'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c]
        ON [d].[parent_column_id] = [c].[column_id]
        AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Viagens]')
        AND [c].[name] = N'HoraFim');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Viagens] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [Viagens] ALTER COLUMN [HoraFim] time(0) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260303140603_CorrigirPrecisaoHorarios'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260303140603_CorrigirPrecisaoHorarios', N'10.0.3');
END;

COMMIT;
