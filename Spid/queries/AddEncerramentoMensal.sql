-- Migration: Criar tabela EncerramentosMensais
-- Executar no banco de dados do SPID

CREATE TABLE [EncerramentosMensais] (
    [Id] int NOT NULL IDENTITY,
    [Ano] int NOT NULL,
    [Mes] int NOT NULL,
    [Encerrado] bit NOT NULL DEFAULT 0,
    [EncerradoPorId] int NULL,
    [DataEncerramento] datetime2 NULL,
    [LiberadoPorId] int NULL,
    [DataLiberacao] datetime2 NULL,
    CONSTRAINT [PK_EncerramentosMensais] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EncerramentosMensais_Usuarios_EncerradoPorId] FOREIGN KEY ([EncerradoPorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EncerramentosMensais_Usuarios_LiberadoPorId] FOREIGN KEY ([LiberadoPorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_EncerramentosMensais_Ano_Mes] ON [EncerramentosMensais] ([Ano], [Mes]);
CREATE INDEX [IX_EncerramentosMensais_EncerradoPorId] ON [EncerramentosMensais] ([EncerradoPorId]);
CREATE INDEX [IX_EncerramentosMensais_LiberadoPorId] ON [EncerramentosMensais] ([LiberadoPorId]);
