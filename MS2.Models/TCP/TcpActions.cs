namespace MS2.Models.TCP;

public static class TcpActions
{
    // Authentication
    public const string LOGIN = "LOGIN";
    public const string LOGOUT = "LOGOUT";

    // Products
    public const string GET_PRODUCTS = "GET_PRODUCTS";
    public const string SEARCH_PRODUCTS = "SEARCH_PRODUCTS";
    public const string GET_PRODUCT_BY_BARCODE = "GET_PRODUCT_BY_BARCODE";
    public const string UPDATE_PRODUCT_PRICE = "UPDATE_PRODUCT_PRICE";
    public const string UPDATE_PRODUCT_STOCK = "UPDATE_PRODUCT_STOCK";

    // Orders
    public const string CREATE_ORDER = "CREATE_ORDER";
    public const string GET_ORDERS = "GET_ORDERS";
    public const string GET_ORDER_DETAILS = "GET_ORDER_DETAILS";

    // Reports
    public const string GET_SALES_REPORT = "GET_SALES_REPORT";
    public const string GET_INVENTORY = "GET_INVENTORY";
    public const string GET_LOW_STOCK_PRODUCTS = "GET_LOW_STOCK_PRODUCTS";

    // Users/Employees
    public const string GET_EMPLOYEES = "GET_EMPLOYEES";
    public const string GET_USERS_BY_ROLE = "GET_USERS_BY_ROLE";

    // Categories
    public const string GET_CATEGORIES = "GET_CATEGORIES";
}