-- Script corregido para actualizar la base de datos del Sistema de Rendiciones
-- Ejecutar este script en la base de datos MySQL

-- Deshabilitar modo seguro temporalmente para permitir actualizaciones
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

-- Verificar si nombre_completo existe en rendiciones
SET @sql = (SELECT IF(
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
     WHERE TABLE_SCHEMA = DATABASE() 
     AND TABLE_NAME = 'rendiciones' 
     AND COLUMN_NAME = 'nombre_completo') = 0,
    'ALTER TABLE rendiciones ADD COLUMN nombre_completo VARCHAR(100) NULL',
    'SELECT "nombre_completo ya existe en rendiciones" as mensaje'
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

-- ===== VERIFICAR TABLA archivos_adjuntos =====
-- Crear tabla si no existe
CREATE TABLE IF NOT EXISTS archivos_adjuntos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    rendicion_id INT NOT NULL,
    nombre_archivo VARCHAR(255) NOT NULL,
    ruta_archivo VARCHAR(500) NOT NULL,
    tipo_archivo VARCHAR(50) NULL,
    tamano_archivo BIGINT NULL,
    fecha_subida DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ===== VERIFICAR TABLA notificaciones =====
-- Crear tabla si no existe
CREATE TABLE IF NOT EXISTS notificaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NOT NULL,
    rendicion_id INT NULL,
    mensaje TEXT NOT NULL,
    leido BOOLEAN DEFAULT FALSE,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Habilitar modo seguro nuevamente
SET SQL_SAFE_UPDATES = 1;

-- Verificar la estructura de las tablas
SELECT 'usuarios' as tabla, COUNT(*) as columnas FROM information_schema.columns WHERE table_name = 'usuarios' AND table_schema = DATABASE();
SELECT 'rendiciones' as tabla, COUNT(*) as columnas FROM information_schema.columns WHERE table_name = 'rendiciones' AND table_schema = DATABASE();
SELECT 'archivos_adjuntos' as tabla, COUNT(*) as columnas FROM information_schema.columns WHERE table_name = 'archivos_adjuntos' AND table_schema = DATABASE();
SELECT 'notificaciones' as tabla, COUNT(*) as columnas FROM information_schema.columns WHERE table_name = 'notificaciones' AND table_schema = DATABASE();

-- Mostrar estructura de la tabla rendiciones
DESCRIBE rendiciones; 