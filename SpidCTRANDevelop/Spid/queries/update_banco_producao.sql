BEGIN TRANSACTION;
ALTER TABLE [Colaboradores] DROP CONSTRAINT [FK_Colaboradores_Setores_SetorId];

ALTER TABLE [ConferenciasMensais] DROP CONSTRAINT [FK_ConferenciasMensais_Setores_SetorId];

ALTER TABLE [Usuarios] DROP CONSTRAINT [FK_Usuarios_Setores_SetorId];

ALTER TABLE [Viagens] DROP CONSTRAINT [FK_Viagens_Setores_SetorId];

EXEC sp_rename N'[Setores]', N'CentrosCusto', 'OBJECT';

EXEC sp_rename N'[Viagens].[SetorId]', N'CentroCustoId', 'COLUMN';

EXEC sp_rename N'[Viagens].[IX_Viagens_SetorId]', N'IX_Viagens_CentroCustoId', 'INDEX';

EXEC sp_rename N'[Usuarios].[SetorId]', N'CentroCustoId', 'COLUMN';

EXEC sp_rename N'[Usuarios].[IX_Usuarios_SetorId]', N'IX_Usuarios_CentroCustoId', 'INDEX';

EXEC sp_rename N'[ConferenciasMensais].[SetorId]', N'CentroCustoId', 'COLUMN';

EXEC sp_rename N'[ConferenciasMensais].[IX_ConferenciasMensais_SetorId_Ano_Mes]', N'IX_ConferenciasMensais_CentroCustoId_Ano_Mes', 'INDEX';

EXEC sp_rename N'[Colaboradores].[SetorId]', N'CentroCustoId', 'COLUMN';

EXEC sp_rename N'[Colaboradores].[IX_Colaboradores_SetorId]', N'IX_Colaboradores_CentroCustoId', 'INDEX';

ALTER TABLE [Colaboradores] ADD CONSTRAINT [FK_Colaboradores_CentrosCusto_CentroCustoId] FOREIGN KEY ([CentroCustoId]) REFERENCES [CentrosCusto] ([Id]) ON DELETE CASCADE;

ALTER TABLE [ConferenciasMensais] ADD CONSTRAINT [FK_ConferenciasMensais_CentrosCusto_CentroCustoId] FOREIGN KEY ([CentroCustoId]) REFERENCES [CentrosCusto] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Usuarios] ADD CONSTRAINT [FK_Usuarios_CentrosCusto_CentroCustoId] FOREIGN KEY ([CentroCustoId]) REFERENCES [CentrosCusto] ([Id]) ON DELETE SET NULL;

ALTER TABLE [Viagens] ADD CONSTRAINT [FK_Viagens_CentrosCusto_CentroCustoId] FOREIGN KEY ([CentroCustoId]) REFERENCES [CentrosCusto] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260521163023_RenameSetorToCentroCusto', N'10.0.5');

COMMIT;
GO

