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