CREATE TABLE ReportingData (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReportingEntityName VARCHAR(255) NOT NULL,
    ReportingEntityType VARCHAR(255) NOT NULL
);

CREATE TABLE ReportingStructure (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReportingDataId INT NOT NULL,
    FOREIGN KEY (ReportingDataId) REFERENCES ReportingData(Id)
);

CREATE TABLE ReportingPlan (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReportingStructureId INT NOT NULL,
    PlanName VARCHAR(255) NOT NULL,
    PlanIdType VARCHAR(255) NOT NULL,
    PlanId VARCHAR(255) NOT NULL,
    PlanMarketType VARCHAR(255) NOT NULL,
    FOREIGN KEY (ReportingStructureId) REFERENCES ReportingStructure(Id)
);

CREATE TABLE InNetworkFile (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReportingStructureId INT NOT NULL,
    Description VARCHAR(255) NOT NULL,
    Location VARCHAR(8096) NOT NULL,
    FOREIGN KEY (ReportingStructureId) REFERENCES ReportingStructure(Id)
);

CREATE TABLE AllowedAmountFile (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReportingStructureId INT NOT NULL,
    Description VARCHAR(255) NOT NULL,
    Location VARCHAR(8096) NOT NULL,
    FOREIGN KEY (ReportingStructureId) REFERENCES ReportingStructure(Id)
);
