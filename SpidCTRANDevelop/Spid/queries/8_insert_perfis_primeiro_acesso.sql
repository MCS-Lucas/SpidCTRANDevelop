-- 1. Excluir os perfis que criamos com a senha errada
DELETE FROM Usuarios WHERE Ponto IN ('P_6355', 'P_7191', 'P_7427', 'P_5420', 'P_922891', 'P_923223', 'P_919827');

-- 2. Inserindo Gestores Titulares (buscando automaticamente o ID do Centro de Custo pela sigla e usando o Hash do próprio Ponto)
INSERT INTO Usuarios (Nome, Email, Ponto, Perfil, SenhaHash, Cpf, Ativo, IsTeste, ContadorAcessos, CentroCustoId)
VALUES 
('Flávio Shinji Mori', '', 'P_6355', 'Gestor Titular', 'AQAAAAIAAYagAAAAEEKSfkBP8MrV+d7eTYJQH4BiwNQpb2cg6ZIxLpza41asgCZ0UKe7l43PLZDnS/n5Aw==', NULL, 1, 0, 0, (SELECT Id FROM CentrosCusto WHERE Nome = 'RESOFI/PRESI')),
('Wilton Souza Alencar', '', 'P_7191', 'Gestor Titular', 'AQAAAAIAAYagAAAAEAPMZMIazHlbcuUfUeKQ3wDPzagatH3G7uoo2l8dRr5uWOx4nX4mz+AY8gPbfbWLSA==', NULL, 1, 0, 0, (SELECT Id FROM CentrosCusto WHERE Nome = 'CAINF/DITEC')),
('Thaissa Carvalho Tavares Morato', '', 'P_7427', 'Gestor Titular', 'AQAAAAIAAYagAAAAEGX5Dgye9ATMlBwvUP6s4x/XFo6fdbSNTSlIL8871sutHqeN14/4XQHP52yXSXJ0XA==', NULL, 1, 0, 0, (SELECT Id FROM CentrosCusto WHERE Nome = 'PROSAUDE/DAS')),
('Valéria Aparecida Olinto Pessoa', '', 'P_5420', 'Gestor Titular', 'AQAAAAIAAYagAAAAEMMQCOj0ALTf2i1I/NwE5ATzHAtKkyFVTEvFYIwzDy2JBTorgXpFxCJ/RJ7aqqPpjw==', NULL, 1, 0, 0, (SELECT Id FROM CentrosCusto WHERE Nome = 'CMULHER/DECOM'));

-- 3. Inserindo Gestores Centrais Padrão (sem Centro de Custo e usando o Hash do próprio Ponto)
INSERT INTO Usuarios (Nome, Email, Ponto, Perfil, SenhaHash, Cpf, Ativo, IsTeste, ContadorAcessos, CentroCustoId)
VALUES 
('Greciane Lopes', '', 'P_922891', 'Gestor Central Padrão', 'AQAAAAIAAYagAAAAEJ5GLal7z09SpnGtBg/2QL+lC/chMm7bJhnU3L9ETtBl/W1W0aSiVhZcExDGsq2Ypw==', NULL, 1, 0, 0, NULL),
('Marcelo dos Reis', '', 'P_923223', 'Gestor Central Padrão', 'AQAAAAIAAYagAAAAEMw3olC7unylUomhWStOkulpuENav6l5Pdu75e/tlZCipk3cCyzM3iz8dzavxKtBpg==', NULL, 1, 0, 0, NULL),
('Sabrina Damacena', '', 'P_919827', 'Gestor Central Padrão', 'AQAAAAIAAYagAAAAEKubbaC1W5YSr9UlSq0JO3TCkpL1stkXvtDTVrQnHIyMT3qVzmMIs5A6fPyZDM0Gbg==', NULL, 1, 0, 0, NULL);
