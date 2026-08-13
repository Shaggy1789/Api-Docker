# Despliegue en producción (Render + Netlify)

Arquitectura: frontend React en Netlify, 3 microservicios .NET en Render (Catalog, Basket, Orders), con PostgreSQL (Neon), Redis y MongoDB Atlas.

## 1. Desplegar el backend en Render

Render admite **Blueprints** (`render.yaml`), que crea los 3 servicios de una vez.

1. En [render.com](https://render.com) → **New** → **Blueprint**.
2. Conecta el repo `Shaggy1789/Api-Docker`.
3. Render lee `render.yaml` y crea `catalog-api-techstore`, `basket-api-techstore`, `orders-api-techstore`.
4. Configura los secretos en **cada servicio → Environment** (los marcados como `sync: false`):

   | Servicio | Variable | Valor |
   |---|---|---|
   | catalog-api-techstore | `ConnectionStrings__CatalogDb` | Cadena PostgreSQL (Neon) |
   | basket-api-techstore | `ConnectionStrings__Database` | Cadena PostgreSQL (Neon) |
   | basket-api-techstore | `ConnectionStrings__Redis` | URL de Redis (p.ej. Upstash) |
   | orders-api-techstore | `MongoDb__ConnectionString` | Cadena MongoDB Atlas |
   | orders-api-techstore | `BasketApi__BaseUrl` | `https://basket-api-techstore.onrender.com` |

5. Deploy. Cada servicio queda en `https://<nombre>.onrender.com`.

## 2. Apuntar el frontend al backend

Los servicios ya fueron modificados para leer la URL desde variables de entorno (Vite):

- `src/api/cartService.js` → `CATALOG_BASE`, `BASKET_BASE`, `ORDERS_BASE`
- `src/api/ordersService.js`, `userService.js`, `productsService.js` → usan esas bases

En **Netlify → Settings → Environment Variables** agrega:

```env
VITE_CATALOG_API=https://catalog-api-techstore.onrender.com
VITE_BASKET_API=https://basket-api-techstore.onrender.com
VITE_ORDERS_API=https://orders-api-techstore.onrender.com
```

> Si una variable no está definida, el frontend cae a rutas relativas (mock/Netlify Functions), lo que sigue siendo útil como respaldo.

Redespliega el frontend en Netlify.

## 3. Verificación

```bash
# Health checks
curl https://catalog-api-techstore.onrender.com/products?pageNumber=1&pageSize=5
curl https://basket-api-techstore.onrender.com/basket/eric
curl https://orders-api-techstore.onrender.com/api/users
```

Luego en `https://appnetifly.netlify.app`: Pedidos → selector de usuarios → pedidos; carrito → checkout → pedido nuevo.

## Nota de seguridad

`docker-compose.override.yml` contiene credenciales reales commiteadas. **Rótalas** (Neon y MongoDB Atlas) y reemplázalas por variables de entorno.
