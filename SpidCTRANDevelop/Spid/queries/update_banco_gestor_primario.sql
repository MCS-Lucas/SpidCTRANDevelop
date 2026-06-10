-- Atualiza a tabela de Usuários
UPDATE Usuarios
SET Perfil = 'Gestor Primário'
WHERE Perfil = 'Gestor Principal';
-- Atualiza a tabela de regras/permissões
UPDATE PerfisRecurso
SET Perfil = 'Gestor Primário'
WHERE Perfil = 'Gestor Principal';