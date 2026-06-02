-- Script para atribuir as mesmas capacidades de banco do perfil Admin para os novos perfis Gestor Central
-- O perfil Admin atualmente possui acesso ao recurso 'ImportarViagens'

DECLARE @RecursoImportarId INT = (SELECT Id FROM Recursos WHERE Chave = 'ImportarViagens');

IF @RecursoImportarId IS NOT NULL
BEGIN
    -- Gestor Central Padrão
    IF NOT EXISTS (SELECT 1 FROM PerfisRecurso WHERE Perfil = 'Gestor Central Padrão' AND RecursoId = @RecursoImportarId)
    BEGIN
        INSERT INTO PerfisRecurso (Perfil, RecursoId) VALUES ('Gestor Central Padrão', @RecursoImportarId);
    END

    -- Gestor Central Ateste
    IF NOT EXISTS (SELECT 1 FROM PerfisRecurso WHERE Perfil = 'Gestor Central Ateste' AND RecursoId = @RecursoImportarId)
    BEGIN
        INSERT INTO PerfisRecurso (Perfil, RecursoId) VALUES ('Gestor Central Ateste', @RecursoImportarId);
    END
END
GO
