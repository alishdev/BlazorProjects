CREATE TABLE IF NOT EXISTS Companies (
    EIN VARCHAR(255) not null,
    CompanyName VARCHAR(255) not null
);

CREATE TABLE IF NOT EXISTS CompanyPlans (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    EIN VARCHAR(255) not null,
    PlanName VARCHAR(255) not null
);
CREATE INDEX idx_companyplans_ein ON CompanyPlans (EIN);