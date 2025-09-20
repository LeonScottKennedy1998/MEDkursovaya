CREATE DATABASE HoneyShop;
GO

USE HoneyShop;
GO

CREATE TABLE Roles (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL
);
GO

CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    NameUser NVARCHAR (100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
	Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordUser NVARCHAR(100) NOT NULL,
    AddressUser NVARCHAR(200) NOT NULL,
	ResetToken NVARCHAR(200),
	ResetTokenExpiry DATETIME2,
    RoleID INT NOT NULL FOREIGN KEY REFERENCES Roles(RoleID) ON DELETE CASCADE
);
GO

CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE Suppliers (
    SupplierID INT PRIMARY KEY IDENTITY(1,1),
    SupplierName NVARCHAR(150) NOT NULL,
    ContactName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(50) NOT NULL,
    AddressS NVARCHAR(200) NOT NULL
);
GO


CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    NameProduct NVARCHAR(100) NOT NULL,
    DescriptionProduct NVARCHAR(500) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    CategoryID INT NOT NULL FOREIGN KEY REFERENCES Categories(CategoryID) ON DELETE CASCADE,
    SupplierID INT NOT NULL FOREIGN KEY REFERENCES Suppliers(SupplierID) ON DELETE CASCADE,
    Stock INT NOT NULL,
    ImageUrl NVARCHAR(255) NOT NULL
);
GO

CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID) ON DELETE CASCADE,
    OrderDate DATETIME2 NOT NULL
);
GO

CREATE TABLE OrderDetails (
    OrderDetailID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderID) ON DELETE CASCADE,
    ProductID INT NOT NULL FOREIGN KEY REFERENCES Products(ProductID) ON DELETE CASCADE,
    Quantity INT NOT NULL,
    TotalPrice DECIMAL(10,2) NOT NULL
);
GO

CREATE TABLE Reviews (
    ReviewID INT IDENTITY(1,1) PRIMARY KEY,
    ProductID INT NOT NULL FOREIGN KEY REFERENCES Products(ProductID) ON DELETE CASCADE,
    UserID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID) ON DELETE CASCADE,
    Rating INT CHECK (Rating BETWEEN 1 AND 5) NOT NULL,
    ReviewText NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL
);
GO

CREATE TABLE AuditLog (
    AuditID INT PRIMARY KEY IDENTITY(1,1),
    TableName NVARCHAR(100) NOT NULL,
    RecordID INT NOT NULL,
    Operation NVARCHAR(10) NOT NULL,
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    ChangedBy NVARCHAR(100) NOT NULL,
    ChangedAt DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE UserSettings (
    SettingID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL FOREIGN KEY REFERENCES Users(UserID) ON DELETE CASCADE,
    Theme NVARCHAR(20) DEFAULT 'light',
    DateFormatUS NVARCHAR(20) DEFAULT 'dd.MM.yyyy',
    NumberFormat NVARCHAR(10) DEFAULT '1 234,56',
    PageSize INT DEFAULT 20
);
GO


INSERT INTO Roles (RoleName)
VALUES 
    ('�����'), 
    ('����������'),
	('��������');
GO

INSERT INTO Categories (CategoryName)
VALUES 
    ('��������� ��'),
    ('��������� ��'),
    ('������� ��'),
    ('̸� � ����������'),
    ('̸� � �������');
GO

INSERT INTO Suppliers (SupplierName, ContactName, Email, Phone, AddressS)
VALUES
('������ ���������� ��', '���� ����������', 'solnechniy@honey.ru', '+7 (900) 123-45-67', '�. �������, ��. ������, �. 15'),

('��� �������� ����', '����� ��������', 'lipahoney@company.ru', '+7 (901) 234-56-78', '�. ������, ��. �����������, �. 42'),

('���������� ��������� ���������� �����', '������� �����������', 'grechka-honey@farm.ru', '+7 (902) 345-67-89', '�. ������, ��. �������, �. 7');
GO
select * from Users

INSERT INTO Users (NameUser, Email, Username, PasswordUser, AddressUser, RoleID)
VALUES
    ('���� ������', 'ivan@mail.com', 'ivan_ivanov', 'ivanbolvan', '��. ������, �. 10', 2),
    ('����� �������', 'maria@mail.com','masha_petrova', 'vashamasha', '��. ��������, �. 25', 2),
    ('������� �������', 'alexey@mail.com', 'alexey_sidorov','lexakartoxa', '��. ���������, �. 30', 2),
	('ϸ�� �������', 'petr@mail.ru', 'petr_sergeev','petyapyatka', '��. ���������, �.16', 2);
GO

INSERT INTO Products (NameProduct, DescriptionProduct, Price, CategoryID, SupplierID, Stock, ImageUrl)
VALUES
    ('��������� ���� 500��', 
     '���� �� � ��������� ������� ��������� �����, ���������� ��������� ������� ���� � ������� ������. ��� ������ ���� � ������� ���������� ������ �������� ��� � ����� ���������� ������ �������. ������� ��� �������� � ��� ����������� ���������.', 
     450.00, 1,1, 50, '/media/catalog/chvetoch.png'),

    ('������� ������ ������� 300��', 
     '��������� �����, ������ �� � �����, ���������� ������ � �������� �������. ��� ������� �������� � �������, ������ �������� ����� ������� ��� ��������� ����� ��������. ����� ������� � ������� ��� ���������� �������� � ������ ������.', 
     320.00, 2,2, 30, '/media/catalog/grechishnyy.png'),

    ('������� ��� 1��', 
     '̸�, ��������� � ������� ��������, �������� ����� ������������ �������� � ��������� ����������. ����������, ����� ���������� �� ���, �� ��������� ������ ��������� ������ � ������� ��������. ��������� �������� ��� ������������ ������� � ���������� ����������.', 
     900.00, 3,3, 20, '/media/catalog/lipa.jpeg'),

    ('�������� ���� 500��', 
     '̸�, ����������� ��������� ���������� ���������, � ��� ��� ������ ������� � ������ � ����������� ������������� � ��� ���������� ����������. ������� ��������������� � ����������, �� �������� ����� ������ � ������� ���������. ������� ��� ������������ ���� �����.', 
     500.00, 4,2, 25, '/media/catalog/propolis.jpeg'),

    ('������� ����� 750��', 
     '����������� ��������� ������� ��� � ��������� ������ ������ ��������� ������ �������� � ��������. ���� �� � ��������� ������� � �����, ���������� ��������� �� ���� ����. ��������� ���������� � ������, �������� � ������� ��������.', 
     600.00, 5,2, 15, '/media/catalog/orehi.jpeg'),

    ('����������� ������� 750��', 
     '���� �� � ����������� ��������� ������� � ��������� ��������� ��� ������ ��������. ������� ���������� ������ ������ � �������� ���������, �� �������� �������� ���������� � ��������� ���������������. ���������� ����� ��� ���, ��� ���� ����������� �������� ��� ��������� ������������.', 
     700.00, 1,1, 10, '/media/catalog/chvetoch1.jpeg'),

    ('����� ���� 500��', 
     '��������� � �������� � ����������� ����� �����, ���� �� �������� ����� �������. ��� ���������� ���� � ������� ���������� ������� � ������ �������� ������� ��� ���������� ����������� � ������ �������� ��� ���. ����� ��������� � ����������� �������.', 
     650.00, 2,1, 18, '/media/catalog/grechishnyy1.jpg'),

    ('������� ��� 500��', 
     '����� ��������� �������� ��� � ����������� ���� ��������� ������ ��� ���� � �������� �����. ���� �� ���������� ��������� � ���������� �������, ����� ��� �������� ����������� � �������� �����, �������� � ��������.', 
     550.00, 3,3, 22, '/media/catalog/lipa1.jpg'),

    ('��������� ظ�� 650��', 
     '˸���� � ���������� �� �� ������ ������ � ��� �������� ��������� ����� ������ ���. ��� ������, ���������� ���� ���������� ���� ����� �������������� ��������. ������� �������� ��� ������������ ������� � ��������� �����������.', 
     750.00, 4,2, 12, '/media/catalog/propolis1.jpg');
GO

INSERT INTO Orders (UserID, OrderDate)
VALUES
    (1, '2024-09-25'), 
    (2, '2024-09-26'),  
    (3, '2024-09-27'), 
    (4, '2024-09-28');
GO

-- ���� �� �������� �� �������
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, TotalPrice)
VALUES
    (1, 1, 2, 900.00),  -- 2 ����� ���������� ���
    (2, 2, 1, 320.00),  -- 1 ����� ���������� ���
    (3, 3, 1, 900.00),  -- 1 �� �������� ���
    (4, 5, 1, 600.00);  -- 1 ����� ��� � �������
GO

SELECT * FROM Users;
GO

SELECT * FROM Products;
GO

SELECT * FROM Categories;
GO

SELECT * FROM Reviews;
GO

CREATE VIEW vw_SalesByProduct AS
SELECT 
    p.NameProduct,
    SUM(od.Quantity) AS TotalQuantity,
    SUM(od.TotalPrice) AS TotalSales
FROM OrderDetails od
JOIN Products p ON od.ProductID = p.ProductID
GROUP BY p.NameProduct;
GO

SELECT * FROM vw_SalesByProduct;
GO


CREATE OR ALTER VIEW vw_UserOrders AS
SELECT 
    u.Username,
    o.OrderID,
    o.OrderDate,
    STRING_AGG(p.NameProduct + ' (' + CAST(od.Quantity AS NVARCHAR(5)) + ' ��.)', ', ') AS ProductsList,
    SUM(od.TotalPrice) AS OrderTotal
FROM Orders o
JOIN Users u ON o.UserID = u.UserID
JOIN OrderDetails od ON o.OrderID = od.OrderID
JOIN Products p ON od.ProductID = p.ProductID
GROUP BY u.Username, o.OrderID, o.OrderDate;
GO


SELECT * FROM vw_UserOrders;
GO

