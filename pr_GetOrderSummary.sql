CREATE PROCEDURE pr_GetOrderSummary
    @StartDate DATETIME,
    @EndDate DATETIME,
    @EmployeeID INT = NULL,
    @CustomerID NVARCHAR(5) = NULL
AS
BEGIN
    SELECT
        CONCAT(e.TitleOfCourtesy, ' ', e.FirstName, ' ', e.LastName) AS EmployeeFullName,
        s.CompanyName AS ShipperCompanyName,
        c.CompanyName AS CustomerCompanyName,
        COUNT(DISTINCT o.OrderID) AS NumberOfOrders,
        CONVERT(DATE, o.OrderDate) AS [Date],
        SUM(o.Freight) AS TotalFreightCost,
        COUNT(DISTINCT od.ProductID) AS NumberOfDifferentProducts,
        SUM(od.Quantity * od.UnitPrice) AS TotalOrderValue
    FROM
        Orders o
        JOIN Employees e ON o.EmployeeID = e.EmployeeID
        JOIN Shippers s ON o.ShipVia = s.ShipperID
        JOIN Customers c ON o.CustomerID = c.CustomerID
        JOIN [Order Details] od ON o.OrderID = od.OrderID
    WHERE
        o.OrderDate >= @StartDate
        AND o.OrderDate <= @EndDate
        AND (@EmployeeID IS NULL OR o.EmployeeID = @EmployeeID)
        AND (@CustomerID IS NULL OR o.CustomerID = @CustomerID)
    GROUP BY
        CONVERT(DATE, o.OrderDate),
        e.TitleOfCourtesy,
        e.FirstName,
        e.LastName,
        s.CompanyName,
        c.CompanyName
    ORDER BY
        [Date] ASC,
        EmployeeFullName ASC,
        ShipperCompanyName ASC,
        CustomerCompanyName ASC
END
