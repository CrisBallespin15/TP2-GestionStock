using MySql.Data.MySqlClient;

namespace GestionStockVentasDB
{
    public static class DB
    {
        const string Conn =
            "Server=localhost;Database=ElectrodomesticosDB;Uid=root;Pwd=admin;CharSet=utf8;";
        public static MySqlConnection Conectar() => new MySqlConnection(Conn);
    }

    public abstract class Producto
    {
        public int    IdProducto     { get; set; }
        public int    Codigo         { get; set; }
        public string Nombre         { get; set; } = "";
        public double Precio         { get; set; }
        public int    Stock          { get; set; }
        public int    IdSucursal     { get; set; }
        public string NombreSucursal { get; set; } = "";

        public abstract double PrecioFinal();

        public virtual void Mostrar()
        {
            Console.WriteLine($"  [{GetType().Name.ToUpper()}]");
            Console.WriteLine($"  ID: {IdProducto} | Código: {Codigo} | {Nombre}");
            Console.WriteLine($"  Base: ${Precio:N2} | Final: ${PrecioFinal():N2} | Stock: {Stock} | Sucursal: {NombreSucursal}");
        }
    }

    public class Televisor : Producto
    {
        public int    Pulgadas     { get; set; }
        public string TipoPantalla { get; set; } = "";

        public Televisor() { }
        public Televisor(int cod, string nom, double precio, int stock, int suc, int pulgadas, string pantalla)
        { Codigo=cod; Nombre=nom; Precio=precio; Stock=stock; IdSucursal=suc; Pulgadas=pulgadas; TipoPantalla=pantalla; }

        public override double PrecioFinal() => Precio * 1.21;
        public override void Mostrar()
        {
            base.Mostrar();
            Console.WriteLine($"  Pulgadas: {Pulgadas}\" | Pantalla: {TipoPantalla}");
        }
    }

    public class Heladera : Producto
    {
        public int    CapacidadLitros { get; set; }
        public string Tipo            { get; set; } = "";

        public Heladera() { }
        public Heladera(int cod, string nom, double precio, int stock, int suc, int litros, string tipo)
        { Codigo=cod; Nombre=nom; Precio=precio; Stock=stock; IdSucursal=suc; CapacidadLitros=litros; Tipo=tipo; }

        public override double PrecioFinal() => Precio * 1.105;
        public override void Mostrar()
        {
            base.Mostrar();
            Console.WriteLine($"  Capacidad: {CapacidadLitros}L | Tipo: {Tipo}");
        }
    }

    public class Lavarropas : Producto
    {
        public double CargaKg { get; set; }
        public string Tipo    { get; set; } = "";

        public Lavarropas() { }
        public Lavarropas(int cod, string nom, double precio, int stock, int suc, double kg, string tipo)
        { Codigo=cod; Nombre=nom; Precio=precio; Stock=stock; IdSucursal=suc; CargaKg=kg; Tipo=tipo; }

        public override double PrecioFinal() => Precio * (Tipo.ToLower().Contains("auto") ? 1.20 : 1.15);
        public override void Mostrar()
        {
            base.Mostrar();
            Console.WriteLine($"  Carga: {CargaKg}kg | Tipo: {Tipo}");
        }
    }

    public class Sucursal
    {
        public int    IdSucursal { get; set; }
        public string Nombre     { get; set; } = "";
        public Sucursal(int id, string nombre) { IdSucursal=id; Nombre=nombre; }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            while (true)
            {
                var sucursales = ObtenerSucursales();
                Console.WriteLine("\n══ ELECTROHOGAR ══");
                for (int i = 0; i < sucursales.Count; i++)
                    Console.WriteLine($"  {i+1} - {sucursales[i].Nombre}");
                Console.WriteLine($"  {sucursales.Count+1} - Salir");
                Console.Write("  > ");

                if (!int.TryParse(Console.ReadLine(), out int op)) continue;
                if (op == sucursales.Count+1) break;
                if (op < 1 || op > sucursales.Count) continue;
                MenuSucursal(sucursales[op-1]);
            }
            Console.WriteLine("\n  Hasta luego.");
        }

        static void MenuSucursal(Sucursal s)
        {
            while (true)
            {
                Console.WriteLine($"\n── {s.Nombre} ──");
                Console.WriteLine("  1-Agregar  2-Listar  3-Modificar  4-Eliminar  5-Venta  6-Historial  7-Volver");
                Console.Write("  > ");
                switch (Console.ReadLine())
                {
                    case "1": Agregar(s);    break;
                    case "2": Listar(s);     break;
                    case "3": Modificar(s);  break;
                    case "4": Eliminar(s);   break;
                    case "5": Venta(s);      break;
                    case "6": Historial(s);  break;
                    case "7": return;
                }
            }
        }

        static List<Sucursal> ObtenerSucursales()
        {
            var lista = new List<Sucursal>();
            using var conn = DB.Conectar(); conn.Open();
            using var cmd = new MySqlCommand("SELECT IdSucursal, Nombre FROM Sucursal ORDER BY IdSucursal", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(new Sucursal(r.GetInt32(0), r.GetString(1)));
            return lista;
        }

        static List<Producto> ObtenerProductos(int idSuc)
        {
            var lista = new List<Producto>();
            using var conn = DB.Conectar(); conn.Open();

            string sql = @"
                SELECT p.IdProducto, p.Codigo, p.Nombre, p.Precio, p.Stock, p.TipoProducto, s.Nombre AS Suc,
                       t.Pulgadas, t.TipoPantalla, NULL AS CapLitros, NULL AS TipoH, NULL AS CargaKg, NULL AS TipoL
                FROM Producto p JOIN Sucursal s ON s.IdSucursal=p.IdSucursal
                LEFT JOIN Televisor t ON t.IdProducto=p.IdProducto
                WHERE p.IdSucursal=@id AND p.TipoProducto='Televisor'
                UNION ALL
                SELECT p.IdProducto, p.Codigo, p.Nombre, p.Precio, p.Stock, p.TipoProducto, s.Nombre,
                       NULL, NULL, h.CapacidadLitros, h.Tipo, NULL, NULL
                FROM Producto p JOIN Sucursal s ON s.IdSucursal=p.IdSucursal
                LEFT JOIN Heladera h ON h.IdProducto=p.IdProducto
                WHERE p.IdSucursal=@id AND p.TipoProducto='Heladera'
                UNION ALL
                SELECT p.IdProducto, p.Codigo, p.Nombre, p.Precio, p.Stock, p.TipoProducto, s.Nombre,
                       NULL, NULL, NULL, NULL, l.CargaKg, l.Tipo
                FROM Producto p JOIN Sucursal s ON s.IdSucursal=p.IdSucursal
                LEFT JOIN Lavarropas l ON l.IdProducto=p.IdProducto
                WHERE p.IdSucursal=@id AND p.TipoProducto='Lavarropas'";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", idSuc);
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                Producto p = r.GetString("TipoProducto") switch
                {
                    "Televisor"  => new Televisor  { Pulgadas=r.IsDBNull(7)?0:r.GetInt32(7), TipoPantalla=r.IsDBNull(8)?"":r.GetString(8) },
                    "Heladera"   => new Heladera   { CapacidadLitros=r.IsDBNull(9)?0:r.GetInt32(9), Tipo=r.IsDBNull(10)?"":r.GetString(10) },
                    _            => new Lavarropas { CargaKg=r.IsDBNull(11)?0:r.GetDouble(11), Tipo=r.IsDBNull(12)?"":r.GetString(12) }
                };
                p.IdProducto=r.GetInt32(0); p.Codigo=r.GetInt32(1); p.Nombre=r.GetString(2);
                p.Precio=r.GetDouble(3); p.Stock=r.GetInt32(4); p.IdSucursal=idSuc;
                p.NombreSucursal=r.GetString("Suc");
                lista.Add(p);
            }
            return lista;
        }

        static void Listar(Sucursal s)
        {
            var prods = ObtenerProductos(s.IdSucursal);
            if (prods.Count == 0) { Console.WriteLine("  No hay productos."); return; }
            Console.WriteLine($"\n══ Productos en {s.Nombre} ══");
            foreach (var p in prods) { Console.WriteLine("  ──"); p.Mostrar(); }
        }

        static void Agregar(Sucursal s)
        {
            Console.WriteLine("  1-Televisor  2-Heladera  3-Lavarropas");
            Console.Write("  > "); string? tipo = Console.ReadLine();

            Console.Write("  Código: ");      int    cod    = LeerInt();
            Console.Write("  Nombre: ");      string nom    = Console.ReadLine() ?? "";
            Console.Write("  Precio base: "); double precio = LeerDouble();
            Console.Write("  Stock: ");       int    stock  = LeerInt();

            Producto? nuevo = tipo switch
            {
                "1" => PedirTelevisor(cod, nom, precio, stock, s.IdSucursal),
                "2" => PedirHeladera (cod, nom, precio, stock, s.IdSucursal),
                "3" => PedirLavarropas(cod, nom, precio, stock, s.IdSucursal),
                _   => null
            };
            if (nuevo == null) { Console.WriteLine("  Tipo inválido."); return; }
            GuardarProducto(nuevo);
        }

        static Televisor PedirTelevisor(int cod, string nom, double precio, int stock, int suc)
        {
            Console.Write("  Pulgadas: ");     int    p = LeerInt();
            Console.Write("  Tipo pantalla: "); string t = Console.ReadLine() ?? "LED";
            return new Televisor(cod, nom, precio, stock, suc, p, t);
        }

        static Heladera PedirHeladera(int cod, string nom, double precio, int stock, int suc)
        {
            Console.Write("  Capacidad (L): ");        int    l = LeerInt();
            Console.Write("  Tipo (No Frost/Freezer): "); string t = Console.ReadLine() ?? "No Frost";
            return new Heladera(cod, nom, precio, stock, suc, l, t);
        }

        static Lavarropas PedirLavarropas(int cod, string nom, double precio, int stock, int suc)
        {
            Console.Write("  Carga (kg): ");           double k = LeerDouble();
            Console.Write("  Tipo (Automático/Semi): "); string t = Console.ReadLine() ?? "Automático";
            return new Lavarropas(cod, nom, precio, stock, suc, k, t);
        }

        static void GuardarProducto(Producto prod)
        {
            using var conn = DB.Conectar(); conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                using var cmd = new MySqlCommand(@"
                    INSERT INTO Producto (Codigo,Nombre,Precio,Stock,TipoProducto,IdSucursal)
                    VALUES (@c,@n,@p,@s,@t,@i); SELECT LAST_INSERT_ID();", conn, tx);
                cmd.Parameters.AddWithValue("@c", prod.Codigo);
                cmd.Parameters.AddWithValue("@n", prod.Nombre);
                cmd.Parameters.AddWithValue("@p", prod.Precio);
                cmd.Parameters.AddWithValue("@s", prod.Stock);
                cmd.Parameters.AddWithValue("@t", prod.GetType().Name);
                cmd.Parameters.AddWithValue("@i", prod.IdSucursal);
                long id = Convert.ToInt64(cmd.ExecuteScalar());

                if (prod is Televisor tv)
                    Exec(conn, tx, "INSERT INTO Televisor VALUES (@id,@a,@b)",
                         ("@id",id),("@a",tv.Pulgadas),("@b",tv.TipoPantalla));
                else if (prod is Heladera h)
                    Exec(conn, tx, "INSERT INTO Heladera VALUES (@id,@a,@b)",
                         ("@id",id),("@a",h.CapacidadLitros),("@b",h.Tipo));
                else if (prod is Lavarropas l)
                    Exec(conn, tx, "INSERT INTO Lavarropas VALUES (@id,@a,@b)",
                         ("@id",id),("@a",l.CargaKg),("@b",l.Tipo));

                tx.Commit();
                Console.WriteLine($"  ✔ '{prod.Nombre}' guardado.");
            }
            catch (Exception ex) { tx.Rollback(); Console.WriteLine($"  ✘ {ex.Message}"); }
        }

        static void Modificar(Sucursal s)
        {
            Listar(s);
            Console.Write("  ID a modificar: "); int id = LeerInt();
            Console.Write("  Nuevo precio: ");   double p = LeerDouble();
            Console.Write("  Nuevo stock: ");    int st = LeerInt();
            using var conn = DB.Conectar(); conn.Open();
            int f = Exec(conn, null, "UPDATE Producto SET Precio=@p, Stock=@s WHERE IdProducto=@id",
                         ("@p",p),("@s",st),("@id",id));
            Console.WriteLine(f > 0 ? "  ✔ Modificado." : "  ✘ No encontrado.");
        }

        static void Eliminar(Sucursal s)
        {
            Listar(s);
            Console.Write("  ID a eliminar: "); int id = LeerInt();
            Console.Write("  ¿Confirmar? (s/n): ");
            if (Console.ReadLine()?.ToLower() != "s") { Console.WriteLine("  Cancelado."); return; }
            using var conn = DB.Conectar(); conn.Open();
            int f = Exec(conn, null, "DELETE FROM Producto WHERE IdProducto=@id", ("@id", id));
            Console.WriteLine(f > 0 ? "  ✔ Eliminado." : "  ✘ No encontrado.");
        }

        static void Venta(Sucursal s)
        {
            Listar(s);
            var items = new List<(int cod, int cant)>();
            do {
                Console.Write("  Código: ");   int cod  = LeerInt();
                Console.Write("  Cantidad: "); int cant = LeerInt();
                items.Add((cod, cant));
                Console.Write("  ¿Otro producto? (s/n): ");
            } while (Console.ReadLine()?.ToLower() == "s");

            using var conn = DB.Conectar(); conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                using var cmdV = new MySqlCommand(
                    "INSERT INTO Venta (Fecha,IdSucursal) VALUES (NOW(),@i); SELECT LAST_INSERT_ID();", conn, tx);
                cmdV.Parameters.AddWithValue("@i", s.IdSucursal);
                long idVenta = Convert.ToInt64(cmdV.ExecuteScalar());

                double total = 0;
                var prods = ObtenerProductos(s.IdSucursal);

                foreach (var (cod, cant) in items)
                {
                    var p = prods.Find(x => x.Codigo == cod)
                            ?? throw new Exception($"Código {cod} no encontrado.");
                    if (p.Stock < cant)
                        throw new Exception($"Stock insuficiente para '{p.Nombre}' (disponible: {p.Stock}).");

                    double pu = p.PrecioFinal();
                    total += pu * cant;

                    Exec(conn, tx, "INSERT INTO DetalleVenta (IdVenta,IdProducto,Cantidad,PrecioUnitario) VALUES (@v,@p,@c,@u)",
                         ("@v",idVenta),("@p",p.IdProducto),("@c",cant),("@u",pu));
                    Exec(conn, tx, "UPDATE Producto SET Stock=Stock-@c WHERE IdProducto=@p",
                         ("@c",cant),("@p",p.IdProducto));

                    Console.WriteLine($"  · {p.Nombre} x{cant} = ${pu*cant:N2}  (stock restante: {p.Stock-cant})");
                }

                tx.Commit();
                Console.WriteLine($"  ✔ Venta #{idVenta} registrada. Total: ${total:N2}");
            }
            catch (Exception ex) { tx.Rollback(); Console.WriteLine($"  ✘ Revertido: {ex.Message}"); }
        }

        static void Historial(Sucursal s)
        {
            using var conn = DB.Conectar(); conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT v.IdVenta, v.Fecha, p.Nombre, dv.Cantidad,
                       dv.Cantidad * dv.PrecioUnitario AS Sub
                FROM Venta v
                JOIN DetalleVenta dv ON dv.IdVenta=v.IdVenta
                JOIN Producto p      ON p.IdProducto=dv.IdProducto
                WHERE v.IdSucursal=@id ORDER BY v.IdVenta DESC", conn);
            cmd.Parameters.AddWithValue("@id", s.IdSucursal);
            using var r = cmd.ExecuteReader();

            int ventaAnt = -1; double total = 0; bool hay = false;
            Console.WriteLine("\n══ Historial de Ventas ══");
            while (r.Read())
            {
                hay = true;
                int idV = r.GetInt32(0);
                if (idV != ventaAnt)
                {
                    if (ventaAnt != -1) Console.WriteLine($"  Subtotal: ${total:N2}\n  ──");
                    total=0; ventaAnt=idV;
                    Console.WriteLine($"\n  Venta #{idV}  {r.GetDateTime(1):dd/MM/yyyy HH:mm}");
                }
                double sub = r.GetDouble("Sub");
                total += sub;
                Console.WriteLine($"    · {r.GetString(2)} x{r.GetInt32(3)} = ${sub:N2}");
            }
            if (hay) Console.WriteLine($"  Subtotal: ${total:N2}");
            else     Console.WriteLine("  No hay ventas registradas.");
        }

        // Helper: ejecuta un comando con parámetros, devuelve filas afectadas
        static int Exec(MySqlConnection conn, MySqlTransaction? tx, string sql,
                        params (string key, object val)[] pars)
        {
            using var cmd = tx != null
                ? new MySqlCommand(sql, conn, tx)
                : new MySqlCommand(sql, conn);
            foreach (var (k, v) in pars) cmd.Parameters.AddWithValue(k, v);
            return cmd.ExecuteNonQuery();
        }

        static int    LeerInt()    { int r;    while (!int.TryParse   (Console.ReadLine(), out r)) Console.Write("  Número entero: "); return r; }
        static double LeerDouble() { double r; while (!double.TryParse(Console.ReadLine(), out r)) Console.Write("  Número: ");        return r; }
    }
}