-- ==========================================================
-- Скрипт создания структуры базы данных SQLite: ContactBook
-- Разработчик схемы: Годунов (Backend)
-- ==========================================================

PRAGMA foreign_keys = ON;

-- Таблица категорий / групп контактов
CREATE TABLE IF NOT EXISTS "Groups" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Groups" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL
);

-- Начальное заполнение базовых групп
INSERT OR IGNORE INTO "Groups" ("Id", "Name") VALUES 
    (1, 'Общие'),
    (2, 'Работа'),
    (3, 'Семья'),
    (4, 'Друзья'),
    (5, 'Избранное');

-- Таблица контактов
CREATE TABLE IF NOT EXISTS "Contacts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Contacts" PRIMARY KEY AUTOINCREMENT,
    "FirstName" TEXT NOT NULL,
    "LastName" TEXT NOT NULL DEFAULT '',
    "PhoneNumber" TEXT NOT NULL,
    "Email" TEXT NULL,
    "BirthDate" TEXT NULL,
    "Notes" TEXT NULL,
    "Photo" BLOB NULL,
    "GroupId" INTEGER NULL,
    CONSTRAINT "FK_Contacts_Groups_GroupId" FOREIGN KEY ("GroupId") 
        REFERENCES "Groups" ("Id") ON DELETE SET NULL
);

-- Индексы для оптимизации поиска
CREATE INDEX IF NOT EXISTS "IX_Contacts_GroupId" ON "Contacts" ("GroupId");
CREATE INDEX IF NOT EXISTS "IX_Contacts_PhoneNumber" ON "Contacts" ("PhoneNumber");
CREATE INDEX IF NOT EXISTS "IX_Contacts_LastName_FirstName" ON "Contacts" ("LastName", "FirstName");
