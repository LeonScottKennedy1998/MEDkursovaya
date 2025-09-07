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
	RoleID INT FOREIGN KEY REFERENCES Roles(RoleID)
);
GO

CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    NameProduct NVARCHAR(100) NOT NULL,
    DescriptionProduct NVARCHAR(500) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    CategoryID INT FOREIGN KEY REFERENCES Categories(CategoryID),
    Stock INT NOT NULL,
	ImageUrl NVARCHAR(255) NOT NULL
);
GO

CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT FOREIGN KEY REFERENCES Users(UserID),
    OrderDate DATETIME2 NOT NULL,
);
GO

CREATE TABLE OrderDetails (
    OrderDetailID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT FOREIGN KEY REFERENCES Orders(OrderID),
    ProductID INT FOREIGN KEY REFERENCES Products(ProductID),
    Quantity INT NOT NULL,
    TotalPrice DECIMAL(10,2) NOT NULL
);
GO

CREATE TABLE Reviews (
    ReviewID INT IDENTITY(1,1) PRIMARY KEY,
    ProductID INT NOT NULL,
    UserID INT NOT NULL,
    Rating INT CHECK (Rating BETWEEN 1 AND 5) NOT NULL,
    ReviewText NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
	UpdatedAt DATETIME2 NULL;
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE CASCADE,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);
GO

CREATE TABLE AuditLog (
    AuditID INT PRIMARY KEY IDENTITY(1,1),
    TableName NVARCHAR(100) NOT NULL,
    RecordID INT NOT NULL,
    Operation NVARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    ChangedBy NVARCHAR(100) NOT NULL,
    ChangedAt DATETIME DEFAULT GETDATE()
);
GO


INSERT INTO Roles (RoleName)
VALUES 
    ('Админ'), 
    ('Покупатель'),
	('Менеджер');
GO

INSERT INTO Categories (CategoryName)
VALUES 
    ('Цветочный мёд'),
    ('Гречишный мёд'),
    ('Липовый мёд'),
    ('Мёд с прополисом'),
    ('Мёд с орехами');
GO

INSERT INTO Users (NameUser, Email, Username, PasswordUser, AddressUser, RoleID)
VALUES
    ('Иван Иванов', 'ivan@mail.com', 'ivan_ivanov', 'ivanbolvan', 'ул. Ленина, д. 10', 2),
    ('Мария Петрова', 'maria@mail.com','masha_petrova', 'vashamasha', 'ул. Гагарина, д. 25', 2),
    ('Алексей Сидоров', 'alexey@mail.com', 'alexey_sidorov','lexakartoxa', 'ул. Советская, д. 30', 2),
	('Пётр Сергеев', 'petr@mail.ru', 'petr_sergeev','petyapyatka', 'ул. Плеханова, д.16', 2);
GO

INSERT INTO Products (NameProduct, DescriptionProduct, Price, CategoryID, Stock, ImageUrl)
VALUES
    ('Солнечная Луга 500гр', 
     'Этот мёд — настоящий подарок цветочных полей, насыщенный ароматами луговых трав и летнего солнца. Его тонкий вкус с легкими цветочными нотами перенесёт вас в самые живописные уголки природы. Идеален для чаепития и как натуральное лакомство.', 
     450.00, 1, 50, '/media/catalog/chvetoch.png'),

    ('Золотой Янтарь Гречихи 300гр', 
     'Настоящий тёмный, густой мёд с ярким, насыщенным вкусом и ароматом гречихи. Его богатая текстура и терпкие, слегка ореховые нотки сделают его фаворитом среди гурманов. Богат железом и идеален для укрепления здоровья в зимний период.', 
     320.00, 2, 30, '/media/catalog/grechishnyy.png'),

    ('Липовый Рой 1кг', 
     'Мёд, собранный с липовых деревьев, славится своим неповторимым ароматом и целебными свойствами. Прозрачный, почти золотистый на вид, он восхищает тонким цветочным вкусом с нотками свежести. Прекрасно подходит для профилактики простуд и укрепления иммунитета.', 
     900.00, 3, 20, '/media/catalog/lipa.jpeg'),

    ('Целебная Сила 500гр', 
     'Мёд, обогащённый целебными свойствами прополиса, — это ваш верный союзник в борьбе с простудными заболеваниями и для укрепления иммунитета. Богатый антиоксидантами и витаминами, он обладает ярким вкусом и тягучей текстурой. Идеален для оздоровления всей семьи.', 
     500.00, 4, 25, '/media/catalog/propolis.jpeg'),

    ('Золотые Орехи 750гр', 
     'Необычайное сочетание нежного мёда и хрустящих орехов создаёт идеальный баланс сладости и текстуры. Этот мёд — настоящая энергия в банке, заряжающая бодростью на весь день. Прекрасно сочетается с сырами, выпечкой и свежими фруктами.', 
     600.00, 5, 15, '/media/catalog/orehi.jpeg'),

    ('Королевский Эликсир 750гр', 
     'Этот мёд с добавлением маточного молочка — настоящее сокровище для вашего здоровья. Обладая необычайно мягким вкусом и кремовой текстурой, он насыщает организм витаминами и полезными микроэлементами. Прекрасный выбор для тех, кто ищет натуральное средство для улучшения самочувствия.', 
     700.00, 1, 10, '/media/catalog/chvetoch1.jpeg'),

    ('Тайны Леса 500гр', 
     'Собранный с деревьев и кустарников диких лесов, этот мёд пропитан духом природы. Его насыщенный вкус с тонкими древесными нотками и густая текстура сделают его прекрасным дополнением к любому завтраку или чаю. Полон минералов и питательных веществ.', 
     650.00, 2, 18, '/media/catalog/grechishnyy1.jpg'),

    ('Ягодный Бум 500гр', 
     'Яркое сочетание сладкого мёда и натуральных ягод превратит каждый ваш день в праздник вкуса. Этот мёд отличается свежестью и фруктовыми нотками, делая его отличным дополнением к утренним кашам, йогуртам и десертам.', 
     550.00, 3, 22, '/media/catalog/lipa1.jpg'),

    ('Акациевый Шёлк 650гр', 
     'Лёгкий и прозрачный мёд из цветов акации — это истинная жемчужина среди сортов мёда. Его мягкий, деликатный вкус понравится даже самым требовательным гурманам. Отлично подходит для диетического питания и улучшения пищеварения.', 
     750.00, 4, 12, '/media/catalog/propolis1.jpg');
GO

INSERT INTO Orders (UserID, OrderDate)
VALUES
    (1, '2024-09-25'), 
    (2, '2024-09-26'),  
    (3, '2024-09-27'), 
    (4, '2024-09-28');
GO

-- ЧТОБ НЕ ПУТАТЬСЯ НА БУДУЩЕЕ
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, TotalPrice)
VALUES
    (1, 1, 2, 900.00),  -- 2 банки цветочного мёда
    (2, 2, 1, 320.00),  -- 1 банка гречишного мёда
    (3, 3, 1, 900.00),  -- 1 кг липового мёда
    (4, 5, 1, 600.00);  -- 1 банка мёда с орехами
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
    STRING_AGG(p.NameProduct + ' (' + CAST(od.Quantity AS NVARCHAR(5)) + ' шт.)', ', ') AS ProductsList,
    SUM(od.TotalPrice) AS OrderTotal
FROM Orders o
JOIN Users u ON o.UserID = u.UserID
JOIN OrderDetails od ON o.OrderID = od.OrderID
JOIN Products p ON od.ProductID = p.ProductID
GROUP BY u.Username, o.OrderID, o.OrderDate;
GO


SELECT * FROM vw_UserOrders;
GO


