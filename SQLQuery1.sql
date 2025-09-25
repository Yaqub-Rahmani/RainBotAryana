IF NOT EXISTS (SELECT 1 FROM OperationClaims WHERE Name = 'tenant.read')
    INSERT INTO OperationClaims (Name) VALUES ('tenant.read');

IF NOT EXISTS (SELECT 1 FROM OperationClaims WHERE Name = 'tenant.write')
    INSERT INTO OperationClaims (Name) VALUES ('tenant.write');
