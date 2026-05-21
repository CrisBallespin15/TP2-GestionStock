# TP2 - Sistema de Gestión de Stock y Ventas
**Programación III — UTN FRT**  
Profesor: Ing. Daniel Moris

---

## Descripción

Sistema de consola desarrollado en C# que permite gestionar el stock y las ventas de una cadena de electrodomésticos con múltiples sucursales, conectado a una base de datos MySQL.

---

## Conceptos de POO aplicados

- **Abstracción** — clase abstracta `Producto` que no puede instanciarse directamente
- **Herencia** — `Televisor`, `Heladera` y `Lavarropas` heredan de `Producto`
- **Polimorfismo** — cada subclase implementa `PrecioFinal()` con su propio cálculo de IVA
- **Composición** — `Sucursal` agrupa productos sin heredar de ellos

---

## Estructura de la base de datos

```
Sucursal
   └── Producto  (tabla base con TipoProducto)
         ├── Televisor    (IdProducto FK + Pulgadas, TipoPantalla)
         ├── Heladera     (IdProducto FK + CapacidadLitros, Tipo)
         └── Lavarropas   (IdProducto FK + CargaKg, Tipo)

Venta
   └── DetalleVenta  (IdVenta FK + IdProducto FK + Cantidad + PrecioUnitario)
```

---

## Cálculo de precios

| Producto    | IVA aplicado                          |
|-------------|---------------------------------------|
| Televisor   | 21%                                   |
| Heladera    | 10,5% (línea blanca)                  |
| Lavarropas  | 20% si es Automático / 15% si es Semi |

---

## Requisitos

- [.NET 8 o superior](https://dotnet.microsoft.com/download)
- [MySQL 8 o superior](https://dev.mysql.com/downloads/mysql/)
- [MySQL Workbench](https://dev.mysql.com/downloads/workbench/) (recomendado)

---

## Instalación y uso

**1. Clonar el repositorio**
```bash
git clone https://github.com/CrisBallespin15/TP2-GestionStock.git
cd TP2-GestionStock
```

**2. Crear la base de datos**

Abrir MySQL Workbench, abrir el archivo `database/ElectrodomesticosDB.sql` y ejecutarlo completo (`Ctrl+Shift+Enter`).

**3. Configurar la contraseña**

En `Program.cs`, clase `DB`, cambiar `Pwd=admin` por la contraseña de tu MySQL:
```csharp
"Server=localhost;Database=ElectrodomesticosDB;Uid=root;Pwd=TU_CONTRASEÑA;CharSet=utf8;"
```

**4. Instalar dependencia**
```bash
dotnet add package MySql.Data
```

**5. Correr el programa**
```bash
dotnet run
```

---

## Funcionalidades

- Selección de sucursal al iniciar
- **Agregar** productos (Televisor, Heladera, Lavarropas)
- **Listar** productos con precio final calculado automáticamente
- **Modificar** precio y stock de un producto
- **Eliminar** producto (CASCADE elimina también el registro hijo)
- **Registrar ventas** con transacción — si algo falla se revierte todo
- **Ver historial** de ventas por sucursal

---

## Buenas prácticas aplicadas

- Parámetros SQL (`@param`) para evitar SQL Injection
- `using` en todas las conexiones para liberar recursos automáticamente
- Transacciones en ventas para garantizar atomicidad
- Conexión centralizada en la clase `DB`