-- =============================================
-- 🔥 ERP DATABASE - SENIOR LEVEL
-- =============================================

CREATE DATABASE ERP_DB_SENIOR;
GO

USE ERP_DB_SENIOR;
GO

-- SNAPSHOT (PRODUCTION)
ALTER DATABASE ERP_DB_SENIOR SET ALLOW_SNAPSHOT_ISOLATION ON;
ALTER DATABASE ERP_DB_SENIOR SET READ_COMMITTED_SNAPSHOT ON;
GO

-- =============================================
-- 🔹 BASE TABLES
-- =============================================

CREATE TABLE Roles (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(50) UNIQUE NOT NULL
);

CREATE TABLE Users (
    Id INT IDENTITY PRIMARY KEY,
    FullName NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255),
    RoleId INT,
    IsDeleted BIT DEFAULT 0,
    RowVersion ROWVERSION,
    CreatedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

CREATE TABLE Customers (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100),
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    IsDeleted BIT DEFAULT 0,
    RowVersion ROWVERSION,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- =============================================
-- 🏬 WAREHOUSE
-- =============================================

CREATE TABLE Warehouses (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100),
    Location NVARCHAR(200),
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- =============================================
-- 📦 PRODUCTS
-- =============================================

CREATE TABLE Products (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(150),
    Price DECIMAL(18,2),
    IsDeleted BIT DEFAULT 0,
    RowVersion ROWVERSION,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- =============================================
-- 📊 INVENTORY (CURRENT STOCK)
-- =============================================

CREATE TABLE Inventory (
    Id INT IDENTITY PRIMARY KEY,
    ProductId INT,
    WarehouseId INT,
    Quantity INT,

    UNIQUE(ProductId, WarehouseId),

    FOREIGN KEY (ProductId) REFERENCES Products(Id),
    FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id)
);

-- =============================================
-- 📜 INVENTORY LEDGER (TRACK ALL MOVEMENTS)
-- =============================================

CREATE TABLE InventoryTransactions (
    Id INT IDENTITY PRIMARY KEY,
    ProductId INT,
    WarehouseId INT,
    QuantityChange INT, -- + nhập / - xuất
    TransactionType NVARCHAR(50), -- IMPORT, EXPORT, SALE
    ReferenceId INT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (ProductId) REFERENCES Products(Id),
    FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id)
);

-- =============================================
-- 🧾 ORDERS
-- =============================================

CREATE TABLE Orders (
    Id INT IDENTITY PRIMARY KEY,
    CustomerId INT,
    Status NVARCHAR(50),
    TotalAmount DECIMAL(18,2),
    CreatedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);

CREATE TABLE OrderDetails (
    Id INT IDENTITY PRIMARY KEY,
    OrderId INT,
    ProductId INT,
    Quantity INT,
    Price DECIMAL(18,2),

    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- =============================================
-- 💰 INVOICE & PAYMENT
-- =============================================

CREATE TABLE Invoices (
    Id INT IDENTITY PRIMARY KEY,
    OrderId INT,
    TotalAmount DECIMAL(18,2),
    Status NVARCHAR(50), -- PAID / UNPAID
    CreatedAt DATETIME2 DEFAULT GETDATE(),

    FOREIGN KEY (OrderId) REFERENCES Orders(Id)
);

CREATE TABLE Payments (
    Id INT IDENTITY PRIMARY KEY,
    InvoiceId INT,
    Amount DECIMAL(18,2),
    PaymentMethod NVARCHAR(50),
    PaidAt DATETIME2,

    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id)
);

-- =============================================
-- 📜 AUDIT LOG
-- =============================================

CREATE TABLE AuditLogs (
    Id INT IDENTITY PRIMARY KEY,
    TableName NVARCHAR(100),
    Action NVARCHAR(50),
    OldData NVARCHAR(MAX),
    NewData NVARCHAR(MAX),
    UserId INT,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- =============================================
-- ⚡ INDEX
-- =============================================

CREATE INDEX IX_Inventory_Product_Warehouse 
ON Inventory(ProductId, WarehouseId);

CREATE INDEX IX_InventoryTransactions_Product 
ON InventoryTransactions(ProductId);

CREATE INDEX IX_Orders_Customer 
ON Orders(CustomerId);

CREATE INDEX IX_Invoices_Order 
ON Invoices(OrderId);

-- =============================================
-- 🔥 TRANSACTION: ORDER PROCESSING
-- =============================================

GO
CREATE PROCEDURE CreateOrderAdvanced
    @CustomerId INT,
    @ProductId INT,
    @WarehouseId INT,
    @Quantity INT
AS
BEGIN
    SET NOCOUNT ON;

    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRANSACTION;

    DECLARE @Stock INT;
    DECLARE @Price DECIMAL(18,2);

    SELECT @Stock = Quantity
    FROM Inventory
    WHERE ProductId = @ProductId AND WarehouseId = @WarehouseId;

    IF (@Stock < @Quantity)
    BEGIN
        ROLLBACK;
        RAISERROR('Out of stock', 16, 1);
        RETURN;
    END

    SELECT @Price = Price FROM Products WHERE Id = @ProductId;

    -- Update Inventory
    UPDATE Inventory
    SET Quantity = Quantity - @Quantity
    WHERE ProductId = @ProductId AND WarehouseId = @WarehouseId;

    -- Insert Ledger
    INSERT INTO InventoryTransactions
    VALUES (@ProductId, @WarehouseId, -@Quantity, 'SALE', NULL, GETDATE());

    -- Create Order
    INSERT INTO Orders(CustomerId, Status, TotalAmount)
    VALUES (@CustomerId, 'Pending', @Quantity * @Price);

    DECLARE @OrderId INT = SCOPE_IDENTITY();

    INSERT INTO OrderDetails(OrderId, ProductId, Quantity, Price)
    VALUES (@OrderId, @ProductId, @Quantity, @Price);

    -- Invoice
    INSERT INTO Invoices(OrderId, TotalAmount, Status)
    VALUES (@OrderId, @Quantity * @Price, 'UNPAID');

    COMMIT;
END;
GO
-- =============================================
-- 🔐 ROLES
-- =============================================
INSERT INTO Roles (Name) VALUES 
('Admin'),
('Staff'),
('Customer');

-- =============================================
-- 👤 USERS
-- =============================================
INSERT INTO Users (FullName, Email, PasswordHash, RoleId)
VALUES 
('Admin System', 'admin@erp.com', 'hashed_pw', 1),
('Nguyen Van Dev', 'dev@erp.com', 'hashed_pw', 2),
('Tran Thi Staff', 'staff@erp.com', 'hashed_pw', 2);

-- =============================================
-- 👥 CUSTOMERS
-- =============================================
INSERT INTO Customers (Name, Phone, Email)
VALUES 
('Nguyen Van A', '0123456789', 'a@gmail.com'),
('Tran Thi B', '0987654321', 'b@gmail.com'),
('Le Van C', '0911222333', 'c@gmail.com');

-- =============================================
-- 🏬 WAREHOUSES
-- =============================================
INSERT INTO Warehouses (Name, Location)
VALUES 
('Kho Hà Nội', 'Hoan Kiem, Ha Noi'),
('Kho Hồ Chí Minh', 'District 1, HCM'),
('Kho Đà Nẵng', 'Hai Chau, Da Nang');

-- =============================================
-- 📦 PRODUCTS
-- =============================================
INSERT INTO Products (Name, Price)
VALUES 
('Laptop Dell XPS 13', 25000000),
('Macbook Pro M2', 35000000),
('Chuột Logitech MX Master', 2000000),
('Bàn phím cơ Keychron', 3000000),
('Màn hình LG 27 inch', 5000000);

-- =============================================
-- 📊 INVENTORY (PHÂN BỔ KHO)
-- =============================================

-- Hà Nội
INSERT INTO Inventory (ProductId, WarehouseId, Quantity)
VALUES 
(1, 1, 10),
(2, 1, 5),
(3, 1, 50),
(4, 1, 30),
(5, 1, 20);

-- HCM
INSERT INTO Inventory (ProductId, WarehouseId, Quantity)
VALUES 
(1, 2, 8),
(2, 2, 6),
(3, 2, 40),
(4, 2, 25),
(5, 2, 15);

-- Đà Nẵng
INSERT INTO Inventory (ProductId, WarehouseId, Quantity)
VALUES 
(1, 3, 5),
(2, 3, 3),
(3, 3, 20),
(4, 3, 15),
(5, 3, 10);

-- =============================================
-- 📜 INVENTORY TRANSACTIONS (LỊCH SỬ NHẬP KHO)
-- =============================================
INSERT INTO InventoryTransactions (ProductId, WarehouseId, QuantityChange, TransactionType, ReferenceId)
VALUES
(1,1,10,'IMPORT',NULL),
(2,1,5,'IMPORT',NULL),
(3,1,50,'IMPORT',NULL),
(1,2,8,'IMPORT',NULL),
(2,2,6,'IMPORT',NULL);

-- =============================================
-- 🧾 ORDERS
-- =============================================
INSERT INTO Orders (CustomerId, Status, TotalAmount)
VALUES 
(1, 'Completed', 27000000),
(2, 'Pending', 5000000);

-- =============================================
-- 📄 ORDER DETAILS
-- =============================================
INSERT INTO OrderDetails (OrderId, ProductId, Quantity, Price)
VALUES 
(1, 1, 1, 25000000),
(1, 3, 1, 2000000),
(2, 5, 1, 5000000);

-- =============================================
-- 💰 INVOICES
-- =============================================
INSERT INTO Invoices (OrderId, TotalAmount, Status)
VALUES 
(1, 27000000, 'PAID'),
(2, 5000000, 'UNPAID');

-- =============================================
-- 💳 PAYMENTS
-- =============================================
INSERT INTO Payments (InvoiceId, Amount, PaymentMethod, PaidAt)
VALUES 
(1, 27000000, 'Bank Transfer', GETDATE());

-- =============================================
-- 📜 AUDIT LOG (DEMO)
-- =============================================
INSERT INTO AuditLogs (TableName, Action, OldData, NewData, UserId)
VALUES 
('Products', 'UPDATE', '{"Price":25000000}', '{"Price":24000000}', 1),
('Orders', 'INSERT', NULL, '{"OrderId":1}', 2);