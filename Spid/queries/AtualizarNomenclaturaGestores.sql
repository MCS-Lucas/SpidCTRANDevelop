-- Atualizar usuários
UPDATE Usuarios 
SET Perfil = 'Gestor Titular' 
WHERE Perfil = 'Gestor Primário';

UPDATE Usuarios 
SET Perfil = 'Gestor Substituto' 
WHERE Perfil = 'Gestor Secundário';

-- Atualizar permissões de recursos
UPDATE PerfisRecurso 
SET Perfil = 'Gestor Titular' 
WHERE Perfil = 'Gestor Primário';

UPDATE PerfisRecurso 
SET Perfil = 'Gestor Substituto' 
WHERE Perfil = 'Gestor Secundário';
