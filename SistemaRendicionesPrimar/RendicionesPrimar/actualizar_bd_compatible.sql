-- Script de actualización compatible con versiones anteriores de MySQL
-- Ejecutar este script en MySQL para corregir todos los problemas

USE sistema_rendiciones;

-- Deshabilitar modo seguro temporalmente
SET SQL_SAFE_UPDATES = 0;

-- ===== ACTUALIZAR TABLA usuarios =====
-- Verificar y agregar columnas faltantes a la tabla usuarios

-- Verificar si nombre_completo existe
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'usuarios' 
     AND COLUMN_NAME = 'nombre_completo') = 0,
    'ALTER TABLE usuarios ADD COLUMN nombre_completo VARCHAR(100) NULL',
    'SELECT "nombre_completo ya existe en usuarios" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si telefono existe
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'usuarios' 
     AND COLUMN_NAME = 'telefono') = 0,
    'ALTER TABLE usuarios ADD COLUMN telefono VARCHAR(20) NULL',
    'SELECT "telefono ya existe en usuarios" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si cargo existe
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'usuarios' 
     AND COLUMN_NAME = 'cargo') = 0,
    'ALTER TABLE usuarios ADD COLUMN cargo VARCHAR(100) NULL',
    'SELECT "cargo ya existe en usuarios" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si departamento existe
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'usuarios' 
     AND COLUMN_NAME = 'departamento') = 0,
    'ALTER TABLE usuarios ADD COLUMN departamento VARCHAR(100) NULL',
    'SELECT "departamento ya existe en usuarios" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si activo existe
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'usuarios' 
     AND COLUMN_NAME = 'activo') = 0,
    'ALTER TABLE usuarios ADD COLUMN activo BOOLEAN DEFAULT TRUE',
    'SELECT "activo ya existe en usuarios" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si fecha_creacion existe
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'usuarios' 
     AND COLUMN_NAME = 'fecha_creacion') = 0,
    'ALTER TABLE usuarios ADD COLUMN fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP',
    'SELECT "fecha_creacion ya existe en usuarios" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ===== ACTUALIZAR TABLA rendiciones =====
-- Verificar y agregar columnas de aprobadores

-- Verificar si aprobador_1_id existe
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'aprobador_1_id') = 0,
    'ALTER TABLE rendiciones ADD COLUMN aprobador_1_id INT NULL',
    'SELECT "aprobador_1_id ya existe en rendiciones" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si aprobador_2_id existe
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'aprobador_2_id') = 0,
    'ALTER TABLE rendiciones ADD COLUMN aprobador_2_id INT NULL',
    'SELECT "aprobador_2_id ya existe en rendiciones" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si fecha_aprobacion_1 existe
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'fecha_aprobacion_1') = 0,
    'ALTER TABLE rendiciones ADD COLUMN fecha_aprobacion_1 DATETIME NULL',
    'SELECT "fecha_aprobacion_1 ya existe en rendiciones" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si fecha_aprobacion_2 existe
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'fecha_aprobacion_2') = 0,
    'ALTER TABLE rendiciones ADD COLUMN fecha_aprobacion_2 DATETIME NULL',
    'SELECT "fecha_aprobacion_2 ya existe en rendiciones" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si fecha_pago existe
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'fecha_pago') = 0,
    'ALTER TABLE rendiciones ADD COLUMN fecha_pago DATETIME NULL',
    'SELECT "fecha_pago ya existe en rendiciones" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si comentarios_aprobador existe
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'comentarios_aprobador') = 0,
    'ALTER TABLE rendiciones ADD COLUMN comentarios_aprobador TEXT NULL',
    'SELECT "comentarios_aprobador ya existe en rendiciones" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si aprobado_por_1 existe
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'aprobado_por_1') = 0,
    'ALTER TABLE rendiciones ADD COLUMN aprobado_por_1 INT NULL',
    'SELECT "aprobado_por_1 ya existe en rendiciones" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si aprobado_por_2 existe
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'aprobado_por_2') = 0,
    'ALTER TABLE rendiciones ADD COLUMN aprobado_por_2 INT NULL',
    'SELECT "aprobado_por_2 ya existe en rendiciones" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si nombre existe en rendiciones
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'nombre') = 0,
    'ALTER TABLE rendiciones ADD COLUMN nombre VARCHAR(100) NULL',
    'SELECT "nombre ya existe en rendiciones" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si apellidos existe en rendiciones
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'apellidos') = 0,
    'ALTER TABLE rendiciones ADD COLUMN apellidos VARCHAR(100) NULL',
    'SELECT "apellidos ya existe en rendiciones" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si rut existe en rendiciones
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'rut') = 0,
    'ALTER TABLE rendiciones ADD COLUMN rut VARCHAR(20) NULL',
    'SELECT "rut ya existe en rendiciones" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si telefono existe en rendiciones
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'telefono') = 0,
    'ALTER TABLE rendiciones ADD COLUMN telefono VARCHAR(20) NULL',
    'SELECT "telefono ya existe en rendiciones" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si cargo existe en rendiciones
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'cargo') = 0,
    'ALTER TABLE rendiciones ADD COLUMN cargo VARCHAR(100) NULL',
    'SELECT "cargo ya existe en rendiciones" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar si departamento existe en rendiciones
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'departamento') = 0,
    'ALTER TABLE rendiciones ADD COLUMN departamento VARCHAR(100) NULL',
    'SELECT "departamento ya existe en rendiciones" as mensaje'
));
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ===== CREAR TABLA archivos_adjuntos SI NO EXISTE =====
CREATE TABLE IF NOT EXISTS archivos_adjuntos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    rendicion_id INT NOT NULL,
    nombre_archivo VARCHAR(255) NOT NULL,
    ruta_archivo VARCHAR(500) NOT NULL,
    tipo_archivo VARCHAR(50) NULL,
    tamano_archivo BIGINT NULL,
    fecha_subida DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_rendicion_id (rendicion_id)
);

-- ===== CREAR TABLA notificaciones SI NO EXISTE =====
CREATE TABLE IF NOT EXISTS notificaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    rendicion_id INT NULL,
    mensaje TEXT NOT NULL,
    leido BOOLEAN DEFAULT FALSE,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    tipo_rol VARCHAR(20) NULL,
    INDEX idx_usuario_id (usuario_id),
    INDEX idx_rendicion_id (rendicion_id),
    INDEX idx_leido (leido)
);

-- ===== CREAR TABLA aprobadores SI NO EXISTE =====
CREATE TABLE IF NOT EXISTS aprobadores (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NULL,
    email VARCHAR(100) NOT NULL,
    rol VARCHAR(20) NOT NULL,
    activo BOOLEAN DEFAULT TRUE,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Habilitar modo seguro nuevamente
SET SQL_SAFE_UPDATES = 1;

-- Verificar la estructura final
SELECT '=== ESTRUCTURA DE TABLAS ===' as info;
DESCRIBE usuarios;
DESCRIBE rendiciones;
DESCRIBE archivos_adjuntos;
DESCRIBE notificaciones;
DESCRIBE aprobadores;

-- Verificar datos existentes
SELECT '=== DATOS EXISTENTES ===' as info;
SELECT COUNT(*) as total_usuarios FROM usuarios;
SELECT COUNT(*) as total_rendiciones FROM rendiciones;
SELECT COUNT(*) as total_archivos FROM archivos_adjuntos;
SELECT COUNT(*) as total_notificaciones FROM notificaciones;

-- Verificar usuarios por rol (usando las columnas que existen)
SELECT '=== USUARIOS POR ROL ===' as info;
SELECT rol, COUNT(*) as cantidad FROM usuarios GROUP BY rol;

-- Verificar estructura de usuarios
SELECT '=== COLUMNAS DE USUARIOS ===' as info;
SHOW COLUMNS FROM usuarios;

SELECT '=== ACTUALIZACIÓN COMPLETADA ===' as mensaje; 