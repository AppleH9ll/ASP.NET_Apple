-- =========================================================
--  Магазин воздуха — база данных для веб-приложения
--  СУБД: PostgreSQL 14+
--  Запуск:  psql -U postgres -f air_shop.sql
-- =========================================================

-- ---------------------------------------------------------
--  0. Создание базы (при необходимости)
-- ---------------------------------------------------------
-- CREATE DATABASE air_shop WITH ENCODING 'UTF8';
-- \c air_shop

-- =========================================================
--  1. РОЛИ
-- =========================================================
CREATE TABLE roles (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(50)  NOT NULL UNIQUE,
    description TEXT
);

-- =========================================================
--  2. ПОЛЬЗОВАТЕЛИ
--     Пароли хранятся только как хеш (bcrypt / argon2).
-- =========================================================
CREATE TABLE users (
    id            SERIAL PRIMARY KEY,
    login         VARCHAR(50)  NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    email         VARCHAR(255) UNIQUE,
    role_id       INTEGER      NOT NULL
        REFERENCES roles(id) ON DELETE RESTRICT,
    is_active     BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at    TIMESTAMP    NOT NULL DEFAULT NOW()
);

-- =========================================================
--  3. КАТЕГОРИИ (с поддержкой вложенности)
-- =========================================================
CREATE TABLE categories (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    parent_id   INTEGER
        REFERENCES categories(id) ON DELETE SET NULL
);

-- =========================================================
--  4. ТОВАРЫ / УСЛУГИ (воздух)
--     volume_liters и origin_region — специфика темы
-- =========================================================
CREATE TABLE products (
    id            SERIAL PRIMARY KEY,
    name          VARCHAR(255)  NOT NULL,
    description   TEXT,
    price         NUMERIC(10,2) NOT NULL CHECK (price >= 0),
    stock         INTEGER       NOT NULL DEFAULT 0 CHECK (stock >= 0),
    category_id   INTEGER
        REFERENCES categories(id) ON DELETE SET NULL,
    volume_liters NUMERIC(6,2)  CHECK (volume_liters > 0),
    origin_region VARCHAR(100),
    is_active     BOOLEAN       NOT NULL DEFAULT TRUE,
    created_at    TIMESTAMP     NOT NULL DEFAULT NOW()
);

-- =========================================================
--  5. КОРЗИНА (одна на пользователя)
-- =========================================================
CREATE TABLE carts (
    id         SERIAL PRIMARY KEY,
    user_id    INTEGER   NOT NULL UNIQUE
        REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- =========================================================
--  6. ПОЗИЦИИ КОРЗИНЫ
-- =========================================================
CREATE TABLE cart_items (
    id         SERIAL PRIMARY KEY,
    cart_id    INTEGER NOT NULL REFERENCES carts(id)    ON DELETE CASCADE,
    product_id INTEGER NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    quantity   INTEGER NOT NULL CHECK (quantity > 0),
    UNIQUE (cart_id, product_id)
);

-- =========================================================
--  7. ЗАКАЗЫ
-- =========================================================
CREATE TABLE orders (
    id           SERIAL PRIMARY KEY,
    user_id      INTEGER       NOT NULL
        REFERENCES users(id) ON DELETE RESTRICT,
    status       VARCHAR(30)   NOT NULL DEFAULT 'created',
    total_amount NUMERIC(12,2) NOT NULL DEFAULT 0 CHECK (total_amount >= 0),
    created_at   TIMESTAMP     NOT NULL DEFAULT NOW(),
    CHECK (status IN ('created','paid','shipped','completed','cancelled'))
);

-- =========================================================
--  8. ПОЗИЦИИ ЗАКАЗА
--     price — снимок цены на момент оформления.
-- =========================================================
CREATE TABLE order_items (
    id         SERIAL PRIMARY KEY,
    order_id   INTEGER       NOT NULL REFERENCES orders(id)   ON DELETE CASCADE,
    product_id INTEGER       NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
    quantity   INTEGER       NOT NULL CHECK (quantity > 0),
    price      NUMERIC(10,2) NOT NULL CHECK (price >= 0),
    UNIQUE (order_id, product_id)
);

-- =========================================================
--  9. ИНДЕКСЫ
-- =========================================================
CREATE INDEX idx_products_category ON products(category_id);
CREATE INDEX idx_cart_items_cart   ON cart_items(cart_id);
CREATE INDEX idx_order_items_order ON order_items(order_id);
CREATE INDEX idx_orders_user       ON orders(user_id);
CREATE INDEX idx_orders_status     ON orders(status);

-- =========================================================
-- 10. ДАННЫЕ: роли
-- =========================================================
INSERT INTO roles (name, description) VALUES
    ('admin',    'Администратор магазина воздуха'),
    ('customer', 'Покупатель'),
    ('manager',  'Менеджер по работе с заказами');

-- =========================================================
-- 11. ДАННЫЕ: пользователи
--     Хеши — примеры bcrypt от пароля "password123".
-- =========================================================
INSERT INTO users (login, password_hash, email, role_id) VALUES
    ('admin',   '$2b$12$KIXQh5x8z3Q2J9wY0uV6QOh1p8x3YQ0m4qW2', 'admin@airshop.ru', 1),
    ('ivanov',  '$2b$12$8sHqX7dK2mP9xQ1p8x3YQ0m4qW2uV6QOh1p', 'ivanov@mail.ru',   2),
    ('petrova', '$2b$12$rB3x9YqW7kP2J9wY0uV6QOh1p8x3YQ0m4qW', 'petrova@mail.ru',  3),
    ('sidorov', '$2b$12$1aB2cD3eF4gH5iJ6kL7mN8oP9qR0sT1uV2wX', 'sidorov@mail.ru',  2);

-- =========================================================
-- 12. ДАННЫЕ: корневые категории
-- =========================================================
INSERT INTO categories (name, description) VALUES
    ('Горный воздух',       'Воздух высокогорья, чистейший'),
    ('Морской воздух',      'Воздух побережья, с ароматом соли'),
    ('Лесной воздух',       'Воздух хвойных и лиственных лесов'),
    ('Экзотический воздух', 'Воздух пустынь, тропиков и прочих локаций');

-- =========================================================
-- 13. ДАННЫЕ: подкатегории
-- =========================================================
INSERT INTO categories (name, description, parent_id) VALUES
    ('Альпийский',       'Воздух швейцарских Альп',              1),
    ('Кавказский',       'Воздух Кавказских гор',                1),
    ('Байкальский',      'Воздух озера Байкал',                  3),
    ('Средиземноморский','Воздух побережья Средиземного моря',   2);

-- =========================================================
-- 14. ДАННЫЕ: товары (воздух)
-- =========================================================
INSERT INTO products
    (name, description, price, stock, category_id, volume_liters, origin_region)
VALUES
    ('Воздух Альп, 1 л',
     'Свежий высокогорный воздух, собран на высоте 3000 м',
     2990.00, 50, 5, 1.00, 'Швейцария, Альпы'),

    ('Воздух Альп, 5 л (премиум)',
     'Подарочная упаковка в стеклянной колбе',
     12900.00, 15, 5, 5.00, 'Швейцария, Альпы'),

    ('Воздух Кавказа, 1 л',
     'Собран в районе Эльбруса, с нотками хвои',
     1990.00, 80, 6, 1.00, 'Россия, Кавказ'),

    ('Воздух Байкала, 0.5 л',
     'Воздух у берегов самого глубокого озера мира',
     1490.00, 120, 7, 0.50, 'Россия, Байкал'),

    ('Морской воздух Средиземноморья, 2 л',
     'С ароматом соли и бриза',
     3490.00, 40, 8, 2.00, 'Греция, Крит'),

    ('Воздух Сахары, 1 л',
     'Тёплый, с лёгким привкусом песка — для ценителей',
     4500.00, 10, 4, 1.00, 'Марокко, Сахара'),

    ('Воздух тропического леса Амазонии, 1 л',
     'Влажный, насыщенный фитонцидами',
     5200.00, 20, 4, 1.00, 'Бразилия, Амазония');

-- =========================================================
-- 15. ДАННЫЕ: корзина пользователя «ivanov» (id = 2)
-- =========================================================
INSERT INTO carts (user_id) VALUES (2);

INSERT INTO cart_items (cart_id, product_id, quantity) VALUES
    (1, 1, 2),   -- 2 × Воздух Альп, 1 л
    (1, 4, 3);   -- 3 × Воздух Байкала, 0.5 л

-- =========================================================
-- 16. ДАННЫЕ: заказ пользователя «ivanov»
-- =========================================================
INSERT INTO orders (user_id, status, total_amount) VALUES
    (2, 'paid', 10450.00);

INSERT INTO order_items (order_id, product_id, quantity, price) VALUES
    (1, 1, 2, 2990.00),   -- 2 × Воздух Альп, 1 л
    (1, 4, 3, 1490.00);   -- 3 × Воздух Байкала, 0.5 л

-- =========================================================
-- 17. ПРОВЕРОЧНЫЕ ЗАПРОСЫ (опционально, для отчёта)
-- =========================================================

-- Содержимое корзины пользователя ivanov
SELECT u.login, p.name AS product, ci.quantity, p.price,
       (p.price * ci.quantity) AS line_total
FROM carts c
JOIN users      u  ON u.id = c.user_id
JOIN cart_items ci ON ci.cart_id = c.id
JOIN products   p  ON p.id = ci.product_id
WHERE u.login = 'ivanov';

-- Состав заказа №1 и сумма строк
SELECT o.id AS order_id, u.login, p.name, oi.quantity, oi.price,
       (oi.quantity * oi.price) AS line_total
FROM orders o
JOIN users       u  ON u.id = o.user_id
JOIN order_items oi ON oi.order_id = o.id
JOIN products    p  ON p.id = oi.product_id
WHERE o.id = 1;

-- Топ категорий по проданному воздуху
SELECT cat.name, SUM(oi.quantity) AS sold_units
FROM order_items oi
JOIN products   p   ON p.id = oi.product_id
JOIN categories cat ON cat.id = p.category_id
GROUP BY cat.name
ORDER BY sold_units DESC;